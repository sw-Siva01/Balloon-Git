using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class BetItemUI : MonoBehaviour
{
    public TextMeshProUGUI betAmountText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI WinText;
    public TextMeshProUGUI multiplierText;
    public GameObject Overlay;
    public void Setup(Betlist bet, int index)
    {
        string currency = $"<color=#FFFFFF80>{APIController.instance.authentication.currency_type}</color>";
        betAmountText.text = bet.bet_amount.ToString("0.00") + " " + currency;
        timeText.text = bet.GetDateAndTimeString();
        multiplierText.text = bet.multiplier.ToString() + "x";
        if (bet.win_amount > bet.bet_amount)
        {
            WinText.color = Color.green;
            multiplierText.color = Color.green;
            WinText.text = "+" + bet.win_amount.ToString("0.00") + " " + currency;
        }
        else
        {
            WinText.color = Color.white;
            multiplierText.color = Color.white;
            WinText.text = bet.win_amount.ToString("0.00") + " " + currency;
        }
        // :white_check_mark: Show overlay only on every 2nd item (index 1, 3, 5, ...)
        Overlay.SetActive((index + 1) % 2 == 0);
        //multiplierText.text = (bet.win_amount / bet.bet_amount).ToString("0.00") + "x";
        multiplierText.text = (bet.multiplier).ToString("0.00") + "x";
    }
}