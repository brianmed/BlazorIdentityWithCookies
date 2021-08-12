using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace IdentityWithCookies.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped<CustomHttpMessageHandler>();
            builder.Services.AddHttpClient("JwtClient", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress + "v1/"))
                .AddHttpMessageHandler<CustomHttpMessageHandler>();
            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("JwtClient"));

            builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();

            builder.Services.AddAuthorizationCore();
            
            await builder.Build().RunAsync();
        }
    }

    public class CustomHttpMessageHandler : DelegatingHandler
    {
        private IJSRuntime JSRuntime { get; init; }

        public CustomHttpMessageHandler(IJSRuntime jSRuntime)
        {
            JSRuntime = jSRuntime;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string bearer = await JSRuntime.InvokeAsync<string>("getCookie", "JwtBearer");

            request.Headers.Add("Bearer", bearer);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
