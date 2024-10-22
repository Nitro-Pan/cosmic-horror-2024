using System;
using System.Collections.Generic;
using Godot;
public class AutomaticTurnManager
{
    public bool ShouldProgressTurnState { get; private set; } = false;

    private BattleCharacter Character { get; set; }
    private CharacterPicker FriendlyCharacters { get; set; }
    private CharacterPicker EnemyCharacters { get; set; }

    public void Set(BattleCharacter character, CharacterPicker friendlyCharacters, CharacterPicker enemyCharacters)
    {
        Character = character;
        FriendlyCharacters = friendlyCharacters;
        EnemyCharacters = enemyCharacters;
    }

    public void ProcessTurn()
    {
        AttackAction attackAction = GetAttackAction();
        CharacterPicker targets = attackAction.IsFriendly ? FriendlyCharacters : EnemyCharacters;
        IReadOnlyList<BattleCharacter> validTargets = targets.GetValidCharactersForAttack(attackAction);

        BattleCharacter selectedCharacterToHit = validTargets[ GD.RandRange(0, validTargets.Count - 1) ];
        selectedCharacterToHit.ApplyHit(attackAction);
        Character.StartAttack(OnAttackFinished);
        // animation maybe? can set a callback here

#if DEBUG
        GD.Print($"Processed turn for {Character}\nHit: {selectedCharacterToHit}\nAttack: {attackAction}");
#endif
    }

    public void TurnStateHandled()
    {
        ShouldProgressTurnState = false;
    }

    private AttackAction GetAttackAction()
    {
        IReadOnlyList<AttackAction> attacks = Character.GetValidAttacks();
        return attacks[ GD.RandRange(0, attacks.Count - 1) ];
    }

    private void OnAttackFinished()
    {
        ShouldProgressTurnState = true;
    }
}

