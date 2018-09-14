using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using KongConfigurationValidation.DTOs;
using KongConfigurationValidation.Helpers;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;

namespace KongConfigurationValidation
{
    public class Program
    {
        private static ITestHelper _testHelper;
	    private static KongAdminClient _kongAdminClient;
        private static KongPlugin _httpLogPlugin;
        
        public static int Main(string[] args)
        {
            //Ensure we definitely remove that plugin, even if we crash
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
            Console.CancelKeyPress += CancelHandler;

            var app = new CommandLineApplication
            {
                Name = "KongConfigurationValidation",
                Description = "Kong configuration validation."
            };

            app.HelpOption("-?|-h|--help");

	        var kongHostOption = app.Option("-H|--host <KongHostname>",
		        "Specify folder containing tests",
		        CommandOptionType.SingleValue);

            var testsFolderOption = app.Option("-t|--testsFolder <testsFolder>",
                "Specify folder containing tests",
                CommandOptionType.SingleValue);

            var portOption = app.Option("-p|--port <HttpLogPort>",
                "Specify local logging listening port",
                CommandOptionType.SingleValue);

            app.OnExecute(async () =>
            {
                var services = new ServiceCollection();
                var testTracker = new TestTracker();
                var startup = new Startup(testTracker);
                startup.ConfigureServices(services);
                var serviceProvider = services.BuildServiceProvider();

                _testHelper = serviceProvider.GetService<ITestHelper>();
	            _kongAdminClient = serviceProvider.GetService<KongAdminClient>();

                var configuration = serviceProvider.GetService<IOptions<Settings>>().Value;

	            if (kongHostOption.HasValue())
		            configuration.KongHost = kongHostOption.Value();
				else if (string.IsNullOrWhiteSpace(configuration.KongHost))
					throw new Exception("Kong hostname is not specified.");

                if (portOption.HasValue())
                    configuration.HttpLogPort = int.Parse(portOption.Value());

                if (testsFolderOption.HasValue())
                    configuration.TestsFolder = testsFolderOption.Value();
                
                //Setup HTTP Logging Plugin
                var localIp = GetMostLikelyIpAddress().ToString();
                var httpEndpoint = $"http://{localIp}:{configuration.HttpLogPort}";
                Log.Information($"Adding HTTP Logging Plugin with url: {httpEndpoint}");
	            _httpLogPlugin = CreateHttpLogPlugin(httpEndpoint);
                await _kongAdminClient.UpsertPlugin(_httpLogPlugin);

                await Task.Delay(5000);

                //Start Logging WebServer
	            BuildWebHost(args, testTracker, configuration.HttpLogPort).Start();
                await WaitForWebServer(configuration.HttpLogPort);
                
                _testHelper.PopulateTests();
                await _testHelper.RunTests();
                await _testHelper.Validate();
#if DEBUG
                Console.WriteLine("Press any key to continue");
                Console.ReadKey();
#endif
                return 0;
            });

            return app.Execute(args);
        }

	    private static KongPlugin CreateHttpLogPlugin(string httpEndpoint)
	    {
		    var plugin = new KongPlugin
		    {
			    Name = "http-log",
			    Config = new Dictionary<string, object>
			    {
				    {"http_endpoint", httpEndpoint}
			    }
		    };
		    return plugin;
	    }


	    private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            RemovePlugin();
        }

        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            RemovePlugin();
        }

        private static void CancelHandler(object sender, ConsoleCancelEventArgs args)
        {
            RemovePlugin();
        }

        private static void RemovePlugin()
        {
            _kongAdminClient?.DeletePlugin(_httpLogPlugin.Id);
        }

        private static IPAddress GetMostLikelyIpAddress()
        {
            var gateway = GetDefaultGateway().ToString();

            var hostName = Dns.GetHostName();  // Resolving Host name
            var ipHostEntry = Dns.GetHostEntry(hostName);
            var addressList = ipHostEntry.AddressList;// Resolving IP Addresses

            foreach (var ipAddress in addressList)
            {
                if (ipAddress.ToString().Substring(0, 6) == gateway.Substring(0, 6))  //dirty match for most likely IPAddress
                    return ipAddress;
            }

            throw new Exception("Unable to determine monitoring IP Address.");
        }

        public static IPAddress GetDefaultGateway()
        {
            return NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up)
                .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .SelectMany(n => n.GetIPProperties()?.GatewayAddresses)
                .Select(g => g?.Address)
                .FirstOrDefault(a => (a != null && a.AddressFamily != AddressFamily.InterNetworkV6));
        }

        private static async Task WaitForWebServer(int port)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri($"http://localhost:{port}");
                var success = false;
                do
                {
                    try
                    {
                        var response = await httpClient.GetAsync("/");
                        success = response.StatusCode == HttpStatusCode.OK;
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, ex.Message);
                    }
                    await Task.Delay(100);
                } while (!success);
            }
        }

        private static IWebHost BuildWebHost(string[] args, TestTracker testTracker, int port)
        {
            return WebHost
                .CreateDefaultBuilder(args)
                .UseUrls($"http://*:{port}", $"http://0.0.0.0:{port}")
                .UseKestrel()
                .UseIISIntegration()
                .UseStartup<Startup>()
                .ConfigureServices(services => services.AddSingleton(testTracker))
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                })
                .Build();
        }
    }
}
