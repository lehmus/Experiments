namespace DataManager.Tests.Mockdata
{
    static class AnalyzableJsonData
    {
        public static string scienceData = @"{""isScience"":true,""source"":""reddit"",""text"":""Curiosity Rover Drills Into Mars Rock, Finds Water...and, in that same sample, the first discovery of organic compounds.""}";
        public static string sportData = @"{""isScience"":false,""isSport"":true,""source"":""reddit"",""text"":""Curiosity Rover Drills Into Mars Rock, Finds Water...and, in that same sample, the first discovery of organic compounds.""}";
        public static string environmentData = @"{""isScience"":false,""isSport"":false, ""isEnvironment"":true,""source"":""reddit"",""text"":""Curiosity Rover Drills Into Mars Rock, Finds Water...and, in that same sample, the first discovery of organic compounds.""}";
    }
}
