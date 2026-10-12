namespace Lessons.Interface;

internal static class Example
{
    public static void Run()
    {
        // 同一个调用方，面对两个不同实现。
        var weapon = new Weapon();
        IDamageable enemy = new Enemy();
        IDamageable crate = new Crate();
        weapon.Attack(enemy, 30);
        weapon.Attack(crate, 30);

        // 接口类型决定能调用哪些成员；运行时对象决定具体实现。
        Console.WriteLine($"敌人：{enemy.Health}，木箱：{crate.Health}");
        // enemy.Roar(); // 编译错误：IDamageable 没有声明 Roar。
    }
}

internal interface IDamageable
{
    int Health { get; }
    void TakeDamage(int amount);
}

// 必须声明 : IDamageable；只有同名方法并不像 Go 那样自动满足接口。
internal sealed class Enemy : IDamageable
{
    public int Health { get; private set; } = 100;
    public void TakeDamage(int amount)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        Health -= Math.Min(amount, Health);
        Console.WriteLine($"敌人受伤，剩余生命值：{Health}");
    }
    public void Roar() => Console.WriteLine("敌人咆哮");
}

internal sealed class Crate : IDamageable
{
    public int Health { get; private set; } = 20;
    public void TakeDamage(int amount)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        Health -= Math.Min(amount, Health);
        Console.WriteLine($"木箱受伤，剩余耐久：{Health}");
    }
}

internal sealed class Weapon
{
    public void Attack(IDamageable target, int amount)
    {
        ArgumentNullException.ThrowIfNull(target);
        target.TakeDamage(amount); // 不判断具体类，不依赖继承体系。
    }
}
