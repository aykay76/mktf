using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using blazorserver.Data;
using Microsoft.Azure.Management.ContainerInstance.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using static System.Text.Json.JsonElement;

// https://docs.microsoft.com/en-us/rest/api/keyvault/vaults/get
namespace blazorserver.Data.Resources
{ 
    public class Route : AzureResource
    {
        public static string AzureType = "Microsoft.Network/routes";
        public static string ApiVersion = "2020-07-01";
        public static string TerraformType = "azurerm_route";

        public string AddressPrefix { get; set; }
        public string NextHopType { get; set; }
        public string NextHopIpAddress { get; set; }

        public static new AzureResource FromJsonElement(JsonElement element)
        {
            Route resource = new Route();
            resource.Description = element;

            // basic information
            resource.ID = element.GetProperty("id").GetString();
            resource.Name = element.GetProperty("name").GetString();

            JsonElement properties = element.GetProperty("properties");

            resource.AddressPrefix = properties.GetProperty("addressPrefix").GetString();
            resource.NextHopType = properties.GetProperty("nextHopType").GetString();
            try
            {
                resource.NextHopIpAddress = properties.GetProperty("nextHopIpAddress").GetString();
            }
            catch
            {

            }

            return resource;
        }

        public override string Emit()
        {
            StringBuilder builder = new StringBuilder();

            // TODO: could refactor as separate resource
            builder.AppendLine($"  route {{");
            builder.AppendLine($"    address_prefix = \"{AddressPrefix}\"");
            builder.AppendLine($"    next_hop_type = \"{NextHopType}\"");
            builder.AppendLine($"    next_hop_in_ip_address = \"{NextHopIpAddress}\"");
            builder.AppendLine($"  }}");
            builder.AppendLine();

            return builder.ToString();
        }
    }
}