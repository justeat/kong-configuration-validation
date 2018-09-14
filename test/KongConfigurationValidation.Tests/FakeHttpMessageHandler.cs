using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace KongConfigurationValidation.Tests
{
	public class FakeHttpMessageHandler : HttpMessageHandler
	{
		public virtual HttpResponseMessage Send(HttpRequestMessage request)
		{
			throw new NotImplementedException("Now we can setup this method with our mocking framework");
		}

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
		{
			return Task.FromResult(Send(request));
		}
	}
}