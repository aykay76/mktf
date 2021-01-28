using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using blazorserver.Data;
using Microsoft.Azure.Management.ContainerInstance.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using static System.Text.Json.JsonElement;

// https://docs.microsoft.com/en-us/rest/api/virtualnetwork/networkprofiles/get
// https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/network_profile
namespace blazorserver.Data.Resources
{
    public class NetworkProfile : AzureResource
    {
        public static string AzureType = "Microsoft.Network/networkProfiles";
        public static string ApiVersion = "2020-07-01";
        public static string TerraformType = "azurerm_network_profile";

        public string ContainerNetworkInterfaceName { get; set; }
        public Dictionary<string, string> IpConfigurations { get; set; }

        public NetworkProfile()
        {
            IpConfigurations = new Dictionary<string, string>();
        }

        public static new AzureResource FromJsonElement(JsonElement element)
        {
            NetworkProfile resource = new NetworkProfile();
            resource.Description = element;

            // basic information
            resource.ID = element.GetProperty("id").GetString();
            resource.Name = element.GetProperty("name").GetString();
            resource.Type = element.GetProperty("type").GetString();
            resource.Location = element.GetProperty("location").GetString();

            JsonElement properties = element.GetProperty("properties");
            resource.ContainerNetworkInterfaceName = properties.GetProperty("containerNetworkInterfaceConfigurations").GetProperty("name").GetString();

            properties = properties.GetProperty("containerNetworkInterfaceConfigurations").GetProperty("properties");
            ArrayEnumerator e = properties.GetProperty("ipConfigurations").EnumerateArray();
            while (e.MoveNext())
            {
                string name = e.Current.GetProperty("name").GetString();
                string subnetId = e.Current.GetProperty("properties").GetProperty("subnet").GetProperty("id").GetString();
                string subnetRef = resource.GetSubnetReference(subnetId);

                resource.IpConfigurations.Add(name, subnetRef);
            }

            return resource;
        }

        public override string Emit()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"resource \"{TerraformType}\" \"{TerraformNameFromResourceName(Name)}\" {{");
            builder.AppendLine($"  resource_group_name = \"{ResourceGroupName}\"");
            builder.AppendLine($"  location            = \"{Location}\"");
            builder.AppendLine($"  name                = \"{Name}\"");
            builder.AppendLine();
            builder.AppendLine($"  container_network_interface {{");
            builder.AppendLine($"    name = \"{ContainerNetworkInterfaceName}\"");
            foreach (KeyValuePair<string, string> ipconfig in IpConfigurations)
            {
                builder.AppendLine("    ip_configuration {");
                builder.AppendLine($"      name      = \"{ipconfig.Key}\"");
                builder.AppendLine($"      subnet_id = \"{ipconfig.Value}\"");
                builder.AppendLine("    }");
            }
            builder.AppendLine($"  }}");
            builder.AppendLine($"}}");
            builder.AppendLine();

            return builder.ToString();
        }
    }
}