using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MoverType { NONE, PLAYER, MONSTER, NPC, WALL };
public enum MoverActionType { Rest, North, East, South, West };

public delegate void MoverActionEvent(int playerID, MoverType moverType, MoverActionType actionType);


public class Mover : MonoBehaviour
{
    public event MoverActionEvent OnAction;

    protected void Emit(MoverActionType action)
    {
        OnAction?.Invoke(MoverID, TypeOfMover, action);
    }

    public MoverType TypeOfMover { get; protected set; }
    public int MoverID { get; protected set; }

    public void Move(Vector2 to)
    {
        transform.position = to;
    }
}
