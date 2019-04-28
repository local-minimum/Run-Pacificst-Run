using System.Collections;
using System.Collections.Generic;
using UnityEngine;
enum TurnEnemyAction { WALK, TURN, ATTACK };

public class TurnEnemy : Enemy
{
    List<TurnType> turnPattern = new List<TurnType>();
    int turnPatternLength;
    int turnIndex = 0;
    AgentActionType heading;

    private void Awake()
    {
        TypeOfAgent = AgentType.MONSTER;
        SetupBehaviour();
    }

    TurnType RandomTurn
    {
        get
        {
            return (TurnType)Random.Range(0, 3);
        }
    }

    void SetupBehaviour()
    {
        heading = RandomHeading;
        SetupTurnPattern();
    }

    void SetupTurnPattern()
    {
        turnPatternLength = Random.Range(1, 6);
        if (turnPatternLength > 1) turnPatternLength -= 1;
        turnPattern.Clear();
        for (int i = 0; i < turnPatternLength; i++)
        {
            turnPattern.Add(RandomTurn);
        }
        turnIndex = -1;
    }

    private void OnEnable()
    {
        GameClock.Instance.OnTick += HandleTick;
        Level.Instance.RegisterAgent(this);
    }

    private void OnDisable()
    {
        if (GameClock.Instance) GameClock.Instance.OnTick -= HandleTick;
        if (Level.Instance) Level.Instance.UnRegisterAgent(this);
    }

    TurnEnemyAction myAction = TurnEnemyAction.WALK;

    protected override void PerformedAttack()
    {
        myAction = TurnEnemyAction.ATTACK;
    }

    int prevX = -1;
    int prevY = -1;
    int annoyance = -1;
    [SerializeField]
    int PanicTurnAtAnnoyance = 3;

    AgentActionType GetTurnedHeading()
    {
        turnIndex += 1;
        turnIndex %= turnPatternLength;
        TurnType turn = turnPattern[turnIndex];
        return Agent.Turn(heading, turn);
    }

    void HandleWalkStatus(Movable m)
    {
        if (m.x == prevX && m.y == prevY && myAction != TurnEnemyAction.ATTACK)
        {
            annoyance += 1;
            myAction = TurnEnemyAction.TURN;
            if (annoyance > PanicTurnAtAnnoyance)
            {                
                heading = RandomHeading;                
            } else
            {
                heading = GetTurnedHeading();
            }
        } else
        {
            myAction = TurnEnemyAction.WALK;
            annoyance = 0;
        }
        prevX = m.x;
        prevY = m.y;
    }

    private void HandleTick(int tick, int partialTick, float tickDuration, bool everyone)
    {
        if (everyone && Level.Instance.HasAgent(AgentID))
        {
            Movable m = Level.Instance.GetMovableById(AgentID);
            HandleWalkStatus(m);
            Emit(heading);
        }
    }
}
