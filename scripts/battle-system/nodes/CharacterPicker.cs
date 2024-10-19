using System;
using System.Collections.Generic;
using Godot;

public partial class CharacterPicker : ActionPicker<BattleCharacter>
{
    [Export] protected override Godot.Collections.Array<BattleCharacter> ActionList { get; set; }
    [Export] private Color DisabledColour { get; set; }

    public override void _Ready()
    {
        Set(ActionList);
        base._Ready();
    }

    public override void Set(IList<BattleCharacter> actionList)
    {
        base.Set(actionList);

        for (int i = 0; i < ActionList.Count; i++)
        {
            ActionList[ i ].SetBattlePosition(i);
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
            foreach (int slot in attack.CanHitSlots)
            {
                if (character.BattlePosition == slot)
                {
                    validCharacters.Add(character);
                    break;
                }
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
        SelectorSprite.Position = ActionList[ index ].Position;
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
}
