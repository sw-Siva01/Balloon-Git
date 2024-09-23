using UnityEngine;
using TMPro;

public class EnableTextColor : MonoBehaviour
{
    TMP_Text text;
    public Color shadowColor = Color.black;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
        text.fontMaterial.SetColor("_UnderlayColor", shadowColor);
        text.fontMaterial.SetFloat("_UnderlayOffsetY",-1f);
    }
}