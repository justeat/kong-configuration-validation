using System;
using System.Net.Http;
using System.Threading.Tasks;
using KongConfigurationValidation.DTOs;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;

namespace KongConfigurationValidation.Helpers
{
	public class KongAdminClient
	{
		private readonly HttpClient _httpClient;

		public KongAdminClient(HttpClient httpClient, IOptions<Settings> options)
		{
			_httpClient = httpClient;
			var settings = options.Value;
			_httpClient.BaseAddress = new Uri($"{settings.KongHost}:{settings.KongAdminPort}");
		}

		public async Task UpsertPlugin(KongPlugin plugin)
		{
			Log.Information(string.IsNullOrWhiteSpace(plugin.Id) ? $"Adding plugin {plugin}" : $"Updating plugin {plugin}");

			var content = new StringContent(JsonConvert.SerializeObject(plugin));
			try
			{
				var response = await _httpClient.PutAsync("/plugins", content);
				var updated = await response.Content.ReadAsAsync<KongPlugin>();
				plugin.Id = updated.Id;
			}
			catch (Exception e)
			{
				Log.Error(e, e.Message);
				throw;
			}
		}

		public async Task DeletePlugin(string pluginId)
		{
			Log.Information($"Deleting plugin with Id: {pluginId}");
			try
			{
				await _httpClient.DeleteAsync($"/plugins/{pluginId}");
			}
			catch (Exception e)
			{
				Log.Error(e, e.Message);
				throw;
			}
		}
	}
}
