using UnityEngine ;

public class SceneData
{
    public string SceneName { get; set; }
    public Vector2 RespawnPosition { get; set; }

    // Constructs a scene data object with a name and a player respawn position
    public SceneData(string sceneName, Vector2 respawnPosition)
    {
        SceneName = sceneName;
        RespawnPosition = respawnPosition;
    }

    //The below methods make SceneData comparable
    #region Comparison Methods
    public override bool Equals(object obj)
    {
        if (obj is SceneData other)
            return SceneName == other.SceneName && RespawnPosition == other.RespawnPosition;
        return false;
    }
    public override int GetHashCode()
    {
        return SceneName.GetHashCode() ^ RespawnPosition.GetHashCode();
    }

    #endregion
}
