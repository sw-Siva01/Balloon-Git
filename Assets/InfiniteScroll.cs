using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfiniteScroll : MonoBehaviour
{
    public ScrollRect scrollRect;
    public RectTransform viewPortTransform;
    public RectTransform contentPanelTransform;
    public HorizontalLayoutGroup HLG;

    public RectTransform[] ItemList;

    private Vector2 OldVelocity;
    bool isUpdated;

    public GameObject centerObject;  // The GameObject at the top of the ScrollView

    // Speed at which the content should automatically scroll
    public float scrollSpeed = 2000f;
    public float decelerationTime = 2f; // Time over which to decelerate to zero speed

    void Start()
    {

        isUpdated = false;
        OldVelocity = Vector2.zero;
        int ItemsToAdd = Mathf.CeilToInt(viewPortTransform.rect.width / (ItemList[0].rect.width + HLG.spacing));

        for (int i = 0; i < ItemsToAdd; i++)
        {
            RectTransform RT = Instantiate(ItemList[i % ItemList.Length], contentPanelTransform);
            RT.SetAsLastSibling();
        }
        for (int i = 0; i < ItemsToAdd; i++)
        {
            int num = ItemList.Length - i - 1;
            while (num < 0)
            {
                num += ItemList.Length;
            }
            RectTransform RT = Instantiate(ItemList[num], contentPanelTransform);
            RT.SetAsFirstSibling();
        }

        contentPanelTransform.localPosition = new Vector3(0 - (ItemList[0].rect.width + HLG.spacing) * ItemsToAdd,
            contentPanelTransform.localPosition.y,
            contentPanelTransform.localPosition.z);

        StartCoroutine(ScrollAutomatically());

        // Start the coroutine to stop scrolling after 5 seconds
        StartCoroutine(StopScrollingAfterTime(Random.Range(3, 6)));
    }

    IEnumerator ScrollAutomatically()
    {
        while (true)
        {
            if (isUpdated)
            {
                isUpdated = false;
                scrollRect.velocity = OldVelocity;
            }

            // Automatically scroll the content
            if (scrollSpeed != 0)
            {
                contentPanelTransform.localPosition += new Vector3(scrollSpeed * Time.deltaTime, 0, 0);
            }

            if (contentPanelTransform.localPosition.x > 0)
            {
                Canvas.ForceUpdateCanvases();
                OldVelocity = scrollRect.velocity;
                contentPanelTransform.localPosition -= new Vector3(ItemList.Length * (ItemList[0].rect.width + HLG.spacing), 0, 0);
                isUpdated = true;
            }
            if (contentPanelTransform.localPosition.x < 0 - (ItemList.Length * (ItemList[0].rect.width + HLG.spacing)))
            {
                Canvas.ForceUpdateCanvases();
                OldVelocity = scrollRect.velocity;
                contentPanelTransform.localPosition += new Vector3(ItemList.Length * (ItemList[0].rect.width + HLG.spacing), 0, 0);
                isUpdated = true;
            }

            yield return null;
        }
    }

    void Update()
    {
        /*if (isUpdated)
        {
            isUpdated = false;
            scrollRect.velocity = OldVelocity;
        }

        // Automatically scroll the content
        if (scrollSpeed != 0)
        {
            contentPanelTransform.localPosition += new Vector3(scrollSpeed * Time.deltaTime, 0, 0);
        }

        if (contentPanelTransform.localPosition.x > 0)
        {
            Canvas.ForceUpdateCanvases();
            OldVelocity = scrollRect.velocity;
            contentPanelTransform.localPosition -= new Vector3(ItemList.Length * (ItemList[0].rect.width + HLG.spacing), 0, 0);
            isUpdated = true;
        }
        if (contentPanelTransform.localPosition.x < 0 - (ItemList.Length * (ItemList[0].rect.width + HLG.spacing)))
        {
            Canvas.ForceUpdateCanvases();
            OldVelocity = scrollRect.velocity;
            contentPanelTransform.localPosition += new Vector3(ItemList.Length * (ItemList[0].rect.width + HLG.spacing), 0, 0);
            isUpdated = true;
        }*/
    }

    private IEnumerator StopScrollingAfterTime(float time)
    {
        #region
        yield return new WaitForSeconds(time);
        float initialSpeed = scrollSpeed;
        float elapsedTime = 0;

        while (elapsedTime < decelerationTime)
        {
            scrollSpeed = Mathf.Lerp(initialSpeed, 0, elapsedTime / decelerationTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        scrollSpeed = 0;
        #endregion


        #region
        /*yield return new WaitForSeconds(time);
        float initialSpeed = scrollSpeed;
        float elapsedTime = 0;

        // Calculate the target position based on the centerObject's center position
        float targetX = -centerObject.GetComponent<RectTransform>().localPosition.x + (viewPortTransform.rect.width / 2);

        while (elapsedTime < decelerationTime)
        {
            // Lerp the scrollSpeed to zero
            scrollSpeed = Mathf.Lerp(initialSpeed, 0, elapsedTime / decelerationTime);
            elapsedTime += Time.deltaTime;

            // Calculate the new position based on the current speed
            contentPanelTransform.localPosition += new Vector3(scrollSpeed * Time.deltaTime, 0, 0);

            yield return null;
        }

        // Ensure the content stops at the target position
        contentPanelTransform.localPosition = new Vector3(targetX, contentPanelTransform.localPosition.y, contentPanelTransform.localPosition.z);
        scrollSpeed = 0;*/
        #endregion
    }
}
