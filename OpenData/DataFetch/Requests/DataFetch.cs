using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using DataFetch.RequestModels;

namespace DataFetch.Requests
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
            string tableName = "statfin_ehk_pxt_001_fi.px";

            log.Info("C# HTTP trigger function begins to process a request.");

            var baseAddress = new Uri(baseAddressString);
            var tablePath = new Uri(
                apiPath +
                "/" + apiVersion +
                "/" + resultsLanguage +
                "/" + databaseName +
                "/" + subjectRealm +
                "/" + subjectSubRealm +
                "/" + tableName,
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
                            values = new string[] { "1970" }
                        }
                    }
                },
                response = new TableResponseSettings { format = "json" }
            };

            var dataFetchResponse = await httpClient.PostAsJsonAsync(dataFetchUri, dataFetchRequestBody);
            var dataFetchResponseString = await dataFetchResponse.Content.ReadAsStringAsync();

            log.Info("C# HTTP trigger function processed a request successfully.");
            log.Info("Response content: " + dataFetchResponseString);
            return triggerRequest.CreateResponse(HttpStatusCode.OK);
        }
    }
}
