using System;
using System.Net;
using System.Net.Http;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using DataFetch.RequestModels;

namespace DataFetch.Functions
{
    public static class DataFetch
    {
        private static readonly HttpClient httpClient = new HttpClient();

        [FunctionName("DataFetch")]
        public static async Task<HttpResponseMessage> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage triggerRequest, TraceWriter log)
        {
            string baseAddressString = "http://pxnet2.stat.fi";
            string apiPath = "/PXWeb/api";
            string apiVersion = "v1";
            string resultsLanguage = "fi";
            string databaseName = "StatFin";
            string subjectRealm = "ene";
            string subjectSubRealm = "ehk";
            string tableName = "statfin_ehk_pxt_001_fi";
            string year = "1970";
            string blobContainerName = "tilastokeskus";

            log.Info("C# HTTP trigger function begins to process a request.");

            var baseAddress = new Uri(baseAddressString);
            var tablePath = new Uri(
                apiPath +
                "/" + apiVersion +
                "/" + resultsLanguage +
                "/" + databaseName +
                "/" + subjectRealm +
                "/" + subjectSubRealm +
                "/" + tableName + ".px",
                UriKind.Relative
                );
            var dataFetchUri = new Uri(baseAddress, tablePath);

            var dataFetchRequestBody = new TableQuery {
                query = new TableQuerySettings[] {
                    new TableQuerySettings {
                        code = "Vuosi",
                        selection = new FilterSelection
                        {
                            filter = "item",
                            values = new string[] { year }
                        }
                    }
                },
                response = new TableResponseSettings { format = "json" }
            };

            var dataFetchResponse = await httpClient.PostAsJsonAsync(dataFetchUri, dataFetchRequestBody);
            var dataFetchResponseString = await dataFetchResponse.Content.ReadAsStringAsync();

            string blobFolder = databaseName + "/" + subjectRealm + "/" + subjectSubRealm + "/" + tableName + "/" + resultsLanguage + "/" + year + ".json";
            WriteToBlob(blobContainerName, blobFolder, dataFetchResponseString);

            log.Info("C# HTTP trigger function processed a request successfully.");
            return triggerRequest.CreateResponse(HttpStatusCode.OK);
        }

        private static void WriteToBlob(string containerName, string folderPath, string fileContent)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["BlobStorage"].ConnectionString;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(folderPath);
            blockBlob.UploadTextAsync(fileContent);
        }
    }
}
