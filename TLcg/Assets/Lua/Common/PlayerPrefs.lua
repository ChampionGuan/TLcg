-- 本地存储
local Type = {
    AccountId = "AccountId",
    Password = "Password",
    LoginE = "LoginE",
    LoginM = "LoginM",
    LoginS = "LoginS",
    GraphicsQuality = "GraphicsQuality",
    AudioMusic = "AudioMusic",
    AudioEffect = "AudioEffect",
    AudioMusicOn = "AudioMusicOn",
    AudioEffectOn = "AudioEffectOn",
    PlatformLoggedon = "PlatformLoggedon",
    PlatformAgreement = "PlatformAgreement",
    AutoDeepfix = "AutoDeepfix"
}
-- c#层
local m_cshapeLocal = CSharp.PlayerPrefs.Instance

-- 本地数据
PlayerPrefs = {}

-- 清除所有
function PlayerPrefs.Clear()
    m_cshapeLocal:Clear()
end

-- 唯一key值
local function GetUnqiodKey()
    -- 服务器id和账户id
    return Common.Account.server.sid .. "_" .. Common.Account.id
end

-- 是否有key
function PlayerPrefs.HasKey(key)
    return m_cshapeLocal:HasKey(key)
end

-- 保存用户帐号
function PlayerPrefs.SaveUserAccount(id, password)
    -- 账户信息
    Common.Account.id = id
    Common.Account.passWord = password

    m_cshapeLocal:SaveString(Type.AccountId, id)
    m_cshapeLocal:SaveString(Type.Password, password)
end

-- 获取登陆E和M
function PlayerPrefs.GetUserAccount()
    local accountId = m_cshapeLocal:GetString(Type.AccountId)
    local password = m_cshapeLocal:GetString(Type.Password)

    Common.Account.id = accountId
    Common.Account.passWord = password

    return accountId, password
end

-- 保存登陆E和M
function PlayerPrefs.SaveLoginE2M(e, m, s)
    m_cshapeLocal:SaveString(Type.LoginE, e)
    m_cshapeLocal:SaveString(Type.LoginM, m)
    m_cshapeLocal:SaveString(Type.LoginS, s)
end

-- 获取登陆E和M
function PlayerPrefs.GetLoginE2M()
    return m_cshapeLocal:GetString(Type.LoginE), m_cshapeLocal:GetString(Type.LoginM)
end

-- 获取登陆S
function PlayerPrefs.GetLoginS()
    return m_cshapeLocal:GetString(Type.LoginS)
end

-- 保存画质状态
function PlayerPrefs.SaveGraphicsQuality(stat)
    m_cshapeLocal:SaveInt(Type.GraphicsQuality, stat)
end

-- 是否保存了画质状态
function PlayerPrefs.HasGraphicsQuality()
    return PlayerPrefs.HasKey(Type.GraphicsQuality)
end

-- 获取画质状态
function PlayerPrefs.GetGraphicsQuality()
    return m_cshapeLocal:GetInt(Type.GraphicsQuality)
end

-- 保存音乐状态
function PlayerPrefs.SetMusicSwitch(stat)
    m_cshapeLocal:SaveInt(Type.AudioMusicOn, stat)
end

-- 获取音乐状态
function PlayerPrefs.GetMusicSwitch()
    return m_cshapeLocal:GetInt(Type.AudioMusicOn)
end

-- 保存音效状态
function PlayerPrefs.SetEffectSwitch(stat)
    m_cshapeLocal:SaveInt(Type.AudioEffectOn, stat)
end

-- 获取音效状态
function PlayerPrefs.GetEffectSwitch()
    return m_cshapeLocal:GetInt(Type.AudioEffectOn)
end

-- 保存音乐音量
function PlayerPrefs.SetMusicVolume(volume)
    m_cshapeLocal:SaveFloat(Type.AudioMusic, volume * 2)
end

-- 获取音乐音量
function PlayerPrefs.GetMusicVolume()
    return m_cshapeLocal:GetFloat(Type.AudioMusic) * 0.5
end

-- 保存音效音量
function PlayerPrefs.SetEffectVolume(volume)
    m_cshapeLocal:SaveFloat(Type.AudioEffect, volume * 2)
end

-- 获取音效音量
function PlayerPrefs.GetEffectVolume()
    return m_cshapeLocal:GetFloat(Type.AudioEffect) * 0.5
end

-- 平台登录过
function PlayerPrefs.SavePlatformLoggedon(type)
    m_cshapeLocal:SaveString(string.format("%s_%s", Type.PlatformLoggedon, type), "0")
end

-- 获取是否平台登录过
function PlayerPrefs.GetPlatformLoggedon(type)
    local id = m_cshapeLocal:GetString(string.format("%s_%s", Type.PlatformLoggedon, type))
    if id ~= "" then
        return true
    else
        return false
    end
end

-- 保存平台用户协议
function PlayerPrefs.SavePlatformAgreement(type, agree)
    local id = "0"
    if not agree then
        id = "1"
    end

    m_cshapeLocal:SaveString(string.format("%s_%s", Type.PlatformAgreement, type), id)
end

-- 获取平台用户协议
function PlayerPrefs.GetPlatformAgreement(type)
    local id = m_cshapeLocal:GetString(string.format("%s_%s", Type.PlatformAgreement, type))
    if id == "1" then
        return false
    else
        return true
    end
end

-- 重度修复中
function PlayerPrefs.SaveAutoDeepfix(value)
    m_cshapeLocal:SaveInt(Type.AutoDeepfix, value and 2 or 1)
end

-- 重度修复中
function PlayerPrefs.GetAutoDeepfix()
    return m_cshapeLocal:GetInt(Type.AutoDeepfix) == 2
end

return PlayerPrefs
