using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using APIMUserNormalization.Models;
using System.Net;
using System.Collections.Specialized;
using System.IO;
using Newtonsoft.Json;
using System.Reflection;
using Microsoft.Graph;

namespace APIMUserNormalization.Services
{
    public class APIMService
    {

        // Read application settings from appsettings.json (tenant ID, app ID, client secret, etc.)
        AppSettings config = AppSettingsFile.ReadFromJsonFile();
       
        private string bearerToken = string.Empty;


        public APIMService(string pAPIMServiceName, string pAPIMResourceGroup, string pAPIMTenantId, string pAPIMSubscriptionId)
        {
            APIMServiceName = pAPIMServiceName;
            APIMResourceGroup = pAPIMResourceGroup;
            APIMTenantId = pAPIMTenantId;
            APIMSubscriptionId = pAPIMSubscriptionId;
        }


        public string APIMServiceName { get; set; }
        public string APIMResourceGroup { get; set; }

        public string APIMTenantId { get; set; }

        public string APIMSubscriptionId { get; set; }

        public async Task<string> GetTokenAsync()
        {
            string token;
            string postURL = config.APIMPostURL;
            string clientID = config.APIMClientID;
            string secret = config.APIMClientSecret;
            string resource = config.APIMResource;
            //string apimTenantId = config.APIMTenantId;

            using (var webClient = new WebClient())
            {
                var requestParameters = new NameValueCollection();
                requestParameters.Add("grant_type", "client_credentials");
                requestParameters.Add("client_id", clientID);
                requestParameters.Add("client_secret", secret);
                requestParameters.Add("resource", resource);

                var url = "https://login.microsoftonline.com/" + APIMTenantId + "/oauth2/token";
                webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                var responseBytes = await webClient.UploadValuesTaskAsync(url, "POST", requestParameters);
                var responseBody = Encoding.UTF8.GetString(responseBytes);

                var jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(responseBody);
                token = jsonObject.Value<string>("access_token");
            }

            return token;

        }

        public async Task<UserContract> GetUserFromAPIM(string email)
        {
            UserCollection users = await GetApiManagementUsersAsync(APIMSubscriptionId, APIMResourceGroup, APIMServiceName);
            foreach (UserContract user in users.value)
            {
                if (user.Properties.Email.Equals(email))
                {
                    return user;
                }
            }
            return null;

        }

        public async Task<UserCollection> GetUsersFromAPIM()
        {
            UserCollection users = await GetApiManagementUsersAsync(APIMSubscriptionId, APIMResourceGroup, APIMServiceName);
            return users;

        }

        /**
         * 
         * string URL = "https://management.azure.com/subscriptions/";
         * string subscriptionId = "439d90f7-9dc7-43f0-b2b5-d4e02f040e22";
         * string resourceGroup = "/resourceGroups/cemex_sandbox";
         * string apiManagementName = "/providers/Microsoft.ApiManagement/service/SANDBOXAPIS";
         * string userId = "/users/5de6e7bec31b6312d454359c/identities";
         * string urlParameters = "?api-version=2019-01-01";
         * 
        */
        private async Task<string> ExecuteGetRequest(string url, string subscriptionId, string resourceGroup, string apiManagementName, string request, string urlParameters)
        {

            if (bearerToken == null || bearerToken.Equals(string.Empty))
            {
                bearerToken = await GetTokenAsync();
            }

            resourceGroup = "/resourceGroups/" + resourceGroup;
            apiManagementName = "/providers/Microsoft.ApiManagement/service/" + apiManagementName;

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url + subscriptionId + resourceGroup + apiManagementName + request + urlParameters);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            System.Net.Http.Headers.AuthenticationHeaderValue bearerHeader = new AuthenticationHeaderValue("Bearer", bearerToken);
            client.DefaultRequestHeaders.Authorization = bearerHeader;
            // List data response.
            HttpResponseMessage response = client.GetAsync(urlParameters).Result;

            var responseValue = string.Empty;

            if (response != null && response.IsSuccessStatusCode)
            {
                Task task = response.Content.ReadAsStreamAsync().ContinueWith(t =>
                {
                    var stream = t.Result;
                    using (var reader = new StreamReader(stream))
                    {
                        responseValue = reader.ReadToEnd();
                    }
                });

                task.Wait();
            }
            else
            {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }
            return responseValue;
        }


        /**
         * 
         * string URL = "https://management.azure.com/subscriptions/";
         * string subscriptionId = "439d90f7-9dc7-43f0-b2b5-d4e02f040e22";
         * string resourceGroup = "/resourceGroups/cemex_sandbox";
         * string apiManagementName = "/providers/Microsoft.ApiManagement/service/SANDBOXAPIS";
         * string userId = "/users/5de6e7bec31b6312d454359c/identities";
         * string urlParameters = "?api-version=2019-01-01";
         * 
        */
        private async Task<string> ExecutePatchRequest(string ifMatch, string url, string subscriptionId, string resourceGroup, string apiManagementName, string request, string urlParameters, StringContent bodyContent)
        {

            if (bearerToken == null)
            {
                bearerToken = await GetTokenAsync();
            }

            resourceGroup = "/resourceGroups/" + resourceGroup;
            apiManagementName = "/providers/Microsoft.ApiManagement/service/" + apiManagementName;
            string apiUrl = url + subscriptionId + resourceGroup + apiManagementName + request + urlParameters;

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (ifMatch.Equals("*"))
            {
                client.DefaultRequestHeaders.Add("If-Match", ifMatch);
            }
            System.Net.Http.Headers.AuthenticationHeaderValue bearerHeader = new AuthenticationHeaderValue("Bearer", bearerToken);
            client.DefaultRequestHeaders.Authorization = bearerHeader;

            /////////////////////////////////////////////////////////////////////////////////
            var responseBody = String.Empty;

            HttpResponseMessage response;
            if (ifMatch.Equals("*"))
            {
                response = await client.PatchAsync(apiUrl, bodyContent);
            }
            else
            {
                response = await client.PutAsync(apiUrl, bodyContent);
            }



            var responseValue = string.Empty;

            if (response != null && response.IsSuccessStatusCode)
            {
                Task task = response.Content.ReadAsStreamAsync().ContinueWith(t =>
                {
                    var stream = t.Result;
                    using (var reader = new StreamReader(stream))
                    {
                        responseValue = reader.ReadToEnd();
                    }
                });

                task.Wait();
            }
            else
            {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }

            return responseValue;



            /////////////////////////////////////////////////////////////////////////////////

        }


        private async Task<UserCollection> GetApiManagementUsersAsync(string subscriptionId, string resourceGroup, string apiManagementName)
        {

            var responseValue = await ExecuteGetRequest("https://management.azure.com/subscriptions/", subscriptionId, resourceGroup, apiManagementName, "/users", "?api-version=2019-01-01");
            if (responseValue != null && !responseValue.Equals(string.Empty))
            {
                UserCollection users = System.Text.Json.JsonSerializer.Deserialize<UserCollection>(responseValue);
                foreach (var user in users.value)
                {
                    GroupContractCollection groups = await GerUserGroups(subscriptionId, resourceGroup, apiManagementName, user.Id);
                    user.Properties.Groups = groups;
                }
                return users;
            }
            else
            {
                return null;
            }
        }

        private async Task<GroupContractCollection> GerUserGroups(string subscriptionId, string resourceGroup, string apiManagementName, string id)
        {
            GroupContractCollection groups = null;

            string[] userIdArray = id.Split('/');
            id = userIdArray[userIdArray.Length - 1];

            var responseValue = await ExecuteGetRequest("https://management.azure.com/subscriptions/", subscriptionId, resourceGroup, apiManagementName, "/users/" + id + "/groups", "?api-version=2019-01-01");
            if (responseValue != null && !responseValue.Equals(string.Empty))
            {
                groups = System.Text.Json.JsonSerializer.Deserialize<GroupContractCollection>(responseValue);
            }
            return groups;
        }

        private static UserIdentityCollection GetUserIdentityCollectionAsync(string responseValue)
        {
            if (responseValue != null && responseValue.Equals(string.Empty))
            {
                return System.Text.Json.JsonSerializer.Deserialize<UserIdentityCollection>(responseValue);
            }
            else
            {
                return null;
            }
        }

        public async Task<string> UpdateUserObjectIdAsync(UserContract missingB2CUser, string id)
        {
            string objectId = id;
            string userId = missingB2CUser.Id;
            string[] userIdArray = userId.Split('/');
            userId = userIdArray[userIdArray.Length - 1];
            UserContractForUpdate userProperties = new UserContractForUpdate();
            PropertiesForUpdate propertiesForUpdate = new PropertiesForUpdate();
            userProperties.Properties = propertiesForUpdate;

            userProperties.Properties.Identities = missingB2CUser.Properties.Identities;
            bool adB2CIdentityFound = false;
            if (userProperties.Properties.Identities != null)
            {
                int i = 0;
                foreach (var identity in userProperties.Properties.Identities)
                {
                    if (identity.Provider.Equals("AadB2C"))
                    {
                        adB2CIdentityFound = true;
                        userProperties.Properties.Identities[i].Id = id;
                    }
                    i++;
                }
                if (!adB2CIdentityFound)
                {
                    userProperties.Properties.AddIdentity("AadB2C", id);
                }
            }
            else
            {
                userProperties.Properties.AddIdentity("AadB2C", id);
            }

            var json = JsonConvert.SerializeObject(userProperties);
            json = json.Replace("Properties", "properties");
            json = json.Replace("Identities", "identities");
            json = json.Replace("Id", "id");
            json = json.Replace("Provider", "provider");

            var data = new StringContent(json, Encoding.UTF8, "application/json");

            string response = await ExecutePatchRequest("*", "https://management.azure.com/subscriptions/", APIMSubscriptionId, APIMResourceGroup, APIMServiceName, "/users/" + userId, "?api-version=2019-01-01", data);

            return response;
        }

        internal async Task<string> CreateUserInApim(UserContract userContract)
        {
            string userId = Guid.NewGuid().ToString("N");

            UserContractForCreate userProperties = new UserContractForCreate();
            PropertiesForCreate propertiesForCreate = new PropertiesForCreate();
            propertiesForCreate.Email = userContract.Properties.Email;
            propertiesForCreate.FirstName = userContract.Properties.FirstName;
            propertiesForCreate.LastName = userContract.Properties.LastName;
            userProperties.Properties = propertiesForCreate;



            var json = JsonConvert.SerializeObject(userProperties);
            json = json.Replace("Properties", "properties");
            json = json.Replace("Identities", "identities");
            json = json.Replace("Id", "id");
            json = json.Replace("Provider", "provider");

            var data = new StringContent(json, Encoding.UTF8, "application/json");

            string response = await ExecutePatchRequest("", "https://management.azure.com/subscriptions/", APIMSubscriptionId, APIMResourceGroup, APIMServiceName, "/users/" + userId, "?api-version=2019-12-01", data);

            /*if(userContract.Properties != null && userContract.Properties.Groups != null && userContract.Properties.Groups.value != null)
            {
                foreach (GroupContract group in userContract.Properties.Groups.value)
                {
                    string groupResponse = await CreateUserGroup(group.Id, userContract.Id);
                }
            }*/
            return response;


        }

        private async Task<string> CreateUserGroup(string groupId, string userId)
        {
            string[] groupIdArray = groupId.Split('/');
            groupId = groupIdArray[groupIdArray.Length - 1];

            string[] userIdArray = userId.Split('/');
            userId = userIdArray[userIdArray.Length - 1];

            var data = new StringContent("", Encoding.UTF8, "application/json");
            string groupUserResponse = await ExecutePatchRequest("*", "https://management.azure.com/subscriptions/", APIMSubscriptionId, APIMResourceGroup, APIMServiceName, "/groups/" + groupId + "/users/" + userId, "?api-version=2019-01-01", data);

            return groupUserResponse;

        }
    }

}
