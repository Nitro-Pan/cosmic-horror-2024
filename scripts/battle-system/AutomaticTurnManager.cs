using System;
using System.Collections.Generic;
using Godot;
public class AutomaticTurnManager
{
    public bool ShouldProgressTurnState { get; private set; } = false;

    private BattleCharacter AttackingCharacter { get; set; }
    private BattleCharacter DefendingCharacter { get; set; }
    private CharacterPicker FriendlyCharacters { get; set; }
    private CharacterPicker EnemyCharacters { get; set; }
    private AttackAction SelectedAttack { get; set; }

    public void Set(BattleCharacter character, CharacterPicker friendlyCharacters, CharacterPicker enemyCharacters)
    {
        AttackingCharacter = character;
        FriendlyCharacters = friendlyCharacters;
        EnemyCharacters = enemyCharacters;
    }

    public void ProcessTurn()
    {
        SelectedAttack = GetAttackAction();

        CharacterPicker targets = SelectedAttack.IsFriendly ? FriendlyCharacters : EnemyCharacters;
        IReadOnlyList<BattleCharacter> validTargets = targets.GetValidCharactersForAttack(SelectedAttack);

        if (validTargets.Count < 1)
        {
            ShouldProgressTurnState = true;
            return;
        }

        DefendingCharacter = validTargets[ GD.RandRange(0, validTargets.Count - 1) ];
        DefendingCharacter.ApplyHit(SelectedAttack);
        AttackingCharacter.StartAttack(OnAttackFinished);

#if DEBUG
        GD.Print($"Processed turn for {AttackingCharacter}\nHit: {DefendingCharacter}\nAttack: {SelectedAttack}");
#endif
    }

    public void TurnStateHandled()
    {
        ShouldProgressTurnState = false;
    }

    private AttackAction GetAttackAction()
    {
        IList<AttackAction> attacks = AttackingCharacter.GetValidAttacks();

        if (attacks.Count > 1)
        {
            for (int i = 0; i < attacks.Count; i++)
            {
                if (attacks[i].ContainsStatus(BattleUtils.StatusType.Passing))
                {
                    attacks.RemoveAt(i);
                    break;
                }
            }
        }

        return attacks[ GD.RandRange(0, attacks.Count - 1) ];
    }

    private void OnAttackFinished()
    {
        CharacterPicker targetPicker = SelectedAttack.IsFriendly ? FriendlyCharacters : EnemyCharacters;

        if (SelectedAttack.ContainsStatus(BattleUtils.StatusType.Moving))
        {
            targetPicker.ShoveCharacters(AttackingCharacter, DefendingCharacter);
        }

        if (SelectedAttack.GetStatusEffect(BattleUtils.StatusType.Pushing) is StatusEffect pushEffect)
        {
            targetPicker.ShoveCharacters(DefendingCharacter, targetPicker.GetFurthestCharacter(DefendingCharacter, pushEffect.Amount));
        }

        if (SelectedAttack.GetStatusEffect(BattleUtils.StatusType.Pulling) is StatusEffect pullEffect)
        {
            targetPicker.ShoveCharacters(DefendingCharacter, targetPicker.GetFurthestCharacter(DefendingCharacter, -pullEffect.Amount));
        }

        ShouldProgressTurnState = true;
    }
}

