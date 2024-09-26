using UnityEngine;
using TMPro;

public class InterTxt : MonoBehaviour
{
    // This Script for Balloon Multiplier Value Text
    TMP_Text text;
    public Color shadowColor = Color.black;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
        text.fontMaterial.SetColor("_UnderlayColor", shadowColor);
        text.fontMaterial.SetFloat("_UnderlayOffsetY", -1f);
        text.fontMaterial.SetFloat("_UnderlaySoftness", 0.3f);
    }
}
