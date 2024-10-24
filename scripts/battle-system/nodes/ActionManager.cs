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

	private enum WinState
	{
		None,
		Win,
		Lose
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
				GD.Print($"PickState: {value}\nPrevious: {previous}\n");
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
	private CharacterPicker SourceCharacterPicker => IsEnemyPicking switch
	{
		true => SelectedAttack.IsFriendly ? AllyCharacters : EnemyCharacters,
		false => SelectedAttack.IsFriendly ? EnemyCharacters : AllyCharacters,
	};

	[Export] public AttackPicker AttackPicker { get; set; }
	[Export] private AnimationPlayer AttackAnimationPlayer { get; set; }
	[Export] private AnimationPlayer StageAnimtationPlayer { get; set; }
	[Export] private AudioStreamPlayer ClickEffectPlayer { get; set; }
	[Export] private Godot.Collections.Array<BattleCharacter> FightOne { get; set; }
	[Export] private Godot.Collections.Array<BattleCharacter> FightTwo { get; set; }
	[Export] private Godot.Collections.Array<BattleCharacter> FightThree { get; set; }

	private int FightIndex { get; set; } = 0;

	private WinState CurrentWinState { get; set; } = WinState.None;

	private AutomaticTurnManager EnemyTurnManager { get; set; } = new AutomaticTurnManager();

	public event Action<PickState> OnPickStateChangedForward;
	public event Action<PickState> OnPickStateChangedBackward;
	public event Action<BattleCharacter> OnCharacterChanged;

	public override void _Ready()
	{
		ResetCharacters();

		OnPickStateChangedForward += PickStateChangedForward;
		OnPickStateChangedBackward += PickStateChangedBackward;
		OnCharacterChanged += CharacterChanged;

		SelectedPickState = PickState.PickingAttack;

		StageAnimtationPlayer.Play("enter");

		base._Ready();
	}

	public override void _Process(double delta)
	{
		if (EnemyTurnManager.ShouldProgressTurnState)
		{
			EnemyTurnManager.TurnStateHandled();

			if (AllyCharacters.IsAllDead())
			{
				CurrentWinState = WinState.Lose;
                StageAnimtationPlayer.Play("lose");
				SetProcessInput(true);
            }
			else
			{
                SelectedCharacterIndex++;
            }
        }

		base._Process(delta);
	}

    public override void _Input(InputEvent @event)
    {
		if (!IsProcessingInput())
		{
			return;
		}

		if (@event.IsActionPressed("ui_forward"))
		{
			if (CurrentWinState == WinState.Win || CurrentWinState == WinState.Lose)
			{
				QueueFree();
                Title pauseNode = GetNode<Title>("/root/TitleScene");
                if (IsInstanceValid(pauseNode))
                {
					pauseNode.Reset();
                }

				return;
            }

			PreviousPickStates.Push(SelectedPickState);
            SelectedPickState++;

            if (SelectedPickState == PickState.Complete)
			{
				PreviousPickStates.Clear();
			}
        }

        if (@event.IsActionPressed("ui_back"))
        {
			GoToPreviousPickState();
            ClickEffectPlayer.Play();
        }

        if (@event.IsActionPressed("ui_left") && @event.GetActionStrength("ui_left") == 1.0)
        {
			TraverseCurrentPickOption(-1);
            ClickEffectPlayer.Play();
        }

        if (@event.IsActionPressed("ui_right") && @event.GetActionStrength("ui_right") == 1.0)
        {
			TraverseCurrentPickOption(1);
            ClickEffectPlayer.Play();
        }

		if (@event.IsActionPressed("ui_cancel"))
		{
			Title pauseNode = GetNode<Title>("/root/TitleScene");
			if (IsInstanceValid(pauseNode))
			{
				pauseNode.Visible = true;
				pauseNode.SetProcessInput(true);
				pauseNode.SetToContinue();
                this.SetProcessInput(false);
			}
		}

        base._Input(@event);
    }

	private void ResetCharacters()
	{
        foreach (BattleCharacter character in AllyCharacters.GetAllActions())
        {
            Characters.Add(character);
            character.OnCharacterDead -= OnCharacterDeath;
            character.OnCharacterDead += OnCharacterDeath;
        }

        foreach (BattleCharacter character in EnemyCharacters.GetAllActions())
        {
            Characters.Add(character);
            character.OnCharacterDead -= OnCharacterDeath;
            character.OnCharacterDead += OnCharacterDeath;
        }

        Characters.Sort((a, b) => a.Speed.CompareTo(b.Speed)); // Sort by speed to determine turn order
    }

	private void TraverseCurrentPickOption(int direction)
	{
		switch (SelectedPickState)
		{
		case PickState.PickingAttack:
			{
				AttackPicker.SelectedActionIndex += Math.Sign(direction);
				TargetCharacterPicker.HighlightValidOptions(SelectedAttack);
				SourceCharacterPicker.DimAllExcept(AttackingCharacter);
                break;
			}
		case PickState.PickingTarget:
			{
				TargetCharacterPicker.ChangeSelectedAction(Math.Sign(direction));
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
				EnemyCharacters.ToggleSelector(false);
				AllyCharacters.ResetHighlights();
				AllyCharacters.ToggleSelector(false);
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
					break;
				}

				if (SelectedAttack.ContainsStatus(BattleUtils.StatusType.Passing))
				{
#if DEBUG
					GD.Print("Passing the turn");
#endif
					SelectedCharacterIndex++;
                    CompleteLoop();
					break;
				}

				TargetCharacterPicker.ToggleSelector(true);
				SourceCharacterPicker.ToggleSelector(false);
				AttackPicker.ToggleSelector(false);

                break;
			}
		case PickState.Complete:
			{
				if (!SelectedAttack.CanHitCharacter(DefendingCharacter))
				{
					GoToPreviousPickState();
					break;
				}

				if (SelectedAttack.ContainsStatus(BattleUtils.StatusType.Moving))
				{
					TargetCharacterPicker.ShoveCharacters(AttackingCharacter, DefendingCharacter);
                    SelectedCharacterIndex++;
                    CompleteLoop();
					return;
                }
				
                AttackingCharacter.StartAttack(() => {
                    if (SelectedAttack.GetStatusEffect(BattleUtils.StatusType.Pushing) is StatusEffect pushEffect)
                    {
                        TargetCharacterPicker.ShoveCharacters(DefendingCharacter, TargetCharacterPicker.GetFurthestCharacter(DefendingCharacter, pushEffect.Amount));
                    }

                    if (SelectedAttack.GetStatusEffect(BattleUtils.StatusType.Pulling) is StatusEffect pullEffect)
                    {
                        TargetCharacterPicker.ShoveCharacters(DefendingCharacter, TargetCharacterPicker.GetFurthestCharacter(DefendingCharacter, -pullEffect.Amount));
                    }

                    SelectedCharacterIndex++;
                    CompleteLoop();
                });
				DefendingCharacter.ApplyHit(SelectedAttack);
				AttackAnimationPlayer.Stop();
				AttackAnimationPlayer.Play("slide_out_right");
				TargetCharacterPicker.ToggleSelector(false);
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
                EnemyCharacters.ToggleSelector(false);
                AllyCharacters.ResetHighlights();
                AllyCharacters.ToggleSelector(false);
                AttackPicker.Set(AttackingCharacter);
                TraverseCurrentPickOption(0);
                break;
            }
        case PickState.PickingTarget:
            {
                TraverseCurrentPickOption(0);
                TargetCharacterPicker.ToggleSelector(true);
                SourceCharacterPicker.ToggleSelector(false);
                AttackPicker.ToggleSelector(false);
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
        AllyCharacters.ResetHighlights();
        EnemyCharacters.ResetHighlights();

        if (EnemyCharacters.ContainsAction(character))
		{
			SetProcessInput(false);
			character.StartSelect(DoEnemyTurn);
		}

		if (AllyCharacters.ContainsAction(character))
		{
			SetProcessInput(true);
            AttackPicker.Set(AttackingCharacter);
			character.StartSelect();
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
            StageAnimtationPlayer.Play("win");
			CurrentWinState = WinState.Win;
        }

		if (AllyCharacters.IsAllDead())
		{
#if DEBUG
			GD.Print("~~~~~~Lose!~~~~~~");
#endif
			StageAnimtationPlayer.Play("lose");
			CurrentWinState = WinState.Lose;
        }

        PreviousPickStates.Clear();
		SelectedPickState = PickState.PickingAttack;
#if DEBUG
		GD.Print("-----BEGINNING OF TURN-----");
#endif
		TraverseCurrentPickOption(0);
	}

	private void SetNewCharacters(StringName name)
	{
        StageAnimtationPlayer.AnimationFinished -= SetNewCharacters;
		SetNewCharacters();
	}

	private void SetNewCharacters()
	{
		switch (FightIndex)
		{
		case 0:
			{
				EnemyCharacters.Reset(FightOne);
				FightIndex = 1;
				break;
			}
		case 1:
			{
				EnemyCharacters.Reset(FightTwo);
				FightIndex = 2;
				break;
			}
		case 2:
			{
				EnemyCharacters.Reset(FightThree);
				FightIndex = 3;
				break;
			}
		default:
			QueueFree();
			break;
		}


		StageAnimtationPlayer.Play("enter");
		ResetCharacters();
	}

	private void DoEnemyTurn()
	{
        EnemyTurnManager.Set(AttackingCharacter, EnemyCharacters, AllyCharacters);
		CurrentCharacterPicker.DimAllExcept(AttackingCharacter);
        EnemyTurnManager.ProcessTurn();
        AttackAnimationPlayer.Stop();
        AttackAnimationPlayer.Play("slide_out_left");
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
		// SelectedCharacterIndex = _selectedCharacterIndex;

#if DEBUG
		GD.Print($"Killed character:\n {character}");
#endif
	}
}