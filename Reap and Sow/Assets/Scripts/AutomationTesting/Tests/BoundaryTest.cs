using System.Collections;
using UnityEngine ;
using UnityEngine.SceneManagement;

/// <summary>
/// Boundary test that nudges the player around using an InputSimulator,
/// detects when they escape camera bounds, get stuck, or hit a timeout,
/// and logs everything in a clean, structured format via GameLogger.
/// </summary>
public class BoundaryTest : AbstractTest
{
    //Override test values
    //  testtype to have correct name
    //  godMode to prevent accidental deaths
    //  disable enemies, to prevent accidental deaths
    public override string TestType => "Boundary";
    protected override bool PlayerGodMode => true;          // If true, set "god mode" flag on the player
    protected override string[] TagsToDisable => new[] { enemyTag }; // Default excludes EnemyTag so existing enemies can remain
    
    //When trying to access LogFileName use my newly created logFilename instead which is assignable
    //  This allows test to set log file name without having to change the abstract class to have setter and getter
    protected override string LogFileName => logFileName;

    private string logFileName = "Boundary Test.csv";


    // ---- Roaming defaults (tweak as needed) ----
    private static readonly Vector2 initialDirection = new Vector2(1f, 1f);
    private const float initialJitterDegrees = 0f;      // adds a bit of angle randomness to initialDirection
    private const float roamMinDistance = 0.15f;        // distance needed to count as "progress"
    private const float roamDuration = 0.75f;           // how long we can go without progress before trying a sweep

    // ---- Random sweep (when stuck) ----
    private const int minDirections = 20;               // minimum directions to try when sweeping
    private const int maxRandomAttempts = 40;           // cap/target for total attempts
    private const float attemptDuration = 0.20f;        // time to push in a single direction
    private const float minDistancePerAttempt = 0.10f;  // distance that counts as "freed"

    // ---- Boundary leniency (reduces false "escaped") ----
    private const float boundaryLeniency = 1.02f;       // expand view bounds by 2%


    private float minX, maxX, minY, maxY;   // expanded camera bounds
    private Vector2 roamDir;                // current move direction
    private Vector2 lastPos;                // last position used to check progress
    private float roamStartTime;            // time we last reset "progress" timer
    private bool sweepFreed;                // did the random sweep free us?

    //Store reference to Coroutines
    private Coroutine mainCoroutine; //stop second
    private Coroutine TestCoroutine; //stop first
     

    #region Overrides
    #region Unused Overrides
    //Unsubscribe from any subscribed events
    protected override void UnsubscribeAllEvents() { }

    //Handles any event subscriptions
    protected override void SubscribeAllEvents() { }
    #endregion


    /// <summary>
    /// Setup initial gamestate if needed
    /// </summary>
    protected override void Setup()
    {
        // Build the logfile name with the active scene
        {
            string sceneName = SceneManager.GetActiveScene().name;
            logFileName = $"{sceneName}_BoundaryTest.csv";
        }

        // Build expanded camera bounds
        var b = GetCameraBounds(Camera.main);
        float cx = (b.minX + b.maxX) * 0.5f;
        float cy = (b.minY + b.maxY) * 0.5f;
        float halfW = (b.maxX - b.minX) * 0.5f * boundaryLeniency;
        float halfH = (b.maxY - b.minY) * 0.5f * boundaryLeniency;

        minX = cx - halfW; maxX = cx + halfW;
        minY = cy - halfH; maxY = cy + halfH;
    }
    protected override IEnumerator StopTest()
    {
        // Stop it safely if it's running
        if (TestCoroutine != null)
        {
            StopCoroutine(TestCoroutine);
            TestCoroutine = null;
        }
        if (mainCoroutine != null)
        {
            StopCoroutine(mainCoroutine);
            mainCoroutine = null;
        }

        //Write results
        //Edit: Standardize this so all stopping is done the same

        logger.Write(LogFileName);
        yield return null;
    }
    #endregion


    /// <summary>
    /// Waits until Player, Camera, and Simulator exist; then configures and runs the loop.
    /// </summary>
    protected override IEnumerator RunTest()
    {
        // Wait for scene pieces to exist
        while (player  == null) yield return null;

        // Initial roam direction (with optional jitter)
        roamDir = initialDirection.sqrMagnitude > 0.000001f ? initialDirection.normalized : Vector2.right;
        if (initialJitterDegrees > 0f)
        {
            float jitter = VectorToDegrees(roamDir) + Random.Range(-initialJitterDegrees, initialJitterDegrees);
            roamDir = DegreesToVector(jitter);
        }

        lastPos = player.transform.position;
        roamStartTime = Time.time;

        // Session header: easy to scan in console, rich in CSV
        //Edit: Start sending loggers as a String List which each value being a cell in a row
        logger.SessionStart(
            "BoundaryTest",
            $"minDir={minDirections} maxAttempts={maxRandomAttempts} attemptDur={attemptDuration:F2}s " +
            $"minDistPerAttempt={minDistancePerAttempt:F2} roamMinDist={roamMinDistance:F2} roamDur={roamDuration:F2}s " +
            $"leniency={boundaryLeniency:P0} bounds=({minX:F2},{minY:F2})�({maxX:F2},{maxY:F2})"
        );

        // Run the test loop
        yield return StartCoroutine(TestLoop());

        testRunning = false;
    }

    /// <summary>
    /// Main loop: move player, monitor progress, sweep if stuck, track escape/timeout.
    /// </summary>
    private IEnumerator TestLoop()
    {
        testStart = Time.time;

        while (!CheckTimeOut())
        {
            // Drive movement via InputSimulator (your EventManager listens to this)
            InputSimulator.Move(roamDir);
            yield return null;

            Vector2 pos = player.transform.position;

            // Escape check against expanded bounds
            if (pos.x < minX || pos.x > maxX || pos.y < minY || pos.y > maxY)
            {
                logger.BoundsEscaped(pos);
                logger.Result("Fail", "escaped bounds", pos, Time.time - testStart);
                logger.Write(LogFileName);
                yield break;
            }

            // Progress check
            float moved = Vector2.Distance(pos, lastPos);
            float roamTime = Time.time - roamStartTime;

            if (moved >= roamMinDistance)
            {
                // Only when threshold crossed (low spam)
                logger.Info("Roam", $"progress >= {roamMinDistance:F2}", pos, Time.time - testStart);
                lastPos = pos;
                roamStartTime = Time.time;
            }
            else if (roamTime >= roamDuration)
            {
                // Try to free ourselves with a random sweep
                sweepFreed = false;
                yield return StartCoroutine(RandomSweep(testStart));

                if (!sweepFreed)
                {
                    logger.Stuck(minDirections, pos);
                    logger.Result("Fail", "stuck", pos, Time.time - testStart);
                    logger.Write(LogFileName);
                    yield break;
                }

                // Reset progress timer after being freed
                lastPos = player.transform.position;
                roamStartTime = Time.time;
            }
        }
    }

    /// <summary>
    /// When stuck, try several random directions until we move enough or give up.
    /// </summary>
    private IEnumerator RandomSweep(float testStart)
    {

        logger.Info("RandomSweep", "starting");

        // Ensure at least minDirections attempts; keep cap semantics
        int attempts = Mathf.Clamp(maxRandomAttempts, minDirections, int.MaxValue);

        for (int i = 1; i <= attempts; i++)
        {
            //Check for timeout
            if (CheckTimeOut())
            {
                yield break;
            }
            
            float angle = Random.Range(0f, 360f);
            roamDir = DegreesToVector(angle);

            Vector2 startPos = player.transform.position;
            float startTime = Time.time;

            // Push in this direction for attemptDuration seconds
            while (Time.time - startTime < attemptDuration)
            {
                InputSimulator.Move(roamDir);
                yield return null;

                Vector2 p = player.transform.position;
                if (p.x < minX || p.x > maxX || p.y < minY || p.y > maxY)
                {
                    // escaped during sweep = hard fail
                    logger.BoundsEscaped(p);
                    logger.Result("Fail", "escaped during sweep", p, Time.time - testStart);
                    logger.Write(LogFileName);
                    sweepFreed = false;
                    yield break;
                }
            }

            // Check how far we actually moved
            Vector2 endPos = player.transform.position;
            float dist = Vector2.Distance(endPos, startPos);

            logger.SweepTry(i, attempts, angle, dist, endPos);

            if (dist >= minDistancePerAttempt)
            {
                logger.SweepFreed(angle, dist, endPos);
                sweepFreed = true;
                yield break;
            }
        }

        // not freed this round
        sweepFreed = false;
        logger.Info("RandomSweep", "completed without freeing");
    }

    // ---- Helpers ----

    /// <summary>
    /// Orthographic camera bounds in world units.
    /// </summary>
    private static (float minX, float maxX, float minY, float maxY) GetCameraBounds(Camera cam)
    {
        float height = 2f * cam.orthographicSize;
        float width = height * cam.aspect;
        Vector2 c = cam.transform.position;

        return (
            c.x - width * 0.5f,
            c.x + width * 0.5f,
            c.y - height * 0.5f,
            c.y + height * 0.5f
        );
    }

    /// <summary>Angle (deg) -> normalized 2D vector.</summary>
    private static Vector2 DegreesToVector(float deg)
    {
        float rad = deg * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
    }

    /// <summary>Vector -> angle (0�360 deg).</summary>
    private static float VectorToDegrees(Vector2 v)
    {
        if (v.sqrMagnitude < 1e-9f) return 0f;
        v.Normalize();
        return (Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg + 360f) % 360f;
    }
}
