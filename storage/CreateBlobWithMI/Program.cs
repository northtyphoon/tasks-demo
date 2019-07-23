using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;

namespace CreateBlobWithMI
{
    class Program
    {
        static async Task Main(string[] args)
        {
            int exitCode = 0;

            try
            {
                if (args.Length != 1)
                {
                    Console.WriteLine($"Usage: dotnet {nameof(CreateBlobWithMI)} url");
                    Environment.Exit(-1);
                }

                string blobName = args[0];

                // https://docs.microsoft.com/en-us/azure/storage/common/storage-auth-aad-msi
                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                var tokenAndFrequency = await TokenRenewerAsync(azureServiceTokenProvider);
                var tokenCredential = new TokenCredential(tokenAndFrequency.Token, TokenRenewerAsync, azureServiceTokenProvider, tokenAndFrequency.Frequency.Value);
                var storageCredentials = new StorageCredentials(tokenCredential);

                var blob = new CloudBlockBlob(new Uri(blobName), storageCredentials);

                Console.WriteLine($"{nameof(CreateBlobWithMI)} creating {blobName}...");

                if (!await blob.ExistsAsync())
                {
                    await blob.UploadTextAsync($"Hello world, {DateTimeOffset.UtcNow}");
                    //blob.Metadata.Add("CreatedBy", "MSI");
                    //await blob.SetMetadataAsync();
                    Console.WriteLine($"{nameof(CreateBlobWithMI)} succeeded.");
                }
                else
                {
                    Console.WriteLine($"{nameof(CreateBlobWithMI)} skipped.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{nameof(CreateBlobWithMI)} failed. Error: {ex}");
                exitCode = -1;
            }

            Environment.Exit(exitCode);
        }

        private static async Task<NewTokenAndFrequency> TokenRenewerAsync(Object state, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Specify the resource ID for requesting Azure AD tokens for Azure Storage.
            // Note that you can also specify the root URI for your storage account as the resource ID.
            const string StorageResource = "https://storage.azure.com/";

            // Use the same token provider to request a new token.
            var authResult = await ((AzureServiceTokenProvider)state).GetAuthenticationResultAsync(StorageResource);

            // Renew the token 5 minutes before it expires.
            var next = (authResult.ExpiresOn - DateTimeOffset.UtcNow) - TimeSpan.FromMinutes(5);
            if (next.Ticks < 0)
            {
                next = default(TimeSpan);
                Console.WriteLine($"{nameof(CreateBlobWithMI)} renewing token...");
            }

            // Return the new token and the next refresh time.
            return new NewTokenAndFrequency(authResult.AccessToken, next);
        }
    }
}
