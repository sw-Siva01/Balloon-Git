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
using Nakama.Helpers;
using System.Linq;
using System.Reflection;
using UnityEngine.Audio;

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
    [SerializeField] Button plusButton, minusButton;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("UI GameObjects")]
    [SerializeField] GameObject[] button_Anim;
    [SerializeField] GameObject numPadButton;
    [SerializeField] GameObject takeCashObj;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("UI GameObjects")]
    [SerializeField] GameObject unPress;
    [SerializeField] GameObject pressed;

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
    // private
    [SerializeField] public bool startGame;
    [SerializeField] public bool _balanceUpdate = false;
    [SerializeField] public bool onClick;
    private bool makeLose;
    private bool isBegin;
    private bool pauseGame;
    private bool isPressed;
    public bool buttonPress;
    private bool takeBetAmount;
    private bool isSet;
    private bool isFire;
    private bool lost;
    private bool gameLost;
    private bool take;
    private bool isBonus_1;
    private bool isBonus_2;
    private bool isBonus_3;
    private bool isNormal;
    private bool touch;
    private bool stopper;
    private bool timerCount = true;
    private bool IsCreateMatchCalled = false;


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
    [SerializeField] GameObject fillArea;
    //[SerializeField] Image FillImage;
    [SerializeField] GameObject slider_txt;
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
    // Heat button
    [SerializeField] Animator heat_Anim;
    [SerializeField] Animator heat_IdleAnim;
    [SerializeField] GameObject takeCash_Anim;
    [SerializeField] GameObject fireObj;
    [SerializeField] GameObject fireIdleObj;
    [SerializeField] List<Animator> unsetected_Buttons = new List<Animator>();
    // slider
    [SerializeField] Animator slider_Anim;
    // bonus Balloon
    [SerializeField] Animator bonusBallon;

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

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("IDLE_TimerCount")]
    [SerializeField] float currentTime;
    [SerializeField] float startCount = 10f;
    [SerializeField] GameObject showPopUp;


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
        audioController.muteAllAudio = true;
        AudioListener.volume = 0;
        CanPlayAudio = false;
        settingsBtn.onClick.AddListener(() => UI_Controller.instance.settingsHandler.CallingSettingPanel());
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

        // Background position
        initialBackgroundPosition = background.localPosition;

        // Initialize slider
        Winbonus = 3;
        slider.maxValue = 7f;
        slider.minValue = 1f;
        timeRemaining = 0f; // Start the timer at 0


        // Start coroutines for IDLE_TimerCount
        currentTime = startCount;
        StartCoroutine(nameof(CoundownTimerforIdle));


        for (int i = 0; i < setected_Buttons.Count; i++)
        {
            int index = i; // Capture the loop variable to avoid closure issues
            setected_Buttons[index].onClick.AddListener(() =>
                SelectBetButton((int)APIController.instance.authentication.entryAmountDetails.betValues[index]));
        }

        for (int i = 0; i < pressed_Buttons.Count; i++)
        {
            int index = i; // Capture the loop variable to avoid closure issues
            pressed_Buttons[index].onClick.AddListener(() =>
                BetButtonPressed((int)APIController.instance.authentication.entryAmountDetails.betValues[index]));
        }

        // Initialize slider-related objects
        slider_bg.SetActive(false);
        fillArea.SetActive(false);
        slider_txt.SetActive(false);
        sliderAutoCashNoTxt.gameObject.SetActive(false);
    }
    private void LateUpdate()
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

                if (!take && !lost && !isScroll && !howToPlay.activeSelf && !numPad && !insufficientBalance.activeSelf && !insufBal_Rumblebets.activeSelf &&
                    !NetworkHandler.instance.ConnectionPanel.activeSelf && !NetworkHandler.instance.waitingForResponse.activeSelf && !settingsPanelHandler.gameObject.activeSelf &&
                    !pauseGame && betAmount >= APIController.instance.authentication.entryAmountDetails.minBetValue)
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
            if (!take && !lost && !isScroll && !howToPlay.activeSelf && !numPad &&
                !NetworkHandler.instance.ConnectionPanel.activeSelf && !settingsPanelHandler.gameObject.activeSelf &&
                !pauseGame && betAmount >= APIController.instance.authentication.entryAmountDetails.minBetValue)
            {
                OnClickUp();
                Button_OFFEnter();
            }
        }

        // Enable/Disable cancel buttons based on TotalAmount
        bool isAmountSufficient = TotalAmount >= APIController.instance.authentication.entryAmountDetails.minBetValue;
        cancelButton.SetActive(isAmountSufficient);
        rumbleBet_cancelButton.SetActive(isAmountSufficient);

        // Handle animation based on internet connectivity
        if (!NetworkHandler.instance.ConnectionPanel.activeSelf)
        {
            Animation_Play();
        }
        else
        {
            Animation_Pause();
        }
    }
    public async void AmountColor_Glow()
    {
        amountGlow.SetActive(false);
        amountGlow.SetActive(true);
        await UniTask.Delay(350);
        amountGlow.SetActive(false);
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
        fireObj.SetActive(true);
        fireIdleObj.SetActive(true);
        unPress.SetActive(true);
        pressed.SetActive(false);

        ////////////////////////////////////////////////////
        ///

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
        PassTxt(betAmountTxt, $"{betAmount:F2} <size=30>{APIController.instance.userDetails.currency_type}</size>");
        PassTxt(totalAmountTxt, $"{TotalAmount:F2} <size=30>{APIController.instance.userDetails.currency_type}</size>");
        DebugHelper.Log("Amount Details Subscribed");
    }
    private void OnSwitchTab(bool isFocus)
    {
        DebugHelper.Log($"SwitchTab Status Check ********** {isFocus} || IsinFocus = {APIController.instance.isInFocus} || IsOnline {APIController.instance.isOnline}");
        if (CanPlayAudio)
            AudioListener.volume = (isFocus && APIController.instance.isOnline && APIController.instance.isInFocus) ? 1 : 0;
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
        if (startGame && !pauseGame && (!NetworkHandler.instance.ConnectionPanel.activeSelf))
        {
            if (Multiplier < 1.01f)
            {
                countTime += 7f * Time.deltaTime;
                slider.value = countTime;
            }
            if (Multiplier >= 1.01f && !isPressed)
            {
                timeSinceLastIncrement += Time.deltaTime;

                slider.gameObject.SetActive(true);

                countTime -= 1f * Time.deltaTime;
                sliderAutoCashNoTxt.text = countTime.ToString(" 0");
                audioController.StopAudio(AudioEnum.startSlider);
                slider.value = countTime;
                if (countTime < 0)
                {
                    countTime = 0;
                }
            }
            else if (Multiplier >= 1.01f && isPressed)
            {
                countTime = 7f;
                slider.maxValue = 7f;
                slider.value = 7f;
            }
        }
        yield return null;
        /*if (!startGame)
        {
            timerCount = false;
        }*/
        StartCoroutine(nameof(TimerCount));
    }
    IEnumerator HolidngButtons()
    {
        // Handle preheating UI visibility
        if (isPressed && Multiplier < 1.01f)
        {
            preHeating_txt.gameObject.SetActive(true);
            sliderTxt.text = null;
        }
        else if (Multiplier > 1.00f)
        {
            preHeating_txt.gameObject.SetActive(false);
        }

        // Handle bet amount updates
        BetAmountUpdates();

        // Handle internet disconnection and button press
        if (NetworkHandler.instance.ConnectionPanel.activeSelf && isPressed)
        {
            isPressed = false;
            Button_OFFEnter();
        }

        // Disable minus button if bet amount is below minimum
        if (betAmount <= APIController.instance.authentication.entryAmountDetails.minBetValue)
        {
            minusButton.enabled = false;
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
            ApplyBalloonParallaxEffect(holdButton.GetComponent<RectTransform>().anchoredPosition.y);
            ApplyParallaxEffect(holdButton.GetComponent<RectTransform>().anchoredPosition.y);

            heat_Anim.SetBool("isPlay1", false);
            heat_IdleAnim.SetBool("isPlay1", false);
            fireObj.SetActive(false);
            fireIdleObj.SetActive(false);
            takeCashbutton.enabled = false;
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
            slider_bg.SetActive(false);
            fillArea.SetActive(false);
            slider_txt.SetActive(false);
            sliderAutoCashNoTxt.gameObject.SetActive(false);
            //heat button
            heat_Anim.SetBool("isPlay1", false);
            heat_Anim.SetBool("isPlay2", false);
            heat_IdleAnim.SetBool("isPlay1", false);
            fireObj.SetActive(false);
            fireIdleObj.SetActive(false);
            //balloon parts
            ballonOut.gameObject.SetActive(true);
            balloonParts.SetActive(false);
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
            audioController.StopAudio(AudioEnum.reverseSlider);
            audioController.StopAudio(AudioEnum.Movement);
            // sliderOBjs
            slider_Anim.SetBool("isOFF", true);
            slider_bg.SetActive(false);
            fillArea.SetActive(false);
            slider_txt.SetActive(false);
            sliderAutoCashNoTxt.gameObject.SetActive(false);
            //balloon, takeCash & heat button
            heat_Anim.SetBool("isPlay1", false);
            heat_IdleAnim.SetBool("isPlay1", false);
            fireObj.SetActive(false);
            fireIdleObj.SetActive(false);
            balloonShake_blue.SetActive(false);
            balloonShake.SetActive(false);
            takeCashObj.SetActive(false);
            balloonParts.SetActive(true);
            heatTxt.color = new Color32(63, 15, 15, 200);
        }

        FireButton();
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
            takeCashbutton.enabled = false;
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
    IEnumerator Backgourn_Ballon_Fly()
    {
        while (true)
        {
            ApplyBalloonParallaxEffect(holdButton.GetComponent<RectTransform>().anchoredPosition.y);
            yield return null;
        }
    }
    IEnumerator CoundownTimerforIdle()
    {
        if (!startGame)
        {
            if (!LoadingPopUp.activeSelf && !take && !lost && !isScroll && !howToPlay.activeSelf && !numPad && !insufficientBalance.activeSelf && !insufBal_Rumblebets.activeSelf &&
                        !NetworkHandler.instance.ConnectionPanel.activeSelf && !NetworkHandler.instance.waitingForResponse.activeSelf && !settingsPanelHandler.gameObject.activeSelf)
            {
                currentTime -= 1 * Time.deltaTime;

                if (currentTime <= 0)
                {
                    currentTime = 0;
                    showPopUp.SetActive(true);
                }
            }
            else if (!LoadingPopUp.activeSelf && !take && !lost && !isScroll && !howToPlay.activeSelf && !numPad && !insufficientBalance.activeSelf && !insufBal_Rumblebets.activeSelf &&
                        !NetworkHandler.instance.ConnectionPanel.activeSelf && !NetworkHandler.instance.waitingForResponse.activeSelf && !settingsPanelHandler.gameObject.activeSelf)
            {
                currentTime = 60;
                showPopUp.SetActive(false);
            }

            if ((audioController != null && audioController.IsAnyAudioPlaying()))
            {
                currentTime = 60;
                showPopUp.SetActive(false);
            }
        }
        else if (startGame)
        {
            currentTime = 60;
            showPopUp.SetActive(false);
        }
        yield return null;
        StartCoroutine(nameof(CoundownTimerforIdle));
    }
    #endregion
    void BonusBalloon()
    {
        if (isBonus_1 || isBonus_2 || isBonus_3 || isScroll)
        {
            balloonBlue_Start.gameObject.SetActive(false);
        }
    }
    void Balloon_Burt()
    {
        //DebugHelper.Log(" GameProcess ===> GameLose");
        ResetBets();
        audioController.PlayAudio(AudioEnum.ballonPopOut);
        holdButton.enabled = false;
        timeSinceLastIncrement = 0f; // Reset the timer
        timeHeld = 0f; // Reset the time button is held
        winCash = 0f;
        _balanceUpdate = true;
        // sliderOBjs
        slider_Anim.SetBool("isOFF", true);
        slider_bg.SetActive(false);
        fillArea.SetActive(false);
        slider_txt.SetActive(false);
        sliderAutoCashNoTxt.gameObject.SetActive(false);
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
    void StartOfTheGame()
    {
        timeHold = Multiplier.ToString("0.00");
        Mstring = Multiplier.ToString("0.00");
        takeCashWintxt.text = TakeCash.ToString("0.00");

        if (startGame && Multiplier <= incrementRate)
        {
            stopper = true;
        }
        if (startGame && !lost)
        {
            Mstring = (Mathf.FloorToInt(Multiplier * 100) / 100f).ToString("0.00");
            Tstring = (betAmount * double.Parse(Mstring)).ToString("0.00");
            TakeCash = double.Parse(Tstring);
            //TakeCash = (betAmount * double.Parse(Mstring));
            takeCashWintxt.text = TakeCash.ToString("0.00");
        }

        if (stopper)
        {
            stopper = false;
            Multiplier += Time.deltaTime;
            Multiplier = Mathf.Min(Multiplier);
            string s = (Mathf.FloorToInt(Multiplier * 100) / 100f).ToString("0.00");
            multiplierTxt.text = s;
            multiplierTxt_Shadow.text = s;
            balloonBlue_Start.SetActive(false);
            ballon_Anim.SetBool("isJump", true);
            balloon_Objs();
        }
        else if (isPressed && startGame && Multiplier >= 1.01f)
        {
            balloonShake_blue.SetActive(false);
            balloonParts.SetActive(false);
            balloonShake.SetActive(true);
            string s = (Mathf.FloorToInt(Multiplier * 100) / 100f).ToString("0.00");
            multiplierTxt.text = s;
            multiplierTxt_Shadow.text = s;

            //TakeCash = betAmount * Multiplier;

            takeCashTxt.text = TakeCash.ToString("0.00");
            // Increment the timer by the time elapsed since the last frame
            timeSinceLastIncrement += Time.deltaTime;
        }

        if (startGame && takeBetAmount)
        {
            takeBetAmount = false;
            holdButton.enabled = false;
            sliderTxt.text = null;
        }

        if (startGame)
        {
            Button_Switch_OFF();
        }

        if (countTime >= 7 && !isPressed && !lost)
        {
            if (!take)
            {
                sliderTxt.text = sliderAutoCashTxt.text.ToString();
                sliderAutoCashNoTxt.gameObject.SetActive(true);
            }
        }

        if (countTime >= 6.5)
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
            takeCashbutton.enabled = true;
            takeCashImg.color = new Color32(255, 255, 255, 255);
            takeCashtxt.color = new Color32(194, 236, 166, 255);
            takeCashWintxt.color = new Color32(194, 236, 166, 255);
            takeCurrencytxt.color = new Color32(194, 236, 166, 255);
            takeCashObj.SetActive(true);
        }
        else
        {
            takeCashbutton.enabled = false;
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
            Multiplier *= incrementRate;
            // Update the multiplier text
            string s = (Mathf.FloorToInt(Multiplier * 100) / 100f).ToString("0.00");
            multiplierTxt.text = s;
            multiplierTxt_Shadow.text = s;

            // Calculate the win amount
            //TakeCash = betAmount * Multiplier;

            // Update the take cash text
            takeCashTxt.text = TakeCash.ToString("0.00");
        }
    }
    public void OnInternetCheckSuccess()
    {
        checking = true;
        DebugHelper.Log("Process Final Winnings Changed");
    }
    bool checking = false;
    public ImageSequencer imageSequencer;
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
        balloonShake_blue.GetComponent<ImageSequencer>().enabled = false;
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
        balloonShake_blue.GetComponent<ImageSequencer>().enabled = true;
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

    public bool isCreateMatchSucceess = false;
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

        CreateMatchAPICall();
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
        //string value = TakeCash.ToString("F2");
        string value = TakeCash.ToString("0.00");
        double amount = double.Parse(value);
        TransactionMetaData val = new TransactionMetaData();
        val.Amount = amount;
        val.Info = message;

        DebugHelper.Log("isCreateMatchSucceess ====> success " + isCreateMatchSucceess);
        WinningBetAPICall(amount, TakeCash);
    }
    public void WinningBetAPICall(double WinAmount, double PotAmount)   //WINNINGBETAPI CALLING METHOD
    {
        TransactionMetaData _metaData = new TransactionMetaData();
        _metaData.Amount = WinAmount;
        _metaData.Info = "Game Won";
        DebugHelper.Log($"1 WinningBetAPI Call ========> {WinAmount}  && POt Amount {PotAmount}");

        DebugHelper.Log(" WinningBetAPI , Checking Internet" + checking + MatchRes.MatchToken);
        APIController.instance.WinningsBetMultiplayerAPI(BetIndex, betID, WinAmount, betAmount, PotAmount, _metaData, (success) =>
        {
            DebugHelper.Log($"2 WinningBetAPI Call ========> {WinAmount}  && POt Amount {PotAmount}  && bet index {BetIndex} , My Bet Amount {betAmount}");

            if (success)
            {
                DebugHelper.Log("3 WinningBetAPI Call Success========> ");
                IsCreateMatchCalled = false;

                take = true;
                startGame = false;
                onClick = false;
                //multiplier = float.Parse(multiplier.ToString("0.00"));
                multiplierTxt.text = Multiplier.ToString();
                multiplierTxt_Shadow.text = Multiplier.ToString();
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
                sliderAutoCashNoTxt.gameObject.SetActive(false);
                slider_Anim.SetBool("isOFF", true);
                ballon_Anim.SetBool("isTake", true);
                //winCount
                //if (!winCount && betAmount <= 5f)
                //{
                //    winCash++;
                //}
                //else
                //{
                isNormal = true;
                //}

                //  Demo_Bonus();

                if ((WinCash_demo < 10))
                {
                    audioController.PlayAudio(AudioEnum.winGame);
                    winPanel.SetActive(true);
                    // TakingCash();
                    Winning_Animations();
                }
                Call_Functions();
                DelayFuction();
            }
            else
            {
                DebugHelper.Log("WinningBetAPIfailed========>");
            }
        }, APIController.instance.userDetails.Id, false, WinAmount == 0 ? false : true, gameName, operatorName, APIController.instance.userDetails.gameId, APIController.instance.userDetails.commission, MatchRes.MatchToken);
    }
    void Call_Functions()
    {
        DebugHelper.Log("Check4");
        //if (winCash == Winbonus)
        //{
        //    isBonus_1 = true;
        //    if (!isSet)
        //        isBonus_2 = true;
        //    else if (isSet)
        //        isBonus_3 = true;
        //}

        if (winCash != Winbonus)
            isNormal = true;
    }
    async void DelayFuction()
    {
        DebugHelper.Log("Check3");
        //if (isBonus_1)
        //{
        //    if (WinCash_demo == 10)
        //    {
        //        await UniTask.Delay(1000);
        //        Bonus_Conditions();
        //    }

        //    await UniTask.Delay(3000);
        //    winPanel.SetActive(false);

        //    await UniTask.Delay(100);
        //    if (WinCash_demo < 10)
        //        Bonus_Conditions();
        //}
        //if (isBonus_2)
        //{
        //    await UniTask.Delay(5500);
        //    TimeDelay();
        //}

        if (isNormal)
        {
            DebugHelper.Log("Check1");
            await UniTask.Delay(3500);
            TimeDelay();
        }

        //if (isBonus_3)
        //{
        //    await UniTask.Delay(3000);
        //    Bonus_Delay();
        //}
    }
    async void DemoAPIReset()
    {
        if (isBonus_1)
        {
            if (WinCash_demo == 10)
            {
                await UniTask.Delay(1000);
                Bonus_Conditions();
            }

            await UniTask.Delay(3000);
            winPanel.SetActive(false);

            await UniTask.Delay(100);
            if (WinCash_demo < 10)
                Bonus_Conditions();
        }
        if (isBonus_2)
        {
            await UniTask.Delay(100);
            TimeDelay();
        }

        if (isNormal)
        {
            await UniTask.Delay(100);
            TimeDelay();
        }

        if (isBonus_3)
        {
            await UniTask.Delay(1000);
            Bonus_Delay();
        }

    }
    void TimeDelay() // Clear UI
    {
        DebugHelper.Log("Check2");
        multiplierTxt.color = new Color32(116, 85, 185, 255);
        xTxt.color = new Color32(116, 85, 185, 255);
        multiplierTxt_Shadow.color = Color.black;
        xTxt_Shadow.color = Color.black;
        Multiplier = 0f;
        multiplierTxt.text = Multiplier.ToString("0.00");
        multiplierTxt_Shadow.text = Multiplier.ToString("0.00");
        TakeCash = 0f;
        takeCashTxt.text = TakeCash.ToString("0.00");
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

        //Slider Animation
        slider_Anim.SetBool("isOFF", false);
        slider_Anim.SetBool("isON", false);

        //balloon
        ballon_Anim.enabled = true;
        ballon_Anim.SetBool("isTake", false);
        ballon_Anim.SetBool("isJump", false);
        balloonShake_blue.SetActive(false);
        balloonShake.SetActive(false);
        balloonParts.transform.localPosition = new Vector3(0f, -430.1323f, 0f);
        TxtObjs.gameObject.SetActive(false);
        BalloonDelay();
        heat_Anim.SetBool("isPlay1", true);
        fireObj.SetActive(false);
        fireIdleObj.SetActive(true);

        // sliderOBjs
        slider_Anim.SetBool("isON", true);
        Slider_Objs();
        takeCashbutton.interactable = true;
    }
    async void BalloonDelay()
    {
        await UniTask.Delay(100);
        balloonParts.SetActive(true);
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
        multiplierTxt.color = new Color32(248, 140, 52, 255);
        xTxt.color = new Color32(248, 140, 52, 255);
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
        ButtonSelect_Anim();
    }
    void Winning_Animations()
    {
        //winTxt.text = Multiplier.ToString("0.00" + " <size=80>X</size>");
        winTxt.text = TakeCash.ToString("0.00" + " <size=70>INR</size>");
        WinTxtObj();
    }
    public void TakingCash()
    {
        //await UniTask.Delay(3000);
        API_Winning();
    }
    async void WinTxtObj()
    {
        await UniTask.Delay(2000);
        winTxt.transform.DORotate(new Vector3(0f, 360f, 0f), 0.5f, RotateMode.FastBeyond360).SetEase(Ease.InOutSine);
        if (APIController.instance.userDetails.currency_type == "USD")
        {
            winTxt.text = TakeCash.ToString("0.00" + " <size=70>USD</size>");
        }
        else if (APIController.instance.userDetails.currency_type == "EUR")
        {
            winTxt.text = TakeCash.ToString("0.00" + " <size=70>EUR</size>");
        }
        else if (APIController.instance.userDetails.currency_type == "INR")
        {
            winTxt.text = TakeCash.ToString("0.00" + " <size=70>INR</size>");
        }

        //winTxt.color = Color.green;
    }
    async void balloon_Objs()
    {
        await UniTask.Delay(400);
        TxtObjs.gameObject.SetActive(true);
        await UniTask.Delay(400);
        ballon_Anim.SetBool("isJump", false);
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
        slider_txt.SetActive(true);
        sliderTxt.text = sliderDupTxt.text.ToString();
        await UniTask.Delay(500);
        slider_bg.SetActive(true);
        fillArea.SetActive(true);
    }

    #region ::::::::::::::::::::::::::: Bonus Functions :::::::::::::::::::::::::::
    void Bonus_Delay()
    {
        multiplierTxt.color = Color.white;
        xTxt.color = Color.white;
        Multiplier = 0f;
        multiplierTxt.text = Multiplier.ToString("0.00");
        multiplierTxt_Shadow.text = Multiplier.ToString("0.00");
        TakeCash = 0f;
        takeCashTxt.text = TakeCash.ToString("0.00");
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
        if (winCash == Winbonus)
        {
            bonusObj.gameObject.SetActive(true);
            audioController.PlayAudio(AudioEnum.bonusEntry3);
            Fill_Img.instance.Bonus_Script();
            bonusBallon.SetBool("isOpen", true);

            // Bonus Reward Fill Img
            if (timeRemaining < totalTime)
            {
                if (betAmount <= 5f)
                {
                    if (betAmount <= 1.4f)
                    {
                        Bonustimer = WinCash_demo;
                    }
                    else if (betAmount >= 1.5f && betAmount <= 2f)
                    {
                        Bonustimer = WinCash_demo;
                    }
                    else if (betAmount >= 2.1f && betAmount <= 5f)
                    {
                        Bonustimer = WinCash_demo;
                    }
                }
            }
            else
            {
                // Timer has reached the total time
                timeRemaining = totalTime; // Ensure timeRemaining does not exceed totalTime
                isScroll = true;
            }
            winCash = 0;
            isBonus_1 = false;
        }
    }
    void Demo_Bonus()
    {
        if (winCash == Winbonus)
        {
            if (betAmount <= 5f)
            {
                if (betAmount <= 1.4f)
                {
                    WinCash_demo += 8f;
                }
                else if (betAmount >= 1.5f && betAmount <= 2f)
                {
                    WinCash_demo += 4f;
                }
                else if (betAmount >= 2.1f && betAmount <= 5f)
                {
                    WinCash_demo += 2f;
                }
            }

            if (WinCash_demo > 10f)
            {
                WinCash_demo = 10f;
            }

        }
        if (WinCash_demo >= 10)
        {
            isSet = true;
            isScroll = true;
            Bonustimer = 0f;
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
    bool InternetCheck;
    public void OnClickDown()
    {
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

            if (!numPad && !buttonPress && !lost)
            {
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
            SetupGameForNewRound();
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
        balloonShake_blue.SetActive(false);
        balloonParts.SetActive(false);
        ApplyParallaxEffect(holdButton.GetComponent<RectTransform>().anchoredPosition.y);
        touch = true;

        // Set animations for heat effect
        heat_Anim.SetBool("isPlay1", true);
        heat_Anim.SetBool("isPlay2", true);
        heat_IdleAnim.SetBool("isPlay1", false);

        // Manage fire-related objects
        fireObj.SetActive(true);
        fireIdleObj.SetActive(false);

        // Disable all button animations
        foreach (var button in button_Anim)
        {
            button.SetActive(false);
        }

        // Disable the take cash button
        takeCashbutton.enabled = false;

        // Update slider text and display settings
        if (isFire && Multiplier >= 1.01f)
        {
            sliderTxt.text = null;
            sliderAutoCashNoTxt.gameObject.SetActive(false);
        }
        else if (!isFire && Multiplier >= 1.01f)
        {
            sliderTxt.text = sliderAutoCashTxt.text.ToString();
            sliderAutoCashNoTxt.gameObject.SetActive(true);
        }

    }
    private void SetupGameForNewRound()
    {
        makeLose = true;

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
        isBegin = true;
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
                balloonShake_blue.SetActive(true);
                balloonShake.SetActive(false);
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

        // Start the background animation
        StartCoroutine(Backgourn_Ballon_Fly());

        // Disable heat button collider and trigger necessary animations
        heatbtnCollider.enabled = true;
        ButtonSelect_Anim();

        // Activate the hand gestures and slider animation
        //   handGestures_btAmt.SetActive(true);
        slider_Anim.SetBool("isON", true);

        // Call method to handle slider objects
        Slider_Objs();
    }
    #endregion

    #region { ::::::::::::::::::::::::: Buttons ::::::::::::::::::::::::: }
    public Button settingsBtn;
    public int currentClickValue;
    public void BetButtonPressed(int betValue)
    {
        if (takeBetAmount)
            audioController.PlayAudio(AudioEnum.buttonClick);

        if (keyBoard.cancelButton.gameObject.activeSelf)
            keyBoard.OnCancelInput();

        if (!startGame && !take && !isScroll && !onClick)
        {
            // Set the bet amount
            betAmount = betValue;
            betAmountTxt.text = $"{betAmount:F2} <size=30>{currencyType}</size>";
            BetAmountTxt_Scaling();

            // Set buttons' active state based on bet amount
            SetBetButtonsActiveState(betValue);

            AmountColor_Glow();
            UpdateButtonAnimations(betValue);

            plusButton.enabled = true;
            minusButton.enabled = true;
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
    public void SelectBetButton(int s)
    {
        audioController.PlayAudio(AudioEnum.buttonClick);

        if (BetInputController.Instance.BetPanel.gameObject.activeSelf)
        {
            BetInputController.Instance.BetAmtInput.textViewport.gameObject.SetActive(false);

            BetInputController.Instance.BetPanel.gameObject.SetActive(false);
            KeyBoardHandler.instance.cancelButton.gameObject.SetActive(false);
            betAmount = s;
            UpdateBetAmountDisplay();
            return;
        }

        if (!startGame && !take && !isScroll && !onClick)
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
        plusButton.enabled = true;
        minusButton.enabled = true;
        plusButtomImg.color = new Color32(255, 255, 255, 255);
        minusButtonImg.color = new Color32(255, 255, 255, 255);
    }
    private void DisablePlusButton()  // SelectBetButton
    {
        plusButton.enabled = false;
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

        if (BetInputController.Instance.BetPanel.gameObject.activeSelf)
        {
            BetInputController.Instance.CloseKeyPadPanel();
            BetInputController.Instance.RestrictInput();
            BetInputController.Instance.BetAmtInput.textViewport.gameObject.SetActive(false);
            return;
        }

        if (keyBoard.cancelButton.gameObject.activeSelf)
            keyBoard.OnCancelInput();

        if (!startGame && !take && !isScroll && !onClick)
        {
            if (betAmount < APIController.instance.authentication.entryAmountDetails.maxBetValue)
            {
                betAmount += APIController.instance.authentication.entryAmountDetails.incrementValue;
                betAmountTxt.text = betAmount.ToString("0.00" + " <size=30>" + currencyType + "</size>");
                BetAmountTxt_Scaling();
                minusButton.enabled = true;
                minusButtonImg.color = new Color32(255, 255, 255, 255);
                AmountColor_Glow();
            }
            if (betAmount > 99.99f)
                if (betAmount >= APIController.instance.authentication.entryAmountDetails.maxBetValue)
                {
                    betAmount = APIController.instance.authentication.entryAmountDetails.maxBetValue;
                    plusButton.enabled = false;
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
    }
    public void MinusButton()
    {
        audioController.PlayAudio(AudioEnum.buttonClick);

        if (BetInputController.Instance.BetPanel.gameObject.activeSelf)
        {
            BetInputController.Instance.CloseKeyPadPanel();
            BetInputController.Instance.RestrictInput();
            BetInputController.Instance.BetAmtInput.textViewport.gameObject.SetActive(false);
            return;
        }

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
                minusButton.enabled = false;
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
        button_1.enabled = true; button_2.enabled = true; button_5.enabled = true; button_10.enabled = true;

        // Enable plus and minus buttons with default color
        EnableButton_Switch_ON(plusButton, plusButtomImg);
        EnableButton_Switch_ON(minusButton, minusButtonImg);
    }
    void Button_Switch_OFF()
    {
        // Disable all buttons
        button_1.enabled = false; button_2.enabled = false; button_5.enabled = false; button_10.enabled = false;

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
        if (onClick/*takeBetAmount*/)
        {
            numPadButton.gameObject.SetActive(false);
        }
        else
        {
            numPadButton.gameObject.SetActive(true);
        }

        if (betAmount <= APIController.instance.authentication.entryAmountDetails.incrementValue)
        {
            minusButton.enabled = false;
            minusButtonImg.color = new Color32(255, 255, 255, 100);
        }
        else if (betAmount > APIController.instance.authentication.entryAmountDetails.incrementValue)
        {
            minusButton.enabled = true;
            minusButtonImg.color = new Color32(255, 255, 255, 255);
        }

        if (betAmount < APIController.instance.authentication.entryAmountDetails.maxBetValue)
        {
            plusButton.enabled = true;
            plusButtomImg.color = new Color32(255, 255, 255, 255);
        }
        else if (betAmount >= APIController.instance.authentication.entryAmountDetails.maxBetValue)
        {
            plusButton.enabled = false;
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
    public void Close_ShowPopUp() // Show PopUp for Close Button
    {
        currentTime = 60;
        showPopUp.SetActive(false);
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