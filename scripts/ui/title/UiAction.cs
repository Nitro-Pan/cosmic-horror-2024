using System;
using Godot;

public partial class UiAction : Sprite2D
{
    private Label ButtonText { get; set; }

    public event Action OnClick;

    public void Confirm()
    {
        OnClick?.Invoke();
    }
}
