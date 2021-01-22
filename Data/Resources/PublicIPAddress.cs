using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

public class PublicIPAddress : AzureResource
{
    public static string AzureType = "Microsoft.Network/publicIPAddresses";
    public static string ApiVersion = "2020-07-01";
    public static string TerraformType = "azurerm_public_ip";

    // additional fields for output to TF
    public string SKU { get; set; }
    public string AllocationMethod { get; set; }

    public static new AzureResource FromJsonElement(JsonElement element)
    {
        PublicIPAddress resource = new PublicIPAddress();
        resource.Description = element;

        // basic information
        resource.ID = element.GetProperty("id").GetString();
        resource.Name = element.GetProperty("name").GetString();
        resource.Type = element.GetProperty("type").GetString();
        resource.Location = element.GetProperty("location").GetString();

        // resource specific information
        resource.AllocationMethod = element.GetProperty("properties").GetProperty("publicIPAllocationMethod").GetString();
        resource.SKU = element.GetProperty("sku").GetProperty("name").GetString();

        return resource;
    }

    public override string Emit()
    {
        StringBuilder builder = new StringBuilder();

        builder.AppendLine($"resource \"{TerraformType}\" \"{TerraformNameFromResourceName(Name)}\" {{");
        builder.AppendLine($"  name                = \"{Name}\"");
        builder.AppendLine($"  resource_group_name = \"{ResourceGroupName}\"");
        builder.AppendLine($"  location            = \"{Location}\"");
        builder.AppendLine();
        builder.AppendLine($"  sku               = \"{SKU}\"");
        builder.AppendLine($"  allocation_method = \"{AllocationMethod}\"");
        builder.AppendLine($"}}");
        builder.AppendLine();

        return builder.ToString();
    }
}