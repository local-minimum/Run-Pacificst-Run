using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InputType {None, North, East, South, West};
public delegate void InputEvent(InputType inputType);

public class GameInput : Singleton<GameInput>
{
    [SerializeField]
    KeyCode keyNorth = KeyCode.UpArrow;

    [SerializeField]
    KeyCode keyEast = KeyCode.RightArrow;

    [SerializeField]
    KeyCode keySouth = KeyCode.DownArrow;

    [SerializeField]
    KeyCode keyWeast = KeyCode.LeftArrow;

    public event InputEvent OnInput;

    InputType curInput;

    private void OnEnable()
    {
        GameClock.Instance.OnTick += HandleGameTick;
    }

    private void OnDisable()
    {
        GameClock.Instance.OnTick -= HandleGameTick;
    }

    private void HandleGameTick(int tick, int partialTick, float tickDuration, bool everyone)
    {
        float delayFraction = 0.2f;
        StartCoroutine(KeepGoing(tickDuration * delayFraction));
    }

    IEnumerator<WaitForSeconds> KeepGoing(float smallDelay)
    {        
        yield return new WaitForSeconds(smallDelay);
        if (curInput != InputType.None)
        {
            OnInput?.Invoke(curInput);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(keyNorth)) {
            curInput = InputType.North;
            OnInput?.Invoke(InputType.North);
        } else if (Input.GetKeyDown(keyEast))
        {
            curInput = InputType.East;
            OnInput?.Invoke(InputType.East);
        } else if (Input.GetKeyDown(keySouth))
        {
            curInput = InputType.South;
            OnInput?.Invoke(InputType.South);
        } else if (Input.GetKeyDown(keyWeast))
        {
            curInput = InputType.West;
            OnInput?.Invoke(InputType.West);
        } else if (Input.GetKeyUp(keyNorth))
        {
            if (curInput == InputType.North)
            {
                curInput = InputType.None;
            }            
        }
        else if (Input.GetKeyUp(keyEast))
        {
            if (curInput == InputType.East)
            {
                curInput = InputType.None;
            }
        }
        else if (Input.GetKeyUp(keySouth))
        {
            if (curInput == InputType.South)
            {
                curInput = InputType.None;
            }
        }
        else if (Input.GetKeyUp(keyWeast))
        {
            if (curInput == InputType.West)
            {
                curInput = InputType.None;
            }
        }
    }

}
