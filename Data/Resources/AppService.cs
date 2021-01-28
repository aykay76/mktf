using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using blazorserver.Data;
using Microsoft.Azure.Management.ContainerInstance.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

// https://docs.microsoft.com/en-us/rest/api/appservice/webapps/get
// https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/app_service
namespace blazorserver.Data.Resources
{
    public class AppService : AzureResource
    {
        public static string AzureType = "Microsoft.Web/sites";
        public static string ApiVersion = "2019-08-01";
        public static string TerraformType = "azurerm_app_service";

        public string AppServicePlanId { get; set; }

        // TODO: do site config, app settings etc.

        public static new AzureResource FromJsonElement(JsonElement element)
        {
            AppService resource = new AppService();
            resource.Description = element;

            // basic information
            resource.ID = element.GetProperty("id").GetString();
            resource.Name = element.GetProperty("name").GetString();
            resource.Type = element.GetProperty("type").GetString();
            resource.Location = element.GetProperty("location").GetString();

            // extract app service plan ID similar to subnet for network profile
            // could be internal or external
            // string aspId = element.GetProperty("???").GetString();
            // resource.AppServicePlanId = resource.GetAppServicePlanReference(aspId);

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

            builder.AppendLine($"}}");
            builder.AppendLine();

            return builder.ToString();
        }
    }
}