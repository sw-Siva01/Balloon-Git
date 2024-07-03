using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class InfiniteScroll : MonoBehaviour
{
    #region { ::::::::::::::::::::::::: Headers ::::::::::::::::::::::::: }
    [Header("GameController Script")]
    public GameController controller;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("ScrollView")]
    public ScrollRect scrollRect;
    public RectTransform viewPortTransform;
    public RectTransform contentPanelTransform;
    public HorizontalLayoutGroup HLG;
    public RectTransform[] ItemList;
    public GameObject targetPositionObject; // Target position GameObject

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    private Vector2 oldVelocity;
    private bool isUpdated;

    [SerializeField] bool autoScroll = false;
    public int Count = 3;
    public float bfloat;
    public float BonusValue;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("Float")]
    [SerializeField] float scrollSpeed = 200f;
    [SerializeField] float decelerationRate = 0.1f;
    [SerializeField] float minScrollTime = 4f;
    [SerializeField] float maxScrollTime = 6f;
    [SerializeField] float slerpDuration = 1.0f; // Duration of the slerp for centering

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("TextMeshProUGUI")]

    public TextMeshProUGUI Bonus_Count_txt;

    //---------------------------------------------------------------------------------------------------------------------------------------------------------------//
    #endregion  ::::::::::::::::::::::::: END :::::::::::::::::::::::::
    private void OnEnable()
    {
        autoScroll = true;
    }
    void Start()
    {
        isUpdated = false;
        oldVelocity = Vector2.zero;

        // Calculate the number of items needed to fill the viewport
        int itemsToAdd = Mathf.CeilToInt(viewPortTransform.rect.width / (ItemList[0].rect.width + HLG.spacing));

        // Add items to the end of the content panel
        for (int i = 0; i < itemsToAdd; i++)
        {
            /*RectTransform rt = Instantiate(ItemList[i % ItemList.Length], contentPanelTransform);
            rt.SetAsLastSibling();*/
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
        Bonus_Count_txt.text = Count.ToString();

        float scrollDuration = Random.Range(minScrollTime, maxScrollTime);
        float elapsedTime = 0f;

        while (elapsedTime < scrollDuration)
        {
            if (Count == 2)
            {
                contentPanelTransform.localPosition -= new Vector3(scrollSpeed * Time.deltaTime, 0, 0);
            }
            else if (Count == 1 || Count == 3)
            {
                contentPanelTransform.localPosition += new Vector3(scrollSpeed * Time.deltaTime, 0, 0);
            }
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

            // Change the text color of the centered item
            TextMeshProUGUI textComponent = closestItem.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                /*textComponent.color = Color.green;*/ // Change to the desired color
                controller.BonusRewardValue = textComponent.text.ToString();
                ///
                string input = controller.BonusRewardValue; // Replace with your actual input
                string numericPart = input.TrimEnd('x'); // Remove the trailing 'x'

                if (float.TryParse(numericPart, out bfloat))
                {
                    // Successfully parsed the numeric part to a float
                    /*Debug.Log($"Parsed value: {bfloat}");*/
                }
                ///

                BonusValue += bfloat;

                controller.BonusRewardTxt.text = textComponent.text.ToString();
                controller.BonusRewardTxt.color = Color.green;

                DelayFuction();
            }
        }
    }
    async void DelayFuction()
    {
        if (Count == 1)
        {
            await UniTask.Delay(1000); // wait for 1 seconds
            controller.winCount = true;
            BounsAmount_Moves();

            await UniTask.Delay(4000); // wait for 4 seconds
            TimerDelay();
            controller.TakeCashOut();
        }
        else if (Count > 0)
        {
            await UniTask.Delay(1000); // wait for 1 seconds
            BounsAmount_Moves();

            await UniTask.Delay(4000); // wait for 4 seconds
            TimeDelay();
        }
    }
    void BounsAmount_Moves()
    {
        Count--;
        // DoTween Text in Sequence
        Sequence sequence = DOTween.Sequence();

        sequence.Append(controller.BonusRewardTxt.transform.DOScale(new Vector3(3f, 3f, 3f), 0.5f).SetEase(Ease.InOutSine))
                .Join(controller.BonusRewardTxt.transform.DOMove(new Vector3(0f, 0.6f, 0f), 1f).SetEase(Ease.InOutSine))
                .Join(controller.BonusRewardTxt.DOColor(new Color32(255, 255, 0, 255), 0.01f).SetEase(Ease.Linear));
        // Add a delay of 1 second
        sequence.AppendInterval(1f);

        Invoke("Object_Delay", 1.6f);

        sequence.Append(controller.BonusRewardTxt.transform.DOScale(new Vector3(1.0008f, 1.0008f, 1.0008f), 0.5f).SetEase(Ease.InOutSine))
                .Join(controller.BonusRewardTxt.transform.DOMove(new Vector3(0f, 3.81f, 0f), 1f).SetEase(Ease.InOutSine))
                .Join(controller.BonusRewardTxt.DOColor(new Color32(95, 255, 0, 255), 0.01f).SetEase(Ease.Linear));
    }
    void TimeDelay()
    {
        autoScroll = true;
    }
    void Object_Delay()
    {
        /*controller.multiplier += bfloat;
        controller.multiplierTxt.text = controller.multiplier.ToString("0.00" + "<size=40>X</size>");*/
        StartCoroutine(Add_BonusValue());
        controller.BonusRewardTxt.text = null;
    }
    void TimerDelay()
    {
        controller.ScrollViewObj.SetActive(false);
        controller.CenterBack_Img.SetActive(false);
        controller.BonusRewardTxt.text = null;
        Count = 3;
    }
    IEnumerator Add_BonusValue()
    {
        while (controller.multiplier < BonusValue)
        {
            controller.multiplier += BonusValue * Time.deltaTime;
            controller.multiplierTxt.text = controller.multiplier.ToString("0.00");
            yield return null;
        }
    }
    private void OnDisable()
    {
        bfloat = 0f;
        BonusValue = 0f;
        controller.winCash_Demo = 0f;
        controller.isScroll = false;
    }
}
