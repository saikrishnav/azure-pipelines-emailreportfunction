import { AzureFunction, Context, HttpRequest } from "@azure/functions";
import * as msrestazure from "ms-rest-azure";
import * as keyvault from "azure-keyvault";

const httpTrigger: AzureFunction = async function (context: Context, req: HttpRequest): Promise<void> {
    context.log('HTTP trigger function processed a request.');
    const name = (req.query.name || (req.body && req.body.name));

    if (name) {
        context.res = {
            // status: 200, /* Defaults to 200 */
            body: "Hello " + (req.query.name || req.body.name)
        };

        const keyVaultName = req.query.name;
        const secretName = req.query.key;
        const vaultUri =  `https://${keyVaultName}.vault.azure.net/`;
        const vaultSecretUri =  `${vaultUri}secrets/${secretName}`;
        let credentials = await msrestazure.MSIAppServiceTokenCredentials() //{resource: vaultUri});
        const keyVaultClient = new keyvault.KeyVaultClient(credentials);
            
        // We're setting the Secret value here and retrieving the secret value
        const secretBundle = await keyVaultClient.getSecret(vaultUri, secretName, "");
        const secretValue = secretBundle;
        
    }
    else {
        context.res = {
            status: 400,
            body: "Please pass a name on the query string or in the request body"
        };
    }
};

export default httpTrigger;