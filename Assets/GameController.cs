using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class GameController : MonoBehaviour
{
    #region { ::::::::::::::::::::::::: Headers ::::::::::::::::::::::::: }
    [SerializeField] float betAmount = 5f;  // Initial bet amount
    [SerializeField] float multiplier = 0.00f;  // Initial multiplier value
    [SerializeField] float incrementRate = 1.01f;  // Increment rate
    [SerializeField] float incrementInterval = 0.2f;  // Time interval between increments in seconds
    [SerializeField] float timeSinceLastIncrement = 0f;   // Timer to track time since last increment
    [SerializeField] float maxHoldTime = 5f; // Maximum time to hold the button
    [SerializeField] float minHoldTime = 2f; // Minimum time to hold the button
    private float timeHeld = 0f;   // Timer to track time button is held
    [SerializeField] float takeCash;  // TakeCash
    private float WinAmount = 0;
    [SerializeField] float TotalAmount = 250.00f;  // Total Amount

    [SerializeField] Button TakeCashbutton;
    [SerializeField] Button holdButton;
    //UI bet Buttons
    [SerializeField] Button button_5, button_25, button_50, button_100;

    [SerializeField] EventTrigger holdButtonEvent;

    // Boolean
    public bool startGame;
    public bool isPressed;
    private bool isOnMouse;
    private bool takeBetAmount;
    private bool lost;
    private bool take;

    // TextMeshProUGUI
    [SerializeField] TextMeshProUGUI multiplierTxt;
    [SerializeField] TextMeshProUGUI takeCashTxt;
    [SerializeField] TextMeshProUGUI winAmountTxt;
    [SerializeField] TextMeshProUGUI totalAmountTxt;
    // UI Bet Amount txt
    [SerializeField] TextMeshProUGUI betAmountTxt;

    // UI background Image
    [SerializeField] RectTransform background;
    [SerializeField] float parallaxAmount = 2000f;
    [SerializeField] float smoothness = 0.05f; // Adjust this value to control smoothness
    private Vector3 initialBackgroundPosition;

    // Balloon Image
    [SerializeField] float speed = 1.0f;
    [SerializeField] RectTransform target;

    // Sliders
    [SerializeField] Slider slider;
    [SerializeField] float countTime;
    private bool timeout;

    // DoTween Text
    [SerializeField] RectTransform txtObj;
    [SerializeField] float timeLength = 1f;

    // DoTween Win_Images 
    [SerializeField] RectTransform LineObj;
    [SerializeField] Image LineImg;
    [SerializeField] float timeTween = 1f;
    #endregion
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------//
    private void Start()
    {
        // Set the initial multiplier text
        multiplierTxt.text = multiplier.ToString("0.00" + "<size=40>X</size>");
        StartCoroutine(HolidngButtons());
        totalAmountTxt.text = TotalAmount.ToString("0.00" + " <size=30>EUR</size>");
        TakeCashbutton.enabled = false;
        TakeCashbutton.onClick.AddListener(TakeCashOut);
        initialBackgroundPosition = background.localPosition;

        // Sliders
        timeout = false;
        slider.maxValue = 6f;
        slider.minValue = 0f;
        StartCoroutine(TimerCount());
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
            if (lost)
            {
                target.anchoredPosition += Vector2.up * speed * Time.deltaTime;
            }
            if (take)
            {
                target.anchoredPosition += Vector2.up * speed * Time.deltaTime;
            }

            StartOfTheGame();

            if (isPressed)
            {
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
                        TakeCashbutton.enabled = false;
                        timeSinceLastIncrement = 0f; // Reset the timer
                        timeHeld = 0f; // Reset the time button is held
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
        ApplyParallaxEffect(holdButton.GetComponent<RectTransform>().anchoredPosition.y);
    }
    void StartOfTheGame()
    {
        if (multiplier >= 1.01 && !lost)
        {
            holdButton.enabled = true;
            TakeCashbutton.enabled = true;
            takeCash = betAmount * multiplier;
            takeCashTxt.text = takeCash.ToString("0.00");
        }

        if (startGame && multiplier < 1.01f)
        {
            multiplier += Time.deltaTime;
            multiplierTxt.text = multiplier.ToString("0.00" + "<size=40>X</size>");
            /*isPressed = false;*/
            /*holdButtonEvent.enabled = false;*/
        }
        else if (isPressed && startGame && multiplier >= 1.01)
        {
            ApplyParallaxEffect(holdButton.GetComponent<RectTransform>().anchoredPosition.y);
            multiplierTxt.text = multiplier.ToString("0.00" + "<size=40>X</size>");
            //holdButtonEvent.enabled = true;
            takeCash = betAmount * multiplier;
            takeCashTxt.text = takeCash.ToString("0.00");

            /*holdButtonEvent.enabled = true;*/

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
            multiplierTxt.text = multiplier.ToString("0.00" + "<size=40>X</size>");

            // Calculate the win amount
            takeCash = betAmount * multiplier;
            // Update the take cash text
            takeCashTxt.text = takeCash.ToString("0.00");
        }
    }
    void TakeCashOut()
    {
        take = true;
        startGame = false;
        WinAmount = takeCash;
        // Change the text color
        multiplierTxt.text = multiplier.ToString("0.00" + "<size=40>X</size>");
        multiplierTxt.color = Color.blue;
        winAmountTxt.gameObject.SetActive(true);
        winAmountTxt.text = WinAmount.ToString("+" + "0.00");
        TotalAmount += WinAmount;
        totalAmountTxt.text = TotalAmount.ToString("0.00" + " <size=30>EUR</size>");
        TakeCashbutton.enabled = false;
        holdButton.enabled = false;
        LineObj.gameObject.SetActive(true);
        holdButtonEvent.enabled = false;

        // DoTween Text in Sequence
        Sequence sequence = DOTween.Sequence();

        sequence.AppendInterval(0.01f);

        // Add the first scale animation and move animation to the sequence
        sequence.Append(txtObj.DOScale(new Vector3(1.7f, 1.7f, 1.7f), timeLength).SetEase(Ease.InOutSine))
                .Join(txtObj.DOMove(new Vector3(0f, -1.5f, 0f), timeLength).SetEase(Ease.InOutSine))
                .Join(LineImg.DOColor(new Color32(255, 255, 255, 255), timeLength).SetEase(Ease.Linear));

        // Add a delay of 1 second
        sequence.AppendInterval(0.01f);

        // Add the second scale animation to the sequence
        sequence.Append(txtObj.DOScale(new Vector3(0f, 0f, 0f), timeLength))
                .Join(LineImg.DOColor(new Color32(255, 255, 255, 0), timeLength).SetEase(Ease.Linear));
        // Start the sequence
        sequence.Play();

        Invoke("TimeDelay", 2.5f);
    }
    void TimeDelay()
    {
        multiplierTxt.color = Color.white;
        multiplier = 0f;
        multiplierTxt.text = multiplier.ToString("0.00" + "<size=40>X</size>");
        takeCash = 0f;
        takeCashTxt.text = takeCash.ToString("0.00");
        winAmountTxt.gameObject.SetActive(false);
        WinAmount = 0;
        winAmountTxt.text = WinAmount.ToString("0.00");
        holdButton.enabled = true;
        holdButtonEvent.enabled = true;
        take = false;
        lost = false;
        background.localPosition = initialBackgroundPosition;

        target.localPosition = new Vector3(0f, -37f, 0f);
        LineObj.gameObject.SetActive(false);

        countTime = 0f;
        slider.value = 0f;
        slider.gameObject.SetActive(true);

        // DoTween Text
        txtObj.DOScale(new Vector3(1f, 1f, 1f), 0.01f);
        txtObj.DOMove(new Vector3(0f, 0f, 0f), 0.01f);

        button_5.gameObject.SetActive(false);
        button_25.gameObject.SetActive(false);
        button_50.gameObject.SetActive(false);
        button_100.gameObject.SetActive(false);
    }
    void ResetBets()
    {
        isPressed = false;
        holdButtonEvent.enabled = false;
        startGame = false;
        multiplierTxt.text = multiplier.ToString("0.00" + "<size=40>X</size>");
        // Change the text color
        multiplierTxt.color = Color.black;

        Invoke("TimeDelay", 1.5f);
    }

    #region { ::::::::::::::::::::::::: Buttons ::::::::::::::::::::::::: }
    public void BetButton_5()
    {
        if (!startGame)
        {
            betAmount = 5f;
            betAmountTxt.text = betAmount.ToString("0.00" + " <size=30>EUR</size>");
            button_5.gameObject.SetActive(true);
            button_25.gameObject.SetActive(false);
            button_50.gameObject.SetActive(false);
            button_100.gameObject.SetActive(false);
        }
    }
    public void BetButton_25()
    {
        if (!startGame)
        {
            betAmount = 25f;
            betAmountTxt.text = betAmount.ToString("0.00" + " <size=30>EUR</size>");
            button_5.gameObject.SetActive(false);
            button_25.gameObject.SetActive(true);
            button_50.gameObject.SetActive(false);
            button_100.gameObject.SetActive(false);
        }
    }
    public void BetButton_50()
    {
        if (!startGame)
        {
            betAmount = 50f;
            betAmountTxt.text = betAmount.ToString("0.00" + " <size=30>EUR</size>");
            button_5.gameObject.SetActive(false);
            button_25.gameObject.SetActive(false);
            button_50.gameObject.SetActive(true);
            button_100.gameObject.SetActive(false);
        }
    }
    public void BetButton_100()
    {
        if (!startGame)
        {
            betAmount = 100f;
            betAmountTxt.text = betAmount.ToString("0.00" + " <size=30>EUR</size>");
            button_5.gameObject.SetActive(false);
            button_25.gameObject.SetActive(false);
            button_50.gameObject.SetActive(false);
            button_100.gameObject.SetActive(true);
        }
    }
    public void PlusButton()
    {
        if (!startGame)
        {
            if (betAmount == 5f)
            {
                betAmount = 25f;
                betAmountTxt.text = betAmount.ToString("0.00" + " <size=30>EUR</size>");
            }
            else if (betAmount == 25f)
            {
                betAmount = 50f;
                betAmountTxt.text = betAmount.ToString("0.00" + " <size=30>EUR</size>");
            }
            else if (betAmount == 50f)
            {
                betAmount = 100f;
                betAmountTxt.text = betAmount.ToString("0.00" + " <size=30>EUR</size>");
            }
        }
    }
    public void MinusButton()
    {
        if (!startGame)
        {
            if (betAmount == 100f)
            {
                betAmount = 50f;
                betAmountTxt.text = betAmount.ToString("0.00" + " <size=30>EUR</size>");
            }
            else if (betAmount == 50f)
            {
                betAmount = 25f;
                betAmountTxt.text = betAmount.ToString("0.00" + " <size=30>EUR</size>");
            }
            else if (betAmount == 25f)
            {
                betAmount = 5f;
                betAmountTxt.text = betAmount.ToString("0.00" + " <size=30>EUR</size>");
            }
        }
    }
    #endregion
    void ApplyParallaxEffect(float holdButtonYPosition)
    {
        // Calculate the target position for the background
        Vector3 targetBackgroundPosition = initialBackgroundPosition;
        targetBackgroundPosition.y -= holdButtonYPosition + parallaxAmount;

        // Smoothly interpolate towards the target position over time
        background.localPosition = Vector3.Lerp(background.localPosition, targetBackgroundPosition, Time.deltaTime * smoothness);
    }
}
