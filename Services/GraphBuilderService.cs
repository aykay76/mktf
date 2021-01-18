using System.Collections.Generic;
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

    public Graph<AzureResource> BuildGraph(string subscriptionId, string resourceGroupName, IGenericResource[] resources)
    {
        Graph<AzureResource> graph = new Graph<AzureResource>();

        // resource group will always be root of graph
        var rg = _azureService.GetResourceGroup(subscriptionId, resourceGroupName);
        Node<AzureResource> resourceGroupNode = new Node<AzureResource>(new ResourceGroup() { Inner = rg});
        graph.AddNode(resourceGroupNode);

        foreach (var resource in resources)
        {
            // start with a generic node, it's not expensive if we recreate it for something specific
            Node<AzureResource> resourceNode = new Node<AzureResource>(new AzureResource() { Inner = resource});

            // see if we can identify what it really is
            if (resource.ResourceProviderNamespace == "Microsoft.Network")
            {
                if (resource.ResourceType == "publicIPAddresses")
                {
                    IPublicIPAddress ip = _azureService.GetPublicIPAddress(subscriptionId, resourceGroupName, resource.Name);
                    resourceNode = new Node<AzureResource>(new PublicIPAddress() { Inner = ip});
                }
                if (resource.ResourceType == "applicationGateways")
                {
                    IApplicationGateway gw = _azureService.GetApplicationGateway(subscriptionId, resourceGroupName, resource.Name);
                    resourceNode = new Node<AzureResource>(new ApplicationGateway() { Inner = gw});
                }
            }
            else if (resource.ResourceProviderNamespace == "Microsoft.ContainerInstance")
            {
                if (resource.ResourceType == "containerGroups")
                {
                    IContainerGroup cg = _azureService.GetContainerGroup(subscriptionId, resourceGroupName, resource.Name);
                    resourceNode = new Node<AzureResource>(new ContainerGroup() { Inner = cg});
                }
            }
            
            // add the node
            graph.AddNode(resourceNode);

            // add edges between the resource node and the resourcegroup node
            graph.AddDirectedEdge(resourceNode, resourceGroupNode, "contained by");
            graph.AddDirectedEdge(resourceGroupNode, resourceNode, "contains");
        }

        // iterate through the nodes, looking for relationships
        List<string> refs = null;
        foreach (var node in graph.Nodes)
        {
            refs = node.Value.GetReferences();
            if (refs != null)
            {
                AddEdgesForRefs(graph, refs, node);
            }
        }

        return graph;
    }

    private void AddEdgesForRefs(Graph<AzureResource> graph, List<string> refs, Node<AzureResource> node)
    {
        foreach (var r in refs)
        {
            foreach (var other in graph.Nodes)
            {
                if (other.Value.Resource().Id == r)
                {
                    // TODO: need a way of defining the reference, is it an attribute or sub-block?
                    graph.AddDirectedEdge(node, other, "related");
                }
            }
        }
    }
}