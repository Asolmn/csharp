namespace Lessons.Struct;

// 学习目标：掌握值复制的边界，能判断改的是原值、副本还是共享引用对象。
// 先预测 Run 中每个检查的结果，再只补全 TODO；保留检查代码验证理解。
internal static class Exercise
{
    public static void Run()
    {
        var original = new Score(10);
        var copy = original;
        var increased = copy.Add(5);
        Check.That(original.Value == 10 && copy.Value == 10, "原值和副本都保持不变");
        Check.That(increased.Value == 15, "Add 返回包含新分数的值");
        Check.That(default(Score).Value == 0, "default 得到零值");
        Check.That(original.Add(0).Value == 10, "允许增加零分");
        Check.Throws<ArgumentOutOfRangeException>(() => new Score(-1), "初始分数不能为负数");
        Check.Throws<ArgumentOutOfRangeException>(() => original.Add(-1), "增加量不能为负数");
        Check.Throws<OverflowException>(() => new Score(int.MaxValue).Add(1), "溢出时拒绝产生错误分数");
    }
}
internal readonly struct Score
{
    public int Value { get; }
    public Score(int value)
    {
        // TODO：校验 value 非负，再初始化只读属性。
        throw new NotImplementedException();
    }
    public Score Add(int delta)
    {
        // TODO：校验 delta 非负，返回新的 Score。使用 checked(...) 检查整数加法溢出。
        throw new NotImplementedException();
    }
}
