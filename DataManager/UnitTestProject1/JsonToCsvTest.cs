using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataManager.CustomActivities.Activities;
using DataManager.Tests.Mockdata;
using DataManager.CustomActivities.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace UnitTestProject1
{
    [TestClass]
    public class JsonToCsvTest
    {
        private JsonToCsv jsonToCsv;

        [TestInitialize]
        public void Init()
        {
            jsonToCsv = new JsonToCsv();
        }

        [TestMethod]
        public void MapTest()
        {
            var datasToTest =  $@"
            [
                {AnalyzableJsonData.scienceData}, 
                {AnalyzableJsonData.sportData}, 
                {AnalyzableJsonData.environmentData}
            ]";

            var dataList = JsonConvert.DeserializeObject<List<TextSample>>(datasToTest);
            var result = jsonToCsv.DataMap(dataList);

            Assert.IsTrue(result != null);
        }
    }
}
