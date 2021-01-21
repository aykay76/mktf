using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using blazorserver.Data;
using Microsoft.Azure.Management.ContainerInstance.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using static System.Text.Json.JsonElement;

public class DnsZoneA : AzureResource
{
    public static string AzureType = "Microsoft.Network/dnszones/A";
    public static string ApiVersion = "2018-05-01";
    public static string TerraformType = "azurerm_dns_a_record";

    public string ZoneName { get; set; }
    public int TTL { get; set; }
    public List<string> IPv4 { get; set; }

    public DnsZoneA()
    {
        IPv4 = new List<string>();
    }

    public static new AzureResource FromJsonElement(JsonElement element)
    {
        DnsZoneA resource = new DnsZoneA();
        resource.Description = element;

        // basic information
        resource.ID = element.GetProperty("id").GetString();
        resource.Name = element.GetProperty("name").GetString();
        resource.Type = element.GetProperty("type").GetString();

        JsonElement properties = element.GetProperty("properties");
        // ZoneName will get set outside this method because it's not included in the record resource
        resource.TTL = properties.GetProperty("TTL").GetInt32();
        ArrayEnumerator recordEnum = properties.GetProperty("ARecords").EnumerateArray();
        while (recordEnum.MoveNext())
        {
            resource.IPv4.Add(recordEnum.Current.GetProperty("ipv4Address").GetString());
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
        builder.Append($"  name                = \"{Name}\"\r\n");
        builder.Append($"  zone_name = {DnsZone.TerraformType}.{TerraformNameFromResourceName(ZoneName)}.name\r\n");
        builder.Append($"  resource_group_name = {ResourceGroupName}\r\n");

        builder.Append($"  ttl = {TTL}\r\n");
        builder.Append("  records = [");
        bool first = true;
        foreach (string record in IPv4)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                builder.Append(",");
            }

            builder.Append($"\"{record}\"");
        }
        builder.Append("]\r\n");

        builder.Append($"}}\r\n");
        builder.Append("\r\n");

        return builder.ToString();
    }
}