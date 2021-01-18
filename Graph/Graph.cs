using System.Collections.Generic;

public class Graph<T>
{
    private NodeList<T> nodeSet;

    public Graph() : this(null) {}
    public Graph(NodeList<T> nodeSet)
    {
        if (nodeSet == null)
            this.nodeSet = new NodeList<T>();
        else
            this.nodeSet = nodeSet;
    }

    public void AddNode(Node<T> node)
    {
        // adds a node to the graph
        nodeSet.Add(node);
    }

    public void AddNode(T value)
    {
        // adds a node to the graph
        nodeSet.Add(new Node<T>(value));
    }

    public void AddDirectedEdge(Node<T> from, Node<T> to, string relation)
    {
        from.Neighbours.Add(to);
        from.Relations.Add(relation);
    }

    public void AddUndirectedEdge(Node<T> from, Node<T> to, string relation)
    {
        from.Neighbours.Add(to);
        from.Relations.Add(relation);

        to.Neighbours.Add(from);
        to.Relations.Add(relation);
    }

    public bool Contains(T value)
    {
        return nodeSet.FindByValue(value) != null;
    }

    public bool Remove(T value)
    {
        // first remove the node from the nodeset
        Node<T> nodeToRemove = (Node<T>) nodeSet.FindByValue(value);
        if (nodeToRemove == null)
            // node wasn't found
            return false;

        // otherwise, the node was found
        nodeSet.Remove(nodeToRemove);

        // enumerate through each node in the nodeSet, removing edges to this node
        foreach (Node<T> gnode in nodeSet)
        {
            int index = gnode.Neighbours.IndexOf(nodeToRemove);
            if (index != -1)
            {
                // remove the reference to the node and associated cost
                gnode.Neighbours.RemoveAt(index);
                gnode.Relations.RemoveAt(index);
            }
        }

        return true;
    }

    public NodeList<T> Nodes
    {
        get
        {
            return nodeSet;
        }
    }

    public int Count
    {
        get { return nodeSet.Count; }
    }
}