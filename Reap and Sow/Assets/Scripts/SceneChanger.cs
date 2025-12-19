using System.ComponentModel;
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem.XR.Haptics;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.Linq;
using UnityEngine.PlayerLoop;

public class SceneChanger : MonoBehaviour
{

    [SerializeField] private string targetSceneName;
    [SerializeField] private float spawnX;
    [SerializeField] private float spawnY;
    [SerializeField] private bool lockWhileEnemiesPresent = false;
    [SerializeField] private bool roomIsLocked = false;
    private bool roomCompleted = false;
    [SerializeField] private BossEnum bossLock = BossEnum.None;

    private bool isLockedByBoss;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private string lockedTag = "BarrierTag";
    private string unlockedTag = "Untagged";
    private string enemyTag = "EnemyTag";
    private EventManager eventManager;

    private CapsuleCollider2D gateCollider; //for blocking movement
    SaveScript saveScript;

    private bool startLock = false;
    private AnimatorStateInfo stateInfo;

    private void Start()
{
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        gateCollider = GetComponent<CapsuleCollider2D>();

        if (bossLock != BossEnum.None) isLockedByBoss = true;
        else isLockedByBoss = false;

        // Get reference to event manager and animator
        eventManager = GameController.instance.EventManager;

        // Sub to appropriate events within event manager
        if (eventManager != null)
        {
            eventManager.Subscribe(EventType.EnemyDestroyed, OnEnemyDestroyedHandler);
            eventManager.Subscribe(EventType.DisableSceneChanger, OnDisableSceneHandler);
        }
        saveScript = GameObject.FindGameObjectWithTag("GameController")?.GetComponent<SaveScript>();

        if (saveScript == null)
            Debug.LogWarning("Save script not found!");

        //Delay one frame to avoid race conditions
        StartCoroutine(DelayStart());
    }

    private void Update()
    {
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("Unlock"))
        {
            if(stateInfo.normalizedTime > 1)
            {
                spriteRenderer.enabled = false;
                return;
            }
        }
    }

    private IEnumerator DelayStart()
    {
        yield return null;

        UpdateLock();
    }

    //Checks if the doors should be locked
    private void UpdateLock()
    {
        // Recheck lock condition
        if (isBossLocked() && isLockedByBoss)
        {
            StartLocked();
        }
        else if (AreEnemiesInRoom() && lockWhileEnemiesPresent)
        {
            if (!startLock)
            {
                Lock();
                startLock = true;
            }
        }
        else
        {
            lockWhileEnemiesPresent = false;
            if(roomIsLocked)
            {
                Unlock();
            }
            else
            {
                roomIsLocked = false;
                spriteRenderer.enabled = false;
                if (gateCollider != null)
                {
                    gateCollider.enabled = false;
                }
            }
        }
    }

    private void StartLocked()
    {
        roomCompleted = false;
        roomIsLocked = true;
        gameObject.tag = lockedTag;

        animator.speed = 0f;
        animator.SetTrigger("vbTrigger");

        if (gateCollider != null)
        {
            gateCollider.enabled = true;
        }
    }

    //Locks the room and changes display to show passage is blocked
    private void Lock()
    {
        roomCompleted = false;
        roomIsLocked = true;
        gameObject.tag = lockedTag;

        animator.speed = 1f;
        animator.SetTrigger("vbStartTrigger");

        if (gateCollider != null)
        {
            gateCollider.enabled = true;
        }
    }

    //UnLocks the room and changes display to show passage is no longer blocked
    private void Unlock()
    {
        if (AreEnemiesInRoom() == false)
        {
            roomCompleted = true;
        }

        roomIsLocked = false;
        gameObject.tag = unlockedTag;

        animator.speed = 1f;
        animator.SetTrigger("vbTrigger");

        if (gateCollider != null)
        {
            gateCollider.enabled = false;
        }
    }

    public bool getroomlocked()
    {
        return roomCompleted;
    }

    // Returns true or false if any enemies are in the room
    private bool AreEnemiesInRoom()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        return enemies.Length > 0;
    }

    /// <summary>
    /// returns true if room should be locked (boss is beaten and required)
    /// </summary>
    /// <returns></returns>
    public bool isBossLocked()
    {
        if (bossLock != BossEnum.None)
        {
            return !GameController.instance.IsBossBeaten(bossLock); //true if beaten so reverse it
        }
        return false; //boss not required
    }

    // When player collides with scenechanger, change to next scene
    private void OnTriggerEnter2D(Collider2D other)
    {
        // If other is Player AND room is not locked then move to target room
        if (other.CompareTag("PlayerTag") && !roomIsLocked)
        {
            //publish sceneChange event
            eventManager.Publish(EventType.ChangeScene, GetSceneData());
        }
    }

    public SceneData GetSceneData()
    {
        return new SceneData(targetSceneName, new Vector2(spawnX, spawnY));
    }

    #region Events
    private void OnDestroy()
    {
        //unsubscribe events
        if (eventManager != null)
        {
            eventManager.Unsubscribe(EventType.DisableSceneChanger, OnDisableSceneHandler);
            eventManager.Unsubscribe(EventType.EnemyDestroyed, OnEnemyDestroyedHandler);
        }
    }

    /// <summary>
    /// This responds to eventd when an enemy dies   (Assumes that if the scenechanger is loaded then the enemy is in the same room)
    /// </summary>
    /// <param name="enemyID"></param>
    private void OnEnemyDestroyedHandler(object enemyID)
    {
        //Check lock conditions
        UpdateLock();
    }

    /// <summary>
    /// When event is called, 
    /// </summary>
    /// <param name="value"></param>
    private void OnDisableSceneHandler(object value)
    {
        //Lock scene changer and then destroy (disable in case destroy doesnt work)
        roomIsLocked = true;
        lockWhileEnemiesPresent = true;
        Destroy(gameObject);
    }

    #endregion

}
