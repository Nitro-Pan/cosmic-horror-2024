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

    [Export] public StatusEffect[] StatusEffects { get; private set; } = new StatusEffect[0];

    [Export] public Texture2D Texture { get; private set; }

    public bool CanPickFromCharacter(BattleCharacter character)
    { 
        foreach (int slot in UsableFromSlots)
        {
            if (character.BattlePosition == slot)
            {
                return true;
            }
        }

        return false;
    }

    public bool CanHitCharacter(BattleCharacter character)
    {
        if (character.IsDead())
        {
            return false;
        }

        foreach (int slot in CanHitSlots)
        {
            if (character.BattlePosition == slot)
            {
                return true;
            }
        }

        return false;
    }

    public override string ToString()
    {
        string output = $"Attack:\n\tDamage: {Damage}\n\tAccuracy: {Accuracy}\n\tCan Hit: ";

        foreach (int slot in CanHitSlots)
        {
            output += $"{slot}, ";
        }

        output += "\n\tUsed From: ";

        foreach (int slot in UsableFromSlots)
        {
            output += $"{slot}, ";
        }

        output += $"\n\tStatusEffects: ";

        foreach (StatusEffect effect in StatusEffects)
        {
            output += $"{effect}, ";
        }

        return output;
    }
}
