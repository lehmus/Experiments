namespace DataFetch.DatabaseModels
{
    public class StatFinDataset
    {
        public string[] FieldNames { get; set; }
        public StatFinDataRow[] Rows { get; set; }
    }

    public class StatFinDataRow
    {
        public string[] Features { get; set; }
        public string Value { get; set; }
    }

    public class StatFinDatasetRaw
    {
        public ColumnDefinition[] columns { get; set; }
        public StatFinDataRowRaw[] data { get; set; }
        public string[] comments { get; set; }
    }

    public class ColumnDefinition
    {
        public string code { get; set; }
        public string text { get; set; }
        public string type { get; set; }
    }

    public class StatFinDataRowRaw
    {
        public string[] key { get; set; }
        public string[] values { get; set; }
        }

    public class StatFinDatasetMeta
    {
        public string Database { get; set; }
        public string SubjectRealm { get; set; }
        public string SubjectSubRealm { get; set; }
        public string TableName { get; set; }
        public string ResultsLanguage { get; set; }
        // public int Year { get; set; }
        // public string BlobContainer { get; set; }
        // public string BlobDirectory { get; set; }
    }
}
