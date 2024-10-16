using System;
using System.Collections;
using System.Collections.Generic;
using Godot;

public partial class AttackPicker : ActionPicker<AttackAction>
{
    [Export] protected override Godot.Collections.Array<AttackAction> ActionList { get; set; }
    [Export] private Sprite2D[] Buttons { get; set; }
    [Export] private Color DisabledColour { get; set; }

    public override void _Ready()
    {
        OnSelectedActionChanged += OnActionChanged;
        base._Ready();
    }


    public override void Set(IList<AttackAction> actionList)
    {
        base.Set(actionList);

        for (int i = 0; i < ActionList.Count || i < Buttons.Length; i++)
        {
            if (i < ActionList.Count && i < Buttons.Length)
            {
                Buttons[ i ].Texture = ActionList[ i ].Texture;
                continue;
            }

            Buttons[ i ].Modulate = DisabledColour;
        }

        OnActionChanged(SelectedActionIndex);
    }

    protected override void PrintSelectedAction(int action)
    {
        GD.Print($"Selected Attack: {ActionList[ SelectedActionIndex ]}");
    }

    public override void CancelAction()
    {
        base.CancelAction();
    }

    public override void ConfirmAction()
    {
        base.ConfirmAction();
    }

    private void OnActionChanged(int index)
    { 
        SelectorSprite.Position = Buttons[ index ].Position;
    }
}

