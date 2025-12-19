public enum EventType
{
    //Input Manager events
    #region Input Events

    Move,
    NumInput,
    Attack,
    Fire,
    DeviceLost,
    DeviceRegained,
    ControlsChanged,
    NextItem,
    PrevItem,
    Pause,
    Resume,
    Click,
    RightClick,
    MiddleClick,
    Plant,

    #endregion

    //Custom Events
    #region Primary Events

    StartGameTimer,
    GameTimerDone,
    PauseTime,
    ResumeTime,
    TutorialToggle,
    SaveGame,
    ClearSave,

    #endregion
    #region Scene Events
    ReloadScene,
    ChangeScene,
    EnterScene,
    LeaveScene,
    LeaveFarm,
    EnterFarm,

    #endregion
    #region Mob Events

    EnemyDeath,
    EnemyDestroyed,
    BossDeath,


    #endregion
    #region Player Events

    GetPlayer,
    PlayerDeath,
    PlayerStart,
    ResetPlayer,
    PlayerAddItem,
    PlayerDamaged,
    ItemUsed,
    ItemAdded,
    InventoryChanged,

    #endregion
    #region Tester Events
    ClearDeathTimer,
    RoomTestResults,
    DisableSceneChanger,
    AutoTestStart,
    AutoTestStop,

    #endregion

    //Categorize these please
    ShieldOn,
    ShieldOff,
    AttackOn,
    AttackOff,
    GetAStarPrefab,
    loadsave
}
