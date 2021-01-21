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

    public static new AzureResource FromJsonElement(JsonElement element)
    {
        ApplicationGateway resource = new ApplicationGateway();
        resource.Description = element;

        // basic information
        resource.ID = element.GetProperty("id").GetString();
        resource.Name = element.GetProperty("name").GetString();
        resource.Type = element.GetProperty("type").GetString();
        resource.Location = element.GetProperty("location").GetString();

        // TODO: resource specific information

        return resource;
    }

    // return a list of resource IDs for other resources that this has some form of relation with
    public override List<string> GetReferences()
    {
        return null;
    }

    public override string Emit()
    {
        StringBuilder builder = new StringBuilder();

        builder.Append($"resource \"{TerraformType}\" \"{Name.Replace('-', '_')}\" {{\r\n");
        builder.Append($"  resource_group_name = {ResourceGroupName}\r\n");
        builder.Append($"  location            = {Location}\r\n");
        builder.Append($"}}\r\n");
        builder.Append("\r\n");

        return builder.ToString();
    }
}