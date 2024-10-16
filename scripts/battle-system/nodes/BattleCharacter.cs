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
		public List<StatusEffect> StatusEffects = new List<StatusEffect>();
		public BattleUtils.HitType LastHitType = BattleUtils.HitType.None;
	}

	[Export] public float BaseHealth { get; protected set; }
	[Export] public float BaseSpeed { get; protected set; }
	[Export] public float BaseArmour { get; protected set; }
    [Export] public Godot.Collections.Array<AttackAction> BaseAttacks { get; protected set; }

	protected Stats CurrentStats { get; set; }
	public float Speed => CurrentStats.Speed;
	public float Health => CurrentStats.Health;
	public float Armour => CurrentStats.Armour;


    public override void _Ready()
    {
        CurrentStats = new Stats
        {
            Health = BaseHealth,
            Speed = BaseSpeed,
            Armour = BaseArmour,
        };

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

		return CurrentStats;
	}
}
