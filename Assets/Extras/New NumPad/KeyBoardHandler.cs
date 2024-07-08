using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Text.RegularExpressions;

public class KeyBoardHandler : MonoBehaviour
{
    public Button[] NumKeys;
    public Button decimalButton, submitButton, backSpaceButton, cancelButton;
    public TMP_InputField displayText;
    private string currentInput = "";
    private float minValue = 0.1f;
    private float maxValue = 100f;
    Action<float> OnSubmitAction, OnValurChangedAction;
    Action<float> OnCancelAction;
    public CanvasGroup canvasGroup;
    public static KeyBoardHandler instance;
    public RectTransform BetInputTextRect;
    [SerializeField] GameController controller;
    private void Awake()
    {
        instance = this;
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {

    }

    private void Start()
    {
        //displayText = BetInputController.Instance.BetAmtInput;
        for (int i = 0; i < NumKeys.Length; i++)
        {
            int j = i;
            NumKeys[i].onClick.AddListener(() => OnNumberPressed(j));
        }
        submitButton.onClick.AddListener(() => OnSubmitInput());
        backSpaceButton.onClick.AddListener(() => OnBackSpacePressed());
        decimalButton.onClick.AddListener(() => OnDecimalValuePressed());
        cancelButton.onClick.AddListener(() => OnCancelInput());
        // CoverBtn.onClick.AddListener(()=> OnCancelInput());
    }

    public void OnNumberPressed(int number)
    {
        /*AudioController.Instance.AudioPlay(true, AudioController.Instance.UIbtnsSFX);*/
        BetInputController.Instance.BetAmtInput.textComponent.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        BetInputController.Instance.BetAmtInput.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        int decimalIndex = currentInput.IndexOf('.');
        if (decimalIndex != -1)
        {
            string decimalPart = currentInput.Substring(decimalIndex + 1);
            if (decimalPart.Length == 2)
                return;
        }
        BetInputController.Instance.BetAmtInput.text = string.Empty;
        currentInput += number;
        //UpdateDisplay(!(number == 0 && currentInput.Length >= 2 && currentInput[currentInput.Length - 2] == '.'));

        //if (currentInput.Length >= 7)
        //    UpdateDisplay(!(number == 0 && currentInput.Length >= 7));
        //else
        UpdateDisplay(false);
    }

    void OnDecimalValuePressed()
    {
        /*AudioController.Instance.AudioPlay(true, AudioController.Instance.UIbtnsSFX);*/

        currentInput += ".";
        UpdateDisplay(false);

    }

    void UpdateDisplay(bool clampValue = true)
    {
        if (float.TryParse(currentInput, out float result))
        {
            if (clampValue)
            {
                result = Mathf.Clamp(result, minValue, maxValue);
                displayText.text = result.ToString();
            }
            else
            {
                displayText.text = currentInput;
            }
        }
        else
        {
            displayText.text = currentInput;
        }
        // float amt=float.Parse(displayText.text)
        currentInput = displayText.text;
        decimalButton.interactable = !currentInput.Contains(".");
        /*TowerGameController.Instance.BettingAmountTxt.gameObject.SetActive(false);*/
        controller.betAmountTxt.gameObject.SetActive(false);
        if (APIController.instance.userDetails.UserDevice == "mobile")
        {
            BetInputController.Instance.BetAmtInput.textViewport.gameObject.SetActive(true);
        }

        if (currentInput.Length > 10)
        {
            BetInputTextRect.localPosition = new Vector2(-((BetInputTextRect.sizeDelta.x / 2) + 20), 0);
        }

    }

    void OnBackSpacePressed()
    {
        /*AudioController.Instance.AudioPlay(true, AudioController.Instance.UIbtnsSFX);*/

        if (currentInput.Length > 0)
        {
            currentInput = displayText.text.Substring(0, displayText.text.Length - 1);
            UpdateDisplay(false);
        }
        if (!currentInput.Contains("."))
        {
            decimalButton.interactable = true;
        }
    }

    public void OnClearButtonPressed()
    {
        currentInput = "";
        displayText.text = "0";

    }

    void OnSubmitInput()
    {
        Debug.Log("OnSubmitInput");
        currentInput = displayText.text;
        if (string.IsNullOrWhiteSpace(currentInput))
        {

            Debug.Log("KeyPad Input WIthout Value");
            /*TowerGameController.Instance.BetAmount = minValue;*/
            controller.betAmount = minValue;
            /*OnSubmitAction?.Invoke(float.Parse(TowerGameController.Instance.BetAmount.ToString("0.00")));*/
            OnSubmitAction?.Invoke(float.Parse(controller.betAmount.ToString("0.00")));
        }
        else
        {
            if (displayText.text == ".")
            {
                currentInput = minValue.ToString();
                /*TowerGameController.Instance.BetAmount = minValue;*/
                controller.betAmount = minValue;

            }
            else
            {
                currentInput = displayText.text;

            }
            float f;
            try
            {
                f = Mathf.Clamp(float.Parse(currentInput), 0.10f, 100.00f);
                OnSubmitAction?.Invoke(f);

            }
            catch
            {
                Debug.Log("Cant Convert Too lengthy");
                OnSubmitAction?.Invoke(100.00f);
                /*TowerGameController.Instance.BetAmount = 100;*/
                controller.betAmount = 100;


            }


            Debug.Log("KeyPad Input WIth Some Values");

        }
        Debug.Log("OnSubmitInput Called");
        BetInputController.Instance.CloseKeyPadPanel();
        // BetAmtInput.gameObject.SetActive(false);
        BetInputController.Instance.BetAmtInput.textViewport.gameObject.SetActive(false);
        // TowerGameController.Instance.BettingAmountTxt.text = TowerGameController.Instance.BetAmount.ToString("0.00");

    }

    void OnCancelInput()
    {
        Debug.Log("OnCancelInput");
        /*AudioController.Instance.AudioPlay(true, AudioController.Instance.UIbtnsSFX);*/

        currentInput = displayText.text;
        if (string.IsNullOrWhiteSpace(currentInput))
        {
            Debug.Log("IsEmptyInput " + BetInputController.Instance.IsEmptyInput);
            BetInputController.Instance.IsEmptyInput = true;

            Debug.Log("KeyPad Input WIthout Value");
            controller.betAmount = minValue;

            /*OnCancelAction?.Invoke(float.Parse(TowerGameController.Instance.BetAmount.ToString("0.00")));*/
            OnCancelAction?.Invoke(float.Parse(controller.betAmount.ToString("0.00")));
        }
        else
        {
            if (displayText.text == ".")
            {
                currentInput = minValue.ToString();
                /*TowerGameController.Instance.BetAmount = minValue;*/
                controller.betAmount = minValue;

            }
            else
            {
                currentInput = displayText.text;

            }
            float f;
            try
            {
                f = Mathf.Clamp(float.Parse(currentInput), 0.10f, 100.00f);
                OnSubmitAction?.Invoke(f);

            }
            catch
            {
                Debug.Log("Cant Convert Too lengthy");
                OnSubmitAction?.Invoke(100.00f);
                /*TowerGameController.Instance.BetAmount = 100;*/
                controller.betAmount = 100;


            }

            Debug.Log("KeyPad Input WIth Some Values");

        }
        Debug.Log("OnCancelInput Called");

        BetInputController.Instance.CloseKeyPadPanel();
        // BetAmtInput.gameObject.SetActive(false);
        BetInputController.Instance.BetAmtInput.textViewport.gameObject.SetActive(false);

    }

    public void ShowKeyBoard(float currentValue, Action<float> _onSubmitAction, Action<float> _onValueChanged, Action<float> _onCancelAction)
    {
        BetInputController.Instance.BetAmtInput.textComponent.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        BetInputController.Instance.BetAmtInput.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        currentInput = string.Empty;
        UpdateDisplay();
        OnSubmitAction = _onSubmitAction;
        OnCancelAction = _onCancelAction;
        OnValurChangedAction = _onValueChanged;
    }
}
