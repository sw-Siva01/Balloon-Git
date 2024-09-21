using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI; // Include this if you are using UI Image

public class ImageSequencer : MonoBehaviour
{
    public Sprite[] sprites; // Array to hold the sprites
    public float frameRate = 0.1f; // Time between frames

    private Image image; // Reference to the UI Image component (if using UI)
    private int currentIndex = 0; // Current sprite index
    private float timer = 0f; // Timer for animation
    private bool isON;
    void OnEnable()
    {
        isON = true;
        // Get the Image component (if you're using UI)
        image = GetComponent<Image>();

        // Optional: Initialize with the first sprite
        if (sprites.Length > 0 && image != null)
        {
            image.sprite = sprites[currentIndex];
        }

        StartCoroutine(nameof(LoopingSequence));
    }

    IEnumerator LoopingSequence()
    {
        while (isON)
        {
            // Update the timer
            timer += Time.deltaTime;

            // Check if it's time to switch to the next sprite
            if (timer >= frameRate)
            {
                // Reset the timer
                timer = 0f;

                // Update the sprite index
                currentIndex = (currentIndex + 1) % sprites.Length;

                // Change the sprite
                if (image != null)
                {
                    image.sprite = sprites[currentIndex];
                }
            }
            yield return null;
        }
    }
    private void OnDisable()
    {
        isON = false;
        StopCoroutine(nameof(LoopingSequence));
    }
}
