using System.Collections.Generic;
using System.Text;
using blazorserver.Data;
using Microsoft.Azure.Management.ContainerInstance.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

public class ContainerGroup : AzureResource
{
    public static string AzureType = "Microsoft.ContainerInstance/containerGroups";
    public static string ApiVersion = "2019-12-01";
    public static string TerraformType = "azurerm_container_group";

    public override List<string> GetReferences()
    {
        List<string> refs = new List<string>();

        // TODO: find any external references - maybe network profile or image server (ACR)

        return refs;
    }

    public override string Emit()
    {
        StringBuilder builder = new StringBuilder();

        // builder.Append($"resource \"azurerm_container_group\" \"{Inner.Name.Replace('-', '_')}\" {{\r\n");
        // builder.Append($"  resource_group_name = {Inner.ResourceGroupName}\r\n");
        // builder.Append($"  location            = {Inner.RegionName}\r\n");
        // builder.Append($"}}\r\n");

        return builder.ToString();
    }
}