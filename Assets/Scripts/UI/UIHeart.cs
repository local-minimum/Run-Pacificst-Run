using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum UIHeartState { Active, Inactive, Locked };

public class UIHeart : MonoBehaviour
{
    [SerializeField]
    Color32 activeColor;

    [SerializeField]
    Color32 inactiveColor;

    [SerializeField]
    Color32 lockedColor;

    [SerializeField]
    Sprite normalHeart;

    [SerializeField]
    Sprite lockedHeart;

    UIHeartState curState = UIHeartState.Inactive;

    public void SetState(UIHeartState heartState)
    {
        Image image = GetComponent<Image>();
        bool stateSwap = curState != heartState;

        switch (heartState)
        {
            case UIHeartState.Active:
                if (stateSwap) StartCoroutine(Beat());
                image.color = activeColor;
                image.sprite = normalHeart;
                break;
            case UIHeartState.Inactive:
                image.color = inactiveColor;
                image.sprite = normalHeart;
                break;
            case UIHeartState.Locked:
                image.color = lockedColor;
                image.sprite = lockedHeart;
                break;
        }

        curState = heartState;
    }

    [SerializeField]
    AnimationCurve beatSizeAnim;

    bool beating = false;

    IEnumerator<WaitForSeconds> Beat()
    {
        if (beating) yield break;
        beating = true;
        RectTransform rt = transform as RectTransform;
        Vector2 baseSize = rt.sizeDelta;
        float duration = beatSizeAnim[beatSizeAnim.length - 1].time;
        float t = 0;
        float step = 0.02f;
        while (t < duration)
        {
            float scale = beatSizeAnim.Evaluate(t);
            rt.sizeDelta = baseSize * scale;
            yield return new WaitForSeconds(step);
            t += step;
        }
        rt.sizeDelta = baseSize;
        beating = false;
    }
}
