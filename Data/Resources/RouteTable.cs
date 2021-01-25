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
    public class RouteTable : AzureResource
    {
        public static string AzureType = "Microsoft.Network/routeTables";
        public static string ApiVersion = "2020-07-01";
        public static string TerraformType = "azurerm_route_table";

        public bool DisableBGPRoutePropagation { get; set; }
        public List<Route> Routes { get; set; }

        public RouteTable()
        {
            Routes = new List<Route>();
        }

        public static new AzureResource FromJsonElement(JsonElement element)
        {
            RouteTable resource = new RouteTable();
            resource.Description = element;

            // basic information
            resource.ID = element.GetProperty("id").GetString();
            resource.Name = element.GetProperty("name").GetString();
            resource.Type = element.GetProperty("type").GetString();
            resource.Location = element.GetProperty("location").GetString();

            JsonElement properties = element.GetProperty("properties");

            resource.DisableBGPRoutePropagation = properties.GetProperty("disableBgpRoutePropagation").GetBoolean();

            ArrayEnumerator e = properties.GetProperty("routes").EnumerateArray();
            while (e.MoveNext())
            {
                Route route = Route.FromJsonElement(e.Current) as Route;
                resource.Routes.Add(route);
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
            builder.AppendLine($"  disable_bgp_route_propagation = {DisableBGPRoutePropagation}");
            builder.AppendLine();
            foreach (Route route in Routes)
            {
                builder.AppendLine(route.Emit());
            }
            builder.AppendLine($"}}");
            builder.AppendLine();

            return builder.ToString();
        }
    }
}