using System.Collections.Generic;
using System.Text;
using blazorserver.Data;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

public class ApplicationGateway : AzureResource
{
    new public IApplicationGateway Inner { get; set; }

    public override IResource Resource()
    {
        return Inner;
    }

    public override List<string> GetReferences()
    {
        List<string> refs = new List<string>();

        foreach (var fe in Inner.Frontends.Values)
        {
            if (fe.IsPublic)
            {
                refs.Add(fe.PublicIPAddressId);
            }
        }

        return refs;
    }

    public override string Emit()
    {
        StringBuilder builder = new StringBuilder();

        builder.Append($"resource \"azurerm_application_gateway\" \"{Inner.Name.Replace('-', '_')}\" {{\r\n");
        builder.Append($"  resource_group_name = {Inner.ResourceGroupName}\r\n");
        builder.Append($"}}\r\n");

        return builder.ToString();
    }
}