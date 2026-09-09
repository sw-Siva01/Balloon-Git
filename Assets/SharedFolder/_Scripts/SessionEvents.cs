using System;
using System.Collections.Generic;
using UnityEngine;

public static class SessionEvents
{
    public static Action<bool> OnSwitchingTab;
    public static Action<NetworkStatus> OnInternetStatusChange;
    public static Action OnUserDetailsUpdate;
}

public static class SessionState
{
    public static string platform = "web";
    public static string Id = "";
    public static bool isInFocus = true;
    public static bool isOnline = true;

    public static BalloonEntryAmountDetails entryAmountDetails = new();

    public static string name = "User_01";
    public static string environment = "DEV";

    public static bool sound = true;
    public static bool music = true;

}

// Named distinctly from ObjectContainers.cs's EntryAmountDetails (the old Lootrix
// shape with pot limits/chaal limits) — this is the simpler Aviator-pattern shape.
[System.Serializable]
public class BalloonEntryAmountDetails
{
    public List<int> betValues = new List<int>() { 1, 2, 5, 10 };
    public int maxBetValue = 100;
    public int minBetValue = 1;
    public int incrementValue = 1;
    public int decrementValue = 1;
    public string currency_type = "USD";
}
