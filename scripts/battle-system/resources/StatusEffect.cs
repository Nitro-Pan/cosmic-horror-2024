using System;
using BattleUtils;
using Godot;

public partial class StatusEffect : Resource
{
    [Export] public StatusType EffectType { get; set; } = StatusType.None;
    [Export] public int Duration { get; set; } = 0;
    [Export] public int Amount { get; set; } = 0;
}

