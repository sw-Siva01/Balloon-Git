using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;

public class BetHistory : MonoBehaviour
{
    public static BetHistory instance;
    public GameObject betItemPrefab; // Assign via Inspector
    public Transform contentParent;  // ScrollView/Viewport/Content
    public List<Betlist> betlistArray = new List<Betlist>();

    [SerializeField] Button loadMore_Btn;
    [SerializeField] Button loadMore_Btn_2;
    [SerializeField] Button close_Btn;
    [SerializeField] ScrollRect Rect;
    [SerializeField] bool isON;
    [SerializeField] RectTransform BgRect;
    [SerializeField] GameObject panel;
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        loadMore_Btn.onClick.AddListener(Load_More_Btn);
        loadMore_Btn_2.onClick.AddListener(loadMore_Btn_Nxt);
        close_Btn.onClick.AddListener(CloseBtn);
    }
    public void AddPlayerBetDetails(List<Betlist> betlistArray, bool rev = false)
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject); // Clear previous data
        }
        for (int i = 0; i < betlistArray.Count; i++)
        {
            var bet = betlistArray[i];
            GameObject item = Instantiate(betItemPrefab, contentParent);
            if (rev)
            {
                item.transform.SetAsFirstSibling(); // Move to top
            }
            BetItemUI betUI = item.GetComponent<BetItemUI>();
            betUI.Setup(bet, i); // Pass index to control overlay visibility
        }

        ValidateHistroy();

        // Optionally show balance somewhere
        DebugHelper.Log("Player balance: ");
    }

    public void ValidateHistroy()
    {
        Rect.vertical = false;
        if (betlistArray.Count < 5) return;

        if (!isON)
        {
            loadMore_Btn.gameObject.SetActive(true);
        }
    }

    private void Load_More_Btn()
    {
        loadMore_Btn.gameObject.SetActive(false);
        BgRect.DOSizeDelta(new Vector2(BgRect.sizeDelta.x, 2160), 0.15f);
        isON = true;
        if (betlistArray.Count < 22) return;
        loadMore_Btn_2.gameObject.SetActive(true);
    }

    void loadMore_Btn_Nxt()
    {
        loadMore_Btn_2.gameObject.SetActive(false);
        Rect.vertical = true;
    }

    void CloseBtn()
    {
        panel.SetActive(false);
        Rect.vertical = false;
        BgRect.DOSizeDelta(new Vector2(BgRect.sizeDelta.x, 760), 0f);
        isON = false;
        loadMore_Btn_2.gameObject.SetActive(false);
    }
}
[System.Serializable]
public class Betlist
{
    public double bet_amount;
    public double win_amount;
    public double multiplier;
    public string dateTime;
    public string matchID;
    public string GetDateAndTimeString()
    {
        return dateTime;
    }
    public DateTime GetDateAndTime()
    {
        DateTime data;
        DateTime.TryParse(dateTime, out data);
        return ConvertToLocalTime(dateTime);
    }
    public DateTime ConvertToLocalTime(string dateTimeString)
    {
        if (DateTime.TryParse(dateTimeString, out DateTime parsedDateTime))
        {
            // Assume parsedDateTime is in UTC
            return parsedDateTime.ToLocalTime();
        }
        else
        {
            return parsedDateTime;
        }
    }
}
