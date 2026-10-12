namespace Lessons.Enum;

internal static class Example
{
    public static void Run()
    {
        var state = CharacterState.Attacking;
        Console.WriteLine($"状态：{state}，底层数字：{(int)state}，标签：{Label(state)}");
        foreach (var input in new[] { "Attacking", "99", "typo" })
        {
            // TryParse 接受数字字符串；已定义成员需额外验证。
            var valid = System.Enum.TryParse<CharacterState>(input, true, out var parsed)
                        && System.Enum.IsDefined(parsed);
            Console.WriteLine($"输入 {input}：是否合法 = {valid}");
        }

        Console.WriteLine($"存活状态可以攻击：{CanAttack(CharacterState.Idle)}");
        Console.WriteLine($"死亡状态可以攻击：{CanAttack(CharacterState.Dead)}");
        var effects = StatusEffects.Burning | StatusEffects.Poisoned;
        Console.WriteLine($"组合效果：{effects}，包含燃烧：{(effects & StatusEffects.Burning) != 0}");
    }

    public static bool CanAttack(CharacterState state) => state is CharacterState.Idle or CharacterState.Moving;

    public static string Label(CharacterState state) => state switch
    {
        CharacterState.Unknown => "未知",
        CharacterState.Idle => "待机",
        CharacterState.Moving => "移动",
        CharacterState.Attacking => "攻击",
        CharacterState.Dead => "死亡",
        _ => throw new ArgumentOutOfRangeException(nameof(state))
    };
}

internal enum CharacterState { Unknown = 0, Idle = 1, Moving = 2, Attacking = 3, Dead = 4 }

// 状态与效果分开建模：状态一次选一个，效果可按位组合。
[Flags]
internal enum StatusEffects { None = 0, Burning = 1, Poisoned = 2, Frozen = 4 }
