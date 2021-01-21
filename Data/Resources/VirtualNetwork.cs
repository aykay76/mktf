using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using blazorserver.Data;
using Microsoft.Azure.Management.ContainerInstance.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using static System.Text.Json.JsonElement;

public class VirtualNetwork : AzureResource
{
    public static string AzureType = "Microsoft.Network/virtualNetworks";
    public static string ApiVersion = "2020-07-01";
    public static string TerraformType = "azurerm_virtual_network";

    public List<string> AddressPrefixes { get; set; }

    public VirtualNetwork()
    {
        AddressPrefixes = new List<string>();
    }

    public static new AzureResource FromJsonElement(JsonElement element)
    {
        VirtualNetwork resource = new VirtualNetwork();
        resource.Description = element;

        // basic information
        resource.ID = element.GetProperty("id").GetString();
        resource.Name = element.GetProperty("name").GetString();
        resource.Type = element.GetProperty("type").GetString();
        resource.Location = element.GetProperty("location").GetString();

        // collect resource specific information
        JsonElement properties = element.GetProperty("properties");
        ArrayEnumerator addressPrefixes = properties.GetProperty("addressSpace").GetProperty("addressPrefixes").EnumerateArray();
        while (addressPrefixes.MoveNext())
        {
            resource.AddressPrefixes.Add(addressPrefixes.Current.GetString());
        }

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
        builder.Append($"  resource_group_name = \"{ResourceGroupName}\"\r\n");
        builder.Append($"  location            = \"{Location}\"\r\n");

        builder.Append($"  address_space = [");
        bool first = true;
        foreach (string addressPrefix in AddressPrefixes)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                builder.Append(",");
            }
            builder.Append($"\"{addressPrefix}\"");
        }
        builder.Append("]\r\n");

        builder.Append($"}}\r\n");
        builder.Append("\r\n");

        return builder.ToString();
    }
}