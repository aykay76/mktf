using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using blazorserver.Data;
using Microsoft.Azure.Management.ContainerInstance.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

public class DnsZoneCNAME : AzureResource
{
    public static string AzureType = "Microsoft.Network/dnszones/CNAME";
    public static string ApiVersion = "2018-05-01";
    public static string TerraformType = "azurerm_dns_cname_record";

    public string ZoneName { get; set; }
    public int TTL { get; set; }
    public string CNAME { get; set; }

    public static new AzureResource FromJsonElement(JsonElement element)
    {
        DnsZoneCNAME resource = new DnsZoneCNAME();
        resource.Description = element;

        // basic information
        resource.ID = element.GetProperty("id").GetString();
        resource.Name = element.GetProperty("name").GetString();
        resource.Type = element.GetProperty("type").GetString();

        JsonElement properties = element.GetProperty("properties");
        // ZoneName will get set outside this method because it's not included in the record resource
        resource.TTL = properties.GetProperty("TTL").GetInt32();
        resource.CNAME = properties.GetProperty("CNAMERecord").GetProperty("cname").GetString();

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
        builder.Append($"  name                = \"{Name}\"\r\n");
        builder.Append($"  zone_name = {DnsZone.TerraformType}.{TerraformNameFromResourceName(ZoneName)}.name\r\n");
        builder.Append($"  resource_group_name = {ResourceGroupName}\r\n");

        builder.Append($"  ttl = {TTL}\r\n");
        builder.Append($"  record = \"{CNAME}\"\r\n");
        builder.Append($"}}\r\n");
        builder.Append("\r\n");

        return builder.ToString();
    }
}