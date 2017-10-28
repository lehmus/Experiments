using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DataManager.CustomActivities.Models
{
    public class TextSample
    {
        public string text { get; set; }
        public bool isScience { get; set; }
        public string source { get; set; }
    }
}
