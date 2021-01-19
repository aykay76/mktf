using System.Collections.Generic;
using System.Text;
using blazorserver.Data;
using Microsoft.Azure.Management.ContainerInstance.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

public class VirtualNetwork : AzureResource
{
    public override List<string> GetReferences()
    {
        List<string> refs = new List<string>();

        // TODO: find any references, just resource group?

        return refs;
    }

    public override string Emit()
    {
        StringBuilder builder = new StringBuilder();

        // builder.Append($"resource \"azurerm_virtual_network\" \"{Inner.Name.Replace('-', '_')}\" {{\r\n");
        // builder.Append($"  resource_group_name = {Inner.ResourceGroupName}\r\n");
        // builder.Append($"  location            = {Inner.RegionName}\r\n");
        // builder.Append($"}}\r\n");

        return builder.ToString();
    }
}