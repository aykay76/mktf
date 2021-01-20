using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

public class ResourceGroup : AzureResource
{
    public static string AzureType = "resourcegroup";
    public static string ApiVersion = "2020-06-01";
    public static string TerraformType = "azurerm_resource_group";

    new public static AzureResource FromJsonElement(JsonElement element)
    {
        ResourceGroup result = new ResourceGroup();
        
        result.ID = element.GetProperty("id").GetString();
        result.Name = element.GetProperty("name").GetString();
        result.Type = element.GetProperty("type").GetString();
        result.Location = element.GetProperty("location").GetString();

        return result;
    }

    public override List<string> GetReferences()
    {
        return null;
    }

    public override string Emit()
    {
        StringBuilder builder = new StringBuilder();

        builder.Append($"resource \"azurerm_resource_group\" \"{Name.Replace('-', '_')}\" {{\r\n");
        builder.Append($"  resource_group_name = \"{Name}\"\r\n");
        builder.Append($"  location            = \"{Location}\"\r\n");
        builder.Append($"}}\r\n");
        builder.Append("\r\n");

        return builder.ToString();
    }
}