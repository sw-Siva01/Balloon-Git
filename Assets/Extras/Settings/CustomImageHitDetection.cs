using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CustomImageHitDetection : MonoBehaviour
{
    [SerializeField] private float AlphaHitThreshold = 1f;

    private void OnEnable()
    {
        if (TryGetComponent(out Image img))
        {
            img.alphaHitTestMinimumThreshold = AlphaHitThreshold;
        }
    }
}
