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

        // resource group will always be root of graph
        Node<AzureResource> resourceGroupNode = new Node<AzureResource>(resourceGroup);
        graph.AddNode(resourceGroupNode);

        foreach (var stub in resources)
        {
            // ask the Azure service to load the full resource, if it can't it should return the same stub
            List<AzureResource> resourceList = await _azureService.LoadResource(stub);
            foreach (AzureResource resource in resourceList)
            {
                resource.ResourceGroupName = resourceGroup.Name;
                
                // create a node and add it to the graph
                Node<AzureResource> resourceNode = new Node<AzureResource>(resource);
                graph.AddNode(resourceNode);

                // TODO: how to handle relationships between subobjects? probably assume first object is parent and subsequent objects are children

                // link the resource node to the resource group node
                graph.AddDirectedEdge(resourceNode, resourceGroupNode, "contained by");
                graph.AddDirectedEdge(resourceGroupNode, resourceNode, "contains");
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