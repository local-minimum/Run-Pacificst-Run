using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public enum FloorType { BASIC, MOVABLE, IMOVABLE };

[Serializable]
struct Imovable
{
    public int x;
    public int y;
    public Imovable(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}

[Serializable]
public struct Movable
{
    public int x;
    public int y;
    int id;
    public int Id { get => id; }
    public MoverType actor;
    public int actionX;
    public int actionY;
    public GameObject who;

    public Movable(int x, int y, MoverType actor, int id, GameObject who)
    {
        this.x = x;
        this.y = y;
        this.id = id;
        this.actor = actor;
        actionX = 0;
        actionY = 0;
        this.who = who;
    }

    public Movable(int x, int y, MoverType actor, int id, GameObject who, int actionX, int actionY)
    {
        this.x = x;
        this.y = y;
        this.id = id;
        this.actor = actor;
        this.actionX = actionX;
        this.actionY = actionY;
        this.who = who;
    }

    public Movable Evolve(MoverActionType actionType)
    {
        int actionX = 0;
        int actionY = 0;
        switch (actionType)
        {
            case MoverActionType.North:
                actionY = 1;
                break;
            case MoverActionType.East:
                actionX = 1;
                break;
            case MoverActionType.South:
                actionY = -1;
                break;
            case MoverActionType.West:
                actionX = -1;
                break;
        }
        return new Movable(x, y, actor, id, who, actionX, actionY);
    }

    public Movable Evolve(int actionX, int actionY)
    {
        return new Movable(x, y, actor, id, who, actionX, actionY);
    }

    public Movable Reset()
    {
        return new Movable(x, y, actor, id, who, 0, 0);
    }

    public Movable Enact()
    {
        return new Movable(NextX, NextY, actor, id, who, 0, 0);
    }

    public int NextX
    {
        get => x + actionX;
    }

    public int NextY
    {
        get => y + actionY;
    }

    public bool WantsToMove
    {
        get => actionX != 0 || actionY != 0;
    }
}


public class Level : Singleton<Level>
{
    static int nextMovableId = 0;

    [SerializeField]
    List<Movable> movables = new List<Movable>();
    [SerializeField]
    List<Imovable> imovables = new List<Imovable>();

    public int RegisterMover(Mover mover)
    {
        int id = nextMovableId;
        nextMovableId++;
        Movable player = new Movable(0, 0, mover.TypeOfMover, id, mover.gameObject);
        movables.Add(player);
        mover.OnAction += HandleMoverAction;
        return id;
    }

    public void UnRegisterMover(Mover mover)
    {
        movables.RemoveAll(m => m.who == mover.gameObject);
        mover.OnAction -= HandleMoverAction;
    }

    private void HandleMoverAction(int moverId, MoverType moverType, MoverActionType actionType)
    {
        for (int i = 0, l = movables.Count; i < l; i++)
        {
            Movable m = movables[i];
            if (m.actor == moverType && m.Id == moverId)
            {
                movables[i] = m.Evolve(actionType);
                return;
            }
        }
        Debug.LogWarning(string.Format("Could not find player id {0}", moverId));
    }

    private void OnEnable()
    {
        GameClock.Instance.OnTick += Instance_OnTick;
    }

    private void OnDisable()
    {
        if (GameClock.Instance)
            GameClock.Instance.OnTick -= Instance_OnTick;
    }

    bool shouldMove;
    bool shouldMoveEveryone;

    private void Instance_OnTick(int tick, int partialTick, float tickDuration, bool everyone)
    {
        shouldMove = true;
        shouldMoveEveryone = everyone;
    }

    private void Update()
    {
        PlaceFloors();
        if (shouldMove) MovePlayers();
        if (shouldMoveEveryone) MoveOthers();
    }

    void MovePlayers()
    {
        int moves = 0;
        for (int i = 0, l = movables.Count(); i < l; i += 1)
        {
            Movable m = movables[i];
            if (m.actor == MoverType.PLAYER && m.WantsToMove)
            {
                if (Enact(m))
                {
                    moves += 1;
                    movables[i] = m.Enact();
                } else
                {
                    movables[i] = m.Reset();
                }
            }
        }
        //Debug.Log(string.Format("{0} players moved", moves));
        shouldMove = false;
    }

    void MoveOthers()
    {
        shouldMoveEveryone = false;
    }

    bool Enact(Movable m)
    {
        int nextX = m.NextX;
        int nextY = m.NextY;
        ResolveConflict(nextX, nextY, m.actor, m.who, m.actionX, m.actionY);
        bool occupied = IsOccupied(nextX, nextY, m.actor);
        if (!occupied)
        {
            m.who.SendMessage("Move", GetPositionAt(nextX, nextY), SendMessageOptions.RequireReceiver);
            return true;
        }
        return false;
    }

    void ResolveConflict(int x, int y, MoverType actor, GameObject who, int xDir, int yDir)
    {
        if (actor != MoverType.PLAYER) return;
        for (int i=0, l=movables.Count(); i<l; i++)
        {
            Movable m = movables[i];
            if (m.x == x && m.y == y && m.actor == MoverType.WALL)
            {
                int nextX = x + xDir;
                int nextY = y + yDir;
                if (!IsOccupied(nextX, nextY, MoverType.WALL))
                {
                    movables[i] = m.Evolve(xDir, yDir).Enact();
                }
                return;
            }
        }
    }

    bool IsOccupied(int x, int y, MoverType actor)
    {
        if (imovables.Any(e => e.x == x && e.y == y)) return true;
        if (movables.Any(e => e.x == x && e.y == y)) return true;

        //TODO: Block if actor is wall and object on pos

        return false;
    }
    
    [SerializeField]
    float gridSize = 1f;
    public float GridSize { get => gridSize; }

    public Movable GetMovableById(int id)
    {
        return movables.FirstOrDefault(m => m.Id == id);
    }

    Vector2 GetPositionAt(int x, int y)
    {
        return new Vector2(gridSize * x, gridSize * y);
    }

    public Movable GetPlayerClosestTo(int x, int y, int maxDist=-1)
    {
        return movables
            .Where(m => m.actor == MoverType.PLAYER)
            .Select(m => new
            {
                dist = Mathf.Abs(m.x - x) + Mathf.Abs(m.y - y),
                movable = m,
            })
            .Where(o => o.dist < maxDist || maxDist < 0)
            .OrderBy(o => o.dist)
            .Select(o => o.movable)
            .FirstOrDefault();
    }

    [SerializeField]
    Sprite floorBasic;
    [SerializeField]
    Sprite wallImovable;
    [SerializeField]
    Sprite wallMovable;

    List<Transform> floors = new List<Transform>();
    int floorIdx;

    Transform GetFloorTransform(FloorType floorType)
    {
        Transform floor;

        if (floorIdx < floors.Count())
        {
            floor = floors[floorIdx];
        }
        else
        {
            GameObject go = new GameObject("Floor");
            go.transform.SetParent(transform);
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();            
            floor = go.transform;
            floors.Add(floor);
        }
        if (floorType == FloorType.BASIC)
        {
            floor.GetComponent<SpriteRenderer>().sprite = floorBasic;
        } else if (floorType == FloorType.IMOVABLE)
        {
            floor.GetComponent<SpriteRenderer>().sprite = wallImovable;
        } else if (floorType == FloorType.MOVABLE)
        {
            floor.GetComponent<SpriteRenderer>().sprite = wallMovable;
        }
        floorIdx++;
        floor.gameObject.SetActive(true);
        return floor;
    }

    void DeactivateLostFloors()
    {
        for (int i=floorIdx, l=floors.Count(); i<l; i++)
        {
            floors[i].gameObject.SetActive(false);
        }
    }

    void PlaceFloors()
    {
        floorIdx = 0;
        Rect camRect = GameCamera.Instance.GetViewRect();        
        int left = Mathf.FloorToInt(camRect.xMin);
        int right = Mathf.CeilToInt(camRect.xMax);
        int bottom = Mathf.FloorToInt(camRect.yMin);
        int top = Mathf.CeilToInt(camRect.yMax);
        FloorType floorType;

        for (int x=left; x<=right; x++)
        {
            for (int y=bottom; y<=top; y++)
            {
                if (imovables.Any(e => e.x == x && e.y == y))
                {
                    floorType = FloorType.IMOVABLE;
                } else if (movables.Any(e => e.x == x && e.y == y && e.actor == MoverType.WALL))
                {
                    floorType = FloorType.MOVABLE;
                } else
                {
                    floorType = FloorType.BASIC;
                }
                Transform t = GetFloorTransform(floorType);
                t.position = GetPositionAt(x, y);
            }
        }
    }
}
