using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class BossDeathscreen : MonoBehaviour
{
    [SerializeField] private GameObject BossPanel;
    [SerializeField] private GameObject DemoPanel;
    [SerializeField] private bool IsDemo = false;
    private Animator transitionAnimator;
    private EventManager eventManager;
    [SerializeField] private float timeForAnimation = 3.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        eventManager = GameController.instance.EventManager;
        transitionAnimator = GameObject.FindGameObjectWithTag("FadeUI")?.GetComponent<Animator>();

        if (transitionAnimator == null)
            Debug.LogWarning("Fade Animator not found!");

        eventManager.Subscribe(EventType.BossDeath, OnBossDeathHandler);
    }

    void OnDestroy()
    {
        if (eventManager != null)
            eventManager.Unsubscribe(EventType.BossDeath, OnBossDeathHandler);
    }

    private void OnBossDeathHandler(object obj)
    {
        StartCoroutine(BossDeathSequence());
    }


    /// <summary>
    /// Transitions the screen to black then plays the screen
    /// </summary>
    /// <returns></returns>
    private IEnumerator BossDeathSequence()
    {
        transitionAnimator.SetTrigger("End");

        // Wait for fade out animation to finish
        yield return new WaitUntil(() =>
            transitionAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f
            && !transitionAnimator.IsInTransition(0)
        );

        BossPanel.SetActive(true);

        // Let the cutscene panel show fully
        yield return new WaitForSecondsRealtime(timeForAnimation);

        BossPanel.SetActive(false);

        transitionAnimator.SetTrigger("Start");

        // Wait for fade back in
        yield return new WaitUntil(() =>
            transitionAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f
            && !transitionAnimator.IsInTransition(0)
        );
        if (IsDemo)
        {
            transitionAnimator.SetTrigger("End");

            // Wait for fade out animation to finish
            yield return new WaitUntil(() =>
                transitionAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f
                && !transitionAnimator.IsInTransition(0)
            );

            DemoPanel.SetActive(true);

            // Let the cutscene panel show fully
            yield return new WaitForSecondsRealtime(7.0f);

            DemoPanel.SetActive(false);

            foreach (GameObject o in UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                Destroy(o);

            }
            SceneManager.LoadScene("Mainmenu");

        }

        Destroy(gameObject);
    }



}
