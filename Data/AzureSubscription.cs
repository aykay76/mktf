using System;
using System.Text.Json;

namespace blazorserver.Data
{
    public class AzureSubscription
    {
        public string ID { get; set; }
        public string DisplayName { get; set; }

        public static AzureSubscription FromJsonElement(JsonElement element)
        {
            AzureSubscription result = new AzureSubscription();
            
            result.ID = element.GetProperty("id").GetString();
            result.DisplayName = element.GetProperty("displayName").GetString();

            return result;
        }
    }
}