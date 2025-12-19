using UnityEngine;
using UnityEngine.Tilemaps; // for TilemapCollider2D
using System.Collections.Generic;

public class NodeGraph : MonoBehaviour
{
    [Header("Tiling")]
    [SerializeField] private GameObject _node;
    [SerializeField] private float _cellSize = 0.2f;
    [SerializeField] private int _maxNodes = 400;

    [Header("Gizmos")]
    [SerializeField] private bool _drawGizmos = true;
    [SerializeField] private bool _drawOrigin = true;
    [SerializeField] private bool _drawAccepted = true;
    [SerializeField] private bool _drawRejected = true;

    [Header("Agent Clearance")]
    [SerializeField] private Vector2 _agentSize = new Vector2(0.13f, 0.18f); // width, height
    public Vector2 AgentSize { get => _agentSize; set => _agentSize = value; }
    [SerializeField] private Vector2 _agentOffset = new Vector2(0f, -0.05f); // from agent center
    [SerializeField] private CapsuleDirection2D _agentDir = CapsuleDirection2D.Vertical;
    [SerializeField] private float _clearancePad = 0.02f; // extra margin

    public void SetAgentFromCapsule(CapsuleCollider2D cap, float pad = 0.02f)
    {
        if (!cap) return;
        _agentSize = cap.size;
        _agentOffset = cap.offset;
        _agentDir = cap.direction == CapsuleDirection2D.Vertical
                     ? CapsuleDirection2D.Vertical : CapsuleDirection2D.Horizontal;
        _clearancePad = Mathf.Max(0f, pad);
    }


    // Walkable/blocked cells (debug + data)
    private readonly HashSet<Vector2Int> _accepted = new HashSet<Vector2Int>();
    private readonly HashSet<Vector2Int> _rejected = new HashSet<Vector2Int>();

    // Graph connections: cell -> its walkable cardinal neighbors
    private readonly Dictionary<Vector2Int, List<Vector2Int>> _connections = new Dictionary<Vector2Int, List<Vector2Int>>();
    public IReadOnlyDictionary<Vector2Int, List<Vector2Int>> Connections => _connections;


    private static readonly Vector2Int[] Cardinal =
    {
        new Vector2Int( 1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int( 0, 1),
        new Vector2Int( 0,-1)
    };

    private void Awake()
    {
        FloodFill();
        BuildConnections();
    }

    private void FloodFill()
    {
        if (_node == null) { Debug.LogWarning("[NodeGraph] No node prefab assigned."); return; }
        if (_cellSize <= 0f) { Debug.LogWarning("[NodeGraph] _cellSize must be > 0."); return; }

        _accepted.Clear(); _rejected.Clear();
        _connections.Clear();

        var originCell = WorldToCell(transform.position);
        var visited = new HashSet<Vector2Int>();
        var q = new Queue<Vector2Int>();
        q.Enqueue(originCell);
        visited.Add(originCell);

        int placed = 0;
        while (q.Count > 0 && placed < _maxNodes)
        {
            var g = q.Dequeue();

            if (CellIsFree(g))
            {
                _accepted.Add(g);
                Instantiate(_node, CellToWorld(g), Quaternion.identity, transform);
                placed++;

                foreach (var s in Cardinal)
                {
                    var n = g + s;
                    if (visited.Add(n)) q.Enqueue(n);
                }
            }
            else
            {
                _rejected.Add(g);
            }
        }
    }

    // Build adjacency from accepted cells (cardinal neighbors only)
    private void BuildConnections()
    {
        _connections.Clear();

        foreach (var cell in _accepted)
        {
            var list = new List<Vector2Int>(4);
            foreach (var step in Cardinal)
            {
                var n = cell + step;
                if (_accepted.Contains(n) && EdgeIsFree(cell, n))
                    list.Add(n);
            }
            _connections[cell] = list;
        }
    }

    // Helper your Pathfinder can call later
    public IReadOnlyList<Vector2Int> GetNeighbors(Vector2Int cell)
    {
        if (_connections.TryGetValue(cell, out var list)) return list;
        return System.Array.Empty<Vector2Int>();
    }

    private int CountUndirectedEdges()
    {
        int e = 0;
        foreach (var kv in _connections) e += kv.Value.Count;
        return e / 2; // each edge counted twice (A->B, B->A)
    }

    // Block if overlapping a TilemapCollider2D or CompositeCollider2D (ignore triggers)
    private bool CellIsFree(Vector2Int g)
    {
        Vector2 center = (Vector2)CellToWorld(g) + _agentOffset;
        Vector2 size = _agentSize + Vector2.one * (_clearancePad * 2f);

        var hits = Physics2D.OverlapCapsuleAll(center, size, _agentDir, 0f);
        foreach (var c in hits)
        {
            if (c == null || c.isTrigger) continue;
            if (c is CompositeCollider2D || c is TilemapCollider2D || c.CompareTag("BarrierTag"))
                return false;
        }
        return true;
    }

    private bool EdgeIsFree(Vector2Int a, Vector2Int b)
    {
        Vector2 start = (Vector2)CellToWorld(a) + _agentOffset;
        Vector2 end = (Vector2)CellToWorld(b) + _agentOffset;
        Vector2 dir = end - start;
        float dist = dir.magnitude;
        if (dist <= 1e-5f) return true;

        Vector2 size = _agentSize + Vector2.one * (_clearancePad * 2f);
        var hits = Physics2D.CapsuleCastAll(start, size, _agentDir, 0f, dir.normalized, dist);
        foreach (var h in hits)
        {
            var c = h.collider;
            if (c == null || c.isTrigger) continue;
            if (c is CompositeCollider2D || c is TilemapCollider2D || c.CompareTag("BarrierTag"))
                return false;
        }
        return true;
    }

    public bool TryGetClosestCell(Vector3 worldPos, out Vector2Int closestCell)
    {
        closestCell = default;

        // If we don't have any connections yet, we can't find a nearest node.
        if (_connections == null || _connections.Count == 0)
        {
            return false;
        }

        float bestSqr = float.PositiveInfinity;

        // _connections.Keys corresponds to all accepted nodes
        foreach (var kv in _connections)
        {
            Vector3 cellWorldPos = CellToWorld(kv.Key);
            float sqr = (cellWorldPos - worldPos).sqrMagnitude;

            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                closestCell = kv.Key;
            }
        }

        return bestSqr < float.PositiveInfinity;
    }



    // --- Grid helpers ---
    public Vector2Int WorldToCell(Vector2 world)
    {
        Vector2 local = world - (Vector2)transform.position;
        float inv = 1f / _cellSize;
        return new Vector2Int(
            Mathf.RoundToInt(local.x * inv),
            Mathf.RoundToInt(local.y * inv)
        );
    }

    public Vector2 CellToWorld(Vector2Int cell)
    {
        return (Vector2)transform.position + new Vector2(cell.x * _cellSize, cell.y * _cellSize);
    }

    // --- Gizmos (toggleable) ---
    private void OnDrawGizmos()
    {
        if (!_drawGizmos) return;

        if (_drawOrigin)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, new Vector2(_cellSize, _cellSize));
        }

        if (_drawAccepted)
        {
            Gizmos.color = Color.green;
            foreach (var g in _accepted)
                Gizmos.DrawWireCube(CellToWorld(g), new Vector2(_cellSize, _cellSize));
        }

        if (_drawRejected)
        {
            Gizmos.color = Color.red;
            foreach (var g in _rejected)
            {
                var p = CellToWorld(g);
                float h = _cellSize * 0.5f;
                Gizmos.DrawLine(p + new Vector2(-h, -h), p + new Vector2(h, h));
                Gizmos.DrawLine(p + new Vector2(-h, h), p + new Vector2(h, -h));
            }
        }
    }
}
