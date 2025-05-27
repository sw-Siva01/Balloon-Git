using System;
using System.Globalization;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class KeyPadManager : MonoBehaviour
{
    public float CurrentBetAmount;
    public TMP_Text CurrentBetInput;
    public TMP_InputField KeyPadInput;
    public float minBetValue;
    public float maxBetValue;
    private float _IncrementValue = 5;
    //public string convertBetString;
    private string pattern = @"^\d*(\.\d{0,2})?$";

    private void Awake()
    {
        UpdateCurrentBetAmount(1f);
        KeyPadInput.onSelect.AddListener(delegate { OnEditInput(); });
        KeyPadInput.onValueChanged.AddListener(delegate { OnValueChangeInput(); });
        KeyPadInput.onEndEdit.AddListener(delegate { OnEndEditBetInputFld(); });
    }

    public void UpdateCurrentBetAmount(float betValue)
    {
        CurrentBetAmount = betValue;
        CurrentBetAmount = Mathf.Clamp(CurrentBetAmount, minBetValue, maxBetValue);
        CurrentBetInput.text = CurrentBetAmount.ToString("F2");
        Debug.Log($"Current Bet Amount : {CurrentBetAmount}");
    }

    public void OnClickPlus()
    {
        UpdateCurrentBetAmount((CurrentBetAmount + _IncrementValue));
    }

    public void OnClickMinus()
    {
        UpdateCurrentBetAmount((CurrentBetAmount - _IncrementValue));
    }

    #region KEYPAD HANDLER

    public void OnEditInput()
    {
        //_InputHandler.gameObject.SetActive(true);
        KeyPadInput.text = string.Empty;
        KeyPadInput.interactable = true;
        CurrentBetInput.gameObject.SetActive(false);
        KeyPadInput.textViewport.gameObject.SetActive(true);
       /* _InputHandler.ShowKeyBoard(CurrentBetAmount,
            (inputValue) =>
            {
                CloseKeyPad(inputValue);
            },
            (value) =>
            {
                //CloseKeyPad(value);
            },
            (input) =>
            {
                CloseKeyPad(input);
            });*/
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || (Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            if (gameObject.activeSelf)
            {
                //_InputHandler.gameObject.SetActive(false);
                OnEndEditBetInputFld();

                KeyPadInput.textViewport.gameObject.SetActive(false);
                CurrentBetInput.gameObject.SetActive(true);
            }
        }
    }

    //private void CloseKeyPad(float inputValue)
    //{
    //    inputValue = Mathf.Clamp(inputValue, minBetValue, maxBetValue);
    //    _InputHandler.gameObject.SetActive(false);
    //    KeyPadInput.textViewport.gameObject.SetActive(false);
    //    CurrentBetInput.gameObject.SetActive(true);
    //    UpdateCurrentBetAmount(inputValue);
    //    KeyPadInput.interactable = true;
    //}

    public void OnEndEditBetInputFld()
    {
        if (KeyPadInput.interactable)
            KeyPadInput.interactable = false;
        KeyPadInput.interactable = true;

        try
        {
            float value = string.IsNullOrWhiteSpace(KeyPadInput.text) ? 0 : float.Parse(KeyPadInput.text);

            if (value > maxBetValue)
            {
                value = maxBetValue;
                UpdateCurrentBetAmount(value);

            }
            else if (value < minBetValue)
            {
                value = minBetValue;
                UpdateCurrentBetAmount(value);
            }
            else
            {
                UpdateCurrentBetAmount(value);
            }

            KeyPadInput.textComponent.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            KeyPadInput.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            if (KeyPadInput.interactable)
                KeyPadInput.interactable = false;
            KeyPadInput.interactable = true;

        }
        catch (FormatException e)
        {
            Debug.Log(e + " Error in conversion");
        }
    }

    public void OnValueChangeInput()
    {
        if (Regex.IsMatch(KeyPadInput.text, pattern))
        {
            KeyPadInput.text = KeyPadInput.text;
        }
        else
        {
            KeyPadInput.text = KeyPadInput.text.Substring(0, KeyPadInput.text.Length - 1);
        }

       CurrentBetAmount = string.IsNullOrWhiteSpace(KeyPadInput.text) ? 0 : float.Parse(KeyPadInput.text.ToString());
    }
    #endregion

   /* void PlusTargetValue()
    {
        audioController.PlayAudio(AudioEnum.buttonClick);
        //plus_Anim.SetBool("isON", true);

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
            if (targetMultiplier < 100)
            {
                targetMultiplier += 1f;
                targetMultiplierTxt.text = targetMultiplier.ToString("0.00");
                //BetAmountTxt_Scaling();
                addValBtns.interactable = true;
                addValBtnsImg.color = new Color32(255, 255, 255, 255);
                //AmountColor_Glow();
            }
            if (targetMultiplier >= 100f)
            {
                targetMultiplier = 100f;
                addValBtns.interactable = false;
                targetMultiplierTxt.text = targetMultiplier.ToString("0.00");
                //BetAmountTxt_Scaling();
                addValBtnsImg.color = new Color32(255, 255, 255, 100);
                //MaxBet_Object();
            }
        }
    }
    void MinusTargetValue()
    {
        audioController.PlayAudio(AudioEnum.buttonClick);
        //minus_Anim.SetBool("isON", true);
        if (BetInputController.Instance.BetPanel.gameObject.activeSelf)
        {
            BetInputController.Instance.CloseKeyPadPanel();
            BetInputController.Instance.RestrictInput();
            BetInputController.Instance.BetAmtInput.textViewport.gameObject.SetActive(false);
            return;
        }

        if (!startGame && !take && !isScroll && !onClick)
        {
            if (targetMultiplier > 1f)
            {
                targetMultiplier -= 1f;
                targetMultiplierTxt.text = targetMultiplier.ToString("0.00");
                //BetAmountTxt_Scaling();
                subValBtnsImg.color = new Color32(255, 255, 255, 255);
                //AmountColor_Glow();
            }
            if (targetMultiplier <= 2f)
            {
                targetMultiplier = 1.01f;
                targetMultiplierTxt.text = targetMultiplier.ToString("0.00");
                subValBtns.interactable = false;
                subValBtnsImg.color = new Color32(255, 255, 255, 100);
            }
            if (targetMultiplier <= 1f)
            {
                targetMultiplier = 1f;
                targetMultiplierTxt.text = targetMultiplier.ToString("0.00");
                //BetAmountTxt_Scaling();
                subValBtns.interactable = false;
                subValBtnsImg.color = new Color32(255, 255, 255, 100);
            }
        }
    }
    void Plus_Minus_Interactive()
    {
        if (targetMultiplier > 1f)
        {
            subValBtnsImg.color = new Color32(255, 255, 255, 255);
            subValBtns.interactable = true;
        }
        else if (targetMultiplier <= 1f)
        {
            subValBtnsImg.color = new Color32(255, 255, 255, 100);
            subValBtns.interactable = false;
        }

        if (targetMultiplier < 100f)
        {
            addValBtnsImg.color = new Color32(255, 255, 255, 255);
            addValBtns.interactable = true;
        }
        else if (targetMultiplier >= 100f)
        {
            addValBtnsImg.color = new Color32(255, 255, 255, 100);
            addValBtns.interactable = false;
        }
    }*/
}
