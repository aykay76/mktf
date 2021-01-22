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
// The basic information in this class comes from a simple list of resources, stub information to then be able to get more
// https://docs.microsoft.com/en-us/rest/api/resources/resources/listbyresourcegroup

public class AzureResource
{
    public Dictionary<string, string> Variables { get; set; }
    public JsonElement Description { get; set; }
    public string ID { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string Location { get; set; }
    public string ResourceGroupName { get; set; }
    public List<DataSource> ExternalReferences { get; set; }
    
    public AzureResource()
    {
        Variables = new Dictionary<string, string>();
        ExternalReferences = new List<DataSource>();
    }

    public static AzureResource FromJsonElement(JsonElement element)
    {
        AzureResource resource = new AzureResource();
        resource.ID = element.GetProperty("id").GetString();
        resource.Name = element.GetProperty("name").GetString();
        resource.Type = element.GetProperty("type").GetString();
        resource.Location = element.GetProperty("location").GetString();
        return resource;
    }

    public static string TerraformNameFromResourceName(string resourceName)
    {
        return resourceName.Replace('-', '_').Replace('.', '_');
    }

    protected static bool ResourceInResourceGroup(string id, string resourceGroupName)
    {
        string[] parts = id.Substring(1).Split('/');
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i] == "resourceGroups" && string.Compare(resourceGroupName, parts[i + 1], true) == 0) return true;
        }

        return false;
    }

    protected static string ExtractSubnetName(string resourceId)
    {
        string[] parts = resourceId.Substring(1).Split('/');
        return parts[9];
    }

    public virtual string Emit()
    {
        return $"# Unhandled resource: {ID}, {Name}, {Type}, {Location}\r\n";
    }
}