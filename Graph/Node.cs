using System.Collections.Generic;

public class Node<T>
{
    // Private member-variables
    private T data;
    private NodeList<T> neighbours = null;
    private List<string> relations;

    public Node() { }
    public Node(T data) : this(data, null) { }
    public Node(T data, NodeList<T> neighbours)
    {
        this.data = data;
        this.neighbours = neighbours;
    }

    public T Value
    {
        get
        {
            return data;
        }
        set
        {
            data = value;
        }
    }

    // TODO: probably expand on this to contain an edge object that can hold many fields

    public NodeList<T> Neighbours
    {
        get
        {
            if (neighbours == null)
                neighbours = new NodeList<T>();

            return neighbours;
        }
        set
        {
            neighbours = value;
        }
    }

    public List<string> Relations
    {
        get
        {
            if (relations == null)
                relations = new List<string>();

            return relations;
        }
    }

}
