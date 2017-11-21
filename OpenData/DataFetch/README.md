# OpenData: DataFetch

Azure Functions project for querying an open data API and save the results to Blob Storage.

Before deploying the function to Azure, add the Storage Account connection string to the App Settings with the name **BlobStorage**, and add the parameters **StatFinDomain** and **StatFinApiPath**.

When running the function locally, add the file *local.settings.json* to the project root with the following content:

```json
{
  "IsEncrypted": false,
  "Values": {
    "StatFinDomain": "http://pxnet2.stat.fi",
    "StatFinApiPath": "/PXWeb/api"
  },
  "ConnectionStrings": {
    "BlobStorage": "DefaultEndpointsProtocol=https;AccountName=mystorageaccount;AccountKey=myaccountkey;EndpointSuffix=core.windows.net"
  }
}
```

Fetch data from the StatFin API
> http://localhost:7071/api/datafetch/tilastokeskus/StatFin/ene/ehk/statfin_ehk_pxt_001_fi/1970/2015

Update master data
> http://localhost:7071/api/updatemasterdata/tilastokeskus/StatFin/ene/ehk/statfin_ehk_pxt_001_fi

