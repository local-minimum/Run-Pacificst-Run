using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using LevelFeatureValue = System.UInt32;

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
    readonly int id;
    public int Id { get => id; }
    public AgentType actor;
    public int actionX;
    public int actionY;
    public GameObject who;

    public Movable(int x, int y, AgentType actor, int id, GameObject who)
    {
        this.x = x;
        this.y = y;
        this.id = id;
        this.actor = actor;
        actionX = 0;
        actionY = 0;
        this.who = who;
    }

    public Movable(int x, int y, AgentType actor, int id, GameObject who, int actionX, int actionY)
    {
        this.x = x;
        this.y = y;
        this.id = id;
        this.actor = actor;
        this.actionX = actionX;
        this.actionY = actionY;
        this.who = who;
    }

    public Movable Evolve(AgentActionType actionType)
    {
        int actionX = 0;
        int actionY = 0;
        switch (actionType)
        {
            case AgentActionType.North:
                actionY = 1;
                break;
            case AgentActionType.East:
                actionX = 1;
                break;
            case AgentActionType.South:
                actionY = -1;
                break;
            case AgentActionType.West:
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

    List<Movable> movables = new List<Movable>();

    LevelFeatureValue[,] levelData;

    public int RegisterAgent(Agent agent)
    {
        int id = nextMovableId;
        nextMovableId++;        
        Movable movable = new Movable(0, 0, agent.TypeOfMover, id, agent.gameObject);
        movables.Add(movable);
        agent.OnAction += HandleAgentAction;
        return id;
    }

    public void UnRegisterAgent(Agent agent)
    {        
        movables.RemoveAll(m => m.who == agent.gameObject);
        agent.OnAction -= HandleAgentAction;
    }

    private void HandleAgentAction(int moverId, AgentType agentType, AgentActionType actionType)
    {
        for (int i = 0, l = movables.Count; i < l; i++)
        {
            Movable m = movables[i];
            if (m.actor == agentType && m.Id == moverId)
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

    private void Start()
    {
        levelData = LevelDesigner.Generate(0);
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
            if (m.actor == AgentType.PLAYER && m.WantsToMove)
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

    bool IsInsideLevel(int x, int y)
    {
        return x >= 0 && y >= 0 && x < levelData.GetLength(0) && y < levelData.GetLength(1);
    }

    void ResolveConflict(int x, int y, AgentType actor, GameObject who, int xDir, int yDir)
    {
        if (actor != AgentType.PLAYER) return;
        if (LevelFeature.FulfillsSemanticGroundMask(true, true, levelData[x, y]))
        {
            if (IsInsideLevel(x + xDir, y + yDir)) {
                if (LevelFeature.IsVacant(levelData[x + xDir, y + yDir]))
                {
                    levelData[x, y] = LevelFeature.EvolveGround(false, false, levelData[x, y]);
                    levelData[x + xDir, y + yDir] = LevelFeature.EvolveGround(true, true, levelData[x + xDir, y + yDir]);
                }
            }
        }
    }

    bool IsOccupied(int x, int y, AgentType actor)
    {
        return LevelFeature.IsBlocked(levelData[x, y]);
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
            .Where(m => m.actor == AgentType.PLAYER)
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

    Transform GetFloorTransform(GroundType floorType)
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
        if (floorType == GroundType.BASIC)
        {
            floor.GetComponent<SpriteRenderer>().sprite = floorBasic;
        } else if (floorType == GroundType.BLOCKING_IMOVABLE)
        {
            floor.GetComponent<SpriteRenderer>().sprite = wallImovable;
        } else if (floorType == GroundType.BLOCKING_MOVABLE)
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
        GroundType floorType;

        for (int x=left; x<=right; x++)
        {
            for (int y=bottom; y<=top; y++)
            {
                if (IsInsideLevel(x, y))
                {
                    floorType = LevelFeature.GetGroundType(levelData[x, y]);
                    Transform t = GetFloorTransform(floorType);
                    t.position = GetPositionAt(x, y);
                }
            }
        }
    }
}
