using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHealthBar : MonoBehaviour
{
    [SerializeField]
    UIHeart heartPrefab;

    List<UIHeart> hearts = new List<UIHeart>();

    [SerializeField]
    int lockedHearts = 0;

    [SerializeField]
    int maxHealth = 3;

    [SerializeField]
    int health = 3;

    [SerializeField]
    int heartSpacing = 0;

    int prevSpacing = 0;

    float heartsRight = 0;

    private void Start()
    {
        hearts.AddRange(GetComponentsInChildren<UIHeart>());
        UpdateHearts();
        RePositionHearts();
    }

    void RePositionHearts()
    {
        for (int heartIdx=0, l=hearts.Count; heartIdx<l; heartIdx++)
        {
            PositionHeart(hearts[heartIdx].transform as RectTransform, heartIdx);
        }

        RectTransform rt = transform as RectTransform;        
        Vector3 size = rt.sizeDelta;
        size.x = heartsRight;
        rt.sizeDelta = size;

        prevSpacing = heartSpacing;
    }

    void AddHeart()
    {
        int heartIdx = hearts.Count;
        RectTransform rt = transform as RectTransform;
        UIHeart heart = Instantiate(heartPrefab);
        RectTransform heartRT = heart.transform as RectTransform;
        heartRT.SetParent(rt);
        PositionHeart(heartRT, heartIdx);
        hearts.Add(heart);
    }

    void PositionHeart(RectTransform heartRT, int heartIdx)
    {
        Vector2 size = heartRT.sizeDelta;
        Vector3 yOff = Vector3.down * 0.5f * size.y;
        float pos = (size.x + heartSpacing) * (heartIdx + 0.5f);
        heartRT.localPosition = Vector3.right * pos + yOff;
        heartsRight = pos + size.x;
    }

    UIHeart GetHeart(int heartIndex)
    {        
        while (heartIndex > hearts.Count - 1)
        {
            AddHeart();
        }        
        UIHeart heart = hearts[heartIndex];
        heart.gameObject.SetActive(true);
        return heart;
    }

    void UpdateHearts()
    {
        for (int idx = 0; idx<maxHealth; idx++)
        {
            UIHeart heart = GetHeart(idx);
            if (idx < lockedHearts)
            {
                heart.SetState(UIHeartState.Locked);
            } else if (idx < health)
            {
                heart.SetState(UIHeartState.Active);
            } else
            {
                heart.SetState(UIHeartState.Inactive);
            }
        }
        for (int idx = maxHealth, l=hearts.Count; idx < l; idx ++)
        {
            hearts[idx].gameObject.SetActive(false);
        }
    }

    private void OnGUI()
    {
        UpdateHearts();
        if (heartSpacing != prevSpacing) RePositionHearts();
    }
}
