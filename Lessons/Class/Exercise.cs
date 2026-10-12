namespace Lessons.Class;

// 学习目标：建立“类实例是引用对象”的准确模型，熟练使用属性和构造函数封装状态。
// 先预测 Run 中每个检查的结果，再只补全 TODO；保留检查代码验证理解。
internal static class Exercise
{
    public static void Run()
    {
        var stock = new InventoryItem("keyboard", 10);
        Check.That(stock.Sku == "keyboard" && stock.Quantity == 10, "构造函数初始化属性");
        var alias = stock;
        alias.Restock(5);
        Check.That(stock.Quantity == 15, "通过另一个引用补货，原对象数量也变化");
        stock.Sell(4);
        Check.That(stock.Quantity == 11, "售出后扣减库存");
        Check.Throws<InvalidOperationException>(() => stock.Sell(12), "库存不足时拒绝售出");
        Check.Throws<ArgumentOutOfRangeException>(() => stock.Restock(0), "补货数量必须为正数");
        Check.Throws<ArgumentOutOfRangeException>(() => stock.Sell(-1), "售出数量必须为正数");
        Check.That(stock.Quantity == 11, "失败操作不改变库存");
        Check.Throws<ArgumentException>(() => new InventoryItem(" ", 1), "SKU 不能为空白");
        Check.Throws<ArgumentOutOfRangeException>(() => new InventoryItem("mouse", -1), "初始库存不能为负数");
        Check.That(new InventoryItem("mouse", 0).Quantity == 0, "允许零库存");
    }
}
internal sealed class InventoryItem
{
    public string Sku { get; }
    public int Quantity { get; private set; }
    public InventoryItem(string sku, int quantity)
    {
        // TODO：校验 SKU 非空白、初始数量非负，再初始化属性。
        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new ArgumentException("SKU 不能为空白", nameof(sku));
        }
        if (quantity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "初始库存不能为负数");
        }
        Sku = sku;
        Quantity = quantity;
    }
    public void Restock(int amount)
    {
        // TODO：amount 必须 > 0，成功后增加库存。
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "补货数量必须为正数");
        }

        Quantity += amount;
    }
    public void Sell(int amount)
    {
        // TODO：amount 必须 > 0；库存不足抛 InvalidOperationException；否则扣减库存。
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "售出数量必须为正数");
        }

        if (Quantity < amount)
        {
            throw new InvalidOperationException("库存不足");
        }

        Quantity -= amount;
    }
}
