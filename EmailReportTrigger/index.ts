import { AzureFunction, Context, HttpRequest } from "@azure/functions";
import * as KeyVault from "@azure/keyvault-secrets";
import * as Identity from "@azure/identity";
import { JsonConfigProvider } from "./JsonConfigProvider";

const httpTrigger: AzureFunction = async function (context: Context, req: HttpRequest): Promise<void> {
    context.log('HTTP trigger function processed a request.');
    const name = (req.query.name || (req.body && req.body.name));

    if (name) {
        const vaultUri =  `https://vsotest.vault.azure.net/`;
        let credentials = new Identity.EnvironmentCredential();
        const keyVaultClient = new KeyVault.SecretsClient(vaultUri, credentials);
            
        // We're setting the Secret value here and retrieving the secret value
        const secretBundle = await keyVaultClient.getSecret("vseqa1-hotmail");
        
        //const report = new EmailReport.AzureDevOpsPipelineReport();
        //await report.GenerateAndSendReportAsync(new JsonConfigProvider(req.body, secretBundle));
        context.res = {
            // status: 200, /* Defaults to 200 */
            body: "Mail Sent"
        };
    }
    else {
        context.res = {
            status: 400,
            body: "Please pass a name on the query string or in the request body"
        };
    }
};

export default httpTrigger;