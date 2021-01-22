using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using blazorserver.Data;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

public class ApplicationGateway : AzureResource
{
    public static string AzureType = "Microsoft.Network/applicationGateways";
    public static string ApiVersion = "2020-07-01";
    public static string TerraformType = "azurerm_application_gateway";

    public string SkuName { get; set; }
    public string SkuTier { get; set; }
    public int Capacity { get; set; }

    public Dictionary<string, string> GatewayIPConfigurations { get; set; }
    public Dictionary<string, int> FrontendPorts { get; set; }

    public ApplicationGateway()
    {
        GatewayIPConfigurations = new Dictionary<string, string>();
        FrontendPorts = new Dictionary<string, int>();
    }

    public static new AzureResource FromJsonElement(JsonElement element)
    {
        ApplicationGateway resource = new ApplicationGateway();
        resource.Description = element;

        // basic information
        resource.ID = element.GetProperty("id").GetString();
        resource.Name = element.GetProperty("name").GetString();
        resource.Type = element.GetProperty("type").GetString();
        resource.Location = element.GetProperty("location").GetString();

        JsonElement properties = element.GetProperty("properties");

        JsonElement sku = properties.GetProperty("sku");
        resource.SkuName = sku.GetProperty("name").GetString();
        resource.SkuTier = sku.GetProperty("tier").GetString();
        resource.Capacity = sku.GetProperty("capacity").GetInt32();

        // gateway IP configurations
        var e = properties.GetProperty("gatewayIPConfigurations").EnumerateArray();
        while (e.MoveNext())
        {
            string name = e.Current.GetProperty("name").GetString();
            string subnetId = e.Current.GetProperty("properties").GetProperty("subnet").GetProperty("id").GetString();
            string subnetName = ExtractSubnetName(subnetId);

            if (ResourceInResourceGroup(subnetId, resource.ResourceGroupName))
            {
                // in same resource group so will get picked up as resource, add reference type
                resource.GatewayIPConfigurations[name] = $"{Subnet.TerraformType}.{TerraformNameFromResourceName(subnetName)}.id";
            }
            else
            {
                // in a different resource group so I assume a data object will be needed
                // somehow i need to get a data object into the graph
                // data "resource_type" "name"{ attr=value...}
                // my reference to it will then be "data.resource_type.name.field"
                resource.GatewayIPConfigurations[name] = $"data.{Subnet.TerraformType}.{TerraformNameFromResourceName(subnetName)}.id";
            }
        }

        // TODO: sslCertificates

        // TODO: frontend IP configurations

        // frontend ports
        e = properties.GetProperty("frontendPorts").EnumerateArray();
        while (e.MoveNext())
        {
            string name = e.Current.GetProperty("name").GetString();
            int port = e.Current.GetProperty("properties").GetProperty("port").GetInt32();

            resource.FrontendPorts.Add(name, port);
        }

        // TODO: backend pools

        // TODO: load distribution policies

        // TODO: backendHttpSettingsCollection

        // TODO: httpListeners

        // TODO: urlPathMaps

        // TODO: requestRoutingRules

        // TODO: probes

        // TODO: rewriteRuleSets

        // TODO: redirectConfigurations

        // TODO: privateLinkConfigurations

        // TODO: privateEndpointConnections

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
        builder.AppendLine($"  sku {{");
        builder.AppendLine($"    name = \"{SkuName}\"");
        builder.AppendLine($"    tier = \"{SkuTier}\"");
        builder.AppendLine($"    capacity = {Capacity}");
        builder.AppendLine($"  }}");
        builder.AppendLine();

        foreach (KeyValuePair<string, string> gatewayIPConfiguration in GatewayIPConfigurations)
        {
            builder.AppendLine($"  gateway_ip_configuration {{");
            builder.AppendLine($"    name      = \"{gatewayIPConfiguration.Key}\"");
            builder.AppendLine($"    subnet_id = {gatewayIPConfiguration.Value}");
            builder.AppendLine($"  }}");
            builder.AppendLine();
        }

        foreach (string frontendPortName in FrontendPorts.Keys)
        {
            builder.AppendLine($"  frontend_port {{");
            builder.AppendLine($"    name = \"{frontendPortName}\"");
            builder.AppendLine($"    port = {FrontendPorts[frontendPortName]}");
            builder.AppendLine($"  }}");
            builder.AppendLine();
        }
        builder.AppendLine($"}}");
        builder.AppendLine();

        return builder.ToString();
    }
}