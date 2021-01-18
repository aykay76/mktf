using System;
using System.Collections.Generic;

namespace blazorserver.Data
{
    public class ResourceType
    {
        public string resourceType { get; set; }
    }

    public class AzureProvider
    {
        public string id { get; set; }
        public string @namespace { get; set; }
        public List<ResourceType> resourceTypes { get; set; }
    }
}