using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AgentActionType { Rest, North, East, South, West };
public enum TurnType { BOUNCE, LEFT, RIGHT };

public delegate void MoverActionEvent(ushort agentID, AgentType agentType, AgentActionType actionType);


public class Agent : MonoBehaviour
{
    public static AgentActionType Turn(AgentActionType action, TurnType turn)
    {
        if (action == AgentActionType.Rest) return AgentActionType.Rest;
        switch (turn)
        {
            case TurnType.BOUNCE:
                switch (action)
                {
                    case AgentActionType.East:
                        return AgentActionType.West;
                    case AgentActionType.West:
                        return AgentActionType.East;
                    case AgentActionType.North:
                        return AgentActionType.South;
                    case AgentActionType.South:
                        return AgentActionType.North;
                }
                break;
            case TurnType.LEFT:
                switch (action)
                {
                    case AgentActionType.North:
                        return AgentActionType.West;
                    case AgentActionType.West:
                        return AgentActionType.South;
                    case AgentActionType.South:
                        return AgentActionType.East;
                    case AgentActionType.East:
                        return AgentActionType.North;
                }
                break;
            case TurnType.RIGHT:
                switch (action)
                {
                    case AgentActionType.North:
                        return AgentActionType.East;
                    case AgentActionType.East:
                        return AgentActionType.South;
                    case AgentActionType.South:
                        return AgentActionType.West;
                    case AgentActionType.West:
                        return AgentActionType.North;
                }
                break;
        }
        throw new System.Exception($"Invalid turn {turn} on aciton {action}");
    }

    public event MoverActionEvent OnAction;

    protected void Emit(AgentActionType action)
    {
        OnAction?.Invoke(AgentID, TypeOfAgent, action);
    }

    protected AgentActionType RandomHeading
    {
        get
        {
            switch (Random.Range(0, 4))
            {
                case 0:
                    return AgentActionType.East;
                case 1:
                    return AgentActionType.North;
                case 2:
                    return AgentActionType.South;
                default:
                    return AgentActionType.West;
            }
        }
    }

    public AgentType TypeOfAgent { get; protected set; }
    public ushort AgentID { get; protected set; }

    public void Move(Vector2 to)
    {
        transform.position = to;
    }

    public void Setup(AgentType agentType, ushort agentId)
    {
        TypeOfAgent = agentType;
        AgentID = agentId;
    }

}
