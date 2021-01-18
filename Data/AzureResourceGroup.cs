using System;

namespace blazorserver.Data
{
    public class AzureResourceGroup
    {
        public Guid SubscriptionID { get; set; }
        public string ID { get; set; }
        public string DisplayName { get; set; }
    }
}