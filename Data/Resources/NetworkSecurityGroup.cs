using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using blazorserver.Data;
using Microsoft.Azure.Management.ContainerInstance.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using static System.Text.Json.JsonElement;

public class NetworkSecurityGroup : AzureResource
{
    public static string AzureType = "Microsoft.Network/networkSecurityGroups";
    public static string ApiVersion = "2020-07-01";
    public static string TerraformType = "azurerm_network_security_group";

    public List<string> Subnets { get; set; }

    public NetworkSecurityGroup()
    {
        Subnets = new List<string>();
    }

    public static new AzureResource FromJsonElement(JsonElement element)
    {
        NetworkSecurityGroup resource = new NetworkSecurityGroup();
        resource.Description = element;

        // basic information
        resource.ID = element.GetProperty("id").GetString();
        resource.Name = element.GetProperty("name").GetString();
        resource.Type = element.GetProperty("type").GetString();
        resource.Location = element.GetProperty("location").GetString();

        // TODO: process the securityRules property

        JsonElement properties = element.GetProperty("properties");
        ArrayEnumerator subnetEnum = properties.GetProperty("subnets").EnumerateArray();
        while (subnetEnum.MoveNext())
        {
            string subnetId = subnetEnum.Current.GetProperty("id").GetString();
            // I only want the name, strip the rest
            string[] parts = subnetId.Split('/');
            resource.Subnets.Add(parts[parts.Length - 1]);
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
        builder.Append($"  resource_group_name  = \"{ResourceGroupName}\"\r\n");
        builder.Append($"  location             = \"{Location}\"\r\n");
        builder.Append("\r\n");
        builder.Append($"  name = \"{Name}\"\r\n");
        builder.Append($"}}\r\n");

        return builder.ToString();
    }
}