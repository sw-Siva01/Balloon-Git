using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class CustomOutline : MonoBehaviour
{
    private TMP_Text _tmpText;
    private Material _combinedMaterial;

    [Header("Outline Settings")]
    public bool useOutline = true;
    public Color outlineColor = Color.black;
    [Min(0)] public float outlineThickness = 0.2f;

    [Header("Shadow Settings")]
    public bool useShadow = true;
    public Color shadowColor = Color.black;
    public Vector2 shadowOffset = new Vector2(1, -1);
    [Min(0)] public float shadowSoftness = 0.5f;

    void Awake()
    {
        _tmpText = GetComponent<TMP_Text>();

        // Create a unique material
        _combinedMaterial = new Material(_tmpText.fontMaterial);
        _combinedMaterial.name = "CombinedMaterial";

        ApplyEffects();
    }

    void OnValidate()
    {
        if (_tmpText == null)
            _tmpText = GetComponent<TMP_Text>();

        // Create a unique material if it does not exist
        if (_combinedMaterial == null)
        {
            _combinedMaterial = new Material(_tmpText.fontMaterial);
            _combinedMaterial.name = "CombinedMaterial";
        }

        ApplyEffects();
    }

    public void SetOutlineColor(Color color)
    {
        outlineColor = color;
    }

    public void SetShadowColor(Color color)
    {
        shadowColor = color;
    }

    public void SetOutlineAndShadowColor(Color outlineColor, Color shadowColor)
    {
        this.outlineColor = outlineColor;
        this.shadowColor = shadowColor;
    }

    void ApplyEffects()
    {
        if (_tmpText != null)
        {
            // Apply Outline if enabled
            if (useOutline)
            {
                _combinedMaterial.SetColor(ShaderUtilities.ID_OutlineColor, outlineColor);
                _combinedMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, outlineThickness);
            }
            else
            {
                _combinedMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0f);
            }

            // Apply Drop Shadow if enabled
            if (useShadow)
            {
                _combinedMaterial.EnableKeyword("UNDERLAY_ON");
                _combinedMaterial.SetColor(ShaderUtilities.ID_UnderlayColor, shadowColor);
                _combinedMaterial.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, shadowOffset.x);
                _combinedMaterial.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, shadowOffset.y);
                _combinedMaterial.SetFloat(ShaderUtilities.ID_UnderlaySoftness, shadowSoftness);
            }
            else
            {
                _combinedMaterial.DisableKeyword("UNDERLAY_ON");
                _combinedMaterial.SetColor(ShaderUtilities.ID_UnderlayColor, Color.clear);
            }

            _tmpText.fontSharedMaterial = _combinedMaterial;
        }
    }
}