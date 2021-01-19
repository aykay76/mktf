using System.Collections.Generic;
using System.Text;
using blazorserver.Data;
using Microsoft.Azure.Management.ContainerInstance.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

public class Subnet : AzureResource
{
    public override List<string> GetReferences()
    {
        List<string> refs = new List<string>();

        // TODO: add reference to vnet

        return refs;
    }

    public override string Emit()
    {
        StringBuilder builder = new StringBuilder();

        // builder.Append($"resource \"azurerm_subnet\" \"{Inner.Name.Replace('-', '_')}\" {{\r\n");
        // builder.Append($"  resource_group_name  = \"{Inner.Parent.ResourceGroupName}\"\r\n");
        // builder.Append($"  virtual_network_name = \"{Inner.Parent.Name}\"\r\n");
        // builder.Append($"  name = \"${Inner.Name}\"\r\n");
        // builder.Append($"  address_prefixes = [\"{Inner.AddressPrefix}\"]\r\n");
        // builder.Append($"}}\r\n");

        return builder.ToString();
    }
}