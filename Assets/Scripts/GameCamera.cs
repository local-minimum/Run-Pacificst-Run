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

    private void OnEnable()
    {
        Level.Instance.OnLevelEvent += Instance_OnLevelEvent;
    }

    private void OnDisable()
    {
        if (Level.Instance) Level.Instance.OnLevelEvent -= Instance_OnLevelEvent;
    }

    private void Instance_OnLevelEvent(LevelEventType eventType)
    {
        switch (eventType)
        {
            case LevelEventType.LOADED:
                CenterOnPlayer();
                break;
        }
        
    }

    void CenterOnPlayer()
    {
        if (playerTransform == null) return;
        Vector3 offset = playerTransform.position - transform.position;
        offset.z = 0;
        transform.Translate(offset);
    }

    Transform playerTransform;

    public void RegisterPlayer(PlayerController player)
    {
        playerTransform = player.transform;
    }

    public void UnregisterPlayer(PlayerController player)
    {
        playerTransform = null;
    }

    [SerializeField]
    float xMove = 0.05f;
    [SerializeField]
    float xMoveTo = .2f;

    [SerializeField]
    float yMove = 0.1f;
    [SerializeField]
    float yMoveTo = 0.15f;

    private void Update()
    {
        if (playerTransform)
        {            
            Vector2 playPos = cam.WorldToScreenPoint(playerTransform.position);
            float offX = 0;
            float offY = 0;
            if (playPos.x < xMove * Screen.width)
            {
                offX = (1 - xMoveTo) * Screen.width;
            } else if (playPos.x > (1 - xMove) * Screen.width)
            {
                offX = xMoveTo * Screen.width;
            } else
            {
                offX = Screen.width * 0.5f;
            }
            if (playPos.y < yMove * Screen.height)
            {
                offY = (1 - yMoveTo) * Screen.height;
            } else if (playPos.y > (1 - yMove) * Screen.height)
            {
                offY = yMoveTo * Screen.height;
            } else
            {
                offY = Screen.height * 0.5f;
            }
            Vector3 offset = 2 * (transform.position - cam.ScreenToWorldPoint(new Vector3(offX, offY, 0)));
            offset.z = 0;
            transform.Translate(offset);
        }
    }
}
