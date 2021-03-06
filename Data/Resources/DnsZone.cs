using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using blazorserver.Data;
using Microsoft.Azure.Management.ContainerInstance.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace blazorserver.Data.Resources
{
    public class DnsZone : AzureResource
    {
        public static string AzureType = "Microsoft.Network/dnszones";
        public static string ApiVersion = "2018-05-01";
        public static string TerraformType = "azurerm_dns_zone";

        public static new AzureResource FromJsonElement(JsonElement element)
        {
            DnsZone resource = new DnsZone();
            resource.Description = element;

            // basic information
            resource.ID = element.GetProperty("id").GetString();
            resource.Name = element.GetProperty("name").GetString();
            resource.Type = element.GetProperty("type").GetString();

            return resource;
        }

        public override string Emit()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append($"resource \"{TerraformType}\" \"{TerraformNameFromResourceName(Name)}\" {{\r\n");
            builder.Append($"  resource_group_name = {ResourceGroupName}\r\n");
            builder.Append($"  name                = {Name}\r\n");
            builder.Append($"}}\r\n");
            builder.Append("\r\n");

            return builder.ToString();
        }
    }
}