using System;
using System.Collections.Generic;
using UnityEngine;


public class AStar
{
    class Node
    {
        public Node(GameObject Tile)
        {
            tile = Tile;
            parent = null;
            gCost = -1;
            hCost = -1;
            fCost = -1;

            isTraversible = true;
            FloorTile t = Tile.GetComponent<FloorTile>();
            if (t != null && t.type == TileType.Wall) isTraversible = false;
        }

        // actual tile represented by this node
        public GameObject tile;
        // previous node in the shortest route to this node
        public Node parent;

        public bool isTraversible;
        
        public int gCost; // distance away from starting node
        public int hCost; // distance away from destination node
        public int fCost; // sum of g anf h cost


        // assigns g and h, sets f as sum of g and h
        public void SetCosts(int GCost, int HCost)
        {
            gCost = GCost;
            hCost = HCost;
            fCost = gCost + hCost;
        }
    }


    // cost is 10 * the number of horizontal steps + 14 * number of diagonal
    // steps required to get form origin to destination
    static int cost(Vector3 origin, Vector3 destination, float step)
    {
        // modify coordinates by step size
        int dx = (int)Mathf.Abs((origin.x - destination.x) / step);
        int dz = (int)Mathf.Abs((origin.z - destination.z) / step);

        // number of diagonal moves will be the smaller of x and z
        // number of horizontal moves will be their difference
        int cost = (Mathf.Min(dx, dz) * 14) + Mathf.Abs(dx - dz) * 10;

        return cost;
    }



    public static List<GameObject> findPath(List<GameObject> tiles, float step, GameObject start, GameObject end, int maxDepth)
    {
        Dictionary<GameObject,List<GameObject>> adjacency_list = ConvertGameObjectsToAdjacencyList(tiles, step);
        Dictionary<GameObject,Node> all_nodes = TileToNodeDictionary(tiles);


        List<Node> open = new List<Node>(); // set of nodes to be evaluated
        List<Node> closed = new List<Node>(); // set of nodes already evaluated


        // add start tile to open
        Node startNode = all_nodes[start];
        startNode.SetCosts(0, cost(start.transform.position, end.transform.position, step));
        open.Add(startNode);


        Node curr = null;
        // iterate until solution found or max depth reached
        for (int depth = 0; depth < maxDepth; ++depth)
        {
            // find best node in open to explore
            curr = BestNode(open);
            if (curr == null) break;

            // remove current node from open, and add it to closed
            open.Remove(curr);
            closed.Add(curr);

            // if current node is target, path has been found
            if (curr.tile == end) break;

            List<Node> neighbors = GetAdjacentNodes(curr, adjacency_list, all_nodes);
            foreach (Node n in neighbors)
            {
                // node inexplorable, or already has been explored
                if (!n.isTraversible || closed.Contains(n)) continue;

                // if neightbor is already an open node
                if (open.Contains(n))
                {
                    int new_g = cost(curr.tile.transform.position, n.tile.transform.position, step) + curr.gCost;
                    // update n with better g cost, maintain h cost
                    if (new_g < n.gCost)
                    {
                        n.SetCosts(new_g, n.hCost);
                        // because this is a better path, neighbors parent should be curr
                        n.parent = curr;
                    }
                }
                else
                {
                    // initialize the neighbors g and h costs, and add neighbor to open
                    int g = cost(curr.tile.transform.position, n.tile.transform.position, step) + curr.gCost;
                    int h = cost(n.tile.transform.position, end.transform.position, step);
                    n.SetCosts(g, h);
                    n.parent = curr;
                    open.Add(n);
                }
            }
        }


        // curr should now be the head of a linked list from end to start
        // convert to array, reverse, and return
        List<GameObject> path = ConvertLinkedListToGameObjectArray(curr);
        return path;
    }



    // creates dictionary where keys are objects, entries are lists of objects adjacent to the key
    static Dictionary<GameObject,List<GameObject>> ConvertGameObjectsToAdjacencyList(List<GameObject> tiles, float step)
    {
        // initialize empty adjacency list
        Dictionary<GameObject,List<GameObject>> adj = new Dictionary<GameObject,List<GameObject>>();

        // populate adjacency list with connections
        foreach (GameObject t in tiles)
        {
            adj.Add(t, new List<GameObject>());

            foreach (GameObject other in tiles)
            {
                // if the other tile is adjacent to this one
                if (isDiagonal(t, other, step) || isHorizontal(t, other, step))
                    adj[t].Add(other);
            }
        }

        return adj;
    }


    // test if two objects are within one diagonal step of each other
    static bool isDiagonal(GameObject a, GameObject b, float step, float margin = 0.1f)
    {
        float min = step - margin;
        float max = step + margin;
        float dx = Mathf.Abs(a.transform.position.x - b.transform.position.x);
        float dz = Mathf.Abs(a.transform.position.z - b.transform.position.z);
        // check if both displacements are in range
        return dx == Mathf.Clamp(dx, min, max) && dz == Mathf.Clamp(dz, min, max);
    }


    // test if two objects are within one horizontal step of each other
    static bool isHorizontal(GameObject a, GameObject b, float step, float margin = 0.1f)
    {
        float min = step - margin;
        float max = step + margin;
        float dx = Mathf.Abs(a.transform.position.x - b.transform.position.x);
        float dz = Mathf.Abs(a.transform.position.z - b.transform.position.z);
        // check if either displacement is in range and one is near zero
        if (dx == Mathf.Clamp(dx, min, max) && dz == Mathf.Clamp(dz, -margin, margin)) return true;
        if (dz == Mathf.Clamp(dz, min, max) && dx == Mathf.Clamp(dx, -margin, margin)) return true;
        return false;
    }


    // Creates a dictionary where keys are objects, and entries are nodes assiciated with each object
    static Dictionary<GameObject,Node> TileToNodeDictionary(List<GameObject> tiles)
    {
        Dictionary<GameObject,Node> nodes = new Dictionary<GameObject,Node>();
        foreach (GameObject t in tiles) nodes.Add(t, new Node(t));
        return nodes;
    }


    // the node in the array with the lowest f cost. If there is a tie, the node with the
    // lowest h cost is returned
    static Node BestNode(List<Node> openNodes)
    {
        Node best = null;
        int lowest_f = int.MaxValue;
        int lowest_h = int.MaxValue;

        foreach (Node node in openNodes)
        {
            // new best node found
            if (node.fCost < lowest_f || (node.fCost == lowest_f && node.hCost <= lowest_h))
            {
                lowest_f = node.fCost;
                lowest_h = node.hCost;
                best = node;
            }
        }
        return best;
    }


    // returns nodes adjacent to the input node
    static List<Node> GetAdjacentNodes(Node node, Dictionary<GameObject,List<GameObject>> adjacency_list, Dictionary<GameObject,Node> all_nodes)
    {
        List<Node> adj = new List<Node>();

        // get adjacent game objects and use dictionary to convert to nodes
        List<GameObject> tiles = adjacency_list[node.tile];
        foreach (GameObject t in tiles) adj.Add(all_nodes[t]);

        return adj;
    }


    // takes the head of a linked node list, from end to start,
    // and creates an array of game objects, from start to end
    static List<GameObject> ConvertLinkedListToGameObjectArray(Node head)
    {
        List<GameObject> arr = new List<GameObject>();

        // convert the linked list to an array
        for (Node curr = head; curr != null; curr = curr.parent)
            arr.Add(curr.tile);

        // reverse the array and return
        arr.Reverse();
        return arr;
    }
}
