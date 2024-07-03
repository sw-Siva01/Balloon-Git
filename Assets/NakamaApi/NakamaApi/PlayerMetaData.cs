
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Globalization;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;

[Serializable]
public class PlayerMetaDatas
{
    #region api
    #region TransactionDetails

    [System.Serializable]
    public struct TransactionMetaData
    {
        public string gameName;
        public string gameCategories;
        public string LobbyName;
    }

    #endregion

    #region InGame
    public void UpdateGameData(string roomID, string lobbyName, string gameName, string gameStateJson)
    {
     //   NakamaManager.Instance.UpdateGameData(gameName, roomID, gameStateJson);
    }
  
    // status , botid , botname

    public async void GetABot(Action<bool, PlayerData> action)
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        //await NakamaManager.Instance.GetABot(async (success, botData) =>
        //{
        //    action.Invoke(success, botData);
        //});

    }

    /// <summary>
    ///  game played and game won worked in cash game only
    /// </summary>
   

    [System.Serializable]
    public struct MatchCreateECStruct
    {
        public string created_by;
        public string match_type; // Eg : GameName  // Can be Ludo or Carrom 
        public string match_date_time;
        public string game_type; // Eg:  Fun Games or Board Games
        public string lobby_Name;
        public string room_Name;
        public int cash_type;
        public string game; // Eg : Ludo Supreme
        public int amount;  // Eg : AddAmount
        public int playerCount; // Eg : PlayerCount
    }


    #endregion
    #endregion


    #region private variables

    #region profile title data
    [SerializeField] private string PlayerId;
    [SerializeField] private string NickName;
    [SerializeField] private string FullName;
    [SerializeField] private string AvatarUrl;
    [SerializeField] private Sprite AvatarSprite;
    [SerializeField] private string MobileNo;
    [SerializeField] private string EmailId;


    [SerializeField] private double GoldVal;
    [SerializeField] private double GoldBonusVal;
    [SerializeField] private double GoldWinningVal;
    [SerializeField] private double GoldDepositVal;
    [SerializeField] private double SilverVal;
    [SerializeField] private double TotalDeposit;
   #endregion

    #region title data
    [SerializeField] private int MaxPing;
    [SerializeField] private int WarningPing;

    [SerializeField] private float WinCommissionPercentage;
    [SerializeField] private string ShareMsgTemplate;
    [SerializeField] private string RoomSummaryShareMsg;
    [SerializeField] private string PaymentCollectUrl;
    [SerializeField] private string PaymentValidateUrl;
    [SerializeField] private string PaymentTransactionUrl;
    [SerializeField] private string PaymentWithdrawUrl;
    [SerializeField] private string TermsUrl;
    [SerializeField] private string RefundPolicy;
    [SerializeField] private string ResponsibleGamingUrl;
    [SerializeField] private string WelcomeText;
    [SerializeField] private string ContactUs;
    [SerializeField] private string PrivacyPolicyUrl;
    [SerializeField] private string FileUploadUrl;
    [SerializeField] private string GetOtpUrl;
    [SerializeField] private string ValidateOtpUrl;
    [SerializeField] private string CopyRights;
    [SerializeField] private int FirstGoldDepositBonusPercentage;
    [SerializeField] private int SignUpBonusGoldDeposit;
    #endregion

    #endregion




    #region Title Data Getter

    public int GetMaxPing()
    {
        return MaxPing;
    }

    public int GetWarningPing()
    {
        return WarningPing;
    }


    public string GetWelcomeText()
    {
        return WelcomeText;
    }

    public float GetCommissionPercentage()
    {
        return 0.2f; //GameController.Instance.gameData.WinCommissionPercentage;
    }


    public string GetShareMsgTemplate()
    {
        return ShareMsgTemplate;
    }
    public string GetRoomSummaryShareMsg()
    {
        return RoomSummaryShareMsg;
    }

    public string GetPaymentCollectUrl()
    {
        return PaymentCollectUrl;
    }

    public string GetPaymentValidateUrl()
    {
        return PaymentValidateUrl;
    }

    public string GetPaymentTransactionUrl()
    {
        return PaymentTransactionUrl;
    }
    public string GetPaymentWithdrawUrl()
    {
        return PaymentWithdrawUrl;
    }

    public string GetTermsUrl()
    {
        return TermsUrl;
    }

    public string GetRefundPolicy()
    {
        return RefundPolicy;
    }

    public string GetContactUs()
    {
        return ContactUs;
    }


    public string GetPrivacyPolicyUrl()
    {
        return PrivacyPolicyUrl;
    }

    public string GetResponsibleGamingUrl()
    {
        return ResponsibleGamingUrl;
    }



    public string GetFileUploadUrl()
    {
        return FileUploadUrl;
    }
    public string GetGetOtpUrl()
    {
        return GetOtpUrl;
    }


    public string GetValidateOtpUrl()
    {
        return ValidateOtpUrl;
    }

    #endregion


    #region Details Getter


    public string GetPlayerID()
    {
         return  PlayerId;
    }
    public string GetNickName()
    {
        return NickName;
    }
    public string GetFullName()
    {
        return FullName;
    }
    //public ProfilePicType GetAvatarIndex()
    //{
    //    return AvatarIndex;
    //}
    public string GetAvatarUrl()
    {
        return AvatarUrl;
    }
    public Sprite GetAvatarSprite()
    {
        return AvatarSprite;
    }
    public Sprite GetAvatarSpriteByIndex(int spriteIndex)
    {
        return null;
    }
    public string GetMobileNumber()
    {
        return MobileNo;
    }

    public int GetFirstGoldDepositBonusPercentage()
    {
        return FirstGoldDepositBonusPercentage;
    }

    public string GetEmailID()
    {
        return EmailId;
    }


    #endregion

    #region Stats Getter


    public void SetPlayerTitleData(CurrentPlayerData playerData)
    {
        PlayerId = playerData.user.Id;
        FullName = playerData.user.Display_Name;
        NickName = playerData.user.Display_Name;
        AvatarUrl = playerData.user.Avatar_Url;
        AvatarSprite = null;
        MobileNo = playerData.user.Username;
        EmailId = playerData.user.Email;
        GoldVal = playerData.wallet.CashDepositVal / 1000;
        GoldBonusVal = playerData.wallet.CashBonusVal / 1000;
        GoldWinningVal = playerData.wallet.CashWinningVal / 1000;
        GoldDepositVal = playerData.wallet.CashDepositVal / 1000;
        SilverVal = playerData.wallet.SilverVal / 1000;
        TotalDeposit = playerData.wallet.TotalDeposit / 1000;
    }

    //#region Details Setter
    public void SetClearingMatchDetails()
    {

    }

    public void SetkillCount(int count)
    {

    }
    public void SetNoGamesWon(int count)
    {

    }
    public void SetwinStreak(int count)
    {

    }
    public void SetGamesPlayedLocal(int count)
    {

    }

    public int GetPlayerXPVal()
    {
        return 0;
    }
    public int GetGamesPlayedLocal()
    {
        return 0;
    }
    public int GetwinStreak()
    {
        return 0;
    }
    public int GetkillCount()
    {
        return 0;
    }

    public double GetGoldVal()
    {
        return (GoldVal + GoldWinningVal);
    }

    public double GetGoldBonusVal()
    {
        return GoldBonusVal;
    }

    public double GetGoldWinningVal()
    {
        return GoldWinningVal;
    }

    public double GetSilverVal()
    {
        return SilverVal;
    }
  
    #endregion




}

#region GameEnum

[Serializable]
public class AppVersion
{
    public float IOSVersion;
    public string WebsiteUrl;
    public string PlayStoreUrl;
    public string SupportEmail;
    public string AndroidAppUrl;
    public float AndroidVersion;
}

[Serializable]
public class PaymentUrls
{
    public string TermsUrl;
    public string ContactUs;
    public string GetOtpUrl;
    public string RefundPolicy;
    public string FileUploadUrl;
    public string ValidateOtpUrl;
    public string PlayerLoginsUrl;
    public string ValidateRefCode;
    public string GerenrateRefCode;
    public string MatchLogsWebHook;
    public string PrivacyPolicyUrl;
    public string UpdateKYCDetails;
    public string PaymentCollectUrl;
    public string RegisterNewPlayer;
    public string RoomClosedWebHook;
    public string RoomCreateWebHook;
    public string UpdateBankDetails;
    public string MatchCreateWebHook;
    public string PaymentValidateUrl;
    public string PaymentWithdrawUrl;
    public string MatchPlayersWebHook;
    public string ValidateMobileNoUrl;
    public string ResponsibleGamingUrl;
    public string PaymentTransactionUrl;
}
[Serializable]
public class GameTitleData
{
    public int MaxPing;
    public int WarningPing;
    public AppVersion AppVersion;
    public string CopyRights;
    public PaymentUrls PaymentUrls;
    public int SignUpBonus;
    public string WelcomeText;
    public int ReferralBonus;
    public double TDSPercentage;
    public int ProfileVerified;
    public string ShareMsgTemplate;
    public int TutorialCompleted;
    public string RefferalMsgTemplate;
    public int ReferralBonusForRedeem;
    public int SignUpBonusGoldDeposit;
    public float WinCommissionPercentage;
    public bool IsServerUnderMaintenance;
    public int FirstGoldDepositBonusLimit;
    public int FirstGoldDepositBonusPercentage;
    public string AssestBundelMetaData;
}


[Serializable]
public class GameMeteData
{
    public int id;
    public string name;
    public bool addBot;
    public bool isActive;
    public int bootAmount;
    public int maxBotCount;
    public int noOfPlayers;
    public List<double> winnerPrice;
    public int maxTurnCount;
    public int tickRateInSec;
    public int maxPlayTimeInMin;
    public int minScore;
    public int maxScore;
    public int avgScore;
}
[Serializable]
public class TextureData
{
    public string name;
    public string video;
    public string image;
}

[Serializable]
public class GameDetail
{
    public string gameName;
    public List<GameMeteData> gamedata;
    public List<TextureData> textureDetails;
    public bool isActive;
    public string url;
    public bool isAutoUpdate;
}

[Serializable]
public class GameDetails
{
    public List<GameDetail> gameDetails;
}


[System.Serializable]
public class PlayerData
{
    public string playerID;
    public string playerName;
    public bool isBot;
    public string profilePicURL;
    public int avatarIndex;
    public double gold;
    public double silver, money;
    public int gamesPlayed;
    public int gamesWon;
    public double totalWinnings;
    public double CurrentRoomAmount;
}

[System.Serializable]
public class CurrentPlayerData
{
    public UserData user = new UserData();
    public PlayerWalletData wallet = new PlayerWalletData();
}
[System.Serializable]
public class PlayerWalletData
{
    public double SilverVal;
    public double NetWinning;
    public double CashBonusVal;
    public double TotalDeposit;
    public double NetAmountLost;
    public double CashDepositVal;
    public double CashWinningVal;
    public double TotalWithdrawal;
    public double TotalDepositByPlayer;
}

[System.Serializable]
public class UserData
{
    public string Id;
    public string Username;
    public string Display_Name;
    public string Avatar_Url;
    public string Lang_Tag;
    public string Metadata;
    public string Email;
}
[Serializable]
public enum CoinType
{
    RUPEES = 1, //Gold Val
    GEMS = 0,   //Silver Val
}

[System.Serializable]
public class GameData
{
    public string userId;
    public bool isFreeGame = false;
    public string gameName = string.Empty;
}

[System.Serializable]
public class ProfilePicture
{
    public ProfilePicType ProfileType;
    public string ProfileUrl;
}

[Serializable]
public class BotData
{
    public UserBOT user = new();
    public string wallet;
    public string custom_id;
}
[Serializable]
public class UserBOT
{
    public string id;
    public string username;
    public string display_name;
    public string avatar_url;
    public string lang_tag;
    public string metadata;
}
[System.Serializable]
public enum ProfilePicType
{
    Index = 1,
    Url = 2,
    Base64 = 3
}

#endregion