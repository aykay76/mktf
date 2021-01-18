using System;
using System.Collections.Generic;
using System.Linq;
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
        public AzureService()
        {
        }

        public Azure.IAuthenticated Authenticate()
        {
            string homePath = (Environment.OSVersion.Platform == PlatformID.Unix ||
                Environment.OSVersion.Platform == PlatformID.MacOSX)
                    ? Environment.GetEnvironmentVariable("HOME")
                    : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");

            AzureCredentialsFactory factory = new AzureCredentialsFactory();
            AzureCredentials creds = factory.FromFile($"{homePath}/.azure/authfile");

            return Azure.Authenticate(creds);
        }

        public async Task<AzureSubscription[]> GetSubscriptions()
        {
            string deviceMessage = string.Empty;

            try
            {
                var auth = Authenticate();

                List<AzureSubscription> subscriptions = new List<AzureSubscription>();

                var subs = await auth.Subscriptions.ListAsync();
                if (subs != null)
                {
                    foreach (var sub in auth.Subscriptions.List().ToArray())
                    {
                        subscriptions.Add(new AzureSubscription { ID = Guid.Parse(sub.SubscriptionId), DisplayName = sub.DisplayName });
                    }
                }

                return subscriptions.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return new AzureSubscription[] { new AzureSubscription { ID = Guid.Empty, DisplayName = ex.ToString() } };
            }
        }

        public async Task<AzureResourceGroup[]> GetResourceGroups(AzureSubscription subscription)
        {
            try
            {
                var auth = Authenticate();

                List<AzureResourceGroup> resourceGroups = new List<AzureResourceGroup>();
                var rgs = await auth.WithSubscription(subscription.ID.ToString()).ResourceGroups.ListAsync(true);
                foreach (IResourceGroup rg in rgs)
                {
                    resourceGroups.Add(new AzureResourceGroup { ID = rg.Id, DisplayName = rg.Name, SubscriptionID = subscription.ID });
                }

                resourceGroups.Sort((i1, i2)=>{return i1.DisplayName.CompareTo(i2.DisplayName);});

                return resourceGroups.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return new AzureResourceGroup[] { new AzureResourceGroup { ID = string.Empty, DisplayName = ex.ToString() } };
            }
        }

        public async Task<IGenericResource[]> GetResources(AzureResourceGroup group)
        {
            try
            {
                var auth = Authenticate();

                List<AzureResourceGroup> resourceGroups = new List<AzureResourceGroup>();
                IAzure azure = auth.WithSubscription(group.SubscriptionID.ToString());
                var resources = await azure.GenericResources.ListByResourceGroupAsync(group.DisplayName);

                return resources.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return new IGenericResource[] { };
            }
        }

        public IResourceGroup GetResourceGroup(string subscriptionId, string resourceGroupName)
        {
            return Authenticate().WithSubscription(subscriptionId).ResourceGroups.GetByName(resourceGroupName);
        }

        public IPublicIPAddress GetPublicIPAddress(string subscriptionId, string resourceGroupName, string resourceName)
        {
            return Authenticate().WithSubscription(subscriptionId).PublicIPAddresses.GetByResourceGroup(resourceGroupName, resourceName);
        }

        public IApplicationGateway GetApplicationGateway(string subscriptionId, string resourceGroupName, string resourceName)
        {
            return Authenticate().WithSubscription(subscriptionId).ApplicationGateways.GetByResourceGroup(resourceGroupName, resourceName);
        }

        public IContainerGroup GetContainerGroup(string subscriptionId, string resourceGroupName, string resourceName)
        {
            return Authenticate().WithSubscription(subscriptionId).ContainerGroups.GetByResourceGroup(resourceGroupName, resourceName);
        }
    }
}
