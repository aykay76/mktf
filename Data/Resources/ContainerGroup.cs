using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using blazorserver.Data;
using Microsoft.Azure.Management.ContainerInstance.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace blazorserver.Data.Resources
{
    public class ContainerGroup : AzureResource
    {
        public static string AzureType = "Microsoft.ContainerInstance/containerGroups";
        public static string ApiVersion = "2019-12-01";
        public static string TerraformType = "azurerm_container_group";

        public static new AzureResource FromJsonElement(JsonElement element)
        {
            ContainerGroup resource = new ContainerGroup();
            resource.Description = element;

            // basic information
            resource.ID = element.GetProperty("id").GetString();
            resource.Name = element.GetProperty("name").GetString();
            resource.Type = element.GetProperty("type").GetString();
            resource.Location = element.GetProperty("location").GetString();

            // TODO: resource specific information

            return resource;
        }

        public override string Emit()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"resource \"{TerraformType}\" \"{TerraformNameFromResourceName(Name)}\" {{");
            builder.AppendLine($"  resource_group_name = \"{ResourceGroupName}\"");
            builder.AppendLine($"  location            = \"{Location}\"");
            builder.AppendLine($"}}");
            builder.AppendLine("");

            return builder.ToString();
        }
    }
}