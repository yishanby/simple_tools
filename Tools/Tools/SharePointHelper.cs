using Azure.Core;
using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace SharePointAccess
{
    public class SharePointHelper
    {
        protected string baseUrl = "https://graph.microsoft.com/beta";
        protected AuthOptions options = null;
        protected HttpClient client = new HttpClient();
        protected IAccount account = null;
        protected IPublicClientApplication app = null;

        public SharePointHelper(AuthOptions options)
        {
            this.options = options;
            string authority = $"https://login.microsoftonline.com/{this.options.TenantId}";

            app = PublicClientApplicationBuilder.Create(this.options.ClientId)
                  .WithAuthority(authority)
                  .Build();
        }


        public async Task<string> GetItemId(string siteId, string relativePath)
        {
            var itemPath = relativePath.Replace("\\", "/");
            var url = $"{baseUrl}/sites/{siteId}/drive/root:/{itemPath}";
            var result = await GraphRequest(url);
            return GetProperty(result, "id");
        }


        public async Task<string> ListChildren(string siteId, string itemId)
        {
            var url = $"{baseUrl}/sites/{siteId}/drive/items/{itemId}/children";
            return await GraphRequest(url);
        }

        public async Task<Stream> GetItemContent(string siteId, string itemId)
        {
            var url = $"{baseUrl}/sites/{siteId}/drive/items/{itemId}/content";
            return await GetResponseStream(url);
        }

        public async Task<Stream> GetItemContentByShareUrl(string shareUrl)
        {
            var url = $"{baseUrl}/shares/{shareUrl}/driveItem/Content";
            return await GetResponseStream(url);
        }


        public async Task<string> ReplaceFileContent(string siteId, string itemId, Stream stream)
        {
            /*  To Replace
             *  PUT /drives/{drive-id}/items/{item-id}/content
                PUT /groups/{group-id}/drive/items/{item-id}/content
                PUT /me/drive/items/{item-id}/content
                PUT /sites/{site-id}/drive/items/{item-id}/content
                PUT /users/{user-id}/drive/items/{item-id}/content
             * 
             * */
            var url = $"{baseUrl}/sites/{siteId}/drive/items/{itemId}/content";
            return await PutStream(url, stream);
        }

        public async Task<string> UploadFileContent(string siteId, string parentId, string fileName, Stream stream)
        {
            /*
             *  To Upload
             *  PUT /drives/{drive-id}/items/{parent-id}:/{filename}:/content
                PUT /groups/{group-id}/drive/items/{parent-id}:/{filename}:/content
                PUT /me/drive/items/{parent-id}:/{filename}:/content
                PUT /sites/{site-id}/drive/items/{parent-id}:/{filename}:/content
                PUT /users/{user-id}/drive/items/{parent-id}:/{filename}:/content
             */

            var url = $"{baseUrl}/sites/{siteId}/drive/items/{parentId}:/{fileName}:/content";
            return await PutStream(url, stream);
        }


        public async Task<string> GetDriveId(string siteId)
        {
            if (!string.IsNullOrEmpty(siteId))
            {
                var driveRequest = $"{baseUrl}/sites/{siteId}/drive?select=id";
                var driveInfo = await GraphRequest(driveRequest);
                var driveId = GetProperty(driveInfo, "id");
                return driveId;
            }
            return null;
        }

        public string GetProperty(string input, string property)
        {
            JObject json = JObject.Parse(input);
            return json[property].ToString();
        }


        public async Task<string> GetSiteInfo(string host, string site)
        {
            var url = $"sites/{host}:/sites/{site}";
            return await GraphRequest(url);
        }

        public async Task<string> GetSiteId(string host, string site)
        {
            var url = $"sites/{host}:/sites/{site}?select=id";
            var result = await GraphRequest(url);
            return GetProperty(result, "id");
        }

        public async Task WriteStreamToLocalFileAsync(Stream inputStream, string outputPath)
        {
            using (FileStream fileStream = new FileStream(outputPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                await inputStream.CopyToAsync(fileStream);
            }
        }

        internal async Task<string> GraphRequest(string url)
        {
            var token = await this.GetGraphToken();

            if (!url.StartsWith(baseUrl))
            {
                url = $"{baseUrl}/{url}";
            }

            return await GetResponseString(client, token, url);

        }

        internal async Task<string> PutStream(string url, Stream stream)
        {
            var token = await this.GetGraphToken();
            if (!url.StartsWith(baseUrl))
            {
                url = $"{baseUrl}/{url}";
            }

            this.client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            HttpContent content = new StreamContent(stream);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, url);
            request.Content = content;
            var response = await this.client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return response.Content.ReadAsStringAsync().Result;
            }

            return string.Empty;
        }

        internal async Task<string> GetGraphToken()
        {
            return await this.GetGraphToken(
                options.TenantId,
                options.ClientId,
                options.UserName,
                options.Password,
                options.Scopes);
        }



        internal async Task<string> GetResponseString(HttpClient httpClient, string token, string url)
        {
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            return await httpClient.GetStringAsync(url);
        }

        internal async Task<Stream> GetResponseStream(string url)
        {
            var token = await this.GetGraphToken();
            return await this.GetResponseStream(this.client, token, url);
        }

        internal async Task<Stream> GetResponseStream(HttpClient httpClient, string token, string url)
        {
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            return await httpClient.GetStreamAsync(url);
        }

        internal GraphServiceClient GraphServiceClient(string token)
        {
            var tokenCredential = new TokenCredentialImplementation(token);
            return new GraphServiceClient(tokenCredential);
        }

        internal GraphServiceClient GraphServiceClient(AuthOptions options)
        {
            var credentials = new UsernamePasswordCredential(options.UserName, options.Password, options.TenantId, options.ClientId);
            var graphClient = new GraphServiceClient(credentials, options.Scopes);
            return graphClient;
        }

        internal async Task<string> GetGraphToken(string tenantId, string clientId, string userName, string password, string[] scopes)
        {
            AuthenticationResult result = null;

            if (this.account != null)
            {
                result = await app.AcquireTokenSilent(scopes, account)
                                 .ExecuteAsync();
            }
            else
            {
                result = await app.AcquireTokenByUsernamePassword(scopes, userName, password).ExecuteAsync();
                var accounts = await app.GetAccountsAsync();
                var account = accounts.FirstOrDefault(i => i.Username == userName);
                if (account != null)
                {
                    this.account = account;
                }
            }

            return result?.AccessToken;
        }

    }

    public class AuthOptions
    {
        public string TenantId { get; set; }
        public string ClientId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string[] Scopes { get; set; }
    }

    public class TokenCredentialImplementation : TokenCredential
    {
        private readonly string _token;

        public TokenCredentialImplementation(string token)
        {
            _token = token;
        }

        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            return new ValueTask<AccessToken>(new AccessToken(_token, DateTimeOffset.MaxValue));
        }

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            return new AccessToken(_token, DateTimeOffset.MaxValue);
        }
    }
}
