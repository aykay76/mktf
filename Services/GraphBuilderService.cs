using System.Collections.Generic;
using System.Threading.Tasks;
using blazorserver.Data;
using Microsoft.Azure.Management.ContainerInstance.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;

public class GraphBuilderService
{
    // TODO: prepare for multi cloud and abstract away Azure specificity
    private AzureService _azureService;

    public GraphBuilderService(AzureService azureService)
    {
        _azureService = azureService;
    }

    public async Task<Graph<AzureResource>> BuildGraph(ResourceGroup resourceGroup, List<AzureResource> resources)
    {
        Graph<AzureResource> graph = new Graph<AzureResource>();

        // Deep load resources so that we can start to identify links between them
        // https://docs.microsoft.com/en-us/rest/api/resources/resources/listbyresourcegroup

        // resource group will always be root of graph
        Node<AzureResource> resourceGroupNode = new Node<AzureResource>(resourceGroup);
        graph.AddNode(resourceGroupNode);

        // foreach (var resource in resources)
        // {
        //     // start with a generic node, it's not expensive if we recreate it for something specific
        //     Node<AzureResource> resourceNode = new Node<AzureResource>(new AzureResource());

        //     // see if we can identify what it really is
        //     if (resource.ResourceProviderNamespace == "Microsoft.Network")
        //     {
        //         if (resource.ResourceType == "publicIPAddresses")
        //         {
        //             IPublicIPAddress ip = await _azureService.GetPublicIPAddress(subscriptionId, resourceGroupName, resource.Name);
        //             resourceNode = new Node<AzureResource>(new PublicIPAddress() { Inner = ip});
        //         }
        //         if (resource.ResourceType == "applicationGateways")
        //         {
        //             IApplicationGateway gw = await _azureService.GetApplicationGateway(subscriptionId, resourceGroupName, resource.Name);
        //             resourceNode = new Node<AzureResource>(new ApplicationGateway() { Inner = gw});
        //         }
        //         if (resource.ResourceType == "virtualNetworks")
        //         {
        //             // TODO: add nodes for the network, subnets and NSG associations
        //             INetwork vnet = await _azureService.GetNetwork(subscriptionId, resourceGroupName, resource.Name);
        //             resourceNode = new Node<AzureResource>(new VirtualNetwork() { Inner = vnet });

        //             foreach (string key in vnet.Subnets.Keys)
        //             {
        //                 ISubnet subnet = vnet.Subnets[key];
        //                 Node<AzureResource> subnetNode = new Node<AzureResource>(new Subnet() { Inner = subnet });
        //                 graph.AddNode(subnetNode);
        //             }
        //         }
        //     }
        //     else if (resource.ResourceProviderNamespace == "Microsoft.ContainerInstance")
        //     {
        //         if (resource.ResourceType == "containerGroups")
        //         {
        //             IContainerGroup cg = await _azureService.GetContainerGroup(subscriptionId, resourceGroupName, resource.Name);
        //             resourceNode = new Node<AzureResource>(new ContainerGroup() { Inner = cg});
        //         }
        //     }
            
        //     // add the node
        //     graph.AddNode(resourceNode);

        //     // add edges between the resource node and the resourcegroup node
        //     graph.AddDirectedEdge(resourceNode, resourceGroupNode, "contained by");
        //     graph.AddDirectedEdge(resourceGroupNode, resourceNode, "contains");
        // }

        // // iterate through the nodes, looking for relationships
        // List<string> refs = null;
        // foreach (var node in graph.Nodes)
        // {
        //     refs = node.Value.GetReferences();
        //     if (refs != null)
        //     {
        //         AddEdgesForRefs(graph, refs, node);
        //     }
        // }

        return graph;
    }

    private void AddEdgesForRefs(Graph<AzureResource> graph, List<string> refs, Node<AzureResource> node)
    {
        foreach (var r in refs)
        {
            foreach (var other in graph.Nodes)
            {
                // TODO: this will need to change because not all resources are derived from IResource - subnets for example are IChildResource
                if (other.Value.ID == r)
                {
                    // TODO: need a way of defining the reference, is it an attribute or sub-block?
                    graph.AddDirectedEdge(node, other, "related");
                }
            }
        }
    }
}