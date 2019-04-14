using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerController : Mover
{

    MoverActionType nextAction;

    private void Awake()
    {
        TypeOfMover = MoverType.PLAYER;
    }

    private void OnEnable()
    {
        GameClock.Instance.OnTick += PlayerController_OnTick;
        GameInput.Instance.OnInput += PlayerController_OnInput;
        MoverID = Level.Instance.RegisterMover(this);
        GameCamera.Instance.RegisterPlayer(this);
    }

    private void OnDisable()
    {
        if (GameClock.Instance)
            GameClock.Instance.OnTick -= PlayerController_OnTick;
        if (GameInput.Instance)
            GameInput.Instance.OnInput -= PlayerController_OnInput;
        if (Level.Instance)
            Level.Instance.UnRegisterMover(this);
        if (GameCamera.Instance)
            GameCamera.Instance.UnregisterPlayer(this);
    }

    private void PlayerController_OnTick(int n, int partialTick, float tickDuration, bool everyone)
    {
        Emit(nextAction);
        nextAction = MoverActionType.Rest;
    }

    private void PlayerController_OnInput(InputType inputType)
    {
        switch(inputType)
        {
            case InputType.North:
                nextAction = MoverActionType.North;
                break;
            case InputType.East:
                nextAction = MoverActionType.East;
                break;
            case InputType.South:
                nextAction = MoverActionType.South;
                break;
            case InputType.West:
                nextAction = MoverActionType.West;
                break;
        }
    }

}
