using UnityEngine;
using UnityEngine.PlayerLoop;

public class Boss3Turret : MonoBehaviour
{
    [SerializeField] private bool isLeftTurret;
    private BossPhase centerBodyPhase;
    private Vector2 direction;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float targetBuffer;
    private Vector3 moveVector;
    [SerializeField] private NodesEnum startNode;

    [SerializeField] private GameObject fireWall;

    private NodesEnum currentNode;
    private NodesEnum nextNode;
    private bool isMoving;
    private Vector2 target;
    private Vector2 startPos;
    EventManager eventManager;
    private Animator animator;
    private AnimatorStateInfo stateInfo;

    private GameObject phase1;
    private GameObject phase2;
    private GameObject phase3;

    public bool isShooting { get; set; }

    void Start()
    {
        animator = GetComponent<Animator>();
        
        fireWall.SetActive(false);
        // Grabs the current phase from the center body
        centerBodyPhase = transform.parent.GetComponent<BossPhaseController>().getCurrentPhase;

        // Gets the position to start at
        startPos = transform.parent.GetComponent<TravelNodes>().GetNodeArray[(int)startNode];
        
        // Sets the position to start pos
        transform.position = new Vector3(startPos.x, startPos.y, transform.position.z);

        // Logs which node we're currently at
        currentNode = startNode;
        target = transform.parent.GetComponent<TravelNodes>().GetNodeArray[(int)currentNode];

        // Get all phase children and store them accordingly
        Transform[] childObjects = GetComponentsInChildren<Transform>();
        foreach (Transform child in childObjects)
        {
            if (child.gameObject.name == "Phase1") phase1 = child.gameObject;
            else if (child.gameObject.name == "Phase2") phase2 = child.gameObject;
            else if (child.gameObject.name == "Phase3") phase3 = child.gameObject;
        }
    }

    public GameObject getFireWall
    {
        get { return fireWall; }
    }

    public bool IsMoving
    {
        get { return isMoving; }
    }

    public NodesEnum GetCurrentNode
    {
        get { return currentNode; }
    }

    public NodesEnum GetNextNode
    {
        get { return nextNode; }
    }
    public void SetNextNode(NodesEnum newNode)
    {
        nextNode = newNode;
    }

    void Update()
    {
        UpdatePhaseState();
        UpdateAnimation();
    }
    private void UpdatePhaseState()
    {
        GetCenterBossPhase();
        if (isLeftTurret)
        {
            // LEFT TURRET

            switch (centerBodyPhase)
            {
                case BossPhase.None:
                    // Go back to start
                    MoveToTarget(startPos);

                    // Set phases to off
                    phase1.SetActive(false);
                    phase2.SetActive(false);
                    phase3.SetActive(false);
                    
                    break;

                case BossPhase.Phase1:
                    // Shotgun shoot thorns

                    // Set phase 1 to on
                    phase1.SetActive(true);
                    phase2.SetActive(false);
                    phase3.SetActive(false);

                    // Move around game field
                    MoveToTarget(target);

                    break;

                case BossPhase.Phase2:
                    // Burrowing Attack
                    // Alternates with fire shield
                    // Set phase 2 to on
                    phase1.SetActive(false);
                    phase2.SetActive(true);
                    phase3.SetActive(false);

                    // Move around game field
                    MoveToTarget(target);

                    break;

                case BossPhase.Phase3:
                    // Shotgun shoot thorns

                    // Set phase 3 to on
                    phase1.SetActive(false);
                    phase2.SetActive(false);
                    phase3.SetActive(true);

                    // Move around game field
                    MoveToTarget(target);

                    break;
            }
        }
        else
        {
            // RIGHT TURRET

            switch (centerBodyPhase)
            {
                case BossPhase.None:
                    // Go back to start
                    MoveToTarget(startPos);

                    // Set phases to off
                    phase1.SetActive(false);
                    phase2.SetActive(false);
                    phase3.SetActive(false);

                    break;

                case BossPhase.Phase1:
                    // Shoot fast pinecones

                    // Set phase 1 to on
                    phase1.SetActive(true);
                    phase2.SetActive(false);
                    phase3.SetActive(false);

                    // Move around game field
                    MoveToTarget(target);

                    break;

                case BossPhase.Phase2:
                    // Burrowing Attack
                    // Alternates with fire sheild

                    // Set phase 2 to on
                    phase1.SetActive(false);
                    phase2.SetActive(true);
                    phase3.SetActive(false);

                    // Move around game field
                    MoveToTarget(target);

                    break;

                case BossPhase.Phase3:
                    // Burrowing Attack

                    // Set phase 3 to on
                    phase1.SetActive(false);
                    phase2.SetActive(false);
                    phase3.SetActive(true);

                    // Move around game field
                    MoveToTarget(target);

                    break;
            }
        }
    }

    public void UpdateTurretTarget(Vector2 target)
    {
        this.target = target;
    }

    public void MoveToTarget(Vector2 target)
    {
        direction = target - new Vector2(transform.position.x, transform.position.y);
        moveVector = new Vector3(direction.normalized.x, direction.normalized.y, 0);
        if (transform.position == new Vector3(target.x, target.y, 0))
        {
            isMoving = false;
            SetNewCurrentNode(target);
            return;
        }
        else if (direction.magnitude < targetBuffer)
        {
            isMoving = false;
            SetNewCurrentNode(target);

            transform.position += new Vector3(direction.x / 2, direction.y / 2, 0);
            if (direction.magnitude < 0.01)
            {
                transform.position = new Vector3(target.x, target.y, 0);
                return;
            }
        }
        else if (new Vector2(transform.position.x, transform.position.y) != target)
        {
            isMoving = true;
            transform.position += moveVector * moveSpeed * Time.deltaTime;
        }
    }

    private void SetNewCurrentNode(Vector2 target)
    {
        Vector2[] nodeArray = transform.parent.GetComponent<TravelNodes>().GetNodeArray;

        for(int i = 0; i < nodeArray.Length; i++)
        {
            if(target == nodeArray[i])
            {
                currentNode = (NodesEnum)i;
            }
        }
    }

    private void GetCenterBossPhase()
    {
        centerBodyPhase = transform.parent.GetComponent<BossPhaseController>().getCurrentPhase;    
    }


    // Triggers: Shoot, Idle
    private void UpdateAnimation()
    {
        if (isShooting)
        {
            animator.SetTrigger("Shoot");
            isShooting = false;
        }
    }
}
