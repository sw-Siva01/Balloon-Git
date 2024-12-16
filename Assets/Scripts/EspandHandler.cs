using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EspandHandler : MonoBehaviour
{
    public RectTransform rectTransform;
    public RectTransform referenceRect;

    float lastPercent = 0;

    void Update()
    {
        float percent = (referenceRect.rect.size.y / referenceRect.rect.size.x);
        if (percent != lastPercent)
        {
            lastPercent = percent;
            if (referenceRect.rect.size.y / referenceRect.rect.size.x > (4f / 3f))
            {
                rectTransform.sizeDelta = new Vector2(referenceRect.rect.size.x, rectTransform.rect.size.y);
            }
            else
            {
                rectTransform.sizeDelta = new Vector2(referenceRect.rect.size.y * 0.75f, rectTransform.rect.size.y);
            }
        }
    }
}