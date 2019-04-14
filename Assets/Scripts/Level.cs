using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum Actor { PLAYER, MONSTER, NPC, OBJECT };


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

    public Movable Reset()
    {
        return new Movable(x, y, actor, id, who, 0, 0);
    }

    public Movable Enacted()
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
    List<Movable> movables = new List<Movable>();

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
                    movables[i] = m.Enacted();
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
        ResolveConflict(nextX, nextY, m.actor, m.who);
        bool occupied = IsOccupied(nextX, nextY, m.actor);
        if (!occupied)
        {
            m.who.SendMessage("Move", GetPositionAt(nextX, nextY), SendMessageOptions.RequireReceiver);
            return true;
        }
        return false;
    }

    void ResolveConflict(int x, int y, Actor actor, GameObject who)
    {

    }

    bool IsOccupied(int x, int y, Actor actor)
    {
        //TODO: Return remaining obstacle
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
    Sprite floor;
    List<Transform> floors = new List<Transform>();
    int floorIdx;

    Transform GetFloorTransform()
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
            sr.sprite = this.floor;
            floor = go.transform;
            floors.Add(floor);
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

        for (int x=left; x<=right; x++)
        {
            for (int y=bottom; y<=top; y++)
            {
                Transform t = GetFloorTransform();
                t.position = GetPositionAt(x, y);                
            }
        }
    }
}
