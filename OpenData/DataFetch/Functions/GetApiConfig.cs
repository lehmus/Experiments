using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace DataFetch.Requests
{
    public static class GetApiConfig
    {
        [FunctionName("GetApiConfig")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage triggerRequest, TraceWriter log)
        {
            log.Info("C# HTTP trigger function begins to process a request.");

            string dataFetchUrl = "http://pxnet2.stat.fi/PXWeb/api";
            string apiVersion = "v1";
            string resultsLanguage = "fi";
            dataFetchUrl +=
                "/" + apiVersion +
                "/" + resultsLanguage +
                "/?config";

            WebRequest dataFetchRequest = WebRequest.Create(dataFetchUrl);
            HttpWebResponse dataFetchResponse = (HttpWebResponse)dataFetchRequest.GetResponse();
            Stream dataStream = dataFetchResponse.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            reader.Close();
            dataStream.Close();
            dataFetchResponse.Close();

            log.Info("C# HTTP trigger function processed a request successfully.");
            return triggerRequest.CreateResponse(HttpStatusCode.OK);
        }
    }
}
