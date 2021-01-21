using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using blazorserver.Data;
using Microsoft.Azure.Management.ContainerInstance.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

public class Subnet : AzureResource
{
    public static string AzureType = "Microsoft.Network/virtualNetworks/subnets";
    public static string ApiVersion = "2020-07-01";
    public static string TerraformType = "azurerm_subnet";

    public string VirtualNetworkName { get; set; }
    public string AddressPrefix { get; set; }

    public static new AzureResource FromJsonElement(JsonElement element)
    {
        Subnet resource = new Subnet();
        resource.Description = element;

        // basic information
        resource.ID = element.GetProperty("id").GetString();
        resource.Name = element.GetProperty("name").GetString();
        resource.Type = element.GetProperty("type").GetString();

        // TODO: resource specific information
        // VirtualNetworkName is not known here, will be set by caller on return
        resource.AddressPrefix = element.GetProperty("properties").GetProperty("addressPrefix").GetString();

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
        builder.Append($"  resource_group_name  = \"{ResourceGroupName}\"\r\n");
        builder.Append($"  virtual_network_name = azurerm_virtual_network.{TerraformNameFromResourceName(VirtualNetworkName)}.name\r\n");
        builder.Append("\r\n");
        builder.Append($"  name             = \"{Name}\"\r\n");
        builder.Append($"  address_prefixes = [\"{AddressPrefix}\"]\r\n");
        builder.Append($"}}\r\n");

        return builder.ToString();
    }
}