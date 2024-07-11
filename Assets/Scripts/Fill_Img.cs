using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class Fill_Img : MonoBehaviour
{
    #region { ::::::::::::::::::::::::: Headers ::::::::::::::::::::::::: }
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------//
    [Header("Script")]
    [SerializeField] GameController controller;
    /*[SerializeField] BonusBallon bonus;*/
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
    [SerializeField] Image GlowEffect;

    public RectTransform bonusObj;
    public GameObject fill_Meter;
    public GameObject numb_Objs;

    public Animator fill_Close;
    public Animator bonusShine;

    public static Fill_Img instance;


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
            /*fill_Img_Dup.fillAmount = fill_Img.fillAmount;*/
            controller.timer = false;
        }

        if (bonusObj.gameObject.activeSelf)
        {
            Timer = controller.bonusTimer;
            TimerUpdate();
        }
    }
    async void TimerUpdate()
    {
        await UniTask.Delay(2500);
        fill_Meter.SetActive(true);
        if (timeRemaining < totalTime)
        {
            if (timeRemaining < Timer)
            {
                timeRemaining += Timer * Time.deltaTime; // Decrease time remaining
                UpdateTimerUI(); // Update the UI
            }
        }
        if (Timer != 10 && !controller.isBonus)
        {
            await UniTask.Delay(2000);
            fill_Close.SetBool("isClose", true);
            Setting_OFF();
        }

        await UniTask.Delay(3500);
        if (Timer >= 10)
        {
            controller.isBonus = true;
            bonusShine.SetBool("isShine", true);
            numb_Objs.SetActive(true);
            GlowEffects();
            timeRemaining = 0f;
            timerBar.fillAmount = 0;
            Timer = 0;
            /*TimeDelay();*/
            /*fill_Img_Dup.fillAmount = fill_Img.fillAmount;*/
        }

    }

    /*async void TimeDelay ()
    {
        await UniTask.Delay(3000);
        if (!controller.isScroll)
        {
            bonusShine.SetBool("isShine", false);
            numb_Objs.SetActive(false);
        }
    }*/

    async void Setting_OFF()
    {
        await UniTask.Delay(200);
        fill_Close.SetBool("isOpen", false);
        fill_Close.SetBool("isClose", false);
        this.gameObject.SetActive(false);
    }
    void GlowEffects()
    {
        // DoTween Text in Sequence
            Sequence sequence = DOTween.Sequence();

        // Add the first scale animation and move animation to the sequence
        sequence.Append(GlowEffect.DOColor(new Color32(255, 255, 255, 255), 1f).SetEase(Ease.Linear));

        // Add a delay of 1 second
        sequence.AppendInterval(0.01f);

        // Add the second scale animation to the sequence
        sequence.Append(GlowEffect.DOColor(new Color32(255, 255, 255, 0), 1f).SetEase(Ease.Linear));
    }
    void UpdateTimerUI()
    {
        float fillAmount = timeRemaining / totalTime;
        timerBar.fillAmount = fillAmount;
    }
    public void CountText_Animation()
    {
        if (scrollView.Count == 0)
        {
            // DoTween Text in Sequence
            Sequence sequence = DOTween.Sequence();

            // Add the first scale animation and move animation to the sequence
            sequence.Append(numbCount_1.DOScale(new Vector3(0f, 0f, 0f), 0.01f).SetEase(Ease.InOutSine));
        }
        else if (scrollView.Count == 1)
        {
            // DoTween Text in Sequence
            Sequence sequence = DOTween.Sequence();

            // Add the first scale animation and move animation to the sequence
            sequence.Append(numbCount_2.DOScale(new Vector3(0f, 0f, 0f), 0.01f).SetEase(Ease.InOutSine))
                    .Join(numbCount_1.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.5f).SetEase(Ease.InOutSine));

            // Add a delay of 1 second
            sequence.AppendInterval(0.01f);

            // Add the second scale animation to the sequence
            sequence.Append(numbCount_1.DOScale(new Vector3(1f, 1f, 1f), 0.5f).SetEase(Ease.InOutSine));
        }
        else if (scrollView.Count == 2)
        {
            // DoTween Text in Sequence
            Sequence sequence = DOTween.Sequence();

            // Add the first scale animation and move animation to the sequence
            sequence.Append(numbCount_3.DOScale(new Vector3(0f, 0f, 0f), 0.01f).SetEase(Ease.InOutSine))
                    .Join(numbCount_2.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.5f).SetEase(Ease.InOutSine));

            // Add a delay of 1 second
            sequence.AppendInterval(0.01f);

            // Add the second scale animation to the sequence
            sequence.Append(numbCount_2.DOScale(new Vector3(1f, 1f, 1f), 0.5f).SetEase(Ease.InOutSine));
        }
        else if (scrollView.Count == 3)
        {
            // DoTween Text in Sequence
            Sequence sequence = DOTween.Sequence();

            // Add the first scale animation and move animation to the sequence
            sequence.Append(numbCount_3.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.5f).SetEase(Ease.InOutSine));

            // Add a delay of 1 second
            sequence.AppendInterval(0.01f);

            // Add the second scale animation to the sequence
            sequence.Append(numbCount_3.DOScale(new Vector3(1f, 1f, 1f), 0.5f).SetEase(Ease.InOutSine));
        }
    }
    private void OnDisable()
    {
        if (Timer >= 10)
        {
            timeRemaining = 0f;
            timerBar.fillAmount = 0;
            Timer = 0;
            /*fill_Img_Dup.fillAmount = fill_Img.fillAmount;*/
        }

        StopAllCoroutines();
    }
}
