﻿using System.Collections;
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

    public void RegisterAgent(Agent agent)
    {
        agent.OnAction += HandleAgentAction;
        if (agent.TypeOfAgent == AgentType.PLAYER)
        {
            PlayerController player = agent as PlayerController;
            player.OnPlayerEvent += HandlePlayerEvent;
        }
    }

    public void UnRegisterAgent(Agent agent)
    {        
        agent.OnAction -= HandleAgentAction;
        if (agent.TypeOfAgent == AgentType.PLAYER)
        {
            PlayerController player = agent as PlayerController;
            player.OnPlayerEvent -= HandlePlayerEvent;
        }
        movables.Remove(agent.AgentID);
    }

    private void HandlePlayerEvent(PlayerEventType eventType)
    {
        if (eventType == PlayerEventType.DEATH)
        {
            currentLevel = 0;
            LoadLevel(currentLevel);
        }
        
    }

    private void HandleAgentAction(ushort agentId, AgentType agentType, AgentActionType actionType)
    {
        if (movables.Keys.Contains(agentId))
        {
            SetMovable(agentId, movables[agentId].Evolve(actionType));            
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
        LoadLevel(currentLevel);
    }

    int currentLevel = 0;

    void LoadLevel(int lvl)
    {
        levelDidReset = true;
        GameClock.Instance.ResetClock();
        DestroyNonPlayer();
        levelData = LevelDesigner.Generate(0);
        PopulateAgents();
        OnLevelEvent?.Invoke(LevelEventType.LOADED);
    }

    private void DestroyNonPlayer()
    {
        var nonplayers = movables.Where(kvp => kvp.Value.agentType != AgentType.PLAYER).Select(kvp => new {kvp.Value.who, agentID = kvp.Key}).ToArray();
        for (int i=0; i<nonplayers.Length; i++)
        {
            Destroy(nonplayers[i].who);            
        }
        movables.Clear();        
    }

    PlayerController GetPlayer(ushort agentId, int x, int y)
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (!player)
        {
            player = Instantiate(playerController);
        }
        SetMovable(agentId, new Movable(x, y, AgentType.PLAYER, player.gameObject), true);
        player.Setup(AgentType.PLAYER, agentId);
        return player;
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
                            PlayerController player = GetPlayer(agentId, x, y);
                            player.Move(GetPositionAt(x, y));
                            break;
                        case AgentType.MONSTER:                            
                            Enemy enemy = Beastiary.Instance.GetABeast(currentLevel);
                            SetMovable(agentId, new Movable(x, y, AgentType.MONSTER, enemy.gameObject), true);                            
                            enemy.Setup(AgentType.MONSTER, agentId);
                            enemy.Move(GetPositionAt(x, y));
                            break;
                    }
                }
            }
        }

        Debug.Log($"{movables.Count} movables in level");
    }

    private void Update()
    {
        PlaceFloors();
        if (shouldMove & !levelDidReset) MovePlayers();
        if (shouldMoveEveryone & !levelDidReset) MoveOthers();
        if (levelDidReset)
        {
            shouldMove = false;
            shouldMoveEveryone = false;
            levelDidReset = false;
        }
    }

    void MovePlayers()
    {
        Move(true);
        shouldMove = false;
    }

    void MoveOthers()
    {
        Move(false);
        shouldMoveEveryone = false;
    }
    bool levelDidReset = false;

    void SetMovable(ushort key, Movable m, bool allowFirstWrite = false)
    {
        if (allowFirstWrite && !movables.ContainsKey(key) || movables[key].who == m.who)
        {
            movables[key] = m;
        } else
        {
            Debug.LogWarning($"Refused setting movabel {key} because game object mismatch or missing.");
        }
    }

    private void Move(bool players)
    {
        ushort[] keys = movables.Keys.ToArray();
        for (int i = 0, l = keys.Length; i < l; i++)
        {
            if (levelDidReset) break;

            ushort key = keys[i];
            Movable m = movables[key];
            if ((m.agentType == AgentType.PLAYER) == players && m.WantsToMove)
            {
                if (Enact(m, key))
                {
                    SetMovable(key, m.Enact());
                }
                else
                {
                    SetMovable(key, m.Reset());
                }
            }
        }
    }

    bool Enact(Movable m, ushort agentId)
    {
        if (m.who == null)
        {
            Debug.LogError($"{agentId} is lacking its game object");
            movables.Remove(agentId);
            Debug.Log($"{movables.Count} movables remain");
            return false;
        }
        int nextX = m.NextX;
        int nextY = m.NextY;
        ResolveConflict(nextX, nextY, m.agentType, m.who, m.actionX, m.actionY);
        if (levelDidReset) return false;
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
        switch (actor)
        {
            case AgentType.PLAYER:
                ResolvePlayerConflict(x, y, xDir, yDir);
                break;
            case AgentType.MONSTER:
                ResolveMonsterConflict(x, y, who);
                break;
        }
    }

    void ResolveMonsterConflict(int x, int y, GameObject monster)
    {
        LevelFeatureValue targetVal = levelData[x, y];
        if (LevelFeature.GetAgentType(targetVal) == AgentType.PLAYER)
        {
            ushort agentID = LevelFeature.GetAgentId(targetVal);
            movables[agentID].who.SendMessage("Hurt", SendMessageOptions.RequireReceiver);
            monster.SendMessage("PerformedAttack", SendMessageOptions.RequireReceiver);
        }
    }

    void ResolvePlayerConflict(int x, int y, int xDir, int yDir) 
    {
        LevelFeatureValue targetVal = levelData[x, y];

        if (LevelFeature.FulfillsSemanticGroundMask(true, true, targetVal))
        {
            if (IsInsideLevel(x + xDir, y + yDir))
            {
                LevelFeatureValue nextVal = levelData[x + xDir, y + yDir];
                if (LevelFeature.DoesntBlockMovableGround(nextVal))
                {
                    levelData[x, y] = LevelFeature.SetGround(false, false, targetVal);
                    levelData[x + xDir, y + yDir] = LevelFeature.SetGround(true, true, nextVal);
                }
                else
                {
                    int xExtraDir = xDir == 0 ? 0 : xDir + Mathf.RoundToInt(Mathf.Sign(xDir));
                    int yExtraDir = yDir == 0 ? 0 : yDir + Mathf.RoundToInt(Mathf.Sign(yDir));
                    if (IsInsideLevel(x + xExtraDir, y + yExtraDir))
                    {
                        LevelFeatureValue nextNextVal = levelData[x + xExtraDir, y + yExtraDir];
                        if (!LevelFeature.WouldBlockMovableGroundWithoutAgent(nextVal))
                        {
                            if (!IsOccupied(x + xExtraDir, y + yExtraDir, LevelFeature.GetAgentType(nextNextVal)))
                            {
                                ushort agentId = LevelFeature.GetAgentId(nextVal);
                                SetMovable(agentId, movables[agentId].Evolve(xDir, yDir));                                
                                movables[agentId].who.SendMessage("Move", GetPositionAt(x + xDir, y + yDir), SendMessageOptions.RequireReceiver);

                                levelData[x + xExtraDir, y + yExtraDir] = LevelFeature.CopyAgent(nextNextVal, levelData[x + xExtraDir, y + yExtraDir]);
                                levelData[x + xDir, y + yDir] = LevelFeature.ClearAgent(nextNextVal);
                                levelData[x, y] = LevelFeature.SetGround(false, false, targetVal);
                                levelData[x + xDir, y + yDir] = LevelFeature.SetGround(true, true, nextVal);
                            }
                        }
                    }
                }
            }
        }
    }

    bool IsOccupied(int x, int y, AgentType actor)
    {
        return LevelFeature.BlocksAgent(levelData[x, y]);
    }
    
    [SerializeField]
    float gridSize = 1f;
    public float GridSize { get => gridSize; }

    public Movable GetMovableById(ushort id)
    {
        return movables[id];
    }

    public bool HasAgent(ushort id)
    {
        return movables.ContainsKey(id);
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
