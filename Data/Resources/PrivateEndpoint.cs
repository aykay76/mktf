using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using blazorserver.Data;
using Microsoft.Azure.Management.ContainerInstance.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using static System.Text.Json.JsonElement;

// https://docs.microsoft.com/en-us/rest/api/virtualnetwork/privateendpoints/get
namespace blazorserver.Data.Resources
{
    public class PrivateEndpoint : AzureResource
    {
        public static string AzureType = "Microsoft.Network/privateEndpoints";
        public static string ApiVersion = "2020-07-01";
        public static string TerraformType = "azurerm_private_endpoint";

        public string SubnetId { get; set; }

        public string ServiceConnectionName { get; set; }
        public string PrivateLinkServiceId { get; set; }
        public List<string> GroupIds { get; set; }

        public PrivateEndpoint()
        {
            GroupIds = new List<string>();
        }

        public static new AzureResource FromJsonElement(JsonElement element)
        {
            PrivateEndpoint resource = new PrivateEndpoint();
            resource.Description = element;

            // basic information
            resource.ID = element.GetProperty("id").GetString();
            resource.Name = element.GetProperty("name").GetString();
            resource.Type = element.GetProperty("type").GetString();
            resource.Location = element.GetProperty("location").GetString();

            string subnetId = element.GetProperty("properties").GetProperty("subnet").GetProperty("id").GetString();

            resource.SubnetId = resource.GetSubnetReference(subnetId);

            ArrayEnumerator e = element.GetProperty("properties").GetProperty("privateLinkServiceConnections").EnumerateArray();
            while (e.MoveNext())
            {
                resource.ServiceConnectionName = e.Current.GetProperty("name").GetString();
                resource.PrivateLinkServiceId = e.Current.GetProperty("properties").GetProperty("privateLinkServiceId").GetString();
                ArrayEnumerator e2 = e.Current.GetProperty("properties").GetProperty("groupIds").EnumerateArray();
                while (e2.MoveNext())
                {
                    resource.GroupIds.Add(e2.Current.GetString());
                }
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
            builder.AppendLine($"  name      = \"{Name}\"");
            builder.AppendLine($"  subnet_id = {SubnetId}");
            builder.AppendLine();
            builder.AppendLine($"  private_service_connection {{");
            builder.AppendLine($"    name = \"{ServiceConnectionName}\"");
            // TODO: this needs to become a link to a resource or data object like subnet TTDL
            builder.AppendLine($"    private_connection_resource_id = {PrivateLinkServiceId}");
            builder.Append($"    subresource_name = [");
            bool first = true;
            foreach (string group in GroupIds)
            {
                if (first) first = false;
                else
                {
                    builder.Append(",");
                }
                builder.Append($"\"{group}\"");
            }
            builder.AppendLine($"]");
            builder.AppendLine($"  }}");
            builder.AppendLine($"}}");
            builder.AppendLine();

            return builder.ToString();
        }
    }
}