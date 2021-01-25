using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using blazorserver.Data;
using Microsoft.Azure.Management.ContainerInstance.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace blazorserver.Data.Resources
{
    public class SubnetNetworkSecurityGroupAssociation : AzureResource
    {
        // This is not a real thing in Azure, just a holder for a terraform resource
        // public static string AzureType = "Microsoft.Network/virtualNetworks/subnetAssociation";
        // public static string ApiVersion = "2020-07-01";
        public static string TerraformType = "azurerm_subnet_network_security_group_association";

        public string SubnetName { get; set; }
        public string NetworkSecurityGroupName { get; set; }

        public override string Emit()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append($"resource \"{TerraformType}\" \"{TerraformNameFromResourceName(SubnetName)}_{TerraformNameFromResourceName(NetworkSecurityGroupName)}\" {{\r\n");
            builder.Append($"  subnet_id                 = {Subnet.TerraformType}.{TerraformNameFromResourceName(SubnetName)}.id\r\n");
            builder.Append($"  network_security_group_id = {NetworkSecurityGroup.TerraformType}.{TerraformNameFromResourceName(NetworkSecurityGroupName)}.id\r\n");
            builder.Append($"}}\r\n");

            return builder.ToString();
        }
    }
}
