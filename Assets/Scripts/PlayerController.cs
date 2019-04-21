using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerController : Agent
{
    int lockedHearts = 0;
    int maxHealth = 3;
    int health = 3;

    AgentActionType nextAction;

    private void Awake()
    {
        TypeOfAgent = AgentType.PLAYER;
    }

    private void OnEnable()
    {
        GameClock.Instance.OnTick += PlayerController_OnTick;
        GameInput.Instance.OnInput += PlayerController_OnInput;
        Level.Instance.RegisterAgent(this);
        GameCamera.Instance.RegisterPlayer(this);
    }

    private void OnDisable()
    {
        if (GameClock.Instance)
            GameClock.Instance.OnTick -= PlayerController_OnTick;
        if (GameInput.Instance)
            GameInput.Instance.OnInput -= PlayerController_OnInput;
        if (Level.Instance)
            Level.Instance.UnRegisterAgent(this);
        if (GameCamera.Instance)
            GameCamera.Instance.UnregisterPlayer(this);
    }

    private void PlayerController_OnTick(int n, int partialTick, float tickDuration, bool everyone)
    {
        Emit(nextAction);
        nextAction = AgentActionType.Rest;
    }

    private void PlayerController_OnInput(InputType inputType)
    {
        switch(inputType)
        {
            case InputType.North:
                nextAction = AgentActionType.North;
                break;
            case InputType.East:
                nextAction = AgentActionType.East;
                break;
            case InputType.South:
                nextAction = AgentActionType.South;
                break;
            case InputType.West:
                nextAction = AgentActionType.West;
                break;
        }
    }

    private void Start()
    {
        UIHealthBar.Instance.SetHealth(maxHealth, health, lockedHearts);
    }

    void Hurt()
    {
        health -= 1;
        if (health <= lockedHearts) lockedHearts = 0;
        if (health <= 0) Kill();
        UIHealthBar.Instance.SetHealth(maxHealth, health, lockedHearts);
    }

    void Kill()
    {
        Debug.Log("Dead!");
    }
}
