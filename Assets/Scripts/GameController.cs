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
    public float betAmount = 1f;  // Initial bet amount
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

    [SerializeField] GameObject[] button_Anim;

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
    private bool startGame;
    private bool isPressed;
    public bool takeBetAmount;
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
    /*[SerializeField] TextMeshProUGUI winAmountTxt;*/
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
    [SerializeField] Image targetImg;
    [SerializeField] RectTransform target;
    [SerializeField] RectTransform bonus_Balloon;
    /*[SerializeField] Vector3 targetPosition;  // To move the Target pos Up at the Start*/

    [SerializeField] GameObject balloonJumpObj;
    [SerializeField] GameObject balloonDefaultObj;
    [SerializeField] GameObject balloonAnimObj;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    // Sliders
    [Header("Sliders")]
    [SerializeField] Slider slider;
    [SerializeField] float countTime;
    [SerializeField] GameObject slider_bg;
    [SerializeField] GameObject fillArea;
    [SerializeField] GameObject slider_txt;
    private bool timeout;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    // DoTween Text
    [Header("DoTween Text")]
    [SerializeField] RectTransform txtObj;
    [SerializeField] float timeLength = 1f;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    // DoTween Win_Images 
    [Header("DoTween Win_Images")]
    [SerializeField] GameObject winPanel;
    [SerializeField] RectTransform winObj;
    [SerializeField] Image winImg;
    [SerializeField] TextMeshProUGUI winTxt;
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
    // Heat button
    [SerializeField] Animator heat_Anim;
    [SerializeField] GameObject heat_Anim_Img;

    // slider
    [SerializeField] Animator slider_Anim;

    // fire in balloon
    [SerializeField] Animator fire_Anim;
    [SerializeField] Animator blueFire_Anim;

    // balloon flying in air
    [SerializeField] Animator balloon;
    [SerializeField] Animator balloon_Air;

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


    #endregion ::::::::::::::::::::::::: END :::::::::::::::::::::::::
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------//
    private void Start()
    {
        takeBetAmount = true;
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

        // sliderOBjs
        slider_bg.SetActive(false);
        fillArea.SetActive(false);
        slider_txt.SetActive(false);

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
        //PassTxt(Controller.BalanceAmountTxt, $"{TotalAmount:F2}" /*+ APIController.instance.userDetails.currency_type*/);
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
                /*slider.gameObject.SetActive(false);*/
            }

            yield return null;
        }
    }
    IEnumerator HolidngButtons()
    {
        while (true)
        {
            //heat_Anim.SetTrigger("isEnd");
            InSufficientBalance();
            NumberPad();

            if (isScroll)
            {
                ApplyBalloonParallaxEffect(holdButton.GetComponent<RectTransform>().anchoredPosition.y);
                ApplyParallaxEffect(holdButton.GetComponent<RectTransform>().anchoredPosition.y);
                balloon_Air.SetBool("inAir", false);
                balloonAnimObj.SetActive(false);
                heat_Anim.SetBool("isStart", false);
                TakeCashbutton.enabled = false;
            }
            else
            {
                balloonAnimObj.SetActive(true);
            }

            if (lost)
            {
                target.anchoredPosition += Vector2.up * speed * Time.deltaTime;
                holdButtonEvent.enabled = false;
                empty_holdButton.gameObject.SetActive(true);
                // sliderOBjs
                slider_Anim.SetBool("isOFF", true);
                slider_bg.SetActive(false);
                fillArea.SetActive(false);
                slider_txt.SetActive(false);
                heat_Anim.SetBool("isStart", false);

                balloon_Air.SetBool("inAir", false);
            }
            if (take)
            {
                target.anchoredPosition += Vector2.up * speed * Time.deltaTime;
                // sliderOBjs
                slider_Anim.SetBool("isOFF", true);
                slider_bg.SetActive(false);
                fillArea.SetActive(false);
                slider_txt.SetActive(false);
                heat_Anim.SetBool("isStart", false);

                balloon_Air.SetBool("inAir", false);
            }
            FireButton();
            StartOfTheGame();

            if (isPressed && isFire && !lost)
            {
                // To move the Target pos Up at the Start
                /*target.anchoredPosition = Vector3.Lerp(target.anchoredPosition, targetPosition, 0.2f * Time.deltaTime);*/
                ApplyParallaxEffect(holdButton.GetComponent<RectTransform>().anchoredPosition.y);
                fire_Anim.SetBool("isFire", true);
                blueFire_Anim.SetBool("isBlue", false);

                startGame = true;

                if (timeSinceLastIncrement >= incrementInterval)
                {
                    IncrementMultiplier();
                    timeSinceLastIncrement = 0f; // Reset the timer
                }

                if (multiplier >= 1.01)
                {
                    balloon.SetBool("isBalloon", false);

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

                        /*fire_Anim.SetBool("isFire", true);*/
                        balloon.SetBool("isBalloon", false);

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
    void NumberPad()
    {
        if (numPad)
        {
            button_1.gameObject.SetActive(false);
            button_2.gameObject.SetActive(false);
            button_5.gameObject.SetActive(false);
            button_10.gameObject.SetActive(false);
            numPad = false;
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
            /*holdButton.enabled = true;*/
            takeCash = betAmount * multiplier;
            takeCashTxt.text = takeCash.ToString("0.00");
        }
        if (multiplier >= 1.01 && !take)
        {
            if (!isScroll)
                TakeCashbutton.enabled = true;
        }

        if (startGame && multiplier < 1.01f)
        {
            multiplier += Time.deltaTime;
            multiplierTxt.text = multiplier.ToString("0.00");

            // sliderOBjs
            slider_Anim.SetBool("isON", true);
            Slider_Objs();

            //balloon objs
            balloonDefaultObj.SetActive(false);
            balloonJumpObj.SetActive(true);
            balloon.SetBool("isBalloon", true);
            balloon_Objs();
        }
        else if (isPressed && startGame && multiplier >= 1.01)
        {
            balloon_Air.SetBool("inAir", true);
            /*ApplyParallaxEffect(holdButton.GetComponent<RectTransform>().anchoredPosition.y);*/
            /*ApplyBalloonParallaxEffect(holdButton.GetComponent<RectTransform>().anchoredPosition.y);*/
            multiplierTxt.text = multiplier.ToString("0.00");
            //holdButtonEvent.enabled = true;
            takeCash = betAmount * multiplier;
            takeCashTxt.text = takeCash.ToString("0.00");

            // Increment the timer by the time elapsed since the last frame
            timeSinceLastIncrement += Time.deltaTime;
        }

        if (startGame && takeBetAmount)
        {
            takeBetAmount = false;
            holdButton.enabled = false;
            API_IntitalizeBetAmount();
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
        TotalAmount += WinAmount;
        totalAmountTxt.text = TotalAmount.ToString("0.00" + " <size=30>EUR</size>");
        holdButton.enabled = false;
        winPanel.SetActive(true);
        holdButtonEvent.enabled = false;
        /*slider.gameObject.SetActive(false);*/
        empty_holdButton.gameObject.SetActive(true);

        // sliderOBjs
        slider_bg.SetActive(false);
        fillArea.SetActive(false);
        slider_txt.SetActive(false);
        slider_Anim.SetBool("isOFF", true);

        API_Winning();

        #region :::::::::::::::::::::::::: DoTween sequence ::::::::::::::::::::::::::

        Winning_Animations();

        #region
        /*// DoTween Text in Sequence
        Sequence sequence = DOTween.Sequence();

        sequence.AppendInterval(0.01f);

        // Add the first scale animation and move animation to the sequence
        sequence.Append(txtObj.DOScale(new Vector3(1.7f, 1.7f, 1.7f), timeLength).SetEase(Ease.InOutSine))
                .Join(txtObj.DOMove(new Vector3(0f, -2.8f, 0f), 1.3f).SetEase(Ease.InOutSine));
        //.Join(LineImg.DOColor(new Color32(255, 255, 255, 255), timeLength).SetEase(Ease.Linear));

        // Add a delay of 1 second
        sequence.AppendInterval(0.01f);

        // Add the second scale animation to the sequence
        sequence.Append(txtObj.DOScale(new Vector3(0f, 0f, 0f), timeLength))
                //.Join(txtObj.DOMove(new Vector3(0f, -2.2f, 0f), timeLength).SetEase(Ease.InOutSine))
                .Join(txtObj.DOMove(new Vector3(0f, -3f, 0f), timeLength).SetEase(Ease.InOutSine));  // new Position
        //.Join(LineImg.DOColor(new Color32(255, 255, 255, 0), timeLength).SetEase(Ease.Linear));

        // Start the sequence
        sequence.Play();*/
        #endregion

        #endregion  :::::::::::::::::::::::::: END ::::::::::::::::::::::::::

        if (!winCount && betAmount <= 5f)
        {
            winCash++;
        }
        else
        {
            isNormal = true;
        }


        balloon.SetBool("isBalloon", false);
        takecashOut.SetBool("isTake", false);

        Demo_Bonus();
        Call_Functions();
        DelayFuction();
    }
    void API_IntitalizeBetAmount()
    {
        APIController.instance.CheckInternetandProcess((success) =>
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
                //Controller.Interactble_Or_UninteractableBtn(startGame, true);
                //ChechkAmountBtns(true);
                //HideOrShowObject(Controller.BetPlacedimg.gameObject, false);
                takeBetAmount = false;
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

        string m = TotalAmount.ToString("0.00");
        /*betAmountTxt = float.Parse(m);*/


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
        //   Controller.BetIndex = APIController.instance.InitlizeBet((Controller.BetAmount), val);
        BetInputController.Instance.CloseKeyPadPanel();
        BetInputController.Instance.BetAmtInput.interactable = false;


        string _s = betAmount.ToString("0.00");
        float amount = float.Parse(_s);

        TransactionMetaData val = new TransactionMetaData();
        val.Amount = amount;
        val.Info = message;

        Debug.Log("Checking Bet Amount 1 : " + amount + "........" + val.Amount);

        List<string> _list = new List<string>();
        _list.Add(APIController.instance.userDetails.Id);
        //APIController.instance.AddPlayers(MatchRes.MatchToken, _list);
        if (!APIController.instance.userDetails.isBlockApiConnection)
        {
            Debug.Log("isBlockApiConnection " + APIController.instance.userDetails.isBlockApiConnection);

            BetIndex = APIController.instance.CreateAndJoinMatch(_index, amount, val, false, lobbyName, APIController.instance.userDetails.Id, false, gameName, operatorName, APIController.instance.userDetails.gameId, APIController.instance.userDetails.isBlockApiConnection, _list, (success, newbetID, res) =>
            {
                if (success)
                {
                    betID = newbetID.ToString();
                    MatchRes = res;
                    Debug.Log("Bet Initiated");
                    APIController.GetUpdatedBalance();

                    /*Debug.Log("Setting_index_FromAPI ");
                    WrongBtnSetter_ByIndex.Instance.Setting_index_FromAPI();
                    Debug.Log("Bet Completed");*/

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
                    Debug.Log("Bet Completed");
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
        string value = WinAmount.ToString("F2");
        TransactionMetaData val = new TransactionMetaData();
        float amount = float.Parse(value);

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
        /*else
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
        }*/
        //     Controller.ShowingAll(Controller.AllController[Controller.DropDown.value].Board);
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
        background.localPosition = initialBackgroundPosition;
        bg.localPosition = iniBackgroundPos;

        if (winCount == true)
        {
            targetImg.enabled = true;
            bonus_Balloon.gameObject.SetActive(false);
            winCount = false;
        }

        /*target.localPosition = new Vector3(0f, 202f, 0f);*/
        target.localPosition = new Vector3(0f, 33f, 0f);
        winPanel.SetActive(false);
        takeBetAmount = true;

        countTime = 0f;
        slider.value = 0f;
        /*slider.gameObject.SetActive(true);*/

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

        balloon.SetBool("isBalloon", false);
        balloonDefaultObj.SetActive(true);
        target.gameObject.SetActive(false);
        balloon_Air.SetBool("inAir", false);

        heat_Anim.SetBool("isStart", true);
        for (int i = 0; i < button_Anim.Length; i++)
        {
            button_Anim[i].SetActive(true);
        }
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
        background.localPosition = initialBackgroundPosition;
        bg.localPosition = iniBackgroundPos;

        /*target.localPosition = new Vector3(0f, -152f, 0f);*/
        target.localPosition = new Vector3(0f, 33f, 0f);
        winPanel.SetActive(false);
        takeBetAmount = true;

        countTime = 0f;
        slider.value = 0f;
        /*slider.gameObject.SetActive(true);*/

        // DoTween Text
        /*txtObj.DOMove(new Vector3(0f, 0.27f, 0f), 0.01f);*/
        //txtObj.DOMove(new Vector3(0f, 1.7f, 0f), 0.01f);// new Position

        button_1.gameObject.SetActive(false);
        button_2.gameObject.SetActive(false);
        button_5.gameObject.SetActive(false);
        button_10.gameObject.SetActive(false);

        slider_Anim.SetBool("isOFF", false);
        slider_Anim.SetBool("isON", false);

        balloon.SetBool("isBalloon", false);
        balloonDefaultObj.SetActive(true);
        target.gameObject.SetActive(false);
        balloon_Air.SetBool("inAir", false);

        heat_Anim.SetBool("isStart", true);
        for (int i = 0; i < button_Anim.Length; i++)
        {
            button_Anim[i].SetActive(true);
        }

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
        if (TotalAmount < 1f)
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
        sequence.Append(winObj.DOScale(new Vector3(1.5f, 1.5f, 1.5f), timeLength).SetEase(Ease.InOutSine))
                .Join(winImg.DOColor(new Color32(255, 255, 255, 255), timeLength).SetEase(Ease.Linear));

        // Add a delay of 1 second
        sequence.AppendInterval(0.01f);

        // Add the second scale animation to the sequence
        sequence.Append(winObj.DOScale(new Vector3(1f, 1f, 1f), timeLength).SetEase(Ease.InOutSine));

        // Add a delay of 1 second
        sequence.AppendInterval(0.5f);
        sequence.Append(winObj.DOScale(new Vector3(0f, 0f, 0f), timeLength).SetEase(Ease.InOutSine))
                .Join(winImg.DOColor(new Color32(255, 255, 255, 0), timeLength).SetEase(Ease.Linear));

        // Start the sequence
        sequence.Play();
    }
    async void WinTxtObj()
    {
        await UniTask.Delay(1000);
        winTxt.transform.DORotate(new Vector3(0f, 360f, 0f), 0.5f, RotateMode.FastBeyond360).SetEase(Ease.InOutSine);
        winTxt.text = takeCash.ToString("0.00" + " <size=70>EUR</size>");
        winTxt.color = Color.green;
    }
    async void balloon_Objs()
    {
        await UniTask.Delay(1010);
        balloonJumpObj.SetActive(false);
        target.gameObject.SetActive(true);
        /*balloon_Air.SetBool("inAir", true);*/
    }
    async void Slider_Objs()
    {
        await UniTask.Delay(200);
        slider_bg.SetActive(true);
        fillArea.SetActive(true);
        slider_txt.SetActive(true);
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
        /*winAmountTxt.gameObject.SetActive(false);*/
        WinAmount = 0;
        /*winAmountTxt.text = WinAmount.ToString("0.00");*/
        /*winObj.gameObject.SetActive(false);*/
        isBonus_3 = false;
        take = false;

        target.localPosition = new Vector3(0f, 0f, 0f);
        targetImg.enabled = false;
        /*bonus_Balloon.localPosition = new Vector3(0f, -130f, 0f);*/
        bonus_Balloon.gameObject.SetActive(true);
        // DoTween Text
        //txtObj.DOScale(new Vector3(1f, 1f, 1f), 0.01f);
        //txtObj.DOMove(new Vector3(0f, 1f, 0f), 0.01f);
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
        if (winCash == 3)
        {
            bonusObj.gameObject.SetActive(true);
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
    /*public void HoldButton()
    {
        Debug.Log("&&&&&&&");
        takeBetAmount = true;
        betAmountTxt.text = betAmount.ToString("0.00" + " <size=30>EUR</size>");
    }*/
    public void OnClickDown()
    {
        isPressed = true;
        ApplyParallaxEffect(holdButton.GetComponent<RectTransform>().anchoredPosition.y);

        heat_Anim.SetBool("isEnd", true);
        heat_Anim_Img.SetActive(true);

        takecashOut.SetBool("isTake", true);

        for (int i = 0; i < button_Anim.Length; i++)
        {
            button_Anim[i].SetActive(false);
        }
    }
    public void OnClickUp()
    {
        isPressed = false;
        /*ApplyParallaxEffect(holdButton.GetComponent<RectTransform>().anchoredPosition.y);*/
        /*heat_Anim.SetTrigger("isEnd");*/
        heat_Anim.SetBool("isEnd", false);
        heat_Anim_Img.SetActive(false);

        fire_Anim.SetBool("isFire", false);
        blueFire_Anim.SetBool("isBlue", true);

        balloon_Air.SetBool("inAir", false);
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
        blueFire_Anim.SetBool("isBlue", false);
        heat_Anim.SetBool("isStart", true);
        StartCoroutine(Backgourn_Ballon_Fly());

        for (int i = 0; i < button_Anim.Length; i++)
        {
            button_Anim[i].SetActive(true);
        }
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
        if (!startGame && !take && !isScroll)
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
        if (!startGame && !take && !isScroll)
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
        if (!startGame && !take && !isScroll)
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
        if (!startGame && !take && !isScroll)
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
        if (!startGame && !take && !isScroll)
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

            winCash = 0;
            winCash_Demo = 0;
            bonusTimer = 0;
            timer = true;
        }
    }
    public void SelectBetButton_2()
    {
        if (!startGame && !take && !isScroll)
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

            winCash = 0;
            winCash_Demo = 0;
            bonusTimer = 0;
            timer = true;
        }
    }
    public void SelectBetButton_5()
    {
        if (!startGame && !take && !isScroll)
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

            winCash = 0;
            winCash_Demo = 0;
            bonusTimer = 0;
            timer = true;
        }
    }
    public void SelectBetButton_10()
    {
        if (!startGame && !take && !isScroll)
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

            winCash = 0;
            winCash_Demo = 0;
            bonusTimer = 0;
            timer = true;
        }
    }
    public void PlusButton()
    {
        if (!startGame && !take && !isScroll)
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
        if (!startGame && !take && !isScroll)
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