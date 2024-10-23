using Godot;
using System;

public partial class AttackDisplay : Node
{
	[Export] private HitFiller HitSlotsDisplay { get; set; }
	[Export] private Label DamageLabel { get; set; }
	[Export] private Label AccuracyLabel { get; set; }
	[Export] private Label EffectLabel { get; set; }

	public void Set(AttackAction attack)
	{
		HitSlotsDisplay.Set(attack.CanHitSlots);
		DamageLabel.Text = $"Damage: {attack.Damage}";
		AccuracyLabel.Text = $"Accuracy: {attack.Accuracy}";
		EffectLabel.Text = "HIT EM!";

		if (attack.StatusEffects.Length > 0)
		{
            EffectLabel.Text = $"{ GetStatusText(attack.StatusEffects[0]) } ";
			if (attack.StatusEffects[0].Amount > 0)
			{
				EffectLabel.Text += $"{ attack.StatusEffects[ 0 ].Amount }";
			}
        }
	}

	private string GetStatusText(StatusEffect effect)
	{
		return effect.EffectType switch
        {
            BattleUtils.StatusType.None => string.Empty,
            BattleUtils.StatusType.Dying => string.Empty,
            BattleUtils.StatusType.Moving => "Move",
            BattleUtils.StatusType.Passing => "Pass the turn",
            BattleUtils.StatusType.Pushing => "Pushes",
            BattleUtils.StatusType.Pulling => "Pulls",
            _ => string.Empty,
        };
	}
}
