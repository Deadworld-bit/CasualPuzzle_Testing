using System;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinder
{
    private class Node
    {
        public Vector2Int position;
        public Node parent;
        public float gCost;
        public float hCost;
        public float fCost => gCost + hCost;
    }

    private static int _gridWidth;
    private static int _gridHeight;

    public static void SetGridDimensions(int width, int height)
    {
        _gridWidth = width;
        _gridHeight = height;
    }

    // My head hurt when doing this
    public static List<Vector2Int> FindPath(Vector2Int start, Vector2Int end, Func<Vector2Int, bool> isWalkable)
    {
        var openSet = new List<Node>();
        var closedSet = new HashSet<Vector2Int>();
        var startNode = new Node { position = start, gCost = 0, hCost = Vector2Int.Distance(start, end) };
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            var currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost || (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode.position);

            if (currentNode.position == end)
            {
                return ReconstructPath(currentNode);
            }

            foreach (var neighbor in GetNeighbors(currentNode.position))
            {
                if (!isWalkable(neighbor) || closedSet.Contains(neighbor))
                    continue;

                float tentativeGCost = currentNode.gCost + 1;
                var neighborNode = openSet.Find(n => n.position == neighbor);
                if (neighborNode == null)
                {
                    neighborNode = new Node { position = neighbor, parent = currentNode, gCost = tentativeGCost, hCost = Vector2Int.Distance(neighbor, end) };
                    openSet.Add(neighborNode);
                }
                else if (tentativeGCost < neighborNode.gCost)
                {
                    neighborNode.parent = currentNode;
                    neighborNode.gCost = tentativeGCost;
                }
            }
        }
        return null;
    }

    private static List<Vector2Int> ReconstructPath(Node node)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        while (node != null)
        {
            path.Add(node.position);
            node = node.parent;
        }
        path.Reverse();
        return path;
    }

    private static List<Vector2Int> GetNeighbors(Vector2Int pos)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        if (pos.x > 0) neighbors.Add(new Vector2Int(pos.x - 1, pos.y));
        if (pos.x < _gridWidth - 1) neighbors.Add(new Vector2Int(pos.x + 1, pos.y));
        if (pos.y > 0) neighbors.Add(new Vector2Int(pos.x, pos.y - 1));
        if (pos.y < _gridHeight - 1) neighbors.Add(new Vector2Int(pos.x, pos.y + 1));
        return neighbors;
    }
}