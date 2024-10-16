using Godot;
using System;
using System.Collections.Generic;

public abstract partial class ActionPicker<[MustBeVariant] T> : Node where T : GodotObject
{
    private const int DEFAULT_MAX_ACTIONS = 4;

    [Export] protected Sprite2D SelectorSprite;

    protected abstract Godot.Collections.Array<T> ActionList { get; set; }
    private int TotalActions => ActionList.Count;

    private int _selectedActionIndex = 0;
    public int SelectedActionIndex
    {
        get => _selectedActionIndex;
        set
        {
            _selectedActionIndex = value % TotalActions < 0 ? TotalActions - 1 : value % TotalActions;
            OnSelectedActionChanged.Invoke(_selectedActionIndex);
        }
    }
    public T SelectedAction => ActionList[SelectedActionIndex];

    public event Action<int> OnSelectedActionChanged;
    public event Action<T> OnActionConfirmed;
    public event Action OnActionCancelled;

    public override void _Ready()
    {
#if DEBUG
        OnSelectedActionChanged += PrintSelectedAction;
#endif

        base._Ready();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    public bool Select(T selectedAction)
    {
        foreach (T action in ActionList)
        {
            if (action == selectedAction)
            {
                SelectedActionIndex = ActionList.IndexOf(action);
                return true;
            }
        }

        return false;
    }

    public virtual void Set(IList<T> actionList)
    {
        ActionList = new Godot.Collections.Array<T>();
        foreach (T action in actionList)
        {
            ActionList.Add(action);
        }
    }

    protected virtual void PrintSelectedAction(int action)
    {
        GD.Print($"Selected Action: { ActionList[ SelectedActionIndex ] }");
    }

    public virtual void ConfirmAction()
    {
        OnActionConfirmed?.Invoke(SelectedAction);
    }

    public virtual void CancelAction()
    {
        OnActionCancelled?.Invoke();
    }

    public virtual IReadOnlyList<T> GetAllActions()
    {
        return ActionList;
    }
}
