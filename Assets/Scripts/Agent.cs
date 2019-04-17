using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AgentActionType { Rest, North, East, South, West };

public delegate void MoverActionEvent(ushort agentID, AgentType agentType, AgentActionType actionType);


public class Agent : MonoBehaviour
{
    public event MoverActionEvent OnAction;

    protected void Emit(AgentActionType action)
    {
        OnAction?.Invoke(AgentID, TypeOfAgent, action);
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
