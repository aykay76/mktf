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
    public class NetworkSecurityGroup : AzureResource
    {
        public static string AzureType = "Microsoft.Network/networkSecurityGroups";
        public static string ApiVersion = "2020-07-01";
        public static string TerraformType = "azurerm_network_security_group";

        public List<string> Subnets { get; set; }
        public List<NetworkSecurityRule> Rules { get; set; }

        public NetworkSecurityGroup()
        {
            Subnets = new List<string>();
            Rules = new List<NetworkSecurityRule>();
        }

        public static new AzureResource FromJsonElement(JsonElement element)
        {
            NetworkSecurityGroup resource = new NetworkSecurityGroup();
            resource.Description = element;

            // basic information
            resource.ID = element.GetProperty("id").GetString();
            resource.Name = element.GetProperty("name").GetString();
            resource.Type = element.GetProperty("type").GetString();
            resource.Location = element.GetProperty("location").GetString();

            JsonElement properties = element.GetProperty("properties");

            ArrayEnumerator e = properties.GetProperty("securityRules").EnumerateArray();
            while (e.MoveNext())
            {
                NetworkSecurityRule rule = NetworkSecurityRule.FromJsonElement(e.Current) as NetworkSecurityRule;
                resource.Rules.Add(rule);
            }

            JsonElement subnets;
            if (properties.TryGetProperty("subnets", out subnets))
            {
                ArrayEnumerator subnetEnum = subnets.EnumerateArray();
                while (subnetEnum.MoveNext())
                {
                    string subnetId = subnetEnum.Current.GetProperty("id").GetString();
                    // I only want the name, strip the rest
                    string[] parts = subnetId.Split('/');
                    resource.Subnets.Add(parts[parts.Length - 1]);
                }
            }

            return resource;
        }

        public override string Emit()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"resource \"{TerraformType}\" \"{TerraformNameFromResourceName(Name)}\" {{");
            builder.AppendLine($"  resource_group_name  = \"{ResourceGroupName}\"");
            builder.AppendLine($"  location             = \"{Location}\"");
            builder.AppendLine();
            builder.AppendLine($"  name = \"{Name}\"");

            foreach (NetworkSecurityRule rule in Rules)
            {
                builder.Append(rule.Emit());
            }

            builder.AppendLine($"}}");

            return builder.ToString();
        }
    }
}
