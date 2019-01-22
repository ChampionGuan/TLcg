-- UI定义
UIDefine = {
    -- ctrl类型
    CtrlType = {
        -- 全屏
        FullScreen = 1,
        -- 镂空
        HollowOut = 2,
        -- 弹框
        PopupBox = 3
    },
    -- ui的渲染分层
    SortingOrder = {
        -- 主城npc对话
        MainCityDialog = 0,
        -- 聊天缩略框
        ChatBrief = 100,
        -- 场景加载
        SceneLoading = 150,
        -- 信息同步
        MsgSync = 200,
        -- 新手引导(248~250)
        NoviceGuide = 250,
        -- 新功能开启
        NewFunctionOpen = 260,
        -- 剧情层级
        ScenarioDialog = 300,
        -- 城池升级
        CityUpgrade = 350,
        -- 城池流亡
        CityExiled = 400,
        -- 全屏展示
        FullScreenShow = 450,
        -- 网络信号
        NetSignal = 500,
        -- 网络弹框
        NetError = 550,
        -- 系统广播
        SystemMsg = 600,
        -- 消息提示
        MsgTips = 650,
        -- 点击特效
        ClickEffect = 700,
        -- 服务器时间
        ServerTime = 1000,
        -- gm指令
        GmOrder = 1001,
        -- 退出app
        QuitApp = 2000
    }
}
