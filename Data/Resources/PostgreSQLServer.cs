using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using blazorserver.Data;
using Microsoft.Azure.Management.ContainerInstance.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

// https://docs.microsoft.com/en-us/rest/api/postgresql/servers/get
namespace blazorserver.Data.Resources
{
    public class PostgreSQLServer : AzureResource
    {
        public static string AzureType = "Microsoft.DBforPostgreSQL/servers";
        public static string ApiVersion = "2017-12-01";
        public static string TerraformType = "azurerm_postgresql_server";

        public string SkuName { get; set; }
        public string Version { get; set; }
        public string AdministratorLogin { get; set; }
        public string AdministratorLoginPassword { get; set; }

        public static new AzureResource FromJsonElement(JsonElement element)
        {
            PostgreSQLServer resource = new PostgreSQLServer();
            resource.Description = element;

            // basic information
            resource.ID = element.GetProperty("id").GetString();
            resource.Name = element.GetProperty("name").GetString();
            resource.Type = element.GetProperty("type").GetString();
            resource.Location = element.GetProperty("location").GetString();

            resource.SkuName = element.GetProperty("sku").GetProperty("name").GetString();

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
            builder.AppendLine($"  sku_name = \"{SkuName}\"");
            builder.AppendLine();
            builder.AppendLine($"}}");
            builder.AppendLine();

            return builder.ToString();
        }
    }
}