using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using blazorserver.Data;
using Microsoft.Azure.Management.ContainerInstance.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using static System.Text.Json.JsonElement;

// https://docs.microsoft.com/en-us/rest/api/aks/managedclusters/get
// https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/kubernetes_cluster
namespace blazorserver.Data.Resources
{
    public class KubernetesCluster : AzureResource
    {
        public static string AzureType = "Microsoft.ContainerService/managedClusters";
        public static string ApiVersion = "2020-11-01";
        public static string TerraformType = "azurerm_kubernetes_cluster";

        public string SkuTier { get; set; }
        public string KubernetesVersion { get; set; }
        public string DnsPrefix { get; set; }
        public string FQDN { get; set; }

        public class PoolProfile
        {
            public string Name { get; set; }
            public string VmSize { get; set; }
            public List<string> AvailabilityZones { get; set; }
            public int MaxPods { get; set; }
            public string Type { get; set; }
            public string SubnetId { get; set; }

            public PoolProfile()
            {
                AvailabilityZones = new List<string>();
            }

            public void FromJsonElement(JsonElement element)
            {
                Name = element.GetProperty("name").GetString();
                VmSize = element.GetProperty("vmSize").GetString();

                ArrayEnumerator e = element.GetProperty("availabilityZones").EnumerateArray();
                while (e.MoveNext())
                {
                    AvailabilityZones.Add(e.Current.GetString());
                }

                MaxPods = element.GetProperty("maxPods").GetInt32();
                Type = element.GetProperty("type").GetString();
            }

            public string Emit()
            {
                StringBuilder builder = new StringBuilder();

                builder.AppendLine($"  default_node_pool {{");
                builder.AppendLine($"    name    = \"{Name}\"");
                builder.AppendLine($"    vm_size = \"{VmSize}\"");
                builder.AppendLine($"    availability_zones = [");
                bool first = true;
                foreach (string s in AvailabilityZones)
                {
                    if (first) first = false;
                    else builder.Append(",");

                    builder.Append($"\"{s}\"");
                }
                builder.AppendLine("]");
                builder.AppendLine($"  max_pods = {MaxPods}");
                builder.AppendLine($"  type = \"{Type}\"");
                builder.AppendLine($"  }}");

                return builder.ToString();
            }
        }

        public PoolProfile DefaultNodeProfile { get; set; }

        public KubernetesCluster()
        {
        }

        public static new AzureResource FromJsonElement(JsonElement element)
        {
            KubernetesCluster resource = new KubernetesCluster();
            resource.Description = element;

            // basic information
            resource.ID = element.GetProperty("id").GetString();
            resource.Name = element.GetProperty("name").GetString();
            resource.Type = element.GetProperty("type").GetString();
            resource.Location = element.GetProperty("location").GetString();

            resource.SkuTier = element.GetProperty("sku").GetProperty("tier").GetString();

            JsonElement properties = element.GetProperty("properties");
            resource.KubernetesVersion = properties.GetProperty("kubernetesVersion").GetString();
            resource.DnsPrefix = properties.GetProperty("dnsPrefix").GetString();
            resource.FQDN = properties.GetProperty("fqdn").GetString();

            ArrayEnumerator e = properties.GetProperty("agentPoolProfiles").EnumerateArray();
            if (e.MoveNext())
            {
                resource.DefaultNodeProfile.FromJsonElement(e.Current);
            }

            return resource;
        }

        public override string Emit()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"resource \"{TerraformType}\" \"{TerraformNameFromResourceName(Name)}\" {{");
            builder.AppendLine($"  resource_group_name = \"{ResourceGroupName}\"");
            builder.AppendLine($"  location            = \"{Location}\"");
            builder.AppendLine();
            builder.AppendLine($"  name               = \"{Name}\"");
            builder.AppendLine($"  sku_tier           = \"{SkuTier}\"");
            builder.AppendLine($"  kubernetes_version = \"{KubernetesVersion}\"");
            builder.AppendLine($"  dns_prefix         = \"{DnsPrefix}\"");
            builder.AppendLine($"  fqdn               = \"{FQDN}\"");
            builder.AppendLine();
            builder.Append(DefaultNodeProfile.Emit());
            builder.AppendLine($"}}");
            builder.AppendLine();

            return builder.ToString();
        }
    }
}