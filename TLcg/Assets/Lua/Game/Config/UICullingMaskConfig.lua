return {
    [Define.LevelType.MainCity] = bit.bor(
        bit.lshift(1, CSharp.LayerMask.NameToLayer("Default")),
        bit.lshift(1, CSharp.LayerMask.NameToLayer("NPC"))
    ),
    [Define.LevelType.Battle] = bit.bor(
        bit.lshift(1, CSharp.LayerMask.NameToLayer("Default")),
        bit.lshift(1, CSharp.LayerMask.NameToLayer("Troops")),
        bit.lshift(1, CSharp.LayerMask.NameToLayer("FightMap"))
    )
}
