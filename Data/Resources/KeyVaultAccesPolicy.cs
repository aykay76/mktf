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
    public class KeyVaultAccessPolicy : AzureResource
    {
        public static string AzureType = "Microsoft.KeyVault/vaults/accessPolicies";
        public static string ApiVersion = "2019-09-01";
        public static string TerraformType = "azurerm_key_vault_access_policy";

        public string KeyVaultName { get; set; }
        public string TenantID { get; set; }
        public string ObjectID { get; set; }
        public List<string> KeyPermissions { get; set; }
        public List<string> CertificatePermissions { get; set; }
        public List<string> SecretPermissions { get; set; }
        public List<string> StoragePermissions { get; set; }

        public KeyVaultAccessPolicy()
        {
            KeyPermissions = new List<string>();
            CertificatePermissions = new List<string>();
            SecretPermissions = new List<string>();
            StoragePermissions = new List<string>();
        }

        public static new AzureResource FromJsonElement(JsonElement element)
        {
            KeyVaultAccessPolicy resource = new KeyVaultAccessPolicy();
            resource.Description = element;

            // basic information
            // resource.ID = element.GetProperty("id").GetString();
            // resource.Name = element.GetProperty("name").GetString();
            // resource.Type = element.GetProperty("type").GetString();
            // resource.Location = element.GetProperty("location").GetString();

            resource.TenantID = element.GetProperty("tenantId").GetString();
            resource.ObjectID = element.GetProperty("objectId").GetString();

            JsonElement permissions = element.GetProperty("permissions");
            ArrayEnumerator e = permissions.GetProperty("keys").EnumerateArray();
            while (e.MoveNext())
            {
                resource.KeyPermissions.Add(e.Current.GetString());
            }

            e = permissions.GetProperty("secrets").EnumerateArray();
            while (e.MoveNext())
            {
                resource.SecretPermissions.Add(e.Current.GetString());
            }

            e = permissions.GetProperty("certificates").EnumerateArray();
            while (e.MoveNext())
            {
                resource.CertificatePermissions.Add(e.Current.GetString());
            }

            return resource;
        }

        public override string Emit()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"resource \"{TerraformType}\" \"{TerraformNameFromResourceName(ObjectID)}\" {{");
            builder.AppendLine($"  key_vault_id = {KeyVault.TerraformType}.{KeyVaultName}.id");
            builder.AppendLine($"  tenant_id = \"{TenantID}\"");
            builder.AppendLine($"  object_id = \"{ObjectID}\"");
            builder.AppendLine($"  key_permissions = [");
            foreach (string p in KeyPermissions)
            {
                builder.AppendLine($"    \"{p}\"");
            }
            builder.AppendLine($"  ]");

            builder.AppendLine($"  secret_permissions = [");
            foreach (string p in SecretPermissions)
            {
                builder.AppendLine($"    \"{p}\"");
            }
            builder.AppendLine($"  ]");

            builder.AppendLine($"  certificate_permissions = [");
            foreach (string p in CertificatePermissions)
            {
                builder.AppendLine($"    \"{p}\"");
            }
            builder.AppendLine($"  ]");

            builder.AppendLine($"}}");
            builder.AppendLine();

            return builder.ToString();
        }
    }
}
