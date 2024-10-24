using Godot;
using System;
using System.Collections.Generic;

public partial class Title : Node2D
{
	private enum TitleState
	{
		Title,
		Credits,
		Levels
	}

	[Export] private UiPicker TitleActions { get; set; }
	[Export] private UiPicker LevelSelectActions { get; set; }
	[Export] private AnimationPlayer ActionAnimations { get; set; }
	[Export] private Label PlayLabel { get; set; }
	[Export] private Polygon2D Fader { get; set; }

	private Node BattleScene { get; set; }
	private bool HasEntered { get; set; }
	private TitleState CurrentState { get; set; } = TitleState.Title;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        IReadOnlyList<UiAction> titleActions = TitleActions.GetAllActions();
		titleActions[ 0 ].OnClick += OnPlayButtonPressed;
		titleActions[ 1 ].OnClick += OnCreditsButtonPressed;
		titleActions[ 2 ].OnClick += OnExitButtonPressed;

		IReadOnlyList<UiAction> levelActions = LevelSelectActions.GetAllActions();
		levelActions[ 0 ].OnClick += OnLevelSelect;
		levelActions[ 1 ].OnClick += OnLevelSelect;
		levelActions[ 2 ].OnClick += OnLevelSelect;

		base._Ready();
	}

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
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
			switch (CurrentState)
			{
			case TitleState.Title:
				{
                    if (HasEntered)
                    {
                        TitleActions.ConfirmAction();
                    }
                    else
                    {
                        if (ActionAnimations.IsPlaying())
                        {
                            ActionAnimations.Advance(5);
                        }
                        else
                        {
                            ActionAnimations.Play("enter_title");
                        }
                    }
					break;
                }
			case TitleState.Levels:
				{
					LevelSelectActions.ConfirmAction();
					break;
				}
			case TitleState.Credits:
			default:
				break;
			}
			
			
        }

		if (@event.IsActionPressed("ui_back"))
		{
			switch (CurrentState)
			{
			case TitleState.Title:
				{
					break;
				}
			case TitleState.Credits:
				{
					break;
				}
			case TitleState.Levels:
				{
					TitleActions.ToggleSelector(true);
                    ActionAnimations.Play("return_title_from_levels");
					CurrentState = TitleState.Title;
                    break;
				}
			}
		}

        if (@event.IsActionPressed("ui_up") && @event.GetActionStrength("ui_up") == 1.0)
        {
			switch (CurrentState)
			{
			case TitleState.Title:
				{
                    TitleActions.ChangeSelectedAction(-1);
                    break;
				}
			case TitleState.Levels:
				{
					LevelSelectActions.ChangeSelectedAction(-1);
					break;
				}
			default:
				break;
			}
        }

        if (@event.IsActionPressed("ui_down") && @event.GetActionStrength("ui_down") == 1.0)
        {
            switch (CurrentState)
            {
            case TitleState.Title:
                {
                    TitleActions.ChangeSelectedAction(1);
                    break;
                }
            case TitleState.Levels:
                {
					LevelSelectActions.ChangeSelectedAction(1);
                    break;
                }
            default:
                break;
            }
        }

        base._Input(@event);
    }

	private void OnEnterAnimationFinished()
	{
		HasEntered = true;
		TitleActions.Reset();
	}

	private void OnContinueButtonPressed()
	{
        this.Visible = false;
        SetProcessInput(false);
        BattleScene.SetProcessInput(true);
    }

	private void OnPlayButtonPressed()
	{
		CurrentState = TitleState.Levels;
		LevelSelectActions.ToggleSelector(true);
        LevelSelectActions.SelectedActionIndex = 0;
		TitleActions.ToggleSelector(false);
		ActionAnimations.Play("enter_levels");
    }

	private void OnLevelSelect()
	{
        if (!IsInstanceValid(BattleScene))
        {
            BattleScene = ResourceLoader.Load<PackedScene>("res://resources/battle-system/battles/first_battle.tscn").Instantiate();
            
        }

		ActionAnimations.Play("stop_menu_sound");
		SetProcessInput(false);
    }

	private void OnLevelTransitionFinish()
	{
		if (IsInstanceValid(BattleScene))
		{
            GetTree().Root.AddChild(BattleScene);
        }

        this.Visible = false;
        BattleScene.SetProcessInput(true);
    }

	private void OnCreditsButtonPressed() 
	{
		
	}

	private void OnExitButtonPressed() 
	{
		GetTree().Quit();
	}

	public void SetToContinue()
	{
        IReadOnlyList<UiAction> actions = TitleActions.GetAllActions();

		actions[ 0 ].OnClick -= OnPlayButtonPressed;
		actions[ 0 ].OnClick -= OnContinueButtonPressed;
		actions[ 0 ].OnClick += OnContinueButtonPressed;

		TitleActions.ToggleSelector(true);
		CurrentState = TitleState.Title;
		PlayLabel.Text = "CONTINUE";
		Fader.Visible = false;
		ActionAnimations.Play("enter_title");
		ActionAnimations.Advance(1000);
    }

	private void SetTitleState(TitleState state)
	{
		CurrentState = state;
	}
}
