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
        public string SslEnforcement { get; set; }
        public string MinimumTlsVersion { get; set; }
        public int StorageMb { get; set; }
        public int BackupRetentionDays { get; set; }
        public string GeoRedundantBackup { get; set; }
        public string StorageAutogrow { get; set; }

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

            JsonElement properties = element.GetProperty("properties");

            resource.Version = properties.GetProperty("version").GetString();
            resource.AdministratorLogin = properties.GetProperty("administratorLogin").GetString();
            resource.SslEnforcement = properties.GetProperty("sslEnforcement").GetString();
            resource.MinimumTlsVersion = properties.GetProperty("minimalTlsVersion").GetString();

            JsonElement storageProfile = properties.GetProperty("storageProfile");
            resource.StorageMb = storageProfile.GetProperty("storageMB").GetInt32();
            resource.BackupRetentionDays = storageProfile.GetProperty("backupRetentionDays").GetInt32();
            resource.GeoRedundantBackup = storageProfile.GetProperty("geoRedundantBackup").GetString();
            resource.StorageAutogrow = storageProfile.GetProperty("storageAutogrow").GetString();

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
            builder.AppendLine($"  version  = \"{Version}\"");
            builder.AppendLine();
            builder.AppendLine($"  administrator_login          = \"{AdministratorLogin}\"");
            builder.AppendLine($"  administrator_login_password = \"{AdministratorLoginPassword}\"");
            builder.AppendLine();
            builder.AppendLine($"  ssl_enforcement_enabled = \"{SslEnforcement}\"");
            builder.AppendLine($"  ssl_minimal_tls_version_enforced = \"{MinimumTlsVersion}\"");
            builder.AppendLine();
            builder.AppendLine($"  storage_mb                   = {StorageMb}");
            builder.AppendLine($"  backup_retention_days        = {BackupRetentionDays}");
            builder.AppendLine($"  geo_redundant_backup_enabled = {GeoRedundantBackup == "Enabled"}");
            builder.AppendLine($"  auto_grow_enabled            = {StorageAutogrow == "Enabled"}");

            builder.AppendLine($"}}");
            builder.AppendLine();

            return builder.ToString();
        }
    }
}