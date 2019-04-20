using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using LevelFeatureValue = System.UInt32;

public enum LevelEventType {LOADED};
public delegate void LevelEvent(LevelEventType eventType);

public struct Movable
{
    public int x;
    public int y;
    public AgentType agentType;
    public int actionX;
    public int actionY;
    public GameObject who;

    public Movable(int x, int y, AgentType agentType, GameObject who)
    {
        this.x = x;
        this.y = y;
        this.agentType = agentType;
        actionX = 0;
        actionY = 0;
        this.who = who;
    }

    public Movable(int x, int y, AgentType agentType, GameObject who, int actionX, int actionY)
    {
        this.x = x;
        this.y = y;
        this.agentType = agentType;
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
        return new Movable(x, y, agentType, who, actionX, actionY);
    }

    public Movable Evolve(int actionX, int actionY)
    {
        return new Movable(x, y, agentType, who, actionX, actionY);
    }

    public Movable Reset()
    {
        return new Movable(x, y, agentType, who, 0, 0);
    }

    public Movable Enact()
    {
        return new Movable(NextX, NextY, agentType, who, 0, 0);
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
    Dictionary<ushort, Movable> movables = new Dictionary<ushort, Movable>();
    public event LevelEvent OnLevelEvent;
    LevelFeatureValue[,] levelData;

    [SerializeField]
    PlayerController playerController;
    [SerializeField]
    SillyEnemy enemyPrefab;

    public void RegisterAgent(Agent agent)
    {
        agent.OnAction += HandleAgentAction;
    }

    public void UnRegisterAgent(Agent agent)
    {        
        agent.OnAction -= HandleAgentAction;
        movables.Remove(agent.AgentID);
    }

    private void HandleAgentAction(ushort agentId, AgentType agentType, AgentActionType actionType)
    {
        if (movables.Keys.Contains(agentId))
        {
            movables[agentId] = movables[agentId].Evolve(actionType);
        } else
        {        
            Debug.LogWarning(string.Format("Could not find player id {0}", agentId));
        }
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
        PopulateAgents();
        OnLevelEvent?.Invoke(LevelEventType.LOADED);
    }

    private void PopulateAgents()
    {
        for (int x=0, width=levelData.GetLength(0); x<width; x++)
        {
            for (int y=0, height=levelData.GetLength(1); y<height; y++)
            {
                LevelFeatureValue val = levelData[x, y];
                if (LevelFeature.HasAgent(val))
                {
                    ushort agentId = LevelFeature.GetAgentId(val);
                    Debug.Log($"Agent {agentId} on {x} {y} of type {LevelFeature.GetAgentType(val)}");
                    switch (LevelFeature.GetAgentType(val)) {
                        case AgentType.PLAYER:                            
                            PlayerController player = Instantiate(playerController);
                            movables[agentId] = new Movable(x, y, AgentType.PLAYER, player.gameObject);
                            player.Setup(AgentType.PLAYER, agentId);
                            player.Move(GetPositionAt(x, y));
                            break;
                        case AgentType.MONSTER:                            
                            SillyEnemy enemy = Instantiate(enemyPrefab);
                            movables[agentId] = new Movable(x, y, AgentType.MONSTER, enemy.gameObject);
                            enemy.Setup(AgentType.MONSTER, agentId);
                            enemy.Move(GetPositionAt(x, y));
                            break;
                    }
                }
            }
        }
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
        ushort[] keys = movables.Keys.ToArray();

        for (int i=0, l=keys.Length; i<l; i++)
        {
            ushort key = keys[i];
            Movable m = movables[key];
            if (m.agentType == AgentType.PLAYER && m.WantsToMove)
            {
                if (Enact(m, key))
                {
                    moves += 1;
                    movables[key] = m.Enact();

                } else
                {
                    movables[key] = m.Reset();
                }
            }
        }
        shouldMove = false;
    }

    void MoveOthers()
    {
        shouldMoveEveryone = false;
    }

    bool Enact(Movable m, ushort agentId)
    {
        int nextX = m.NextX;
        int nextY = m.NextY;
        ResolveConflict(nextX, nextY, m.agentType, m.who, m.actionX, m.actionY);
        bool occupied = IsOccupied(nextX, nextY, m.agentType);
        if (!occupied)
        {
            levelData[nextX, nextY] = LevelFeature.CopyAgent(levelData[m.x, m.y], levelData[nextX, nextY]);
            levelData[m.x, m.y] = LevelFeature.ClearAgent(levelData[m.x, m.y]);
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
                    levelData[x, y] = LevelFeature.SetGround(false, false, levelData[x, y]);
                    levelData[x + xDir, y + yDir] = LevelFeature.SetGround(true, true, levelData[x + xDir, y + yDir]);
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

    public Movable GetMovableById(ushort id)
    {
        return movables[id];
    }

    Vector2 GetPositionAt(int x, int y)
    {
        return new Vector2(gridSize * x, gridSize * y);
    }

    public Movable GetPlayerClosestTo(int x, int y, int maxDist=-1)
    {
        return movables
            .Where(m => m.Value.agentType == AgentType.PLAYER)
            .Select(m => new
            {
                dist = Mathf.Abs(m.Value.x - x) + Mathf.Abs(m.Value.y - y),
                movable = m.Value,
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
