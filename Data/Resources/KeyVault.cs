using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using blazorserver.Data;
using Microsoft.Azure.Management.ContainerInstance.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

// https://docs.microsoft.com/en-us/rest/api/keyvault/vaults/get
namespace blazorserver.Data.Resources
{
    public class KeyVault : AzureResource
    {
        public static string AzureType = "Microsoft.KeyVault/vaults";
        public static string ApiVersion = "2019-09-01";
        public static string TerraformType = "azurerm_key_vault";

        public string TenantID { get; set; }
        public string SKU { get; set; }
        public bool EnabledForDeployment { get; set; }
        public bool EnabledForDiskEncryption { get; set; }
        public bool EnabledForTemplateDeployment { get; set; }
        public bool EnableRBACAuthorisation { get; set; }

        public static new AzureResource FromJsonElement(JsonElement element)
        {
            KeyVault resource = new KeyVault();
            resource.Description = element;

            // basic information
            resource.ID = element.GetProperty("id").GetString();
            resource.Name = element.GetProperty("name").GetString();
            resource.Type = element.GetProperty("type").GetString();
            resource.Location = element.GetProperty("location").GetString();

            JsonElement properties = element.GetProperty("properties");
            resource.TenantID = properties.GetProperty("tenantId").GetString();
            resource.SKU = properties.GetProperty("sku").GetProperty("name").GetString();
            resource.EnabledForDeployment = properties.GetProperty("enabledForDeployment").GetBoolean();
            try
            {
                resource.EnabledForDiskEncryption = properties.GetProperty("enabledForDiskEncryption").GetBoolean();
            }
            catch
            {

            }

            try
            {
                resource.EnabledForTemplateDeployment = properties.GetProperty("enabledForTemplateDeployment").GetBoolean();
            }
            catch
            {

            }

            try
            {
                resource.EnableRBACAuthorisation = properties.GetProperty("enableRbacAuthorization").GetBoolean();
            }
            catch
            {

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
            builder.AppendLine($"  sku_name  = \"{SKU}\"");
            builder.AppendLine($"  tenant_id = \"{TenantID}\"");
            builder.AppendLine();
            builder.AppendLine($"  enabled_for_deployment = {EnabledForDeployment}");
            builder.AppendLine($"  enabled_for_disk_encryption = {EnabledForDiskEncryption}");
            builder.AppendLine($"  enabled_for_template_deployment = {EnabledForTemplateDeployment}");
            builder.AppendLine($"  enable_rbac_authorization = {EnableRBACAuthorisation}");
            builder.AppendLine($"}}");
            builder.AppendLine();

            return builder.ToString();
        }
    }
}