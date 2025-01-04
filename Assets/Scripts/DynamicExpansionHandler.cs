using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicExpansionHandler : MonoBehaviour
{
    public RectTransform ScreenRefCanvas;
    public List<ExpandableElementData> ExpandableElements;
    public float MinAspect = 0.75f;
    public float MaxAspect = 1f;

    private void Update()
    {
        if (ExpandableElements.Count > 0)
        {
            float currentAspect = ScreenRefCanvas.sizeDelta.x / ScreenRefCanvas.sizeDelta.y;
            float value = Mathf.InverseLerp(MinAspect, MaxAspect, currentAspect);
            foreach (ExpandableElementData element in ExpandableElements)
            {
                float width = Mathf.Lerp(element.MinWidth, element.MaxWidth, value);
                element.ElementRect.sizeDelta = new(width, element.ElementRect.sizeDelta.y);
            }
        }
    }
}

[System.Serializable]
public class ExpandableElementData
{
    public RectTransform ElementRect;
    public float MinWidth;
    public float MaxWidth;
}