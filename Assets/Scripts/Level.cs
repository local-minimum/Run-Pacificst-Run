using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public enum Actor { PLAYER, MONSTER, NPC, WALL };
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
struct Movable
{
    public int x;
    public int y;
    public int id;
    public Actor actor;
    public int actionX;
    public int actionY;
    public GameObject who;

    public Movable(int x, int y, Actor actor, int id, GameObject who)
    {
        this.x = x;
        this.y = y;
        this.id = id;
        this.actor = actor;
        actionX = 0;
        actionY = 0;
        this.who = who;
    }

    public Movable(int x, int y, Actor actor, int id, GameObject who, int actionX, int actionY)
    {
        this.x = x;
        this.y = y;
        this.id = id;
        this.actor = actor;
        this.actionX = actionX;
        this.actionY = actionY;
        this.who = who;
    }

    public Movable Evolve(PlayerActionType actionType)
    {
        int actionX = 0;
        int actionY = 0;
        switch (actionType)
        {
            case PlayerActionType.North:
                actionY = 1;
                break;
            case PlayerActionType.East:
                actionX = 1;
                break;
            case PlayerActionType.South:
                actionY = -1;
                break;
            case PlayerActionType.West:
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
    [SerializeField]
    List<Movable> movables = new List<Movable>();
    [SerializeField]
    List<Imovable> imovables = new List<Imovable>();

    public void RegisterPlayer(PlayerController playerController)
    {
        Movable player = new Movable(0, 0, Actor.PLAYER, playerController.playerID, playerController.gameObject);
        movables.Add(player);
        playerController.OnPlayerAction += PlayerController_OnPlayerAction;
    }

    public void UnRegisterPlayer(PlayerController playerController)
    {
        movables.RemoveAll(m => m.who == playerController.gameObject);
        playerController.OnPlayerAction -= PlayerController_OnPlayerAction;
    }

    private void PlayerController_OnPlayerAction(int playerID, PlayerActionType actionType)
    {
        for (int i = 0, l = movables.Count; i < l; i++)
        {
            Movable m = movables[i];
            if (m.actor == Actor.PLAYER && m.id == playerID)
            {
                movables[i] = m.Evolve(actionType);
                return;
            }
        }
        Debug.LogWarning(string.Format("Could not find player id {0}", playerID));
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
            if (m.actor == Actor.PLAYER && m.WantsToMove)
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

    void ResolveConflict(int x, int y, Actor actor, GameObject who, int xDir, int yDir)
    {
        if (actor != Actor.PLAYER) return;
        for (int i=0, l=movables.Count(); i<l; i++)
        {
            Movable m = movables[i];
            if (m.x == x && m.y == y && m.actor == Actor.WALL)
            {
                int nextX = x + xDir;
                int nextY = y + yDir;
                if (!IsOccupied(nextX, nextY, Actor.WALL))
                {
                    movables[i] = m.Evolve(xDir, yDir).Enact();
                }
                return;
            }
        }
    }

    bool IsOccupied(int x, int y, Actor actor)
    {
        if (imovables.Any(e => e.x == x && e.y == y)) return true;
        if (movables.Any(e => e.x == x && e.y == y)) return true;

        //TODO: Block if actor is wall and object on pos

        return false;
    }

    [SerializeField]
    float gridSize = 1f;
    public float GridSize { get => gridSize; }

    Vector2 GetPositionAt(int x, int y)
    {
        return new Vector2(gridSize * x, gridSize * y);
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
                } else if (movables.Any(e => e.x == x && e.y == y && e.actor == Actor.WALL))
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
