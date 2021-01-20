using System.Collections.Generic;
using System.Text;
using blazorserver.Data;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

public class ApplicationGateway : AzureResource
{
    public static string AzureType = "Microsoft.Network/applicationGateways";
    public static string ApiVersion = "2020-07-01";
    public static string TerraformType = "azurerm_application_gateway";

    public override List<string> GetReferences()
    {
        List<string> refs = new List<string>();

        // foreach (var fe in Frontends.Values)
        // {
        //     if (fe.IsPublic)
        //     {
        //         refs.Add(fe.PublicIPAddressId);
        //     }
        // }

        return refs;
    }

    public override string Emit()
    {
        StringBuilder builder = new StringBuilder();

        // builder.Append($"resource \"azurerm_application_gateway\" \"{Name.Replace('-', '_')}\" {{\r\n");
        // builder.Append($"  resource_group_name = {ResourceGroupName}\r\n");
        // builder.Append($"  location            = {Inner.RegionName}\r\n");
        // builder.Append($"}}\r\n");

        return builder.ToString();
    }
}