import { AzureFunction, Context, HttpRequest } from "@azure/functions";
import * as KeyVault from "@azure/keyvault-secrets";
import * as Identity from "@azure/identity";
import { JsonConfigProvider } from "./JsonConfigProvider";
import { EmailSender } from "./EmailSender";
import { ReportConfiguration } from "./config/ReportConfiguration";
import { ReportProvider } from "./providers/ReportProvider";
import { DataProviderFactory } from "./providers/DataProviderFactory";
import { ReportManager } from "./ReportManager";
import { HTMLReportCreator } from "./htmlreport/HTMLReportCreator";

const httpTrigger: AzureFunction = async function (context: Context, req: HttpRequest): Promise<void> {
    context.log('HTTP trigger function processed a request.');

    const vaultUri = `https://vsotest.vault.azure.net/`;
    let credentials = new Identity.EnvironmentCredential();
    const keyVaultClient = new KeyVault.SecretsClient(vaultUri, credentials);

    // We're setting the Secret value here and retrieving the secret value
    const secretBundle = await keyVaultClient.getSecret("vseqa1-hotmail");

    const configProvider = new JsonConfigProvider(req.body, secretBundle);
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