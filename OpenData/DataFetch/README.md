# Experiments

Before deploying the function to Azure, add the Storage Account connection string to the App Settings with the name **BlobStorage**.

When running the function locally, add the file *local.settings.json* to the project root with the following content:

> {
>   "IsEncrypted": false,
>   "ConnectionStrings": {
>     "BlobStorage": "DefaultEndpointsProtocol=https;AccountName=mystorageaccount;AccountKey=myaccountkey;EndpointSuffix=core.windows.net"
>   }
> }
