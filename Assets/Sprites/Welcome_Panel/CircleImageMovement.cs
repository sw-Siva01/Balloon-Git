using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CircleImageMovement : MonoBehaviour
{
    public RectTransform[] images; // Array to hold the image RectTransforms
    public float radius = 100f; // Radius of the circle
    public float moveDistance = 50f; // Distance to move outward
    public float moveDuration = 1f; // Duration for the move
    public float delayBetweenMoves = 0.5f; // Delay between movements

    private void Start()
    {
        // Initialize the positions of the images in a circle
        InitializeCircle();
        // Start the movement sequence
        StartCoroutine(MoveImagesInCircle());
    }

    private void InitializeCircle()
    {
        int count = images.Length;
        /*float angleStep = 360f / count;
        for (int i = 0; i < count; i++)
        {
            float angle = i * angleStep;
            Vector2 position = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad) * radius, Mathf.Sin(angle * Mathf.Deg2Rad) * radius);
            images[i].anchoredPosition = position;
        }*/
    }

    private IEnumerator MoveImagesInCircle()
    {
        while (true)
        {
            for (int i = 0; i < images.Length; i++)
            {
                StartCoroutine(MoveImage(images[i]));
                // Wait for the specified delay before moving the next image
                yield return new WaitForSeconds(delayBetweenMoves);
            }

            // Optionally, wait a bit before repeating the sequence
            yield return new WaitForSeconds(delayBetweenMoves * images.Length);
        }
    }

    private IEnumerator MoveImage(RectTransform image)
    {
        Vector2 originalPosition = image.anchoredPosition;
        // Calculate the outward direction
        Vector2 direction = (originalPosition).normalized;
        Vector2 targetPosition = originalPosition + direction * moveDistance;

        // Move out
        float elapsedTime = 0f;
        while (elapsedTime < moveDuration)
        {
            image.anchoredPosition = Vector2.Lerp(originalPosition, targetPosition, (elapsedTime / moveDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        image.anchoredPosition = targetPosition;

        // Wait before moving back
        yield return new WaitForSeconds(delayBetweenMoves);

        // Move back
        elapsedTime = 0f;
        while (elapsedTime < moveDuration)
        {
            image.anchoredPosition = Vector2.Lerp(targetPosition, originalPosition, (elapsedTime / moveDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        image.anchoredPosition = originalPosition;
    }
}
