namespace DataFetch.RequestModels
{
    public class TableQuery
    {
        public TableQuerySettings[] query { get; set; }
        public TableResponseSettings response { get; set; }
    }

    public class TableQuerySettings
    {
        public string code { get; set; }
        public FilterSelection selection { get; set; }
    }

    public class FilterSelection
    {
        public string filter { get; set; }
        public string[] values { get; set; }
    }

    public class TableResponseSettings
    {
        public string format { get; set; }
    }
}
