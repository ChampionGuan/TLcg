UIDefine = {}

-- ctrl类型
UIDefine.CtrlType = {
    -- 全屏
    FullScreen = 1,
    -- 镂空
    HollowOut = 2,
    -- 弹框
    PopupBox = 3
}

-- ui的渲染分层
UIDefine.SortingOrder = {
    -- 场景加载
    Loading = 100,
    -- 信息同步
    Sync = 200,
    -- 新手引导
    NoviceGuide = 300,
    -- 剧情层级
    Dialogue = 400,
    -- 全屏展示
    FullScreenShow = 500,
    -- 网络信号
    NetSignal = 600,
    -- 网络弹框
    NetError = 700,
    -- 系统广播
    SystemMsg = 800,
    -- 消息提示
    TipsMsg = 900,
    -- 点击特效
    ClickEffect = 1000,
    -- 服务器时间
    ServerTime = 1100,
    -- gm指令
    GmOrder = 1200,
    -- 退出app
    QuitApp = 1300
}
