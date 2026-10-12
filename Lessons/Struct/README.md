# struct 知识讲解：值类型、复制、初始化与不可变设计

本文先独立解释 struct 的语言规则，最后再看项目里的坐标、伤害值示例与积分练习。文中的片段分别解释概念，不要全部直接拼接为一个文件。

## 面向有编程开发经验读者的阅读路线

目标：掌握值复制的边界，能判断改的是原值、副本还是共享引用对象。

- **第一遍掌握**：第 1～7 节，以及第 9 节的类型选择。优先理解默认传参、ref、default、readonly 和浅复制。
- **第二遍再学**：in 的防御性复制、装箱优化和自定义相等性；先理解概念，再做性能优化。
- **理解自检**：struct 中有一个 List 字段时，为什么外层值独立而列表仍可能共享？

本课面向具有一定年限编程开发工作经验的读者，使用 .NET 控制台验证 C# 语言规则。已掌握的循环、分支、函数和集合基础可快速略读；重点理解 C# 的语法、行为规则及容易与已有经验混淆的地方。文中的 Go 对照作为可选补充，不熟悉 Go 可跳过。游戏名称只提供语境，不引入 Unity API、组件或编辑器操作。

## 1. struct 是什么

**struct 是用户定义的值类型，用于把相关的数据和行为组成一个值。** 它不是只能包含字段的记录容器，也能有属性、构造函数、实例方法、静态成员，并实现接口。

```csharp
public struct Point
{
    public int X { get; set; }
    public int Y { get; set; }

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public int Sum() => X + Y;
}
```

`public struct Point` 声明类型，`new Point(2, 3)` 创建一个值并调用构造函数。`new` 不意味着它一定像 class 一样单独创建一个堆对象。

Go 的 `type Point struct { X, Y int }` 同样具有值语义；Go 接收者方法写在类型外，C# 方法写在类型内。C# struct 成员可见性通过 public/private 等修饰符指定，普通成员省略时默认 private。

## 2. “值类型”的实际含义：赋值复制值

```csharp
var a = new Point(2, 3);
var b = a;
b.X = 100;
Console.WriteLine(a.X); // 2
Console.WriteLine(b.X); // 100
```

`b = a` 后，b 保存了独立的 Point 值。它与 class 的差异是赋值后的共享行为，而不是代码有没有写 `new`。

| 操作 | struct | class |
| --- | --- | --- |
| `b = a` | 复制值 | 复制引用 |
| 修改 b 的普通数值字段 | 不改变 a | 若两者指向同一对象，会看到共同变化 |
| 默认传参 | 参数接收值的副本 | 参数接收引用的副本 |
| 默认值 | 各字段的默认值组成的值 | null 引用 |
| 能否继承自另一个自定义类/结构体 | 不能 | 可继承一个类 |

## 3. 按值传参、ref、out、in

默认参数会产生值的副本：

```csharp
static void Move(Point p) { p.X++; }
static void MoveByRef(ref Point p) { p.X++; }
```

```csharp
var p = new Point(2, 3);
Move(p);          // p.X 仍为 2
MoveByRef(ref p); // p.X 变成 3
```

Go 可以传入 `*Point` 达到修改原值的目的。C# 的 `ref` 是语言支持的按引用参数，不是可以随意保存和做指针运算的普通指针。

| 参数形式 | 含义 | 调用前要求 | 方法内规则 |
| --- | --- | --- | --- |
| `Point p` | 按值传入 | 已赋值 | 修改副本 |
| `ref Point p` | 可读写的按引用传入 | 已赋值 | 可读写调用方变量 |
| `out Point p` | 输出参数 | 可以未赋值 | 正常返回前必须赋值 |
| `in Point p` | 只读的按引用传入 | 提供有效值 | 不能直接改写参数 |

```csharp
static void CreateOrigin(out Point p) { p = new Point(0, 0); }
CreateOrigin(out var origin);
```

`in` 可以减少某些大结构体的传值成本，但调用非 readonly 成员可能触发防御性复制，不能认为加上 `in` 就一定更快。先理解语义，再按测量结果优化。

## 4. 默认值与构造函数

结构体存在不经过自定义构造逻辑的默认值：

```csharp
Point p = default;
var points = new Point[2];
Console.WriteLine(p.X);         // 0
Console.WriteLine(points[0].Y); // 0
```

`default(Point)` 让字段取默认值：数值为 0、bool 为 false、引用为 null，嵌套值类型递归取默认值。数组新建后每个元素同样是默认值。局部变量 `Point p;` 则涉及确定赋值规则，不能立即读取整个未赋值变量；学习时使用构造函数或 default 明确初始化。

当前项目的 C# 支持显式无参 struct 构造函数，但要区分 `new T()` 与 `default(T)`：

```csharp
public struct Seed
{
    public int Value { get; }
    public Seed() { Value = 10; }
}
```

```csharp
Console.WriteLine(new Seed().Value);    // 10：调用显式无参构造函数
Console.WriteLine(default(Seed).Value); // 0：直接得到默认值
var seeds = new Seed[1];
Console.WriteLine(seeds[0].Value);       // 0：数组元素为默认值
```

显式无参 struct 构造函数必须是 public。没有显式无参构造函数时，`new Point()` 得到默认初始化的 Point。即使你在带参构造函数中禁止某些输入，也不能阻止 default 或数组创建产生零值。因此设计结构体时要让默认值有合理含义，或在使用处检查有效性。

## 5. readonly struct：让修改变成创建新值

可变 struct 容易让调用者误以为改到了原值。小型值对象通常适合不可变设计：

```csharp
public readonly struct Distance
{
    public int Meters { get; }

    public Distance(int meters)
    {
        if (meters < 0) throw new ArgumentOutOfRangeException(nameof(meters));
        Meters = meters;
    }

    public Distance Add(Distance other)
    {
        return new Distance(checked(Meters + other.Meters));
    }
}
```

```csharp
var a = new Distance(10);
var b = a.Add(new Distance(5)); // a 为 10，b 为 15
```

`readonly struct` 要求实例字段是 readonly，普通可写自动属性也不能这样声明；上例使用 getter-only 自动属性。它的方法不能修改自身字段，所以通过返回新值表示改变。

但 `readonly` 不等于变量永远不能重新赋值：`a = b` 仍然合法。它限制的是结构体实例状态的修改。也不意味着内部引用对象递归不可变，下一节会说明。

## 6. 值复制不是深拷贝

```csharp
public struct Tags
{
    public List<string> Items { get; set; }
}
```

```csharp
var a = new Tags { Items = new List<string> { "backend" } };
var b = a;
b.Items.Add("csharp");
Console.WriteLine(a.Items.Count); // 2
b.Items = new List<string>();
Console.WriteLine(a.Items.Count); // 仍为 2
```

复制 struct 时复制每个字段。Items 的值是一个引用，所以两个 struct 最初共享同一个列表；后来给 b.Items 重新赋值，只改变 b 保存的引用。

这与 Go 复制一个包含指针、map 或 slice 的 struct 很相似：外层值复制不代表底层引用数据也复制了。把这个 struct 改成 readonly，也不会让共享列表停止接受 Add。

## 7. 集合和属性里的可变 struct 陷阱

`List<T>` 的索引器读取普通 struct 时返回一个值，不是可直接修改原元素的引用：

```csharp
var points = new List<Point> { new Point(1, 2) };
// points[0].X = 9; // 编译错误：不能直接修改索引器返回的值类型副本
var copy = points[0];
copy.X = 9;
points[0] = copy; // 显式写回
```

普通返回 struct 的属性也有类似问题。数组元素则可以直接作为变量访问，`new Point[1]` 后对 `array[0].X` 赋值是允许的。不要把 List 和数组的行为混为一谈。

使用不可变 struct 时，写 `points[0] = 新值` 通常更容易看清修改发生在哪里。

## 8. null、装箱与相等性

普通 Point 不能赋 null；需要“没有坐标”时使用 `Point?`，它是 `Nullable<Point>`：

```csharp
Point? point = null;
Console.WriteLine(point.HasValue); // False
Point actual = point ?? new Point(0, 0);
```

与 class 的可空注解不同，`Point?` 是一个不同的值类型包装。访问空的 `point.Value` 会抛异常，应先检查 HasValue 或使用模式匹配、`??`。

把 struct 转成 object 或接口类型通常发生装箱：复制值并放进一个对象中。再转换回来是拆箱：

```csharp
var a = new Point(1, 2);
object boxed = a;
a.X = 9;
var b = (Point)boxed;
Console.WriteLine(b.X); // 1：装箱时保存的副本
```

拆箱必须匹配实际装箱的类型，不是任意数值转换。`object n = 1; (long)n` 会抛 InvalidCastException，需先 `(int)n` 拆箱再做数值转换。

普通 struct 默认 `Equals` 按实例字段比较，但不自动生成 `==` 运算符。字段如果是列表引用，也不会自动按列表元素比较。需要明确值相等性时可实现 `IEquatable<T>`，或继续学习 `record struct`。Go 的 struct 则要求所有字段可比较才能使用 `==`，两种语言规则不同。

## 9. 选择 struct 时看语义，不凭“栈更快”

适合较小、自包含、无独立身份、复制后应作为独立值使用的数据，如坐标、尺寸、金额。大结构体反复复制可能昂贵；可变状态需要共享时通常更适合 class。

struct 并不保证在栈上：它可作为 class 的字段、数组的元素，也可以被装箱。存储位置取决于上下文和运行时，值语义才是语言层面要掌握的事实。

struct 不能继承其他自定义 class/struct，也不能作为被继承的基类；它隐式继承 System.ValueType，可以实现接口。不需要继承也能为 struct 定义业务方法。

## 配套示例与练习

以下将前面的语言知识应用到项目代码。命令均在 `/Users/zaijing/project/csharp` 目录执行。

### 完整示例：坐标、伤害值与浅复制

阅读 [Example.cs](Example.cs)，依次观察可变值、不可变值、包含引用字段的值。

```sh
dotnet run -- struct example
```

预期输出：

```text
原值 X：10，副本 X：99
按值传参后 X：10
ref 传参后 X：11
原始伤害：30，减伤后：20
default 伤害：0
复制后向共享列表添加标签：2
```

Position 演示默认复制；DamageInfo.ReduceBy 返回新值；TagSet 演示外层 struct 复制后内部 List 仍共享。不要把最后一种情况误解为 struct 变成了引用类型。

完成示例后，练习刻意换一个题材，检查你能否迁移同一套语言规则。业务场景无需掌握额外框架。

### 自己练习：不可变积分

打开 [Exercise.cs](Exercise.cs)，完成 `readonly struct Score`：

1. 构造函数拒绝负分，抛 `ArgumentOutOfRangeException`，合法值写入 `Value`。
2. `Add(delta)` 拒绝负增量，抛同一种异常；零增量合法。
3. 返回一个新的 `Score`，原积分不变。
4. 使用 `checked(Value + delta)`，溢出时抛 `OverflowException`。不要把溢出后的负数当成正常结果。

```sh
dotnet run -- struct exercise
```

检查包含：返回新值、旧值不变、默认零值、负数校验，以及 `int.MaxValue + 1` 的边界。

### 自测问题

- 把坐标参数加上 `ref` 后，为什么输出改变？
- `default(Score)` 会进入你写的带参数构造函数吗？
- 如果 struct 包含 `List<string>`，复制后能否通过副本影响原列表？

答案要点：ref 访问调用方变量；default 不执行该构造函数；可以，因为列表引用被复制但列表对象共享。
