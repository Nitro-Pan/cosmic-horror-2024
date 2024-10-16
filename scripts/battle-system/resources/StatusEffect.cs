using System;
using BattleUtils;
using Godot;

public partial class StatusEffect : Resource
{
    [Export] public StatusType EffectType = StatusType.None;
    [Export] public int Duration = 0;
}

