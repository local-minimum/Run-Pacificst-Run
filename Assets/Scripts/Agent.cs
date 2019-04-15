using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AgentType { NONE, PLAYER, MONSTER, NPC, WALL };
public enum AgentActionType { Rest, North, East, South, West };

public delegate void MoverActionEvent(int playerID, AgentType moverType, AgentActionType actionType);


public class Agent : MonoBehaviour
{
    public event MoverActionEvent OnAction;

    protected void Emit(AgentActionType action)
    {
        OnAction?.Invoke(MoverID, TypeOfMover, action);
    }

    public AgentType TypeOfMover { get; protected set; }
    public int MoverID { get; protected set; }

    public void Move(Vector2 to)
    {
        transform.position = to;
    }
}
