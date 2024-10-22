using Godot;
using System;
using System.Collections.Generic;

public abstract partial class BattleCharacter : Node2D
{
	public class Stats
	{
		public float Health = 0.0f;
		public float Speed = 0.0f;
		public float Armour = 0.0f;
		public int BattlePosition = 0;
		public List<StatusEffect> StatusEffects = new List<StatusEffect>();
		public BattleUtils.HitType LastHitType = BattleUtils.HitType.None;
	}

	[Export] public float BaseHealth { get; protected set; }
	[Export] public float BaseSpeed { get; protected set; }
	[Export] public float BaseArmour { get; protected set; }
    [Export] public Godot.Collections.Array<AttackAction> BaseAttacks { get; protected set; }
	[Export] public Label HealthLabel { get; protected set; }
	[Export] protected CharacterAnimation CharacterSprite { get; set; }
	[Export] private AnimationPlayer Animator { get; set; }
	[Export] private Color SelectableColor { get; set; }
	[Export] private Color UnselectableColor { get; set; }

	protected Stats CurrentStats { get; set; }
	public float Speed => CurrentStats.Speed;
	public float Health => CurrentStats.Health;
	public float Armour => CurrentStats.Armour;
	public int BattlePosition => CurrentStats.BattlePosition;

	public event Action<BattleCharacter> OnCharacterDead;

    public override void _Ready()
    {
        CurrentStats = new Stats
        {
            Health = BaseHealth,
            Speed = BaseSpeed,
            Armour = BaseArmour,
        };

        HealthLabel.Text = $"{Health}/{BaseHealth}";

        base._Ready();
    }


    public override void _Process(double delta)
    {
		base._Process(delta);
    }

    public Stats ApplyHit(AttackAction attack)
	{
		CurrentStats.LastHitType = BattleUtils.HitType.Hit;

		RandomNumberGenerator random = new RandomNumberGenerator();
		if (random.Randf() > attack.Accuracy)
		{
			CurrentStats.LastHitType = BattleUtils.HitType.Missed;
			return CurrentStats;
		}

		float lastHealth = CurrentStats.Health;
        CurrentStats.Health -= attack.Damage * 1 / CurrentStats.Armour;

		if (lastHealth == CurrentStats.Health)
		{
			CurrentStats.LastHitType = BattleUtils.HitType.Blocked;
		}

		if (CurrentStats.StatusEffects.Find((effect) => effect.EffectType == BattleUtils.StatusType.Dying) != null)
		{
            CurrentStats.Health = 1;
            CurrentStats.StatusEffects.Add(new StatusEffect { EffectType = BattleUtils.StatusType.Dying, Duration = 9999 });
		}

		if (Health <= 0)
		{
			OnCharacterDead.Invoke(this);
			OnDeath();
		}

		CurrentStats.Health = Math.Clamp(Health, 0, BaseHealth);

        SetUI();

        return CurrentStats;
	}

	public void StartAttack()
	{
		Animator.Play("attack");
	}

	public void EndAttack()
	{
		Animator.Play("RESET");
	}

	public void SetSelectableState(bool isSelectable)
	{
		CharacterSprite.SetColour(isSelectable ? SelectableColor : UnselectableColor);
	}

	public void SetBattlePosition(int battlePosition)
	{
		CurrentStats.BattlePosition = battlePosition;
	}

	public IReadOnlyList<AttackAction> GetValidAttacks()
	{
		List<AttackAction> validAttacks = new List<AttackAction>();

		foreach (AttackAction action in BaseAttacks)
		{
			foreach (int slot in action.UsableFromSlots)
			{
				if (slot == BattlePosition)
				{
					validAttacks.Add(action);
					break;
				}
			}
		}

		return validAttacks;
	}

	private void SetUI()
	{
		HealthLabel.Text = $"{Health}/{BaseHealth}";
	}

	private void OnDeath()
	{
		CharacterSprite.FlipV = true;
    }

	public bool IsDead()
	{
		return Health <= 0;
	}

	public override string ToString()
	{
		return $"Character:\n\tHealth: {Health}\n\tSpeed: {Speed}\n\tArmour: {Armour}\n\tBattlePosition: {BattlePosition}\n";
	}
}
