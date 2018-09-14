using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KongConfigurationValidation.DTOs;
using Newtonsoft.Json;

namespace KongConfigurationValidation.Helpers
{
    public interface ITestFileHelper
    {
        IEnumerable<Test> ParseTestFile(string filename);
        IEnumerable<Test> ParseTestFiles(List<string> testFiles);
        bool GetFiles(string dataPath, out List<string> files);
    }

    public class TestFileHelper : ITestFileHelper
    {
        private const string FileExtension = ".json";

        public bool GetFiles(string dataPath, out List<string> files)
        {
            try
            {
                files = Directory.EnumerateFiles(dataPath, $"*{FileExtension}", SearchOption.AllDirectories).ToList();
            }
            catch (Exception)
            {
                files = new List<string>();
                return false;
            }
            return true;
        }

        public IEnumerable<Test> ParseTestFiles(List<string> testFiles)
        {
            var tests = new List<Test>();
            foreach (var testFile in testFiles)
            {
                tests.AddRange(ParseTestFile($"{testFile}"));
            }
            return tests;
        }

        public IEnumerable<Test> ParseTestFile(string filename)
        {
            var text = File.ReadAllText($"{filename}");
            var testFile = JsonConvert.DeserializeObject<TestFile>(text);

            testFile.Service = Path.GetFileNameWithoutExtension(filename);

            return (from testGroup in testFile.Tests
                from request in testGroup.requests
                select new Test
                {
                    Route = testGroup.Route,
                    Service = testFile.Service,
                    RequestUri = request.Uri,
                    Method = testGroup.Method,
                    Payload = request.Payload,
                    Headers = request.Headers
                }).ToList();
        }
    }
}
