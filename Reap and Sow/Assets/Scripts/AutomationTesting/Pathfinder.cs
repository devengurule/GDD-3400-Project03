using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Pathfinder : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NodeGraph graph;   // Assign your NodeGraph in Inspector
    public void SetGraph(NodeGraph g) => graph = g;


    /// <summary>
    /// World -> world path using A* over NodeGraph cells.
    /// Returns world-space waypoints (center of each cell).
    /// </summary>
    public List<Vector2> FindPathWorld(Vector2 worldStart, Vector2 worldEnd)
    {
        var result = new List<Vector2>();

        if (!graph)
        {
            Debug.LogWarning("[Pathfinder] No NodeGraph assigned.");
            return result;
        }

        // Convert to grid cells
        var startCell = graph.WorldToCell(worldStart);
        var endCell = graph.WorldToCell(worldEnd);

        // Strict: both endpoints must be valid nodes in the graph
        if (!graph.Connections.ContainsKey(startCell) || !graph.Connections.ContainsKey(endCell))
            {
                // Graph has no nodes at all
                Debug.LogWarning("[Pathfinder] No valid start cell and no nodes in graph.");
                return result;
            }

        // If the end cell is not in the graph, snap to the closest node
        if (!graph.Connections.ContainsKey(endCell))
        {
            if (!graph.TryGetClosestCell(worldEnd, out endCell))
            {
                Debug.LogWarning("[Pathfinder] No valid end cell and no nodes in graph.");
                return result;
            }
        }

        // Now both endpoints are guaranteed to be on valid nodes.
        var cellPath = FindPathCells(startCell, endCell);

        if (cellPath == null || cellPath.Count == 0)
        {
            // Still no path (e.g. disconnected islands)
            // For now, just return empty. You could add extra fallback here if you want.
            return result;
        }

        // Convert cell path back to world positions (node centers)
        for (int i = 0; i < cellPath.Count; i++)
        {
            result.Add(graph.CellToWorld(cellPath[i]));
        }

        return result;
    }


    /// <summary>
    /// Cell -> cell path using A* (open set, closed set, cameFromNode, costSoFar, costToEnd).
    /// Heuristic = Euclidean distance in world space via NodeGraph.CellToWorld.
    /// </summary>
    public List<Vector2Int> FindPathCells(Vector2Int startCell, Vector2Int endCell)
    {
        // STEP 1: Data structures
        List<Vector2Int> openSet = new List<Vector2Int>();
        HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFromNode = new Dictionary<Vector2Int, Vector2Int>();
        Dictionary<Vector2Int, float> costSoFar = new Dictionary<Vector2Int, float>();   // g
        Dictionary<Vector2Int, float> costToEnd = new Dictionary<Vector2Int, float>();   // f = g + h

        // Guards
        if (!graph || graph.Connections == null ||
            !graph.Connections.ContainsKey(startCell) ||
            !graph.Connections.ContainsKey(endCell))
            return new List<Vector2Int>();

        // STEP 2: Seed start
        openSet.Add(startCell);
        costSoFar[startCell] = 0f;
        costToEnd[startCell] = Heuristic(startCell, endCell);

        // STEP 3: Loop
        while (openSet.Count > 0)
        {
            // 3a) pick lowest f
            Vector2Int current = GetLowestCost(openSet, costToEnd);

            // 3b) goal reached
            if (current == endCell)
                return ReconstructPath(cameFromNode, current);

            // 3c) advance
            openSet.Remove(current);
            closedSet.Add(current);

            // 3d) neighbors
            var neighbors = graph.GetNeighbors(current);
            for (int i = 0; i < neighbors.Count; i++)
            {
                var neighbor = neighbors[i];
                if (closedSet.Contains(neighbor)) continue;

                float tentativeG = costSoFar[current] + 1f; // uniform edge cost (set to graph cell size if you prefer)

                // Discover or improve best path to neighbor
                if (!costSoFar.ContainsKey(neighbor) || tentativeG < costSoFar[neighbor])
                {
                    cameFromNode[neighbor] = current;
                    costSoFar[neighbor] = tentativeG;
                    costToEnd[neighbor] = tentativeG + Heuristic(neighbor, endCell);

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        // No path
        return new List<Vector2Int>();
    }

    // --- Internals ---

    // Euclidean distance in *world* space (uses NodeGraph centers)
    private float Heuristic(Vector2Int a, Vector2Int b)
    {
        if (!graph) return 0f;
        return Vector2.Distance(graph.CellToWorld(a), graph.CellToWorld(b));
    }

    private Vector2Int GetLowestCost(List<Vector2Int> open, Dictionary<Vector2Int, float> fMap)
    {
        Vector2Int best = open[0];
        float bestF = fMap.TryGetValue(best, out var fv) ? fv : float.PositiveInfinity;

        for (int i = 1; i < open.Count; i++)
        {
            var n = open[i];
            float fn = fMap.TryGetValue(n, out var v) ? v : float.PositiveInfinity;
            if (fn < bestF) { best = n; bestF = fn; }
        }
        return best;
    }

    private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFromNode, Vector2Int current)
    {
        var total = new List<Vector2Int> { current };
        while (cameFromNode.TryGetValue(current, out var prev))
        {
            current = prev;
            total.Insert(0, current);
        }
        return total;
    }

    public Vector2Int GetNearestWalkableNode(Vector2 worldPos)
    {
        if (graph == null) return Vector2Int.zero;

        Vector2Int currentCell = graph.WorldToCell(worldPos);

        // If the node is already free, return it
        if (CellIsFree(currentCell))
            return currentCell;

        // Otherwise, search outward (BFS) for nearest free node
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        queue.Enqueue(currentCell);
        visited.Add(currentCell);

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            if (CellIsFree(node))
                return node;

            foreach (var neighbor in graph.GetNeighbors(node))
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        // Fallback: nowhere to go
        return currentCell;
    }

    // Helper: checks if a node is free (copy from NodeGraph or Pathfinder if you have access)
    private bool CellIsFree(Vector2Int g)
    {
        Vector2 center = graph.CellToWorld(g); // + offset if needed
        Vector2 size = graph.AgentSize + Vector2.one * 0.02f; // _clearancePad
        var hits = Physics2D.OverlapCapsuleAll(center, size, CapsuleDirection2D.Vertical, 0f);
        foreach (var c in hits)
        {
            if (c == null || c.isTrigger) continue;
            if (c is CompositeCollider2D || c is TilemapCollider2D || c.CompareTag("BarrierTag"))
                return false;
        }
        return true;
    }
}
