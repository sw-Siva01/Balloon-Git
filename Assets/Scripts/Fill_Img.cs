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
    
    [Header("GameObject")]
    [SerializeField] RectTransform bonusObj;
    [SerializeField] GameObject fill_Meter;
    [SerializeField] GameObject numb_Objs;
    [SerializeField] GameObject scrollViewAnim;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("Bool")]
    public bool bonus;
    public bool bonusTimer;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("Animator")]

    [SerializeField] Animator fill_Close;
    [SerializeField] Animator bonusShine;

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
        if (controller.timer)
        {
            timeRemaining = 0f;
            timerBar.fillAmount = 0;
            Timer = 0;
            controller.timer = false;
        }

        if (bonus)
        {
            Timer = controller.Bonustimer;
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
        // Add a delay of 1 second
        sequence.AppendInterval(2f);
        // Add the second scale animation to the sequence
        sequence.Append(bonusTxt_Img.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.5f).SetEase(Ease.InSine));
        // Add a delay of 1 second
        sequence.AppendInterval(0.01f);
        // Add the second scale animation to the sequence
        sequence.Append(bonusTxt_Img.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InSine));
    }
    public async void TimerUpdate()
    {
        // Initial delay before showing the fill meter
        await UniTask.Delay(2500);
        fill_Meter.SetActive(true);
        bonusTimer = true;

        // Handle case when Timer is not 10 and bonus is not active
        if (Timer != 10 && !controller.isBonus)
        {
            await UniTask.Delay(2000);
            fill_Close.SetBool("isClose", true);
            Setting_OFF();
        }

        // Handle case when Timer is 10 or more
        if (Timer >= 10)
        {
            // Play audio and show animations
            await UniTask.Delay(900);
            audioController.PlayAudio(AudioEnum.bonus);
            bonusShine.SetBool("isShine", true);
            bonus_Txt.SetActive(false);
            BonusTxt_Animation();

            // Activate scroll view animation and set bonus state
            await UniTask.Delay(900);
            scrollViewAnim.SetActive(true);
            controller.isBonus = true;

            // Handle scrolling logic
            if (controller.isBonus && controller.isScroll)
            {
                scrollView.BonusMultiplier_txt.gameObject.SetActive(true);
                numb_Objs.SetActive(true);
                fill_Img.gameObject.SetActive(false);
                timeRemaining = 0f;
                timerBar.fillAmount = 0;
                Timer = 0;
            }

            // Delay for scroll animation and reset state
            await UniTask.Delay(3000);
            scrollViewAnim.SetActive(false);

            // Set bonus multiplier
            controller.Winbonus = 3;
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

        controller.Winbonus = 3;
    }
    void UpdateTimerUI()
    {
        float fillAmount = timeRemaining / totalTime;
        timerBar.fillAmount = fillAmount;
    }
    #region ----- Trying Content -------
    public void CountText_Animation()
    {
        Sequence sequence = DOTween.Sequence();

        // Get the appropriate elements based on the scrollView count
        Transform activeElement = null;
        Transform previousElement = null;

        switch (scrollView.Count)
        {
            case 1:
                activeElement = numbCount_1;
                previousElement = numbCount_2;
                break;
            case 2:
                activeElement = numbCount_2;
                previousElement = numbCount_3;
                break;
            case 3:
                activeElement = numbCount_3;
                break;
        }

        // Play animations
        if (previousElement != null)
        {
            sequence.Append(previousElement.DOScale(Vector3.zero, 0.01f).SetEase(Ease.InOutSine));
        }

        if (activeElement != null)
        {
            sequence.Append(activeElement.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.5f).SetEase(Ease.InOutSine))
                    .AppendInterval(0.01f)
                    .Append(activeElement.DOScale(Vector3.one, 0.5f).SetEase(Ease.InOutSine));
        }
    }

    void SettingOFF()
    {
        // Reset numbCount_1 scale to zero
        numbCount_1.DOScale(Vector3.zero, 0.01f).SetEase(Ease.InOutSine);
    }
    #endregion

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
