using Godot;
using System;
using System.Collections.Generic;

public partial class UiPicker : ActionPicker<UiAction>
{
    [Export] protected override Godot.Collections.Array<UiAction> ActionList { get; set; }

    public override void _Ready()
    {
        Reset();
        base._Ready();
    }

    public void Reset()
    {
        SelectorTargetPosition = ActionList[ 0 ].Position;
    }

    protected override void OnActionChanged(int index)
    {
        SelectorTargetPosition = ActionList[ index ].Position;
    }

    public override void ConfirmAction()
    {
        SelectedAction.Confirm();
        base.ConfirmAction();
    }
}
