using Godot;
using System;
using static ActionManager;

public partial class Title : Node2D
{
	[Export] private UiPicker Actions { get; set; }
	[Export] private AnimationPlayer ActionAnimations { get; set; }

	private Node BattleScene { get; set; }
	private bool HasEntered { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var actions = Actions.GetAllActions();

		actions[ 0 ].OnClick += OnPlayButtonPressed;
		actions[ 1 ].OnClick += OnCreditsButtonPressed;
		actions[ 2 ].OnClick += OnExitButtonPressed;

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
			if (HasEntered)
			{
                Actions.ConfirmAction();
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
			
        }

        if (@event.IsActionPressed("ui_up") && @event.GetActionStrength("ui_up") == 1.0)
        {
			Actions.ChangeSelectedAction(-1);
        }

        if (@event.IsActionPressed("ui_down") && @event.GetActionStrength("ui_down") == 1.0)
        {
			Actions.ChangeSelectedAction(1);
        }

        base._Input(@event);
    }

	private void OnEnterAnimationFinished()
	{
		HasEntered = true;
		Actions.Reset();
	}

	private void OnContinueButtonPressed()
	{
        this.Visible = false;
        SetProcessInput(false);
        BattleScene.SetProcessInput(true);
    }

	private void OnPlayButtonPressed()
	{
		if (!IsInstanceValid(BattleScene))
		{
            BattleScene = ResourceLoader.Load<PackedScene>("res://resources/battle-system/battles/first_battle.tscn").Instantiate();
            GetTree().Root.AddChild(BattleScene);
        }

		this.Visible = false;
		SetProcessInput(false);
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
        var actions = Actions.GetAllActions();

		actions[ 0 ].OnClick -= OnPlayButtonPressed;
		actions[ 0 ].OnClick -= OnContinueButtonPressed;
		actions[ 0 ].OnClick += OnContinueButtonPressed;
    }
}
