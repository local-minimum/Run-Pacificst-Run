using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SillyEnemy : Agent
{
    private void Awake()
    {
        TypeOfAgent = AgentType.MONSTER;
    }

    private void OnEnable()
    {
        GameClock.Instance.OnTick += HandleTick;
        Level.Instance.RegisterAgent(this);
    }

    private void OnDisable()
    {
        if (GameClock.Instance)
            GameClock.Instance.OnTick -= HandleTick;
        if (Level.Instance)
            Level.Instance.UnRegisterAgent(this);
    }

    [SerializeField]
    int activationRange = 5;

    private void HandleTick(int tick, int partialTick, float tickDuration, bool everyone)
    {
        if (everyone)
        {
            Movable m = Level.Instance.GetMovableById(AgentID);
            Movable player = Level.Instance.GetPlayerClosestTo(m.x, m.y, activationRange);
            if (player.agentType == AgentType.PLAYER)
            {
                int deltaX = player.x - m.x;
                int deltaY = player.y - m.y;
                if (Mathf.Abs(deltaX) < Mathf.Abs(deltaY))
                {
                    Emit(deltaY > 0 ? AgentActionType.North : AgentActionType.South);
                } else
                {
                    Emit(deltaX > 0 ? AgentActionType.East : AgentActionType.West);
                }
            }
          
        }
    }
}
