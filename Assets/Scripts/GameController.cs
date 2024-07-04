using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class GameController : MonoBehaviour
{
    #region { ::::::::::::::::::::::::: Headers ::::::::::::::::::::::::: }
    [Header("Float")]
    [SerializeField] float betAmount = 1f;  // Initial bet amount
    public float multiplier = 0.00f;  // Initial multiplier value
    [SerializeField] float incrementRate = 1.01f;  // Increment rate
    [SerializeField] float incrementInterval = 0.2f;  // Time interval between increments in seconds
    [SerializeField] float timeSinceLastIncrement = 0f;   // Timer to track time since last increment
    [SerializeField] float maxHoldTime = 5f; // Maximum time to hold the button
    [SerializeField] float minHoldTime = 2f; // Minimum time to hold the button
    private float timeHeld = 0f;   // Timer to track time button is held
    [SerializeField] float takeCash;  // TakeCash
    private float WinAmount = 0;
    [SerializeField] float TotalAmount = 250.00f;  // Total Amount
    public float bonusTimer;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("Buttons")]
    [SerializeField] Button TakeCashbutton;
    [SerializeField] Button holdButton;
    [SerializeField] Button holdButton_dup;
    [SerializeField] Button empty_holdButton;
    //UI bet Buttons
    [Header("UI bet Buttons")]
    [SerializeField] Button button_1;
    [SerializeField] Button button_2;
    [SerializeField] Button button_5;
    [SerializeField] Button button_10;
    [SerializeField] Button plusButton, minusButton;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    //UI bet Buttons Imgaes
    [Header("UI bet Buttons Images")]
    [SerializeField] Image plusButtomImg;
    [SerializeField] Image minusButtonImg;

    [SerializeField] EventTrigger holdButtonEvent;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    // Boolean
    [Header("Boolean")]
    private bool startGame;
    private bool isPressed;
    private bool takeBetAmount;
    private bool isSet;
    private bool isFire;
    private bool lost;
    private bool take;
    public bool isScroll;
    public bool winCount;
    private bool isBonus_1;
    private bool isBonus_2;
    private bool isBonus_3;
    private bool isNormal;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    // TextMeshProUGUI
    [Header("TextMeshProUGUI")]
    public TextMeshProUGUI multiplierTxt;
    [SerializeField] TextMeshProUGUI Xtxt;
    [SerializeField] TextMeshProUGUI takeCashTxt;
    [SerializeField] TextMeshProUGUI winAmountTxt;
    [SerializeField] TextMeshProUGUI totalAmountTxt;
    // UI Bet Amount txt
    [SerializeField] TextMeshProUGUI betAmountTxt;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    // UI background Image
    [Header("UI background Cloud Image")]
    [SerializeField] RectTransform background;
    [SerializeField] float parallaxAmount = 2000f;
    [SerializeField] float smoothness = 0.05f; // Adjust this value to control smoothness
    private Vector3 initialBackgroundPosition;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    // UI background Image
    [Header("UI background Balloon Image")]
    [SerializeField] RectTransform bg;
    [SerializeField] float parallaxAmt = 10f;
    [SerializeField] float smooth = 0.05f; // Adjust this value to control smoothness
    private Vector3 iniBackgroundPos;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    // Balloon Image
    [Header("Balloon Image")]
    [SerializeField] float speed = 1.0f;
    [SerializeField] Image targetImg;
    [SerializeField] RectTransform target;
    [SerializeField] RectTransform bonus_Balloon;
    [SerializeField] Vector3 targetPosition;  // To move the Target pos Up at the Start

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    // Sliders
    [Header("Sliders")]
    [SerializeField] Slider slider;
    [SerializeField] float countTime;
    private bool timeout;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    // DoTween Text
    [Header("DoTween Text")]
    [SerializeField] RectTransform txtObj;
    [SerializeField] float timeLength = 1f;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    // DoTween Win_Images 
    [Header("DoTween Win_Images")]
    [SerializeField] RectTransform LineObj;
    [SerializeField] Image LineImg;
    [SerializeField] float timeTween = 1f;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    // Bonus Reward Fill Img
    [Header("Bonus Reward Fill Img")]
    [SerializeField] float totalTime = 10f;  // Total time for the timer
    private float timeRemaining;    // Time remaining for the timer
    [SerializeField] Image timerBar;          // Reference to the UI Image representing the timer

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    // Bonuse Rewards Winning int
    [Header("Bonus Rewards Winning int")]
    [SerializeField] float winCash;
    public float winCash_Demo;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    // Bonus GameObjects
    [Header("Bonus GameObjects")]
    [SerializeField] RectTransform bonusObj;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    // ScrollView GameObjects
    [Header("ScrollView GameObjects")]
    public GameObject ScrollViewObj;
    public GameObject CenterBack_Img;
    public string BonusRewardValue;
    public float BonusRewardfloatValue;
    public TextMeshProUGUI BonusRewardTxt;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    // ScrollView GameObjects
    [Header("Animator")]
    [SerializeField] Animator heat_Anim;
    [SerializeField] GameObject heat_Anim_Img;


    #endregion ::::::::::::::::::::::::: END :::::::::::::::::::::::::
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------//
    private void Start()
    {
        // Set the initial multiplier text
        multiplierTxt.text = multiplier.ToString("0.00"/* + "<size=40>X</size>"*/);
        StartCoroutine(HolidngButtons());
        totalAmountTxt.text = TotalAmount.ToString("0.00" + " <size=30>EUR</size>");
        /*TakeCashbutton.enabled = false;
        TakeCashbutton.onClick.AddListener(TakeCashOut);*/
        initialBackgroundPosition = background.localPosition;

        // Sliders
        timeout = false;
        slider.maxValue = 6f;
        slider.minValue = 0f;
        StartCoroutine(TimerCount());

        // Bonus Reward Fill Img
        timeRemaining = 0f; // Start the timer at 0
        UpdateTimerUI(); // Initialize the UI
        StartCoroutine(FillImg());
    }
    IEnumerator FillImg()
    {
        while (true)
        {
            if (timeRemaining >= 10)
            {
                isSet = true;
            }
            yield return null;
        }
    }
    IEnumerator TimerCount()
    {
        while (true)
        {
            if (startGame && multiplier < 1.01f)
            {
                countTime += 7f * Time.deltaTime;
                slider.value = countTime;
            }
            if (startGame && multiplier >= 1.01 && !isPressed)
            {
                timeSinceLastIncrement += Time.deltaTime;

                slider.gameObject.SetActive(true);

                countTime -= 1f * Time.deltaTime;

                slider.value = countTime;

                if (countTime >= 0)
                {
                    timeout = true;
                }
            }
            else if (startGame && multiplier >= 1.01 && isPressed)
            {
                countTime = 7f;
                slider.maxValue = 7f;
                slider.value = 7f;
                slider.gameObject.SetActive(false);
            }

            // Tween Geometric Line Objs
            if (LineObj.gameObject.activeSelf)
            {
                LineObj.Rotate(0, 0, timeTween * Time.deltaTime);
            }

            yield return null;
        }
    }
    IEnumerator HolidngButtons()
    {
        while (true)
        {
            //heat_Anim.SetTrigger("isEnd");

            ApplyBalloonParallaxEffect(holdButton.GetComponent<RectTransform>().anchoredPosition.y);

            if (isScroll)
            {
                ApplyBalloonParallaxEffect(holdButton.GetComponent<RectTransform>().anchoredPosition.y);
                ApplyParallaxEffect(holdButton.GetComponent<RectTransform>().anchoredPosition.y);
            }

            if (lost)
            {
                target.anchoredPosition += Vector2.up * speed * Time.deltaTime;
                holdButtonEvent.enabled = false;
                empty_holdButton.gameObject.SetActive(true);
            }
            if (take)
            {
                target.anchoredPosition += Vector2.up * speed * Time.deltaTime;
            }
            FireButton();
            StartOfTheGame();

            if (isPressed && isFire && !lost)
            {
                heat_Anim.SetTrigger("isEnd");
                heat_Anim_Img.SetActive(true);

                // To move the Target pos Up at the Start
                target.anchoredPosition = Vector3.Lerp(target.anchoredPosition, targetPosition, 0.2f * Time.deltaTime);

                startGame = true;

                if (timeSinceLastIncrement >= incrementInterval)
                {
                    IncrementMultiplier();
                    timeSinceLastIncrement = 0f; // Reset the timer
                }

                if (multiplier >= 1.01)
                {
                    // Increment the time button is held
                    timeHeld += Time.deltaTime;

                    // Check if the button has been held for a random time between min and max hold time
                    if (timeHeld >= Random.Range(minHoldTime, maxHoldTime))
                    {
                        // Bet is lost

                        ResetBets();
                        lost = true;
                        holdButton.enabled = false;
                        /*TakeCashbutton.enabled = false;*/
                        timeSinceLastIncrement = 0f; // Reset the timer
                        timeHeld = 0f; // Reset the time button is held
                        winCash = 0f;
                    }
                }
            }

            // Check if the timer has reached 7 seconds and no button is pressed
            else if (timeSinceLastIncrement >= 7f)
            {
                // Automatically take cash
                TakeCashOut();
                holdButton.enabled = false;
                TakeCashbutton.enabled = false;
                // Reset the timer
                timeSinceLastIncrement = 0f;
                timeHeld = 0f;
            }

            yield return null;
        }
    }
    void FireButton()
    {
        if (isFire)
        {
            holdButton.gameObject.SetActive(true);
            holdButtonEvent.enabled = true;
            holdButton_dup.gameObject.SetActive(false);
        }
        if (!isFire)
        {
            holdButtonEvent.enabled = false;
            holdButton.gameObject.SetActive(false);
            holdButton_dup.gameObject.SetActive(true);
            OnClickUp();
        }
        if (!holdButton_dup.gameObject.activeSelf)
        {
            isSet = false;
        }
    }
    void StartOfTheGame()
    {
        if (multiplier >= 1.01 && !lost)
        {
            holdButton.enabled = true;
            takeCash = betAmount * multiplier;
            takeCashTxt.text = takeCash.ToString("0.00");
        }
        if (multiplier >= 1.01 && !take)
        {
            TakeCashbutton.enabled = true;
        }

        if (startGame && multiplier < 1.01f)
        {
            multiplier += Time.deltaTime;
            multiplierTxt.text = multiplier.ToString("0.00");
        }
        else if (isPressed && startGame && multiplier >= 1.01)
        {
            ApplyParallaxEffect(holdButton.GetComponent<RectTransform>().anchoredPosition.y);
            /*ApplyBalloonParallaxEffect(holdButton.GetComponent<RectTransform>().anchoredPosition.y);*/
            multiplierTxt.text = multiplier.ToString("0.00");
            //holdButtonEvent.enabled = true;
            takeCash = betAmount * multiplier;
            takeCashTxt.text = takeCash.ToString("0.00");

            // Increment the timer by the time elapsed since the last frame
            timeSinceLastIncrement += Time.deltaTime;
        }

        if (takeBetAmount)
        {
            takeBetAmount = false;
            holdButton.enabled = false;
            // Taking the bet amount that we choose
            TotalAmount -= betAmount;
            totalAmountTxt.text = TotalAmount.ToString("0.00" + " <size=30>EUR</size>");
        }
    }
    void IncrementMultiplier()
    {
        if (isPressed && multiplier >= 1.01)
        {
            // Increase the multiplier by the increment rate
            multiplier *= incrementRate;
            // Update the multiplier text
            multiplierTxt.text = multiplier.ToString("0.00");

            // Calculate the win amount
            takeCash = betAmount * multiplier;
            // Update the take cash text
            takeCashTxt.text = takeCash.ToString("0.00");
        }
    }
    public void TakeCashOut()
    {
        take = true;
        startGame = false;
        WinAmount = takeCash;
        // Change the text color
        multiplierTxt.text = multiplier.ToString("0.00");
        multiplierTxt.color = Color.green;
        Xtxt.color = Color.green;
        winAmountTxt.gameObject.SetActive(true);
        winAmountTxt.text = WinAmount.ToString("+" + "0.00");
        TotalAmount += WinAmount;
        totalAmountTxt.text = TotalAmount.ToString("0.00" + " <size=30>EUR</size>");
        holdButton.enabled = false;
        LineObj.gameObject.SetActive(true);
        holdButtonEvent.enabled = false;
        slider.gameObject.SetActive(false);
        empty_holdButton.gameObject.SetActive(true);

        #region :::::::::::::::::::::::::: DoTween sequence ::::::::::::::::::::::::::
        // DoTween Text in Sequence
        Sequence sequence = DOTween.Sequence();

        sequence.AppendInterval(0.01f);

        // Add the first scale animation and move animation to the sequence
        sequence.Append(txtObj.DOScale(new Vector3(1.7f, 1.7f, 1.7f), timeLength).SetEase(Ease.InOutSine))
                .Join(txtObj.DOMove(new Vector3(0f, -2.8f, 0f), 1.3f).SetEase(Ease.InOutSine))
                .Join(LineImg.DOColor(new Color32(255, 255, 255, 255), timeLength).SetEase(Ease.Linear));

        // Add a delay of 1 second
        sequence.AppendInterval(0.01f);

        // Add the second scale animation to the sequence
        sequence.Append(txtObj.DOScale(new Vector3(0f, 0f, 0f), timeLength))
                //.Join(txtObj.DOMove(new Vector3(0f, -2.2f, 0f), timeLength).SetEase(Ease.InOutSine))
                .Join(txtObj.DOMove(new Vector3(0f, -3f, 0f), timeLength).SetEase(Ease.InOutSine))  // new Position
                .Join(LineImg.DOColor(new Color32(255, 255, 255, 0), timeLength).SetEase(Ease.Linear));
        // Start the sequence
        sequence.Play();

        #endregion  :::::::::::::::::::::::::: END ::::::::::::::::::::::::::

        if (!winCount)
        {
            winCash++;
        }
        else
        {
            isNormal = true;
        }

        Demo_Bonus();
        Call_Functions();
        DelayFuction();
    }
    void Call_Functions()
    {
        if (winCash == 3)
        {
            isBonus_1 = true;
            if (!isSet)
                isBonus_2 = true;
            else if (isSet)
                isBonus_3 = true;
        }

        if (winCash != 3)
            isNormal = true;

    }
    async void DelayFuction()
    {
        if (isBonus_1)
        {
            await UniTask.Delay(2500);
            Bonus_Conditions();
        }
        if (isBonus_2)
        {
            await UniTask.Delay(6000); // wait for 5 seconds
            TimeDelay_WinCash();
        }

        if (isNormal)
        {
            await UniTask.Delay(2500); // wait for 2.5 seconds
            TimeDelay();
        }

        if (isBonus_3)
        {
            await UniTask.Delay(5500); // wait for 2.5 seconds
            Bonus_Delay();
        }
    }
    void TimeDelay()
    {
        multiplierTxt.color = Color.white;
        Xtxt.color = Color.white;
        multiplier = 0f;
        multiplierTxt.text = multiplier.ToString("0.00");
        takeCash = 0f;
        takeCashTxt.text = takeCash.ToString("0.00");
        winAmountTxt.gameObject.SetActive(false);
        WinAmount = 0;
        winAmountTxt.text = WinAmount.ToString("0.00");
        holdButton.enabled = true;
        holdButtonEvent.enabled = true;
        empty_holdButton.gameObject.SetActive(false);
        take = false;
        lost = false;
        isNormal = false;
        background.localPosition = initialBackgroundPosition;
        bg.localPosition = iniBackgroundPos;

        if (winCount == true)
        {
            targetImg.enabled = true;
            bonus_Balloon.gameObject.SetActive(false);
            winCount = false;
        }

        target.localPosition = new Vector3(0f, -152f, 0f);
        LineObj.gameObject.SetActive(false);

        countTime = 0f;
        slider.value = 0f;
        slider.gameObject.SetActive(true);

        // DoTween Text
        txtObj.DOScale(new Vector3(1f, 1f, 1f), 0.01f);
        /*txtObj.DOMove(new Vector3(0f, 0f, 0f), 0.01f);*/
        txtObj.DOMove(new Vector3(0f, 0.27f, 0f), 0.01f);   // new Position

        button_1.gameObject.SetActive(false);
        button_2.gameObject.SetActive(false);
        button_5.gameObject.SetActive(false);
        button_10.gameObject.SetActive(false);
    }
    void TimeDelay_WinCash()
    {
        multiplierTxt.color = Color.white;
        Xtxt.color = Color.white;
        multiplier = 0f;
        multiplierTxt.text = multiplier.ToString("0.00");
        takeCash = 0f;
        takeCashTxt.text = takeCash.ToString("0.00");
        winAmountTxt.gameObject.SetActive(false);
        WinAmount = 0;
        winAmountTxt.text = WinAmount.ToString("0.00");
        holdButton.enabled = true;
        holdButtonEvent.enabled = true;
        empty_holdButton.gameObject.SetActive(false);
        take = false;
        lost = false;
        isBonus_2 = false;
        background.localPosition = initialBackgroundPosition;
        bg.localPosition = iniBackgroundPos;

        target.localPosition = new Vector3(0f, -152f, 0f);
        LineObj.gameObject.SetActive(false);

        countTime = 0f;
        slider.value = 0f;
        slider.gameObject.SetActive(true);

        // DoTween Text
        txtObj.DOScale(new Vector3(1f, 1f, 1f), 0.01f);
        txtObj.DOMove(new Vector3(0f, 0.27f, 0f), 0.01f);   // new Position

        button_1.gameObject.SetActive(false);
        button_2.gameObject.SetActive(false);
        button_5.gameObject.SetActive(false);
        button_10.gameObject.SetActive(false);
    }
    void ResetBets()
    {
        isPressed = false;
        startGame = false;
        multiplierTxt.text = multiplier.ToString("0.00");
        // Change the text color
        multiplierTxt.color = Color.black;
        Xtxt.color = Color.black;

        Invoke("TimeDelay", 1.5f);
    }
    #region ::::::::::::::::::::::::::: Bonus Functions :::::::::::::::::::::::::::
    void Bonus_Delay()
    {
        multiplierTxt.color = Color.white;
        Xtxt.color = Color.white;
        multiplier = 0f;
        multiplierTxt.text = multiplier.ToString("0.00");
        txtObj.gameObject.SetActive(true);
        takeCash = 0f;
        takeCashTxt.text = takeCash.ToString("0.00");
        winAmountTxt.gameObject.SetActive(false);
        WinAmount = 0;
        winAmountTxt.text = WinAmount.ToString("0.00");
        LineObj.gameObject.SetActive(false);
        isBonus_3 = false;
        take = false;

        target.localPosition = new Vector3(0f, 0f, 0f);
        targetImg.enabled = false;
        /*bonus_Balloon.localPosition = new Vector3(0f, -130f, 0f);*/
        bonus_Balloon.gameObject.SetActive(true);
        // DoTween Text
        txtObj.DOScale(new Vector3(1f, 1f, 1f), 0.01f);
        txtObj.DOMove(new Vector3(0f, 1f, 0f), 0.01f);
        ScrollViewer();
    }
    async void ScrollViewer()
    {
        await UniTask.Delay(1000); // wait for 2.5 seconds
        ScrollView_Conditions();
    }
    void Bonus_Conditions()
    {
        if (winCash == 3)
        {
            bonusObj.gameObject.SetActive(true);

            // Bonus Reward Fill Img
            if (timeRemaining < totalTime)
            {
                if (betAmount <= 5f)
                {
                    if (betAmount <= 1f)
                    {
                        bonusTimer += 10;
                        /*timeRemaining += 8f; // increase time remaining
                        UpdateTimerUI(); // Update the UI*/
                    }
                    if (betAmount > 1f)
                    {
                        bonusTimer += 4;
                    }
                    if (betAmount > 2f)
                    {
                        bonusTimer += 2;
                    }

                }
            }
            else
            {
                // Timer has reached the total time
                timeRemaining = totalTime; // Ensure timeRemaining does not exceed totalTime
                UpdateTimerUI(); // Update the UI to reflect that the timer is full
                isScroll = true;
            }
            winCash = 0;
            isBonus_1 = false;
        }
    }
    void Demo_Bonus()
    {
        if (winCash == 3)
        {
            if (betAmount <= 5f)
            {
                if (betAmount <= 1f)
                {
                    winCash_Demo += 10f;
                }
                if (betAmount > 1f)
                {
                    winCash_Demo += 4f;
                }
                if (betAmount > 2f)
                {
                    winCash_Demo += 2f;
                }
            }
        }
        if (winCash_Demo >= 10)
        {
            isSet = true;
            isScroll = true;
            bonusTimer = 0f;
        }
    }
    void ScrollView_Conditions()
    {
        ScrollViewObj.SetActive(true);
        CenterBack_Img.SetActive(true);
        timeRemaining = 0;
        isSet = false;
    }
    #endregion

    #region ::::::::::::::::::::::::::: Event Trigger Buttons :::::::::::::::::::::::::::
    public void HoldButton()
    {
        takeBetAmount = true;
        betAmountTxt.text = betAmount.ToString("0.00" + " <size=30>EUR</size>");
    }
    public void OnClickDown()
    {
        isPressed = true;
        ApplyParallaxEffect(holdButton.GetComponent<RectTransform>().anchoredPosition.y);
    }
    public void OnClickUp()
    {
        isPressed = false;
        /*ApplyParallaxEffect(holdButton.GetComponent<RectTransform>().anchoredPosition.y);*/
    }
    public void Button_ONEnter()
    {
        isFire = true;
    }
    public void Button_OFFEnter()
    {
        isFire = false;
    }
    #endregion

    #region ::::::::::::::::::::::::::: Bonus Reward Fill Img :::::::::::::::::::::::::::
    void UpdateTimerUI()
    {
        float fillAmount = timeRemaining / totalTime;
        timerBar.fillAmount = fillAmount;
    }
    #endregion

    #region { ::::::::::::::::::::::::: Buttons ::::::::::::::::::::::::: }
    public void BetButton_1()
    {
        if (!startGame)
        {
            betAmount = 1f;
            betAmountTxt.text = betAmount.ToString("0.00" + " <size=30>EUR</size>");
            button_1.gameObject.SetActive(true);
            button_2.gameObject.SetActive(false);
            button_5.gameObject.SetActive(false);
            button_10.gameObject.SetActive(false);

            plusButton.enabled = true;
            minusButton.enabled = true;
            plusButtomImg.color = new Color32(255, 255, 255, 255);
            minusButtonImg.color = new Color32(255, 255, 255, 255);
        }
    }
    public void BetButton_2()
    {
        if (!startGame)
        {
            betAmount = 2f;
            betAmountTxt.text = betAmount.ToString("0.00" + " <size=30>EUR</size>");
            button_1.gameObject.SetActive(false);
            button_2.gameObject.SetActive(true);
            button_5.gameObject.SetActive(false);
            button_10.gameObject.SetActive(false);

            plusButton.enabled = true;
            minusButton.enabled = true;
            plusButtomImg.color = new Color32(255, 255, 255, 255);
            minusButtonImg.color = new Color32(255, 255, 255, 255);
        }
    }
    public void BetButton_5()
    {
        if (!startGame)
        {
            betAmount = 5f;
            betAmountTxt.text = betAmount.ToString("0.00" + " <size=30>EUR</size>");
            button_1.gameObject.SetActive(false);
            button_2.gameObject.SetActive(false);
            button_5.gameObject.SetActive(true);
            button_10.gameObject.SetActive(false);

            plusButton.enabled = true;
            minusButton.enabled = true;
            plusButtomImg.color = new Color32(255, 255, 255, 255);
            minusButtonImg.color = new Color32(255, 255, 255, 255);
        }
    }
    public void BetButton_10()
    {
        if (!startGame)
        {
            betAmount = 10f;
            betAmountTxt.text = betAmount.ToString("0.00" + " <size=30>EUR</size>");
            button_1.gameObject.SetActive(false);
            button_2.gameObject.SetActive(false);
            button_5.gameObject.SetActive(false);
            button_10.gameObject.SetActive(true);

            plusButton.enabled = true;
            minusButton.enabled = true;
            plusButtomImg.color = new Color32(255, 255, 255, 255);
            minusButtonImg.color = new Color32(255, 255, 255, 255);
        }
    }

    // Select bet buttons
    public void SelectBetButton_1()
    {
        if (!startGame)
        {
            if (betAmount < 100f)
            {
                betAmount += 1f;
                betAmountTxt.text = betAmount.ToString("0.00" + " <size=30>EUR</size>");
                plusButton.enabled = true;
                minusButton.enabled = true;
                plusButtomImg.color = new Color32(255, 255, 255, 255);
                minusButtonImg.color = new Color32(255, 255, 255, 255);
            }

            if (betAmount >= 100)
            {
                betAmount = 100f;
                betAmountTxt.text = betAmount.ToString("0.00" + " <size=30>EUR</size>");
                plusButton.enabled = false;
                plusButtomImg.color = new Color32(255, 255, 255, 120);
            }
        }
    }
    public void SelectBetButton_2()
    {
        if (!startGame)
        {
            if (betAmount < 100f)
            {
                betAmount += 2f;
                betAmountTxt.text = betAmount.ToString("0.00" + " <size=30>EUR</size>");
                plusButton.enabled = true;
                minusButton.enabled = true;
                plusButtomImg.color = new Color32(255, 255, 255, 255);
                minusButtonImg.color = new Color32(255, 255, 255, 255);
            }

            if (betAmount >= 100)
            {
                betAmount = 100f;
                betAmountTxt.text = betAmount.ToString("0.00" + " <size=30>EUR</size>");
                plusButton.enabled = false;
                plusButtomImg.color = new Color32(255, 255, 255, 120);
            }
        }
    }
    public void SelectBetButton_5()
    {
        if (!startGame)
        {
            if (betAmount < 100f)
            {
                betAmount += 5f;
                betAmountTxt.text = betAmount.ToString("0.00" + " <size=30>EUR</size>");
                plusButton.enabled = true;
                minusButton.enabled = true;
                plusButtomImg.color = new Color32(255, 255, 255, 255);
                minusButtonImg.color = new Color32(255, 255, 255, 255);
            }

            if (betAmount >= 100)
            {
                betAmount = 100f;
                betAmountTxt.text = betAmount.ToString("0.00" + " <size=30>EUR</size>");
                plusButton.enabled = false;
                plusButtomImg.color = new Color32(255, 255, 255, 120);
            }
        }
    }
    public void SelectBetButton_10()
    {
        if (!startGame)
        {
            if (betAmount < 100f)
            {
                betAmount += 10f;
                betAmountTxt.text = betAmount.ToString("0.00" + " <size=30>EUR</size>");
                plusButton.enabled = true;
                minusButton.enabled = true;
                plusButtomImg.color = new Color32(255, 255, 255, 255);
                minusButtonImg.color = new Color32(255, 255, 255, 255);
            }

            if (betAmount >= 100)
            {
                betAmount = 100f;
                betAmountTxt.text = betAmount.ToString("0.00" + " <size=30>EUR</size>");
                plusButton.enabled = false;
                plusButtomImg.color = new Color32(255, 255, 255, 120);
            }
        }
    }
    public void PlusButton()
    {
        if (!startGame)
        {
            if (betAmount < 100)
            {
                betAmount += 0.10f;
                betAmountTxt.text = betAmount.ToString("0.00" + " <size=30>EUR</size>");
                minusButton.enabled = true;
                minusButtonImg.color = new Color32(255, 255, 255, 255);
            }
            if (betAmount >= 100)
            {
                betAmount = 100f;
                betAmountTxt.text = betAmount.ToString("0.00" + " <size=30>EUR</size>");
                plusButton.enabled = false;
                plusButtomImg.color = new Color32(255, 255, 255, 120);
            }
        }
    }
    public void MinusButton()
    {
        if (!startGame)
        {
            if (betAmount > 0.10f)
            {
                betAmount -= 0.10f;
                betAmountTxt.text = betAmount.ToString("0.00" + " <size=30>EUR</size>");
                plusButtomImg.color = new Color32(255, 255, 255, 255);
            }
            if (betAmount <= 0.10f)
            {
                betAmount = 0.10f;
                betAmountTxt.text = betAmount.ToString("0.00" + " <size=30>EUR</size>");
                minusButton.enabled = false;
                minusButtonImg.color = new Color32(255, 255, 255, 120);
            }
        }
    }
    #endregion

    #region { ::::::::::::::::::::::::: ParallaxEffect ::::::::::::::::::::::::: }
    void ApplyParallaxEffect(float holdButtonYPosition)     // Moving BackGround Main Image
    {
        // Calculate the target position for the background
        Vector3 targetBackgroundPosition = initialBackgroundPosition;
        targetBackgroundPosition.y -= holdButtonYPosition + parallaxAmount;

        // Smoothly interpolate towards the target position over time
        background.localPosition = Vector3.Lerp(background.localPosition, targetBackgroundPosition, Time.deltaTime * smoothness);
    }

    void ApplyBalloonParallaxEffect(float holdButtonYPos)    // Moving Balloon Image
    {
        // Calculate the target position for the background
        Vector3 targetBackgroundPosition = iniBackgroundPos;
        targetBackgroundPosition.y += holdButtonYPos + parallaxAmt;

        // Smoothly interpolate towards the target position over time
        bg.localPosition = Vector3.Lerp(bg.localPosition, targetBackgroundPosition, Time.deltaTime * smooth);
    }
    #endregion
}