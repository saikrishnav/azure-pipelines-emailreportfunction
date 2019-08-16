using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EmailReportFunction
{
    public static class KeyVaultReader
    {
        public async static Task<SecretBundle> FetchSecret(string keyVaultName, string secretName, int retryCount, ILogger log)
        {
            for (int i = 0; i < retryCount + 1; i++)
            {
                try
                {
                    var azureServiceTokenProvider = new AzureServiceTokenProvider();
                    var keyVaultClient = new KeyVaultClient(
                        new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
                    var secret = await keyVaultClient.GetSecretAsync($"https://{keyVaultName}.vault.azure.net/secrets/{secretName}")
                            .ConfigureAwait(false);
                    return secret;
                }
                catch (KeyVaultErrorException keyVaultException)
                {
                    log.LogError($"EmailReportPPEFunction: Unable to fetch azpipes credentials from keyvault: {keyVaultException.Message}");
                }
            }

            return null;
        }
    }
}
