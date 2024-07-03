mergeInto(LibraryManager.library, {
  CloseWindow: function () {
  ExitGame();
  },
  GetUpdatedBalance: function () {
  GetBalance();
  },
  DisconnectGame: function (msg) {
  Disconnect(UTF8ToString(msg));
  },
  GetLoginData: function () {
  GetLoginDetails("game");
  },
  FullScreen: function () {
  SetFullScreen();
  },
  ShowDeposit: function () {
  Deposit();
  },
  CheckOnlineStatus: function() {
    CheckNetworkStatus();
  },
  ExecuteExternalUrl: function (url,timeout) {
  ExecuteExternalRequestWithResponse(UTF8ToString(url),timeout);
  },
  GetABot: function () {
  GetBotData();
  },
  InitPlayerBet: function (type,index,game_user_Id,game_Id,metaData,isAbleToCancel,bet_amount,isBot) {
   
    InitBet(UTF8ToString(type),index,UTF8ToString(game_user_Id),UTF8ToString(game_Id),UTF8ToString(metaData),UTF8ToString(isAbleToCancel),bet_amount,isBot);
  },
  CancelPlayerBet: function (type,id,metaData,game_user_Id,game_Id,amount,isBot) {
    
  CancelBet(UTF8ToString(type),UTF8ToString(id),UTF8ToString(metaData),UTF8ToString(game_user_Id),UTF8ToString(game_Id),amount,isBot);
  },
  FinilizePlayerBet: function (type,id,metaData,game_user_Id,game_Id,isBot) {
      
  FinilizeBet(UTF8ToString(type),UTF8ToString(id),UTF8ToString(metaData),UTF8ToString(game_user_Id),UTF8ToString(game_Id),isBot);
  },
  WinningsPlayerBet: function (type,id,metaData,game_user_Id,game_Id,win_amount,spend_amount,isBot) {
   
  WinningsBet(UTF8ToString(type),UTF8ToString(id),UTF8ToString(metaData),UTF8ToString(game_user_Id),UTF8ToString(game_Id),win_amount,spend_amount,isBot);
  },
  MultiplayerWinningsPlayerBet: function (type,id,metaData,game_user_Id,game_Id,win_amount,spend_amount,pot_amount,isBot,isWinner) {
   
  WinningsBetMultiplayer(UTF8ToString(type),UTF8ToString(id),UTF8ToString(metaData),UTF8ToString(game_user_Id),UTF8ToString(game_Id),win_amount,spend_amount,pot_amount,isBot,isWinner);
  },
  AddPlayerBet: function (type,index,id,metaData,game_user_Id,game_Id,amount,isBot) {
   
  AddBet(UTF8ToString(type),UTF8ToString(index),UTF8ToString(id),UTF8ToString(metaData),UTF8ToString(game_user_Id),UTF8ToString(game_Id),amount,isBot);
  },
  GetRandomPrediction: function(type,rowCount,columnCount,predictedCount)
  {
    GetRandomPrediction(UTF8ToString(type),rowCount,columnCount,predictedCount);
  }
});