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
    public class IpConfiguration : AzureResource
    {
        public static string AzureType = "Microsoft.Network/networkInterfaces/ipConfigurations";
        public static string ApiVersion = "2020-07-01";
        public static string TerraformType = "";

        public string SubnetId { get; set; }
        public string PrivateIpAllocationMethod { get; set; }
        public string PrivateIpAddressVersion { get; set; }

        public static new AzureResource FromJsonElement(JsonElement element)
        {
            IpConfiguration resource = new IpConfiguration();
            resource.Description = element;

            // basic information
            resource.Name = element.GetProperty("name").GetString();
            resource.Type = element.GetProperty("type").GetString();

            JsonElement properties = element.GetProperty("properties");
            resource.PrivateIpAllocationMethod = properties.GetProperty("privateIPAllocationMethod").GetString();
            resource.PrivateIpAddressVersion = properties.GetProperty("privateIPAddressVersion").GetString();
            resource.SubnetId = resource.GetSubnetReference(properties.GetProperty("subnet").GetProperty("id").GetString());

            return resource;
        }

        public override string Emit()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"  ip_configuration {{");
            builder.AppendLine($"    name                          = \"{Name}\"");
            builder.AppendLine($"    subnet_id                     = {SubnetId}");
            builder.AppendLine($"    private_ip_address_allocation = \"{PrivateIpAllocationMethod}\"");
            builder.AppendLine($"    private_ip_address_version    = \"{PrivateIpAddressVersion}\"");
            builder.AppendLine($"}}");
            builder.AppendLine();

            return builder.ToString();
        }
    }
}