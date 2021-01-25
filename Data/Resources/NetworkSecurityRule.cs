using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using blazorserver.Data;
using Microsoft.Azure.Management.ContainerInstance.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using static System.Text.Json.JsonElement;

namespace blazorserver.Data.Resources
{
    public class NetworkSecurityRule : AzureResource
    {
        public static string AzureType = "Microsoft.Network/networkSecurityGroups/securityRules";
        public static string ApiVersion = "2020-07-01";
        public static string TerraformType = "azurerm_network_security_rule";

        public string Protocol { get; set; }
        public string SourcePortRange { get; set; }
        public string DestinationPortRange { get; set; }
        public string SourceAddressPrefix { get; set; }
        public string DestinationAddressPrefix { get; set; }
        public string Access { get; set; }
        public int Priority { get; set; }
        public string Direction { get; set; }
        public List<string> SourcePortRanges { get; set; }
        public List<string> DestinationPortRanges { get; set; }
        public List<string> SourceAddressPrefixes { get; set; }
        public List<string> DestinationAddressPrefixes { get; set; }

        public NetworkSecurityRule()
        {
            SourcePortRanges = new List<string>();
            DestinationPortRanges = new List<string>();
            SourceAddressPrefixes = new List<string>();
            DestinationAddressPrefixes = new List<string>();
        }

        public static new AzureResource FromJsonElement(JsonElement element)
        {
            NetworkSecurityRule resource = new NetworkSecurityRule();
            resource.Description = element;

            resource.Name = element.GetProperty("name").GetString();

            JsonElement properties = element.GetProperty("properties");
            resource.Protocol = properties.GetProperty("protocol").GetString();

            try
            {
                resource.SourcePortRange = properties.GetProperty("sourcePortRange").GetString();
            }
            catch
            {

            }

            try
            {
                resource.DestinationPortRange = properties.GetProperty("destinationPortRange").GetString();
            }
            catch
            {

            }

            try
            {
                resource.SourceAddressPrefix = properties.GetProperty("sourceAddressPrefix").GetString();
            }
            catch
            {

            }

            try
            {
                resource.DestinationAddressPrefix = properties.GetProperty("destinationAddressPrefix").GetString();
            }
            catch
            {

            }

            resource.Access = properties.GetProperty("access").GetString();
            resource.Priority = properties.GetProperty("priority").GetInt32();
            resource.Direction = properties.GetProperty("direction").GetString();

            var e = properties.GetProperty("sourcePortRanges").EnumerateArray();
            while (e.MoveNext())
            {
                resource.SourcePortRanges.Add(e.Current.GetString());
            }

            e = properties.GetProperty("destinationPortRanges").EnumerateArray();
            while (e.MoveNext())
            {
                resource.DestinationPortRanges.Add(e.Current.GetString());
            }

            e = properties.GetProperty("sourceAddressPrefixes").EnumerateArray();
            while (e.MoveNext())
            {
                resource.SourceAddressPrefixes.Add(e.Current.GetString());
            }

            e = properties.GetProperty("destinationAddressPrefixes").EnumerateArray();
            while (e.MoveNext())
            {
                resource.DestinationAddressPrefixes.Add(e.Current.GetString());
            }

            return resource;
        }

        public override string Emit()
        {
            StringBuilder builder = new StringBuilder();

            // emitting as an internal block because there seem to be no limits, this could easily change to be a resource in its own right
            // https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/network_security_rule

            builder.AppendLine($"  security_rule {{");
            builder.AppendLine($"    name = \"{Name}\"");
            builder.AppendLine($"    priority = {Priority}");
            builder.AppendLine($"    direction = \"{Direction}\"");
            builder.AppendLine($"    access = \"{Access}\"");
            builder.AppendLine($"    protocol = \"{Protocol}\"");
            if (SourcePortRanges.Count == 0)
            {
                builder.AppendLine($"    source_port_range = \"{SourcePortRange}\"");
            }
            else
            {
                builder.Append($"    source_port_ranges = [");
                bool first = true;
                foreach (string s in SourcePortRanges)
                {
                    if (first) first = false;
                    else builder.Append(",");
                    builder.Append($"\"{s}\"");
                }
                builder.AppendLine("]");
            }

            if (DestinationPortRanges.Count == 0)
            {
                builder.AppendLine($"    destination_port_range = \"{DestinationPortRange}\"");
            }
            else
            {
                builder.Append($"    destination_port_ranges = [");
                bool first = true;
                foreach (string s in DestinationPortRanges)
                {
                    if (first) first = false;
                    else builder.Append(",");
                    builder.Append($"\"{s}\"");
                }
                builder.AppendLine("]");
            }

            if (SourceAddressPrefixes.Count == 0)
            {
                builder.AppendLine($"    source_address_prefix = \"{SourceAddressPrefix}\"");
            }
            else
            {
                builder.Append($"    source_address_prefixes = [");
                bool first = true;
                foreach (string s in SourceAddressPrefixes)
                {
                    if (first) first = false;
                    else builder.Append(",");
                    builder.Append($"\"{s}\"");
                }
                builder.AppendLine("]");
            }

            if (DestinationAddressPrefixes.Count == 0)
            {
                builder.AppendLine($"    destination_address_prefix = \"{DestinationAddressPrefix}\"");
            }
            else
            {
                builder.Append($"    destination_address_prefixes = [");
                bool first = true;
                foreach (string s in DestinationAddressPrefixes)
                {
                    if (first) first = false;
                    else builder.Append(",");
                    builder.Append($"\"{s}\"");
                }
                builder.AppendLine("]");
            }

            builder.AppendLine($"  }}\r\n");

            return builder.ToString();
        }
    }
}