-- 协议黑名单(此名单下，都不转菊花,当value为true表示断线时弹框提示)
local BlacklistId = {
    -- c2s_gm
    ["3-1"] = false,
    -- c2s_heart_beat
    ["5-1"] = false,
    -- c2s_background_heart_beat
    ["5-35"] = false,
    -- c2s_background_weakup
    ["5-36"] = false,
    -- c2s_client_log
    ["5-7"] = false,
    -- c2s_sync_time
    ["5-8"] = false,
    -- c2s_update_self_view
    ["7-148"] = true,
    -- c2s_close_view
    ["7-150"] = true,
    -- c2s_request_military_push
    ["7-165"] = true,
    -- c2s_request_team_count
    ["22-2"] = true,
    -- c2s_request_team_list
    ["22-5"] = true,
    -- c2s_request_invite_list
    ["22-37"] = true,
    -- c2s_request_team_detail
    ["22-39"] = true,
    -- c2s_request_rank
    ["24-23"] = true,
    -- c2s_request_self_rank
    ["24-26"] = true,
    -- c2s_steal_log_list
    ["38-39"] = true,
    -- c2s_can_steal_list
    ["38-48"] = true
}

-- 预登录和完整登录之间的协议
local LoginProtoId = {
    -- c2s_create_hero
    ["1-3"] = true,
    -- c2s_login
    ["1-7"] = true,
    -- c2s_set_tutorial_progress
    ["1-18"] = true,
    -- c2s_loaded
    ["1-10"] = true,
    -- c2s_config
    ["5-3"] = true,
    -- c2s_ping
    ["5-15"] = true,
    -- c2s_client_version
    ["5-17"] = true,
    -- c2s_configlua
    ["5-76"] = true,
}

return {
    BlacklistId = BlacklistId,
    LoginProtoId = LoginProtoId
}
