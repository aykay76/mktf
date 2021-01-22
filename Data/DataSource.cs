using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using blazorserver.Data;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

public class DataSource
{
    public string ResourceType { get; set; }
    public string SourceName { get; set; }
    public Dictionary<string, string> Attributes { get; set; }

    public DataSource()
    {
        Attributes = new Dictionary<string, string>();
    }

    public string Emit()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"data \"{ResourceType}\" \"{SourceName}\" {{");
        foreach (KeyValuePair<string, string> kvp in Attributes)
        {
            builder.AppendLine($"{kvp.Key} = \"{kvp.Value}\"");
        }
        builder.AppendLine($"}}");
        builder.AppendLine();

        return builder.ToString();
    }
}