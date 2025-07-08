using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Newtonsoft.Json;

public class GameController : MonoBehaviour
{
    #region { ::::::::::::::::::::::::: Headers ::::::::::::::::::::::::: }
    [Header("Float")]
    [SerializeField] public double betAmount = 1f;  // Initial bet amount
    public float Multiplier = 0.00f;  // Initial multiplier value
    public string Mstring;  // Initial multiplier value in String
    [SerializeField] float incrementRate = 1.01f;  // Increment rate
    [SerializeField] float incrementInterval = 0.2f;  // Time interval between increments in seconds
    [SerializeField] float timeSinceLastIncrement = 0f;   // Timer to track time since last increment
    [SerializeField] float maxHoldTime = 5f; // Maximum time to hold the button
    [SerializeField] float minHoldTime = 2f; // Minimum time to hold the button
    [SerializeField] float holdHeight = 2f; // Minimum time to hold the button
    [SerializeField] float timeHeld = 0f;   // Timer to track time button is held
    [SerializeField] string timeHold;   // Timer to track time 
    public double TakeCash;  // TakeCash
    public string Tstring;  // TakeCash in String
    [SerializeField] int gameCounts;
    [SerializeField] double TotalAmount = 250.00f;  // Total Amount
    public float Bonustimer;

    public float Difference;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("Script")]
    [SerializeField] KeyBoardHandler keyBoard;
    [SerializeField] SettingsPanelHandler settingsPanelHandler;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("Buttons")]
    [SerializeField] Button takeCashbutton;
    [SerializeField] Button holdButton;
    [SerializeField] TextMeshProUGUI heatTxt;
    [SerializeField] TMP_Text[] unSelectedBtnTxt;
    [SerializeField] TMP_Text[] SelectedBtnTxt;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    //UI bet Buttons
    [Header("UI bet Buttons")]
    [SerializeField] Button button_1;
    [SerializeField] Button button_2;
    [SerializeField] Button button_5;
    [SerializeField] Button button_10;

    [SerializeField] List<Button> setected_Buttons = new List<Button>();
    [SerializeField] List<Button> pressed_Buttons = new List<Button>();
    [SerializeField] List<Button> unclicked_Buttons = new List<Button>();
    [SerializeField] Button plusButton, minusButton;

    [SerializeField] Image[] selected_Anim;
    [SerializeField] TMP_Text[] btnAmtTxt;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("UI GameObjects")]
    [SerializeField] GameObject[] button_Anim;
    [SerializeField] GameObject numPadButton;
    [SerializeField] GameObject takeCashObj;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("UI GameObjects")]
    [SerializeField] GameObject unPress;
    [SerializeField] GameObject pressed;
    [SerializeField] GameObject BetArea_numPad;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("TakeCash TextMeshProUGUI")]
    [SerializeField] TextMeshProUGUI takeCashtxt;
    [SerializeField] TextMeshProUGUI takeCashWintxt;
    [SerializeField] TextMeshProUGUI takeCurrencytxt;
    [SerializeField] Image takeCashImg;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    //UI bet Buttons Imgaes
    [Header("UI bet Buttons Images")]
    [SerializeField] Image plusButtomImg;
    [SerializeField] Image minusButtonImg;

    // [SerializeField] EventTrigger holdButtonEvent;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    // Boolean
    [Header("Boolean")]
    public bool isAutoPlay;
    public bool isScroll;
    public bool winCount;
    public bool timer;
    public bool numPad;
    public bool isBonus;
    public bool isBonus_OFF;
    public bool demo;
    public bool numBool;
    public bool bonusCount;
    public bool isWin;
    public bool netCheck;
    [SerializeField] bool isCreateMatchSucceess = false;
    // private
    [SerializeField] public bool startGame;
    [SerializeField] public bool _balanceUpdate = false;
    [SerializeField] public bool onClick;
    [SerializeField] bool isChecked;
    [SerializeField] bool makeLose;
    private bool isBegin;
    private bool pauseGame;
    private bool isPressed;
    private bool buttonPress;
    private bool takeBetAmount;
    private bool isSet;
    private bool isFire;
    private bool lost;
    private bool gameLost;
    public bool take;
    private bool isBonus_1;
    private bool isBonus_2;
    private bool isBonus_3;
    private bool isNormal;
    private bool touch;
    private bool stopper;
    private bool timerCount = true;
    private bool IsCreateMatchCalled = false;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    // AutoPlay
    [Header("AutoPlay")]
    public List<int> setRounds = new List<int> { 9, 19, 49, 99, int.MaxValue };
    [SerializeField] public GameObject autoPlayPanel;
    [SerializeField] AutoplayInputHandler[] autoplayInputHandler;
    [SerializeField] bool autoPlayBtnPrss;
    [SerializeField] bool stopAutoPlay;
    [SerializeField] bool resetToggle, numClick;
    [SerializeField] int rounds;
    [SerializeField] float targetMultiplier, stopCashDecrease, stopSingleWin;
    [SerializeField] float check_stopCashDecrease, check_stopSingleWin;
    [SerializeField] double totalCash;
    [SerializeField] Toggle stop_CashDecrease, stop_SingleWin;
    [SerializeField] List<Button> roundSetBtns = new List<Button>();
    [SerializeField] List<Button> selectedroundBtns = new List<Button>();
    [SerializeField] Button addValBtns, subValBtns;
    [SerializeField] Button autoPlayBtn, stopAutoPlayBtn;
    [SerializeField] Button startBtn;
    public Button resetBtn;
    [SerializeField] Image addValBtnsImg, subValBtnsImg, infiniteImg;
    [SerializeField] TMP_InputField targetInputField;
    [SerializeField] TMP_Text startBtn_Txt, restartBtn_Txt;
    [SerializeField] TMP_Text autoPlay_Txt;
    [SerializeField] TMP_Text roundsTxt;


    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    // TextMeshProUGUI
    [Header("TextMeshProUGUI")]
    [SerializeField] TextMeshProUGUI multiplierTxt;
    [SerializeField] TextMeshProUGUI xTxt;
    [SerializeField] TextMeshProUGUI multiplierTxt_Shadow;
    [SerializeField] TextMeshProUGUI xTxt_Shadow;
    [SerializeField] TextMeshProUGUI takeCashTxt;
    [SerializeField] TextMeshProUGUI totalAmountTxt;
    [SerializeField] TextMeshProUGUI takeCurrenyType;
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
    [Header("Balloon Parts")]
    [SerializeField] GameObject ballonOut;
    [SerializeField] GameObject flewAway_Txt;
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
    /// <summary>
    [SerializeField] GameObject sliderbg;
    [SerializeField] GameObject pressToBetBtn;
    /// 
    /// </summary>
    [SerializeField] GameObject fillArea;
    //[SerializeField] Image FillImage;
    [SerializeField] GameObject slider_txt;
    [SerializeField] GameObject keepHolding_txt;
    [SerializeField] GameObject preHeating_txt;
    [SerializeField] TextMeshProUGUI sliderTxt;
    [SerializeField] TextMeshProUGUI sliderDupTxt;
    [SerializeField] TextMeshProUGUI sliderAutoCashTxt;
    [SerializeField] TextMeshProUGUI sliderAutoCashNoTxt;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    // DoTween Win_Images 
    [Header("DoTween Win_Images")]
    [SerializeField] GameObject winPanel;
    [SerializeField] TextMeshProUGUI winTxt;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    // Bonus Reward Fill Img
    [Header("Bonus Reward Fill Img")]
    [SerializeField] float totalTime = 10f;  // Total time for the timer
    private float timeRemaining;    // Time remaining for the timer

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    // Bonuse Rewards Winning int
    [Header("Bonus Rewards Winning int")]
    [SerializeField] float winCash;
    public float Winbonus;
    public float WinCash_demo;

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
    public TextMeshProUGUI BonusRewardTxt;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    // ScrollView GameObjects
    [Header("Animator")]
    // balloon
    [SerializeField] Animator ballon_Anim;
    [SerializeField] Animator ballon_shake;
    // Heat button
    [SerializeField] Animator heat_Anim;
    [SerializeField] Animator heat_IdleAnim;
    [SerializeField] GameObject takeCash_Anim;
    [SerializeField] GameObject fireObj;
    [SerializeField] GameObject fireIdleObj;
    [SerializeField] GameObject idleFireObj;
    [SerializeField] GameObject idleFireGlow;
    [SerializeField] List<Animator> unsetected_Buttons = new List<Animator>();
    // slider
    [SerializeField] Animator slider_Anim;
    [SerializeField] Animator sliderBg_Anim;
    // bonus Balloon
    [SerializeField] Animator bonusBallon;

    // Minus & Plus Anim
    [SerializeField] Animator minus_Anim;
    [SerializeField] Animator plus_Anim;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    // ScrollView GameObjects
    [Header("Insufficient Balance")]
    [SerializeField] GameObject insufficientBalance;
    [SerializeField] GameObject insufBal_Rumblebets;
    [SerializeField] GameObject cancelButton;
    [SerializeField] GameObject rumbleBet_cancelButton;
    [SerializeField] GameObject howToPlay;
    [SerializeField] GameObject amountGlow;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("HandGestures")]
    [SerializeField] GameObject handGestures_start;
    [SerializeField] GameObject handGestures_btAmt;
    [SerializeField] GameObject handGestures_start_Img;
    [SerializeField] GameObject handGestures_btAmt_Img;
    [SerializeField] Collider heatbtnCollider;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("MaxBet_Reached")]
    [SerializeField] GameObject maxBet_Reached;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("Audio Script")]
    [SerializeField] MasterAudioController audioController;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("Panel")]
    [SerializeField]
    public GameObject RedirectionPanel;
    public ImageSequencer imageSequencer;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("IDLE_TimerCount")]
    [SerializeField] float currentTime;
    [SerializeField] float startCount = 10f;
    [SerializeField] GameObject sessionTimeOut;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("GameLimits_TMP-Txt")]
    [SerializeField] private TMP_Text minBetTxt;
    [SerializeField] private TMP_Text maxBetTxt;
    [SerializeField] private TMP_Text maxWinOneBetTxt;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    // ScrollView GameObjects
    [Header("API Controller")]
    private string currencyType;
    private string playerID;
    private string playerName;
    private string operatorName;
    private string gameName;
    private string lobbyName;
    private string betID;
    private int BetIndex;
    public CreateMatchResponse MatchRes;
    private bool IsInTab = true;
    public bool CanPlayAudio;
    [SerializeField] GameObject LoadingPopUp;
    /*[SerializeField] GameObject ResponsePopUp;*/
    [SerializeField] Image inputField;
    public bool needToCancelBet;
    public static GameController instance;

    #endregion ::::::::::::::::::::::::: END :::::::::::::::::::::::::
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------//
    private void Awake()
    {
        instance = this;
        LoadingPopUp.SetActive(true);
        audioController.muteAllAudio = true;
        AudioListener.volume = 0;
        CanPlayAudio = false;
        takeCashbutton.interactable = false;
        settingsPanelHandler.settingsBtn.onClick.AddListener(() => UI_Controller.instance.settingsHandler.CallingSettingPanel());
        settingsPanelHandler.settingsCloseBtn.onClick.AddListener(() => UI_Controller.instance.settingsHandler.HideMe());

        // AutoPlay
        setRounds = new List<int> { 9, 19, 49, 99, int.MaxValue };

    }
    private void Start()
    {
        // Subscribe to API events
        APIController apiController = APIController.instance;
        apiController.OnSwitchingTab += OnSwitchTab;
        apiController.OnUserDetailsUpdate += InitPlayerDetails;
        apiController.OnUserBalanceUpdate += InitAmountDetails;
        apiController.OnUserDeposit += InitUserDeposit;

        // Initialize button states
        unPress.SetActive(true);
        pressed.SetActive(false);
        touch = true;
        takeBetAmount = true;
        Difference = 0;

        // Initialize take cash UI elements
        Color32 disabledColor = new Color32(194, 236, 166, 120);
        takeCashImg.color = new Color32(140, 140, 140, 255);
        takeCashtxt.color = disabledColor;
        takeCashWintxt.color = disabledColor;
        takeCurrencytxt.color = disabledColor;
        takeCashObj.SetActive(false);

        // Initialize multiplier text
        string multiplierText = Multiplier.ToString("0.00");
        multiplierTxt.text = multiplierText;
        multiplierTxt_Shadow.text = multiplierText;

        // Start coroutines
        StartCoroutine(HolidngButtons());
        StartCoroutine(nameof(TimerCount));
        StartCoroutine(FillImg());

        // Settings Button
        settingsPanelHandler.settingsCloseBtn.gameObject.SetActive(false);
        settingsPanelHandler.settingsBtn.gameObject.SetActive(true);

        // Background position
        initialBackgroundPosition = background.localPosition;

        // Initialize slider
        Winbonus = 3;
        slider.maxValue = 7f;
        slider.minValue = 1f;
        timeRemaining = 0f; // Start the timer at 0


        // Start coroutines for IDLE_TimerCount
        currentTime = startCount;
        //StartCoroutine(nameof(CoundownTimerforIdle));

        takeCashbutton.onClick.AddListener(() => TakeCashOut());

        for (int i = 0; i < setected_Buttons.Count; i++)
        {
            int index = i; // Capture the loop variable to avoid closure issues
            setected_Buttons[index].onClick.AddListener(() =>
                SelectBetButton((int)APIController.instance.authentication.entryAmountDetails.betValues[index], index));
        }

        for (int i = 0; i < pressed_Buttons.Count; i++)
        {
            int index = i; // Capture the loop variable to avoid closure issues
            pressed_Buttons[index].onClick.AddListener(() =>
                BetButtonPressed((int)APIController.instance.authentication.entryAmountDetails.betValues[index], index));
        }

        #region :::::::::::: { Auto Play } :::::::::::::::
        // For AutoPlay
        for (int i = 0; i < roundSetBtns.Count; i++)
        {
            int index = i;
            roundSetBtns[index].onClick.AddListener(() =>
            NumCountRounds(setRounds[index]));
        }

        //addValBtns.onClick.AddListener(() => PlusTargetValue());
        //subValBtns.onClick.AddListener(() => MinusTargetValue());

        autoPlayBtn.onClick.AddListener(() => AutoPlayBtnPress());
        stopAutoPlayBtn.onClick.AddListener(() => Stop_AutoPlayBtn());
        startBtn.onClick.AddListener(() => Start_AutoPlayBtn());
        resetBtn.onClick.AddListener(() => Reset_AutoPlayBtn());


        #endregion :::::::::::: { Auto Play } :::::::::::::::

        // Initialize slider-related objects
        slider_bg.SetActive(false);
        fillArea.SetActive(false);
        slider_txt.SetActive(false);
        keepHolding_txt.SetActive(false);
        //sliderAutoCashNoTxt.gameObject.SetActive(false);
        sliderAutoCashNoTxt.text = " ";

        netCheck = false;
    }
    private void LateUpdate()
    {
        if (!isAutoPlay)
        {
            // Get ray origin and direction
            Vector3 rayOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 rayDirection = Vector3.forward;
            bool isRayHitActive = false;

            // Raycast to detect the Heat_button
            if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit) && hit.collider.gameObject.name == "Heat_button")
            {
                // Handle Mouse Button Down
                if (Input.GetMouseButtonDown(0))
                {
                    isRayHitActive = true;

                    if (keyBoard.gameObject.activeInHierarchy)
                        keyBoard.OnCancelInput();

                    if (!take && !lost && !isScroll && !howToPlay.activeSelf && !numPad && !insufficientBalance.activeSelf && !insufBal_Rumblebets.activeSelf && !RedirectionPanel.activeSelf && !autoPlayPanel.activeSelf &&
                        !NetworkHandler.instance.ConnectionPanel.activeSelf && !NetworkHandler.instance.waitingForResponse.activeSelf && !settingsPanelHandler.gameObject.activeSelf && !NetworkHandler.instance.ServerPopPanel.activeSelf &&
                        !pauseGame && betAmount >= APIController.instance.authentication.entryAmountDetails.minBetValue && !settingsPanelHandler.GameLimits.activeSelf && !NetworkHandler.instance.SessionPopup.activeSelf)
                    {
                        Button_ONEnter();
                        OnClickDown();
                    }
                    else if (NetworkHandler.instance.ConnectionPanel.activeSelf && isPressed)
                    {
                        isPressed = false;
                        Button_OFFEnter();
                    }
                }

                // Handle Mouse Button Held
                if (Input.GetMouseButton(0))
                {
                    isRayHitActive = true;
                }
            }


            // Handle Mouse Button Release or when no ray hit
            if (!isRayHitActive)
            {
                if (!take && !lost && !isScroll && !howToPlay.activeSelf && !numPad && !RedirectionPanel.activeSelf && !NetworkHandler.instance.SessionPopup.activeSelf &&
                    !NetworkHandler.instance.ConnectionPanel.activeSelf && !settingsPanelHandler.gameObject.activeSelf &&
                    !pauseGame && betAmount >= APIController.instance.authentication.entryAmountDetails.minBetValue && !settingsPanelHandler.GameLimits.activeSelf)
                {
                    OnClickUp();
                    Button_OFFEnter();
                }
            }

            // Enable/Disable cancel buttons based on TotalAmount
            bool isAmountSufficient = TotalAmount >= APIController.instance.authentication.entryAmountDetails.minBetValue;
            cancelButton.SetActive(isAmountSufficient);
            rumbleBet_cancelButton.SetActive(isAmountSufficient);
        }

        // Handle animation based on internet connectivity
        if (!NetworkHandler.instance.ConnectionPanel.activeSelf)
        {
            Animation_Play();
        }
        else
        {
            Animation_Pause();
        }

        if (isAutoPlay)
        {
            if (!take && !lost && !isScroll && !howToPlay.activeSelf && !numPad && !insufficientBalance.activeSelf && !insufBal_Rumblebets.activeSelf && !RedirectionPanel.activeSelf &&
                        !NetworkHandler.instance.ConnectionPanel.activeSelf && !NetworkHandler.instance.waitingForResponse.activeSelf && !settingsPanelHandler.gameObject.activeSelf &&
                        !pauseGame && betAmount >= APIController.instance.authentication.entryAmountDetails.minBetValue && rounds >= 0 && !settingsPanelHandler.GameLimits.activeSelf && !NetworkHandler.instance.SessionPopup.activeSelf)
            {
                Button_ONEnter();
                OnClickDown();
                Button_SwitchingOFF();
            }

            if (isAutoPlay && isPressed)
            {
                if (GetTruncatedValue_float(Multiplier) >= Difference)
                {
                    Multiplier = Mathf.Clamp(Multiplier, 0f, Difference);
                    multiplierTxt_Shadow.text = $"{Multiplier:F2}";
                    multiplierTxt.text = $"{Multiplier:F2}";
                    Debug.Log($"CheckingForValue >>> : " + TakeCash);
                    isPressed = false;
                    //TakeCashOut();
                    timeSinceLastIncrement = 7f;
                    OnClickUp();
                    Button_OFFEnter();
                }
            }

            bool isAmountSufficient = TotalAmount >= APIController.instance.authentication.entryAmountDetails.minBetValue;
            cancelButton.SetActive(isAmountSufficient);
            rumbleBet_cancelButton.SetActive(isAmountSufficient);

            if ((rounds > -1 && rounds <= 0))
            {
                Stop_AutoPlayBtn();
                rounds = -1;
            }

            if (insufficientBalance.activeSelf || insufBal_Rumblebets.activeSelf)
            {
                stopAutoPlay = false;
                Stop_AutoPlayBtn();
                TimeDelay();
            }

        }
    }
    #region { ::::::::::::::::::::::::: API ::::::::::::::::::::::::: }
    public void InitPlayerDetails()
    {
        DebugHelper.Log("UserDetails" + APIController.instance.userDetails.Id + " player ID " + playerID);

        playerID = APIController.instance.userDetails.Id;
        playerName = APIController.instance.userDetails.name;
        UI_Controller.instance.settingsHandler.playerNameTxt.text = playerName;
        operatorName = APIController.instance.userDetails.game_Id.Split("_")[0].ToString();
        gameName = APIController.instance.userDetails.game_Id.Split("_")[1].ToString();
        lobbyName = "Room : " + DateTime.UtcNow + UnityEngine.Random.Range(100, 999);
        if (APIController.instance.authentication.operatorname == "demo")
            demo = true;
        else
            demo = false;

        if (LoadingPopUp.activeSelf)
        {
            LoadingPopUp.SetActive(false);
            CanPlayAudio = true;
            settingsPanelHandler.HideMe();
            Welcom_Button();
            audioController.BackgroundAudio.PlayAudioBGM();
        }

        for (int i = 0; i < 4; i++)
        {
            unSelectedBtnTxt[i].text = APIController.instance.authentication.entryAmountDetails.betValues[i].ToString();
            SelectedBtnTxt[i].text = APIController.instance.authentication.entryAmountDetails.betValues[i].ToString();
        }

        //////////////////////////////////////////////////////////////////////
        betAmount = APIController.instance.authentication.entryAmountDetails.minBetValue;
        betAmountTxt.text = $"{betAmount:F2} <size=30>{currencyType}</size>";
        ButtonSelect_Anim();
        idleFireObj.SetActive(false);
        fireObj.SetActive(true);
        fireIdleObj.SetActive(true);
        unPress.SetActive(true);
        pressed.SetActive(false);
        ////////////////////////////////////////////////////////////////////// 

        //// Game Limits /////
        minBetTxt.text = $"{APIController.instance.authentication.entryAmountDetails.minBetValue.ToString("N2", new System.Globalization.CultureInfo("en-IN"))} <size=25>{currencyType}</size>";
        maxBetTxt.text = $"{APIController.instance.authentication.entryAmountDetails.maxBetValue.ToString("N2", new System.Globalization.CultureInfo("en-IN"))} <size=25>{currencyType}</size>";

        if (APIController.instance.authentication.operatorname == "demo")
            maxWinOneBetTxt.text = $"{10000.ToString("N2", new System.Globalization.CultureInfo("en-IN"))} <size=25>{APIController.instance.userDetails.currency_type}</size>";
        else
            maxWinOneBetTxt.text = $"{100000.ToString("N2", new System.Globalization.CultureInfo("en-IN"))} <size=25>{APIController.instance.userDetails.currency_type}</size>";
        //// Game Limits /////


        DebugHelper.Log("Player Details Subscribed");
        settingsPanelHandler.SetToggleValueFromAPI(APIController.instance.authentication.sound, APIController.instance.authentication.music);
        audioController.muteAllAudio = false;
    }
    public void InitAmountDetails()
    {
        TotalAmount = APIController.instance.userDetails.balance;
        string m = TotalAmount.ToString("0.00");
        TotalAmount = double.Parse(m);
        currencyType = APIController.instance.userDetails.currency_type;
        PassTxt(totalAmountTxt, APIController.instance.userDetails.currency_type);
        PassTxt(takeCurrenyType, APIController.instance.userDetails.currency_type);
        /*PassTxt(betAmountTxt, betAmount.ToString("0.00") + " " + APIController.instance.userDetails.currency_type);*/
        PassTxt(betAmountTxt, $"{betAmount:F2} <size={totalAmountTxt.fontSize - 10}>{APIController.instance.userDetails.currency_type}</size>");
        PassTxt(totalAmountTxt, $"{TotalAmount:F2} <size={totalAmountTxt.fontSize - 10}>{APIController.instance.userDetails.currency_type}</size>");
        DebugHelper.Log("Amount Details Subscribed");
    }
    private void OnSwitchTab(bool isFocus)
    {
        DebugHelper.Log($"SwitchTab Status Check ********** {isFocus} || IsinFocus = {APIController.instance.isInFocus} || IsOnline {APIController.instance.isOnline}");
        if (CanPlayAudio)
        {
            AudioListener.volume = (isFocus && APIController.instance.isOnline && APIController.instance.isInFocus) ? 1 : 0;

            if (isFocus)
            {
                if (!netCheck && !NetworkHandler.instance.ConnectionPanel.activeSelf)
                {
                    if ((DateTime.Now - NetworkHandler.instance.GetLastActiveTime()).TotalSeconds >= 60)
                    {
                        DebugHelper.Log($"Session Check on Switch Tab Success");
                        if (!NetworkHandler.instance.SessionPopup.activeSelf)
                            NetworkHandler.instance.SessionPopup.SetActive(true);
                    }
                    else
                    {
                        DebugHelper.Log($"Session Check on Switch Tab Entered");
                        NetworkHandler.instance.SetDelay(60 - (DateTime.Now - NetworkHandler.instance.GetLastActiveTime()).TotalSeconds);
                        NetworkHandler.instance.StartIdleSession();
                    }
                }
            }
        }
        IsInTab = isFocus;
    }
    public void InitUserDeposit()
    {
        DebugHelper.Log("Deposit Called");
    }
    public void PassTxt(TMP_Text _txt, string _passingValue)
    {
        _txt.text = _passingValue;
    }
    #endregion ::::::::::::::::::::::::: END :::::::::::::::::::::::::

    #region { ::::::::::::::::::::::::: Coroutine ::::::::::::::::::::::::: }
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
        if (startGame && !pauseGame && !take && (!NetworkHandler.instance.ConnectionPanel.activeSelf))
        {
            if (Multiplier < 1.01f && isChecked)
            {
                countTime += 7f * Time.deltaTime;
                slider.value = countTime;
                if (!isAutoPlay)
                    sliderAutoCashNoTxt.text = countTime.ToString(" 0");
            }
            if (Multiplier >= 1.01f && !isPressed && !take)
            {
                timeSinceLastIncrement += Time.deltaTime;

                slider.gameObject.SetActive(true);

                countTime -= 1f * Time.deltaTime;
                //sliderAutoCashNoTxt.text = countTime.ToString(" 0");
                audioController.StopAudio(AudioEnum.startSlider);
                slider.value = countTime;
                if (countTime < 1)
                {
                    countTime = 0;
                }
                if (!isAutoPlay)
                    sliderAutoCashNoTxt.text = countTime.ToString(" 0");
            }
            else if (Multiplier >= 1.01f && isPressed)
            {
                countTime = 7f;
                slider.maxValue = 7f;
                slider.value = 7f;
            }
        }
        yield return null;
        StartCoroutine(nameof(TimerCount));
    }
    IEnumerator HolidngButtons()
    {
        // Handle bet amount updates
        if (!startGame && !onClick)
            BetAmountUpdates();

        Check_AutoplayValue();
        StartBtnHadler();

        // Handle internet disconnection and button press
        if (NetworkHandler.instance.ConnectionPanel.activeSelf && isPressed)
        {
            isPressed = false;
            Button_OFFEnter();
        }

        // Disable minus button if bet amount is below minimum
        if (betAmount <= APIController.instance.authentication.entryAmountDetails.minBetValue)
        {
            minusButton.interactable = false;
            minusButtonImg.color = new Color32(255, 255, 255, 100);
        }

        // Handle insufficient balance display based on operator
        if (buttonPress == true)
        {
            if (APIController.instance.authentication.operatorname == "demo")
            {
                insufficientBalance.SetActive(true);
                insufBal_Rumblebets.SetActive(false);
            }
            else
            {
                insufficientBalance.SetActive(false);
                insufBal_Rumblebets.SetActive(true);
                Debug.Log("InsufficientPopUpAppers ==>>>_1 : " + buttonPress);
            }
        }

        if (startGame && !take)
        {
            Color color = betAmountTxt.color;
            color.a = 0.5f;
            betAmountTxt.color = color;

            for (int i = 0; i < btnAmtTxt.Length; i++)
            {
                if (btnAmtTxt[i] != null)
                {
                    Color color1 = btnAmtTxt[i].color;
                    color1.a = 0.5f;
                    btnAmtTxt[i].color = color1;
                }
            }
        }

        // Handle take cash scenario
        if (takeBetAmount)
        {
            winPanel.SetActive(false);
        }

        // Handle bonus balloon
        BonusBalloon();

        // Bonus Scroll View
        if (isScroll)
        {
            //ApplyBalloonParallaxEffect(holdButton.GetComponent<RectTransform>().anchoredPosition.y);
            ApplyParallaxEffect(holdButton.GetComponent<RectTransform>().anchoredPosition.y);

            heat_Anim.SetBool("isPlay1", false);
            heat_IdleAnim.SetBool("isPlay1", false);
            fireObj.SetActive(false);
            fireIdleObj.SetActive(false);
            takeCashbutton.interactable = false;
            takeCashImg.color = new Color32(140, 140, 140, 255);
            takeCashtxt.color = new Color32(194, 236, 166, 120);
            takeCashWintxt.color = new Color32(194, 236, 166, 120);
            takeCurrencytxt.color = new Color32(194, 236, 166, 120);
            heatTxt.color = new Color32(63, 15, 15, 200);
        }
        // Game Lose
        if (lost)
        {
            startGame = false;
            onClick = false;
            preHeating_txt.gameObject.SetActive(false);
            audioController.StopAudio(AudioEnum.reverseSlider);
            audioController.StopAudio(AudioEnum.startSlider);
            audioController.StopAudio(AudioEnum.Movement);
            ballon_Anim.SetBool("isOut", true);
            // sliderOBjs
            slider_Anim.SetBool("isOFF", true);
            sliderBg_Anim.SetBool("isFalse", true);
            slider_bg.SetActive(false);
            fillArea.SetActive(false);
            slider_txt.SetActive(false);
            keepHolding_txt.SetActive(false);
            //sliderAutoCashNoTxt.gameObject.SetActive(false);
            sliderAutoCashNoTxt.text = " ";
            //heat button
            heat_Anim.SetBool("isPlay1", false);
            heat_Anim.SetBool("isPlay2", false);
            heat_IdleAnim.SetBool("isPlay1", false);
            idleFireGlow.SetActive(false);
            idleFireObj.SetActive(true);
            fireObj.SetActive(false);
            fireIdleObj.SetActive(false);
            //balloon parts
            ballonOut.gameObject.SetActive(true);
            balloonParts.SetActive(false);
            ballon_shake.SetBool("itsON", false);
            balloonShake.SetActive(false);
            balloonShake_blue.SetActive(false);
            //takeCash & heat button 
            takeCashImg.color = new Color32(140, 140, 140, 255);
            takeCashtxt.color = new Color32(194, 236, 166, 120);
            takeCashWintxt.color = new Color32(194, 236, 166, 120);
            takeCurrencytxt.color = new Color32(194, 236, 166, 120);
            heatTxt.color = new Color32(63, 15, 15, 200);
            takeCashObj.SetActive(false);
        }
        // Taking cash
        if (take)
        {
            startGame = false;
            onClick = false;
            unPress.SetActive(true);
            pressed.SetActive(false);
            sliderAutoCashNoTxt.text = " ";
            audioController.StopAudio(AudioEnum.reverseSlider);
            audioController.StopAudio(AudioEnum.Movement);
            takeCashbutton.interactable = false;
            // sliderOBjs
            slider_Anim.SetBool("isOFF", true);
            sliderBg_Anim.SetBool("isFalse", true);
            slider_bg.SetActive(false);
            fillArea.SetActive(false);
            slider_txt.SetActive(false);
            keepHolding_txt.SetActive(false);
            //sliderAutoCashNoTxt.gameObject.SetActive(false);
            sliderAutoCashNoTxt.text = " ";
            //balloon, takeCash & heat button
            heat_Anim.SetBool("isPlay1", false);
            heat_IdleAnim.SetBool("isPlay1", false);
            idleFireGlow.SetActive(false);
            idleFireObj.SetActive(true);
            fireObj.SetActive(false);
            fireIdleObj.SetActive(false);
            ballon_shake.SetBool("itsON", false);
            balloonShake_blue.SetActive(false);
            balloonShake.SetActive(false);
            takeCashObj.SetActive(false);
            balloonParts.SetActive(true);
            heatTxt.color = new Color32(63, 15, 15, 200);
        }

        FireButton();
        if (isChecked)
            StartOfTheGame();

        if (startGame && !takeBetAmount)
        {
            if (Multiplier >= holdHeight)
            {
                //DebugHelper.Log("mStringValue =====> " + float.Parse(Mstring) + holdHeight);
                gameLost = true;
                lost = true;
                Balloon_Burt();
            }
        }
        if (startGame && isPressed && isFire && !lost)
        {
            // To move the Target pos Up at the Start
            ApplyParallaxEffect(holdButton.GetComponent<RectTransform>().anchoredPosition.y);

            //startGame = true;

            if (timeSinceLastIncrement >= incrementInterval)
            {
                IncrementMultiplier();
                timeSinceLastIncrement = 0f; // Reset the timer
            }
        }
        // Check if the timer has reached 7 seconds and no button is pressed
        else if (timeSinceLastIncrement >= 6f)
        {
            // Automatically take cash
            heatbtnCollider.enabled = false;
            TakeCashOut();
            holdButton.enabled = false;
            takeCashbutton.interactable = false;
            // Reset the timer
            timeSinceLastIncrement = 5.9f;
            timeHeld = 0f;
            isPressed = false;
        }
        if (bonusCount && APIController.instance.isOnline && !NetworkHandler.instance.ConnectionPanel.activeSelf)
        {
            TakeCashOut();
            bonusCount = false;
        }
        yield return null;
        StartCoroutine(nameof(HolidngButtons));
    }
    IEnumerator CoundownTimerforIdle()
    {
        if (!startGame)
        {
            if (!LoadingPopUp.activeSelf && !take && !lost && !isScroll && !howToPlay.activeSelf && !numPad && !insufficientBalance.activeSelf && !insufBal_Rumblebets.activeSelf &&
                        !NetworkHandler.instance.ConnectionPanel.activeSelf && !NetworkHandler.instance.waitingForResponse.activeSelf && !settingsPanelHandler.gameObject.activeSelf
                        && !autoPlayPanel.activeSelf && !settingsPanelHandler.GameLimits.activeSelf && !RedirectionPanel.activeSelf && !NetworkHandler.instance.ServerPopPanel.activeSelf)
            {
                currentTime -= 1 * Time.deltaTime;

                if (currentTime <= 0)
                {
                    currentTime = 0;
                    sessionTimeOut.SetActive(true);
                }
            }
            else if (!LoadingPopUp.activeSelf && !take && !lost && !isScroll && !howToPlay.activeSelf && !numPad && !insufficientBalance.activeSelf && !insufBal_Rumblebets.activeSelf &&
                        !NetworkHandler.instance.ConnectionPanel.activeSelf && !NetworkHandler.instance.waitingForResponse.activeSelf && !settingsPanelHandler.gameObject.activeSelf
                        && !autoPlayPanel.activeSelf && !settingsPanelHandler.GameLimits.activeSelf && !RedirectionPanel.activeSelf && !NetworkHandler.instance.ServerPopPanel.activeSelf)
            {
                currentTime = 60;
                sessionTimeOut.SetActive(false);
            }

            if ((audioController != null && audioController.IsAnyAudioPlaying()))
            {
                currentTime = 60;
                sessionTimeOut.SetActive(false);
            }
        }
        else if (startGame)
        {
            currentTime = 60;
            sessionTimeOut.SetActive(false);
        }
        yield return null;
        StartCoroutine(nameof(CoundownTimerforIdle));
    }
    #endregion ::::::::::::::::::::::::: END :::::::::::::::::::::::::
    public async void AmountColor_Glow()
    {
        amountGlow.SetActive(false);
        amountGlow.SetActive(true);
        await UniTask.Delay(350);
        amountGlow.SetActive(false);
    }
    void BonusBalloon()
    {
        if (isBonus_1 || isBonus_2 || isBonus_3 || isScroll)
        {
            balloonBlue_Start.gameObject.SetActive(false);
        }
    }
    void Balloon_Burt()
    {
        ResetBets();
        audioController.PlayAudio(AudioEnum.ballonPopOut);
        holdButton.enabled = false;
        timeSinceLastIncrement = 0f; // Reset the timer
        timeHeld = 0f; // Reset the time button is held
        winCash = 0f;
        _balanceUpdate = true;

        // sliderOBjs
        slider_Anim.SetBool("isOFF", true);
        sliderBg_Anim.SetBool("isFalse", true);
        slider_bg.SetActive(false);
        fillArea.SetActive(false);
        slider_txt.SetActive(false);
        keepHolding_txt.SetActive(false);
        sliderAutoCashNoTxt.text = " ";
        takeCashObj.SetActive(false);
    }
    void FireButton()
    {
        if (isFire)
        {
            if (isFire && Multiplier >= 1.01f && !isPressed)
            {
                if (touch)
                {
                    audioController.PlayAudio(AudioEnum.reverseSlider, true);
                    touch = false;
                }
            }
        }
        if (!isFire)
        {
            audioController.StopAudio(AudioEnum.startSlider);
            audioController.StopAudio(AudioEnum.Movement);
            isPressed = false;
            heat_Anim.SetBool("isPlay2", false);
            fireObj.SetActive(false);
            if (!lost && !take && Multiplier >= 1.01f && !isScroll)
            {
                balloonShake_blue.SetActive(true);
                balloonShake.SetActive(false);
                ballon_shake.SetBool("itsON", false);
            }
        }

        if (!isFire && Multiplier >= 1.01f)
        {
            if (touch)
            {
                audioController.PlayAudio(AudioEnum.reverseSlider, true);
                touch = false;
            }
        }
    }
    private float GetTruncatedValue(double value)
    {
        return (float)(Math.Floor((decimal)(value) * 100) / 100);
    }
    private float GetTruncatedValue_float(float value)
    {
        return (float)(Math.Floor((decimal)(value) * 100) / 100);
    }
    void StartOfTheGame()
    {
        timeHold = GetTruncatedValue_float(Multiplier).ToString("F2");
        Mstring = GetTruncatedValue_float(Multiplier).ToString("F2");
        takeCashWintxt.text = GetTruncatedValue(TakeCash).ToString("F2");

        if (startGame && Multiplier <= incrementRate)
        {
            stopper = true;
        }
        if (startGame && !lost)
        {
            if (isAutoPlay)
                Multiplier = Mathf.Clamp(Multiplier, 0f, Difference);

            Mstring = GetTruncatedValue_float(Multiplier).ToString("F2");
            Debug.Log($"Check_BetAmount : {betAmount} , {Mstring}");
            TakeCash = (betAmount * double.Parse(Mstring));

            takeCashWintxt.text = GetTruncatedValue(TakeCash).ToString("F2");
            Debug.Log($"Multipler1 TakeCash==>>> : {TakeCash}");
        }

        if (stopper)
        {
            stopper = false;
            Multiplier += Time.deltaTime;
            Multiplier = Mathf.Min(Multiplier);

            if (isAutoPlay)
                Multiplier = Mathf.Clamp(Multiplier, 0f, Difference);

            string s = GetTruncatedValue_float(Multiplier).ToString("F2");
            multiplierTxt.text = s;
            multiplierTxt_Shadow.text = s;
            balloon_Objs();
        }
        else if (isPressed && startGame && Multiplier >= 1.01f)
        {
            balloonParts.SetActive(false);
            ballon_shake.SetBool("itsON", true);

            if (isAutoPlay)
                Multiplier = Mathf.Clamp(Multiplier, 0f, Difference);

            string s = GetTruncatedValue_float(Multiplier).ToString("F2");
            multiplierTxt.text = s;
            multiplierTxt_Shadow.text = s;

            Debug.Log("CheckingTakeCash Value : " + GetTruncatedValue(TakeCash));
            takeCashTxt.text = GetTruncatedValue(TakeCash).ToString("F2");

            // Increment the timer by the time elapsed since the last frame
            timeSinceLastIncrement += Time.deltaTime;
        }

        if (startGame && takeBetAmount)
        {
            takeBetAmount = false;
            holdButton.enabled = false;
        }

        if (startGame)
        {
            Button_Switch_OFF();
            ApplyBalloonParallaxEffect(holdButton.GetComponent<RectTransform>().anchoredPosition.y);
        }

        if (countTime >= 7 && !isPressed && !lost)
        {
            if (!take)
            {
                sliderTxt.text = sliderAutoCashTxt.text.ToString();
                //sliderAutoCashNoTxt.gameObject.SetActive(true);
                if (!isAutoPlay)
                    sliderAutoCashNoTxt.text = countTime.ToString(" 0");
            }
        }

        if (isPressed && Multiplier >= 1.01f && !isAutoPlay && !take)
            sliderAutoCashNoTxt.text = " 7";

        if (startGame &&/* countTime >= 6.5*/Multiplier >= 1.01f && !isAutoPlay)
        {
            TakeButtonColor();
        }

        if (isBonus && isScroll)
        {
            isBonus_OFF = true;
        }
    }
    void TakeButtonColor()
    {
        if (startGame)
        {
            takeCashbutton.interactable = true;
            takeCashImg.color = new Color32(255, 255, 255, 255);
            takeCashtxt.color = new Color32(194, 236, 166, 255);
            takeCashWintxt.color = new Color32(194, 236, 166, 255);
            takeCurrencytxt.color = new Color32(194, 236, 166, 255);
            takeCashObj.SetActive(true);
        }
        else
        {
            takeCashbutton.interactable = false;
            takeCashImg.color = new Color32(140, 140, 140, 255);
            takeCashtxt.color = new Color32(194, 236, 166, 120);
            takeCashWintxt.color = new Color32(194, 236, 166, 120);
            takeCurrencytxt.color = new Color32(194, 236, 166, 120);
            takeCashObj.SetActive(false);
        }
    }
    void IncrementMultiplier()
    {
        if (isPressed && Multiplier >= 1.01f)
        {
            // Increase the multiplier by the increment rate
            Multiplier *= GetTruncatedValue_float(incrementRate);
            string s = GetTruncatedValue_float(Multiplier).ToString("F2");
            multiplierTxt.text = s;
            multiplierTxt_Shadow.text = s;

            // Update the take cash text
            Debug.Log("CheckingTakeCash Value_2 : " + GetTruncatedValue(TakeCash));
            takeCashTxt.text = GetTruncatedValue(TakeCash).ToString("F2");
        }
    }
    public void Animation_Pause()
    {
        heat_IdleAnim.SetBool("isPlay1", false);
        imageSequencer.enabled = false;
        for (int i = 0; i < unsetected_Buttons.Count; i++)
        {
            unsetected_Buttons[i].enabled = false;
        }
        handGestures_start_Img.SetActive(false);
        handGestures_btAmt_Img.SetActive(false);
        takeCash_Anim.SetActive(false);
        //balloonShake_blue.GetComponent<ImageSequencer>().enabled = false;
    }
    public void Animation_Play()
    {
        heat_IdleAnim.SetBool("isPlay1", true);
        imageSequencer.enabled = true;
        for (int i = 0; i < unsetected_Buttons.Count; i++)
        {
            unsetected_Buttons[i].enabled = true;
        }
        handGestures_start_Img.SetActive(true);
        handGestures_btAmt_Img.SetActive(true);
        takeCash_Anim.SetActive(true);
    }
    public void TakeCashOut() // TakeCash button
    {
        InternetCheck = true;

        if (!isWin)
        {
            TakingCash();

            isWin = true;
        }
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
            DebugHelper.Log("CheckInternetandProcess Betbtn");
            if (success)
            {
                DebugHelper.Log("CheckInternetandProcess Betbtn Success");
                LocalInitializeBet();
            }
            else
            {
                DebugHelper.Log("CheckInternetandProcess Failed");
                BetInputController.Instance.BetAmtInput.interactable = true;
            }
        });
    }
    void LocalInitializeBet()
    {
        DebugHelper.Log("Entered LocalInitializeBet");
        if (betAmount > TotalAmount)
        {
            if (TotalAmount >= .1f)
            {
                // Controller.BetAmount = 5;
                PassTxt(betAmountTxt, betAmount.ToString("0.00") + " " + APIController.instance.userDetails.currency_type);
            }
            else
            {
                DebugHelper.Log("nmnm");
            }

            DebugHelper.Log("LocalInitializeBet 2 " + betAmount);
            if (demo)
            {
                insufficientBalance.SetActive(true);
                insufBal_Rumblebets.SetActive(false);
                BetResetForInsufficient();
            }
            else
            {
                insufficientBalance.SetActive(false);
                insufBal_Rumblebets.SetActive(true);
                Debug.Log("InsufficientPopUpAppers ==>>>_2 : " + buttonPress);
                BetResetForInsufficient();
            }
            return;
        }

        isCreateMatchSucceess = false;

        #region
        int _index = UnityEngine.Random.Range(100, 999);
        string message = "Bet Initiated";
        TransactionMetaData val = new TransactionMetaData();
        val.Amount = betAmount;
        val.Info = message;
        string m = betAmount.ToString("0.00");
        betAmount = double.Parse(m);
        DebugHelper.Log("LocalInitializeBet Controller.BetButtonclik ");
        DebugHelper.Log("BetAMount ********* " + betAmount + " " + " Balance ******* " + TotalAmount);
        List<string> _list = new List<string>();
        _list.Add(APIController.instance.userDetails.Id);

        GetPredictionProcess(async () =>
        {
            await UniTask.Delay(100);
            Debug.Log($"GetPredictionProcess Done");
            CreateMatchAPICall(); // After PedictionProcess done. Cal your CreateMatchMethod here

        });
        //CreateMatchAPICall();
        #endregion
    }
    public void CreateMatchAPICall()    //CREATEMATCHAPICALL CALLING METHOD
    {
        IsCreateMatchCalled = true;
        DebugHelper.Log("CreateMatchAPICalled========>");
        TransactionMetaData TransData = new();
        TransData.Amount = betAmount;
        TransData.Info = "InitBet";
        int _index = UnityEngine.Random.Range(100, 999);
        List<string> _list = new();
        lobbyName = "Room : " + DateTime.UtcNow + UnityEngine.Random.Range(100, 999);
        _list.Add(APIController.instance.userDetails.Id);
        APIController.instance.CreateAndJoinMatch(_index, betAmount, TransData, false, lobbyName, APIController.instance.userDetails.Id,
               false, gameName, operatorName, APIController.instance.userDetails.gameId, APIController.instance.userDetails.isBlockApiConnection, _list,
               (initialized) =>
               {

               },
     (betIndex, response) =>
     {
         DebugHelper.Log("Is match getting ===>>>> + Success");
         BetIndex = betIndex;
         MatchRes = response;
         betID = response.Message;
         startGame = true;
         PressToBet();
         SetupGameForNewRound();
         HandleGameStart();
     },
     (failed) =>
     {
         // Resetting all game Data......
         DebugHelper.Log("Is match getting ===>>>> + Failed");
         startGame = false;
         onClick = false;
         ResetBets();
     });
    }
    void API_Winning()
    {
        string message = "Game Won";
        TransactionMetaData val = new TransactionMetaData();
        val.Amount = TakeCash;
        val.Info = message;

        DebugHelper.Log("isCreateMatchSucceess ====> success " + isCreateMatchSucceess);
        WinningBetAPICall(TakeCash, TakeCash);
    }
    public void WinningBetAPICall(double WinAmount, double PotAmount)   //WINNINGBETAPI CALLING METHOD
    {
        TransactionMetaData _metaData = new TransactionMetaData();
        _metaData.Amount = WinAmount;
        _metaData.Info = "Game Won";
        Debug.Log("WinningsAmtBet ... : " + WinAmount);
        APIController.instance.WinningsBetMultiplayerAPI(BetIndex, betID, WinAmount, betAmount, PotAmount, _metaData, (success) =>
        {
            if (success)
            {
                IsCreateMatchCalled = false;

                take = true;
                startGame = false;
                onClick = false;
                holdButton.enabled = false;
                // TakeCash
                takeCashImg.color = new Color32(140, 140, 140, 255);
                takeCashtxt.color = new Color32(194, 236, 166, 120);
                takeCashWintxt.color = new Color32(194, 236, 166, 120);
                takeCurrencytxt.color = new Color32(194, 236, 166, 120);
                takeCashObj.SetActive(false);
                // sliderOBjs
                slider_bg.SetActive(false);
                fillArea.SetActive(false);
                slider_txt.SetActive(false);
                keepHolding_txt.SetActive(false);
                //sliderAutoCashNoTxt.gameObject.SetActive(false);
                sliderAutoCashNoTxt.text = " ";
                slider_Anim.SetBool("isOFF", true);
                sliderBg_Anim.SetBool("isFalse", true);
                ballon_Anim.SetBool("isTake", true);

                if (isAutoPlay && (stopSingleWin > 0) && (TakeCash > stopSingleWin))
                {
                    Stop_AutoPlayBtn();
                }

                isNormal = true;

                if ((WinCash_demo < 10))
                {
                    audioController.PlayAudio(AudioEnum.winGame);
                    winPanel.SetActive(true);
                    // TakingCash();
                    Winning_Animations();
                }
                Call_Functions();
                DelayFuction();

                //Betlist localBet = new Betlist
                //{
                //    bet_amount = betAmount,
                //    win_amount = WinAmount,
                //    multiplier = Multiplier,
                //    dateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                //};
                //APIController.instance.betlistArray.Add(localBet);
                //BetHistory.instance.AddPlayerBetDetails(APIController.instance.betlistArray, true);
            }
            else
            {
                DebugHelper.Log("WinningBetAPIfailed========>");

                Betlist localBet = new Betlist
                {
                    bet_amount = betAmount,
                    win_amount = 0,
                    multiplier = 0,
                    dateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                };
                APIController.instance.betlistArray.Add(localBet);
                BetHistory.instance.AddPlayerBetDetails(APIController.instance.betlistArray, true);
            }
        }, APIController.instance.userDetails.Id, false, WinAmount == 0 ? false : true, gameName, operatorName, APIController.instance.userDetails.gameId, APIController.instance.userDetails.commission, MatchRes.MatchToken);
    }
    void Call_Functions()
    {
        if (winCash != Winbonus)
            isNormal = true;
    }
    async void DelayFuction()
    {
        if (isNormal)
        {
            DebugHelper.Log("Check1");
            await UniTask.Delay(3500);
            TimeDelay();
        }
    }
    void TimeDelay() // Clear UI
    {
        DebugHelper.Log("Check2");
        multiplierTxt_Shadow.color = Color.black;
        xTxt_Shadow.color = Color.black;
        Multiplier = 0f;
        multiplierTxt.text = Multiplier.ToString("0.00");
        multiplierTxt_Shadow.text = Multiplier.ToString("0.00");
        TakeCash = 0f;
        Mstring = " ";
        takeCashTxt.text = GetTruncatedValue(TakeCash).ToString("F2");
        holdButton.enabled = true;
        timeSinceLastIncrement = 0f;
        timeHeld = 0f;
        gameLost = false;
        _balanceUpdate = false;
        take = false;
        lost = false;
        isNormal = false;
        heat_IdleAnim.SetBool("isPlay1", true);
        isBonus_2 = false;
        ballonOut.gameObject.SetActive(false);
        flewAway_Txt.SetActive(false);
        ballon_Anim.SetBool("isOut", false);
        background.localPosition = initialBackgroundPosition;
        bg.localPosition = iniBackgroundPos;
        heatbtnCollider.enabled = true;
        if (winCount == true)
        {
            winCount = false;
        }

        if (!isAutoPlay)
            autoPlayBtn.interactable = true;

        Button_Switch_ON();
        //colors
        takeCashImg.color = new Color32(140, 140, 140, 255);
        takeCashtxt.color = new Color32(194, 236, 166, 120);
        takeCashWintxt.color = new Color32(194, 236, 166, 120);
        takeCurrencytxt.color = new Color32(194, 236, 166, 120);
        heatTxt.color = new Color32(63, 15, 15, 255);
        autoPlay_Txt.color = new Color32(146, 158, 167, 255);

        // takeCash
        takeCashbutton.interactable = false;
        takeCashObj.SetActive(false);
        isBegin = false;
        winPanel.SetActive(false);
        takeBetAmount = true;
        isWin = false;
        countTime = 0f;
        slider.value = 0f;

        ButtonSelect_Anim();

        if (isAutoPlay && (totalCash > 0) && (TotalAmount <= totalCash))
            Stop_AutoPlayBtn();

        //Slider Animation
        slider_Anim.SetBool("isOFF", false);
        slider_Anim.SetBool("isON", false);
        sliderBg_Anim.SetBool("isFalse", false);
        sliderBg_Anim.SetBool("isTrue", false);

        //balloon
        ballon_Anim.enabled = true;
        ballon_Anim.SetBool("isTake", false);
        ballon_Anim.SetBool("isJump", false);
        balloonShake_blue.SetActive(false);
        balloonShake.SetActive(false);
        ballon_shake.SetBool("itsON", false);
        balloonParts.transform.localPosition = new Vector3(0f, -430.1323f, 0f);
        TxtObjs.gameObject.SetActive(false);
        BalloonDelay();
        heat_Anim.SetBool("isPlay1", true);
        fireObj.SetActive(false);
        idleFireObj.SetActive(false);
        fireIdleObj.SetActive(true);
        idleFireGlow.SetActive(true);
        isChecked = false;

        // sliderOBjs
        slider_Anim.SetBool("isON", true);
        sliderbg.SetActive(false);
        sliderBg_Anim.SetBool("isTrue", false);

        if (!isAutoPlay || !stopAutoPlay)
            pressToBetBtn.gameObject.SetActive(true);

        //Slider_Objs();
        NetworkHandler.instance.StartIdleSession();

        Color color = betAmountTxt.color;
        color.a = 1f;
        betAmountTxt.color = color;

        for (int i = 0; i < btnAmtTxt.Length; i++)
        {
            if (btnAmtTxt[i] != null)
            {
                Color color1 = btnAmtTxt[i].color;
                color1.a = 1f;
                btnAmtTxt[i].color = color1;
            }
        }

        // AutoPlay
        if (isAutoPlay && (rounds >= 0 && rounds <= 100))
        {
            rounds--;
            roundsTxt.text = rounds.ToString();
        }

        if (!stopAutoPlay)
        {
            Stop_AutoPlayBtn();
            Reset_AutoPlayBtn();
        }

        netCheck = false;
    }
    async void BalloonDelay()
    {
        await UniTask.Delay(100);
        balloonParts.SetActive(true);
        if (!startGame && !winPanel.activeInHierarchy)
            balloonBlue_Start.SetActive(true);
    }
    public void HandleInsuffitient()
    {
        Button_Switch_ON();
        //colors
        takeCashImg.color = new Color32(140, 140, 140, 255);
        takeCashtxt.color = new Color32(194, 236, 166, 120);
        takeCashWintxt.color = new Color32(194, 236, 166, 120);
        takeCurrencytxt.color = new Color32(194, 236, 166, 120);
        heatTxt.color = new Color32(63, 15, 15, 255);

        // takeCash
        takeCashObj.SetActive(false);
        isBegin = false;
        winPanel.SetActive(false);
        takeBetAmount = true;
        isWin = false;
        countTime = 0f;
        slider.value = 0f;

        ButtonSelect_Anim();
    }
    void ResetBets()
    {
        isPressed = false;
        pauseGame = false;
        startGame = false;
        onClick = false;
        winPanel.SetActive(false);
        flewAway_Txt.SetActive(true);
        multiplierTxt.text = Multiplier.ToString("0.00");
        multiplierTxt_Shadow.text = Multiplier.ToString("0.00");
        makeLose = false;
        unPress.SetActive(true);
        pressed.SetActive(false);

        IsCreateMatchCalled = false;
        Invoke("TimeDelay", 1.5f);
    }
    public void BetResetForInsufficient()
    {
        isPressed = false;
        pauseGame = false;
        startGame = false;
        onClick = false;
        unPress.SetActive(true);
        pressed.SetActive(false);

        if (!startGame)
            ButtonSelect_Anim();
    }
    void Winning_Animations()
    {
        winTxt.text = $"{GetTruncatedValue(TakeCash):F2} {currencyType}";

        WinTxtObj();
    }
    public void TakingCash()
    {
        Debug.Log($"TakeCashBetAmount 1 : _ { TakeCash }");
        API_Winning();
    }
    async void WinTxtObj()
    {
        await UniTask.Delay(2000);
        winTxt.transform.DORotate(new Vector3(0f, 360f, 0f), 0.5f, RotateMode.FastBeyond360).SetEase(Ease.InOutSine);
        if (APIController.instance.userDetails.currency_type == "USD")
        {
            winTxt.text = $"{GetTruncatedValue(TakeCash):F2} {currencyType}";
        }
        else if (APIController.instance.userDetails.currency_type == "EUR")
        {
            winTxt.text = $"{GetTruncatedValue(TakeCash):F2} {currencyType}";
        }
        else if (APIController.instance.userDetails.currency_type == "INR")
        {
            winTxt.text = $"{GetTruncatedValue(TakeCash):F2} {currencyType}";
        }
    }
    async void balloon_Objs()
    {
        await UniTask.Delay(400);
        await UniTask.Delay(400);
        ballon_Anim.SetBool("isJump", false);
        balloonParts.SetActive(false);
        if (isPressed)
        {
            balloonShake_blue.SetActive(true);
            ballon_shake.SetBool("itsON", true);
        }
        else
        {
            balloonShake_blue.SetActive(true);
            ballon_shake.SetBool("itsON", false);
        }
    }
    async void Slider_Objs()
    {
        if (!isAutoPlay)
        {
            await UniTask.Delay(200);
            sliderTxt.text = sliderAutoCashTxt.text.ToString();
            await UniTask.Delay(500);
            fillArea.SetActive(true);
        }
    }
    void Button_SwitchingOFF()
    {
        autoPlayBtn.interactable = false;
        minusButton.interactable = false;
        plusButton.interactable = false;
        // Disable all buttons
        button_1.interactable = false; button_2.interactable = false; button_5.interactable = false; button_10.interactable = false;

        for (int i = 0; i < pressed_Buttons.Count; i++)
        {
            pressed_Buttons[i].interactable = false;
            selectedroundBtns[i].interactable = false;
        }
        BetArea_numPad.SetActive(false);
        plusButtomImg.color = new Color32(255, 255, 255, 100);
        minusButtonImg.color = new Color32(255, 255, 255, 100);
    }

    #region ::::::::::::::::::::::::::: Event Trigger Buttons :::::::::::::::::::::::::::
    bool InternetCheck;
    public void OnClickDown()
    {
        netCheck = true;
        NetworkHandler.instance.StartIdleSession(false);

        APIController.instance.CheckInternetandProcess(async (success) =>
        {
            if (!success)
            {
                InternetCheck = false;
                DebugHelper.Log("CheckInternetandProcess failed: " + success);
                return;
            }

            // Proceed if internet check is successful
            InternetCheck = true;
            onClick = true;

            // Handle button press logic if conditions are met
            if (!startGame && !pauseGame && TotalAmount < betAmount)
            {
                buttonPress = true;
                //DebugHelper.Log("Bigger_Amount " + buttonPress);
            }

            if (!numPad && !buttonPress && !lost && !isAutoPlay)
            {
                Button_SwitchingOFF();
                HandleGameStart();
            }

            // Format amounts with two decimal places
            TotalAmount = double.Parse(TotalAmount.ToString("0.00"));
            betAmount = double.Parse(betAmount.ToString("0.00"));

            // Deactivate active button animations
            foreach (var button in button_Anim.Where(button => button.activeSelf))
            {
                button.SetActive(false);
            }

            // Game setup if conditions are met
            if (!startGame && !buttonPress && !gameLost && !isBegin && !NetworkHandler.instance.ConnectionPanel.activeSelf)
            {
                //SetupGameForNewRound();
                if (!IsCreateMatchCalled)
                {
                    API_IntitalizeBetAmount();
                }
                else
                {
                    return;
                }
            }
            while (!startGame)
            {
                await UniTask.Delay(100);
            }

            /*if (!isAutoPlay)
                SetupGameForNewRound();*/
        });

        // Early return if no internet connection
        if (!InternetCheck)
        {
            return;
        }
    }
    private void HandleGameStart()
    {

        // Update button states
        unPress.SetActive(false);
        pressed.SetActive(true);

        // Disable hand gestures and bet input
        if (handGestures_start.activeSelf)
            handGestures_start.SetActive(false);

        BetInputController.Instance.BetAmtInput.textViewport.gameObject.SetActive(false);

        if (BetInputController.Instance.BetPanel.gameObject.activeSelf)
        {
            BetInputController.Instance.CloseKeyPadPanel();
            BetInputController.Instance.RestrictInput();
        }

        // Start game animations and audio
        isPressed = true;
        audioController.StopAudio(AudioEnum.reverseSlider);
        audioController.PlayAudio(AudioEnum.startSlider, true);
        audioController.PlayAudio(AudioEnum.Movement, true);

        // Handle balloon states and parallax effect
        //balloonShake_blue.SetActive(false);
        ballon_shake.SetBool("itsON", false);
        balloonParts.SetActive(false);
        ApplyParallaxEffect(holdButton.GetComponent<RectTransform>().anchoredPosition.y);
        touch = true;

        // Set animations for heat effect
        heat_Anim.SetBool("isPlay1", true);
        heat_Anim.SetBool("isPlay2", true);
        heat_IdleAnim.SetBool("isPlay1", false);

        // Manage fire-related objects
        //idleFireGlow.SetActive(false);
        fireObj.SetActive(true);
        idleFireObj.SetActive(true);
        fireIdleObj.SetActive(false);

        // Disable all button animations
        foreach (var button in button_Anim)
        {
            button.SetActive(false);
        }

        // Disable the take cash button
        takeCashbutton.interactable = false;
    }
    private void SetupGameForNewRound()
    {
       /* makeLose = true;

        // Ensure hand gestures are disabled
        if (handGestures_start.activeSelf)
            handGestures_start.SetActive(false);

        gameCounts++;

        // Randomize height based on game count
        if (gameCounts != 15)
        {
            holdHeight = UnityEngine.Random.Range(0.80f, 9.8f);
        }
        else
        {
            holdHeight = UnityEngine.Random.Range(0.75f, 0.99f);
            gameCounts = 0;
        }

        // Initialize bet amount
        //API_IntitalizeBetAmount();
        isBegin = true;*/
    }
    public void RNG_APICall()   //RNG_APICALL CALLING METHOD
    {
        APIController.instance.GetRNG_API(betAmount, operatorName, APIController.instance.userDetails.gameId, (_IsWin, _MaxWin, _gameCount) =>
        {
            makeLose = _IsWin;
            holdHeight = _MaxWin;
            if (_IsWin)
            {
                DebugHelper.Log($"RNG Calculation:\n==============\n RNG Value Check : randomGamePlay \n MaxHeight : {_MaxWin}\n GameCount : {_gameCount}\n==============\n");
            }
            else
            {
                DebugHelper.Log($"RNG Calculation:\n==============\n RNG Value Check : LoseThisGame \n MaxHeight : {_MaxWin}\n GameCount : {_gameCount}\n==============\n");
            }

        }, gameName, 0);
    }

    #region GET PREDICTION FROM API
    public async void GetPredictionProcess(Action onSuccess)
    {
        bool predictionValue = false;

        Dictionary<string, string> bodyDict = new Dictionary<string, string>
        {
            //{ "GameName", "Balloon"}
            // Send Data to server
        };
        string payload = JsonConvert.SerializeObject(bodyDict);
        string reqID = "";
        reqID = APIController.instance.GetPredictionValue(payload, (init) =>
        {
            DebugHelper.Log("GetPredictionProcess Init Response Prediciton" + init.ToString());
        },
        (success) =>
        {
            predictionValue = true;
            DebugHelper.Log("GetPredictionProcess success Response Prediciton" + success.ToString());
            JObject data = JObject.Parse(success);
            string heightString = data["prediction"].ToString();
            float height = float.Parse(heightString);
            height = (float)Math.Round(height, 2);
            holdHeight = height;
            Debug.Log($"CheckingRNGvalue :  + {height} , {holdHeight}");
  
            // For Debugging RNG data
            if (true) // Need to do
            {
                onSuccess?.Invoke();
                DebugHelper.Log($"RNG Calculation:\n==============\n Selected numbers from server are {height}\n==============\n");
            }
        },
        (failure) =>
        {
            DebugHelper.Log("Set_API_Index failure Response Prediciton" + failure.ToString());
        });
        float time = Time.time;

        while (!predictionValue)
        {
            if (Time.time - time > 5)
            {
                if (BaseSocketController.instance.IsOnline())
                {
                    BaseSocketController.instance.RemoveRequestEvent(reqID);
                    DebugHelper.Log("NewCreateAndJoinMatch_1  Retry Called");
                    GetPredictionProcess(onSuccess);
                    return;
                }
            }
            await UniTask.Delay(100);
        }
    }
    #endregion
    public void OnClickUp()
    {
        APIController.instance.CheckInternetandProcess((success) =>
        {
            // Check internet connection status
            if (!success)
            {
                InternetCheck = false;
                DebugHelper.Log("CheckInternetandProcess failed: " + success);
                return;
            }

            // Proceed if internet check is successful
            InternetCheck = true;
            unPress.SetActive(true);
            pressed.SetActive(false);

            // Reset button press state and update animations
            isPressed = false;
            heat_Anim.SetBool("isPlay2", false);
            heat_IdleAnim.SetBool("isPlay1", true);

            // Manage fire-related objects
            fireObj.SetActive(false);
            idleFireObj.SetActive(false);
            fireIdleObj.SetActive(true);

            // Handle audio and balloon states
            if (isFire && Multiplier >= 1.01f)
            {
                audioController.PlayAudio(AudioEnum.reverseSlider, true);
                touch = false;
            }

            // Stop specific audio tracks
            audioController.StopAudio(AudioEnum.startSlider);
            audioController.StopAudio(AudioEnum.Movement);

            // Update balloon shake states if conditions are met
            if (!lost && !take && Multiplier >= 1.01f)
            {
                balloonShake.SetActive(false);
                balloonShake_blue.SetActive(true);
                ballon_shake.SetBool("itsON", false);
            }
        });

        // Early exit if no internet connection
        if (!InternetCheck)
        {
            return;
        }
    }
    public void Button_ONEnter()
    {
        isFire = true;
    }
    public void Button_OFFEnter()
    {
        isFire = false;

        if (touch && Multiplier >= 1.01f)
        {
            audioController.PlayAudio(AudioEnum.reverseSlider, true);
            touch = false;
        }
    }
    public void Welcom_Button()
    {
        // Deactivate fire-related objects
        fireObj.SetActive(false);
        fireIdleObj.SetActive(false);

        // Disable heat button collider and trigger necessary animations
        heatbtnCollider.enabled = true;

        if (!startGame)
            ButtonSelect_Anim();

        // Activate the hand gestures and slider animation
        //   handGestures_btAmt.SetActive(true);

        slider_Anim.SetBool("isON", true);

        if (!isAutoPlay)
            pressToBetBtn.gameObject.SetActive(true);
        /*sliderbg.SetActive(true);
        sliderBg_Anim.SetBool("isTrue", true);*/

        // Call method to handle slider objects
        //Slider_Objs();
    }
    private void PressToBet()
    {
        balloonBlue_Start.SetActive(false);
        ballon_Anim.SetBool("isJump", true);

        pressToBetBtn.gameObject.SetActive(false);

        if (!isAutoPlay)
        {
            sliderbg.SetActive(true);
            sliderBg_Anim.SetBool("isTrue", true);
        }

        Slider_Objs();
        SliderDelay();
    }
    async void SliderDelay()
    {
        await UniTask.Delay(400);
        TxtObjs.gameObject.SetActive(true);
        if (!isAutoPlay)
        {
            slider_txt.SetActive(true);
            keepHolding_txt.SetActive(true);
            slider_bg.SetActive(true);
        }
        await UniTask.Delay(400);
        isChecked = true;
    }
    #endregion

    #region { ::::::::::::::::::::::::: Buttons ::::::::::::::::::::::::: }
    public int currentClickValue;
    public void TakeButtonPress() // Function used in TakeButton Inspector in Editor
    {
        startGame = false;
    }
    public void BetButtonPressed(int betValue, int buttonIndex)
    {
        audioController.PlayAudio(AudioEnum.buttonClick);

        if (BetInputController.Instance.BetPanel.gameObject.activeSelf)
        {
            BetInputController.Instance.BetAmtInput.textViewport.gameObject.SetActive(false);
            BetInputController.Instance.BetPanel.gameObject.SetActive(false);
            KeyBoardHandler.instance.cancelButton.gameObject.SetActive(false);
            UpdateBetAmountDisplay();
            return;
        }
        if (keyBoard.cancelButton.gameObject.activeSelf)
            keyBoard.OnCancelInput();

        if (!startGame && !take && !isScroll && !onClick && !isAutoPlay)
        {
            // Set the bet amount
            betAmount = betValue;
            betAmountTxt.text = $"{betAmount:F2} <size=30>{currencyType}</size>";
            BetAmountTxt_Scaling();

            if (buttonIndex >= 0 && buttonIndex < selected_Anim.Length)
            {
                selected_Anim[buttonIndex].gameObject.SetActive(true);
                selected_Anim[buttonIndex].DOKill();
                selected_Anim[buttonIndex].DOFade(1, 0.15f).From(0).SetLoops(1, LoopType.Yoyo).OnComplete(() => selected_Anim[buttonIndex].gameObject.SetActive(false));
                selected_Anim[buttonIndex].transform.DOScale(new Vector3(1.07f, 1.07f, 1.07f), 0.15f).From(Vector3.one);

            }

            // Set buttons' active state based on bet amount
            SetBetButtonsActiveState(betValue);

            AmountColor_Glow();
            UpdateButtonAnimations(betValue);

            plusButton.interactable = true;
            minusButton.interactable = true;
            plusButtomImg.color = new Color32(255, 255, 255, 255);
            minusButtonImg.color = new Color32(255, 255, 255, 255);

            winCash = 0;
            WinCash_demo = 0;
            Bonustimer = 0;
            timer = true;

            HandGesture();
        }
    }
    private void SetBetButtonsActiveState(int activeBet)
    {
        var betValues = APIController.instance.authentication.entryAmountDetails.betValues;
        button_1.gameObject.SetActive(activeBet == betValues[0]);
        button_2.gameObject.SetActive(activeBet == betValues[1]);
        button_5.gameObject.SetActive(activeBet == betValues[2]);
        button_10.gameObject.SetActive(activeBet == betValues[3]);
    }
    private void UpdateButtonAnimations(int betValue)
    {
        // Disable all button animations initially
        for (int i = 0; i < button_Anim.Length; i++)
        {
            button_Anim[i].SetActive(false);
        }

        // Retrieve the index of the current bet value
        int betIndex = APIController.instance.authentication.entryAmountDetails.betValues.IndexOf(betValue);

        // Enable the appropriate button animations based on the bet index
        if (betIndex >= 0 && betIndex < button_Anim.Length)
        {
            for (int i = 0; i < button_Anim.Length; i++)
            {
                if (i != betIndex)
                {
                    button_Anim[i].SetActive(true);
                }
            }
        }
    }
    // Select bet buttons
    public void SelectBetButton(int s, int btnIndex)
    {
        audioController.PlayAudio(AudioEnum.buttonClick);

        if (!startGame && !take && !isScroll && !onClick && !isAutoPlay)
        {
            if (betAmount < APIController.instance.authentication.entryAmountDetails.maxBetValue)
            {
                betAmount += s;
                UpdateBetAmountDisplay();
                BetAmountTxt_Scaling();
                EnableButtons();
                AmountColor_Glow();
            }

            if (betAmount >= APIController.instance.authentication.entryAmountDetails.maxBetValue)
            {
                betAmount = APIController.instance.authentication.entryAmountDetails.maxBetValue;
                UpdateBetAmountDisplay();
                DisablePlusButton();
                MaxBet_Object();
            }

            if (btnIndex >= 0 && btnIndex < selected_Anim.Length)
            {
                selected_Anim[btnIndex].gameObject.SetActive(true);
                selected_Anim[btnIndex].DOKill();
                selected_Anim[btnIndex].DOFade(1, 0.15f).From(0).SetLoops(1, LoopType.Yoyo).OnComplete(() => selected_Anim[btnIndex].gameObject.SetActive(false));
                selected_Anim[btnIndex].transform.DOScale(new Vector3(1.07f, 1.07f, 1.07f), 0.15f).From(Vector3.one);

            }


            ResetGameVariables();
        }
    }
    private void UpdateBetAmountDisplay()  // SelectBetButton
    {
        betAmountTxt.text = betAmount.ToString("0.00" + " <size=30>" + currencyType + "</size>");
        betAmountTxt.gameObject.SetActive(true);
    }
    private void EnableButtons()  // SelectBetButton
    {
        plusButton.interactable = true;
        minusButton.interactable = true;
        plusButtomImg.color = new Color32(255, 255, 255, 255);
        minusButtonImg.color = new Color32(255, 255, 255, 255);
    }
    private void DisablePlusButton()  // SelectBetButton
    {
        plusButton.interactable = false;
        plusButtomImg.color = new Color32(255, 255, 255, 120);
    }
    private void ResetGameVariables()   // SelectBetButton
    {
        winCash = 0;
        WinCash_demo = 0;
        Bonustimer = 0;
        timer = true;
    }
    public async void MaxBet_Object()
    {
        maxBet_Reached.SetActive(true);
        await UniTask.Delay(3000);
        maxBet_Reached.SetActive(false);

    }
    public void PlusButton()
    {
        audioController.PlayAudio(AudioEnum.buttonClick);
        plus_Anim.SetBool("isON", true);
        if (BetInputController.Instance.BetPanel.gameObject.activeSelf)
        {
            BetInputController.Instance.CloseKeyPadPanel();
            BetInputController.Instance.RestrictInput();
            BetInputController.Instance.BetAmtInput.textViewport.gameObject.SetActive(false);
            return;
        }
        if (keyBoard.gameObject.activeSelf)
            keyBoard.OnCancelInput();

        if (!startGame && !take && !isScroll && !onClick && !isAutoPlay)
        {
            if (betAmount < APIController.instance.authentication.entryAmountDetails.maxBetValue)
            {
                betAmount += APIController.instance.authentication.entryAmountDetails.incrementValue;
                betAmountTxt.text = betAmount.ToString("0.00" + " <size=30>" + currencyType + "</size>");
                BetAmountTxt_Scaling();
                minusButton.interactable = true;
                minusButtonImg.color = new Color32(255, 255, 255, 255);
                AmountColor_Glow();
            }

            if (betAmount >= APIController.instance.authentication.entryAmountDetails.maxBetValue)
            {
                betAmount = APIController.instance.authentication.entryAmountDetails.maxBetValue;
                plusButton.interactable = false;
                betAmountTxt.text = betAmount.ToString("0.00" + " <size=30>" + currencyType + "</size>");
                BetAmountTxt_Scaling();
                plusButtomImg.color = new Color32(255, 255, 255, 100);
                MaxBet_Object();
            }

            if (betAmount != APIController.instance.authentication.entryAmountDetails.maxBetValue)
            {
                winCash = 0;
                WinCash_demo = 0;
                Bonustimer = 0;
                timer = true;
            }
            HandGesture();
        }
        Plus_AnimOFF();
    }
    public void MinusButton()
    {
        audioController.PlayAudio(AudioEnum.buttonClick);
        minus_Anim.SetBool("isON", true);
        if (BetInputController.Instance.BetPanel.gameObject.activeSelf)
        {
            BetInputController.Instance.CloseKeyPadPanel();
            BetInputController.Instance.RestrictInput();
            BetInputController.Instance.BetAmtInput.textViewport.gameObject.SetActive(false);
            return;
        }
        if (keyBoard.gameObject.activeSelf)
            keyBoard.OnCancelInput();

        if (!startGame && !take && !isScroll && !onClick)
        {
            if (betAmount > APIController.instance.authentication.entryAmountDetails.minBetValue)
            {
                betAmount -= APIController.instance.authentication.entryAmountDetails.decrementValue;
                betAmountTxt.text = betAmount.ToString("0.00" + " <size=30>" + currencyType + "</size>");
                BetAmountTxt_Scaling();
                plusButtomImg.color = new Color32(255, 255, 255, 255);
                AmountColor_Glow();
            }
            if (betAmount <= APIController.instance.authentication.entryAmountDetails.minBetValue)
            {
                betAmount = APIController.instance.authentication.entryAmountDetails.minBetValue;
                betAmountTxt.text = betAmount.ToString("0.00" + " <size=30>" + currencyType + "</size>");
                BetAmountTxt_Scaling();
                minusButton.interactable = false;
                minusButtonImg.color = new Color32(255, 255, 255, 100);
            }

            if (betAmount != APIController.instance.authentication.entryAmountDetails.minBetValue)
            {
                winCash = 0;
                WinCash_demo = 0;
                Bonustimer = 0;
                timer = true;
            }
        }
        Minus_AnimOFF();
    }
    async void Plus_AnimOFF()
    {
        await UniTask.Delay(100);
        plus_Anim.SetBool("isON", false);
    }
    async void Minus_AnimOFF()
    {
        await UniTask.Delay(100);
        minus_Anim.SetBool("isON", false);
    }
    public void BetAmountTxt_Scaling()
    {
        // Create and configure a DoTween sequence for scaling animation
        DOTween.Sequence()
            .Append(betAmountTxt.transform.DOScale(new Vector3(0.95f, 0.95f, 0.95f), 0.2f).SetEase(Ease.InSine))
            .AppendInterval(0.02f)
            .Append(betAmountTxt.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.InOutSine));
    }
    void Button_Switch_ON()
    {
        // Enable all buttons
        button_1.interactable = true; button_2.interactable = true; button_5.interactable = true; button_10.interactable = true;

        for (int i = 0; i < unclicked_Buttons.Count; i++)
        {
            unclicked_Buttons[i].interactable = true;
        }

        // Enable plus and minus buttons with default color
        EnableButton_Switch_ON(plusButton, plusButtomImg);
        EnableButton_Switch_ON(minusButton, minusButtonImg);
    }
    void Button_Switch_OFF()
    {
        // Disable all buttons
        button_1.interactable = false; button_2.interactable = false; button_5.interactable = false; button_10.interactable = false;

        for (int i = 0; i < unclicked_Buttons.Count; i++)
        {
            unclicked_Buttons[i].interactable = false;
        }

        // Disable plus and minus buttons with faded color
        DisableButton_Switch_OFF(plusButton, plusButtomImg);
        DisableButton_Switch_OFF(minusButton, minusButtonImg);
    }
    private void EnableButton_Switch_ON(Button button, Image buttonImage)
    {
        button.interactable = true;
        buttonImage.color = new Color32(255, 255, 255, 255);
    }
    private void DisableButton_Switch_OFF(Button button, Image buttonImage)
    {
        button.interactable = false;
        buttonImage.color = new Color32(255, 255, 255, 120);
    }
    void BetAmountUpdates()
    {
        if (onClick)
        {
            numPadButton.gameObject.SetActive(false);
        }
        else
        {
            numPadButton.gameObject.SetActive(true);
        }

        if ((betAmount <= APIController.instance.authentication.entryAmountDetails.incrementValue) && !isAutoPlay)
        {
            minusButton.interactable = false;
            minusButtonImg.color = new Color32(255, 255, 255, 100);
        }
        else if ((betAmount > APIController.instance.authentication.entryAmountDetails.incrementValue) && !isAutoPlay)
        {
            minusButton.interactable = true;
            minusButtonImg.color = new Color32(255, 255, 255, 255);
        }

        if ((betAmount < APIController.instance.authentication.entryAmountDetails.maxBetValue) && !isAutoPlay)
        {
            plusButton.interactable = true;
            plusButtomImg.color = new Color32(255, 255, 255, 255);
        }
        else if ((betAmount >= APIController.instance.authentication.entryAmountDetails.maxBetValue) && !isAutoPlay)
        {
            plusButton.interactable = false;
            plusButtomImg.color = new Color32(255, 255, 255, 100);
        }
    }
    void ButtonSelect_Anim()
    {
        // Determine which button is active
        GameObject activeButton = null;

        if (button_1.gameObject.activeSelf) activeButton = button_1.gameObject;
        else if (button_2.gameObject.activeSelf) activeButton = button_2.gameObject;
        else if (button_5.gameObject.activeSelf) activeButton = button_5.gameObject;
        else if (button_10.gameObject.activeSelf) activeButton = button_10.gameObject;

        // If an active button is found, update animations
        if (activeButton != null)
        {
            // Loop through all button animations and deactivate the one that matches the active button
            for (int i = 0; i < button_Anim.Length; i++)
            {
                bool isActiveButton = button_Anim[i].gameObject == activeButton;
                button_Anim[i].SetActive(!isActiveButton);
            }

            // Enable the other buttons that aren't active
            foreach (var button in button_Anim)
            {
                if (!button.gameObject.activeSelf)
                    button.SetActive(true);
            }
        }
    }
    public void HandGesture()
    {
        // Deactivate and activate UI elements
        handGestures_btAmt.SetActive(false);
        // handGestures_start.SetActive(true);

        // Enable necessary components
        heatbtnCollider.enabled = true;
        fireIdleObj.SetActive(true);

        // Play animation
        heat_IdleAnim.SetBool("isPlay1", true);
    }
    public void Insufficient_OFF()
    {
        buttonPress = false;
        isBegin = false;
        BetResetForInsufficient();
    }
    public void Close_sessionTimeOut() // Show PopUp for Close Button
    {
        currentTime = 60;
        sessionTimeOut.SetActive(false);
    }
    #endregion ::::::::::::::::::::::::: END :::::::::::::::::::::::::

    #region { ::::::::::::::::::::::::: Auto_Play ::::::::::::::::::::::::: }
    void NumCountRounds(int _rounds)
    {
        if (keyBoard.cancelButton.gameObject.activeSelf)
            keyBoard.OnCancelInput();

        if (!startGame && !take && !isScroll && !onClick)
        {
            audioController.PlayAudio(AudioEnum.buttonClick);

            // Set the bet amount
            rounds = _rounds;
            if (rounds <= 99)
            {
                roundsTxt.text = rounds.ToString();
                if (infiniteImg.gameObject.activeSelf)
                    infiniteImg.gameObject.SetActive(false);
            }
            else if (rounds == int.MaxValue)
            {
                infiniteImg.gameObject.SetActive(true);
                roundsTxt.text = " ";
            }

            //CheckMinCondition();

            // Set buttons' active state based on bet amount
            SetRoundBtnActiveState(_rounds);
        }
    }
    private void SetRoundBtnActiveState(int activeBet)
    {
        var _setRounds = setRounds;

        for (int i = 0; i < selectedroundBtns.Count; i++)
        {
            selectedroundBtns[i].gameObject.SetActive(activeBet == _setRounds[i]);
        }
    }
    void AutoPlayBtnPress()
    {
        audioController.PlayAudio(AudioEnum.buttonClick);
        Reset_AutoPlayBtn();
        if (BetInputController.Instance.BetPanel.gameObject.activeSelf)
        {
            BetInputController.Instance.CloseKeyPadPanel();
            BetInputController.Instance.RestrictInput();
            BetInputController.Instance.BetAmtInput.textViewport.gameObject.SetActive(false);
            return;
        }
        if (keyBoard.cancelButton.gameObject.activeSelf)
            keyBoard.OnCancelInput();

        if (!startGame)
            autoPlayPanel.SetActive(true);
    }
    void Start_AutoPlayBtn()
    {
        audioController.PlayAudio(AudioEnum.buttonClick);
        AutoPlayHandler();
        Difference = targetMultiplier;
        if (rounds >= 0)
        {
            isAutoPlay = true;
            autoPlayPanel.SetActive(false);
            autoPlayBtnPrss = true;
            autoPlayBtn.gameObject.SetActive(false);
            stopAutoPlayBtn.gameObject.SetActive(true);
            autoPlayBtn.interactable = false;
            stopAutoPlay = true;

            if (stop_CashDecrease.isOn)
                totalCash = TotalAmount - stopCashDecrease;

            autoPlay_Txt.color = new Color32(255, 255, 255, 255);
        }
    }
    void Stop_AutoPlayBtn()
    {
        audioController.PlayAudio(AudioEnum.buttonClick);
        autoPlayBtnPrss = false;
        stopAutoPlayBtn.gameObject.SetActive(false);
        autoPlayBtn.gameObject.SetActive(true);
        stopAutoPlay = false;
        if (infiniteImg.gameObject.activeSelf)
            infiniteImg.gameObject.SetActive(false);

        autoPlay_Txt.color = new Color32(146, 158, 167, 255);
    }
    void Reset_AutoPlayBtn()
    {
        audioController.PlayAudio(AudioEnum.buttonClick);
        if (setRounds.Count >= 0)
            NumCountRounds(setRounds[0]);

        ResetBtnOFF();
        autoplayInputHandler[0].GetAudioBool(true);
        autoplayInputHandler[1].GetAudioBool(true);
        autoplayInputHandler[2].GetAudioBool(true);
        isAutoPlay = false;
        rounds = 9;
        roundsTxt.text = rounds.ToString();
        targetInputField.text = 2f.ToString("0.00");
        targetMultiplier = 2f;
        autoplayInputHandler[0].ResetToggles();
        autoPlayBtn.interactable = true;
        BetArea_numPad.SetActive(true);
        infiniteImg.gameObject.SetActive(false);
        stop_CashDecrease.isOn = false;
        stop_SingleWin.isOn = false;

        autoPlay_Txt.color = new Color32(146, 158, 167, 255);
        autoplayInputHandler[0].GetAudioBool(false);
        autoplayInputHandler[1].GetAudioBool(false);
        autoplayInputHandler[2].GetAudioBool(false);
    }
    void AutoPlayHandler()
    {
        targetMultiplier = autoplayInputHandler[0].GetValue();
        stopCashDecrease = autoplayInputHandler[1].GetValue();
        stopSingleWin = autoplayInputHandler[2].GetValue();
    }
    void Check_AutoplayValue()
    {
        check_stopCashDecrease = autoplayInputHandler[1].GetValue();
        check_stopSingleWin = autoplayInputHandler[2].GetValue();
    }
    void StartBtnHadler()
    {
        if (autoPlayPanel.activeSelf)
        {
            if (stop_CashDecrease.isOn && stop_SingleWin.isOn)
            {
                startBtn.interactable = (check_stopCashDecrease > 0 && check_stopSingleWin > 0);
                GameController.instance.ResetBtnON();
                GameController.instance.resetToggle = true;
            }
            else if (!stop_CashDecrease.isOn && !stop_SingleWin.isOn)
            {
                startBtn.interactable = true;
                GameController.instance.ResetBtnOFF();
                GameController.instance.resetToggle = false;
            }
            else if (stop_CashDecrease.isOn && !stop_SingleWin.isOn)
            {
                startBtn.interactable = (check_stopCashDecrease > 0);
            }
            else
            {
                startBtn.interactable = (check_stopSingleWin > 0);
            }

            Color color = startBtn_Txt.color;
            color.a = startBtn.interactable ? 1f : 0.3f;
            startBtn_Txt.color = color;

            CheckMinCondition();
        }
    }
    public void AutoPlayCloseBtn()
    {
        audioController.PlayAudio(AudioEnum.buttonClick);
        Reset_AutoPlayBtn();
        autoPlayPanel.SetActive(false);
    }
    void ResetBtnOFF()
    {
        resetBtn.interactable = false;
        Color color = restartBtn_Txt.color;
        color.a = 0.5f;
        restartBtn_Txt.color = color;
    }
    public void ResetBtnON()
    {
        resetBtn.interactable = true;
        Color color = restartBtn_Txt.color;
        color.a = 1f;
        restartBtn_Txt.color = color;
    }
    void CheckMinCondition()
    {
        if (rounds > 9 || stop_CashDecrease.isOn || stop_SingleWin.isOn || autoplayInputHandler[0].GetValue() != 2f)
        {
            ResetBtnON();
            numClick = true;
        }
        else if (rounds <= 9 && (!stop_CashDecrease.isOn && !stop_SingleWin.isOn))
        {
            ResetBtnOFF();
            numClick = false;
        }
    }
    #endregion { ::::::::::::::::::::::::: Auto_Play ::::::::::::::::::::::::: }

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
    #endregion ::::::::::::::::::::::::: END :::::::::::::::::::::::::

    #region { ::::::::::::::::::::::::: Testing_Amount ::::::::::::::::::::::::: }
    [SerializeField] float testAmount = 10f;
    [ContextMenu("ForceUpdateBalance")]
    private void ForceUpdateBalance()
    {
        APIController.instance.UpdateBalanceResponse(testAmount);
    }
    #endregion ::::::::::::::::::::::::: END :::::::::::::::::::::::::
}

[System.Serializable]
public class BetData
{
    public string DateAndTime;
    public string BetAmount;
    public string WinAmount;
    public string MultiplierValue;
    public string currencyType;
    public bool IsWin;
}