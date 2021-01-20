using System;
using System.Collections.Generic;
using System.IO;
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
using static System.Text.Json.JsonElement;

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

        public async Task<List<AzureResource>> GetResourceGroups(AzureSubscription subscription)
        {
            try
            {
                List<AzureResource> resourceGroups = new List<AzureResource>();

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

        public async Task<List<AzureResource>> LoadResource(AzureResource stub)
        {
            // using a list because in special cases we might load subresources too
            List<AzureResource> resources = new List<AzureResource>();
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
                        // generic method of loading a single resource
                        string apiVersion = t.GetField("ApiVersion").GetValue(null).ToString();
                        JsonDocument result = await CallARM($"https://management.azure.com{stub.ID}?api-version={apiVersion}");
                        MethodInfo method = t.GetMethod("FromJsonElement");
                        if (method != null)
                        {
                            resource = (AzureResource)method.Invoke(null, new object[] {result.RootElement});
                            resources.Add(resource);
                        }

                        // in special cases we want to load subresources, such as subnets in a virtual network that we want to create as separate objects
                        if (stub.Type == "Microsoft.Network/virtualNetworks")
                        {
                            ArrayEnumerator subnetEnum = result.RootElement.GetProperty("properties").GetProperty("subnets").EnumerateArray();
                            while (subnetEnum.MoveNext())
                            {
                                Subnet subnet = Subnet.FromJsonElement(subnetEnum.Current) as Subnet;
                                subnet.VirtualNetworkName = stub.Name;
                                resources.Add(subnet);
                            }
                        }

                        // TODO: need to handle subnet-NSG associations as a separate object because we're handling the subnets separately
                        // inline blocks for subnets don't support delegations and service endpoints.

                        string filename = $"../{stub.Type.Replace('/', '_')}.json";
                        if (File.Exists(filename) == false)
                        {
                            string s = result.RootElement.ToString();
                            File.WriteAllText(filename, s);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return resources;
        }
    }
}
