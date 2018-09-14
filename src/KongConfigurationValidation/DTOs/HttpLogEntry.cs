using System.Collections.Generic;
using Newtonsoft.Json;

namespace KongConfigurationValidation.DTOs
{

    public class HttpLogEntry
    {
        public Latencies latencies { get; set; }
        public Service service { get; set; }
        public Request request { get; set; }
        public string client_ip { get; set; }
        public Api api { get; set; }
        public string upstream_uri { get; set; }
        public Response response { get; set; }
        public Route route { get; set; }
        public long started_at { get; set; }
    }

    public class Latencies
    {
        public int request { get; set; }
        public int kong { get; set; }
        public int proxy { get; set; }
    }

    public class Service
    {
        public string host { get; set; }
        public int created_at { get; set; }
        public int connect_timeout { get; set; }
        public string id { get; set; }
        public string protocol { get; set; }
        public string name { get; set; }
        public int read_timeout { get; set; }
        public int port { get; set; }
        public object path { get; set; }
        public int updated_at { get; set; }
        public int retries { get; set; }
        public int write_timeout { get; set; }
    }

    public class Request
    {
        public Querystring querystring { get; set; }
        public string size { get; set; }
        public string uri { get; set; }
        public string url { get; set; }
        [JsonProperty("headers")]
        public RequestHeaders headers { get; set; }
        public string method { get; set; }
    }

    public class Querystring
    {
    }

    public class RequestHeaders : Dictionary<string, string>{}

    public class Tries
    {
    }

    public class Api
    {
    }

    public class Response
    {
        [JsonProperty("headers")]
        public ResponseHeaders headers { get; set; }
        public int status { get; set; }
        public string size { get; set; }
    }

    public class ResponseHeaders
    {
        public string connection { get; set; }
        public string contenttype { get; set; }
        public string transferencoding { get; set; }
        public string server { get; set; }
    }

    public class Route
    {
        public int created_at { get; set; }
        public bool strip_path { get; set; }
        public object[] hosts { get; set; }
        public bool preserve_host { get; set; }
        public int regex_priority { get; set; }
        public int updated_at { get; set; }
        public string[] paths { get; set; }
        public Service1 service { get; set; }
        public string[] methods { get; set; }
        public string[] protocols { get; set; }
        public string id { get; set; }
    }

    public class Service1
    {
        public string id { get; set; }
    }

}
