using System.IO;
using KongConfigurationValidation.DTOs;
using KongConfigurationValidation.Helpers;
using KongConfigurationValidation.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;

namespace KongConfigurationValidation
{
    public class Startup
    {
        private readonly TestTracker _testTracker;

        public Startup(TestTracker testTracker)
        {
            _testTracker = testTracker;

	        Log.Logger = new LoggerConfiguration()
		        .WriteTo.Console()
		        .CreateLogger();
        }

        // This Method gets called by the runtime. Use this Method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
		        .SetBasePath(Directory.GetCurrentDirectory())
		        .AddJsonFile("appsettings.json")
		        .Build();
			services.Configure<Settings>(x => configuration.Bind(nameof(Settings), x));

            services.Replace(new ServiceDescriptor(typeof(TestTracker), _testTracker));
            services.AddSingleton<ITestFileHelper, TestFileHelper>();
            services.AddSingleton<ITestHelper, TestHelper>();

	        services
		        .AddTransient<EnsureSuccessHandler>()
		        .AddHttpClient<KongAdminClient>()
		        .AddHttpMessageHandler<EnsureSuccessHandler>();
			
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This Method gets called by the runtime. Use this Method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
