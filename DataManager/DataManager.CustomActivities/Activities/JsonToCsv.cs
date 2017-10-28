using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Management.DataFactories.Models;
using Microsoft.Azure.Management.DataFactories.Runtime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            string inputName = "FolderSource";
            string outputName = "FolderDestination";

            // log all extended properties            
            /*
            IDictionary<string, string> extendedProperties = dotNetActivity.ExtendedProperties;
            logger.Write("Logging extended properties if any...");
            foreach (KeyValuePair<string, string> entry in extendedProperties)
            {
                logger.Write("<key:{0}> <value:{1}>", entry.Key, entry.Value);
            }
            */

            AzureStorageLinkedService inputLinkedService;

            // get the input dataset
            Dataset inputDataset = datasets.Single(dataset => dataset.Name == inputName);
            // Dataset inputDataset = datasets.Single(dataset => dataset.Name == activity.Inputs.Single().Name);
            // int numberOfInputs = activity.Inputs.Count();
            // string inputName = activity.Inputs.Single().Name;
            // int numberOfDatasets = datasets.Count();
            // string inputDatasetName = inputDataset.Name;

            // declare variables to hold type properties of input/output datasets
            // AzureBlobDataset inputTypeProperties, outputTypeProperties;

            // get type properties from the dataset object
            // inputTypeProperties = inputDataset.Properties.TypeProperties as AzureBlobDataset;

            // get the input Azure Storage linked service from linkedServices object
            inputLinkedService = linkedServices.Single(
                    linkedService =>
                    linkedService.Name == inputDataset.Properties.LinkedServiceName
                ).Properties.TypeProperties
                as AzureStorageLinkedService;

            // get the connection string in the linked service
            string inputConnectionString = inputLinkedService.ConnectionString;

            // get the folder path from the input dataset definition
            string folderPath = GetFolderPath(inputDataset);

            // create storage client for input. Pass the connection string.
            CloudStorageAccount inputStorageAccount = CloudStorageAccount.Parse(inputConnectionString);
            CloudBlobClient inputClient = inputStorageAccount.CreateCloudBlobClient();

            string output = string.Empty;
            // Store all samples in the memory for now
            // List<TextSample> samples = new List<TextSample>();
            // initialize the continuation token before using it in the do-while loop.
            BlobContinuationToken continuationToken = null;
            do
            {   // get the list of input blobs from the input storage client object.
                BlobResultSegment blobList = inputClient.ListBlobsSegmented(folderPath,
                                         true,
                                         BlobListingDetails.Metadata,
                                         null,
                                         continuationToken,
                                         null,
                                         null);

                // Calculate method returns the number of occurrences of
                // the search term (“Microsoft”) in each blob associated
                // with the data slice. definition of the method is shown in the next step.

                // output = Calculate(blobList, logger, folderPath, ref continuationToken, "Microsoft");
                // logger.Write("Number of blobs in the segment: {0}", blobList.Results.Count());

                // JObject googleSearch = JObject.Parse(googleSearchText);
                // string[] blobListAsText = BlobsToStrings(blobList, logger, folderPath, ref continuationToken);
                // output = FlattenBlobs(blobList, logger, folderPath, ref continuationToken);
                output += BlobsToString(blobList, logger, folderPath, ref continuationToken);
                // logger.Write("Number of blobs in the list: {0}", blobListAsText.Length);
                // logger.Write("First blob in the list: {0}", blobListAsText[0]);

            } while (continuationToken != null);
            // logger.Write(output);
            // logger.Write("Output string length: {0}", output.Length);

            // get the output dataset using the name of the dataset matched to a name in the Activity output collection.
            // Dataset outputDataset = datasets.Single(dataset => dataset.Name == activity.Outputs.Single().Name);
            Dataset outputDataset = datasets.Single(dataset => dataset.Name == outputName);

            // get type properties for the output dataset
            // outputTypeProperties = outputDataset.Properties.TypeProperties as AzureBlobDataset;

            AzureStorageLinkedService outputLinkedService;

            outputLinkedService = linkedServices.Single(
                    linkedService =>
                    linkedService.Name == outputDataset.Properties.LinkedServiceName
                ).Properties.TypeProperties
                as AzureStorageLinkedService;

            // get the folder path from the output dataset definition
            folderPath = GetFolderPath(outputDataset);
            // log the output folder path   
            // logger.Write("Writing blob to the folder: {0}", folderPath);

            // create a storage object for the output blob.
            string outputConnectionString = outputLinkedService.ConnectionString;
            CloudStorageAccount outputStorageAccount = CloudStorageAccount.Parse(outputConnectionString);
            // write the name of the file.
            Uri outputBlobUri = new Uri(outputStorageAccount.BlobEndpoint, folderPath + "/" + GetFileName(outputDataset));

            // log the output file name
            // logger.Write("output blob URI: {0}", outputBlobUri.ToString());

            output = output.Replace(Environment.NewLine, String.Empty);
            // output = output.Substring(1);
            // output = output.Substring(0, output.Length-2);
            string[] blobs = output.Split(new string[] { "}{" }, StringSplitOptions.RemoveEmptyEntries);
            // logger.Write("Number of blobs in the list: {0}", blobs.Length);
            // logger.Write(String.Format("Second blob in the list: {0}", blobs[1]));
            // logger.Write("Third blob in the list: {0}", blobs[2]);
            // logger.Write("First blob in the list: {0}", blobs[0]);
            // TextSample testi = JsonConvert.DeserializeObject<TextSample>(blobs[2]);
            string outputStringCsv = "Source, IsScience, Text" + Environment.NewLine;
            // blobs[0] = String.Empty;
            /*
            foreach (var blobString in blobs)
            {
                // logger.Write("Blob: {0}", blobString);
                TextSample testi = JsonConvert.DeserializeObject<TextSample>("{" + blobString + "}");
                // logger.Write("Text: {0}", testi.text);
                // logger.Write("Source: {0}", testi.source);
                string outputText = String.Join("", testi.text.Split(',' ,'.' ,';', '\'', '@', '#'));
                outputStringCsv += testi.source + ", " + testi.isScience + ", " + outputText + Environment.NewLine;
            }
            */
            for (int idx = 1; idx <= blobs.Length - 2; idx++)
            {
                TextSample testi = JsonConvert.DeserializeObject<TextSample>("{" + blobs[idx] + "}");
                string outputText = String.Join("", testi.text.Split(',', '.', ';', '\'', '@', '#'));
                outputStringCsv += testi.source + ", " + testi.isScience + ", " + outputText + Environment.NewLine;
            }

            // create a blob and upload the output text.
            CloudBlockBlob outputBlob = new CloudBlockBlob(outputBlobUri, outputStorageAccount.Credentials);
            // logger.Write("Writing {0} characters to the output blob", output.Length);
            outputBlob.UploadText(outputStringCsv);
            // var jObj = TextSample.Parse(json);

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

            // get type properties of the dataset   
            AzureBlobDataset blobDataset = dataArtifact.Properties.TypeProperties as AzureBlobDataset;
            if (blobDataset == null)
            {
                return null;
            }

            // return the folder path found in the type properties
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

            // get type properties of the dataset
            AzureBlobDataset blobDataset = dataArtifact.Properties.TypeProperties as AzureBlobDataset;
            if (blobDataset == null)
            {
                return null;
            }

            // return the blob/file name in the type properties
            return blobDataset.FileName;
        }

        /// <summary>
        /// Iterates through each blob (file) in the folder, counts the number of instances of search term in the file,
        /// and prepares the output text that is written to the output blob.
        /// </summary>
        /*
        public static string Calculate(BlobResultSegment Bresult, IActivityLogger logger, string folderPath, ref BlobContinuationToken token, string searchTerm)
        {
            string output = string.Empty;
            logger.Write("number of blobs found: {0}", Bresult.Results.Count<IListBlobItem>());
            foreach (IListBlobItem listBlobItem in Bresult.Results)
            {
                CloudBlockBlob inputBlob = listBlobItem as CloudBlockBlob;
                if ((inputBlob != null) && (inputBlob.Name.IndexOf("$$$.$$$") == -1))
                {
                    string blobText = inputBlob.DownloadText(Encoding.ASCII, null, null, null);
                    logger.Write("input blob text: {0}", blobText);
                    string[] source = blobText.Split(new char[] { '.', '?', '!', ' ', ';', ':', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    var matchQuery = from word in source
                                     where word.ToLowerInvariant() == searchTerm.ToLowerInvariant()
                                     select word;
                    int wordCount = matchQuery.Count();
                    output += string.Format("{0} occurrences(s) of the search term \"{1}\" were found in the file {2}.\r\n", wordCount, searchTerm, inputBlob.Name);
                }
            }
            return output;
        }
        */

        public static string BlobsToString(BlobResultSegment Bresult, IActivityLogger logger, string folderPath, ref BlobContinuationToken token)
        {
            string output = string.Empty;
            // List<string> output = new List<string>();
            // logger.Write("number of blobs in the segment: {0}", Bresult.Results.Count<IListBlobItem>());
            foreach (IListBlobItem listBlobItem in Bresult.Results)
            {
                CloudBlockBlob inputBlob = listBlobItem as CloudBlockBlob;
                if ((inputBlob != null) && (inputBlob.Name.IndexOf("$$$.$$$") == -1))
                {
                    string blobText = inputBlob.DownloadText( /*Encoding.UTF8, null, null, null*/ );
                    output += blobText;
                    // output.Add(blobText);
                    // JObject o = JObject.Parse(blobText);
                    // logger.Write("First character of string: {0}", blobText.Substring(0, 1));
                    // logger.Write("Attempting to parse JSON: {0}", blobText);
                    // TextSample testi = JsonConvert.DeserializeObject<TextSample>(blobText);
                    // logger.Write("Text: {0}", testi.text);
                }
            }
            return output;
            // return output.ToArray();
        }
    }
}
