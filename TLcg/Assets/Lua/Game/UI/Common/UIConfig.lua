UIConfig = {}

-- 窗口脚本
UIConfig.ControllerName = {
    Popup = "Game.UI.Popup.PopupController",
    Login = "Game.UI.Login.LoginController",
    Autofix = "Game.UI.Autofix.AutofixController",
    Loading = "Game.UI.Loading.LoadingController",
    ServerList = "Game.UI.ServerList.ServerListController",
    Maincity = "Game.UI.Maincity.MaincityController",
    Battle = "Game.UI.Battle.BattleController"
}

-- view脚本
UIConfig.ViewName = {
    Popup = "Game.UI.Popup.PopupView",
    Login = "Game.UI.Login.LoginView",
    Autofix = "Game.UI.Autofix.AutofixView",
    Loading = "Game.UI.Loading.LoadingView",
    ServerList = "Game.UI.ServerList.ServerListView",
    Maincity = "Game.UI.Maincity.MaincityView",
    Battle = "Game.UI.Battle.BattleView"
}

-- ui消息类型
UIConfig.Event = {
    LOGIN_OK = "LOGIN_OK",
    LOADING_P = "LOADING_P",
    LOADING_OK = "LOADING_OK"
}
