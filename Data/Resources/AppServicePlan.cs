using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using blazorserver.Data;
using Microsoft.Azure.Management.ContainerInstance.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

// https://docs.microsoft.com/en-us/rest/api/appservice/appserviceplans/get
// https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/app_service_plan
namespace blazorserver.Data.Resources
{
    public class AppServicePlan : AzureResource
    {
        public static string AzureType = "Microsoft.Web/serverFarms";
        public static string ApiVersion = "2019-08-01";
        public static string TerraformType = "azurerm_app_service_plan";

        public string Kind { get; set; }
        public string SkuTier { get; set; }
        public string SkuSize { get; set; }
        public int MaximumNumberOfWorkers { get; set; }

        public static new AzureResource FromJsonElement(JsonElement element)
        {
            AppServicePlan resource = new AppServicePlan();
            resource.Description = element;

            // basic information
            resource.ID = element.GetProperty("id").GetString();
            resource.Name = element.GetProperty("name").GetString();
            resource.Type = element.GetProperty("type").GetString();
            resource.Location = element.GetProperty("location").GetString();

            resource.Kind = element.GetProperty("kind").GetString();
            resource.SkuTier = element.GetProperty("sku").GetProperty("tier").GetString();
            resource.SkuSize = element.GetProperty("sku").GetProperty("size").GetString();
            resource.MaximumNumberOfWorkers = element.GetProperty("properties").GetProperty("maximumNumberOfWorkers").GetInt32();

            return resource;
        }

        public override string Emit()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"resource \"{TerraformType}\" \"{TerraformNameFromResourceName(Name)}\" {{");
            builder.AppendLine($"  resource_group_name = \"{ResourceGroupName}\"");
            builder.AppendLine($"  location            = \"{Location}\"");
            builder.AppendLine();
            builder.AppendLine($"  name = \"{Name}\"");
            builder.AppendLine($"  kind = \"{Kind}\"");
            builder.AppendLine();
            builder.AppendLine($"  sku {{");
            builder.AppendLine($"    tier = \"{SkuTier}\"");
            builder.AppendLine($"    size = \"{SkuSize}\"");
            builder.AppendLine($"  }}");
            builder.AppendLine();
            builder.AppendLine($"  maximum_elastic_worker_count = {MaximumNumberOfWorkers}");
            builder.AppendLine($"}}");
            builder.AppendLine();

            return builder.ToString();
        }
    }
}