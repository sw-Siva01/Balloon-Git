using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;

public class GameController : MonoBehaviour
{
    #region { ::::::::::::::::::::::::: Headers ::::::::::::::::::::::::: }
    [Header("Float")]
    [SerializeField] public float betAmount = 1f;  // Initial bet amount
    public float multiplier = 0.00f;  // Initial multiplier value
    [SerializeField] float incrementRate = 1.01f;  // Increment rate
    [SerializeField] float incrementInterval = 0.2f;  // Time interval between increments in seconds
    [SerializeField] float timeSinceLastIncrement = 0f;   // Timer to track time since last increment
    [SerializeField] float maxHoldTime = 5f; // Maximum time to hold the button
    [SerializeField] float minHoldTime = 2f; // Minimum time to hold the button
    private float timeHeld = 0f;   // Timer to track time button is held
    [SerializeField] float takeCash;  // TakeCash
    /*public float WinAmount = 0;*/
    [SerializeField] float TotalAmount = 250.00f;  // Total Amount
    public float bonusTimer;
    [SerializeField] KeyBoardHandler keyBoard;

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

    [SerializeField] GameObject[] button_Anim;
    [SerializeField] GameObject numPadButton;
    [SerializeField] GameObject TakeCashAnimImg;
    [SerializeField] TextMeshProUGUI TakeCashtxt;
    [SerializeField] Image TakeCashImg;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    //UI bet Buttons Imgaes
    [Header("UI bet Buttons Images")]
    [SerializeField] Image plusButtomImg;
    [SerializeField] Image minusButtonImg;

    [SerializeField] EventTrigger holdButtonEvent;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    // Boolean
    [Header("Boolean")]
    public bool isScroll;
    public bool winCount;
    public bool timer;
    public bool numPad;
    public bool isBonus;
    public bool isBonus_OFF;
    public bool makeLose;
    public bool startGame;
    private bool isPressed;
    private bool takeBetAmount;
    private bool isSet;
    private bool isFire;
    private bool lost;
    private bool take;
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
    [SerializeField] TextMeshProUGUI totalAmountTxt;

    // UI Bet Amount txt
    public TextMeshProUGUI betAmountTxt;

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

    [SerializeField] RectTransform target;
    [SerializeField] GameObject ballonOut;
    [SerializeField] RectTransform TxtObjs;

    [SerializeField] GameObject balloonBlue_Start;
    [SerializeField] GameObject balloonParts;
    [SerializeField] GameObject balloonShake;
    [SerializeField] GameObject balloonShake_blue;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    // Sliders
    [Header("Sliders")]
    [SerializeField] Slider slider;
    [SerializeField] float countTime;
    [SerializeField] GameObject slider_bg;
    [SerializeField] GameObject fillArea;
    [SerializeField] Image FillImage;
    [SerializeField] GameObject slider_txt;
    [SerializeField] TextMeshProUGUI sliderTxt;
    [SerializeField] TextMeshProUGUI sliderDupTxt;
    [SerializeField] TextMeshProUGUI sliderAutoCashTxt;
    [SerializeField] TextMeshProUGUI sliderAutoCashNoTxt;
    /*private bool timeout;*/

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    // DoTween Win_Images 
    [Header("DoTween Win_Images")]
    [SerializeField] GameObject winPanel;
    [SerializeField] RectTransform winObj;
    [SerializeField] Image winImg;
    [SerializeField] TextMeshProUGUI winTxt;
    /*[SerializeField] float timeTween = 1f;*/

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    // Bonus Reward Fill Img
    [Header("Bonus Reward Fill Img")]
    [SerializeField] float totalTime = 10f;  // Total time for the timer
    private float timeRemaining;    // Time remaining for the timer
    /*[SerializeField] Image timerBar;          // Reference to the UI Image representing the timer*/

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    // Bonuse Rewards Winning int
    [Header("Bonus Rewards Winning int")]
    [SerializeField] float winCash;
    public float winBonus;
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
    [SerializeField] Animator ballon_Anim;

    // Heat button
    [SerializeField] Animator heat_Anim;
    [SerializeField] GameObject heat_Anim_Img;

    // slider
    [SerializeField] Animator slider_Anim;

    [SerializeField] Animator bonusBallon;

    // takeCash button
    [SerializeField] Animator takecashOut;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    // ScrollView GameObjects
    [Header("Insufficient Balance")]
    [SerializeField] GameObject InsufficientBalance;
    [SerializeField] GameObject cancelButton;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    // ScrollView GameObjects
    [Header("API Controller")]
    public string currencyType;
    public string playerID;
    public string playerName;
    public GameObject DemoText;

    public string operatorName;
    public string gameName;
    public string lobbyName;
    public string betID;
    public int BetIndex;
    public CreateMatchResponse MatchRes;
    public GameObject internetDisconnectPannel;

    public static GameController instance;

    #endregion ::::::::::::::::::::::::: END :::::::::::::::::::::::::
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------//
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        takeBetAmount = true;
        button_1.gameObject.SetActive(true);
        TakeCashImg.color = new Color32(140, 140, 140, 255);
        TakeCashtxt.color = new Color32(140, 140, 140, 255);
        TakeCashAnimImg.SetActive(false);
        // Set the initial multiplier text
        multiplierTxt.text = multiplier.ToString("0.00"/* + "<size=40>X</size>"*/);
        StartCoroutine(HolidngButtons());
        /*totalAmountTxt.text = TotalAmount.ToString("0.00" + " <size=35>USD</size>");*/
        /*TakeCashbutton.enabled = false;
        TakeCashbutton.onClick.AddListener(TakeCashOut);*/
        initialBackgroundPosition = background.localPosition;

        //winBonus = UnityEngine.Random.Range(3, 6);    // use this always
        winBonus = 3;                                   // only for testing
        ButtonSelect_Anim();
        // Sliders
        /*timeout = false;*/
        slider.maxValue = 8f;
        slider.minValue = 1f;
        StartCoroutine(TimerCount());

        // Bonus Reward Fill Img
        timeRemaining = 0f; // Start the timer at 0
        /* UpdateTimerUI(); // Initialize the UI*/
        StartCoroutine(FillImg());

        // sliderOBjs
        slider_bg.SetActive(false);
        fillArea.SetActive(false);
        slider_txt.SetActive(false);
        sliderAutoCashNoTxt.gameObject.SetActive(false);

        APIController.instance.OnUserDetailsUpdate += InitPlayerDetails;
        APIController.instance.OnUserBalanceUpdate += InitAmountDetails;
        APIController.instance.OnUserDeposit += InitUserDeposit;
    }
    #region { ::::::::::::::::::::::::: API ::::::::::::::::::::::::: }
    public void InitPlayerDetails()
    {
        if (APIController.instance.userDetails.isBlockApiConnection)
        {
            HideOrShowObject(DemoText, true);
            Debug.Log("demo true");
        }
        else
        {
            HideOrShowObject(DemoText, false);
            Debug.Log("demo false");
        }

        playerID = APIController.instance.userDetails.Id;
        playerName = APIController.instance.userDetails.name;
        //PassTxt(TowerUIController.instance.SettingsPanel.playerName, playerName);

        operatorName = APIController.instance.userDetails.game_Id.Split("_")[0].ToString();
        gameName = APIController.instance.userDetails.game_Id.Split("_")[1].ToString();
        lobbyName = "Room : " + DateTime.UtcNow + UnityEngine.Random.Range(100, 999);
        //UIController.LoadingPanel.gameObject.SetActive(false);
        Debug.Log("Player Details Subscribed");
    }
    public void InitAmountDetails()
    {
        TotalAmount = (float)APIController.instance.userDetails.balance;
        string m = TotalAmount.ToString("0.00");
        TotalAmount = float.Parse(m);
        currencyType = APIController.instance.userDetails.currency_type;
        PassTxt(totalAmountTxt, APIController.instance.userDetails.currency_type);
        PassTxt(betAmountTxt, betAmount.ToString("0.00") + " " + APIController.instance.userDetails.currency_type);
        PassTxt(totalAmountTxt, $"{TotalAmount:F2} <size=35>{APIController.instance.userDetails.currency_type}</size>");
        /*PassTxt(totalAmountTxt, $"{TotalAmount:F2}" + APIController.instance.userDetails.currency_type);*/
        Debug.Log("Amount Details Subscribed");
    }
    public void InitUserDeposit()
    {
        Debug.Log("Deposit Called");
    }
    public void HideOrShowObject(GameObject _Obj, bool _show)
    {
        _Obj.SetActive(_show);
    }
    public void PassTxt(TMP_Text _txt, string _passingValue)
    {
        _txt.text = _passingValue;
    }
    #endregion ::::::::::::::::::::::::: END :::::::::::::::::::::::::
    void No_Internet()
    {
        if (internetDisconnectPannel.activeSelf)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
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
            if (startGame)
            {
                //FadeINOut();
                if (startGame && multiplier < 1.01f)
                {
                    countTime += 8f * Time.deltaTime;
                    slider.value = countTime;
                }
                if (startGame && multiplier >= 1.01f && !isPressed)
                {
                    timeSinceLastIncrement += Time.deltaTime;

                    slider.gameObject.SetActive(true);

                    countTime -= 1f * Time.deltaTime;
                    sliderAutoCashNoTxt.text = countTime.ToString(" <size=45>0</size>");

                    slider.value = countTime;
                }
                else if (startGame && multiplier >= 1.01f && isPressed)
                {
                    countTime = 8f;
                    slider.maxValue = 8f;
                    slider.value = 8f;
                }
            }
            yield return null;
        }
    }
    IEnumerator HolidngButtons()
    {
        while (true)
        {
            if (isPressed)
            {
                sliderTxt.text = null;
            }

            if (multiplier > 0.77f && makeLose)
                Balloon_Burt();

            BetAmountUpdates();

            if (isScroll)
            {
                ApplyBalloonParallaxEffect(holdButton.GetComponent<RectTransform>().anchoredPosition.y);
                ApplyParallaxEffect(holdButton.GetComponent<RectTransform>().anchoredPosition.y);

                heat_Anim.SetBool("isStart", false);
                TakeCashbutton.enabled = false;
                TakeCashImg.color = new Color32(140, 140, 140, 255);
            }

            if (lost)
            {
                holdButtonEvent.enabled = false;
                empty_holdButton.gameObject.SetActive(true);
                // sliderOBjs
                slider_Anim.SetBool("isOFF", true);
                slider_bg.SetActive(false);
                fillArea.SetActive(false);
                slider_txt.SetActive(false);
                sliderAutoCashNoTxt.gameObject.SetActive(false);
                heat_Anim.SetBool("isStart", false);
                heat_Anim.SetBool("isEnd", false);

                ballonOut.gameObject.SetActive(true);
                balloonParts.SetActive(false);
                balloonShake.SetActive(false);
                balloonShake_blue.SetActive(false);

                TakeCashImg.color = new Color32(140, 140, 140, 255);
                TakeCashtxt.color = new Color32(140, 140, 140, 255);
                TakeCashAnimImg.SetActive(false);
            }

            if (take)
            {
                // sliderOBjs
                slider_Anim.SetBool("isOFF", true);
                slider_bg.SetActive(false);
                fillArea.SetActive(false);
                slider_txt.SetActive(false);
                sliderAutoCashNoTxt.gameObject.SetActive(false);
                heat_Anim.SetBool("isStart", false);

                balloonShake_blue.SetActive(false);
                balloonParts.SetActive(true);
                balloonShake.SetActive(false);
                TakeCashAnimImg.SetActive(false);
            }
            FireButton();
            StartOfTheGame();

            if (isPressed && isFire && !lost)
            {
                // To move the Target pos Up at the Start
                ApplyParallaxEffect(holdButton.GetComponent<RectTransform>().anchoredPosition.y);
                /*fire_Anim.SetBool("isFire", true);*/
                /*blueFire_Anim.SetBool("isBlue", false);*/

                startGame = true;


                if (timeSinceLastIncrement >= incrementInterval)
                {
                    IncrementMultiplier();
                    timeSinceLastIncrement = 0f; // Reset the timer
                }

                if (multiplier >= 0.80f)
                {
                    // Increment the time button is held
                    timeHeld += Time.deltaTime;

                    // Check if the button has been held for a random time between min and max hold time
                    if (timeHeld >= UnityEngine.Random.Range(minHoldTime, maxHoldTime))
                    {
                        // Bet is lost
                        Balloon_Burt();
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
    void Balloon_Burt()
    {
        ResetBets();
        lost = true;
        holdButton.enabled = false;
        /*TakeCashbutton.enabled = false;*/
        timeSinceLastIncrement = 0f; // Reset the timer
        timeHeld = 0f; // Reset the time button is held
        winCash = 0f;

        // sliderOBjs
        slider_Anim.SetBool("isOFF", true);
        slider_bg.SetActive(false);
        fillArea.SetActive(false);
        slider_txt.SetActive(false);
        sliderAutoCashNoTxt.gameObject.SetActive(false);

        takecashOut.SetBool("isTake", false);
    }
    IEnumerator Backgourn_Ballon_Fly()
    {
        while (true)
        {
            ApplyBalloonParallaxEffect(holdButton.GetComponent<RectTransform>().anchoredPosition.y);

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
            isPressed = false;
            heat_Anim.SetBool("isEnd", false);
            if (!lost && !take && multiplier >= 1.01 && !isScroll)
            {
                balloonShake_blue.SetActive(true);
                /*balloonParts.SetActive(true);*/
                balloonShake.SetActive(false);
            }
        }
        if (!holdButton_dup.gameObject.activeSelf)
        {
            isSet = false;
        }
    }
    void StartOfTheGame()
    {
        if (multiplier >= 1.01f && !lost)
        {
            takeCash = betAmount * multiplier;
            takeCashTxt.text = takeCash.ToString("0.00");
        }
        if (startGame && multiplier <= 1.01f)
        {
            multiplier += Time.deltaTime;
            multiplierTxt.text = multiplier.ToString("0.00");

            balloonBlue_Start.SetActive(false);
            ballon_Anim.SetBool("isJump", true);

            Debug.Log(" Multiplier value : " + multiplier);

            balloon_Objs();
        }
        else if (isPressed && startGame && multiplier >= 1.01f)
        {
            balloonShake_blue.SetActive(false);
            balloonParts.SetActive(false);
            balloonShake.SetActive(true);

            multiplierTxt.text = multiplier.ToString("0.00");
            takeCash = betAmount * multiplier;
            takeCashTxt.text = takeCash.ToString("0.00");

            // Increment the timer by the time elapsed since the last frame
            timeSinceLastIncrement += Time.deltaTime;
        }

        if (startGame && takeBetAmount)
        {
            takeBetAmount = false;
            holdButton.enabled = false;
            sliderTxt.text = null;

            // Taking the bet amount that we choose
            //TotalAmount -= betAmount;
            /*totalAmountTxt.text = TotalAmount.ToString("0.00" + " <size=35>USD</size>");*/

            /*API_IntitalizeBetAmount();*/

            //minHoldTime = UnityEngine.Random.Range(-2, UnityEngine.Random.Range(5, 15));   // use this always
            minHoldTime = UnityEngine.Random.Range(0, 15);            // only for testing
        }

        if (startGame)
        {
            Button_Switch_OFF();
        }

        if (countTime > 6 && !isPressed && !lost)
        {
            if (!take)
            {
                sliderTxt.text = sliderAutoCashTxt.text.ToString();
                sliderAutoCashNoTxt.gameObject.SetActive(true);
            }
        }

        if (countTime > 6)
        {
            TakeButtonColor();
        }

        if (isBonus && isScroll)
        {
            isBonus_OFF = true;
        }
    }
    async void TakeButtonColor()
    {

        if (startGame)
        {
            await UniTask.Delay(1000);
            TakeCashbutton.enabled = true;
            TakeCashImg.color = new Color32(255, 255, 255, 255);
            TakeCashtxt.color = new Color32(255, 255, 255, 255);
            TakeCashAnimImg.SetActive(true);

            takecashOut.SetBool("isTake", true);
        }
        else
        {
            TakeCashbutton.enabled = false;
            TakeCashImg.color = new Color32(140, 140, 140, 255);
            TakeCashtxt.color = new Color32(140, 140, 140, 255);
            TakeCashAnimImg.SetActive(false);

            takecashOut.SetBool("isTake", false);
        }
    }
    void IncrementMultiplier()
    {
        if (isPressed && multiplier >= 1.01f)
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
    public async void TakeCashOut()
    {
        #region ________ Internet Checking ________
        bool haveInternet = false;

        while (!haveInternet)
        {
            Debug.Log("CashoutBtnFn While Loope Entered");

            APIController.instance.CheckInternetandProcess((success) =>
            {
                Debug.Log("CashoutBtnFn While Loope CheckInternetandProcess ");

                if (success)
                {
                    Debug.Log("CashoutBtnFn While Loope success ");

                    haveInternet = true;
                }
                Debug.Log("Cashout Checking Internet HaveInternet " + haveInternet);

            });
            int count = 0;
            while (!haveInternet && count < 15)
            {
                await UniTask.Delay(100);
                count++;
            }

            Debug.Log("CashoutBtnFn While Loope Completed ");

        }
        Debug.Log("ShowWinnerResult 1");
        #endregion

        take = true;
        startGame = false;
        /*WinAmount = takeCash;*/
        ballon_Anim.enabled = true;

        ballon_Anim.SetBool("isTake", true);
        ballon_Anim.SetBool("isJump", false);

        takecashOut.SetBool("isTake", false);
        // Change the text color
        multiplier = float.Parse(multiplier.ToString("0.00"));
        multiplierTxt.text = multiplier.ToString(/*"0.00"*/);
        //TotalAmount += WinAmount;
        /*totalAmountTxt.text = TotalAmount.ToString("0.00");*/
        holdButton.enabled = false;
        winPanel.SetActive(true);
        holdButtonEvent.enabled = false;
        /*slider.gameObject.SetActive(false);*/
        empty_holdButton.gameObject.SetActive(true);

        TakeCashImg.color = new Color32(140, 140, 140, 255);
        TakeCashtxt.color = new Color32(140, 140, 140, 255);
        TakeCashAnimImg.SetActive(false);

        // sliderOBjs
        slider_bg.SetActive(false);
        fillArea.SetActive(false);
        slider_txt.SetActive(false);
        sliderAutoCashNoTxt.gameObject.SetActive(false);
        slider_Anim.SetBool("isOFF", true);

        TakingCash();
        Winning_Animations();

        if (!winCount && betAmount <= 5f)
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
    void API_IntitalizeBetAmount()
    {
        BetInputController.Instance.BetAmtInput.textViewport.gameObject.SetActive(false);

        if (BetInputController.Instance.BetPanel.gameObject.activeSelf)
        {
            BetInputController.Instance.CloseKeyPadPanel();
            BetInputController.Instance.RestrictInput();
        }

        APIController.instance.CheckInternetandProcess((success) =>
        {
            Debug.Log("CheckInternetandProcess Betbtn");
            if (success)
            {
                Debug.Log("CheckInternetandProcess Betbtn Success");
                //if (!APIController.instance.userDetails.isBlockApiConnection)
                //{
                //    Debug.Log("Calling LocalInitializeBet Calling Demo");
                LocalInitializeBet();
                //}
            }
            else
            {
                Debug.Log("CheckInternetandProcess Failed");
                BetInputController.Instance.BetAmtInput.interactable = true;
            }
        });
        //if (APIController.instance.userDetails.isBlockApiConnection)
        //{
        //    Debug.Log("Calling LocalInitializeBet Calling Live");

        //    LocalInitializeBet();
        //}
    }
    void LocalInitializeBet()
    {
        Debug.Log("Entered LocalInitializeBet");

        //  APIController.instance.StartCheckInternetLoop();

        if (betAmount <= TotalAmount)
        {
            if (betAmount < 0.1f)
            {
                if (TotalAmount >= .1f)
                {
                    // Controller.BetAmount = 5;
                    PassTxt(betAmountTxt, betAmount.ToString("0.00") + " " + APIController.instance.userDetails.currency_type);
                }
                else
                {
                    Debug.Log("nmnm");
                }
            }
        }
        else
        {
            return;
        }
        string message = "Bet Initiated";

        int _index = UnityEngine.Random.Range(100, 999);
        ////   Controller.BetIndex = APIController.instance.InitlizeBet((Controller.BetAmount), val);
        //BetInputController.Instance.CloseKeyPadPanel();
        //BetInputController.Instance.BetAmtInput.interactable = false;


        //string _s = betAmount.ToString("0.00");
        //float amount = float.Parse(_s);

        TransactionMetaData val = new TransactionMetaData();
        val.Amount = betAmount;
        val.Info = message;

        string m = betAmount.ToString("0.00");
        betAmount = float.Parse(m);
        Debug.Log("LocalInitializeBet Controller.BetButtonclik ");
        Debug.Log("BetAMount ********* " + betAmount + " " + " Balance ******* " + TotalAmount);

        List<string> _list = new List<string>();
        _list.Add(APIController.instance.userDetails.Id);
        //APIController.instance.AddPlayers(MatchRes.MatchToken, _list);
        Debug.Log("isBlockApiConnection " + APIController.instance.userDetails.isBlockApiConnection);

        if (!APIController.instance.userDetails.isBlockApiConnection)
        {
            BetIndex = APIController.instance.CreateAndJoinMatch(_index, betAmount, val, false, lobbyName, APIController.instance.userDetails.Id, false, gameName, operatorName, APIController.instance.userDetails.gameId, APIController.instance.userDetails.isBlockApiConnection, _list, (success, newbetID, res) =>
            {
                if (success)
                {
                    Debug.Log("Bet Initiated");
                    Debug.Log("Live Mode");
                    startGame = true;
                }
                else
                {
                    Debug.Log("Bet Initiate Failed");
                    startGame = false;
                }
            });
        }
        else
        {
            BetIndex = APIController.instance.InitlizeBet(betAmount, val, false, (success) =>
            {
                if (success)
                {
                    Debug.Log("Bet Initiated");
                    Debug.Log("Demo Mode");
                    startGame = true;
                }
                else
                {
                    Debug.Log("Bet Initiate Failed");
                    startGame = false;
                }
            }, APIController.instance.userDetails.Id, false);
        }
    }
    void API_Winning()
    {
        string message = "Game Won";
        string value = takeCash.ToString("F2");
        float amount = float.Parse(value);
        TransactionMetaData val = new TransactionMetaData();
        val.Amount = amount;
        val.Info = message;

        // APIController.instance.WinningsBet(Controller.BetIndex, Controller.WonAmount, Controller.BetAmount, val);
        if (!APIController.instance.userDetails.isBlockApiConnection)
        {
            APIController.instance.WinningsBetMultiplayerAPI(BetIndex, betID, amount, betAmount, takeCash, val, (success) =>
            {
                if (success)
                {
                    Debug.Log("Winning Bet Initiated");
                    APIController.GetUpdatedBalance();
                }
                else
                {
                    Debug.Log("Winning Bet Failed");

                }
            }, APIController.instance.userDetails.Id, false, takeCash == 0 ? false : true, gameName, operatorName, APIController.instance.userDetails.gameId, APIController.instance.userDetails.commission, MatchRes.MatchToken);
        }
        else
        {
            APIController.instance.WinningsBet(BetIndex, takeCash, betAmount, val, (success) =>
            {
                if (success)
                {
                    Debug.Log("Winning Bet Initiated");
                }
                else
                {
                    Debug.Log("Winning Bet Failed");

                }
            }, APIController.instance.userDetails.Id, false);
        }
    }
    void Call_Functions()
    {
        if (winCash == /*3*/winBonus)
        {
            isBonus_1 = true;
            if (!isSet)
                isBonus_2 = true;
            else if (isSet)
                isBonus_3 = true;
        }

        if (winCash != /*3*/ winBonus)
            isNormal = true;

    }
    async void DelayFuction()
    {
        if (isBonus_1)
        {
            await UniTask.Delay(3000);
            winPanel.SetActive(false);

            await UniTask.Delay(100);
            Bonus_Conditions();
        }
        if (isBonus_2)
        {
            await UniTask.Delay(6000); // wait for 5 seconds
            TimeDelay_WinCash();
        }

        if (isNormal)
        {
            await UniTask.Delay(3500); // wait for 2.5 seconds
            TimeDelay();
        }

        if (isBonus_3)
        {
            await UniTask.Delay(5000); // wait for 2.5 seconds
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
        /*winAmountTxt.gameObject.SetActive(false);*/
        /*WinAmount = 0;*/
        /*winAmountTxt.text = WinAmount.ToString("0.00");*/
        holdButton.enabled = true;
        holdButtonEvent.enabled = true;
        empty_holdButton.gameObject.SetActive(false);
        take = false;
        lost = false;
        isNormal = false;
        ballonOut.gameObject.SetActive(false);

        background.localPosition = initialBackgroundPosition;
        bg.localPosition = iniBackgroundPos;

        if (winCount == true)
        {
            winCount = false;
        }
        Button_Switch_ON();
        TakeCashImg.color = new Color32(140, 140, 140, 255);
        TakeCashtxt.color = new Color32(140, 140, 140, 255);
        TakeCashAnimImg.SetActive(false);

        winPanel.SetActive(false);
        takeBetAmount = true;

        countTime = 0f;
        slider.value = 0f;

        ButtonSelect_Anim();

        slider_Anim.SetBool("isOFF", false);
        slider_Anim.SetBool("isON", false);

        /*balloon.SetBool("isBalloon", false);*/
        ballon_Anim.enabled = true;
        ballon_Anim.SetBool("isJump", false);
        ballon_Anim.SetBool("isTake", false);

        balloonShake_blue.SetActive(false);
        balloonShake.SetActive(false);
        balloonParts.transform.localPosition = new Vector3(0f, -430.1323f, 0f);
        TxtObjs.transform.localPosition = new Vector3(-85f, 34f, 0f);
        BalloonDelay();



        heat_Anim.SetBool("isStart", true);
        // sliderOBjs
        slider_Anim.SetBool("isON", true);
        Slider_Objs();
    }
    void TimeDelay_WinCash()
    {
        multiplierTxt.color = Color.white;
        Xtxt.color = Color.white;
        multiplier = 0f;
        multiplierTxt.text = multiplier.ToString("0.00");
        takeCash = 0f;
        takeCashTxt.text = takeCash.ToString("0.00");
        /*winAmountTxt.gameObject.SetActive(false);*/
        /*WinAmount = 0;*/
        /*winAmountTxt.text = WinAmount.ToString("0.00");*/
        holdButton.enabled = true;
        holdButtonEvent.enabled = true;
        empty_holdButton.gameObject.SetActive(false);
        take = false;
        lost = false;
        isBonus_2 = false;
        ballonOut.gameObject.SetActive(false);
        background.localPosition = initialBackgroundPosition;
        bg.localPosition = iniBackgroundPos;
        ButtonSelect_Anim();
        winPanel.SetActive(false);
        takeBetAmount = true;
        Button_Switch_ON();
        countTime = 0f;
        slider.value = 0f;

        TakeCashImg.color = new Color32(140, 140, 140, 255);
        TakeCashtxt.color = new Color32(140, 140, 140, 255);
        TakeCashAnimImg.SetActive(false);

        slider_Anim.SetBool("isOFF", false);
        slider_Anim.SetBool("isON", false);

        ballon_Anim.enabled = true;
        ballon_Anim.SetBool("isJump", false);
        ballon_Anim.SetBool("isTake", false);

        balloonShake_blue.SetActive(false);
        balloonShake.SetActive(false);

        balloonParts.transform.localPosition = new Vector3(0f, -430.1323f, 0f);
        TxtObjs.transform.localPosition = new Vector3(-85f, 34f, 0f);
        BalloonDelay();

        heat_Anim.SetBool("isStart", true);

        // sliderOBjs
        slider_Anim.SetBool("isON", true);
        Slider_Objs();
    }
    async void BalloonDelay()
    {
        await UniTask.Delay(100);
        balloonParts.SetActive(true);
        balloonBlue_Start.SetActive(true);
    }
    void ResetBets()
    {
        isPressed = false;
        startGame = false;
        multiplierTxt.text = multiplier.ToString("0.00");
        // Change the text color
        multiplierTxt.color = Color.black;
        Xtxt.color = Color.black;
        makeLose = false;

        Invoke("TimeDelay", 1.5f);
    }
    void Winning_Animations()
    {
        winTxt.text = multiplier.ToString("0.00" + " <size=80>X</size>");
        WinTxtObj();
    }
    async void TakingCash()
    {
        await UniTask.Delay(3000);
        API_Winning();
    }
    async void WinTxtObj()
    {
        await UniTask.Delay(2000);
        winTxt.transform.DORotate(new Vector3(0f, 360f, 0f), 0.5f, RotateMode.FastBeyond360).SetEase(Ease.InOutSine);
        winTxt.text = takeCash.ToString("0.00" + " <size=70>USD</size>");
        winTxt.color = Color.green;
        //totalAmountTxt.text = TotalAmount.ToString("0.00" + " <size=70>USD</size>");
    }
    async void balloon_Objs()
    {
        await UniTask.Delay(800);
        ballon_Anim.SetBool("isJump", false);
        ballon_Anim.enabled = false;
        balloonParts.SetActive(false);

        if (isPressed)
        {
            balloonShake.SetActive(true);
        }
        else
        {
            balloonShake_blue.SetActive(true);
        }
    }
    async void Slider_Objs()
    {
        await UniTask.Delay(200);
        slider_bg.SetActive(true);
        fillArea.SetActive(true);
        slider_txt.SetActive(true);
        sliderTxt.text = sliderDupTxt.text.ToString();
    }
    #region ::::::::::::::::::::::::::: Bonus Functions :::::::::::::::::::::::::::
    void Bonus_Delay()
    {
        multiplierTxt.color = Color.white;
        Xtxt.color = Color.white;
        multiplier = 0f;
        multiplierTxt.text = multiplier.ToString("0.00");
        /*txtObj.gameObject.SetActive(true);*/
        takeCash = 0f;
        takeCashTxt.text = takeCash.ToString("0.00");
        /*WinAmount = 0;*/
        /*winObj.gameObject.SetActive(false);*/
        isBonus_3 = false;
        take = false;
        ScrollViewer();
    }
    async void ScrollViewer()
    {
        await UniTask.Delay(1000); // wait for 2.5 seconds
        ScrollView_Conditions();
    }
    void Bonus_Conditions()
    {
        if (winCash == /*3*/ winBonus)
        {
            bonusObj.gameObject.SetActive(true);
            Fill_Img.instance.Bonus_Script();
            bonusBallon.SetBool("isOpen", true);

            // Bonus Reward Fill Img
            if (timeRemaining < totalTime)
            {
                if (betAmount <= 5f)
                {
                    if (betAmount <= 1f)
                    {
                        bonusTimer = winCash_Demo;
                    }
                    else if (betAmount >= 1.5f && betAmount <= 2f)
                    {
                        bonusTimer = winCash_Demo;
                    }
                    else if (betAmount >= 2.1f && betAmount <= 5f)
                    {
                        bonusTimer = winCash_Demo;
                    }
                }

                /*bonusTimer = winCash_Demo;*/
            }
            else
            {
                // Timer has reached the total time
                timeRemaining = totalTime; // Ensure timeRemaining does not exceed totalTime
                /*UpdateTimerUI(); // Update the UI to reflect that the timer is full*/
                isScroll = true;
            }
            winCash = 0;
            isBonus_1 = false;
        }
    }
    void Demo_Bonus()
    {
        if (winCash == /*3*/ winBonus)
        {
            if (betAmount <= 5f)
            {
                if (betAmount <= 1f)
                {
                    winCash_Demo += 8f;
                }
                else if (betAmount >= 1.5f && betAmount <= 2f)
                {
                    winCash_Demo += 4f;
                }
                else if (betAmount >= 2.1f && betAmount <= 5f)
                {
                    winCash_Demo += 2f;
                }
            }

            if (winCash_Demo > 10f)
            {
                winCash_Demo = 10f;
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
    public void OnClickDown()
    {
        if (TotalAmount >= 0.10f && !numPad && (betAmount <= TotalAmount))
        {
            isPressed = true;
            balloonShake_blue.SetActive(false);
            balloonParts.SetActive(false);
            ApplyParallaxEffect(holdButton.GetComponent<RectTransform>().anchoredPosition.y);

            heat_Anim.SetBool("isEnd", true);
            heat_Anim_Img.SetActive(true);

            takecashOut.SetBool("isTake", true);

            for (int i = 0; i < button_Anim.Length; i++)
            {
                button_Anim[i].SetActive(false);
            }

            TakeCashbutton.enabled = false;
            /*sliderTxt.text = sliderAutoCashTxt.text.ToString();*/
            sliderTxt.text = null;
            sliderAutoCashNoTxt.gameObject.SetActive(false);
        }

        string s1 = TotalAmount.ToString("0.00");
        TotalAmount = float.Parse(s1);
        string s2 = betAmount.ToString("0.00");
        betAmount = float.Parse(s2);



        if (!startGame)
        {
            API_IntitalizeBetAmount();

            if ((betAmount > TotalAmount) || (betAmount < 0.10))
            {
                Debug.Log("InSufficientBalance");
                InsufficientBalance.SetActive(true);
                return;
            }
        }
    }
    public void OnClickUp()
    {
        isPressed = false;
        heat_Anim.SetBool("isEnd", false);
        heat_Anim_Img.SetActive(false);

        /*fire_Anim.SetBool("isFire", false);*/

        if (!lost && !take && multiplier >= 1.01f)
        {
            balloonShake_blue.SetActive(true);
            /*balloonParts.SetActive(true);*/
            balloonShake.SetActive(false);
        }
    }
    public void Button_ONEnter()
    {
        isFire = true;
    }
    public void Button_OFFEnter()
    {
        isFire = false;
    }
    public void Welcom_Button()
    {
        /*blueFire_Anim.SetBool("isBlue", false);*/
        heat_Anim.SetBool("isStart", true);
        StartCoroutine(Backgourn_Ballon_Fly());

        /*for (int i = 0; i < button_Anim.Length; i++)
        {
            button_Anim[i].SetActive(true);
        }*/
        // sliderOBjs
        slider_Anim.SetBool("isON", true);
        Slider_Objs();
    }
    #endregion

    #region { ::::::::::::::::::::::::: Buttons ::::::::::::::::::::::::: }
    public void BetButton_1()
    {
        if (keyBoard.cancelButton.gameObject.activeSelf)
            keyBoard.OnCancelInput();

        if (!startGame && !take && !isScroll)
        {
            betAmount = 1f;
            betAmountTxt.text = betAmount.ToString("0.00" + " USD");
            button_1.gameObject.SetActive(true);
            button_2.gameObject.SetActive(false);
            button_5.gameObject.SetActive(false);
            button_10.gameObject.SetActive(false);
            plusButton.enabled = true;
            minusButton.enabled = true;
            plusButtomImg.color = new Color32(255, 255, 255, 255);
            minusButtonImg.color = new Color32(255, 255, 255, 255);

            for (int i = 0; i < button_Anim.Length; i++)
            {
                if (button_Anim[i].activeSelf)
                    button_Anim[i].SetActive(false);
            }

            winCash = 0;
            winCash_Demo = 0;
            bonusTimer = 0;
            timer = true;
        }
    }
    public void BetButton_2()
    {
        if (keyBoard.cancelButton.gameObject.activeSelf)
            keyBoard.OnCancelInput();

        if (!startGame && !take && !isScroll)
        {
            betAmount = 2f;
            betAmountTxt.text = betAmount.ToString("0.00" + " USD");
            button_1.gameObject.SetActive(false);
            button_2.gameObject.SetActive(true);
            button_5.gameObject.SetActive(false);
            button_10.gameObject.SetActive(false);

            for (int i = 0; i < button_Anim.Length; i++)
            {
                if (button_Anim[i].activeSelf)
                    button_Anim[i].SetActive(false);
            }

            plusButton.enabled = true;
            minusButton.enabled = true;
            plusButtomImg.color = new Color32(255, 255, 255, 255);
            minusButtonImg.color = new Color32(255, 255, 255, 255);
            winCash = 0;
            winCash_Demo = 0;
            bonusTimer = 0;
            timer = true;
        }
    }
    public void BetButton_5()
    {
        if (keyBoard.cancelButton.gameObject.activeSelf)
            keyBoard.OnCancelInput();

        if (!startGame && !take && !isScroll)
        {
            betAmount = 5f;
            betAmountTxt.text = betAmount.ToString("0.00" + " USD");
            button_1.gameObject.SetActive(false);
            button_2.gameObject.SetActive(false);
            button_5.gameObject.SetActive(true);
            button_10.gameObject.SetActive(false);

            for (int i = 0; i < button_Anim.Length; i++)
            {
                if (button_Anim[i].activeSelf)
                    button_Anim[i].SetActive(false);
            }

            plusButton.enabled = true;
            minusButton.enabled = true;
            plusButtomImg.color = new Color32(255, 255, 255, 255);
            minusButtonImg.color = new Color32(255, 255, 255, 255);

            winCash = 0;
            winCash_Demo = 0;
            bonusTimer = 0;
            timer = true;
        }
    }
    public void BetButton_10()
    {
        if (keyBoard.cancelButton.gameObject.activeSelf)
            keyBoard.OnCancelInput();

        if (!startGame && !take && !isScroll)
        {
            betAmount = 10f;
            betAmountTxt.text = betAmount.ToString("0.00" + " USD");
            button_1.gameObject.SetActive(false);
            button_2.gameObject.SetActive(false);
            button_5.gameObject.SetActive(false);
            button_10.gameObject.SetActive(true);

            for (int i = 0; i < button_Anim.Length; i++)
            {
                if (button_Anim[i].activeSelf)
                    button_Anim[i].SetActive(false);
            }

            plusButton.enabled = true;
            minusButton.enabled = true;
            plusButtomImg.color = new Color32(255, 255, 255, 255);
            minusButtonImg.color = new Color32(255, 255, 255, 255);

            winCash = 0;
            winCash_Demo = 0;
            bonusTimer = 0;
            timer = true;
        }
    }

    // Select bet buttons
    public void SelectBetButton_1()
    {
        if (keyBoard.cancelButton.gameObject.activeSelf)
            keyBoard.OnCancelInput();

        if (!startGame && !take && !isScroll)
        {
            if (betAmount < 100f)
            {
                betAmount += 1f;
                betAmountTxt.text = betAmount.ToString("0.00" + " USD");
                plusButton.enabled = true;
                minusButton.enabled = true;
                plusButtomImg.color = new Color32(255, 255, 255, 255);
                minusButtonImg.color = new Color32(255, 255, 255, 255);
            }

            if (betAmount >= 100)
            {
                betAmount = 100f;
                betAmountTxt.text = betAmount.ToString("0.00" + " USD");
                plusButton.enabled = false;
                plusButtomImg.color = new Color32(255, 255, 255, 120);
            }

            winCash = 0;
            winCash_Demo = 0;
            bonusTimer = 0;
            timer = true;
        }
    }
    public void SelectBetButton_2()
    {
        if (keyBoard.cancelButton.gameObject.activeSelf)
            keyBoard.OnCancelInput();

        if (!startGame && !take && !isScroll)
        {
            if (betAmount < 100f)
            {
                betAmount += 2f;
                betAmountTxt.text = betAmount.ToString("0.00" + " USD");
                plusButton.enabled = true;
                minusButton.enabled = true;
                plusButtomImg.color = new Color32(255, 255, 255, 255);
                minusButtonImg.color = new Color32(255, 255, 255, 255);
            }

            if (betAmount >= 100)
            {
                betAmount = 100f;
                betAmountTxt.text = betAmount.ToString("0.00" + " USD");
                plusButton.enabled = false;
                plusButtomImg.color = new Color32(255, 255, 255, 120);
            }

            winCash = 0;
            winCash_Demo = 0;
            bonusTimer = 0;
            timer = true;
        }
    }
    public void SelectBetButton_5()
    {
        if (keyBoard.cancelButton.gameObject.activeSelf)
            keyBoard.OnCancelInput();

        if (!startGame && !take && !isScroll)
        {
            if (betAmount < 100f)
            {
                betAmount += 5f;
                betAmountTxt.text = betAmount.ToString("0.00" + " USD");
                plusButton.enabled = true;
                minusButton.enabled = true;
                plusButtomImg.color = new Color32(255, 255, 255, 255);
                minusButtonImg.color = new Color32(255, 255, 255, 255);
            }

            if (betAmount >= 100)
            {
                betAmount = 100f;
                betAmountTxt.text = betAmount.ToString("0.00" + " USD");
                plusButton.enabled = false;
                plusButtomImg.color = new Color32(255, 255, 255, 120);
            }

            winCash = 0;
            winCash_Demo = 0;
            bonusTimer = 0;
            timer = true;
        }
    }
    public void SelectBetButton_10()
    {
        if (keyBoard.cancelButton.gameObject.activeSelf)
            keyBoard.OnCancelInput();

        if (!startGame && !take && !isScroll)
        {
            if (betAmount < 100f)
            {
                betAmount += 10f;
                betAmountTxt.text = betAmount.ToString("0.00" + " USD");
                plusButton.enabled = true;
                minusButton.enabled = true;
                plusButtomImg.color = new Color32(255, 255, 255, 255);
                minusButtonImg.color = new Color32(255, 255, 255, 255);
            }

            if (betAmount >= 100)
            {
                betAmount = 100f;
                betAmountTxt.text = betAmount.ToString("0.00" + " USD");
                plusButton.enabled = false;
                plusButtomImg.color = new Color32(255, 255, 255, 120);
            }

            winCash = 0;
            winCash_Demo = 0;
            bonusTimer = 0;
            timer = true;
        }
    }
    public void PlusButton()
    {
        if (BetInputController.Instance.BetPanel.gameObject.activeSelf)
        {
            BetInputController.Instance.CloseKeyPadPanel();
            BetInputController.Instance.RestrictInput();
            BetInputController.Instance.BetAmtInput.textViewport.gameObject.SetActive(false);
            return;
        }

        if (keyBoard.cancelButton.gameObject.activeSelf)
            keyBoard.OnCancelInput();

        if (!startGame && !take && !isScroll)
        {
            if (betAmount < 100)
            {
                betAmount += 0.10f;
                betAmountTxt.text = betAmount.ToString("0.00" + " USD");
                minusButton.enabled = true;
                minusButtonImg.color = new Color32(255, 255, 255, 255);
            }
            if (betAmount >= 100)
            {
                betAmount = 100f;
                betAmountTxt.text = betAmount.ToString("0.00" + " USD");
                plusButton.enabled = false;
                plusButtomImg.color = new Color32(255, 255, 255, 120);
            }

            for (int i = 0; i < button_Anim.Length; i++)
            {
                if (button_Anim[i].activeSelf)
                    button_Anim[i].SetActive(false);
            }

            if (betAmount != 100)
            {
                winCash = 0;
                winCash_Demo = 0;
                bonusTimer = 0;
                timer = true;
            }
        }
    }
    public void MinusButton()
    {
        /*keyBoard.OnCancelInput();*/
        if (BetInputController.Instance.BetPanel.gameObject.activeSelf)
        {
            BetInputController.Instance.CloseKeyPadPanel();
            BetInputController.Instance.RestrictInput();
            BetInputController.Instance.BetAmtInput.textViewport.gameObject.SetActive(false);
            return;
        }

        if (!startGame && !take && !isScroll)
        {
            if (betAmount > 0.10f)
            {
                betAmount -= 0.10f;
                betAmountTxt.text = betAmount.ToString("0.00" + " USD");
                plusButtomImg.color = new Color32(255, 255, 255, 255);
            }
            if (betAmount <= 0.10f)
            {
                betAmount = 0.10f;
                betAmountTxt.text = betAmount.ToString("0.00" + " USD");
                minusButton.enabled = false;
                minusButtonImg.color = new Color32(255, 255, 255, 120);
            }

            for (int i = 0; i < button_Anim.Length; i++)
            {
                if (button_Anim[i].activeSelf)
                    button_Anim[i].SetActive(false);
            }

            if (betAmount != 0.10f)
            {
                winCash = 0;
                winCash_Demo = 0;
                bonusTimer = 0;
                timer = true;
            }
        }
    }

    void Button_Switch_ON()
    {
        button_1.enabled = true; button_2.enabled = true; button_5.enabled = true; button_10.enabled = true;

        plusButton.interactable = true; plusButtomImg.color = new Color32(255, 255, 255, 255);
        minusButton.interactable = true; minusButtonImg.color = new Color32(255, 255, 255, 255);
    }
    void Button_Switch_OFF()
    {
        button_1.enabled = false; button_2.enabled = false; button_5.enabled = false; button_10.enabled = false;

        plusButton.interactable = false; plusButtomImg.color = new Color32(255, 255, 255, 120);
        minusButton.interactable = false; minusButtonImg.color = new Color32(255, 255, 255, 120);
    }

    void BetAmountUpdates()
    {
        if (startGame)
        {
            numPadButton.gameObject.SetActive(false);
            /*sliderTxt.text = null;*/
        }
        else
        {
            numPadButton.gameObject.SetActive(true);
        }

        if (betAmount > 0.10f)
        {
            minusButton.enabled = true;
            minusButtonImg.color = new Color32(255, 255, 255, 255);
        }
        else if (betAmount <= 0.10f)
        {
            minusButton.enabled = false;
            minusButtonImg.color = new Color32(255, 255, 255, 120);
        }

        if (betAmount < 100f)
        {
            plusButton.enabled = true;
            plusButtomImg.color = new Color32(255, 255, 255, 255);
        }
        else if (betAmount >= 100f)
        {
            plusButton.enabled = false;
            plusButtomImg.color = new Color32(255, 255, 255, 120);
        }

        if (betAmount < 0.10f)
        {
            cancelButton.SetActive(false);
        }
        else if (betAmount >= 0.10f)
        {
            cancelButton.SetActive(true);
        }
    }
    void ButtonSelect_Anim()
    {
        if (button_1.gameObject.activeSelf)
        {
            for (int i = 0; i < button_Anim.Length; i++)
            {
                button_Anim[0].SetActive(false);
                button_Anim[1].SetActive(true); button_Anim[2].SetActive(true); button_Anim[3].SetActive(true);
            }
        }
        else if (button_2.gameObject.activeSelf)
        {
            for (int i = 0; i < button_Anim.Length; i++)
            {
                button_Anim[1].SetActive(false);
                button_Anim[0].SetActive(true); button_Anim[2].SetActive(true); button_Anim[3].SetActive(true);
            }
        }
        else if (button_5.gameObject.activeSelf)
        {
            for (int i = 0; i < button_Anim.Length; i++)
            {
                button_Anim[2].SetActive(false);
                button_Anim[0].SetActive(true); button_Anim[1].SetActive(true); button_Anim[3].SetActive(true);
            }
        }
        else if (button_10.gameObject.activeSelf)
        {
            for (int i = 0; i < button_Anim.Length; i++)
            {
                button_Anim[3].SetActive(false);
                button_Anim[0].SetActive(true); button_Anim[1].SetActive(true); button_Anim[2].SetActive(true);
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