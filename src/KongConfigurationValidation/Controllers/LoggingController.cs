using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KongConfigurationValidation.DTOs;
using KongConfigurationValidation.Helpers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace KongConfigurationValidation.Controllers
{
    [Route("/")]
    [ApiController]
    public class LoggingController : ControllerBase
    {
        private readonly ITestHelper _testHelper;

        public LoggingController(ITestHelper testHelper)
        {
            _testHelper = testHelper;
        }

        [HttpPost]
        [Route("{*path}")]
        public async Task<ActionResult<string>> Post(string path)
        {
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                try
                {
                    var result = await reader.ReadToEndAsync();
                    var logEntry = JsonConvert.DeserializeObject<HttpLogEntry>(result, new JsonSerializerSettings { NullValueHandling  = NullValueHandling.Ignore, MissingMemberHandling = MissingMemberHandling.Ignore });

                    if (!logEntry.request.headers.ContainsKey("test-id") || string.IsNullOrWhiteSpace(logEntry.request.headers["test-id"]) ||!_testHelper.Tests.TestResultTasks.ContainsKey(logEntry.request.headers["test-id"])) return "";

                    var matchedRoute = logEntry.route == null? "NoMatch" : logEntry.route.paths.FirstOrDefault();

                    var serviceName = logEntry.service == null? "NoMatch" : logEntry.service.name;
                    _testHelper.Tests.TestResultTasks[logEntry.request.headers["test-id"]].SetResult(new Test
                    {
                        Route = matchedRoute,
                        RequestUri = logEntry.request.uri,
                        Service = serviceName,
                        Method = logEntry.request.method,
                        Headers = logEntry.request.headers
                    });

                }
                catch
                {
                    // ignored
                }

                return "";
            }
        }

        [HttpGet]
        [Route("/")]
        public ActionResult<string> Get()
        {
            return new ActionResult<string>($"Kongverge Test Endpoint\n\n");
        }

    }
}
