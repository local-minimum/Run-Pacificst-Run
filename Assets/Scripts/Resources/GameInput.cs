using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InputType {North, East, South, West};
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

    private void Update()
    {
        if (Input.GetKeyDown(keyNorth)) {
            OnInput?.Invoke(InputType.North);
        } else if (Input.GetKeyDown(keyEast))
        {
            OnInput?.Invoke(InputType.East);
        } else if (Input.GetKeyDown(keySouth))
        {
            OnInput?.Invoke(InputType.South);
        } else if (Input.GetKeyDown(keyWeast))
        {
            OnInput?.Invoke(InputType.West);
        }
    }

}
