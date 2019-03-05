-- 游戏
Common = {
	-- 模式（启动模式、游戏模式）
	Mode = 0,
	-- 语音信息
	Voice = {Id = "993557476", Key = "bc1b66fc869b23c9825ccc143956778b"},
	-- UI分辨率
	UIResolution = {x = 1334, y = 750},
	-- 屏幕分辨率
	ScreenResolution = {x = CS.UnityEngine.Screen.width, y = CS.UnityEngine.Screen.height},
	-- 登陆信息
	LoginInfo = {
		-- 平台信息
		Desc = nil,
		-- 登陆标识
		Identification = "dev",
		-- 系统类型0-ios 1-android
		OSType = "0",
		-- 登陆地址
		LoginAddr = "https://login.lightpaw.com",
		-- 资源地址
		HotAddr = "https://male7client-1256076575.cos.ap-guangzhou.myqcloud.com/male7ab",
		-- 资源版号
		ResVersion = "1.0.5",
		-- true表示没有注册
		HaveNotRegister = false,
		-- true表示有ip输入框
		HaveIpInput = false,
		-- 1内网登陆 2msdk渠道登陆 3play800渠道登录
		LoginMode = 0
	},
	-- 版本信息
	Version = {
		-- 配置版本号
		ConfigVersion = nil,
		-- 服务器资源版号
		ServerResVersion = nil,
		-- 需要检测的资源版号
		CheckResVersion = nil,
		-- 本地发包时的初始版号
		OriginalResVersion = "0.1580.1.0"
	},
	-- 登录信息;
	Account = {
		-- 登陆服务器信息
		Server = nil,
		-- 平台登陆类型（msdk,p8...）
		PlatformType = nil
	}
}
