using System;

namespace Sortis.Combat;

public enum IntentType
{
    Attack,
    Defend
}

public class Enemy
{
    public string Name { get; }
    public int Hp { get; private set; }
    public int MaxHp { get; }
    public int Block { get; private set; }
    public IntentType CurrentIntent { get; private set; }
    public int CurrentIntentValue { get; private set; }
    public bool IsDead => Hp <= 0;

    private int _turnIndex;
    private readonly (IntentType type, int value)[] _pattern;

    public Enemy(string name, int maxHp, (IntentType type, int value)[] pattern)
    {
        Name = name;
        MaxHp = maxHp;
        Hp = maxHp;
        _pattern = pattern;
        _turnIndex = 0;
        UpdateIntent();
    }

    public static Enemy ShadowKnight() => new("그림자 기사", 40, new[]
    {
        (IntentType.Attack, 12),
        (IntentType.Attack, 8),
        (IntentType.Defend, 6)
    });

    public void TakeDamage(int damage)
    {
        int remaining = damage - Block;
        Block = Math.Max(0, Block - damage);
        if (remaining > 0)
            Hp = Math.Max(0, Hp - remaining);
    }

    public int GetAttackDamage()
    {
        return CurrentIntent == IntentType.Attack ? CurrentIntentValue : 0;
    }

    public void ExecuteTurn()
    {
        if (CurrentIntent == IntentType.Defend)
            Block += CurrentIntentValue;

        _turnIndex = (_turnIndex + 1) % _pattern.Length;
        UpdateIntent();
    }

    private void UpdateIntent()
    {
        var current = _pattern[_turnIndex];
        CurrentIntent = current.type;
        CurrentIntentValue = current.value;
    }
}
