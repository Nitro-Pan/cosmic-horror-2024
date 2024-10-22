using System;
using System.Collections;
using System.Collections.Generic;
using Godot;

public partial class AttackPicker : ActionPicker<AttackAction>
{
    [Export] protected override Godot.Collections.Array<AttackAction> ActionList { get; set; }
    [Export] private Sprite2D[] Buttons { get; set; }
    [Export] private Color DisabledColour { get; set; }
    [Export] private Color EnabledColour { get; set; } = new Color(1.0f, 1.0f, 1.0f);

    private BattleCharacter SourceCharacter { get; set; }

    public override void _Ready()
    {
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

    public void Set(BattleCharacter source)
    {
        Set(source.BaseAttacks);

        SourceCharacter = source;

        for (int i = 0; i < ActionList.Count && i < Buttons.Length; i++)
        {
            Buttons[ i ].Modulate = DisabledColour;
            foreach (int validPosition in ActionList[i].UsableFromSlots)
            {
                if (source.BattlePosition == validPosition)
                {
                    Buttons[ i ].Modulate = EnabledColour;
                    break;
                }
            }
        }

        ToggleSelector(true);
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

    protected override void OnActionChanged(int index)
    { 
        SelectorTargetPosition = Buttons[ index ].Position;
    }
}

