using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Management.DataFactories.Models;
using Microsoft.Azure.Management.DataFactories.Runtime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using DataManager.Automation.CustomActivities.Models;

namespace DataManager.Automation.CustomActivities.Activities
{
    public class JsonToCsv : IDotNetActivity
    {
        // Define the dataset table names
        private const string InputTableReddit = "FolderReddit";
        // TODO: Add more inputs
        // private static readonly string[] InputTables = { "FolderReddit", "FolderWikipedia", "FolderTwitter" };
        private const string OutputTable = "FolderDestination";

        public IDictionary<string, string> Execute(
            IEnumerable<LinkedService> linkedServices,
            IEnumerable<Dataset> datasets,
            Activity activity,
            IActivityLogger logger)
        {

            var datasetsByName = datasets.ToDictionary(dataset => dataset.Name);
            var linkedServicesByName = linkedServices.ToDictionary(linkedService => linkedService.Name);

            var inputDataset = datasetsByName[InputTableReddit];
            var inputLinkedService = linkedServicesByName[inputDataset.Properties.LinkedServiceName];
            var inputLinkedServiceStorage = (AzureStorageLinkedService)inputLinkedService.Properties.TypeProperties;
            var jsonDatas = ReadDataFromBlobs(inputDataset, inputLinkedServiceStorage, logger);

            var outputDataset = datasetsByName[OutputTable];
            var outputLinkedService = linkedServicesByName[outputDataset.Properties.LinkedServiceName];
            var outputLinkedServiceStorage = (AzureStorageLinkedService)outputLinkedService.Properties.TypeProperties;

            var outputCsv = DataMap(jsonDatas);
            WriteDataToBlob(outputDataset, outputLinkedServiceStorage, logger, outputCsv);

            // The dictionary can be used to chain custom activities together in the future.
            // This feature is not implemented yet, so just return an empty dictionary.  
            return new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets the folderPath value from the input/output dataset.
        /// </summary>
        private static string GetFolderPath(Dataset dataArtifact)
        {
            if (dataArtifact == null || dataArtifact.Properties == null)
            {
                return null;
            }

            AzureBlobDataset blobDataset = dataArtifact.Properties.TypeProperties as AzureBlobDataset;
            if (blobDataset == null)
            {
                return null;
            }

            return blobDataset.FolderPath;
        }

        /// <summary>
        /// Gets the fileName value from the input/output dataset.   
        /// </summary>
        private static string GetFileName(Dataset dataArtifact)
        {
            if (dataArtifact == null || dataArtifact.Properties == null)
            {
                return null;
            }

            AzureBlobDataset blobDataset = dataArtifact.Properties.TypeProperties as AzureBlobDataset;
            if (blobDataset == null)
            {
                return null;
            }

            return blobDataset.FileName;
        }

        public static List<TextSample> DownloadBlobs(BlobResultSegment Bresult, IActivityLogger logger, string folderPath, ref BlobContinuationToken token)
        {
            var textSamples = new List<TextSample>();
            foreach (IListBlobItem listBlobItem in Bresult.Results)
            {
                CloudBlockBlob inputBlob = listBlobItem as CloudBlockBlob;
                if ((inputBlob != null) && (inputBlob.Name.IndexOf("$$$.$$$") == -1))
                {
                    var blobText = inputBlob.DownloadText();
                    textSamples.Add(JsonConvert.DeserializeObject<TextSample>(blobText));
                }
            }

            return textSamples;
        }

        public string DataMap(List<TextSample> textSamples)
        {
            var outputCsv = "Source, IsScience, IsSport, IsEnvironment, Text" + Environment.NewLine;

            foreach(var textSample in textSamples)
            {
                string outputText = String.Join("", textSample.text.Split(',', '.', ';', '\'', '@', '#'));
                outputCsv += $"{textSample.source}, {textSample.isScience}, {textSample.isSport}, {textSample.isEnvironment}, {outputText} {Environment.NewLine}";
            }

            return outputCsv;
        }

        private static List<TextSample> ReadDataFromBlobs(
            Dataset inputDataset,
            AzureStorageLinkedService inputLinkedService,
            IActivityLogger logger
            )
        {
            var result = new List<TextSample>();
            var folderPath = GetFolderPath(inputDataset);

            var inputConnectionString = inputLinkedService.ConnectionString;
            var inputStorageAccount = CloudStorageAccount.Parse(inputConnectionString);
            var inputClient = inputStorageAccount.CreateCloudBlobClient();

            BlobContinuationToken continuationToken = null;
            do
            {
                var blobList = inputClient.ListBlobsSegmented(folderPath,
                                         true,
                                         BlobListingDetails.Metadata,
                                         null,
                                         continuationToken,
                                         null,
                                         null);

                result.AddRange(DownloadBlobs(blobList, logger, folderPath, ref continuationToken));
            } while (continuationToken != null);

            return result;
        }

        private static void WriteDataToBlob(
            Dataset outputDataset,
            AzureStorageLinkedService outputLinkedService,
            IActivityLogger logger,
            string csvData
            )
        {
            var folderPath = GetFolderPath(outputDataset);

            // create a storage object for the output blob.
            var outputConnectionString = outputLinkedService.ConnectionString;
            var outputStorageAccount = CloudStorageAccount.Parse(outputConnectionString);
            var outputBlobUri = new Uri(outputStorageAccount.BlobEndpoint, folderPath + "/" + GetFileName(outputDataset));

            var outputBlob = new CloudBlockBlob(outputBlobUri, outputStorageAccount.Credentials);
            outputBlob.UploadText(csvData);
        }
    }
}
