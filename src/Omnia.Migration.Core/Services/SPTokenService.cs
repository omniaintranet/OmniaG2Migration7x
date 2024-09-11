using Microsoft.Extensions.Options;
using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using Omnia.Migration.Models.Configuration;
using SharePointPnP.IdentityModel.Extensions.S2S.Protocols.OAuth2;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Omnia.Migration.Core.Services
{
    public class SPTokenService
    {
        /// <summary>
        /// SharePoint principal.
        /// </summary>
        public const string SharePointPrincipal = "00000003-0000-0ff1-ce00-000000000000";

        private const string S2SProtocol = "OAuth2";
        private const string DelegationIssuance = "DelegationIssuance1.0";
        private const string AcsMetadataEndPointRelativeUrl = "metadata/json/1";
        private static string GlobalEndPointPrefix = "accounts";
        private static string AcsHostUrl = "accesscontrol.windows.net";

        private IOptions<MigrationSettings> MigrationSettings;

        public SPTokenService(IOptions<MigrationSettings> migrationSettings)
        {
            MigrationSettings = migrationSettings;
        }



        public async ValueTask<ClientContext> CreateAppOnlyClientContextAsync(string spUrl = "")
        {
            var appAccessToken = await GetAppOnlyAccessTokenAsync(spUrl);

            ClientContext clientContext = new ClientContext(spUrl);
            RegisterClientContextToken(appAccessToken, clientContext);

            return clientContext;
        }

        private void RegisterClientContextToken(string accessToken, ClientContext clientContext)
        {
            //Hieu rem
            /*clientContext.AuthenticationMode = ClientAuthenticationMode.Anonymous;
            clientContext.FormDigestHandlingEnabled = false;
            clientContext.ExecutingWebRequest +=
                delegate (object oSender, WebRequestEventArgs webRequestEventArgs)
                {
                    //webRequestEventArgs.WebRequestExecutor.RequestHeaders["Authorization"] =
                    //    "Bearer " + accessToken;
                    //webRequestEventArgs.WebRequestExecutor.WebRequest.UserAgent = Core.Constants.App.UserAgent;
                    webRequestEventArgs.WebRequest.Headers.Add("Authorization", "Bearer " + accessToken);
                    //webRequestEventArgs.WebRequest.Headers.Add("UserAgent", Core.Constants.App.UserAgent);
                };*/
        }


        /// <summary>
        /// Gets the application only access token.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public async ValueTask<string> GetAppOnlyAccessTokenAsync(string url)
        {
            Uri webUri = new Uri(url);

            // Verify web uri
            if (webUri == null)
            {
                throw new ArgumentNullException("webUri");
            }

            // Get realm and token
            string webRealm = GetRealmFromTargetUrl(webUri);
            OAuth2AccessTokenResponse token = await GetAppOnlyAccessTokenAsync(SharePointPrincipal, webUri.Authority, webRealm);

            return token.AccessToken;
        }

        /// <summary>
        /// Retrieves an app-only access token from ACS to call the specified principal 
        /// at the specified targetHost. The targetHost must be registered for target principal.  If specified realm is 
        /// null, the "Realm" setting in web.config will be used instead.
        /// </summary>
        /// <param name="targetPrincipalName">Name of the target principal to retrieve an access token for</param>
        /// <param name="targetHost">Url authority of the target principal</param>
        /// <param name="targetRealm">Realm to use for the access token's nameid and audience</param>
        /// <returns>An access token with an audience of the target principal</returns>
        private async ValueTask<OAuth2AccessTokenResponse> GetAppOnlyAccessTokenAsync(
            string targetPrincipalName,
            string targetHost,
            string targetRealm)
        {
            string resource = GetFormattedPrincipal(targetPrincipalName, targetHost, targetRealm);
            string clientId = GetFormattedPrincipal(MigrationSettings.Value.SharePointSecuritySettings.ClientId, null, targetRealm);

            OAuth2AccessTokenRequest oauth2Request = OAuth2MessageFactory.CreateAccessTokenRequestWithClientCredentials(clientId, MigrationSettings.Value.SharePointSecuritySettings.ClientSecret, resource);
            oauth2Request.Resource = resource;

            // Get token
            OAuth2S2SClient client = new OAuth2S2SClient();
            OAuth2AccessTokenResponse oauth2Response;
            try
            {
                oauth2Response =
                    client.Issue(AcsMetadataParser.GetStsUrl(targetRealm), oauth2Request) as OAuth2AccessTokenResponse;
            }
            catch (WebException wex)
            {
                using (StreamReader sr = new StreamReader(wex.Response.GetResponseStream()))
                {
                    string responseText = await sr.ReadToEndAsync();
                    throw new WebException(wex.Message + " - " + responseText, wex);
                }
            }

            return oauth2Response;
        }

        /// <summary>
        /// Get authentication realm from SharePoint
        /// </summary>
        /// <param name="targetApplicationUri">Url of the target SharePoint site</param>
        /// <returns>String representation of the realm GUID</returns>
        private string GetRealmFromTargetUrl(Uri targetApplicationUri)
        {
            WebRequest request = WebRequest.Create(targetApplicationUri + "/_vti_bin/client.svc");
            request.Headers.Add("Authorization: Bearer ");

            try
            {
                using (request.GetResponse())
                {
                }
            }
            catch (WebException e)
            {
                if (e.Response == null)
                {
                    return null;
                }

                var response = (HttpWebResponse)e.Response;
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw e;
                }

                string bearerResponseHeader = e.Response.Headers["WWW-Authenticate"];
                if (string.IsNullOrEmpty(bearerResponseHeader))
                {
                    return null;
                }

                const string bearer = "Bearer realm=\"";
                int bearerIndex = bearerResponseHeader.IndexOf(bearer, StringComparison.Ordinal);
                if (bearerIndex < 0)
                {
                    return null;
                }

                int realmIndex = bearerIndex + bearer.Length;

                if (bearerResponseHeader.Length >= realmIndex + 36)
                {
                    string targetRealm = bearerResponseHeader.Substring(realmIndex, 36);

                    Guid realmGuid;

                    if (Guid.TryParse(targetRealm, out realmGuid))
                    {
                        return targetRealm;
                    }
                }
            }
            return null;
        }


        private string GetFormattedPrincipal(string principalName, string hostName, string realm)
        {
            if (!String.IsNullOrEmpty(hostName))
            {
                return String.Format(CultureInfo.InvariantCulture, "{0}/{1}@{2}", principalName, hostName, realm);
            }

            return String.Format(CultureInfo.InvariantCulture, "{0}@{1}", principalName, realm);
        }

        #region AcsMetadataParser

        // This class is used to get MetaData document from the global STS endpoint. It contains
        // methods to parse the MetaData document and get endpoints and STS certificate.
        public static class AcsMetadataParser
        {
            public static X509Certificate2 GetAcsSigningCert(string realm)
            {
                JsonMetadataDocument document = GetMetadataDocument(realm);

                if (null != document.keys && document.keys.Count > 0)
                {
                    JsonKey signingKey = document.keys[0];

                    if (null != signingKey && null != signingKey.keyValue)
                    {
                        return new X509Certificate2(Encoding.UTF8.GetBytes(signingKey.keyValue.value));
                    }
                }

                throw new Exception("Metadata document does not contain ACS signing certificate.");
            }

            public static string GetDelegationServiceUrl(string realm)
            {
                JsonMetadataDocument document = GetMetadataDocument(realm);

                JsonEndpoint delegationEndpoint = document.endpoints.SingleOrDefault(e => e.protocol == DelegationIssuance);

                if (null != delegationEndpoint)
                {
                    return delegationEndpoint.location;
                }
                throw new Exception("Metadata document does not contain Delegation Service endpoint Url");
            }

            private static JsonMetadataDocument GetMetadataDocument(string realm)
            {
                string acsMetadataEndpointUrlWithRealm = String.Format(CultureInfo.InvariantCulture, "{0}?realm={1}",
                                                                       GetAcsMetadataEndpointUrl(),
                                                                       realm);
                byte[] acsMetadata = null;
                using (WebClient webClient = new WebClient())
                {
                    TransientExceptionRetry(() =>
                    {
                        acsMetadata = webClient.DownloadData(acsMetadataEndpointUrlWithRealm);
                    },
                     new TransientExceptionRetryStrategy()
                     {
                         RetryCount = 3,
                         ExponentialDelayMilliseconds = 3000,
                         IncludeInnerExceptions = true,
                         TransientExceptionTypes = new List<Type>
                         {
                            typeof(WebException)
                         }
                     });
                }
                string jsonResponseString = Encoding.UTF8.GetString(acsMetadata);

                //JavaScriptSerializer serializer = new JavaScriptSerializer();
                //JsonMetadataDocument document = serializer.Deserialize<JsonMetadataDocument>(jsonResponseString);
                JsonMetadataDocument document = JsonConvert.DeserializeObject<JsonMetadataDocument>(jsonResponseString);

                if (null == document)
                {
                    throw new Exception("No metadata document found at the global endpoint " + acsMetadataEndpointUrlWithRealm);
                }

                return document;
            }

            private static void TransientExceptionRetry(Action action, TransientExceptionRetryStrategy strategy)
            {
                int currentRetry = 0;
                for (; ; )
                {
                    try
                    {
                        action.Invoke();
                        break;
                    }
                    catch (Exception ex)
                    {
                        currentRetry++;
                        // Check if the exception thrown was a transient exception
                        // based on the logic in the error detection strategy.
                        // Determine whether to retry the operation, as well as how 
                        // long to wait, based on the retry strategy.
                        if (currentRetry > strategy.RetryCount || !IsTransientException(ex, strategy))
                        {
                            // If this is not a transient error 
                            // or we should not retry re-throw the exception. 
                            throw;
                        }
                    }

                    // Calculate how long time to wait
                    var waitTime = currentRetry == 1 ?
                        strategy.RetryDelayMilliseconds :
                        strategy.RetryDelayMilliseconds + (strategy.ExponentialDelayMilliseconds * (currentRetry - 1));
                    System.Threading.Thread.Sleep(waitTime);
                }
            }

            private static bool IsTransientException(Exception ex, TransientExceptionRetryStrategy strategy)
            {
                //Check if the exception type is considered transient
                if (strategy.TransientExceptionTypes.Any(q => q == ex.GetType()))
                    return true;

                //Check if the hresult code is considered transient
                if (strategy.TransientExceptionResultCodes.Any(q => q == ex.HResult))
                    return true;

                //Check if the a custom matcher is considering this as a transient error
                if (strategy.TransientExceptionMatchers.Any(q => q.Invoke(ex)))
                    return true;

                //Check if we should recursively check inner exceptions
                if (strategy.IncludeInnerExceptions && ex.InnerException != null)
                    return IsTransientException(ex.InnerException, strategy);

                return false;
            }

            private static string GetAcsMetadataEndpointUrl()
            {
                return Path.Combine(GetAcsGlobalEndpointUrl(), AcsMetadataEndPointRelativeUrl);
            }

            private static string GetAcsGlobalEndpointUrl()
            {
                return String.Format(CultureInfo.InvariantCulture, "https://{0}.{1}/", GlobalEndPointPrefix, AcsHostUrl);
            }

            public static string GetStsUrl(string realm)
            {
                JsonMetadataDocument document = GetMetadataDocument(realm);

                JsonEndpoint s2sEndpoint = document.endpoints.SingleOrDefault(e => e.protocol == S2SProtocol);

                if (null != s2sEndpoint)
                {
                    return s2sEndpoint.location;
                }

                throw new Exception("Metadata document does not contain STS endpoint url");
            }

            private class JsonMetadataDocument
            {
                public string serviceName { get; set; }
                public List<JsonEndpoint> endpoints { get; set; }
                public List<JsonKey> keys { get; set; }
            }

            private class JsonEndpoint
            {
                public string location { get; set; }
                public string protocol { get; set; }
                public string usage { get; set; }
            }

            private class JsonKeyValue
            {
                public string type { get; set; }
                public string value { get; set; }
            }

            private class JsonKey
            {
                public string usage { get; set; }
                public JsonKeyValue keyValue { get; set; }
            }
        }

        public class TransientExceptionRetryStrategy
        {
            /// <summary>
            /// Gets or sets the retry count default is 3.
            /// </summary>
            /// <value>
            /// The retry count.
            /// </value>
            public int RetryCount { get; set; }

            /// <summary>
            /// Gets or sets the retry delay default is 1000ms.
            /// </summary>
            /// <value>
            /// The retry delay.
            /// </value>
            public int RetryDelayMilliseconds { get; set; }

            /// <summary>
            /// Gets or sets the exponential delay default is 0.
            /// </summary>
            /// <value>
            /// The exponential delay.
            /// </value>
            public int ExponentialDelayMilliseconds { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the retry operation should be done if a match is in a inner exception [include inner exceptions].
            /// </summary>
            /// <value>
            /// <c>true</c> if [include inner exceptions]; otherwise, <c>false</c>.
            /// </value>
            public bool IncludeInnerExceptions { get; set; }

            /// <summary>
            /// Gets or sets the exception types that should be treated as a transient exception.
            /// </summary>
            /// <value>
            /// The transient exceptions.
            /// </value>
            public List<Type> TransientExceptionTypes { get; set; }

            /// <summary>
            /// Gets or sets the transient exception HResult codes that should be treated as a transient exception.
            /// </summary>
            /// <value>
            /// The transient exception result codes.
            /// </value>
            public List<int> TransientExceptionResultCodes { get; set; }

            /// <summary>
            /// Gets or sets the transient exception matchers which can be used to create custom code to decide if the exception is transient.
            /// </summary>
            /// <value>
            /// The transient exception matchers.
            /// </value>
            public List<Func<Exception, bool>> TransientExceptionMatchers { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="TransientExceptionRetryStrategy"/> class.
            /// </summary>
            public TransientExceptionRetryStrategy()
            {
                RetryCount = 3;
                RetryDelayMilliseconds = 1000;
                ExponentialDelayMilliseconds = 0;
                TransientExceptionTypes = new List<Type>();
                TransientExceptionResultCodes = new List<int>();
            }

        }
        #endregion
    }
}
