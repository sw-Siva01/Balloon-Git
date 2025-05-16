using UnityEngine;

public class RotateUI : MonoBehaviour
{
    public float rotationSpeed = 100f; // Degrees per second

    void Update()
    {
        // Rotate around Z axis (for 2D UI)
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }
}
