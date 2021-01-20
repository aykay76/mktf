using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

public class PublicIPAddress : AzureResource
{
    public static string AzureType = "Microsoft.Network/publicIPAddresses";
    public static string ApiVersion = "2020-07-01";
    public static string TerraformType = "azurerm_public_ip";

    public override List<string> GetReferences()
    {
        List<string> refs = new List<string>();

        // if (Inner.HasAssignedLoadBalancer)
        // {
        //     // oddity - public IP attached to application gateway shows it has a load balancer, but it's null
        //     try
        //     {
        //         ILoadBalancerPublicFrontend fe = Inner.GetAssignedLoadBalancerFrontend();
        //         ILoadBalancer lb = fe.Parent;

        //         // got the load balancer name (lb.Name) - check the other resources and make a link

        //         // return reference to load balancer from graph
        //         // TODO: add method FindByResourceType and return the node that matches
        //         // graph.Nodes.FindByValue();
        //         Console.WriteLine(lb.Name);
        //     }
        //     catch
        //     {
                
        //     }
        // }
        // if (Inner.HasAssignedNetworkInterface)
        // {
        //     INicIPConfiguration ipconfig = Inner.GetAssignedNetworkInterfaceIPConfiguration();
        //     Console.WriteLine(ipconfig.Name);
        // }

        return refs;
    }

    public override string Emit()
    {
        StringBuilder builder = new StringBuilder();

        // builder.Append($"resource \"azurerm_public_ip\" \"{Inner.Name.Replace('-', '_')}\" {{\r\n");
        // builder.Append($"  resource_group_name = {Inner.ResourceGroupName}\r\n");
        // builder.Append($"  location            = {Inner.RegionName}\r\n");
        // builder.Append($"}}\r\n");

        return builder.ToString();
    }
}