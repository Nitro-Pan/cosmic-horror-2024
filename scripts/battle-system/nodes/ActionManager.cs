using Godot;
using System;
using System.Collections.Generic;

public partial class ActionManager : Node
{
	public enum PickState
	{
		PickingAttack,
		PickingTarget,
		Complete
	}

	private PickState _selectedPickState;
	private PickState SelectedPickState
	{
		get => _selectedPickState;
		set
		{
			Action<PickState> onPickChanged = PreviousPickStates.Count > 0 && (value == PreviousPickStates.Peek()) ? OnPickStateChangedForward : OnPickStateChangedBackward;
            _selectedPickState = value;

            onPickChanged?.Invoke(_selectedPickState);
		}
	}
	private Stack<PickState> PreviousPickStates { get; set; } = new Stack<PickState>();
	private bool IsEnemyPicking { get; set; }

	private List<BattleCharacter> Characters { get; set; } = new List<BattleCharacter>();
	private int SelectedCharacterIndex { get; set; }
	private AttackAction SelectedAttack => AttackPicker.SelectedAction;

	[Export] public CharacterPicker EnemyCharacters { get; set; }
	[Export] public CharacterPicker AllyCharacters { get; set; }
	[Export] public AttackPicker AttackPicker { get; set; }

	public event Action<PickState> OnPickStateChangedForward;
	public event Action<PickState> OnPickStateChangedBackward;

	public override void _Ready()
	{
		foreach (BattleCharacter character in EnemyCharacters.GetAllActions())
		{
			Characters.Add(character);
		}

		foreach (BattleCharacter character in AllyCharacters.GetAllActions())
		{
			Characters.Add(character);
		}

		Characters.Sort((a, b) => a.Speed.CompareTo(b.Speed)); // Sort by speed to determine turn order

		OnPickStateChangedForward += PickStateChangedForward;
		OnPickStateChangedBackward += PickStateChangedBackward;
		SelectedPickState = PickState.PickingAttack;

		base._Ready();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
	}

    public override void _Input(InputEvent @event)
    {
		if (@event.IsActionPressed("ui_select"))
		{
			PickState lastState = SelectedPickState;
            SelectedPickState++;
            PreviousPickStates.Push(lastState);

            if (SelectedPickState == PickState.Complete)
			{
				PreviousPickStates.Clear();
			}
		}

        if (@event.IsActionPressed("ui_cancel"))
        {
			if (PreviousPickStates.TryPop(out PickState lastState))
			{
				SelectedPickState = lastState;
            }
        }

        if (@event.IsActionPressed("ui_left"))
        {
			TraverseCurrentPickOption(-1);
        }

        if (@event.IsActionPressed("ui_right"))
        {
			TraverseCurrentPickOption(1);
        }

        base._Input(@event);
    }

	private void TraverseCurrentPickOption(int direction)
	{
		switch (SelectedPickState)
		{
		case PickState.PickingAttack:
			{
                AttackPicker.SelectedActionIndex += Math.Clamp(direction, -1, 1);
                break;
			}
		case PickState.PickingTarget:
			{
                CharacterPicker characterSelector = IsEnemyPicking switch {
					true => SelectedAttack.IsFriendly ? EnemyCharacters : AllyCharacters,
					false => SelectedAttack.IsFriendly ? AllyCharacters : EnemyCharacters,
                };
                characterSelector.SelectedActionIndex += Math.Clamp(direction, -1, 1);
                break;
			}
        case PickState.Complete:
			{
                break;
            }
		}
	}

	private void PickStateChangedForward(PickState state)
	{
		switch (state)
		{
        case PickState.PickingAttack:
			{
                BattleCharacter attackingCharacter = Characters[ SelectedCharacterIndex ];
                AttackPicker.Set(attackingCharacter.BaseAttacks);
                break;
			}
		case PickState.PickingTarget:
			{
				break;
			}
		case PickState.Complete:
			{
				break;
			}
		default:
			{
                break;
            }
		}
	}

    private void PickStateChangedBackward(PickState state)
    {
        switch (state)
        {
        case PickState.PickingAttack:
            {
                BattleCharacter attackingCharacter = Characters[ SelectedCharacterIndex ];
                AttackPicker.Set(attackingCharacter.BaseAttacks);
                break;
            }
        case PickState.PickingTarget:
            {
                break;
            }
        case PickState.Complete:
            {
                break;
            }
        default:
            {
                break;
            }
        }
    }
}