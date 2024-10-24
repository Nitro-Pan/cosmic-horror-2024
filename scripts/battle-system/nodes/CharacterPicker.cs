using System;
using System.Collections.Generic;
using System.IO;
using Godot;

public partial class CharacterPicker : ActionPicker<BattleCharacter>
{
    [Export] protected override Godot.Collections.Array<BattleCharacter> ActionList { get; set; }
    [Export] private Color DisabledColour { get; set; }
    private List<Vector2> SettledPositions { get; set; } = new List<Vector2>();

    public override void _Ready()
    {
        Set(ActionList);
        base._Ready();
    }

    public override void Set(IList<BattleCharacter> actionList)
    {
        base.Set(actionList);

        SettledPositions.Clear();
        for (int i = 0; i < ActionList.Count; i++)
        {
            ActionList[ i ].SetBattlePosition(i);
            SettledPositions.Add(actionList[i].Position);
        }

        OnActionChanged(SelectedActionIndex);
    }

    public void Set(Godot.Collections.Array<BattleCharacter> actionList)
    {
        base.Set(actionList);

        SettledPositions.Clear();
        for (int i = 0; i < ActionList.Count; i++)
        {
            ActionList[ i ].SetBattlePosition(i);
            SettledPositions.Add(actionList[ i ].Position);
        }

        OnActionChanged(SelectedActionIndex);
    }

    public void Reset(IList<BattleCharacter> actionList)
    {
        base.Set(actionList);

        for (int i = 0; i < ActionList.Count; i++)
        {
            ActionList[ i ].SetBattlePosition(i);
            ActionList[ i ].Position = SettledPositions[ i ];
        }

        OnActionChanged(SelectedActionIndex);
    }

    public void HighlightValidOptions(AttackAction attack)
    {
        foreach (BattleCharacter character in ActionList)
        {
            character.SetSelectableState(false);
        }

        foreach (BattleCharacter character in GetValidCharactersForAttack(attack))
        {
            character.SetSelectableState(true);
        }
    }

    public IReadOnlyList<BattleCharacter> GetValidCharactersForAttack(AttackAction attack)
    {
        List<BattleCharacter> validCharacters = new List<BattleCharacter>();

        foreach (BattleCharacter character in ActionList)
        {
            if (attack.CanHitCharacter(character))
            {
                validCharacters.Add(character);
            }
        }

        return validCharacters;
    }

    public void ResetHighlights()
    {
        foreach (BattleCharacter character in ActionList)
        {
            character.SetSelectableState(true);
        }
    }

    public void DimAllExcept(params BattleCharacter[] doNotDim)
    {
        foreach (BattleCharacter character in ActionList)
        {
            character.SetSelectableState(false);

            if (doNotDim == null || doNotDim.Length == 0)
            {
                continue;
            }

            foreach (BattleCharacter notDimmed in doNotDim)
            {
                if (character == notDimmed)
                {
                    character.SetSelectableState(true);
                }
            }
        }
    }

    // there should be a better way to do this that doesn't involve taking random refs but w/e
    public void SwapCharacters(BattleCharacter from, BattleCharacter to)
    {
        if (!(ActionList.Contains(from) && ActionList.Contains(to)))
        {
#if DEBUG
            GD.Print("Trying to swap two invalid characters, aborting");
#endif
            return;
        }

        int fromListPosition = ActionList.IndexOf(from);
        int toListPosition = ActionList.IndexOf(to);

        ActionList[ fromListPosition ] = to;
        ActionList[ toListPosition ] = from;

        int toBattlePosition = to.BattlePosition;
        to.SetBattlePosition(from.BattlePosition);
        from.SetBattlePosition(toBattlePosition);

        to.MoveToPosition(SettledPositions[fromListPosition]);
        from.MoveToPosition(SettledPositions[toListPosition]);
    }

    public void ShoveCharacters(BattleCharacter from, BattleCharacter to)
    {
        if (!(ActionList.Contains(from) && ActionList.Contains(to)))
        {
#if DEBUG
            GD.Print("Trying to swap two invalid characters, aborting");
#endif
            return;
        }

        int amount = ActionList.IndexOf(to) - ActionList.IndexOf(from);
        int amountMagnitude = Math.Abs(amount);
        int direction = Math.Sign(amount);

        for (int i = 0; i < amountMagnitude; i++)
        {
            BattleCharacter nextCharacter = direction switch
            {
                1 => GetNextCharacter(from),
                -1 => GetPreviousCharacter(from),
                _ => from
            };

            SwapCharacters(from, nextCharacter);
        }
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

    protected override void OnActionChanged(int index)
    {
        SelectorTargetPosition = ActionList[ index ].Position;
    }

    public bool IsAllDead()
    {
        foreach (BattleCharacter character in ActionList)
        {
            if (!character.IsDead())
            {
                return false;
            }
        }

        return true;
    }

    public BattleCharacter GetNextCharacter(BattleCharacter origin)
    {
        int index = ActionList.IndexOf(origin) + 1;
        if (index >= ActionList.Count)
        {
            return origin;
        }

        return ActionList[ index ];
    }

    public BattleCharacter GetPreviousCharacter(BattleCharacter origin)
    {
        int index = ActionList.IndexOf(origin) - 1;
        if (index < 0)
        {
            return origin;
        }

        return ActionList[ index ];
    }

    public BattleCharacter GetFurthestCharacter(BattleCharacter origin, int delta)
    {
        int index = Math.Clamp(ActionList.IndexOf(origin) + delta, 0, TotalActions - 1);
        return ActionList[ index ];
    }
}
