using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Management.DataFactories.Models;
using Microsoft.Azure.Management.DataFactories.Runtime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using DataManager.CustomActivities.Models;

namespace DataManager.CustomActivities.Activities
{
    public class JsonToCsv : IDotNetActivity
    {

        public IDictionary<string, string> Execute(
            IEnumerable<LinkedService> linkedServices,
            IEnumerable<Dataset> datasets,
            Activity activity,
            IActivityLogger logger)
        {
            var inputName = "FolderReddit";
            var outputName = "FolderDestination";

            var jsonDatas = readDataFromBlobs(datasets, linkedServices, activity, logger, inputName);
            var outputCsv = DataMap(jsonDatas);
            writeDataToBlob(datasets, linkedServices, activity, logger, outputCsv, outputName);

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

        List<TextSample> readDataFromBlobs(IEnumerable<Dataset> datasets, IEnumerable<LinkedService> linkedServices, Activity activity, IActivityLogger logger, string inputName)
        {
            var result = new List<TextSample>();
            Dataset inputDataset = datasets.Single(dataset => dataset.Name == inputName);
            string folderPath = GetFolderPath(inputDataset);
            AzureStorageLinkedService inputLinkedService = linkedServices.Single(
                    linkedService =>
                    linkedService.Name == inputDataset.Properties.LinkedServiceName
                ).Properties.TypeProperties
                as AzureStorageLinkedService;

            string inputConnectionString = inputLinkedService.ConnectionString;
            CloudStorageAccount inputStorageAccount = CloudStorageAccount.Parse(inputConnectionString);
            CloudBlobClient inputClient = inputStorageAccount.CreateCloudBlobClient();

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

        void writeDataToBlob(IEnumerable<Dataset> datasets, IEnumerable<LinkedService> linkedServices, Activity activity, IActivityLogger logger, string csvData, string outputName)
        {
            Dataset outputDataset = datasets.Single(dataset => dataset.Name == outputName);
            var folderPath = GetFolderPath(outputDataset);
            AzureStorageLinkedService outputLinkedService = linkedServices.Single(
                    linkedService =>
                    linkedService.Name == outputDataset.Properties.LinkedServiceName
                ).Properties.TypeProperties
                as AzureStorageLinkedService;

            // create a storage object for the output blob.
            string outputConnectionString = outputLinkedService.ConnectionString;
            CloudStorageAccount outputStorageAccount = CloudStorageAccount.Parse(outputConnectionString);
            Uri outputBlobUri = new Uri(outputStorageAccount.BlobEndpoint, folderPath + "/" + GetFileName(outputDataset));

            CloudBlockBlob outputBlob = new CloudBlockBlob(outputBlobUri, outputStorageAccount.Credentials);
            outputBlob.UploadText(csvData);
        }
    }
}
