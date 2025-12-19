public enum NPCState
{
    Idle, //NPC Is Not Moving
    Flee, //NPC Is Moving Away from Targets
    Wander, //Wander between min and max range of target
    Chase //NPC Is Moving Towards from Targets
}
