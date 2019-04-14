﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SillyEnemy : Mover
{
    int myMovableId;

    private void Awake()
    {
        TypeOfMover = MoverType.MONSTER;
    }

    private void OnEnable()
    {
        GameClock.Instance.OnTick += HandleTick;
        MoverID = Level.Instance.RegisterMover(this);
    }

    private void OnDisable()
    {
        if (GameClock.Instance)
            GameClock.Instance.OnTick -= HandleTick;
        if (Level.Instance)
            Level.Instance.UnRegisterMover(this);
    }

    [SerializeField]
    int activationRange = 5;

    private void HandleTick(int tick, int partialTick, float tickDuration, bool everyone)
    {
        if (everyone)
        {
            Movable m = Level.Instance.GetMovableById(myMovableId);
            if (m.Id == MoverID)
            {
                Movable player = Level.Instance.GetPlayerClosestTo(m.x, m.y, activationRange);
                if (player.actor == MoverType.PLAYER)
                {
                    int deltaX = player.x - m.x;
                    int deltaY = player.y - m.y;
                    if (Mathf.Abs(deltaX) < Mathf.Abs(deltaY))
                    {
                        Emit(deltaY > 0 ? MoverActionType.North : MoverActionType.South);
                    } else
                    {
                        Emit(deltaX > 0 ? MoverActionType.East : MoverActionType.West);
                    }
                }
            }            
        }
    }
}