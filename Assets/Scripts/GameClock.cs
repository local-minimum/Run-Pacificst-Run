using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void TickEvent(int tick, int partialTick, float tickDuration, bool everyone);

public class GameClock : Singleton<GameClock>
{
    [SerializeField]
    float gameTickTime = 1;

    [SerializeField]
    int playerTicksPerTick = 1;

    public event TickEvent OnTick;

    bool playerTicksAreValid = true;

    int ticks;
    int partialTick;

    bool ticking;

    protected override void LateAwake()
    {
        OnTick += GameClock_OnTick;
    }

    private void GameClock_OnTick(int tick, int partialTick, float tickDuration, bool everyone)
    {
        Debug.Log(string.Format("tick {0}:{1} duration {2} everyone {3}", tick, partialTick, tickDuration, everyone));
    }

    private void OnEnable()
    {
        ticking = true;
        StartCoroutine(Ticker());
    }

    private void OnDisable()
    {
        ticking = false;
    }

    IEnumerator<WaitForSeconds> Ticker()
    {
        int partialsPerTick = playerTicksPerTick;
        float duration = gameTickTime;
        while (ticking)
        {
            if (ticks >= 0)
            {
                partialTick += 1;
                if (playerTicksAreValid || partialTick == partialsPerTick)
                {
                    OnTick?.Invoke(ticks, partialTick, duration, partialTick == partialsPerTick);
                }

                yield return new WaitForSeconds(duration / partialsPerTick);

                if (partialTick == partialsPerTick)
                {
                    partialsPerTick = playerTicksPerTick;
                    playerTicksAreValid = true;
                    ticks += 1;
                    partialTick = 0;
                    duration = gameTickTime;
                }
            }
            else {
                partialsPerTick = playerTicksPerTick;
                playerTicksAreValid = true;
                ticks += 1;
                partialTick = 0;
                duration = gameTickTime;
                yield return new WaitForSeconds(duration);
            }
        }
    }

    public void ResetClock()
    {
        ticks = -3;
    }
}
