using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using blazorserver.Data;
using Microsoft.Azure.Management.ContainerInstance.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using static System.Text.Json.JsonElement;

public class DnsZoneTXT : AzureResource
{
    public static string AzureType = "Microsoft.Network/dnszones/TXT";
    public static string ApiVersion = "2018-05-01";
    public static string TerraformType = "azurerm_dns_txt_record";

    public string ZoneName { get; set; }
    public int TTL { get; set; }
    public List<string> TXT { get; set; }

    public DnsZoneTXT()
    {
        TXT = new List<string>();
    }

    public static new AzureResource FromJsonElement(JsonElement element)
    {
        DnsZoneTXT resource = new DnsZoneTXT();
        resource.Description = element;

        // basic information
        resource.ID = element.GetProperty("id").GetString();
        resource.Name = element.GetProperty("name").GetString();
        resource.Type = element.GetProperty("type").GetString();

        JsonElement properties = element.GetProperty("properties");
        // ZoneName will get set outside this method because it's not included in the record resource
        resource.TTL = properties.GetProperty("TTL").GetInt32();
        ArrayEnumerator TXTenum = properties.GetProperty("TXTRecords").EnumerateArray();
        while (TXTenum.MoveNext())
        {
            ArrayEnumerator valueEnum = TXTenum.Current.GetProperty("value").EnumerateArray();
            while (valueEnum.MoveNext())
            {
                resource.TXT.Add(valueEnum.Current.GetString());
            }
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

        builder.AppendLine($"resource \"{TerraformType}\" \"{TerraformNameFromResourceName(Name)}\" {{");
        builder.AppendLine($"  name                = \"{Name}\"");
        builder.AppendLine($"  zone_name = {DnsZone.TerraformType}.{TerraformNameFromResourceName(ZoneName)}.name");
        builder.AppendLine($"  resource_group_name = {ResourceGroupName}");

        builder.AppendLine("  record {");
        foreach (string txt in TXT)
        {
            builder.AppendLine($"    value = \"{txt}\"");
        }
        builder.AppendLine("  }");

        builder.AppendLine($"}}");
        builder.AppendLine();

        return builder.ToString();
    }
}