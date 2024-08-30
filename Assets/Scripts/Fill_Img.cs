using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using TMPro;

public class Fill_Img : MonoBehaviour
{
    #region { ::::::::::::::::::::::::: Headers ::::::::::::::::::::::::: }
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------//
    [Header("Script")]
    [SerializeField] GameController controller;
    [SerializeField] InfiniteScroll scrollView;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("Image")]
    [SerializeField] Image fill_Img;
    [SerializeField] Image timerBar;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("Float")]
    [SerializeField] float totalTime = 10f;  // Total time for the timer
    [SerializeField] float timeRemaining;    // Time remaining for the timer
    public float Timer;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("Bonus_Numb_Count")]

    [SerializeField] RectTransform numbCount_1;
    [SerializeField] RectTransform numbCount_2;
    [SerializeField] RectTransform numbCount_3;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("Text Objects")]
    [SerializeField] GameObject bonus_Txt;
    [SerializeField] RectTransform bonusTxt_Img;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]


    public RectTransform bonusObj;
    public GameObject fill_Meter;
    public GameObject numb_Objs;
    public GameObject scrollViewAnim;

    public bool bonus;
    public bool bonusTimer;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("Animator")]

    public Animator fill_Close;
    public Animator bonusShine;

    public static Fill_Img instance;

    public MasterAudioController audioController;
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------//
    #endregion ::::::::::::::::::::::::: END :::::::::::::::::::::::::

    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        timeRemaining = 0f;
    }
    void Update()
    {
        /*Timer = controller.bonusTimer;*/

        if (controller.timer)
        {
            timeRemaining = 0f;
            timerBar.fillAmount = 0;
            Timer = 0;
            controller.timer = false;
        }

        if (/*bonusObj.gameObject.activeSelf*/ bonus)
        {
            Timer = controller.bonusTimer;
            /*TimerUpdate();*/
        }

        if (bonusTimer)
        {
            if (timeRemaining < totalTime)
            {
                if (timeRemaining < Timer)
                {
                    timeRemaining += Timer * Time.deltaTime; // Decrease time remaining
                    UpdateTimerUI(); // Update the UI
                }
            }
        }

        if (controller.isBonus_OFF)
        {
            ResetTimer();
        }
    }
    void ResetTimer()
    {
        if (controller.isBonus && !controller.isScroll)
        {
            numb_Objs.SetActive(false);
            bonusShine.SetBool("isShine", false);
            fill_Close.SetBool("isClose", true);
            controller.isBonus = false;
            controller.isBonus_OFF = false;
            fill_Meter.SetActive(false);
            SettingOFF();
            TimeDelay();
        }
    }
    async void TimeDelay()
    {
        await UniTask.Delay(1000);
        Debug.Log("Bonus_Delay =========> 3");
        fill_Close.SetBool("isOpen", false);
        fill_Close.SetBool("isClose", false);
        fill_Img.gameObject.SetActive(true);
        scrollView.BonusMultiplier_txt.gameObject.SetActive(false);
        this.gameObject.SetActive(false);
    }
    public void Bonus_Script()
    {
        bonus = true;
        if (bonus)
        {
            Bonus_Txt();
            TimerUpdate();
        }
    }
    public async void Bonus_Txt()
    {
        await UniTask.Delay(1000);
        bonus_Txt.SetActive(true);
    }
    public void BonusTxt_Animation()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(bonusTxt_Img.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.5f).SetEase(Ease.InSine));

        // Add a delay of 1 second
        sequence.AppendInterval(0.01f);

        // Add the second scale animation to the sequence
        sequence.Append(bonusTxt_Img.DOScale(new Vector3(1f, 1f, 1f), 0.5f).SetEase(Ease.OutBounce));

        sequence.AppendInterval(2f);

        sequence.Append(bonusTxt_Img.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.5f).SetEase(Ease.InSine));

        // Add a delay of 1 second
        sequence.AppendInterval(0.01f);

        // Add the second scale animation to the sequence
        sequence.Append(bonusTxt_Img.DOScale(new Vector3(0f, 0f, 0f), 0.5f).SetEase(Ease.InSine));
    }
    public async void TimerUpdate()
    {
        await UniTask.Delay(2500);
        fill_Meter.SetActive(true);
        bonusTimer = true;

        if (Timer != 10 && !controller.isBonus)
        {
            await UniTask.Delay(2000);
            fill_Close.SetBool("isClose", true);
            Setting_OFF();
        }

        if (Timer >= 10)
        {
            await UniTask.Delay(900);
            audioController.PlayAudio(AudioEnum.bonus);
            bonusShine.SetBool("isShine", true);
            bonus_Txt.SetActive(false);
            BonusTxt_Animation();

            await UniTask.Delay(900);
            scrollViewAnim.SetActive(true);
            controller.isBonus = true;


            if (controller.isBonus && controller.isScroll)
            {
                scrollView.BonusMultiplier_txt.gameObject.SetActive(true);
                numb_Objs.SetActive(true);
                fill_Img.gameObject.SetActive(false);
                timeRemaining = 0f;
                timerBar.fillAmount = 0;
                Timer = 0;
            }

            await UniTask.Delay(3000);
            scrollViewAnim.SetActive(false);

            if (!APIController.instance.userDetails.isBlockApiConnection)
            {
                controller.winBonus = UnityEngine.Random.Range(10, 20);      // use this always
              //controller.winBonus = 3;                                    // for testing only
            }
            else if (APIController.instance.userDetails.isBlockApiConnection)
            {
                controller.winBonus = UnityEngine.Random.Range(5, 8);
            }

        }
    }
    async void Setting_OFF()
    {
        await UniTask.Delay(200);
        fill_Close.SetBool("isOpen", false);
        bonus_Txt.SetActive(false);

        await UniTask.Delay(200);
        fill_Close.SetBool("isClose", false);

        fill_Meter.SetActive(false);

        await UniTask.Delay(1000);
        bonusObj.gameObject.SetActive(false);

        if (!APIController.instance.userDetails.isBlockApiConnection)
        {
            controller.winBonus = UnityEngine.Random.Range(8, 15);       // use this always
          //controller.winBonus = 3;                                    // for testing only
        }
        else if (APIController.instance.userDetails.isBlockApiConnection)
        {
            controller.winBonus = UnityEngine.Random.Range(8, 15);
        }
    }
    void UpdateTimerUI()
    {
        float fillAmount = timeRemaining / totalTime;
        timerBar.fillAmount = fillAmount;
    }
    public void CountText_Animation()
    {
        Sequence sequence = DOTween.Sequence();
        switch (scrollView.Count)
        {
            case 1:
                // Add the first scale animation and move animation to the sequence
                sequence.Append(numbCount_2.DOScale(new Vector3(0f, 0f, 0f), 0.01f).SetEase(Ease.InOutSine))
                        .Join(numbCount_1.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.5f).SetEase(Ease.InOutSine));

                // Add a delay of 1 second
                sequence.AppendInterval(0.01f);

                // Add the second scale animation to the sequence
                sequence.Append(numbCount_1.DOScale(new Vector3(1f, 1f, 1f), 0.5f).SetEase(Ease.InOutSine));
                break;
            case 2:
                // Add the first scale animation and move animation to the sequence
                sequence.Append(numbCount_3.DOScale(new Vector3(0f, 0f, 0f), 0.01f).SetEase(Ease.InOutSine))
                        .Join(numbCount_2.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.5f).SetEase(Ease.InOutSine));

                // Add a delay of 1 second
                sequence.AppendInterval(0.01f);

                // Add the second scale animation to the sequence
                sequence.Append(numbCount_2.DOScale(new Vector3(1f, 1f, 1f), 0.5f).SetEase(Ease.InOutSine));
                break;
            case 3:
                // Add the first scale animation and move animation to the sequence
                sequence.Append(numbCount_3.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.5f).SetEase(Ease.InOutSine));

                // Add a delay of 1 second
                sequence.AppendInterval(0.01f);

                // Add the second scale animation to the sequence
                sequence.Append(numbCount_3.DOScale(new Vector3(1f, 1f, 1f), 0.5f).SetEase(Ease.InOutSine));
                break;
        }
    }
    void SettingOFF()
    {
        // DoTween Text in Sequence
        Sequence sequence = DOTween.Sequence();
        // Add the first scale animation and move animation to the sequence
        sequence.Append(numbCount_1.DOScale(new Vector3(0f, 0f, 0f), 0.01f).SetEase(Ease.InOutSine));
    }
    private void OnDisable()
    {
        if (Timer >= 10)
        {
            timeRemaining = 0f;
            timerBar.fillAmount = 0;
            Timer = 0;
        }
        bonus_Txt.SetActive(false);
        scrollViewAnim.SetActive(false);
        bonus = false;
        bonusTimer = false;
        fill_Meter.SetActive(false);

        StopAllCoroutines();
    }
}
