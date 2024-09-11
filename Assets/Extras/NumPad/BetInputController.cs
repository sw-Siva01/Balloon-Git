using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
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
       // BetAmtInput.onSelect.AddListener(delegate { OpenKeyPadPanel(); });
        Done.onClick.AddListener(delegate { CloseKeyPadPanel(); });
        //  Coverimg.onClick.AddListener(delegate { CloseKeyPadPanel(); });
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

        //if (clickCount == 0)
        //{
        //     BetPanel.DOAnchorPosY(1050f, .2f);
        BetPanel.gameObject.SetActive(true);
        //    TowerGameController.Instance.BettingAmountTxt.gameObject.SetActive(false);
        if (KeyBoardHandler.instance != null)
        {
            KeyBoardHandler.instance.cancelButton.gameObject.SetActive(true);
            DisableBetInput();
        }


        // clickCount++;
        // }
    }
    public void OnBetAmountEdit()
    {
        /*Debug.Log("OnBetAmountEdit");*/

        //if (APIController.instance.userDetails.UserDevice == "mobile")
        //{
        //    return;

        //}

        //  BetAmtInput.textViewport.gameObject.SetActive(true);
        string pattern = @"^\d*(\.\d{0,2})?$";
        if (Regex.IsMatch(BetAmtInput.text, pattern))
        {
            BetAmtInput.text = BetAmtInput.text;
        }
        else
        {

            BetAmtInput.text = BetAmtInput.text.Substring(0, BetAmtInput.text.Length - 1);
        }

        // Move the caret to the end of the text
        //     BetAmtInput.caretPosition = BetAmtInput.text.Length;
        //float amount = float.Parse(BetAmtInput.text);
        //amount = Mathf.Clamp(amount, 0.1f, 100);
        //double val = double.Parse(BetAmtInput.text);

        //TowerGameController.Instance.BetAmount = amount;
        //string _s = TowerGameController.Instance.BetAmount.ToString("0.00");
        //TowerGameController.Instance.BetAmount=double.Parse(_s);
        //BetAmtInput.text = val > 100 ? "100.00" : BetAmtInput.text;
        //  BetAmtInput.text = val < 0.10f ? "0.10" : BetAmtInput.text;
    }
    public void OnEndEditBetAmount(float amount)
    {
        //    IsEmptyInput = false;
        Debug.Log("OnEndEditBetAmount");

        //if (APIController.instance.userDetails.UserDevice == "mobile")
        //{
        //    return;

        //}
        //  BetAmtInput.textViewport.gameObject.SetActive(false);
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
            /*TowerGameController.Instance.BetAmount = amount;
            string _s = TowerGameController.Instance.BetAmount.ToString("0.00");
            TowerGameController.Instance.BetAmount = float.Parse(_s);*/

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
                 controller.betAmountTxt.text = controller.betAmount.ToString("F2") + " " + APIController.instance.userDetails.currency_type;
                 CloseKeyPadPanel();
                 controller.betAmountTxt.gameObject.SetActive(true);
                 Debug.Log("Done " + controller.betAmount);

                 controller.AmountColor_Glow();
                 controller.BetAmountTxt_Scaling();
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
                 controller.betAmountTxt.text = controller.betAmount.ToString("F2") + " " + APIController.instance.userDetails.currency_type;
                 CloseKeyPadPanel();
                 controller.betAmountTxt.gameObject.SetActive(true);
                 Debug.Log("Done " + controller.betAmount);



             });
            Debug.Log(" Done  ");
        }


        

        // otherBetHandler.gameObject.SetActive(false);
    }
    public void RestrictInput()
    {
        if (string.IsNullOrWhiteSpace(BetAmtInput.text))
        {
            Debug.Log("Closing +");

            IsEmptyInput = true;
            /*TowerGameController.Instance.BetAmount = 0.1f;*/
            controller.betAmount = 0.1f;
        }
        else
        {
            /*float amount=(float)TowerGameController.Instance.BetAmount;*/
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
            /*TowerGameController.Instance.BetAmount = amount;
            string _s = TowerGameController.Instance.BetAmount.ToString("0.00");
            TowerGameController.Instance.BetAmount = float.Parse(_s);*/

            controller.betAmount = amount;
            string _s = controller.betAmount.ToString("0.00");
            controller.betAmount = float.Parse(_s);
            //  TowerGameController.Instance.BettingAmountTxt.text = TowerGameController.Instance.BetAmount.ToString("F2") + " " + APIController.instance.userDetails.currency_type;

        }
    }
    public void CloseKeyPadPanel()
    {
        controller.numPad = false;
        Debug.Log("CloseKeyPadPanel ");
        BetPanel.gameObject.SetActive(false);
        RestrictInput();
        //  BetPanel.DOAnchorPosY(178f, .2f);
        if (KeyBoardHandler.instance != null)
        {
            KeyBoardHandler.instance.cancelButton.gameObject.SetActive(false);

        }

        EnableBetInput();
        Debug.Log("CloseKeyPadPanel Done ");
        controller.AmountColor_Glow();
        controller.BetAmountTxt_Scaling();
        Debug.Log(" Done  Done");
        controller.HandGesture();

    }

    public void EnableBetInput()
    {
        Debug.Log("EnableBetInput");
       // if (APIController.instance.userDetails.UserDevice == "desktop") BetAmtInput.textViewport.gameObject.SetActive(false);
        BetAmtInput.interactable = true;
        if (APIController.instance.userDetails.UserDevice == "mobile")
        {
            if (APIController.instance.userDetails.UserDevice == "mobile")
            {
                /*TowerGameController.Instance.BettingAmountTxt.gameObject.SetActive(true);*/
                controller.betAmountTxt.gameObject.SetActive(true);

            }
            else
            {
                 /*TowerGameController.Instance.BettingAmountTxt.gameObject.SetActive(false);*/
                controller.betAmountTxt.gameObject.SetActive(false);

            }
        }

        /*string _s = TowerGameController.Instance.BetAmount.ToString("0.00") ;
        TowerGameController.Instance.BetAmount = float.Parse(_s);
        TowerGameController.Instance.BettingAmountTxt.text = TowerGameController.Instance.BetAmount.ToString("0.00") + " " + APIController.instance.userDetails.currency_type;*/

        string _s = controller.betAmount.ToString("0.00");
        controller.betAmount = float.Parse(_s);
        controller.betAmountTxt.text = controller.betAmount.ToString("0.00") + " " + APIController.instance.userDetails.currency_type;

        Debug.Log("EnableBetInput Called");

    }

    public void DisableBetInput()
    {
        Debug.Log("DisableBetInput");

        /*TowerGameController.Instance.BettingAmountTxt.gameObject.SetActive(false);
        BetAmtInput.text = TowerGameController.Instance.BetAmount.ToString("0.00");
        string _s = TowerGameController.Instance.BetAmount.ToString("0.00");
        TowerGameController.Instance.BetAmount = float.Parse(_s);*/

        controller.betAmountTxt.gameObject.SetActive(false);
        BetAmtInput.text = controller.betAmount.ToString("0.00");
        string _s = controller.betAmount.ToString("0.00");
        controller.betAmount = float.Parse(_s);

        BetAmtInput.textViewport.gameObject.SetActive(true);
        //  if (APIController.instance.userDetails.UserDevice == "mobile") BetAmtInput.interactable = false;
        Debug.Log("DisableBetInput");

    }
}
