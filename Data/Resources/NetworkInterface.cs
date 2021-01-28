using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using blazorserver.Data;
using Microsoft.Azure.Management.ContainerInstance.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using static System.Text.Json.JsonElement;

// https://docs.microsoft.com/en-us/rest/api/virtualnetwork/networkinterfaces/get
// https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/network_interface
namespace blazorserver.Data.Resources
{
    public class NetworkInterface : AzureResource
    {
        public static string AzureType = "Microsoft.Network/networkInterfaces";
        public static string ApiVersion = "2020-07-01";
        public static string TerraformType = "azurerm_network_interface";

        public List<IpConfiguration> IpConfigs { get; set; }

        public NetworkInterface()
        {
            IpConfigs = new List<IpConfiguration>();
        }

        public static new AzureResource FromJsonElement(JsonElement element)
        {
            NetworkInterface resource = new NetworkInterface();
            resource.Description = element;

            // basic information
            resource.ID = element.GetProperty("id").GetString();
            resource.Name = element.GetProperty("name").GetString();
            resource.Type = element.GetProperty("type").GetString();
            resource.Location = element.GetProperty("location").GetString();

            ArrayEnumerator e = element.GetProperty("properties").GetProperty("ipConfigurations").EnumerateArray();
            while (e.MoveNext())
            {
                IpConfiguration config = IpConfiguration.FromJsonElement(e.Current) as IpConfiguration;
                resource.IpConfigs.Add(config);
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
            builder.AppendLine($"  name     = \"{Name}\"");
            foreach (IpConfiguration config in IpConfigs)
            {
                builder.AppendLine(config.Emit());
            }
            builder.AppendLine($"}}");
            builder.AppendLine();

            return builder.ToString();
        }
    }
}