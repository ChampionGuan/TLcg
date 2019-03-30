return {
    [1] = {
        Id = 1,
        Type = Define.LevelType.Bootup,
        LogicScript = "Game.LevelLogic.BootupLevelLogic",
        SceneName = "Bootup",
        AudioId = 1
    },
    [2] = {
        Id = 2,
        Type = Define.LevelType.MainCity,
        LogicScript = "Game.LevelLogic.MaincityLevelLogic",
        SceneName = "Maincity",
        AudioId = 2
    },
    [3] = {
        Id = 3,
        Type = Define.LevelType.Battle,
        LogicScript = "Game.LevelLogic.BattleLevelLogic",
        SceneName = "Battle",
        AudioId = 3
    }
}
