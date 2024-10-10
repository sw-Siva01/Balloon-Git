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
    public string currentInput = "";
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
    private void Start()
    {
        for (int i = 0; i < NumKeys.Length; i++)
        {
            int j = i;
            NumKeys[i].onClick.AddListener(() => OnNumberPressed(j));
        }
        submitButton.onClick.AddListener(() => { UI_Controller.instance.PlayButtonSound(); OnSubmitInput(); });
        backSpaceButton.onClick.AddListener(() => OnBackSpacePressed());
        decimalButton.onClick.AddListener(() => OnDecimalValuePressed());
        cancelButton.onClick.AddListener(() => OnCancelInput());
    }
    public void OnNumberPressed(int number)
    {
        MasterAudioController.instance.PlayAudio(AudioEnum.buttonClick);
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

        UpdateDisplay(false);
    }

    void OnDecimalValuePressed()
    {
        MasterAudioController.instance.PlayAudio(AudioEnum.buttonClick);
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
                controller.betAmount = float.Parse(currentInput);
            }
        }
        else
        {
            displayText.text = currentInput;
            MasterAudioController.instance.PlayAudio(AudioEnum.buttonClick);
        }

        currentInput = displayText.text;
        decimalButton.interactable = !currentInput.Contains(".");
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
        MasterAudioController.instance.PlayAudio(AudioEnum.buttonClick);
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
            controller.betAmount = minValue;
            OnSubmitAction?.Invoke(float.Parse(controller.betAmount.ToString("0.00")));
        }
        else
        {
            if (displayText.text == ".")
            {
                currentInput = minValue.ToString();
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
                controller.betAmount = 100;
            }
            Debug.Log("KeyPad Input WIth Some Values");
        }
        Debug.Log("OnSubmitInput Called");
        BetInputController.Instance.CloseKeyPadPanel();
        BetInputController.Instance.BetAmtInput.textViewport.gameObject.SetActive(false);
    }

    public void OnCancelInput()
    {
        Debug.Log("OnCancelInput");
        MasterAudioController.instance.PlayAudio(AudioEnum.buttonClick);
        currentInput = displayText.text;
        if (string.IsNullOrWhiteSpace(currentInput))
        {
            Debug.Log("IsEmptyInput " + BetInputController.Instance.IsEmptyInput);
            BetInputController.Instance.IsEmptyInput = true;

            Debug.Log("KeyPad Input WIthout Value");
            controller.betAmount = minValue;

            OnCancelAction?.Invoke(float.Parse(controller.betAmount.ToString("0.00")));
        }
        else
        {
            if (displayText.text == ".")
            {
                currentInput = minValue.ToString();
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
                controller.betAmount = 100;
            }

            Debug.Log("KeyPad Input WIth Some Values");
        }
        Debug.Log("OnCancelInput Called");

        BetInputController.Instance.CloseKeyPadPanel();
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
