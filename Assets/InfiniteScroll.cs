using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class InfiniteScroll : MonoBehaviour
{
    public ScrollRect scrollRect;
    public RectTransform viewPortTransform;
    public RectTransform contentPanelTransform;
    public HorizontalLayoutGroup HLG;
    public RectTransform[] ItemList;
    public GameObject targetPositionObject; // Target position GameObject

    private Vector2 oldVelocity;
    private bool isUpdated;

    public bool autoScroll = false;
    public float scrollSpeed = 200f;
    public float decelerationRate = 0.1f;
    public float minScrollTime = 4f;
    public float maxScrollTime = 6f;

    public float slerpDuration = 1.0f; // Duration of the slerp for centering

    void Start()
    {
        isUpdated = false;
        oldVelocity = Vector2.zero;

        // Calculate the number of items needed to fill the viewport
        int itemsToAdd = Mathf.CeilToInt(viewPortTransform.rect.width / (ItemList[0].rect.width + HLG.spacing));

        // Add items to the end of the content panel
        for (int i = 0; i < itemsToAdd; i++)
        {
            RectTransform rt = Instantiate(ItemList[i % ItemList.Length], contentPanelTransform);
            rt.SetAsLastSibling();
        }

        // Add items to the beginning of the content panel
        for (int i = 0; i < itemsToAdd; i++)
        {
            int num = ItemList.Length - i - 1;
            while (num < 0)
            {
                num += ItemList.Length;
            }
            RectTransform rt = Instantiate(ItemList[num], contentPanelTransform);
            rt.SetAsFirstSibling();
        }

        // Adjust the starting position of the content panel
        contentPanelTransform.localPosition = new Vector3(
            -(ItemList[0].rect.width + HLG.spacing) * itemsToAdd,
            contentPanelTransform.localPosition.y,
            contentPanelTransform.localPosition.z
        );
    }

    void Update()
    {
        if (isUpdated)
        {
            isUpdated = false;
            scrollRect.velocity = oldVelocity;
        }

        // Check if the content has scrolled past the left boundary
        if (contentPanelTransform.localPosition.x > 0)
        {
            Canvas.ForceUpdateCanvases();
            oldVelocity = scrollRect.velocity;
            contentPanelTransform.localPosition -= new Vector3(
                ItemList.Length * (ItemList[0].rect.width + HLG.spacing), 0, 0
            );
            isUpdated = true;
        }
        // Check if the content has scrolled past the right boundary
        if (contentPanelTransform.localPosition.x < -(ItemList.Length * (ItemList[0].rect.width + HLG.spacing)))
        {
            Canvas.ForceUpdateCanvases();
            oldVelocity = scrollRect.velocity;
            contentPanelTransform.localPosition += new Vector3(
                ItemList.Length * (ItemList[0].rect.width + HLG.spacing), 0, 0
            );
            isUpdated = true;
        }

        // Handle automatic scrolling
        if (autoScroll)
        {
            // Shuffle the children of the content panel
            ShuffleContentChildren();

            StartCoroutine(AutoScrollForRandomTime());
            autoScroll = false;
        }
    }

    private void ShuffleContentChildren()
    {
        int childCount = contentPanelTransform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform child = contentPanelTransform.GetChild(i);
            int randomIndex = Random.Range(0, childCount);
            child.SetSiblingIndex(randomIndex);
        }
    }

    private IEnumerator AutoScrollForRandomTime()
    {
        float scrollDuration = Random.Range(minScrollTime, maxScrollTime);
        float elapsedTime = 0f;

        while (elapsedTime < scrollDuration)
        {
            contentPanelTransform.localPosition += new Vector3(scrollSpeed * Time.deltaTime, 0, 0);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        while (scrollRect.velocity.magnitude > 0.1f)
        {
            scrollRect.velocity = Vector2.Lerp(scrollRect.velocity, Vector2.zero, Time.deltaTime * decelerationRate);
            yield return null;
        }

        /*// Adjust to the nearest centered item after stopping
        CenterOnClosestItem();*/

        // Adjust to the nearest centered item after stopping
        StartCoroutine(CenterOnClosestItem());
    }

    /*private void CenterOnClosestItem()
    {
        float closestDistance = float.MaxValue;
        RectTransform closestItem = null;

        foreach (RectTransform item in contentPanelTransform)
        {
            Vector3 itemPos = viewPortTransform.InverseTransformPoint(item.position);
            Vector3 targetPos = viewPortTransform.InverseTransformPoint(targetPositionObject.transform.position);
            float distance = Mathf.Abs(itemPos.x - targetPos.x);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestItem = item;
            }
        }

        if (closestItem != null)
        {
            Vector3 closestItemPos = viewPortTransform.InverseTransformPoint(closestItem.position);
            Vector3 targetPos = viewPortTransform.InverseTransformPoint(targetPositionObject.transform.position);
            float adjustment = closestItemPos.x - targetPos.x;

            contentPanelTransform.localPosition -= new Vector3(adjustment, 0, 0);
        }
    }*/

    private bool IsItemCentered(RectTransform item)
    {
        // Get the position of the item and the target position in the viewport
        Vector3 itemPos = viewPortTransform.InverseTransformPoint(item.position);
        Vector3 targetPos = viewPortTransform.InverseTransformPoint(targetPositionObject.transform.position);

        // Check if the item's x position is close to the target position's x position
        return Mathf.Abs(itemPos.x - targetPos.x) < 0.1f;
    }

    private IEnumerator CenterOnClosestItem()
    {
        float closestDistance = float.MaxValue;
        RectTransform closestItem = null;

        foreach (RectTransform item in contentPanelTransform)
        {
            Vector3 itemPos = viewPortTransform.InverseTransformPoint(item.position);
            Vector3 targetPos = viewPortTransform.InverseTransformPoint(targetPositionObject.transform.position);
            float distance = Mathf.Abs(itemPos.x - targetPos.x);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestItem = item;
            }
        }

        if (closestItem != null)
        {
            Vector3 closestItemPos = viewPortTransform.InverseTransformPoint(closestItem.position);
            Vector3 targetPos = viewPortTransform.InverseTransformPoint(targetPositionObject.transform.position);
            float adjustment = closestItemPos.x - targetPos.x;

            Vector3 startPosition = contentPanelTransform.localPosition;
            Vector3 endPosition = contentPanelTransform.localPosition - new Vector3(adjustment, 0, 0);
            float elapsedTime = 0f;

            while (elapsedTime < slerpDuration)
            {
                contentPanelTransform.localPosition = Vector3.Slerp(startPosition, endPosition, elapsedTime / slerpDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            contentPanelTransform.localPosition = endPosition;
        }
    }
}
