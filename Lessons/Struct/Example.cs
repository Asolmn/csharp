namespace Lessons.Struct;

internal static class Example
{
    public static void Run()
    {
        var original = new Position(10, 20);
        var copy = original; // 类比 Go 复制 struct 值。
        copy.X = 99;
        Console.WriteLine($"原值 X：{original.X}，副本 X：{copy.X}");
        MoveCopy(original);
        Console.WriteLine($"按值传参后 X：{original.X}");
        MoveOriginal(ref original);
        Console.WriteLine($"ref 传参后 X：{original.X}");

        var damage = new DamageInfo(30);
        var reduced = damage.ReduceBy(10);
        Console.WriteLine($"原始伤害：{damage.Amount}，减伤后：{reduced.Amount}");
        Console.WriteLine($"default 伤害：{default(DamageInfo).Amount}");

        // 值复制不是深拷贝：引用字段仍共享对象。
        var tags = new TagSet { Items = new List<string> { "fire" } };
        var tagsCopy = tags;
        tagsCopy.Items.Add("critical");
        Console.WriteLine($"复制后向共享列表添加标签：{tags.Items.Count}");
    }

    private static void MoveCopy(Position position) => position.X++;
    private static void MoveOriginal(ref Position position) => position.X++;
}

// 可变 struct 用于演示复制；不是继承自任何游戏引擎类型。
internal struct Position
{
    public int X { get; set; }
    public int Y { get; set; }
    public Position(int x, int y) { X = x; Y = y; }
}

internal readonly struct DamageInfo
{
    public int Amount { get; }
    public DamageInfo(int amount)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
        Amount = amount;
    }

    public DamageInfo ReduceBy(int armor)
    {
        if (armor < 0) throw new ArgumentOutOfRangeException(nameof(armor));
        return new DamageInfo(Math.Max(0, Amount - armor));
    }
}

internal struct TagSet
{
    public List<string> Items { get; set; }
}
