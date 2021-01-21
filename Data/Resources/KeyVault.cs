using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using blazorserver.Data;
using Microsoft.Azure.Management.ContainerInstance.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

// https://docs.microsoft.com/en-us/rest/api/keyvault/vaults/get
public class KeyVault : AzureResource
{
    public static string AzureType = "Microsoft.KeyVault/vaults";
    public static string ApiVersion = "2019-09-01";
    public static string TerraformType = "azurerm_key_vault";

    public static new AzureResource FromJsonElement(JsonElement element)
    {
        KeyVault resource = new KeyVault();
        resource.Description = element;

        // basic information
        resource.ID = element.GetProperty("id").GetString();
        resource.Name = element.GetProperty("name").GetString();
        resource.Type = element.GetProperty("type").GetString();
        resource.Location = element.GetProperty("location").GetString();

        // TODO: resource specific information

        return resource;
    }

    public override List<string> GetReferences()
    {
        return null;
    }

    public override string Emit()
    {
        StringBuilder builder = new StringBuilder();

        builder.Append($"resource \"{TerraformType}\" \"{TerraformNameFromResourceName(Name)}\" {{\r\n");
        builder.Append($"  resource_group_name = {ResourceGroupName}\r\n");
        builder.Append($"  location            = {Location}\r\n");
        builder.Append($"}}\r\n");
        builder.Append("\r\n");

        return builder.ToString();
    }
}