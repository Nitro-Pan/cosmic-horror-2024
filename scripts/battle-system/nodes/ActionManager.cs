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
			bool backwards = PreviousPickStates.Count > 0 && (value == PreviousPickStates.Peek());

#if DEBUG
			if (PreviousPickStates.TryPeek(out PickState previous))
			{
                GD.Print($"PickState: { value }\nPrevious: { previous }\n");
            }
			
#endif

			Action<PickState> onPickChanged = backwards ? OnPickStateChangedBackward : OnPickStateChangedForward;
            _selectedPickState = value;
            onPickChanged?.Invoke(_selectedPickState);
		}
	}
	private Stack<PickState> PreviousPickStates { get; set; } = new Stack<PickState>();
	private bool IsEnemyPicking { get; set; }

	private List<BattleCharacter> Characters { get; set; } = new List<BattleCharacter>();
	private int _selectedCharacterIndex;
	private int SelectedCharacterIndex
	{
		get => _selectedCharacterIndex;
		set
		{
			value %= Characters.Count;
			IsEnemyPicking = EnemyCharacters.ContainsAction(Characters[ value ]);
			_selectedCharacterIndex = value;
			CurrentCharacterPicker.Select(Characters[ _selectedCharacterIndex ]);
#if DEBUG
			GD.Print($"Selected Character for attack:\n\tEnemy? {IsEnemyPicking}\n\t{Characters[ _selectedCharacterIndex ]}");
#endif
			OnCharacterChanged.Invoke(AttackingCharacter);
		}
	}
	private BattleCharacter AttackingCharacter => Characters[ _selectedCharacterIndex ];
	private AttackAction SelectedAttack => AttackPicker.SelectedAction;
	private BattleCharacter DefendingCharacter { get; set; }

	[Export] public CharacterPicker EnemyCharacters { get; set; }
	[Export] public CharacterPicker AllyCharacters { get; set; }
	private CharacterPicker CurrentCharacterPicker => IsEnemyPicking ? EnemyCharacters : AllyCharacters;
	private CharacterPicker TargetCharacterPicker => IsEnemyPicking switch
    {
        true => SelectedAttack.IsFriendly ? EnemyCharacters : AllyCharacters,
        false => SelectedAttack.IsFriendly ? AllyCharacters : EnemyCharacters,
    };
    [Export] public AttackPicker AttackPicker { get; set; }

	private AutomaticTurnManager EnemyTurnManager { get; set; } = new AutomaticTurnManager();

	public event Action<PickState> OnPickStateChangedForward;
	public event Action<PickState> OnPickStateChangedBackward;
	public event Action<BattleCharacter> OnCharacterChanged;

	public override void _Ready()
	{
        foreach (BattleCharacter character in AllyCharacters.GetAllActions())
        {
            Characters.Add(character);
			character.OnCharacterDead += OnCharacterDeath;
        }

        foreach (BattleCharacter character in EnemyCharacters.GetAllActions())
		{
			Characters.Add(character);
			character.OnCharacterDead += OnCharacterDeath;
		}

		Characters.Sort((a, b) => a.Speed.CompareTo(b.Speed)); // Sort by speed to determine turn order

		OnPickStateChangedForward += PickStateChangedForward;
		OnPickStateChangedBackward += PickStateChangedBackward;
		OnCharacterChanged += CharacterChanged;

		SelectedPickState = PickState.PickingAttack;

		base._Ready();
	}

	public override void _Process(double delta)
	{
		if (EnemyTurnManager.ShouldProgressTurnState)
		{
			EnemyTurnManager.TurnStateHandled();
			SelectedCharacterIndex++;
			AttackPicker.Set(AttackingCharacter);
		}

		base._Process(delta);
	}

    public override void _Input(InputEvent @event)
    {
		if (@event.IsActionPressed("ui_select"))
		{
			PreviousPickStates.Push(SelectedPickState);
            SelectedPickState++;

            if (SelectedPickState == PickState.Complete)
			{
				PreviousPickStates.Clear();
			}
		}

        if (@event.IsActionPressed("ui_cancel"))
        {
			GoToPreviousPickState();
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
				TargetCharacterPicker.HighlightValidOptions(SelectedAttack);
                break;
			}
		case PickState.PickingTarget:
			{
                TargetCharacterPicker.SelectedActionIndex += Math.Clamp(direction, -1, 1);
				DefendingCharacter = TargetCharacterPicker.SelectedAction;
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
				EnemyCharacters.ResetHighlights();
				AllyCharacters.ResetHighlights();
                AttackPicker.Set(AttackingCharacter);
                TraverseCurrentPickOption(0);
                break;
			}
		case PickState.PickingTarget:
			{
                TraverseCurrentPickOption(0);

                if (!SelectedAttack.CanPickFromCharacter(AttackingCharacter))
				{
					GoToPreviousPickState();
				}

                break;
			}
		case PickState.Complete:
			{
				if (!SelectedAttack.CanHitCharacter(DefendingCharacter))
				{
					GoToPreviousPickState();
					break;
				}

                AttackingCharacter.StartAttack();
                SelectedCharacterIndex++;
				DefendingCharacter.ApplyHit(SelectedAttack);
				CompleteLoop();
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
				EnemyCharacters.ResetHighlights();
				AllyCharacters.ResetHighlights();
                BattleCharacter attackingCharacter = Characters[ SelectedCharacterIndex ];
                AttackPicker.Set(attackingCharacter);
                TraverseCurrentPickOption(0);
                break;
            }
        case PickState.PickingTarget:
            {
                TraverseCurrentPickOption(0);
                break;
            }
        case PickState.Complete:
            {
				GD.PushWarning("Hit Complete state from backwards, check code");
				CompleteLoop();
                break;
            }
        default:
            {
                break;
            }
        }
    }

	private void CharacterChanged(BattleCharacter character)
	{
		if (EnemyCharacters.ContainsAction(character))
		{
			SetProcessInput(false);
			DoEnemyTurn();
			AllyCharacters.ResetHighlights();
			SetProcessInput(true);
		}
	}

	private void CompleteLoop()
	{
#if DEBUG
        GD.Print("-----END OF TURN-----");
#endif
        if (EnemyCharacters.IsAllDead())
		{
#if DEBUG
            GD.Print("~~~~~~Win!~~~~~~");
#endif
        }

		if (AllyCharacters.IsAllDead())
		{
#if DEBUG
			GD.Print("~~~~~~Lose!~~~~~~");
#endif
        }

        PreviousPickStates.Clear();
		SelectedPickState = PickState.PickingAttack;
#if DEBUG
		GD.Print("-----BEGINNING OF TURN-----");
#endif
	}

	private void DoEnemyTurn()
	{
        EnemyTurnManager.Set(AttackingCharacter, EnemyCharacters, AllyCharacters);
        EnemyTurnManager.ProcessTurn();
		// SelectedCharacterIndex++;
    }

    private bool IsCharacterFriendly(BattleCharacter character)
	{
		foreach (BattleCharacter check in AllyCharacters.GetAllActions())
		{
			if (character == check)
			{
				return true;
			}
		}

		return false;
	}

	private bool IsAllCharactersDead()
	{
		return AllyCharacters.IsAllDead() && EnemyCharacters.IsAllDead();
	}

	private bool GoToPreviousPickState()
	{
        if (PreviousPickStates.TryPop(out PickState lastState))
        {
            SelectedPickState = lastState;
			return true;
        }

		return false;
    }

	private void OnCharacterDeath(BattleCharacter character)
	{
		Characters.Remove(character);
		SelectedCharacterIndex = _selectedCharacterIndex;

#if DEBUG
		GD.Print($"Killed character:\n {character}");
#endif
	}
}