using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;
using static System.Net.Mime.MediaTypeNames;
using Debug = UnityEngine.Debug;
using Input = UnityEngine.Input;

public class BetInputController : MonoBehaviour
{
    public KeyBoardHandler _KeyBoardHandler;
    public TMP_InputField BetAmtInput;
    public Button Done;
    int clickCount = 0;
    public RectTransform BetPanel;

    public static BetInputController Instance;
    public bool IsEmptyInput=false;

    [SerializeField] GameController controller;
    private void Awake()
    {
        Instance = this;
        BetAmtInput.onValueChanged.AddListener(delegate { OnBetAmountEdit(); });
        BetAmtInput.onEndEdit.AddListener(delegate { OnEndEditBetAmount(BetAmtInput.text.Length<15?100f:(float.Parse(BetAmtInput.text))); });
    }
    void Start()
    {
        Done.onClick.AddListener(delegate { CloseKeyPadPanel(); });
        BetAmtInput.text = "1.00";

    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("Enter key was pressed!");
            CloseKeyPadPanel();
            if (APIController.instance.userDetails.UserDevice == "mobile")
            {

                BetAmtInput.textViewport.gameObject.SetActive(false);
            }
        }
        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            Debug.Log("Enter key was pressed!");
            CloseKeyPadPanel();
            if (APIController.instance.userDetails.UserDevice == "mobile")
            {

                BetAmtInput.textViewport.gameObject.SetActive(false);
            }

        }
    }
    public void OpenKeyPadPanel()
    {
        controller.numPad = true;
        Debug.Log(" TOwer UserDevice ::" + APIController.instance.userDetails.UserDevice);
        if (APIController.instance.userDetails.UserDevice == "desktop")
        {
            return;
        }
        BetPanel.gameObject.SetActive(true);
        if (KeyBoardHandler.instance != null)
        {
            KeyBoardHandler.instance.cancelButton.gameObject.SetActive(true);
            DisableBetInput();
        }
    }
    public void OnBetAmountEdit()
    {
        string pattern = @"^\d*(\.\d{0,2})?$";
        if (Regex.IsMatch(BetAmtInput.text, pattern))
        {
            BetAmtInput.text = BetAmtInput.text;
        }
        else
        {

            BetAmtInput.text = BetAmtInput.text.Substring(0, BetAmtInput.text.Length - 1);
        }
    }
    public void OnEndEditBetAmount(float amount)
    {
        Debug.Log("OnEndEditBetAmount");
        if (APIController.instance.userDetails.UserDevice == "desktop")
        {
            if (!string.IsNullOrWhiteSpace(BetAmtInput.text))
            {
                if (BetAmtInput.text == ".")
                {
                    amount = 0.1f;

                }
                try
                {
                    amount = Mathf.Clamp((float)amount, 0.10f, 100.00f);
                }
                catch 
                {
                    Debug.Log("Cant convert Too Lengthy OnEndEditBetAmount");
                }       
            }
            else
            {
                if (APIController.instance.userDetails.UserDevice == "desktop")
                {
                    amount = 0.1f;

                }
                Debug.Log("eMPTY iNPUT eNDeDIT");
                IsEmptyInput = true;

            }
        }
        if (APIController.instance.userDetails.UserDevice == "desktop")
        {
            controller.betAmount = amount;
            string _s = controller.betAmount.ToString("0.00");
            controller.betAmount  = float.Parse(_s);

            EnableBetInput();
            BetAmtInput.interactable = false;
            BetAmtInput.interactable = true;
            BetAmtInput.textComponent.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            BetAmtInput.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            BetInputController.Instance.BetAmtInput.textViewport.gameObject.SetActive(false);

            if (APIController.instance.userDetails.UserDevice == "desktop")
            {
                controller.betAmountTxt.gameObject.SetActive(true);
            }
        }
        BetAmtInput.interactable = false;
        BetAmtInput.interactable = true;

    }
    public void OnEditInput()
    {

        if (APIController.instance.userDetails.UserDevice == "desktop")
        {
            string val = controller.betAmount.ToString("F2");
            BetAmtInput.text = val;
           DisableBetInput();
        }
        else
        {
            Debug.Log("OnEditInput ELse");
            
            controller.numPad = true;
            OpenKeyPadPanel();

            _KeyBoardHandler.ShowKeyBoard((float)controller.betAmount,
             (inputValue) =>
             {
                 Debug.Log("EDIT OVER" + inputValue + (float)controller.betAmount);
                 // DisableKeyBoard();
                 controller.betAmount = (float)inputValue;
                 string _s = controller.betAmount.ToString("0.00");
                 controller.betAmount = float.Parse(_s);
                 /*controller.betAmountTxt.text = controller.betAmount.ToString("F2") + " " + APIController.instance.userDetails.currency_type;*/
                 controller.betAmountTxt.text = controller.betAmount.ToString("F2") + " <size=30>" + APIController.instance.userDetails.currency_type + "</size>";
                 CloseKeyPadPanel();
                 controller.betAmountTxt.gameObject.SetActive(true);
                 Debug.Log("Done " + controller.betAmount);
             },
             (value) =>
             {
                 controller.betAmountTxt.gameObject.SetActive(false);
             },
             (input) =>
             {
                 Debug.Log("Cancelled ");

                 controller.betAmount = (float)input;
                 string _s = controller.betAmount.ToString("0.00");
                 controller.betAmount = float.Parse(_s);
                 /*controller.betAmountTxt.text = controller.betAmount.ToString("F2") + " " + APIController.instance.userDetails.currency_type;*/
                 controller.betAmountTxt.text = controller.betAmount.ToString("F2") + " <size=30>" + APIController.instance.userDetails.currency_type + "</size>";
                 CloseKeyPadPanel();
                 controller.betAmountTxt.gameObject.SetActive(true);
                 Debug.Log("Done " + controller.betAmount);
             });
            Debug.Log(" Done  ");
        }
    }
    public void RestrictInput()
    {
        if (string.IsNullOrWhiteSpace(BetAmtInput.text))
        {
            Debug.Log("Closing +");

            IsEmptyInput = true;
            controller.betAmount = 0.1f;
        }
        else
        {
            float amount = (float)controller.betAmount;
            if (BetAmtInput.text == ".")
            {
                amount = 0.1f;

            }
            else
            {
                if (BetAmtInput.text.Length > 15)
                {
                    amount = 100f;
                }
                else
                {
                    amount = float.Parse(BetAmtInput.text);
                }
            }
            amount = Mathf.Clamp(amount, 0.10f, 100.00f);
            IsEmptyInput = false;

            controller.betAmount = amount;
            string _s = controller.betAmount.ToString("0.00");
            controller.betAmount = float.Parse(_s);
        }
    }
    public void CloseKeyPadPanel()
    {
        controller.numPad = false;
        Debug.Log("CloseKeyPadPanel ");
        BetPanel.gameObject.SetActive(false);
        RestrictInput();

        if (KeyBoardHandler.instance != null)
        {
            KeyBoardHandler.instance.cancelButton.gameObject.SetActive(false);

        }

        EnableBetInput();
        Debug.Log("CloseKeyPadPanel Done ");
        controller.HandGesture();

    }

    public void EnableBetInput()
    {
        Debug.Log("EnableBetInput");

        BetAmtInput.interactable = true;
        if (APIController.instance.userDetails.UserDevice == "mobile")
        {
            if (APIController.instance.userDetails.UserDevice == "mobile")
            {
                controller.betAmountTxt.gameObject.SetActive(true);

            }
            else
            {
                controller.betAmountTxt.gameObject.SetActive(false);

            }
        }

        string _s = controller.betAmount.ToString("0.00");
        controller.betAmount = float.Parse(_s);
        /*controller.betAmountTxt.text = controller.betAmount.ToString("0.00") + " " + APIController.instance.userDetails.currency_type;*/
        controller.betAmountTxt.text = controller.betAmount.ToString("F2") + " <size=30>" + APIController.instance.userDetails.currency_type + "</size>";
        /*controller.betAmountTxt.text = controller.betAmount.ToString("F2") + APIController.instance.userDetails.currency_type;*/

        Debug.Log("EnableBetInput Called");

    }

    public void DisableBetInput()
    {
        Debug.Log("DisableBetInput");
        controller.betAmountTxt.gameObject.SetActive(false);
        BetAmtInput.text = controller.betAmount.ToString("0.00");
        string _s = controller.betAmount.ToString("0.00");
        controller.betAmount = float.Parse(_s);

        BetAmtInput.textViewport.gameObject.SetActive(true);
        //  if (APIController.instance.userDetails.UserDevice == "mobile") BetAmtInput.interactable = false;
        Debug.Log("DisableBetInput");

    }
}
