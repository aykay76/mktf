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

            return resource;
        }

        public override string Emit()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"resource \"{TerraformType}\" \"{TerraformNameFromResourceName(Name)}\" {{");
            builder.AppendLine($"  resource_group_name = \"{ResourceGroupName}\"");
            builder.AppendLine($"  location            = \"{Location}\"");
            builder.AppendLine();
            builder.AppendLine($"  name     = \"{Name}\"");
            builder.AppendLine($"  sku_tier = \"{SkuTier}\"");
            builder.AppendLine($"}}");
            builder.AppendLine();

            return builder.ToString();
        }
    }
}