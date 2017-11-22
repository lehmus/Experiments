using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Configuration;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using DataFetch.DatabaseModels;

namespace DataFetch.Functions
{
    public static class UpdateMasterData
    {
        private static string connectionString = ConfigurationManager.ConnectionStrings["BlobStorage"].ConnectionString;

        [FunctionName("UpdateMasterData")]
        public static HttpResponseMessage RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "updatemasterdata/{blobContainerName}/{database}/{subjectRealm}/{subjectSubRealm}/{tableName}")]HttpRequestMessage triggerRequest,
            string blobContainerName,
            string database,
            string subjectRealm,
            string subjectSubRealm,
            string tableName,
            TraceWriter logger
            )
        {
            var resultsLanguage = "fi"; // TODO: find available languages and loop over (or include language variable in the dataset)

            logger.Info("Beginning to process a HTTP trigger request.");

            var storageAccount = CloudStorageAccount.Parse(connectionString); // TODO: check for existence
            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(blobContainerName); // TODO: check for existence

            var datasetDetails = new StatFinDatasetMeta
            {
                Database = database,
                SubjectRealm = subjectRealm,
                SubjectSubRealm = subjectSubRealm,
                TableName = tableName,
                ResultsLanguage = resultsLanguage
            };

            var rawDatasets = ReadStageBlob(blobContainer, datasetDetails);
            var masterDataset = ToMasterDataset(rawDatasets, logger);
            WriteMasterBlobCsv(masterDataset, datasetDetails, blobContainer);
            // TODO: Save in JSON as well

            logger.Info("HTTP trigger request processed successfully.");
            return triggerRequest.CreateResponse(HttpStatusCode.OK);
        }

        private static StatFinDatasetRaw[] ReadStageBlob(CloudBlobContainer blobContainer, StatFinDatasetMeta datasetDetails)
        {
            var datasets = new List<StatFinDatasetRaw>();
            var blobDir = GetStageDirectoryPath(datasetDetails);
            var blobDirItems = blobContainer.ListBlobs(blobDir); // ListBlobs (for container) returns the directory itself, not its contents
            foreach (var blobDirItem in blobDirItems)
            {
                var directory = (CloudBlobDirectory)blobDirItem;
                var blobs = directory.ListBlobs();
                foreach (var blobItem in blobs)
                {
                    var blob = (CloudBlockBlob)blobItem;
                    var blobContentText = string.Empty;
                    // Read the Blob contents to memory
                    using (var memoryStream = new MemoryStream())
                    {
                        blob.DownloadToStream(memoryStream);
                        blobContentText = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
                    }
                    // TODO: try catch
                    var blobContent = JsonConvert.DeserializeObject<StatFinDatasetRaw>(blobContentText);
                    datasets.Add(blobContent);
                }
            }
            return datasets.ToArray();
        }

        private static StatFinDataset ToMasterDataset(StatFinDatasetRaw[] rawDatasets, TraceWriter logger)
        {
            var dataRows = new List<StatFinDataRow>();
            if (rawDatasets.Length == 0)
                return new StatFinDataset {};
            // Get the field headers
            var headers = new List<string>();
            var numberOfColumns = rawDatasets[0].columns.Length - 1; // Last column is obsolete
            for (var idx = 0; idx < numberOfColumns; idx++)
            {
                headers.Add(rawDatasets[0].columns[idx].text);
            }
            // Get the data rows
            foreach (var rawDataset in rawDatasets)
            {
                var cleanedRawDataset = CleanDataset(rawDataset, logger);
                foreach (var dataRowRaw in cleanedRawDataset.data)
                {
                    dataRows.Add(new StatFinDataRow { Features = dataRowRaw.key, Value = dataRowRaw.values[0] });
                }
            }
            return new StatFinDataset { FieldNames = headers.ToArray(), Rows = dataRows.ToArray() };
        }

        private static StatFinDatasetRaw CleanDataset(StatFinDatasetRaw dirtyDataset, TraceWriter logger)
        {
            // TODO: instead of creating a duplicate dataset, remove the entries from the dirty dataset and return the same dataset object
            var cleanedDataset = new StatFinDatasetRaw
            {
                columns = dirtyDataset.columns,
                comments = dirtyDataset.comments
            };
            var cleanRows = new List<StatFinDataRowRaw>();
            foreach (var dataRow in dirtyDataset.data)
            {
                if (dataRow.values.Length != 1)
                {
                    logger.Info("Invalid data row: the number of values is not equal to one. Values: " + dataRow.values.ToString());
                    continue;
                }
                if (dataRow.values[0] != ".")
                    cleanRows.Add(dataRow);
            }
            cleanedDataset.data = cleanRows.ToArray();
            return cleanedDataset;
        }

        private static void WriteMasterBlobCsv(StatFinDataset dataset, StatFinDatasetMeta datasetDetails, CloudBlobContainer blobContainer)
        {
            var blobPath = GetMasterBlobPathCsv(datasetDetails);
            var blockBlob = blobContainer.GetBlockBlobReference(blobPath);
            var datasetText = ToCsv(dataset);
            blockBlob.UploadTextAsync(datasetText);
        }

        private static string ToCsv(StatFinDataset dataset)
        {
            var datasetText = String.Empty;
            var headerRow = String.Join(", ", dataset.FieldNames) + ", M‰‰r‰" + Environment.NewLine;
            datasetText += headerRow;
            foreach (var dataRow in dataset.Rows)
            {
                var dataRowText = String.Join(", ", dataRow.Features) + ", " + dataRow.Value + Environment.NewLine;
                datasetText += dataRowText;
            }
            return datasetText;
        }

        private static string ToJson(StatFinDataset dataset, StatFinDatasetMeta datasetDetails, CloudBlobContainer blobContainer)
        {
            // TODO
            return String.Empty;
        }

        // TODO: Move this somewhere so that it can be used from other classes as well
        private static string GetStageDirectoryPath(StatFinDatasetMeta blobFolder)
        {
            return "stage/" + blobFolder.Database + "/" + blobFolder.SubjectRealm + "/" + blobFolder.SubjectSubRealm + "/" + blobFolder.TableName + "/" + blobFolder.ResultsLanguage;
        }

        // TODO: Move this somewhere so that it can be used from other classes as well
        private static string GetMasterBlobPathCsv(StatFinDatasetMeta blobFolder)
        {
            return "master/" + blobFolder.Database + "/" + blobFolder.SubjectRealm + "/" + blobFolder.SubjectSubRealm + "/" + blobFolder.ResultsLanguage + "/csv/" + blobFolder.TableName + ".csv";
        }

        // TODO: Move this somewhere so that it can be used from other classes as well
        private static string GetMasterBlobPathJson(StatFinDatasetMeta blobFolder)
        {
            return "master/" + blobFolder.Database + "/" + blobFolder.SubjectRealm + "/" + blobFolder.SubjectSubRealm + "/" + blobFolder.ResultsLanguage + "/json/" + blobFolder.TableName + ".json";
        }
    }
}
