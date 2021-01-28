using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using blazorserver.Data;
using Microsoft.Azure.Management.ContainerInstance.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

// https://docs.microsoft.com/en-us/rest/api/storagerp/storageaccounts/getproperties
// https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/storage_account
namespace blazorserver.Data.Resources
{
    public class StorageAccount : AzureResource
    {
        public static string AzureType = "Microsoft.Storage/storageAccounts";
        public static string ApiVersion = "2019-06-01";
        public static string TerraformType = "azurerm_storage_account";

        public string AccountKind { get; set; }
        public string AccountTier { get; set; }
        public string AccountReplicationType { get; set; }
        public bool HttpsOnly { get; set; }
        public bool IsHnsEnabled { get; set; }

        // TODO: add custom domains and network rules.

        public static new AzureResource FromJsonElement(JsonElement element)
        {
            StorageAccount resource = new StorageAccount();
            resource.Description = element;

            // basic information
            resource.ID = element.GetProperty("id").GetString();
            resource.Name = element.GetProperty("name").GetString();
            resource.Type = element.GetProperty("type").GetString();
            resource.Location = element.GetProperty("location").GetString();

            resource.AccountKind = element.GetProperty("kind").GetString();
            resource.AccountTier = element.GetProperty("sku").GetProperty("tier").GetString();
            resource.AccountReplicationType = element.GetProperty("sku").GetProperty("name").GetString();
            resource.AccountReplicationType = resource.AccountReplicationType.Replace(resource.AccountTier, "").Replace("_", "");
            resource.HttpsOnly = element.GetProperty("properties").GetProperty("supportsHttpsTrafficOnly").GetBoolean();
            resource.IsHnsEnabled = element.GetProperty("properties").GetProperty("isHnsEnabled").GetBoolean();

            return resource;
        }

        public override string Emit()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"resource \"{TerraformType}\" \"{TerraformNameFromResourceName(Name)}\" {{");
            builder.AppendLine($"  resource_group_name = \"{ResourceGroupName}\"");
            builder.AppendLine($"  location            = \"{Location}\"");
            builder.AppendLine();
            builder.AppendLine($"  name                      = \"{Name}\"");
            builder.AppendLine($"  account_tier              = \"{AccountTier}\"");
            builder.AppendLine($"  account_replication_type  = \"{AccountReplicationType}\"");
            builder.AppendLine($"  account_kind              = \"{AccountKind}\"");
            builder.AppendLine($"  enable_https_traffic_only = {HttpsOnly}");
            builder.AppendLine($"  is_hns_enabled            = {IsHnsEnabled}");
            builder.AppendLine($"}}");
            builder.AppendLine();

            return builder.ToString();
        }
    }
}