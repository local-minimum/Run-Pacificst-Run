using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerActionType { Rest, North, East, South, West };

public delegate void PlayerAction(int playerID, PlayerActionType actionType);

public class PlayerController : MonoBehaviour
{
    private static int nextPlayerID;
    public int playerID { get; private set; }

    public event PlayerAction OnPlayerAction;

    PlayerActionType nextAction;

    private void Awake()
    {
        playerID = nextPlayerID;
        nextPlayerID += 1;
    }

    private void OnEnable()
    {
        GameClock.Instance.OnTick += PlayerController_OnTick;
        GameInput.Instance.OnInput += PlayerController_OnInput;
        Level.Instance.RegisterPlayer(this);
    }

    private void OnDisable()
    {
        if (GameClock.Instance)
            GameClock.Instance.OnTick -= PlayerController_OnTick;
        if (GameInput.Instance)
            GameInput.Instance.OnInput -= PlayerController_OnInput;
        if (Level.Instance)
            Level.Instance.UnRegisterPlayer(this);
    }

    private void PlayerController_OnTick(int n, int partialTick, float tickDuration, bool everyone)
    {
        OnPlayerAction?.Invoke(playerID, nextAction);
        nextAction = PlayerActionType.Rest;
    }

    private void PlayerController_OnInput(InputType inputType)
    {
        switch(inputType)
        {
            case InputType.North:
                nextAction = PlayerActionType.North;
                break;
            case InputType.East:
                nextAction = PlayerActionType.East;
                break;
            case InputType.South:
                nextAction = PlayerActionType.South;
                break;
            case InputType.West:
                nextAction = PlayerActionType.West;
                break;
        }
    }

    public void Move(Vector2 to)
    {
        transform.position = to;
    }
}
