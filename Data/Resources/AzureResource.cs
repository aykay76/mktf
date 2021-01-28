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

namespace blazorserver.Data.Resources
{
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

        protected static string ExtractPart(string resourceId, int part)
        {
            string[] parts = resourceId.Substring(1).Split('/');
            return parts[part];
        }

        // TODO: might be able to refactor to generic method?
        //"/subscriptions/{subscriptionID}/resourceGroups/{groupName}/providers/Microsoft.Web/serverfarms/{appServicePlanName}".
        protected string GetAppServicePlanReference(string aspId)
        {
            string subnetRef = string.Empty;

            // TODO: refactor to generic get-x-field-from-id method
            string resourceGroupName = ExtractPart(aspId, 3);
            string aspName = ExtractPart(aspId, 7);

            if (string.Compare(resourceGroupName, ResourceGroupName, true) == 0)
            {
                // in same resource group so will get picked up as resource, add reference type
                subnetRef = $"{AppServicePlan.TerraformType}.{TerraformNameFromResourceName(aspName)}.id";
            }
            else
            {
                subnetRef = $"data.{Subnet.TerraformType}.{TerraformNameFromResourceName(aspName)}.id";

                DataSource source = new DataSource();
                source.ResourceType = Subnet.TerraformType;
                source.SourceName = aspName;
                source.Attributes.Add("name", aspName);
                source.Attributes.Add("resource_group_name", resourceGroupName);
                ExternalReferences.Add(source);
            }

            return subnetRef;
        }

        protected string GetSubnetReference(string subnetId)
        {
            string subnetRef = string.Empty;
            string subnetName = ExtractSubnetName(subnetId);

            if (ResourceInResourceGroup(subnetId, ResourceGroupName))
            {
                // in same resource group so will get picked up as resource, add reference type
                subnetRef = $"{Subnet.TerraformType}.{TerraformNameFromResourceName(subnetName)}.id";
            }
            else
            {
                string[] parts = subnetId.Substring(1).Split('/');

                subnetRef = $"data.{Subnet.TerraformType}.{TerraformNameFromResourceName(subnetName)}.id";

                DataSource source = new DataSource();
                source.ResourceType = Subnet.TerraformType;
                source.SourceName = subnetName;
                source.Attributes.Add("name", subnetName);
                source.Attributes.Add("virtual_network_name", parts[7]);
                source.Attributes.Add("resource_group_name", parts[3]);
                ExternalReferences.Add(source);
            }

            return subnetRef;
        }

        public virtual string Emit()
        {
            return $"# Unhandled resource: {ID}, {Name}, {Type}, {Location}\r\n";
        }
    }
}