# class 知识讲解：类、对象、封装与继承

本文先讲语言概念和规则，不需要打开 Example.cs 就能学习。最后再结合项目示例和练习验证理解。文中的短代码块用于解释单个概念；除特别注明外，不应把所有代码块直接拼接到同一个文件。

## 面向有编程开发经验读者的阅读路线

目标：建立“类实例是引用对象”的准确模型，熟练使用属性和构造函数封装状态。

- **第一遍掌握**：第 1～7 节：重点看属性、构造函数、引用复制与 ref 的区别。
- **第二遍再学**：第 8 节继承细节；相等性重写和复杂继承体系可以后置。
- **理解自检**：如果把当前 class 换成 struct，赋值和方法调用的可观察行为会怎样变化？

本课面向具有一定年限编程开发工作经验的读者，使用 .NET 控制台验证 C# 语言规则。已掌握的循环、分支、函数和集合基础可快速略读；重点理解 C# 的语法、行为规则及容易与已有经验混淆的地方。文中的 Go 对照作为可选补充，不熟悉 Go 可跳过。游戏名称只提供语境，不引入 Unity API、组件或编辑器操作。

## 1. class 是什么：类型与对象的区别

**类是一个自定义类型，用来描述一类对象具有的数据和行为；对象是这个类的具体实例。** 例如，`User` 是类型，用户 Alice 和 Bob 是两个对象。字段和属性描述数据，方法描述行为。

```csharp
public class User
{
    public string Name { get; set; } = "";

    public void SayHello()
    {
        Console.WriteLine($"Hello, {Name}");
    }
}
```

声明类型本身不会创建用户。下面才是创建对象和调用行为：

```csharp
User alice = new User();
alice.Name = "Alice";
alice.SayHello(); // Hello, Alice
```

逐项看：`User` 是变量的类型，`alice` 是变量名，`new User()` 创建对象并调用构造函数，`.` 用于访问对象成员。`var alice = new User()` 只是让编译器推断类型，推断结果仍是 `User`。

Go 把类型声明和接收者方法分开写；C# 的普通实例方法写在类型内部，通过 `this` 访问当前对象。`this` 在含义上接近接收者，通常可以省略。

## 2. 声明语法与成员可见性

基本形式如下，方括号表示可选部分，并非实际 C# 代码：

```text
[访问修饰符] [abstract 或 sealed] class 类名 [: 基类, 接口1, 接口2]
{
    字段、属性、构造函数、方法等成员
}
```

C# 不使用名字首字母大小写区分是否导出，而是使用访问修饰符：

| 修饰符 | 谁可以访问 | 常见用途 |
| --- | --- | --- |
| `public` | 任何能访问所在类型的代码 | 对外 API |
| `private` | 声明它的类型内部 | 内部状态、辅助方法 |
| `protected` | 声明它的类及派生类 | 给继承者的扩展点 |
| `internal` | 当前程序集内部 | 项目内部实现 |

程序集通常是项目编译产生的 DLL/EXE，不等于命名空间。命名空间层级不会自动带来访问权限。

顶级类未写访问修饰符时默认是 `internal`；类的普通成员未写时默认是 `private`。顶级类不能写 `private` 或 `protected`，嵌套类型则有不同的可见性选择。即使成员是 `public`，也不能突破包含它的类型的可见性。

## 3. 字段和属性：存储数据与控制访问

**字段是直接存储状态的变量；属性是访问数据的入口，可以在读写时执行逻辑。** 两者的调用形式都可能是 `user.Name`，但不是同一种成员。

```csharp
public class Customer
{
    private string _name = ""; // 字段：真正存储名字

    public string Name       // 属性：控制如何读写
    {
        get { return _name; }
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("名字不能为空");
            _name = value;
        }
    }
}
```

执行 `customer.Name = "Alice"` 会调用 `set`；`value` 是本次赋入的值。读取 `customer.Name` 会调用 `get`。属性不是必须依赖字段，也可以计算结果，例如 `public decimal Total => UnitPrice * Quantity;`。

不需要自定义逻辑时可以使用自动属性，编译器会生成存储字段：

| 写法 | 读写规则 |
| --- | --- |
| `public string Name { get; set; } = "";` | 外部可读写 |
| `public int Count { get; private set; }` | 外部只读，类型内部可写 |
| `public int Id { get; }` | getter-only 自动属性，可在声明处或构造函数中赋值 |
| `public string Code { get; init; } = "";` | 可在对象初始化期间设置，之后不能普通赋值 |

`init` 可以配合对象初始化器：

```csharp
// 假设 Product 定义了 public string Code { get; init; } = "";
var product = new Product { Code = "P-001" };
// product.Code = "P-002"; // 编译错误：初始化阶段已经结束
```

对象初始化器在构造函数执行后设置成员；它不会绕过 setter 或 init 访问器。`init` 不意味着必须赋值，也不意味着属性指向的对象递归不可变。

对照 Go，私有字段加 `Name()` / `SetName(...)` 方法可以达到类似的封装目的；C# 提供了专门的属性语法。

## 4. 构造函数：建立合法的初始状态

构造函数与类同名，没有返回类型，在 `new` 创建实例时执行。它用于接收依赖、校验参数和初始化状态。

```csharp
public class Member
{
    public string Name { get; }
    public int Level { get; private set; }

    public Member(string name) : this(name, 1) { }

    public Member(string name, int level)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("名字不能为空", nameof(name));
        if (level < 1)
            throw new ArgumentOutOfRangeException(nameof(level));
        Name = name;
        Level = level;
    }
}
```

这里有两个构造函数，即构造函数重载。`: this(name, 1)` 复用同一个类的另一个构造函数，避免重复初始化规则。

如果普通类完全没有声明实例构造函数，编译器会提供无参构造函数；一旦声明了带参构造函数，就不会再自动补一个无参版本。因此上面的 `new Member()` 无法编译。

Go 常用 `NewMember(...)` 普通函数表达构造约定；C# 构造函数是语言机制。二者都可以封装初始化规则，但 C# 的构造函数不通过返回 `(value, error)` 报告失败，这里使用异常。

## 5. 方法、实例成员、static、const 与 readonly

方法基本形式是 `访问修饰符 返回类型 方法名(参数列表) { 方法体 }`。返回类型 `void` 表示没有返回值。普通实例方法操作某个对象；`static` 方法属于类型，不依赖某个实例。

```csharp
public class Counter
{
    public int Value { get; private set; } // 每个实例各有一份
    public static int Created { get; private set; } // 类型共享
    public const int Step = 1;

    public Counter() { Created++; }
    public void Increment() { Value += Step; }
}
```

`new Counter().Increment()` 是实例调用，`Counter.Created` 是静态成员访问。静态方法中没有 `this`，要访问实例数据必须先有一个对象。上面的计数只演示归属关系，`Created++` 并不是并发安全计数。

| 关键字 | 作用 | 容易混淆的点 |
| --- | --- | --- |
| `static` | 成员属于类型；普通非泛型类的静态字段由实例共享 | 不自动保证线程安全 |
| `const` | 编译期常量，声明时赋值 | 隐含静态语义，不能写运行期计算式 |
| `readonly` 字段 | 在声明处或对应构造函数中赋值 | 限制字段重新赋值，不冻结其引用的对象 |

例如 `private readonly List<string> _tags = new();` 仍允许 `_tags.Add("VIP")`，只是不能在普通方法内把 `_tags` 换成另一个列表。

## 6. 引用语义：复制变量不复制对象

```csharp
var a = new User { Name = "Alice" };
var b = a;
b.Name = "Bob";
Console.WriteLine(a.Name); // Bob

b = new User { Name = "Carol" };
Console.WriteLine(a.Name); // 仍为 Bob
```

第一步 `b = a` 复制引用，两者指向同一个对象。第二次 `b = new ...` 只修改变量 b 保存的引用，a 仍指向旧对象。这类似 Go 复制 `*User`；直接复制 Go 的 `User` 结构体值则不是这种行为。

**引用类型不等于默认引用传参。C# 默认按值传参，class 传入的是引用的副本。**

```csharp
static void Rename(User user) { user.Name = "Changed"; }
static void Replace(User user) { user = new User { Name = "New" }; }
static void ReplaceCaller(ref User user) { user = new User { Name = "New" }; }
```

调用 `Rename(a)` 会改变 a 指向的对象；`Replace(a)` 不会替换 a；`ReplaceCaller(ref a)` 才会改掉调用方变量。方法声明和调用都需要写 `ref`。

## 7. null 与相等性

`User? user = null;` 表示这个引用允许为空。访问空引用成员会抛 `NullReferenceException`。可使用条件访问和空值合并：

```csharp
User? user = null;
string name = user?.Name ?? "匿名";
```

`?.` 表示为空时停止成员访问并得到 null；`??` 在左侧为 null 时取右侧值。可空引用类型主要是编译期分析；`User` 与 `User?` 不是两种不同的运行时类。局部变量仍需要先赋值才能使用，不能把“引用默认值是 null”理解为所有未赋值局部变量都能读取。

身份相等和内容相等是两件事。`ReferenceEquals(a, b)` 判断是不是同一个对象；普通 class 未重写时，`Equals` 和未重载的引用 `==` 默认比较引用身份。`string`、`record` 或重写相等性逻辑的类可以比较内容。自定义 `Equals` 时还需保持 `GetHashCode` 一致，后续学习集合时再深入。

## 8. 继承与多态：virtual、override、abstract、sealed

类可以继承一个基类，获得可继承的成员，并添加或重写行为。C# 单一类继承不等同于 Go 的嵌入。

```csharp
public class Animal
{
    public virtual string Speak() => "...";
}
public class Dog : Animal
{
    public override string Speak() => "汪";
}
```

```csharp
Animal animal = new Dog();
Console.WriteLine(animal.Speak()); // 汪
```

变量静态类型是 `Animal`，实际对象类型是 `Dog`。`virtual` 允许派生类重写，`override` 提供重写实现，运行时调用实际对象对应的实现，这就是此处的多态。普通方法默认不是 virtual；用 `new` 隐藏方法不等于 override，不应混用。

`abstract class` 不能直接实例化，可包含普通成员和没有方法体的 `abstract` 成员；非抽象派生类必须实现继承来的抽象成员。`sealed class` 禁止继承；`sealed override` 则禁止继续重写某个已重写成员。

```csharp
public abstract class Shape
{
    public abstract double Area();
}
public sealed class Circle : Shape
{
    private readonly double _radius;
    public Circle(double radius) { _radius = radius; }
    public override double Area() => Math.PI * _radius * _radius;
}
```

派生类构造函数可以用 `: base(...)` 调用基类构造函数；若基类没有可用无参构造函数，就需要明确指定。构造函数本身不会被继承。

## 9. 后端开发中何时用 class

有身份和生命周期的订单、需要共享状态的实体、持有依赖的服务都适合用 class。也可以设计不可变 class，引用类型不强制可变。继承适合真正的“是一个”关系；“订单服务使用通知器”是组合关系，通常持有接口字段更直接。

本节应掌握：类型与对象、属性和字段、构造函数、可见性、引用复制、实例与静态成员。继承和抽象类先能读懂即可，不必为了练习而给每个类建立继承体系。

## 配套示例与练习

以下将前面的语言知识应用到项目代码。命令均在 `/Users/zaijing/project/csharp` 目录执行。

### 完整示例：角色状态与引用传递

阅读 [Example.cs](Example.cs)。这是普通 C# 对象，角色只是帮助理解状态的命名。

```sh
dotnet run -- class example
```

先预测输出，再运行：

```text
共享修改：70，同一对象：True
普通参数重新赋值后：Knight
ref 替换变量后：hero=Knight，alias=Replacement
治疗上限：100/100
生命值：0，是否死亡：True
```

对应的语言知识：构造函数初始化只读属性；private setter 控制状态修改；计算属性 IsDead 根据 Health 计算结果；普通参数复制引用，ref 参数可以替换调用方变量。先解释这四点，再阅读扣血和治疗的计算。

完成示例后，练习刻意换一个题材，检查你能否迁移同一套语言规则。业务场景无需掌握额外框架。

### 自己练习：商品库存

打开 [Exercise.cs](Exercise.cs)，补全 `InventoryItem` 的三个 TODO：

1. 构造函数：SKU 非空白，初始库存非负。错误分别抛 `ArgumentException`、`ArgumentOutOfRangeException`。
2. `Restock`：补货量必须大于零，否则抛 `ArgumentOutOfRangeException`；合法时增加数量。
3. `Sell`：售出量必须大于零，否则抛 `ArgumentOutOfRangeException`；库存不足抛 `InvalidOperationException`；合法时扣减数量。

不要放开 `Quantity` 的 setter。题目的目的之一就是让修改通过方法完成。数量取普通小整数，本练习不要求处理库存加法的整数溢出。

```sh
dotnet run -- class exercise
```

检查包含：正常补货和售出、共享引用、参数错误、库存不足，以及失败后数量保持不变。

### 自测问题

先口头回答，再修改例子观察：

- `alias.Restock(5)` 为什么会改变 `stock.Quantity`？
- 如果执行 `alias = new InventoryItem("other", 1)`，`stock` 会跟着改变吗？
- 为什么限制 setter，比要求调用者“不要传负数”更可靠？

答案要点：两个变量最初指向同一对象；重新赋值只改变 alias 保存的引用；封装让每一次状态修改都经过规则检查。
