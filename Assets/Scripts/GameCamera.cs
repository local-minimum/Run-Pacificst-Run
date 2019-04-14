using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : Singleton<GameCamera>
{
    Camera cam;
    protected override void LateAwake()
    {
        cam = GetComponent<Camera>();
    }

    int rectBuffer = 3;
    public Rect GetViewRect()
    {
        float buffer = Level.Instance.GridSize * rectBuffer;        
        Vector3 center = cam.ScreenToWorldPoint(new Vector3(Screen.width / 2 - 0.5f, Screen.height / 2 - 0.5f));
        Vector3 lowerLeft = cam.ScreenToWorldPoint(Vector3.zero);
        Vector2 screenSize = 2 * (center - lowerLeft);
        return new Rect(lowerLeft.x - buffer, lowerLeft.y - buffer, screenSize.x + 2 * buffer, screenSize.y + 2 * buffer);
    }
}
