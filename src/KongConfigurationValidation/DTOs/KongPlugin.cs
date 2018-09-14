using System.Collections.Generic;
using Newtonsoft.Json;

namespace KongConfigurationValidation.DTOs
{
	public sealed class KongPlugin
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("created_at")]
		public long? CreatedAt { get; set; }

		[JsonProperty("consumer_id")]
		public string ConsumerId { get; set; }

		[JsonProperty("service_id")]
		public string ServiceId { get; set; }

		[JsonProperty("route_id")]
		public string RouteId { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("config")]
		public Dictionary<string, object> Config { get; set; }

		public override string ToString()
		{
			return $"Id: {Id}, Name: {Name}";
		}
	}
}
