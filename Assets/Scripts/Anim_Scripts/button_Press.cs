using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class button_Press : MonoBehaviour
{
    public Sprite glowObj; // Reference to the glowing sprite
    public Sprite defaultSprite; // Reference to the original sprite
    private Image imageComponent; // Reference to the Image component
    private Coroutine changeSpriteCoroutine; // Reference to the coroutine

    private void OnEnable()
    {
        // Get the Image component attached to this GameObject
        imageComponent = GetComponent<Image>();

        // Start the coroutine to change the sprite after 1 second
        changeSpriteCoroutine = StartCoroutine(ChangeSpriteAfterDelay(0.1f));
    }

    private IEnumerator ChangeSpriteAfterDelay(float delay)
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(delay);

        // Change the image to glowObj
        if (imageComponent != null && glowObj != null)
        {
            imageComponent.sprite = glowObj;
        }
    }

    private void OnDisable()
    {
        // Set back to the default image
        if (imageComponent != null && defaultSprite != null)
        {
            imageComponent.sprite = defaultSprite;
        }

        StopCoroutine(changeSpriteCoroutine);
    }
}
