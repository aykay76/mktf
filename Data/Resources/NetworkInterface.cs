using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using blazorserver.Data;
using Microsoft.Azure.Management.ContainerInstance.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

// https://docs.microsoft.com/en-us/rest/api/virtualnetwork/networkinterfaces/get
namespace blazorserver.Data.Resources
{
    public class NetworkInterface : AzureResource
    {
        public static string AzureType = "Microsoft.Network/networkInterfaces";
        public static string ApiVersion = "2020-07-01";
        public static string TerraformType = "azurerm_network_interface";


        public static new AzureResource FromJsonElement(JsonElement element)
        {
            NetworkInterface resource = new NetworkInterface();
            resource.Description = element;

            // basic information
            resource.ID = element.GetProperty("id").GetString();
            resource.Name = element.GetProperty("name").GetString();
            resource.Type = element.GetProperty("type").GetString();
            resource.Location = element.GetProperty("location").GetString();



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