using UnityEngine;

public class TravelNodes : MonoBehaviour
{
    // Node Variables
    [SerializeField] private Vector2 node1;
    [SerializeField] private Vector2 node2;
    [SerializeField] private Vector2 node3;
    [SerializeField] private Vector2 node4;

    private Vector2[] nodeArray = new Vector2[4];

    // Turret Variables
    // Turret 1

    [SerializeField] private GameObject t1;
    private Boss3Turret t1Info;

    // Turret 2

    [SerializeField] private GameObject t2;
    private Boss3Turret t2Info;

    // Timer Variable
    [SerializeField] private Vector2 changeNodeDurration;

    [SerializeField] private float fireTimerDuration;

    private Timer t1Timer;
    private Timer t2Timer;
    private Timer tFireTimer;
    private bool canChangeFire = true;

    [Tooltip("Sets the chance a turret will stay at its current node")]
    [SerializeField, Range(0f, 1f)] private float chanceOfStayingInPlace;

    public Vector2[] GetNodeArray
    {
        get { return nodeArray; }
    }

    private void Awake()
    {
        // Populate array with nodes
        nodeArray[0] = node1;
        nodeArray[1] = node2;
        nodeArray[2] = node3;
        nodeArray[3] = node4;
    }

    private void Start()
    {
        t1Info = t1.GetComponent<Boss3Turret>();
        t2Info = t2.GetComponent<Boss3Turret>();

        // Set up timers
        t1Timer = gameObject.AddComponent<Timer>();
        t1Timer.Duration = Random.Range(changeNodeDurration.x, changeNodeDurration.y);
        t1Timer.AddTimerFinishedListener(UpdateT1);

        t2Timer = gameObject.AddComponent<Timer>();
        t2Timer.Duration = Random.Range(changeNodeDurration.x, changeNodeDurration.y);
        t2Timer.AddTimerFinishedListener(UpdateT2);

        tFireTimer = gameObject.AddComponent<Timer>();
        tFireTimer.Duration = fireTimerDuration;
    }
    private void Update()
    {
        // Run Timers only when current phase is not None
        if (gameObject.GetComponent<BossPhaseController>().getCurrentPhase != BossPhase.None)
        {
            // Only run timers if they are not already running
            if(!t1Timer.Running) t1Timer.Run();
            if(!t2Timer.Running) t2Timer.Run();
        }

        SetFireWalls();
    }

    private void UpdateT1()
    {
        if (!t1Info.IsMoving)
        {
            NodesEnum temp = FindNextNode(t1Info.GetCurrentNode, t2Info.GetCurrentNode, t2Info.GetNextNode);
            t1Info.SetNextNode(temp);
            if (temp != NodesEnum.None)
            {
                // We have a node to travel to
                t1Info.UpdateTurretTarget(NodeToVector(temp));
            }
            else return;
            t1Timer.Run();
        }
    }

    private void UpdateT2()
    {
        if (!t2Info.IsMoving)
        {
            NodesEnum temp = FindNextNode(t2Info.GetCurrentNode, t1Info.GetCurrentNode, t1Info.GetNextNode);
            t2Info.SetNextNode(temp);
            if (temp != NodesEnum.None)
            {
                // We have a node to travel to
                t2Info.UpdateTurretTarget(NodeToVector(temp));
            }
            else return;
            t2Timer.Run();
        }
    }

    /// <summary>
    /// Returns the next node to travel to based on current node and the other turrets current and next node
    /// </summary>
    /// <param name="t1CurrentNode"></param>
    /// <param name="t2CurrentNode"></param>
    /// <param name="t2NextNode"></param>
    /// <returns></returns>
    private NodesEnum FindNextNode(NodesEnum t1CurrentNode, NodesEnum t2CurrentNode, NodesEnum t2NextNode)
    {
        switch (t1CurrentNode)
        {
            case NodesEnum.Node1:

                if(Random.Range(0f,1f) <= chanceOfStayingInPlace)
                {
                    return t1CurrentNode;
                }
                else if(t2CurrentNode == NodesEnum.Node2 || t2NextNode == NodesEnum.Node2)
                {
                    // Go to node 3
                    return NodesEnum.Node3;
                }
                else if(t2CurrentNode == NodesEnum.Node3 || t2NextNode == NodesEnum.Node3)
                {
                    // Go to node 2
                    return NodesEnum.Node2;
                }
                else
                {
                    // Pick random node to go to
                    if (Random.Range(0f, 1f) > 0.5f) return NodesEnum.Node3;
                    else return NodesEnum.Node2;
                }

            case NodesEnum.Node2:

                if (Random.Range(0f, 1f) <= chanceOfStayingInPlace)
                {
                    return t1CurrentNode;
                }
                else if (t2CurrentNode == NodesEnum.Node1 || t2NextNode == NodesEnum.Node1)
                {
                    // Go to node 4
                    return NodesEnum.Node4;
                }
                else if (t2CurrentNode == NodesEnum.Node4 || t2NextNode == NodesEnum.Node4)
                {
                    // Go to node 1
                    return NodesEnum.Node1;
                }
                else
                {
                    // Pick random node to go to
                    if (Random.Range(0f, 1f) > 0.5f) return NodesEnum.Node4;
                    else return NodesEnum.Node1;
                }

            case NodesEnum.Node3:

                if (Random.Range(0f, 1f) <= chanceOfStayingInPlace)
                {
                    return t1CurrentNode;
                }
                else if (t2CurrentNode == NodesEnum.Node1 || t2NextNode == NodesEnum.Node1)
                {
                    // Go to node 4
                    return NodesEnum.Node4;
                }
                else if (t2CurrentNode == NodesEnum.Node4 || t2NextNode == NodesEnum.Node4)
                {
                    // Go to node 1
                    return NodesEnum.Node1;
                }
                else
                {
                    // Pick random node to go to
                    if (Random.Range(0f, 1f) > 0.5f) return NodesEnum.Node4;
                    else return NodesEnum.Node1;
                }

            case NodesEnum.Node4:

                if (Random.Range(0f, 1f) <= chanceOfStayingInPlace)
                {
                    return t1CurrentNode;
                }
                else if (t2CurrentNode == NodesEnum.Node2 || t2NextNode == NodesEnum.Node2)
                {
                    // Go to node 3
                    return NodesEnum.Node3;
                }
                else if (t2CurrentNode == NodesEnum.Node3 || t2NextNode == NodesEnum.Node3)
                {
                    // Go to node 2
                    return NodesEnum.Node2;
                }
                else
                {
                    // Pick random node to go to
                    if (Random.Range(0f, 1f) > 0.5f) return NodesEnum.Node3;
                    else return NodesEnum.Node2;
                }
        }
        return NodesEnum.None;
    }

    private Vector2 NodeToVector(NodesEnum node)
    {
        if (node != NodesEnum.None) return nodeArray[(int)node];
        else return Vector2.zero;
    }

    private void OnDrawGizmos()
    {
        // Draw nodes only if they've been assigned
        Gizmos.color = Color.green;

        // Draw small spheres at each node position
        Gizmos.DrawWireSphere(node1, 0.1f);
        Gizmos.DrawWireSphere(node2, 0.1f);
        Gizmos.DrawWireSphere(node3, 0.1f);
        Gizmos.DrawWireSphere(node4, 0.1f);
    }

    private void SetFireWalls()
    {
        BossPhase bossPhase = GetComponent<BossPhaseController>().getCurrentPhase;
        GameObject t1Fire = t1.GetComponent<Boss3Turret>().getFireWall;
        GameObject t2Fire = t2.GetComponent<Boss3Turret>().getFireWall;

        if (bossPhase == BossPhase.Phase2)
        {
            if (!tFireTimer.Running)
            {
                if (Random.Range(0f, 1f) > 0.5f)
                {
                    // t1 flame on

                    t1Fire.SetActive(true);

                    // t2 flame off

                    t2Fire.SetActive(false);

                    tFireTimer.Run();
                }
                else
                {
                    // t2 flame on

                    t2Fire.SetActive(true);

                    // t1 flame off

                    t1Fire.SetActive(false);

                    tFireTimer.Run();
                }
            }
        }
        else
        {
            t1Fire.SetActive(false);
            t2Fire.SetActive(false);
        }
    }
}

/// <summary>
/// Enum of nodes turrets can visit
/// None means nothing happens
/// </summary>
public enum NodesEnum
{
    Node1,
    Node2,
    Node3,
    Node4,
    None
}
