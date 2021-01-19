using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using blazorserver.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.ResourceManager.Fluent.GenericResource.Update;
using Microsoft.Azure.Management.ResourceManager.Fluent.Models;

/// This is the base class for Azure resources that will be included in output
// each superclass will implement its own methods to handle references, naming, code emission etc.
public class AzureResource
{
    public Dictionary<string, string> Variables { get; set; }
    public JsonElement Description { get; set; }
    public string ID { get; set; }
    public string ResourceProviderNamespace { get; set; }
    public string Name { get; set; }
    public string ResourceType { get; set; }
    public string Location { get; set; }
    
    // TODO: implement sub-blocks

    public AzureResource()
    {
    }

    public static AzureResource FromJsonElement(JsonElement element)
    {
        AzureResource resource = new AzureResource();
        resource.ID = element.GetProperty("id").GetString();
        resource.Name = element.GetProperty("name").GetString();
        resource.Location = element.GetProperty("location").GetString();
        return resource;
    }

    public virtual List<string> GetReferences()
    {
        return null;
    }

    public virtual string Emit()
    {
        return $"Unhandled resource: {ID}, {Name}, {ResourceType}\r\n";
    }
}