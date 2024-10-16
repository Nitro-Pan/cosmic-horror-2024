using System;
using System.Collections.Generic;
using Godot;

public partial class CharacterPicker : ActionPicker<BattleCharacter>
{
    [Export] protected override Godot.Collections.Array<BattleCharacter> ActionList { get; set; }
    [Export] private Color DisabledColour { get; set; }

    public override void _Ready()
    {
        OnSelectedActionChanged += OnActionChanged;
        base._Ready();
    }

    public override void Set(IList<BattleCharacter> actionList)
    {
        base.Set(actionList);
        OnActionChanged(SelectedActionIndex);
    }

    protected override void PrintSelectedAction(int action)
    {
        GD.Print($"Selected Character: {ActionList[ SelectedActionIndex ]}");
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
        SelectorSprite.Position = ActionList[ index ].Position;
    }
}
