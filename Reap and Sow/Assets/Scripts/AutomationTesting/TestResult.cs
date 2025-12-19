using System.Collections.Generic;

public class TestResult
{

    public string roomName = "";
    public string status = "";
    public string resultString = "";
    public int healthStart = 0;
    public int healthEnd = 0;
    public int damageTaken = 0;
    public int healsUsed = 0;
    public int projectilesUsed = 0;
    public int meleeAttacks = 0;
    public int remainingEnemies = 0;
    public int timeMinutes = 0;
    public int timeSeconds = 0;
    public float totalTimeSeconds = 0f;

    public List<string> GetResultList()
    {
        //create a list of strings from all parameters
        List<string> results = new List<string>();
        results.Add($"{resultString}");
        results.Add($"{roomName}");
        results.Add($"{status}");
        results.Add($"{healthStart}");
        results.Add($"{healthEnd}");
        results.Add($"{healsUsed}");
        results.Add($"{projectilesUsed}");
        results.Add($"{meleeAttacks}");
        results.Add($"{remainingEnemies}");
        results.Add($"{timeMinutes}");
        results.Add($"{timeSeconds}");
        results.Add($"{totalTimeSeconds}");

        return results;
    }
    public static List<string> getColNames()
    {
        //create a list of strings from all parameters
        List<string> results = new List<string>();

        results.Add($"Result");
        results.Add($"Room");
        results.Add($"Status");
        results.Add($"Health Start");
        results.Add($"Health End");
        results.Add($"Heals Used");
        results.Add($"Projectiles Used");
        results.Add($"Melee Attacks");
        results.Add($"Remaining Enemies");
        results.Add($"Time Minutes");
        results.Add($"Time Seconds");
        results.Add($"Total Elapsed Time");

        return results;
    }
}
