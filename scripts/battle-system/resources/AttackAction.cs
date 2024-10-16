using System;
using System.Collections.Generic;
using Godot;

public partial class AttackAction : Resource
{
    [Export] public int[] CanHitSlots { get; private set; }
    [Export] public int[] UsableFromSlots { get; private set; }
    [Export] public bool IsFriendly { get; private set; }

    [Export] public float Damage { get; private set; }
    [Export] public float Accuracy { get; private set; }

    [Export] public StatusEffect StatusEffect { get; private set; }

    [Export] public Texture2D Texture { get; private set; }
}
