using System.ComponentModel;
using UnityEngine;

public class BossPhaseController : MonoBehaviour
{

    private HealthScript centerBodyHealthScript;
    private int maxHealth;
    private int currentHealth;

    [SerializeField] private BossPhase currentPhase = BossPhase.None;

    // Sets up the health requirements to move to the next phase
    [Tooltip("Sets the percetage of health remaining to move to phase 2")]
    [Range(0f, 1f)]
    [SerializeField] private float phase2HealthPercent;
    [Tooltip("Sets the percetage of health remaining to move to phase 3")]
    [Range(0f, 1f)]
    [SerializeField] private float phase3HealthPercent;

    [SerializeField] private bool autoSwitchPhase;

    [Tooltip("Use keys 1 & 2 to manually switch phases")]
    [SerializeField] private bool debug;

    [SerializeField] private GameObject phase3;
    
    void Start()
    {
        centerBodyHealthScript = GetComponent<HealthScript>();
        maxHealth = centerBodyHealthScript.GetMaxHealth;
    }

    void Update()
    {
        UpdatePhaseState();
        CyclePhases();
    }

    public BossPhase getCurrentPhase
    {
        get { return currentPhase; }
    }

    private void UpdatePhaseState()
    {
        switch (currentPhase)
        {
            case BossPhase.None:
                phase3.SetActive(false);
                break;

            case BossPhase.Phase1:
                phase3.SetActive(false);
                break;

            case BossPhase.Phase2:
                phase3.SetActive(false);
                break;

            case BossPhase.Phase3:
                // Shoots AOE poison cloud attacks at a random spot in the game field
                phase3.SetActive(true);
                break;
        }

        ChangeState();
    }

    private void ChangeState()
    {
        if (maxHealth > 0f && autoSwitchPhase)
        {
            currentHealth = centerBodyHealthScript.GetHealth;

            float healthPercent = currentHealth / (float)maxHealth;

            if(healthPercent > phase2HealthPercent)
            {
                // Start at phase 1
                currentPhase = BossPhase.Phase1;
            }
            else if (healthPercent <= phase2HealthPercent && healthPercent > phase3HealthPercent)
            {
                // Move onto phase 2
                currentPhase = BossPhase.Phase2;
            }
            else if (healthPercent <= phase3HealthPercent)
            {
                // Move onto phase 3
                currentPhase = BossPhase.Phase3;
            }
        }
    }

    private void CyclePhases()
    {
        if (debug)
        {
            // For testing
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                if (currentPhase == BossPhase.Phase1)
                {
                    currentPhase = BossPhase.None;
                }
                else if (currentPhase == BossPhase.Phase2)
                {
                    currentPhase = BossPhase.Phase1;
                }
                else if (currentPhase == BossPhase.Phase3)
                {
                    currentPhase = BossPhase.Phase2;
                }
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                if (currentPhase == BossPhase.Phase1)
                {
                    currentPhase = BossPhase.Phase2;
                }
                else if (currentPhase == BossPhase.Phase2)
                {
                    currentPhase = BossPhase.Phase3;
                }
                else if (currentPhase == BossPhase.None)
                {
                    currentPhase = BossPhase.Phase1;
                }
            }
        }
    }
}
