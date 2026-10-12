namespace Lessons.Class;

internal static class Example
{
    public static void Run()
    {
        // class 变量复制引用，接近 Go 复制 *T。
        var hero = new Character("Knight", 100);
        var alias = hero;
        alias.TakeDamage(30);
        Console.WriteLine($"共享修改：{hero.Health}，同一对象：{ReferenceEquals(hero, alias)}");

        // 默认传参依然按值传递：方法拿到的是引用的副本。
        ReplaceLocal(hero);
        Console.WriteLine($"普通参数重新赋值后：{hero.Name}");
        ReplaceCaller(ref alias);
        Console.WriteLine($"ref 替换变量后：hero={hero.Name}，alias={alias.Name}");

        hero.Heal(100);
        Console.WriteLine($"治疗上限：{hero.Health}/{hero.MaxHealth}");
        hero.TakeDamage(200);
        Console.WriteLine($"生命值：{hero.Health}，是否死亡：{hero.IsDead}");
        // hero.Health = -1; // 编译错误：外部不能使用 private setter。
    }

    private static void ReplaceLocal(Character character)
    {
        character = new Character("Local", 50);
    }

    private static void ReplaceCaller(ref Character character)
    {
        character = new Character("Replacement", 50);
    }
}

// 普通 C# 类：构造函数初始化，方法维护状态，属性控制访问。
internal sealed class Character
{
    public string Name { get; }
    public int MaxHealth { get; }
    public int Health { get; private set; }
    public bool IsDead => Health == 0; // 计算属性，不单独保存容易失步的 bool 字段。

    public Character(string name, int maxHealth)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("名字不能为空", nameof(name));
        if (maxHealth <= 0) throw new ArgumentOutOfRangeException(nameof(maxHealth));
        Name = name;
        MaxHealth = maxHealth;
        Health = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        Health -= Math.Min(amount, Health);
    }

    public void Heal(int amount)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        if (IsDead) throw new InvalidOperationException("死亡角色不能治疗");
        Health += Math.Min(amount, MaxHealth - Health);
    }
}
