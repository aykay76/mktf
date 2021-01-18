using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

public class ResourceGroup : AzureResource
{
    new public IResourceGroup Inner { get; set; }

    public override IResource Resource()
    {
        return Inner;
    }

    public override List<string> GetReferences()
    {
        return null;
    }

    public override string Emit()
    {
        StringBuilder builder = new StringBuilder();

        builder.Append($"resource \"azurerm_resource_group\" \"{Inner.Name.Replace('-', '_')}\" {{\r\n");
        builder.Append($"  resource_group_name = {Inner.Name}\r\n");
        builder.Append($"  location            = {Inner.RegionName}\r\n");
        builder.Append($"}}\r\n");

        return builder.ToString();
    }
}