import { AzureFunction, Context, HttpRequest } from "@azure/functions";
import * as KeyVault from "@azure/keyvault-secrets";
import * as Identity from "@azure/identity";
import { EmailSender } from "azure-devops-emailreporttask/EmailSender";
import { ReportConfiguration } from "azure-devops-emailreporttask/config/ReportConfiguration";
import { ReportProvider } from "azure-devops-emailreporttask/providers/ReportProvider";
import { DataProviderFactory } from "azure-devops-emailreporttask/providers/DataProviderFactory";
import { ReportManager } from "azure-devops-emailreporttask/ReportManager";
import { HTMLReportCreator } from "azure-devops-emailreporttask/htmlreport/HTMLReportCreator";
import { JsonConfigProvider } from "./JsonConfigProvider";
import { SmtpConfiguration } from "azure-devops-emailreporttask/config/mail/SmtpConfiguration";

const httpTrigger: AzureFunction = async function (context: Context, req: HttpRequest): Promise<void> {
    context.log('HTTP trigger function processed a request.');

    const vaultName = req.body.SmtpConnectionInfo.KeyVaultName;
    const secretName = req.body.SmtpConnectionInfo.SecretName;
    const userName = req.body.SmtpConnectionInfo.UserName;

    const vaultUri = `https://${vaultName}.vault.azure.net/`;
    let credentials = new Identity.EnvironmentCredential();
    const keyVaultClient = new KeyVault.SecretsClient(vaultUri, credentials);

    // We're setting the Secret value here and retrieving the secret value
    const secretBundle = await keyVaultClient.getSecret(secretName);

    const smtpHost = req.body.SmtpConnectionInfo.SmtpHost;
    const password = secretBundle.value;
    const enableSSLOnSmtpConnection = false;

    const smtpConfig = new SmtpConfiguration(userName, password, smtpHost, enableSSLOnSmtpConnection);

    const configProvider = new JsonConfigProvider(req.body, smtpConfig);
    const reportConfiguration = new ReportConfiguration(configProvider);
    const reportProvider = new ReportProvider(new DataProviderFactory(configProvider.getPipelineConfiguration()));

    const reportManager = new ReportManager(
        reportProvider,
        new HTMLReportCreator(),
        new EmailSender());

    await reportManager.sendReportAsync(reportConfiguration);
    context.res = {
        // status: 200, /* Defaults to 200 */
        body: "Mail Sent"
    };
};

export default httpTrigger;