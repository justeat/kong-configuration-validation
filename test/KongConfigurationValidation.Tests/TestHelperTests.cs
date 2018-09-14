using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using KongConfigurationValidation.DTOs;
using KongConfigurationValidation.Helpers;
using Microsoft.Extensions.Options;
using Moq;
using Serilog;
using Serilog.Events;
using Xunit;

namespace KongConfigurationValidation.Tests
{
    public class TestHelperTests
    {
        public TestHelper GenerateTestHelper(TestTracker testTracker = null)
        {
            if (testTracker == null) testTracker = new TestTracker();
            var testFileHelper = new Mock<ITestFileHelper>();
            var config = new Settings { KongHost = "localhost" };
            var settings = Options.Create(config);
            return new TestHelper(testTracker, testFileHelper.Object, settings);
        }

        [Fact]
        public void AddTestAddsTest()
        {
            //Arrange
            var sut = GenerateTestHelper();
            var test = new Test
            {
                Headers = new Dictionary<string, string> { { "Accept-Language", "en-GB" } },
                Route = "/test/route",
                Method = "PUT"
            };

            //Act
            sut.AddTest(test);

            //Assert
            Assert.Equal(1, sut.Tests.TestResultTasks.Count);
        }

        [Fact]
        public async Task UnSupportedMethodsThrow()
        {
            //Arrange
            var sut = GenerateTestHelper();
            var test = new Test
            {
                Headers = new Dictionary<string, string> { { "Accept-Language", "en-GB" } },
                Route = "/test/route",
                Method = "FOO"
            };

            sut.AddTest(test);
            //Act//Assert
            await Assert.ThrowsAsync<MethodNotImplementedException>(() => sut.RunTests(new FakeHttpMessageHandler()));
        }

        [Fact]
        public async Task ValidatesCorrectly()
        {
            //Arrange
            var logEvents = new List<LogEvent>();

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Observers(e => e.Subscribe(x => logEvents.Add(x)))
                .CreateLogger();

            var test1 = new Test
            {
                Route = "/testRoute",
                RequestUri = "/testRoute",
                Service = "goodservice",
                Method = "GET",
                Headers = null
            };
            var test2 = new Test
            {
                Route = "/testRoute",
                RequestUri = "/testRoute",
                Service = "badservice",
                Method = "PUT",
                Headers = null
            };

            var testList = new Dictionary<string, Test> { { "test1", test1 }, { "test2", test2 }, };
            var resultList = new Dictionary<string, TaskCompletionSource<Test>>
            {
                {"test1", new TaskCompletionSource<Test>()},
                {"test2", new TaskCompletionSource<Test>()}
            };
            resultList["test1"].SetResult(test1);
            resultList["test2"].SetResult(test1);

            var testTracker = new TestTracker
            {
                TestList = testList,
                TestResultTasks = resultList
            };
            var sut = GenerateTestHelper(testTracker);

            //Act
            await sut.Validate();

            //Assert
            Assert.Equal("goodservice matched expectations", logEvents[0].MessageTemplate.Text);
            Assert.Equal(LogEventLevel.Information, logEvents[0].Level);

            Assert.Contains("badservice failed expectations", logEvents[1].MessageTemplate.Text);
            Assert.Equal(LogEventLevel.Error, logEvents[1].Level);
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("POST")]
        [InlineData("PATCH")]
        [InlineData("PUT")]
        [InlineData("DELETE")]
        public async void CorrectMethodWasCalled(string method)
        {
            //Arrange
            var httpMethod = HttpMethod.Get;
            switch (method)
            {
                case "GET":
                    httpMethod = HttpMethod.Get;
                    break;
                case "POST":
                    httpMethod = HttpMethod.Post;
                    break;
                case "PATCH":
                    httpMethod = HttpMethod.Patch;
                    break;
                case "PUT":
                    httpMethod = HttpMethod.Put;
                    break;
                case "DELETE":
                    httpMethod = HttpMethod.Delete;
                    break;
            }

            var requestMessage = new HttpRequestMessage();

            var messageHandlerMock = new Mock<FakeHttpMessageHandler> { CallBase = true };
            messageHandlerMock.Setup(x => x.Send(It.IsAny<HttpRequestMessage>()))
                .Callback<HttpRequestMessage>(obj => requestMessage = obj)
                .Returns(new HttpResponseMessage(HttpStatusCode.OK));

            var sut = GenerateTestHelper();

            var test = new Test
            {
                Headers = new Dictionary<string, string> { { "Accept-Language", "en-GB" } },
                Route = "/test/route",
                Payload = "sometext",
                Method = method
            };
            var testHttpClient = new HttpClient(messageHandlerMock.Object);

            //Act
            await sut.RunTest(new KeyValuePair<string, Test>("test", test), testHttpClient);

            //Assert
            messageHandlerMock.Verify(x => x.Send(It.IsAny<HttpRequestMessage>()), Times.Once);
            Assert.Equal(httpMethod, requestMessage.Method);
        }
    }
}

