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
    public float mValue;
    public string Svalue;

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
    public TextMeshProUGUI BonusMultiplier_txt;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("Bonus_Numb_Count")]

    [SerializeField] RectTransform numbCount_1;
    [SerializeField] RectTransform numbCount_2;
    [SerializeField] RectTransform numbCount_3;

    [SerializeField] Image GlowEffect;

    public MasterAudioController audioController;

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
            CountText_Animation();
            Fill_Img.instance.CountText_Animation();
            // Shuffle the children of the content panel
            ShuffleContentChildren();
            audioController.PlayAudio(AudioEnum.infiniteScrollview, true);
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
            audioController.StopAudio(AudioEnum.infiniteScrollview);
            yield return null;
        }

        /*// Adjust to the nearest centered item after stopping
        CenterOnClosestItem();*/

        // Adjust to the nearest centered item after stopping
        StartCoroutine(CenterOnClosestItem());
    }
    private IEnumerator CenterOnClosestItem()
    {
        audioController.StopAudio(AudioEnum.infiniteScrollview);
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
                    Debug.Log($"Parsed value: {bfloat}");
                }
                ///
                BonusValue = float.Parse(BonusValue.ToString("0.00"));

                BonusValue += bfloat;


                controller.BonusRewardTxt.text = textComponent.text.ToString();
                controller.BonusRewardTxt.color = Color.green;
                //controller.BonusRewardTxt.text = $"+{textComponent.text.ToString()}";
                DelayFuction();
            }
        }
    }

    #region
    /*async void ObjDelayed()
    {
        await UniTask.Delay(1000);
        if (controller.multiplier < BonusValue)
        {
            controller.mString = controller.multiplier.ToString("0.00");

            controller.multiplier += bfloat * Time.deltaTime;

            controller.tString = (controller.betAmount * float.Parse(controller.mString)).ToString("0.00");
            controller.takeCash = float.Parse(controller.tString);
            *//*controller.takeCash = controller.betAmount * controller.multiplier;*//*

            BonusMultiplier_txt.text = controller.multiplier.ToString("0.00");

        }
    }*/
    #endregion
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
    async void BounsAmount_Moves()
    {
        Count--;
        // DoTween Text in Sequence
        Sequence sequence = DOTween.Sequence();

        sequence.Append(controller.BonusRewardTxt.transform.DOScale(new Vector3(3f, 3f, 3f), 0.5f).SetEase(Ease.InOutSine))
                .Join(controller.BonusRewardTxt.transform.DOMove(new Vector3(0f, 0.4f, 0f), 1f).SetEase(Ease.InOutSine))
                /*.Join(controller.BonusRewardTxt.DOColor(new Color32(255, 255, 0, 255), 0.01f).SetEase(Ease.Linear));*/
                .Join(controller.BonusRewardTxt.DOColor(new Color32(110, 0, 0, 255), 0.01f).SetEase(Ease.Linear));
        // Add a delay of 1 second
        sequence.AppendInterval(1f);

        Invoke(nameof(Object_Delay), 1.6f);

        sequence.Append(controller.BonusRewardTxt.transform.DOScale(new Vector3(1.0008f, 1.0008f, 1.0008f), 0.5f).SetEase(Ease.InOutSine))
                .Join(controller.BonusRewardTxt.transform.DOMove(new Vector3(0f, 3.81f, 0f), 1f).SetEase(Ease.InOutSine))
                .Join(controller.BonusRewardTxt.DOColor(new Color32(95, 255, 0, 255), 0.01f).SetEase(Ease.Linear));

        await UniTask.Delay(200);
        controller.BonusRewardTxt.text = $"+{controller.BonusRewardTxt.text.ToString()}";
    }
    void TimeDelay()
    {
        autoScroll = true;
    }
    void Object_Delay()
    {
        StartCoroutine(Add_BonusValue());
        controller.BonusRewardTxt.text = null;
    }
    void TimerDelay()
    {
        controller.ScrollViewObj.SetActive(false);
        controller.CenterBack_Img.SetActive(false);
        controller.BonusRewardTxt.text = null;
        NumbCountSet_OFF();
        Count = 3;
    }
    IEnumerator Add_BonusValue()
    {
        BonusValue = float.Parse(BonusValue.ToString("0.00"));
        controller.multiplier = float.Parse(controller.multiplier.ToString("0.00"));

        while (controller.multiplier < BonusValue)
        {
            controller.mString = controller.multiplier.ToString("0.00");

            if (controller.multiplier < BonusValue)
            {
                controller.multiplier += bfloat * Time.deltaTime;
                controller.multiplier = Mathf.Min(controller.multiplier, BonusValue);
            }

            //controller.tString = (controller.betAmount * float.Parse(controller.mString)).ToString("0.00");
            controller.tString = (controller.betAmount * controller.multiplier).ToString();
            controller.takeCash = float.Parse(controller.tString);

            BonusMultiplier_txt.text = controller.multiplier.ToString("0.00");

            yield return null;
        }
    }
    void NumbCountSet_OFF()
    {
        // DoTween Text in Sequence
        Sequence sequence = DOTween.Sequence();

        // Add the first scale animation and move animation to the sequence
        sequence.Append(numbCount_1.DOScale(new Vector3(0f, 0f, 0f), 0.01f).SetEase(Ease.InOutSine))
            .Join(GlowEffect.DOColor(new Color32(255, 255, 255, 0), 1f).SetEase(Ease.Linear));
    }
    void CountText_Animation()
    {
        // DoTween Text in Sequence
        Sequence sequence = DOTween.Sequence();
        switch (Count)
        {
            case 1:
                // Add the first scale animation and move animation to the sequence
                sequence.Append(numbCount_2.DOScale(new Vector3(0f, 0f, 0f), 0.01f).SetEase(Ease.InOutSine))
                        .Join(numbCount_1.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.5f).SetEase(Ease.InOutSine))
                        .Join(GlowEffect.DOColor(new Color32(255, 255, 255, 255), 1f).SetEase(Ease.Linear));

                // Add a delay of 1 second
                sequence.AppendInterval(0.01f);

                // Add the second scale animation to the sequence
                sequence.Append(numbCount_1.DOScale(new Vector3(1f, 1f, 1f), 0.5f).SetEase(Ease.InOutSine))
                        .Join(GlowEffect.DOColor(new Color32(255, 255, 255, 0), 1f).SetEase(Ease.Linear));
                break;
            case 2:
                // Add the first scale animation and move animation to the sequence
                sequence.Append(numbCount_3.DOScale(new Vector3(0f, 0f, 0f), 0.01f).SetEase(Ease.InOutSine))
                        .Join(numbCount_2.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.5f).SetEase(Ease.InOutSine))
                        .Join(GlowEffect.DOColor(new Color32(255, 255, 255, 255), 1f).SetEase(Ease.Linear));

                // Add a delay of 1 second
                sequence.AppendInterval(0.01f);

                // Add the second scale animation to the sequence
                sequence.Append(numbCount_2.DOScale(new Vector3(1f, 1f, 1f), 0.5f).SetEase(Ease.InOutSine))
                        .Join(GlowEffect.DOColor(new Color32(255, 255, 255, 0), 1f).SetEase(Ease.Linear));
                break;
            case 3:
                // Add the first scale animation and move animation to the sequence
                sequence.Append(numbCount_1.DOScale(new Vector3(0f, 0f, 0f), 0.01f).SetEase(Ease.InOutSine))
                        .Join(numbCount_3.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.5f).SetEase(Ease.InOutSine))
                        .Join(GlowEffect.DOColor(new Color32(255, 255, 255, 255), 1f).SetEase(Ease.Linear));

                // Add a delay of 1 second
                sequence.AppendInterval(0.01f);

                // Add the second scale animation to the sequence
                sequence.Append(numbCount_3.DOScale(new Vector3(1f, 1f, 1f), 0.5f).SetEase(Ease.InOutSine))
                        .Join(GlowEffect.DOColor(new Color32(255, 255, 255, 0), 1f).SetEase(Ease.Linear));
                break;
        }
    }
    private void OnDisable()
    {
        bfloat = 0f;
        BonusValue = 0f;
        controller.winCash_Demo = 0f;
        BonusMultiplier_txt.text = 0.00f.ToString("0.00");
        controller.isScroll = false;
        StopAllCoroutines();
    }
}
