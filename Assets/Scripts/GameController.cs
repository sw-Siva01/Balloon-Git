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
    public float Mlose = 0.00f;
    public string mString;  // Initial multiplier value
    [SerializeField] float incrementRate = 1.01f;  // Increment rate
    [SerializeField] float incrementInterval = 0.2f;  // Time interval between increments in seconds
    [SerializeField] float timeSinceLastIncrement = 0f;   // Timer to track time since last increment
    [SerializeField] float maxHoldTime = 5f; // Maximum time to hold the button
    [SerializeField] float minHoldTime = 2f; // Minimum time to hold the button
    [SerializeField] float holdHeight = 2f; // Minimum time to hold the button
    public float timeHeld = 0f;   // Timer to track time button is held
    public string timeHold;   // Timer to track time 
    public float takeCash;  // TakeCash
    public string tString;
    public float WinAmount = 0;
    public int gameCounts;
    [SerializeField] float TotalAmount = 250.00f;  // Total Amount
    public float bonusTimer;
    [SerializeField] KeyBoardHandler keyBoard;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("Buttons")]
    [SerializeField] Button TakeCashbutton;
    [SerializeField] Button holdButton;
    [SerializeField] Button holdButton_dup;

    /*[SerializeField] Button empty_holdButton;
    [SerializeField] GameObject heatButton_Anim;*/

    [SerializeField] Image heatButton_BG;
    [SerializeField] TextMeshProUGUI heatTxt;

    //UI bet Buttons
    [Header("UI bet Buttons")]
    [SerializeField] Button button_1;
    [SerializeField] Button button_2;
    [SerializeField] Button button_5;
    [SerializeField] Button button_10;

    [SerializeField] List<Button> setected_Buttons = new List<Button>();
    public Button plusButton, minusButton;

    [SerializeField] GameObject[] button_Anim;
    [SerializeField] GameObject numPadButton;
    [SerializeField] GameObject takeCashObj;
    /*[SerializeField] GameObject TakeCashAnimImg;*/
    [SerializeField] TextMeshProUGUI TakeCashtxt;
    [SerializeField] TextMeshProUGUI takeCashWintxt;
    [SerializeField] TextMeshProUGUI takeCurrencytxt;
    [SerializeField] Image TakeCashImg;
    //public Image AmountGlow;

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
    public bool makeLose;
    public bool startGame;
    //
    private bool isBegin;
    private bool pauseGame;
    private bool isPressed;
    private bool buttonPress;
    private bool takeBetAmount;
    private bool isSet;
    private bool isFire;
    private bool lost;
    private bool gameLost;
    private bool isWin;
    private bool take;
    private bool isBonus_1;
    private bool isBonus_2;
    private bool isBonus_3;
    private bool isNormal;
    private bool touch;
    private bool stopper;
    private bool timerCount = true;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    // TextMeshProUGUI
    [Header("TextMeshProUGUI")]
    public TextMeshProUGUI multiplierTxt;
    [SerializeField] TextMeshProUGUI Xtxt;
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
    [SerializeField] GameObject preHeating_txt;
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
    [SerializeField] GameObject fireObj;

    // slider
    [SerializeField] Animator slider_Anim;

    [SerializeField] Animator bonusBallon;

    /*// takeCash button
    [SerializeField] Animator takecashOut;*/

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    // ScrollView GameObjects
    [Header("Insufficient Balance")]
    [SerializeField] GameObject InsufficientBalance;
    [SerializeField] GameObject InsufBal_Rumblebets;
    [SerializeField] GameObject cancelButton;
    [SerializeField] GameObject rumbleBet_cancelButton;
    [SerializeField] GameObject HowToPlay;

    public GameObject AmountGlow;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("HandGestures")]
    [SerializeField] GameObject HandGestures_start;
    [SerializeField] GameObject HandGestures_btAmt;
    [SerializeField] Collider heatbtnCollider;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("MaxBet_Reached")]
    [SerializeField] GameObject maxBet_Reached;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("Audio Script")]
    public MasterAudioController audioController;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    // ScrollView GameObjects
    [Header("API Controller")]
    private string currencyType;
    private string playerID;
    private string playerName;
    private GameObject DemoText;
    private string operatorName;
    private string gameName;
    private string lobbyName;
    public string betID;
    private int BetIndex;
    private CreateMatchResponse MatchRes;
    public GameObject internetDisconnectPannel;
    public bool IsInTab = true;
    public bool IsInterNet;
    public bool CanPlayAudio;

    public GameObject LoadingPopUp;
    public GameObject ResponsePopUp;
    [SerializeField] Image inputField;
    public static GameController instance;

    public GameObject test;

    #endregion ::::::::::::::::::::::::: END :::::::::::::::::::::::::
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------//
    private void Awake()
    {
        instance = this;
        CanPlayAudio = false;
        settingsBtn.onClick.AddListener(() => UI_Controller.instance.settingsHandler.CallingSettingPanel());
    }
    private void Start()
    {
        APIController.instance.OnSwitchingTab += OnSwitchTab;

        touch = true;
        takeBetAmount = true;
        TakeCashImg.color = new Color32(140, 140, 140, 255);
        TakeCashtxt.color = new Color32(140, 140, 140, 255);
        takeCashWintxt.color = new Color32(140, 140, 140, 255);
        takeCurrencytxt.color = new Color32(140, 140, 140, 255);
        /*TakeCashAnimImg.SetActive(false);*/
        takeCashObj.SetActive(false);
        // Set the initial multiplier text
        multiplierTxt.text = multiplier.ToString("0.00"/* + "<size=40>X</size>"*/);
        StartCoroutine(HolidngButtons());
        /*totalAmountTxt.text = TotalAmount.ToString("0.00" + " <size=35>USD</size>");*/
        /*TakeCashbutton.enabled = false;
        TakeCashbutton.onClick.AddListener(TakeCashOut);*/
        initialBackgroundPosition = background.localPosition;

        //winBonus = UnityEngine.Random.Range(3, 6);    // use this always
        winBonus = 5;                                   // only for testing
        /*ButtonSelect_Anim();*/
        // Sliders
        /*timeout = false;*/
        slider.maxValue = 7f;
        slider.minValue = 1f;
        StartCoroutine(TimerCount());

        // Bonus Reward Fill Img
        timeRemaining = 0f; // Start the timer at 0
        /* UpdateTimerUI(); // Initialize the UI*/
        StartCoroutine(FillImg());

        setected_Buttons[0].onClick.AddListener(delegate { SelectBetButton(1); });
        setected_Buttons[1].onClick.AddListener(delegate { SelectBetButton(2); });
        setected_Buttons[2].onClick.AddListener(delegate { SelectBetButton(5); });
        setected_Buttons[3].onClick.AddListener(delegate { SelectBetButton(10); });

        // sliderOBjs
        slider_bg.SetActive(false);
        fillArea.SetActive(false);
        slider_txt.SetActive(false);
        sliderAutoCashNoTxt.gameObject.SetActive(false);
        Debug.Log("UserDetails" + APIController.instance.userDetails.Id + " player ID " + playerID);

        APIController.instance.OnUserDetailsUpdate += InitPlayerDetails;
        APIController.instance.OnUserBalanceUpdate += InitAmountDetails;
        APIController.instance.OnUserDeposit += InitUserDeposit;
    }
    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //test.gameObject.SetActive(true);
            //AmountColor_Glow();
        }
        Vector3 rayOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 rayDirection = Vector3.forward; // Change this to whatever direction you need
        RaycastHit hit;
        bool ISActive = false;
        if (Physics.Raycast(rayOrigin, rayDirection, out hit))
        {
            if (hit.collider.gameObject.name == "Heat_button")
            {
                if (Input.GetMouseButtonDown(0))
                {
                    ISActive = true;
                    if (!take && !lost && !isScroll && !HowToPlay.activeSelf && !ResponsePopUp.activeSelf)
                    {
                        Button_ONEnter();
                        OnClickDown();
                    }
                }
                if (Input.GetMouseButton(0))
                {
                    ISActive = true;
                }
            }
        }

        if (isPressed && !ISActive)
        {
            //isPressed = false;
            if (!take && !lost && !isScroll && !HowToPlay.activeSelf && !ResponsePopUp.activeSelf)
            {
                OnClickUp();
                Button_OFFEnter();
            }
        }

        if (TotalAmount <= 0.09f)
        {
            cancelButton.SetActive(false);
            rumbleBet_cancelButton.SetActive(false);
        }
        else if (TotalAmount >= 0.10f)
        {
            cancelButton.SetActive(true);
            rumbleBet_cancelButton.SetActive(true);
        }
    }
    public async void AmountColor_Glow()
    {
        AmountGlow.SetActive(false);
        AmountGlow.SetActive(true);
        await UniTask.Delay(350);
        AmountGlow.SetActive(false);
    }

    #region { ::::::::::::::::::::::::: API ::::::::::::::::::::::::: }
    public void InitPlayerDetails()
    {
        Debug.Log("UserDetails" + APIController.instance.userDetails.Id + " player ID " + playerID);

        playerID = APIController.instance.userDetails.Id;
        playerName = APIController.instance.userDetails.name;
        UI_Controller.instance.settingsHandler.playerNameTxt.text = playerName;
        //PassTxt(TowerUIController.instance.SettingsPanel.playerName, playerName);
        operatorName = APIController.instance.userDetails.game_Id.Split("_")[0].ToString();
        gameName = APIController.instance.userDetails.game_Id.Split("_")[1].ToString();
        lobbyName = "Room : " + DateTime.UtcNow + UnityEngine.Random.Range(100, 999);
        //UIController.LoadingPanel.gameObject.SetActive(false);
        if (GameController.instance.LoadingPopUp.activeSelf)
            GameController.instance.LoadingPopUp.SetActive(false);

        GameController.instance.ResponsePopUp.SetActive(true);
        Debug.Log("Player Details Subscribed");

    }
    public void InitAmountDetails()
    {
        TotalAmount = (float)APIController.instance.userDetails.balance;
        string m = TotalAmount.ToString("0.00");
        TotalAmount = float.Parse(m);
        currencyType = APIController.instance.userDetails.currency_type;
        PassTxt(totalAmountTxt, APIController.instance.userDetails.currency_type);
        PassTxt(takeCurrenyType, APIController.instance.userDetails.currency_type);
        PassTxt(betAmountTxt, betAmount.ToString("0.00") + " " + APIController.instance.userDetails.currency_type);
        PassTxt(totalAmountTxt, $"{TotalAmount:F2} <size=35>{APIController.instance.userDetails.currency_type}</size>");
        /*PassTxt(totalAmountTxt, $"{TotalAmount:F2}" + APIController.instance.userDetails.currency_type);*/    //takeCurrenyType
        Debug.Log("Amount Details Subscribed");
    }
    private void OnSwitchTab(bool isFocus)
    {
        #region
        /*if (APIController.instance.isInFocus && isFocus && APIController.instance.isOnline)
        {
            Debug.Log("Switch Sounds 1 = " + isFocus);
            if (!InternetChecking.instance.connectionPanel.activeSelf && !ResponsePopUp.activeSelf)
            {
                AudioListener.volume = 1;
                Debug.Log(" ^^^^^^^^^ 2 : " + AudioListener.volume);
                Debug.Log("Switch Sounds 2 = " + isFocus);
            }
        }
        else
        {
            Debug.Log("Switch Sounds 3 = " + isFocus);
            AudioListener.volume = 0;
        }*/
        #endregion

        Debug.Log($"SwitchTab Status Check ********** {isFocus} || IsinFocus = {APIController.instance.isInFocus} || IsOnline {APIController.instance.isOnline}");
        if (CanPlayAudio)
            AudioListener.volume = (isFocus && APIController.instance.isOnline && APIController.instance.isInFocus) ? 1 : 0;
        IsInTab = isFocus;

    }
    public void InitUserDeposit()
    {
        Debug.Log("Deposit Called");
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
        while (timerCount)
        {
            Debug.Log(" ^^^^^^^^^^^^^^========> ");
            if (startGame && !pauseGame && (!InternetChecking.instance.connectionPanel.activeSelf))
            {
                if (!pauseGame && multiplier < 1.01f)
                {
                    countTime += 7f * Time.deltaTime;
                    slider.value = countTime;
                }
                if (!pauseGame && multiplier >= 1.01f && !isPressed)
                {
                    timeSinceLastIncrement += Time.deltaTime;

                    slider.gameObject.SetActive(true);

                    countTime -= 1f * Time.deltaTime;
                    sliderAutoCashNoTxt.text = countTime.ToString(" 0");
                    Debug.Log("Internet Checking : " + InternetCheck);
                    audioController.StopAudio(AudioEnum.startSlider);
                    slider.value = countTime;
                    if (countTime < 0)
                    {
                        countTime = 0;
                    }
                }
                else if (!pauseGame && multiplier >= 1.01f && isPressed)
                {
                    countTime = 7f;
                    slider.maxValue = 7f;
                    slider.value = 7f;
                }
            }
            //yield return null;
            yield return new WaitForSeconds(0.1f);
            if (!startGame)
            {
                timerCount = false;
            }

        }
    }
    IEnumerator HolidngButtons()
    {
        while (true)
        {
            if (isPressed && multiplier < 1.01f)
            {
                preHeating_txt.gameObject.SetActive(true);
                sliderTxt.text = null;
            }
            else if (multiplier > 1.00f)
            {
                preHeating_txt.gameObject.SetActive(false);
            }
            BetAmountUpdates();

            if (buttonPress == true)
            {
                if (APIController.instance.userDetails.isBlockApiConnection)
                {
                    InsufficientBalance.SetActive(true);
                    InsufBal_Rumblebets.SetActive(false);
                }
                else
                {
                    InsufficientBalance.SetActive(false);
                    InsufBal_Rumblebets.SetActive(true);
                }
            }

            if (isScroll)
            {
                ApplyBalloonParallaxEffect(holdButton.GetComponent<RectTransform>().anchoredPosition.y);
                ApplyParallaxEffect(holdButton.GetComponent<RectTransform>().anchoredPosition.y);

                /*heat_Anim.SetBool("isStart", false);*/
                heat_Anim.SetBool("isPlay1", false);
                fireObj.SetActive(false);
                TakeCashbutton.enabled = false;
                TakeCashImg.color = new Color32(140, 140, 140, 255);

                /*   heatButton_Anim.SetActive(false);*/
                heatButton_BG.color = new Color32(140, 140, 140, 255);
                heatTxt.color = new Color32(63, 15, 15, 200);
            }
            if (lost)
            {
                //holdButtonEvent.enabled = false;
                startGame = false;
                preHeating_txt.gameObject.SetActive(false);
                audioController.StopAudio(AudioEnum.reverseSlider);
                audioController.StopAudio(AudioEnum.startSlider);
                audioController.StopAudio(AudioEnum.Movement);
                /*    empty_holdButton.gameObject.SetActive(true);*/
                ballon_Anim.SetBool("isOut", true);
                // sliderOBjs
                slider_Anim.SetBool("isOFF", true);
                slider_bg.SetActive(false);
                fillArea.SetActive(false);
                slider_txt.SetActive(false);
                sliderAutoCashNoTxt.gameObject.SetActive(false);
                /*heat_Anim.SetBool("isStart", false);
                heat_Anim.SetBool("isEnd", false);*/
                heat_Anim.SetBool("isPlay1", false);
                heat_Anim.SetBool("isPlay2", false);
                fireObj.SetActive(false);

                ballonOut.gameObject.SetActive(true);
                balloonParts.SetActive(false);
                balloonShake.SetActive(false);
                balloonShake_blue.SetActive(false);

                TakeCashImg.color = new Color32(140, 140, 140, 255);
                TakeCashtxt.color = new Color32(140, 140, 140, 255);
                takeCashWintxt.color = new Color32(140, 140, 140, 255);
                takeCurrencytxt.color = new Color32(140, 140, 140, 255);
                /*    heatButton_Anim.SetActive(false);*/
                heatButton_BG.color = new Color32(140, 140, 140, 255);
                heatTxt.color = new Color32(63, 15, 15, 200);
                /*TakeCashAnimImg.SetActive(false);*/
                takeCashObj.SetActive(false);
            }
            if (take)
            {
                // sliderOBjs
                startGame = false;
                audioController.StopAudio(AudioEnum.reverseSlider);
                audioController.StopAudio(AudioEnum.Movement);
                slider_Anim.SetBool("isOFF", true);
                slider_bg.SetActive(false);
                fillArea.SetActive(false);
                slider_txt.SetActive(false);
                sliderAutoCashNoTxt.gameObject.SetActive(false);
                /*heat_Anim.SetBool("isStart", false);*/
                heat_Anim.SetBool("isPlay1", false);
                fireObj.SetActive(false);

                balloonShake_blue.SetActive(false);
                balloonShake.SetActive(false);
                /*TakeCashAnimImg.SetActive(false);*/
                takeCashObj.SetActive(false);
                balloonParts.SetActive(true);
                /*   heatButton_Anim.SetActive(false);*/
                heatButton_BG.color = new Color32(140, 140, 140, 255);
                heatTxt.color = new Color32(63, 15, 15, 200);
            }
            FireButton();
            StartOfTheGame();

            if (startGame && !takeBetAmount)
            {
                Debug.Log(" PresseEvent ====> 1");
                timeHold = multiplier.ToString("F2");
                // Check if the button has been held for a random time between min and max hold time
                if (float.Parse(timeHold) >= holdHeight)
                {
                    // Bet is lost
                    Balloon_Burt();
                }
            }


            if (isPressed && isFire && !lost)
            {
                // To move the Target pos Up at the Start
                ApplyParallaxEffect(holdButton.GetComponent<RectTransform>().anchoredPosition.y);

                startGame = true;

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
                TakeCashbutton.enabled = false;
                // Reset the timer
                timeSinceLastIncrement = 5.9f;
                timeHeld = 0f;
                isPressed = false;
            }

            yield return null;
        }
    }
    void Balloon_Burt()
    {
        gameLost = true;
        ResetBets();
        lost = true;
        audioController.PlayAudio(AudioEnum.ballonPopOut);
        holdButton.enabled = false;
        timeSinceLastIncrement = 0f; // Reset the timer
        timeHeld = 0f; // Reset the time button is held
        winCash = 0f;

        // sliderOBjs
        slider_Anim.SetBool("isOFF", true);
        slider_bg.SetActive(false);
        fillArea.SetActive(false);
        slider_txt.SetActive(false);
        sliderAutoCashNoTxt.gameObject.SetActive(false);
        takeCashObj.SetActive(false);
        /*takecashOut.SetBool("isTake", false);*/
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
            if (isFire && multiplier >= 1.01f && !isPressed)
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
            /*heat_Anim.SetBool("isEnd", false);*/
            heat_Anim.SetBool("isPlay2", false);
            fireObj.SetActive(false);
            if (!lost && !take && multiplier >= 1.01f && !isScroll)
            {
                balloonShake_blue.SetActive(true);
                balloonShake.SetActive(false);
            }
        }
        /*if (!holdButton_dup.gameObject.activeSelf)
        {
            isSet = false;
        }*/

        if (!isFire && multiplier >= 1.01f)
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
        timeHold = multiplier.ToString("0.00");
        mString = multiplier.ToString("0.00");
        takeCashWintxt.text = takeCash.ToString("0.00");

        if (startGame && multiplier <= incrementRate)
        {
            stopper = true;
        }
        if (startGame && !lost)
        {
            tString = (betAmount * float.Parse(mString)).ToString("0.00");
            //takeCash = betAmount * multiplier;
            takeCash = float.Parse(tString);
            takeCashWintxt.text = takeCash.ToString("0.00");
        }

        if (stopper)
        {
            stopper = false;

            multiplier += Time.deltaTime;
            multiplier = Mathf.Min(multiplier);

            multiplierTxt.text = multiplier.ToString("0.00");
            balloonBlue_Start.SetActive(false);
            ballon_Anim.SetBool("isJump", true);
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

        if (countTime >= 7)
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
            TakeCashbutton.enabled = true;
            TakeCashImg.color = new Color32(255, 255, 255, 255);
            TakeCashtxt.color = new Color32(255, 255, 255, 255);
            takeCashWintxt.color = new Color32(255, 255, 255, 255);
            takeCurrencytxt.color = new Color32(255, 255, 255, 255);
            /*TakeCashAnimImg.SetActive(true);*/
            takeCashObj.SetActive(true);

            /*takecashOut.SetBool("isTake", true);*/
        }
        else
        {
            TakeCashbutton.enabled = false;
            TakeCashImg.color = new Color32(140, 140, 140, 255);
            TakeCashtxt.color = new Color32(140, 140, 140, 255);
            takeCashWintxt.color = new Color32(140, 140, 140, 255);
            takeCurrencytxt.color = new Color32(140, 140, 140, 255);
            /*TakeCashAnimImg.SetActive(false);*/
            takeCashObj.SetActive(false);

            /*takecashOut.SetBool("isTake", false);*/
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
    public void TakeCashOut()
    {
        #region ________ Internet Checking : 1 ________
        APIController.instance.CheckInternetandProcess((success) =>
        {
            if (success)
            {
                InternetCheck = true;

                if (!isWin)
                {
                    take = true;
                    startGame = false;
                    /*WinAmount = takeCash;*/
                    // Change the text color
                    multiplier = float.Parse(multiplier.ToString("0.00"));
                    multiplierTxt.text = multiplier.ToString();
                    //TotalAmount += WinAmount;
                    /*totalAmountTxt.text = TotalAmount.ToString("0.00");*/
                    holdButton.enabled = false;
                    /*   empty_holdButton.gameObject.SetActive(true);*/

                    TakeCashImg.color = new Color32(140, 140, 140, 255);
                    TakeCashtxt.color = new Color32(140, 140, 140, 255);
                    takeCashWintxt.color = new Color32(140, 140, 140, 255);
                    takeCurrencytxt.color = new Color32(140, 140, 140, 255);
                    /*TakeCashAnimImg.SetActive(false);*/
                    takeCashObj.SetActive(false);
                    // sliderOBjs
                    slider_bg.SetActive(false);
                    fillArea.SetActive(false);
                    slider_txt.SetActive(false);
                    sliderAutoCashNoTxt.gameObject.SetActive(false);
                    slider_Anim.SetBool("isOFF", true);
                    ballon_Anim.SetBool("isTake", true);

                    if (!winCount && betAmount <= 5f)
                    {
                        winCash++;
                    }
                    else
                    {
                        isNormal = true;
                    }

                    Debug.Log(" WinCashCheck : " + winCash);
                    Demo_Bonus();

                    if ((winCash_Demo < 10))
                    {
                        audioController.PlayAudio(AudioEnum.winGame);
                        winPanel.SetActive(true);
                        TakingCash();
                        Winning_Animations();
                    }

                    Call_Functions();
                    DelayFuction();
                    isWin = true;
                }

            }
            else
            {
                InternetCheck = false;
                Debug.Log("CheckInternetandProcess ============>  down" + success);
                return;
            }
        });

        if (!InternetCheck)
        {
            return;
        }
        #endregion
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
            Debug.Log("CheckInternetandProcess Betbtn");
            if (success)
            {
                Debug.Log("CheckInternetandProcess Betbtn Success");
                LocalInitializeBet();
            }
            else
            {
                Debug.Log("CheckInternetandProcess Failed");
                BetInputController.Instance.BetAmtInput.interactable = true;
            }
        });
    }
    void LocalInitializeBet()
    {
        Debug.Log("Entered LocalInitializeBet");

        //  APIController.instance.StartCheckInternetLoop();

        if (betAmount > TotalAmount)
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

            Debug.Log("LocalInitializeBet 2 " + betAmount);
            if (APIController.instance.userDetails.isBlockApiConnection)
            {
                InsufficientBalance.SetActive(true);
                InsufBal_Rumblebets.SetActive(false);
            }
            else
            {
                InsufficientBalance.SetActive(false);
                InsufBal_Rumblebets.SetActive(true);
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
        betAmount = float.Parse(m);
        Debug.Log("LocalInitializeBet Controller.BetButtonclik ");
        Debug.Log("BetAMount ********* " + betAmount + " " + " Balance ******* " + TotalAmount);

        List<string> _list = new List<string>();
        _list.Add(APIController.instance.userDetails.Id);
        Debug.Log("isBlockApiConnection " + APIController.instance.userDetails.isBlockApiConnection);

        if (!APIController.instance.userDetails.isBlockApiConnection)
        {
            //live
            CreateMatchAPICall();
        }
        else
        {
            //demo
            BetIndex = APIController.instance.InitlizeBet(betAmount, val, false, (success) =>
            {
                if (success)
                {
                    Debug.Log("Bet Initiated");
                    Debug.Log("Demo Mode");
                    startGame = true;
                    pauseGame = false;
                    isCreateMatchSucceess = true;

                }
                else
                {
                    Debug.Log("Bet Initiate Failed");
                    pauseGame = true;
                }
            }, APIController.instance.userDetails.Id, false);
        }
        #endregion
    }
    public void CreateMatchAPICall()    //CREATEMATCHAPICALL CALLING METHOD
    {
        Debug.Log("CreateMatchAPICalled========>");
        TransactionMetaData TransData = new();
        TransData.Amount = betAmount;
        TransData.Info = "InitBet";
        int _index = UnityEngine.Random.Range(100, 999);
        List<string> _list = new();
        _list.Add(APIController.instance.userDetails.Id);
        BetIndex = APIController.instance.CreateAndJoinMatch(_index, betAmount, TransData, false, lobbyName, APIController.instance.userDetails.Id,
            false, gameName, operatorName, APIController.instance.userDetails.gameId, APIController.instance.userDetails.isBlockApiConnection, _list, (success, newbetID, res) =>
            {
                if (success)
                {
                    startGame = true;
                    betID = newbetID.ToString();
                    MatchRes = res;
                    APIController.GetUpdatedBalance();
                    pauseGame = false;
                    isCreateMatchSucceess = true;
                    Debug.Log("CreateMatchAPICalled========>");
                }
                else
                {
                    pauseGame = true;
                    Debug.Log("CreateMatchAPIFailed========>");
                }
            });
    }
    async void API_Winning()
    {
        string message = "Game Won";
        string value = takeCash.ToString("F2");
        float amount = float.Parse(value);
        TransactionMetaData val = new TransactionMetaData();
        val.Amount = amount;
        val.Info = message;
        Debug.Log("WinningBetAPICalled========> 1");
        if (!APIController.instance.userDetails.isBlockApiConnection)
        {
            Debug.Log("isCreateMatchSucceess ====> " + isCreateMatchSucceess);
            while (!isCreateMatchSucceess)
            {
                await UniTask.Delay(100);
            }
            Debug.Log("isCreateMatchSucceess ====> success " + isCreateMatchSucceess);

            Debug.Log("WinningBetAPICalled========> 2");
            WinningBetAPICall(amount, takeCash);
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
    public void WinningBetAPICall(double WinAmount, double PotAmount)   //WINNINGBETAPI CALLING METHOD
    {
        Debug.Log("WinningBetAPICalled========> 3");
        TransactionMetaData _metaData = new TransactionMetaData();
        _metaData.Amount = WinAmount;
        _metaData.Info = "Game Won";
        APIController.instance.WinningsBetMultiplayerAPI(BetIndex, betID, WinAmount, betAmount, PotAmount, _metaData, (success) =>
        {
            Debug.Log("BetIndex value 0: " + BetIndex);
            Debug.Log("BetIndex value 1: " + betID);

            if (success)
            {
                Debug.Log("WinningBetAPICalled========> 4");
                APIController.GetUpdatedBalance();

            }
            else
            {
                Debug.Log("WinningBetAPICalled========> 5");
                Debug.Log("WinningBetAPIfailed========>");
            }
        }, APIController.instance.userDetails.Id, false, WinAmount == 0 ? false : true, gameName, operatorName, APIController.instance.userDetails.gameId, APIController.instance.userDetails.commission, MatchRes.MatchToken);
    }
    void Call_Functions()
    {
        if (winCash == winBonus)
        {
            isBonus_1 = true;
            if (!isSet)
                isBonus_2 = true;
            else if (isSet)
                isBonus_3 = true;
        }

        if (winCash != winBonus)
            isNormal = true;

    }
    async void DelayFuction()
    {
        if (isBonus_1)
        {
            if (winCash_Demo == 10)
            {
                await UniTask.Delay(1000);
                Bonus_Conditions();
            }

            await UniTask.Delay(3000);
            winPanel.SetActive(false);

            await UniTask.Delay(100);
            if (winCash_Demo < 10)
                Bonus_Conditions();
        }
        if (isBonus_2)
        {
            await UniTask.Delay(5500); // wait for 5 seconds
            TimeDelay_WinCash();
        }

        if (isNormal)
        {
            await UniTask.Delay(3500); // wait for 2.5 seconds
            Debug.Log("TimeDelay =========> 1");
            TimeDelay();
        }

        if (isBonus_3)
        {
            await UniTask.Delay(3000); // wait for 2.5 seconds
            Bonus_Delay();
        }
    }
    void TimeDelay()
    {
        multiplierTxt.color = Color.white;
        winTxt.color = Color.white;
        Xtxt.color = Color.white;
        multiplier = 0f;
        multiplierTxt.text = multiplier.ToString("0.00");
        takeCash = 0f;
        takeCashTxt.text = takeCash.ToString("0.00");
        /*winAmountTxt.gameObject.SetActive(false);*/
        /*WinAmount = 0;*/
        /*winAmountTxt.text = WinAmount.ToString("0.00");*/
        holdButton.enabled = true;
        timeSinceLastIncrement = 0f;
        timeHeld = 0f;
        gameLost = false;
        take = false;
        lost = false;
        isNormal = false;
        ballonOut.gameObject.SetActive(false);
        ballon_Anim.SetBool("isOut", false);
        background.localPosition = initialBackgroundPosition;
        bg.localPosition = iniBackgroundPos;
        heatbtnCollider.enabled = true;
        if (winCount == true)
        {
            winCount = false;
        }
        Button_Switch_ON();
        TakeCashImg.color = new Color32(140, 140, 140, 255);
        TakeCashtxt.color = new Color32(140, 140, 140, 255);
        takeCashWintxt.color = new Color32(140, 140, 140, 255);
        takeCurrencytxt.color = new Color32(140, 140, 140, 255);
        /*   heatButton_Anim.SetActive(true);*/
        heatButton_BG.color = new Color32(255, 255, 255, 255);
        heatTxt.color = new Color32(63, 15, 15, 255);
        /*TakeCashAnimImg.SetActive(false);*/
        takeCashObj.SetActive(false);
        isBegin = false;
        //minus_Anim.SetActive(true); plus_Anim.SetActive(true);

        winPanel.SetActive(false);
        takeBetAmount = true;
        isWin = false;
        countTime = 0f;
        slider.value = 0f;

        ButtonSelect_Anim();

        slider_Anim.SetBool("isOFF", false);
        slider_Anim.SetBool("isON", false);

        /*balloon.SetBool("isBalloon", false);*/
        ballon_Anim.enabled = true;
        ballon_Anim.SetBool("isTake", false);
        ballon_Anim.SetBool("isJump", false);

        balloonShake_blue.SetActive(false);
        balloonShake.SetActive(false);
        balloonParts.transform.localPosition = new Vector3(0f, -430.1323f, 0f);
        /*TxtObjs.transform.localPosition = new Vector3(-85f, 34f, 0f);*/
        TxtObjs.gameObject.SetActive(false);
        BalloonDelay();

        /*heat_Anim.SetBool("isStart", true);*/
        heat_Anim.SetBool("isPlay1", true);
        fireObj.SetActive(false);
        // sliderOBjs
        slider_Anim.SetBool("isON", true);
        Slider_Objs();
        TakeCashbutton.interactable = true;
    }
    void TimeDelay_WinCash()
    {
        multiplierTxt.color = Color.white;
        winTxt.color = Color.white;
        Xtxt.color = Color.white;
        multiplier = 0f;
        multiplierTxt.text = multiplier.ToString("0.00");
        takeCash = 0f;
        takeCashTxt.text = takeCash.ToString("0.00");
        holdButton.enabled = true;
        timeSinceLastIncrement = 0f;
        timeHeld = 0f;
        gameLost = false;
        take = false;
        lost = false;
        isBonus_2 = false;
        ballonOut.gameObject.SetActive(false);
        background.localPosition = initialBackgroundPosition;
        bg.localPosition = iniBackgroundPos;
        ButtonSelect_Anim();
        winPanel.SetActive(false);
        takeBetAmount = true;
        heatbtnCollider.enabled = true;
        Button_Switch_ON();
        countTime = 0f;
        slider.value = 0f;
        ballon_Anim.SetBool("isOut", false);
        TakeCashImg.color = new Color32(140, 140, 140, 255);
        TakeCashtxt.color = new Color32(140, 140, 140, 255);
        takeCashWintxt.color = new Color32(140, 140, 140, 255);
        takeCurrencytxt.color = new Color32(140, 140, 140, 255);
        /*   heatButton_Anim.SetActive(true);*/
        heatButton_BG.color = new Color32(255, 255, 255, 255);
        heatTxt.color = new Color32(63, 15, 15, 255);
        /*TakeCashAnimImg.SetActive(false);*/
        takeCashObj.SetActive(false);
        isWin = false;
        isBegin = false;
        //minus_Anim.SetActive(true); plus_Anim.SetActive(true);

        slider_Anim.SetBool("isOFF", false);
        slider_Anim.SetBool("isON", false);

        ballon_Anim.enabled = true;
        ballon_Anim.SetBool("isJump", false);
        ballon_Anim.SetBool("isTake", false);

        balloonShake_blue.SetActive(false);
        balloonShake.SetActive(false);

        balloonParts.transform.localPosition = new Vector3(0f, -430.1323f, 0f);
        /*TxtObjs.transform.localPosition = new Vector3(-85f, 34f, 0f);*/
        TxtObjs.gameObject.SetActive(false);
        BalloonDelay();

        /*heat_Anim.SetBool("isStart", true);*/
        heat_Anim.SetBool("isPlay1", true);
        fireObj.SetActive(false);

        // sliderOBjs
        slider_Anim.SetBool("isON", true);
        Slider_Objs();
        TakeCashbutton.interactable = true;
    }
    async void BalloonDelay()
    {
        await UniTask.Delay(100);
        Debug.Log(" Balloon Pos ============> 1 ");
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
    public async void TakingCash()
    {
        await UniTask.Delay(3000);
        API_Winning();
    }
    async void WinTxtObj()
    {
        await UniTask.Delay(2000);
        winTxt.transform.DORotate(new Vector3(0f, 360f, 0f), 0.5f, RotateMode.FastBeyond360).SetEase(Ease.InOutSine);
        if (APIController.instance.userDetails.isBlockApiConnection)
            winTxt.text = takeCash.ToString("0.00" + " <size=70>USD</size>");
        else
        {
            winTxt.text = takeCash.ToString("0.00" + " <size=70>EUR</size>");
        }
        winTxt.color = Color.green;
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
        takeCash = 0f;
        takeCashTxt.text = takeCash.ToString("0.00");
        isBonus_3 = false;
        take = false;
        ScrollViewer();
        Debug.Log(" Balloon Pos ============> 3 ");
    }
    async void ScrollViewer()
    {
        await UniTask.Delay(1000); // wait for 2.5 seconds
        ScrollView_Conditions();
    }
    void Bonus_Conditions()
    {
        if (winCash == winBonus)
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
        if (winCash == winBonus)
        {
            if (betAmount <= 5f)
            {
                if (betAmount <= 1.4f)
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
    bool InternetCheck;
    bool isAbleToPlay;
    public void OnClickDown()
    {
        #region
        APIController.instance.CheckInternetandProcess((success) =>
        {
            if (success && !InternetChecking.instance.connectionPanel.activeSelf)
            {
                InternetCheck = true;
                if (!startGame && !pauseGame && (TotalAmount < betAmount))
                {
                    buttonPress = true;
                    Debug.Log(" Bigger_Amount" + buttonPress);
                }

                if (!numPad && !buttonPress && !lost)
                {
                    Debug.Log(" Button ====> PressDown");
                    if (HandGestures_start.activeSelf)
                    {
                        HandGestures_start.SetActive(false);
                    }
                    isPressed = true;

                    audioController.StopAudio(AudioEnum.reverseSlider);
                    audioController.PlayAudio(AudioEnum.startSlider, true);
                    audioController.PlayAudio(AudioEnum.Movement, true);
                    balloonShake_blue.SetActive(false);
                    balloonParts.SetActive(false);
                    ApplyParallaxEffect(holdButton.GetComponent<RectTransform>().anchoredPosition.y);
                    touch = true;
                    heat_Anim.SetBool("isPlay2", true);
                    fireObj.SetActive(true);
                    //heat_Anim_Img.SetActive(true);

                    for (int i = 0; i < button_Anim.Length; i++)
                    {
                        button_Anim[i].SetActive(false);
                    }

                    TakeCashbutton.enabled = false;
                    //sliderTxt.text = null;
                    //sliderAutoCashNoTxt.gameObject.SetActive(false);

                    if (isFire && multiplier >= 1.01f)
                    {
                        sliderTxt.text = null;
                        sliderAutoCashNoTxt.gameObject.SetActive(false);
                    }
                    else if (!isFire && multiplier >= 1.01f)
                    {
                        sliderTxt.text = sliderAutoCashTxt.text.ToString();
                        sliderAutoCashNoTxt.gameObject.SetActive(true);
                    }

                }

                string s1 = TotalAmount.ToString("0.00");
                TotalAmount = float.Parse(s1);
                string s2 = betAmount.ToString("0.00");
                betAmount = float.Parse(s2);

                for (int i = 0; i < button_Anim.Length; i++)
                {
                    if (button_Anim[i].activeSelf)
                        button_Anim[i].SetActive(false);
                }

                if (!startGame && !gameLost && !isBegin)
                {
                    if (HandGestures_start.activeSelf)
                    {
                        HandGestures_start.SetActive(false);
                    }

                    if (!APIController.instance.userDetails.isBlockApiConnection && !buttonPress)
                        RNG_APICall();
                    else if (APIController.instance.userDetails.isBlockApiConnection && !buttonPress)
                    {
                        gameCounts++;

                        if (gameCounts != 15)
                            holdHeight = UnityEngine.Random.Range(float.Parse(0.80f.ToString("0.00")), float.Parse(9.8f.ToString("0.00")));
                        else if (gameCounts == 15)
                        {
                            holdHeight = UnityEngine.Random.Range(float.Parse(0.75f.ToString("0.00")), float.Parse(0.99f.ToString("0.00")));

                            if (gameCounts == 15)
                            {
                                gameCounts = 0;
                            }
                        }
                        //Debug.Log($"RNG Value Check:\n==============\n gameCounts : {gameCounts}\n maxHeight : {holdHeight}\n==============\n");
                    }
                    //minus_Anim.SetActive(false); plus_Anim.SetActive(false);
                    Debug.Log("buttonPress=== Count");
                    API_IntitalizeBetAmount();
                    isBegin = true;
                }
            }
            else
            {
                InternetCheck = false;
                Debug.Log("CheckInternetandProcess ============>  down" + success);
                return;
            }
        });

        if (!InternetCheck)
        {
            return;
        }
        #endregion
    }
    public void RNG_APICall()   //RNG_APICALL CALLING METHOD
    {
        APIController.instance.GetRNG_API(betAmount, operatorName, APIController.instance.userDetails.gameId, (_IsWin, _MaxWin, _gameCount) =>
        {
            makeLose = _IsWin;
            holdHeight = _MaxWin;
            if (_IsWin)
            {
                Debug.Log($"RNG Calculation:\n==============\n RNG Value Check : randomGamePlay \n MaxHeight : {_MaxWin}\n GameCount : {_gameCount}\n==============\n");
            }
            else
            {
                Debug.Log($"RNG Calculation:\n==============\n RNG Value Check : LoseThisGame \n MaxHeight : {_MaxWin}\n GameCount : {_gameCount}\n==============\n");
            }

        }, gameName, 0);
    }
    public void OnClickUp()
    {
        #region
        APIController.instance.CheckInternetandProcess((success) =>
        {
            if (success)
            {
                InternetCheck = true;

                Debug.Log(" Button ====> PressUP");
                isPressed = false;
                /*heat_Anim.SetBool("isEnd", false);*/
                heat_Anim.SetBool("isPlay2", false);
                fireObj.SetActive(false);
                /*heat_Anim_Img.SetActive(false);*/

                if (isFire && multiplier >= 1.01f)
                {
                    audioController.PlayAudio(AudioEnum.reverseSlider, true);
                    touch = false;
                }

                audioController.StopAudio(AudioEnum.startSlider);
                audioController.StopAudio(AudioEnum.Movement);
                if (!lost && !take && multiplier >= 1.01f)
                {
                    balloonShake_blue.SetActive(true);
                    balloonShake.SetActive(false);
                }

                Debug.Log("startGame " + startGame);
            }
            else
            {
                InternetCheck = false;
                Debug.Log("CheckInternetandProcess ============>  up" + success);
                return;
            }
        });

        if (!InternetCheck)
        {
            return;
        }
        #endregion
    }
    public void Button_ONEnter()
    {
        isFire = true;
    }
    public void Button_OFFEnter()
    {
        isFire = false;

        if (touch && multiplier >= 1.01f)
        {
            audioController.PlayAudio(AudioEnum.reverseSlider, true);
            touch = false;
        }
    }
    public void Welcom_Button()
    {
        fireObj.SetActive(false);
        StartCoroutine(Backgourn_Ballon_Fly());

        heatbtnCollider.enabled = false;
        ButtonSelect_Anim();
        HandGestures_btAmt.SetActive(true);
        // sliderOBjs
        slider_Anim.SetBool("isON", true);
        Slider_Objs();
        UI_Controller.instance.settingsHandler.Welcomebtn_OFF();
    }
    #endregion

    #region { ::::::::::::::::::::::::: Buttons ::::::::::::::::::::::::: }
    public Button settingsBtn;
    public void BetButton_1()
    {
        if (takeBetAmount)
            audioController.PlayAudio(AudioEnum.buttonClick);

        if (keyBoard.cancelButton.gameObject.activeSelf)
            keyBoard.OnCancelInput();

        if (!startGame && !take && !isScroll)
        {
            betAmount = 1f;
            betAmountTxt.text = betAmount.ToString("0.00 " + currencyType);
            BetAmountTxt_Scaling();
            button_1.gameObject.SetActive(true);
            button_2.gameObject.SetActive(false);
            button_5.gameObject.SetActive(false);
            button_10.gameObject.SetActive(false);
            AmountColor_Glow();
            for (int i = 0; i < button_Anim.Length; i++)
            {
                if (button_Anim[i].activeSelf)
                    button_Anim[i].SetActive(false);

                button_Anim[0].SetActive(false);
                button_Anim[1].SetActive(true);
                button_Anim[2].SetActive(true);
                button_Anim[3].SetActive(true);
            }

            plusButton.enabled = true;
            minusButton.enabled = true;
            plusButtomImg.color = new Color32(255, 255, 255, 255);
            minusButtonImg.color = new Color32(255, 255, 255, 255);


            winCash = 0;
            winCash_Demo = 0;
            bonusTimer = 0;
            timer = true;

            HandGesture();
        }
    }
    public void BetButton_2()
    {
        if (takeBetAmount)
            audioController.PlayAudio(AudioEnum.buttonClick);

        if (keyBoard.cancelButton.gameObject.activeSelf)
            keyBoard.OnCancelInput();

        if (!startGame && !take && !isScroll)
        {
            betAmount = 2f;
            betAmountTxt.text = betAmount.ToString("0.00 " + currencyType);
            BetAmountTxt_Scaling();
            button_1.gameObject.SetActive(false);
            button_2.gameObject.SetActive(true);
            button_5.gameObject.SetActive(false);
            button_10.gameObject.SetActive(false);
            AmountColor_Glow();
            for (int i = 0; i < button_Anim.Length; i++)
            {
                if (button_Anim[i].activeSelf)
                    button_Anim[i].SetActive(false);

                button_Anim[1].SetActive(false);
                button_Anim[0].SetActive(true);
                button_Anim[2].SetActive(true);
                button_Anim[3].SetActive(true);
            }
            AmountColor_Glow();
            plusButton.enabled = true;
            minusButton.enabled = true;
            plusButtomImg.color = new Color32(255, 255, 255, 255);
            minusButtonImg.color = new Color32(255, 255, 255, 255);
            winCash = 0;
            winCash_Demo = 0;
            bonusTimer = 0;
            timer = true;

            HandGesture();
        }
    }
    public void BetButton_5()
    {
        if (takeBetAmount)
            audioController.PlayAudio(AudioEnum.buttonClick);

        if (keyBoard.cancelButton.gameObject.activeSelf)
            keyBoard.OnCancelInput();

        if (!startGame && !take && !isScroll)
        {
            betAmount = 5f;
            betAmountTxt.text = betAmount.ToString("0.00 " + currencyType);
            BetAmountTxt_Scaling();
            button_1.gameObject.SetActive(false);
            button_2.gameObject.SetActive(false);
            button_5.gameObject.SetActive(true);
            button_10.gameObject.SetActive(false);

            for (int i = 0; i < button_Anim.Length; i++)
            {
                if (button_Anim[i].activeSelf)
                    button_Anim[i].SetActive(false);

                button_Anim[2].SetActive(false);
                button_Anim[0].SetActive(true);
                button_Anim[1].SetActive(true);
                button_Anim[3].SetActive(true);
            }
            AmountColor_Glow();
            plusButton.enabled = true;
            minusButton.enabled = true;
            plusButtomImg.color = new Color32(255, 255, 255, 255);
            minusButtonImg.color = new Color32(255, 255, 255, 255);

            winCash = 0;
            winCash_Demo = 0;
            bonusTimer = 0;
            timer = true;

            HandGesture();
        }
    }
    public void BetButton_10()
    {
        if (takeBetAmount)
            audioController.PlayAudio(AudioEnum.buttonClick);

        if (keyBoard.cancelButton.gameObject.activeSelf)
            keyBoard.OnCancelInput();

        if (!startGame && !take && !isScroll)
        {
            betAmount = 10f;
            betAmountTxt.text = betAmount.ToString("0.00 " + currencyType);
            BetAmountTxt_Scaling();
            button_1.gameObject.SetActive(false);
            button_2.gameObject.SetActive(false);
            button_5.gameObject.SetActive(false);
            button_10.gameObject.SetActive(true);

            for (int i = 0; i < button_Anim.Length; i++)
            {
                if (button_Anim[i].activeSelf)
                    button_Anim[i].SetActive(false);

                button_Anim[3].SetActive(false);
                button_Anim[0].SetActive(true);
                button_Anim[1].SetActive(true);
                button_Anim[2].SetActive(true);
            }
            AmountColor_Glow();
            plusButton.enabled = true;
            minusButton.enabled = true;
            plusButtomImg.color = new Color32(255, 255, 255, 255);
            minusButtonImg.color = new Color32(255, 255, 255, 255);

            winCash = 0;
            winCash_Demo = 0;
            bonusTimer = 0;
            timer = true;

            if (HandGestures_btAmt.activeSelf)
            {
                HandGestures_btAmt.SetActive(false);
                HandGestures_start.SetActive(true);
                heatbtnCollider.enabled = true;
                heat_Anim.SetBool("isPlay1", true);
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

            Debug.Log("SelectBetButton_1 ==> 2");

            BetInputController.Instance.BetPanel.gameObject.SetActive(false);
            KeyBoardHandler.instance.cancelButton.gameObject.SetActive(false);
            betAmount = s;
            betAmountTxt.text = betAmount.ToString("0.00 " + currencyType);
            betAmountTxt.gameObject.SetActive(true);
            return;
        }

        if (!startGame && !take && !isScroll)
        {
            if (betAmount < 100f)
            {
                betAmount += s;
                betAmountTxt.text = betAmount.ToString("0.00 " + currencyType);
                BetAmountTxt_Scaling();
                plusButton.enabled = true;
                minusButton.enabled = true;
                plusButtomImg.color = new Color32(255, 255, 255, 255);
                minusButtonImg.color = new Color32(255, 255, 255, 255);
                AmountColor_Glow();
            }
            if (betAmount >= 100)
            {
                betAmount = 100f;
                betAmountTxt.text = betAmount.ToString("0.00 " + currencyType);
                plusButton.enabled = false;
                plusButtomImg.color = new Color32(255, 255, 255, 120);
                MaxBet_Object();
            }

            winCash = 0;
            winCash_Demo = 0;
            bonusTimer = 0;
            timer = true;
        }
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

        //minus_Anim.SetActive(false); plus_Anim.SetActive(false);

        if (!startGame && !take && !isScroll)
        {
            if (betAmount < 100f)
            {
                betAmount += 0.10f;
                betAmountTxt.text = betAmount.ToString("0.00 " + currencyType);
                BetAmountTxt_Scaling();
                minusButton.enabled = true;
                minusButtonImg.color = new Color32(255, 255, 255, 255);
                AmountColor_Glow();
            }
            if (betAmount > 99.90f)
            {
                betAmount = 100f;
                plusButton.enabled = false;
                betAmountTxt.text = betAmount.ToString("0.00 " + currencyType);
                BetAmountTxt_Scaling();
                plusButtomImg.color = new Color32(255, 255, 255, 100);
                MaxBet_Object();
            }

            if (betAmount != 100f)
            {
                winCash = 0;
                winCash_Demo = 0;
                bonusTimer = 0;
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

        //minus_Anim.SetActive(false); plus_Anim.SetActive(false);

        if (!startGame && !take && !isScroll)
        {
            if (betAmount > 0.10f)
            {
                betAmount -= 0.10f;
                betAmountTxt.text = betAmount.ToString("0.00 " + currencyType);
                BetAmountTxt_Scaling();
                plusButtomImg.color = new Color32(255, 255, 255, 255);
                AmountColor_Glow();
            }
            if (betAmount <= 0.20f)
            {
                betAmount = 0.10f;
                betAmountTxt.text = betAmount.ToString("0.00 " + currencyType);
                BetAmountTxt_Scaling();
                minusButton.enabled = false;
                minusButtonImg.color = new Color32(255, 255, 255, 100);
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
    public void BetAmountTxt_Scaling()
    {
        // DoTween Sequence
        Sequence sequence = DOTween.Sequence();
        sequence.Append(betAmountTxt.transform.DOScale(new Vector3(0.8f, 0.8f, 0.8f), 0.2f).SetEase(Ease.InSine));
        sequence.AppendInterval(0.02f);
        sequence.Append(betAmountTxt.transform.DOScale(new Vector3(1f, 1f, 1f), 0.2f).SetEase(Ease.InOutSine));
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
        if (!takeBetAmount)
        {
            numPadButton.gameObject.SetActive(false);
            inputField.raycastTarget = false;
        }
        else
        {
            numPadButton.gameObject.SetActive(true);
            inputField.raycastTarget = true;
        }

        if (betAmount <= 0.10f)
        {
            minusButton.enabled = false;
            minusButtonImg.color = new Color32(255, 255, 255, 100);
        }
        else if (betAmount > 0.10f)
        {
            minusButton.enabled = true;
            minusButtonImg.color = new Color32(255, 255, 255, 255);
        }

        if (betAmount < 100f)
        {
            plusButton.enabled = true;
            plusButtomImg.color = new Color32(255, 255, 255, 255);
        }
        else if (betAmount >= 100f)
        {
            plusButton.enabled = false;
            plusButtomImg.color = new Color32(255, 255, 255, 100);
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
    public void HandGesture()
    {
        if (HandGestures_btAmt.activeSelf)
        {
            HandGestures_btAmt.SetActive(false);
            HandGestures_start.SetActive(true);
            heatbtnCollider.enabled = true;
            heat_Anim.SetBool("isPlay1", true);
        }
    }
    public void Insufficient_OFF()
    {
        buttonPress = false;
        isBegin = false;
        Debug.Log(" Bigger_Amount" + buttonPress);
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