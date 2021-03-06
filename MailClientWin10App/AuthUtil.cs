using System;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web.Core;
using Windows.Security.Credentials;

namespace MailClientWin10App
{
    class AuthUtil
    {
        private static async Task<string> GetAccessToken()
        {
            string token = null;

            //first try to get the token silently
            WebAccountProvider aadAccountProvider = await WebAuthenticationCoreManager.FindAccountProviderAsync("https://login.windows.net");
            WebTokenRequest webTokenRequest = new WebTokenRequest(aadAccountProvider, String.Empty, App.Current.Resources["ida:ClientID"].ToString(), WebTokenRequestPromptType.Default);
            webTokenRequest.Properties.Add("authority", "https://login.windows.net");
            webTokenRequest.Properties.Add("resource", "https://outlook.office.com/");
            WebTokenRequestResult webTokenRequestResult = await WebAuthenticationCoreManager.GetTokenSilentlyAsync(webTokenRequest);
            if (webTokenRequestResult.ResponseStatus == WebTokenRequestStatus.Success)
            {
                WebTokenResponse webTokenResponse = webTokenRequestResult.ResponseData[0];
                token = webTokenResponse.Token;
            }
            else if (webTokenRequestResult.ResponseStatus == WebTokenRequestStatus.UserInteractionRequired)
            {
                //get token through prompt
                webTokenRequest = new WebTokenRequest(aadAccountProvider, String.Empty, App.Current.Resources["ida:ClientID"].ToString(), WebTokenRequestPromptType.ForceAuthentication);
                webTokenRequest.Properties.Add("authority", "https://login.windows.net");
                webTokenRequest.Properties.Add("resource", "https://outlook.office.com/");
                webTokenRequestResult = await WebAuthenticationCoreManager.RequestTokenAsync(webTokenRequest);
                if (webTokenRequestResult.ResponseStatus == WebTokenRequestStatus.Success)
                {
                    WebTokenResponse webTokenResponse = webTokenRequestResult.ResponseData[0];
                    token = webTokenResponse.Token;
                }
            }

            return token;
        }

        public static async Task<OutlookServicesClient> EnsureClient()
        {
            return new OutlookServicesClient(new Uri("https://outlook.office.com/api/v1.0"), async () => {
                return await GetAccessToken();
            });
        }
    }
}