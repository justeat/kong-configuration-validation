using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using KongConfigurationValidation.DTOs;
using Microsoft.Extensions.Options;
using Serilog;

namespace KongConfigurationValidation.Helpers
{
    public interface ITestHelper
    {
        TestTracker Tests { get; set; }

        void AddTest(Test test);
        Task Validate();
        Task RunTests(HttpMessageHandler messageHandler);
        Task RunTests();
        Task RunTest(KeyValuePair<string, Test> test, HttpClient httpClient);
        void PopulateTests();
    }

    public class TestHelper : ITestHelper
    {
        public TestTracker Tests { get; set; }
        private readonly ITestFileHelper _testFileHelper;
        private readonly Settings _configuration;

        public TestHelper(TestTracker testTracker, ITestFileHelper testFileHelper, IOptions<Settings> configuration)
        {
            Tests = testTracker;
            _testFileHelper = testFileHelper;
            _configuration = configuration.Value;
        }

        public void AddTest(Test test)
        {
            var testId = Guid.NewGuid().ToString();
            Tests.TestResultTasks.Add(testId, new TaskCompletionSource<Test>());
            Tests.TestList.Add(testId, test);
        }
       
        public async Task Validate()
        {
            if (Tests != null)
            {
                Task.WaitAll(Tests.TestResultTasks.Values.Select(x => x.Task as Task).ToArray(), 30000);

                foreach (var test in Tests.TestResultTasks)
                {
                    var expected = Tests.TestList[test.Key];
                    var actual = await test.Value.Task;

                    if (expected.Equals(actual))
                    {
                        Log.Information($"{expected.Service} matched expectations");
                    }
                    else
                    {
                        Log.Error($"{expected.Service} failed expectations:");
                        Log.Error($"RequestUri: {actual.RequestUri}");
                        Log.Error($"Request Method: {actual.Method}");

                        PrintDiff(expected, actual);
                    }
                }
            }
        }

        public static void PrintDiff(Test expected, Test actual)
        {
            var diff = expected.DetailedCompare(actual);
            PrintDiff(diff);
        }

        public static void PrintDiff(IEnumerable<Variance> diff)
        {
            foreach (var variance in diff)
            {
                Log.Information($"\t{variance.Field}\tExpected: {variance.Existing}\t\tGot: {variance.New}");
            }
        }

        public Task RunTests()
        {
            return RunTests(new HttpClientHandler());
        }

        public async Task RunTests(HttpMessageHandler messageHandler)
        {
            var httpClient = new HttpClient(messageHandler);
            foreach (var test in Tests.TestList)
            {
                await RunTest(test, httpClient);
            }
        }

        public async Task RunTest(KeyValuePair<string, Test> test, HttpClient httpClient)
        {
            var requestUri = test.Value.RequestUri;
            Log.Information($"Testing {test.Value.Service} with {requestUri}");

            var httpRequestMessage = new HttpRequestMessage
            {
                RequestUri = new Uri($"http://{_configuration.KongHost}{requestUri}")
            };

            httpRequestMessage.Headers.Add("Test-id", test.Key);

            if (test.Value.Headers != null)
            {
                foreach (var header in test.Value.Headers)
                {
                    httpRequestMessage.Headers.Add(header.Key, header.Value);
                }
            }

            switch (test.Value.Method.ToUpper())
            {
                case "GET":
                {
                    httpRequestMessage.Method = HttpMethod.Get;
                    break;
                }
                case "POST":
                {
                    httpRequestMessage.Content = new StringContent(test.Value.Payload, Encoding.UTF8, "application/json");
                    httpRequestMessage.Method = HttpMethod.Post;
                    break;
                }
                case "PUT":
                {
                    httpRequestMessage.Content = new StringContent(test.Value.Payload, Encoding.UTF8, "application/json");
                    httpRequestMessage.Method = HttpMethod.Put;
                    break;
                }
                case "PATCH":
                {
                    httpRequestMessage.Content = new StringContent(test.Value.Payload, Encoding.UTF8, "application/json");
                    httpRequestMessage.Method = HttpMethod.Patch;
                    break;
                }
                case "DELETE":
                {
                    httpRequestMessage.Method = HttpMethod.Delete;
                    break;
                }
                default:
                    throw new MethodNotImplementedException("Unsupported Method");
            }

            await httpClient.SendAsync(httpRequestMessage);
        }

        public void PopulateTests()
        {
            _testFileHelper.GetFiles("tests", out var testFiles);

            var tests = _testFileHelper.ParseTestFiles(testFiles);

            foreach (var test in tests)
            {

                AddTest(test);

            }
        }
    }

    public class MethodNotImplementedException : Exception
    {
        public MethodNotImplementedException(string message) : base(message)
        {
        }

        public MethodNotImplementedException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
