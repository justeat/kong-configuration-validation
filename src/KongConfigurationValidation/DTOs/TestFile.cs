using System.Collections.Generic;
using Newtonsoft.Json;

namespace KongConfigurationValidation.DTOs
{
    public class TestFile
    {
        [JsonProperty("service")]
        public string Service { get; set; }
        [JsonProperty("tests")]
        public TestFileTest[] Tests { get; set; }
    }

    public class TestFileTest
    {
        [JsonProperty("route")]
        public string Route { get; set; }
        [JsonProperty("method")]
        public string Method { get; set; }
        [JsonProperty("Requests")]
        public TestFileRequest[] requests { get; set; }
    }

    public class TestFileRequest
    {
        [JsonProperty("uri")]
        public string Uri { get; set; }
        
        [JsonProperty("headers")]
        public Dictionary<string,string> Headers { get; set; }
        [JsonProperty("payload")]
        public string Payload { get; set; }
    }

}
