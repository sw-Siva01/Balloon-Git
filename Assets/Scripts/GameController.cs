using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System;

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
    private float WinAmount = 0;
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
    private bool startGame;
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

    [SerializeField] GameObject balloonParts;
    [SerializeField] GameObject balloonShake;
    [SerializeField] RectTransform fireObj;
    [SerializeField] RectTransform blueFire;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    // Sliders
    [Header("Sliders")]
    [SerializeField] Slider slider;
    [SerializeField] float countTime;
    [SerializeField] GameObject slider_bg;
    [SerializeField] GameObject fillArea;
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
    [SerializeField] Animator jumpBallon;
    [SerializeField] Animator takeBallon;

    // Heat button
    [SerializeField] Animator heat_Anim;
    [SerializeField] GameObject heat_Anim_Img;

    // slider
    [SerializeField] Animator slider_Anim;

    // fire in balloon
    [SerializeField] Animator fire_Anim;
    /*[SerializeField] Animator blueFire_Anim;*/
    [SerializeField] Animator bonusBallon;

    // takeCash button
    [SerializeField] Animator takecashOut;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    // ScrollView GameObjects
    [Header("Insufficient Balance")]
    [SerializeField] GameObject InsufficientBalance;

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

        winBonus = UnityEngine.Random.Range(3, 6);

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
                countTime += 8f * Time.deltaTime;
                slider.value = countTime;
            }
            if (startGame && multiplier >= 1.01 && !isPressed)
            {
                timeSinceLastIncrement += Time.deltaTime;

                slider.gameObject.SetActive(true);

                countTime -= 1f * Time.deltaTime;
                sliderAutoCashNoTxt.text = countTime.ToString(" <size=45>0</size>");

                slider.value = countTime;
            }
            else if (startGame && multiplier >= 1.01 && isPressed)
            {
                countTime = 8f;
                slider.maxValue = 8f;
                slider.value = 8f;
            }



            yield return null;
        }
    }
    IEnumerator HolidngButtons()
    {
        while (true)
        {
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
                fire_Anim.SetBool("isFire", true);
                /*blueFire_Anim.SetBool("isBlue", false);*/

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
                    if (timeHeld >= UnityEngine.Random.Range(minHoldTime, maxHoldTime))
                    {
                        // Bet is lost

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
            if (!lost)
            {
                balloonParts.SetActive(true);
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
        if (multiplier >= 1.01 && !lost)
        {
            takeCash = betAmount * multiplier;
            takeCashTxt.text = takeCash.ToString("0.00");
        }
        if (startGame && multiplier < 1.01f)
        {
            multiplier += Time.deltaTime;
            multiplierTxt.text = multiplier.ToString("0.00");

            jumpBallon.SetBool("isJump", true);
            blueFire.gameObject.SetActive(false);
            fireObj.gameObject.SetActive(false);

            balloon_Objs();
        }
        else if (isPressed && startGame && multiplier >= 1.01)
        {
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

            // Taking the bet amount that we choose
            //TotalAmount -= betAmount;
            totalAmountTxt.text = TotalAmount.ToString("0.00" + " <size=35>USD</size>");

            minHoldTime = UnityEngine.Random.Range(-2, 7);
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
        jumpBallon.enabled = true;

        jumpBallon.SetBool("isTake", true);
        jumpBallon.SetBool("isJump", false);

        takecashOut.SetBool("isTake", false);
        // Change the text color
        multiplier = float.Parse(multiplier.ToString("0.00"));
        multiplierTxt.text = multiplier.ToString(/*"0.00"*/);
        //TotalAmount += WinAmount;
        totalAmountTxt.text = TotalAmount.ToString("0.00" + " <size=35>USD</size>");
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
        /*API_Winning();*/

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

        APIController.instance.CheckInternetForButtonClick((success) =>
        {
            Debug.Log("CheckInternetandProcess Betbtn");
            if (success)
            {
                Debug.Log("CheckInternetandProcess Betbtn Success");
                if (APIController.instance.userDetails.isBlockApiConnection)
                {
                    Debug.Log("Calling LocalInitializeBet Calling Demo");
                    LocalInitializeBet();
                }
            }
            else
            {
                Debug.Log("CheckInternetandProcess Failed");
                BetInputController.Instance.BetAmtInput.interactable = true;
            }
        });
        if (!APIController.instance.userDetails.isBlockApiConnection)
        {
            Debug.Log("Calling LocalInitializeBet Calling Live");

            LocalInitializeBet();
        }
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
                }
                else
                {
                    Debug.Log("Bet Initiate Failed");
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
                }
                else
                {
                    Debug.Log("Bet Initiate Failed");
                }
            }, APIController.instance.userDetails.Id, false);
        }
    }
    void API_Winning()
    {
        string message = "Game Won";
        string value = WinAmount.ToString("F2");
        float amount = float.Parse(value);
        TransactionMetaData val = new TransactionMetaData();
        val.Amount = amount;
        val.Info = message;

        // APIController.instance.WinningsBet(Controller.BetIndex, Controller.WonAmount, Controller.BetAmount, val);
        if (!APIController.instance.userDetails.isBlockApiConnection)
        {
            APIController.instance.WinningsBetMultiplayerAPI(BetIndex, betID, amount, betAmount, WinAmount, val, (success) =>
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
            }, APIController.instance.userDetails.Id, false, WinAmount == 0 ? false : true, gameName, operatorName, APIController.instance.userDetails.gameId, APIController.instance.userDetails.commission, MatchRes.MatchToken);
        }
        else
        {
            APIController.instance.WinningsBet(BetIndex, WinAmount, betAmount, val, (success) =>
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
        WinAmount = 0;
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
            /*targetImg.enabled = true;*/
            /*bonus_Balloon.gameObject.SetActive(false);*/
            winCount = false;
        }

        TakeCashImg.color = new Color32(140, 140, 140, 255);
        TakeCashtxt.color = new Color32(140, 140, 140, 255);
        TakeCashAnimImg.SetActive(false);

        winPanel.SetActive(false);
        takeBetAmount = true;

        countTime = 0f;
        slider.value = 0f;
        /*slider.gameObject.SetActive(true);*/
        InSufficientBalance();
        // DoTween Text
        //txtObj.DOScale(new Vector3(1f, 1f, 1f), 0.01f);
        ///*txtObj.DOMove(new Vector3(0f, 0.27f, 0f), 0.01f);*/
        //txtObj.DOMove(new Vector3(0f, 1f, 0f), 0.01f);// new Position

        button_1.gameObject.SetActive(false);
        button_2.gameObject.SetActive(false);
        button_5.gameObject.SetActive(false);
        button_10.gameObject.SetActive(false);

        slider_Anim.SetBool("isOFF", false);
        slider_Anim.SetBool("isON", false);

        /*balloon.SetBool("isBalloon", false);*/
        jumpBallon.enabled = true;
        jumpBallon.SetBool("isJump", false);
        jumpBallon.SetBool("isTake", false);
        /*balloonDefaultObj.SetActive(true);*/
        /*balloon_Shake.SetBool("isShake", false);*/
        balloonParts.SetActive(true);
        balloonShake.SetActive(false);
        balloonParts.transform.localPosition = new Vector3(0f, -430.1323f, 0f);
        TxtObjs.transform.localPosition = new Vector3(-85f, 34f, 0f);

        heat_Anim.SetBool("isStart", true);
        for (int i = 0; i < button_Anim.Length; i++)
        {
            button_Anim[i].SetActive(true);
        }

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
        WinAmount = 0;
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
        InSufficientBalance();

        winPanel.SetActive(false);
        takeBetAmount = true;

        countTime = 0f;
        slider.value = 0f;

        TakeCashImg.color = new Color32(140, 140, 140, 255);
        TakeCashtxt.color = new Color32(140, 140, 140, 255);
        TakeCashAnimImg.SetActive(false);

        button_1.gameObject.SetActive(false);
        button_2.gameObject.SetActive(false);
        button_5.gameObject.SetActive(false);
        button_10.gameObject.SetActive(false);

        slider_Anim.SetBool("isOFF", false);
        slider_Anim.SetBool("isON", false);


        jumpBallon.enabled = true;
        jumpBallon.SetBool("isJump", false);
        jumpBallon.SetBool("isTake", false);

        balloonParts.SetActive(true);
        balloonShake.SetActive(false);
        balloonParts.transform.localPosition = new Vector3(0f, -430.1323f, 0f);
        TxtObjs.transform.localPosition = new Vector3(-85f, 34f, 0f);

        heat_Anim.SetBool("isStart", true);
        for (int i = 0; i < button_Anim.Length; i++)
        {
            button_Anim[i].SetActive(true);
        }

        // sliderOBjs
        slider_Anim.SetBool("isON", true);
        Slider_Objs();

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
    void InSufficientBalance()
    {
        if (TotalAmount < 0.10)
        {
            Debug.Log("InSufficientBalance");
            InsufficientBalance.SetActive(true);
        }
    }
    void Winning_Animations()
    {
        winTxt.text = multiplier.ToString("0.00" + " <size=80>X</size>");
        WinTxtObj();
        // DoTween Text in Sequence
        Sequence sequence = DOTween.Sequence();

        // Add the first scale animation and move animation to the sequence
        sequence.Append(winObj.DOScale(new Vector3(1.5f, 1.5f, 1.5f), 1f).SetEase(Ease.InOutSine))
                .Join(winImg.DOColor(new Color32(255, 255, 255, 255), 1f).SetEase(Ease.Linear));

        // Add a delay of 1 second
        sequence.AppendInterval(0.01f);

        // Add the second scale animation to the sequence
        sequence.Append(winObj.DOScale(new Vector3(1f, 1f, 1f), 1f).SetEase(Ease.InOutSine));

        // Add a delay of 1 second
        sequence.AppendInterval(0.5f);
        sequence.Append(winObj.DOScale(new Vector3(0f, 0f, 0f), 1f).SetEase(Ease.InOutSine))
                .Join(winImg.DOColor(new Color32(255, 255, 255, 0), 1f).SetEase(Ease.Linear));

        // Start the sequence
        sequence.Play();
    }
    async void TakingCash()
    {
        await UniTask.Delay(3000);
        API_Winning();
    }
    async void WinTxtObj()
    {
        await UniTask.Delay(1000);
        winTxt.transform.DORotate(new Vector3(0f, 360f, 0f), 0.5f, RotateMode.FastBeyond360).SetEase(Ease.InOutSine);
        winTxt.text = takeCash.ToString("0.00" + " <size=70>USD</size>");
        winTxt.color = Color.green;
    }
    async void balloon_Objs()
    {
        await UniTask.Delay(800);
        jumpBallon.SetBool("isJump", false);
        jumpBallon.enabled = false;
        fireObj.transform.localPosition = new Vector3(0f, -758f, 0f);
        blueFire.transform.localPosition = new Vector3(8f, -768.9f, 0f);
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
        WinAmount = 0;
        /*winObj.gameObject.SetActive(false);*/
        isBonus_3 = false;
        take = false;

        /*target.localPosition = new Vector3(0f, 0f, 0f);*/
        /*targetImg.enabled = false;*/
        /*bonus_Balloon.gameObject.SetActive(true);*/
        ScrollViewer();
    }
    async void ScrollViewer()
    {
        await UniTask.Delay(1000); // wait for 2.5 seconds
        ScrollView_Conditions();
    }
    async void WinPanel_SetOFF()
    {
        await UniTask.Delay(1000);
        winPanel.SetActive(false);
    }
    void Bonus_Conditions()
    {
        if (winCash == /*3*/ winBonus)
        {
            bonusObj.gameObject.SetActive(true);
            Fill_Img.instance.Bonus_Script();
            bonusBallon.SetBool("isOpen", true);
            WinPanel_SetOFF();

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

            sliderTxt.text = null;
            sliderAutoCashNoTxt.gameObject.SetActive(false);
        }

        if (betAmount >= TotalAmount)
        {
            Debug.Log("InSufficientBalance");
            InsufficientBalance.SetActive(true);
            return;
        }
        API_IntitalizeBetAmount();
    }
    public void OnClickUp()
    {
        isPressed = false;
        heat_Anim.SetBool("isEnd", false);
        heat_Anim_Img.SetActive(false);

        fire_Anim.SetBool("isFire", false);

        if (!lost)
        {
            balloonParts.SetActive(true);
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

    void BetAmountUpdates()
    {
        if (startGame)
        {
            numPadButton.gameObject.SetActive(false);
        }
        else
        {
            numPadButton.gameObject.SetActive(true);
        }

        if (betAmount >= 0.20)
        {
            minusButton.enabled = true;
            minusButtonImg.color = new Color32(255, 255, 255, 255);
        }
        else if (betAmount <= 0.10)
        {
            minusButton.enabled = false;
            minusButtonImg.color = new Color32(255, 255, 255, 120);
        }


        if (betAmount < 100)
        {
            plusButton.enabled = true;
            plusButtomImg.color = new Color32(255, 255, 255, 255);
        }
        else if (betAmount >= 100)
        {
            plusButton.enabled = false;
            plusButtomImg.color = new Color32(255, 255, 255, 120);
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