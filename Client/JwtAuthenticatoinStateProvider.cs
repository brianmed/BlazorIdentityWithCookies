using System;
using System.Net.Http;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace IdentityWithCookies.Client
{
    public class JwtAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime JSRuntime;
        private readonly HttpClient httpClient;

        private AuthenticationState Anonymous => new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        public JwtAuthenticationStateProvider(IJSRuntime jSRuntime)
        {
            JSRuntime = jSRuntime;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            string jwtBearer = await JSRuntime.InvokeAsync<string>("getCookie", "JwtBearer");

            if (String.IsNullOrEmpty(jwtBearer))
            {
                return Anonymous;
            }

            return BuildAuthenticationState(jwtBearer);
        }

        public AuthenticationState BuildAuthenticationState(string jwtBearer)
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(jwtBearer), "jwt")));
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

            JwtSecurityToken token = handler.ReadJwtToken(jwt);

            return token.Claims;
        }

        public async Task Login()
        {
            string jwtBearer = await JSRuntime.InvokeAsync<string>("getCookie", "JwtBearer");

            AuthenticationState authState = BuildAuthenticationState(jwtBearer);

            NotifyAuthenticationStateChanged(Task.FromResult(authState));
        }

        public async Task Logout()
        {
            await JSRuntime.InvokeVoidAsync("clearCookie", "JwtBearer");

            NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
        }
    }
}
