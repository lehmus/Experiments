using System;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using DataFetch.RequestModels;
using DataFetch.DatabaseModels;

namespace DataFetch.Functions
{
    public static class DataFetch
    {
        private static string baseAddressString = ConfigurationManager.AppSettings["StatFinDomain"];
        private static string apiPath = ConfigurationManager.AppSettings["StatFinApiPath"];
        private static string connectionString = ConfigurationManager.ConnectionStrings["BlobStorage"].ConnectionString;
        private static readonly HttpClient httpClient = new HttpClient();

        [FunctionName("DataFetch")]
        public static async Task<HttpResponseMessage> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "datafetch/{blobContainerName}/{database}/{subjectRealm}/{subjectSubRealm}/{tableName}/{yearMin}/{yearMax}")]HttpRequestMessage triggerRequest,
            string blobContainerName,
            string database,
            string subjectRealm,
            string subjectSubRealm,
            string tableName,
            string yearMin,
            string yearMax,
            TraceWriter log
            )
        {
            string apiVersion = "v1";
            string resultsLanguage = "fi"; // TODO: find available languages and loop over (or include language variable in the dataset)

            log.Info("C# HTTP trigger function begins to process a request.");

            var datasetDetails = new StatFinDatasetMeta
            {
                Database = database,
                SubjectRealm = subjectRealm,
                SubjectSubRealm = subjectSubRealm,
                TableName = tableName,
                ResultsLanguage = resultsLanguage
            };
            var dataFetchUri = ConstructRequestUri(datasetDetails, apiVersion, resultsLanguage);

            // Get data for each year individually and save to Blob
            var years = GetYearRange(yearMin, yearMax);
            foreach (var year in years) {
                var dataFetchRequestBody = ConstructRequestBody( new string[] { year });

                var dataFetchResponse = await httpClient.PostAsJsonAsync(dataFetchUri, dataFetchRequestBody);
                var dataFetchResponseString = await dataFetchResponse.Content.ReadAsStringAsync();

                string blobFilePath = GetStageBlobPath(datasetDetails, year);
                WriteToBlob(blobContainerName, blobFilePath, dataFetchResponseString);
            }

            log.Info("C# HTTP trigger function processed a request successfully.");
            return triggerRequest.CreateResponse(HttpStatusCode.OK);
        }

        private static string[] GetYearRange(string yearMinText, string yearMaxText)
        {
            var years = new List<string>();
            // TODO: try catch
            var yearMin = Int32.Parse(yearMinText);
            var yearMax = Int32.Parse(yearMaxText);
            for (var year = yearMin; year <= yearMax; year++)
            {
                years.Add(year.ToString());
            }
            return years.ToArray();
        }

        private static Uri ConstructRequestUri(StatFinDatasetMeta datasetDetails, string apiVersion, string resultsLanguage)
        {
            var baseAddress = new Uri(baseAddressString);
            var tablePath = new Uri(
                apiPath +
                "/" + apiVersion +
                "/" + resultsLanguage +
                "/" + datasetDetails.Database +
                "/" + datasetDetails.SubjectRealm +
                "/" + datasetDetails.SubjectSubRealm +
                "/" + datasetDetails.TableName + ".px",
                UriKind.Relative
                );
            return new Uri(baseAddress, tablePath);
        }

        private static TableQuery ConstructRequestBody(string[] years)
        {
            var dataFetchRequestBody = new TableQuery {
                query = new TableQuerySettings[] {
                    new TableQuerySettings {
                        code = "Vuosi",
                        selection = new FilterSelection
                        {
                            filter = "item",
                            values = years
                        }
                    }
                },
                response = new TableResponseSettings { format = "json" }
            };
            return dataFetchRequestBody;
        }

        private static void WriteToBlob(string containerName, string folderPath, string fileContent)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);
            var blockBlob = container.GetBlockBlobReference(folderPath);
            blockBlob.UploadTextAsync(fileContent);
        }

        // TODO: Move this somewhere so that it can be used from other classes as well
        private static string GetStageBlobPath(StatFinDatasetMeta datasetDetails, string year)
        {
            return "stage/" + datasetDetails.Database + "/" + datasetDetails.SubjectRealm + "/" + datasetDetails.SubjectSubRealm + "/" + datasetDetails.TableName + "/" + datasetDetails.ResultsLanguage + "/" + year + ".json";
        }
    }
}
