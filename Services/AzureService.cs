using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ContainerInstance.Fluent;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;

namespace blazorserver.Data
{
    public class AzureService
    {
        private string accessToken = string.Empty;
        private DateTime expiryTime = DateTime.Now;
        private string subscriptionId = string.Empty;

        public AzureService()
        {
        }

        public async Task Authenticate()
        {
            if (DateTime.Now > expiryTime || accessToken == string.Empty)
            {
                using (HttpClient client = new HttpClient())
                {
                    string clientId = System.Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
                    string clientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");
                    subscriptionId = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
                    string tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");

                    Dictionary<string, string> body = new Dictionary<string, string>() {
                        { "grant_type", "client_credentials" },
                        { "resource", "https://management.azure.com" },
                        { "client_id", clientId },
                        { "client_secret", clientSecret }
                    };
                    
                    var response = await client.PostAsync($"https://login.microsoftonline.com/{tenantId}/oauth2/token", new FormUrlEncodedContent(body));
                    string result = await response.Content.ReadAsStringAsync();
                    JsonDocument doc = JsonDocument.Parse(result);
                    accessToken = doc.RootElement.GetProperty("access_token").GetString();
                    int expiryEpoch = int.Parse(doc.RootElement.GetProperty("expires_on").GetString());
                    expiryTime = DateTimeOffset.FromUnixTimeSeconds(expiryEpoch).DateTime;
                }
            }
        }

        public async Task<JsonDocument> CallARM(string url)
        {
            await Authenticate();

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
                var resources = await client.GetAsync(url);
                var result = await resources.Content.ReadAsStringAsync();
                Console.WriteLine(url);
                Console.WriteLine(result);
                var doc = JsonDocument.Parse(result);                
                return doc;
            }
        }


        public async Task<List<AzureSubscription>> GetSubscriptions()
        {
            string deviceMessage = string.Empty;

            try
            {
                List<AzureSubscription> subscriptions = new List<AzureSubscription>();

                JsonDocument result = await CallARM("https://management.azure.com/subscriptions?api-version=2020-01-01");
                var e = result.RootElement.GetProperty("value").EnumerateArray();
                while (e.MoveNext())
                {
                    subscriptions.Add(AzureSubscription.FromJsonElement(e.Current));
                }

                return subscriptions;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public async Task<List<ResourceGroup>> GetResourceGroups(AzureSubscription subscription)
        {
            try
            {
                List<ResourceGroup> resourceGroups = new List<ResourceGroup>();

                JsonDocument result = await CallARM($"https://management.azure.com/subscriptions/{subscriptionId}/resourcegroups?api-version=2020-06-01");
                var e = result.RootElement.GetProperty("value").EnumerateArray();
                while (e.MoveNext())
                {
                    resourceGroups.Add(ResourceGroup.FromJsonElement(e.Current));
                }
                
                resourceGroups.Sort((i1, i2)=>{return i1.Name.CompareTo(i2.Name);});

                return resourceGroups;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public async Task<List<AzureResource>> GetResources(ResourceGroup resourceGroup)
        {
            try
            {
                List<AzureResource> resources = new List<AzureResource>();

                JsonDocument result = await CallARM($"https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup.Name}/resources?api-version=2020-06-01");
                var e = result.RootElement.GetProperty("value").EnumerateArray();
                while (e.MoveNext())
                {
                    resources.Add(AzureResource.FromJsonElement(e.Current));
                }

                return resources;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public async Task<AzureResource> GetResourceGroup(string resourceGroupName)
        {
            JsonDocument result = await CallARM($"https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}?api-version=2020-06-01");
            return null;
        }

        public async Task<AzureResource> LoadResource(AzureResource stub)
        {
            AzureResource resource = stub;

            try
            {
                Assembly a = Assembly.GetExecutingAssembly();
                List<Type> types = a.GetTypes().Where(t => t.IsClass && t.BaseType == typeof(AzureResource)).ToList();

                foreach (Type t in types)
                {
                    FieldInfo[] fields = t.GetFields();
                    FieldInfo f = t.GetField("AzureType");
                    if (f != null && f.GetValue(null).ToString() == stub.Type)
                    {
                        string apiVersion = t.GetField("ApiVersion").GetValue(null).ToString();
                        JsonDocument result = await CallARM($"https://management.azure.com{stub.ID}?api-version={apiVersion}");
                        resource = (AzureResource)t.GetMethod("FromJsonElement").Invoke(null, new object[] {result.RootElement});
                    }
                }

                 //t.GetProperty("Type").GetValue(null).ToString() == stub.Type
    //            AzureResource resource = (AzureResource)Activator.CreateInstance(t);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return resource;
        }

        // public async Task<IPublicIPAddress> GetPublicIPAddress(string subscriptionId, string resourceGroupName, string resourceName)
        // {
        //     return (await Authenticate()).WithSubscription(subscriptionId).PublicIPAddresses.GetByResourceGroup(resourceGroupName, resourceName);
        // }

        // public async Task<IApplicationGateway> GetApplicationGateway(string subscriptionId, string resourceGroupName, string resourceName)
        // {
        //     return (await Authenticate()).WithSubscription(subscriptionId).ApplicationGateways.GetByResourceGroup(resourceGroupName, resourceName);
        // }

        // public async Task<INetwork> GetNetwork(string subscriptionId, string resourceGroupName, string resourceName)
        // {
        //     return (await Authenticate()).WithSubscription(subscriptionId).Networks.GetByResourceGroup(resourceGroupName, resourceName);
        // }

        // public async Task<IContainerGroup> GetContainerGroup(string subscriptionId, string resourceGroupName, string resourceName)
        // {
        //     return (await Authenticate()).WithSubscription(subscriptionId).ContainerGroups.GetByResourceGroup(resourceGroupName, resourceName);
        // }
    }
}
