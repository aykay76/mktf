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
            AzureResource resource = null;

            try
            {
                Assembly a = Assembly.GetExecutingAssembly();
                List<Type> types = a.GetTypes().Where(t => t.IsClass && t.BaseType == typeof(AzureResource)).ToList();

                foreach (Type t in types)
                {
                    FieldInfo fi = t.GetField("AzureType");
                    if (fi != null && fi.GetValue(null).ToString() == stub.Type)
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
                        if (stub.Type == VirtualNetwork.AzureType)
                        {
                            ArrayEnumerator subnetEnum = result.RootElement.GetProperty("properties").GetProperty("subnets").EnumerateArray();
                            while (subnetEnum.MoveNext())
                            {
                                Subnet subnet = Subnet.FromJsonElement(subnetEnum.Current) as Subnet;
                                subnet.VirtualNetworkName = stub.Name;
                                resources.Add(subnet);
                            }
                        }
                        if (stub.Type == NetworkSecurityGroup.AzureType)
                        {
                            // if subnet is defined we need to add an association because inline subnet blocks don't support delegation or service endpoints
                            NetworkSecurityGroup nsg = resource as NetworkSecurityGroup;
                            foreach (string subnet in nsg.Subnets)
                            {
                                SubnetNetworkSecurityGroupAssociation assoc = new SubnetNetworkSecurityGroupAssociation();
                                assoc.SubnetName = subnet;
                                assoc.NetworkSecurityGroupName = nsg.Name;
                                resources.Add(assoc);
                            }                        
                        }
                        if (stub.Type == DnsZone.AzureType)
                        {
                            // DNS zones don't contain records so we need to add each one as a separate resource
                            JsonDocument records = await CallARM($"https://management.azure.com{stub.ID}/all?api-version={apiVersion}");

                            ArrayEnumerator recordEnum = records.RootElement.GetProperty("value").EnumerateArray();
                            while (recordEnum.MoveNext())
                            {
                                // this could be a recursive call, is it better to load the object twice or to duplicate this code? :S
                                string type = recordEnum.Current.GetProperty("type").GetString();

                                if (type == "Microsoft.Network/dnszones/CNAME")
                                {
                                    var record = DnsZoneCNAME.FromJsonElement(recordEnum.Current) as DnsZoneCNAME;
                                    record.Location = stub.Location;
                                    record.ZoneName = stub.Name;
                                    resources.Add(record);
                                }
                                if (type == "Microsoft.Network/dnszones/TXT")
                                {
                                    var record = DnsZoneTXT.FromJsonElement(recordEnum.Current) as DnsZoneTXT;
                                    record.Location = stub.Location;
                                    record.ZoneName = stub.Name;
                                    resources.Add(record);
                                }
                                if (type == "Microsoft.Network/dnszones/A")
                                {
                                    var record = DnsZoneA.FromJsonElement(recordEnum.Current) as DnsZoneA;
                                    record.Location = stub.Location;
                                    record.ZoneName = stub.Name;
                                    resources.Add(record);
                                }
                            }
                        }
                        if (stub.Type == KeyVault.AzureType)
                        {
                            // enumerate the access policies and create separately - inline blocks only support 16 access policies
                            ArrayEnumerator policyEnum = result.RootElement.GetProperty("properties").GetProperty("accessPolicies").EnumerateArray();
                            while (policyEnum.MoveNext())
                            {
                                var policy = KeyVaultAccessPolicy.FromJsonElement(policyEnum.Current) as KeyVaultAccessPolicy;
                                policy.KeyVaultName = stub.Name;
                                resources.Add(policy);
                            }
                        }

                        string filename = $"../{stub.Type.Replace('/', '_')}.json";
                        if (File.Exists(filename) == false)
                        {
                            string s = result.RootElement.ToString();
                            File.WriteAllText(filename, s);
                        }
                    }
                }

                if (resource == null)
                {
                    resources.Add(stub);
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
