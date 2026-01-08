# GameObject 装备系统使用说明

## 概述

这是一个专门为 **Character_114** 等通过 **GameObject 显示/隐藏** 来切换装备的角色模型设计的换装系统。

与传统的 Mesh 替换系统不同，这个系统通过**激活/禁用 GameObject** 来实现换装效果。

---

## 系统结构

### 1. 核心文件

- **GameObjectEquipmentType.cs** - 装备部位枚举定义
- **GameObjectEquipmentSlot.cs** - 装备槽位配置类
- **GameObjectEquipmentManager.cs** - 装备管理器（核心组件）
- **GameObjectEquipmentExample.cs** - 使用示例脚本

### 2. 支持的装备部位

- **Bag** - 背包
- **Top** - 上衣
- **Bottom** - 裤子/下装
- **Shoes** - 鞋子
- **Glove** - 手套
- **Hair** - 头发
- **Headgear** - 头部装备（帽子/头盔）
- **Eyewear** - 眼镜
- **BodyTop** - 身体上半部分
- **BodyBottom** - 身体下半部分

---

## 快速上手

### 步骤 1：添加管理器组件

1. 选择你的角色 GameObject（例如：`Character_114`）
2. 添加 `GameObjectEquipmentManager` 组件
3. 添加 `GameObjectEquipmentExample` 组件（可选，用于测试）

### 步骤 2：自动设置装备槽位

1. 在 Inspector 中找到 `GameObjectEquipmentManager` 组件
2. 在 **"快速设置"** 区域，将 `Character_114/Parts` 节点拖到 **"Parts 根节点"** 字段
3. 点击 **"自动设置装备槽位"** 按钮（或在运行时自动扫描）
4. 系统会自动扫描并配置所有装备槽位（运行时也会在未配置时根据 Parts 子节点自动生成）

### 步骤 3：测试换装

1. 选择 `GameObjectEquipmentExample` 组件
2. 右键点击组件标题，选择以下测试选项：
   - **"示例：穿上一套装备"** - 装备一套默认装备
   - **"示例：随机换装"** - 随机装备所有部位
   - **"测试：装备"** - 装备指定部位
   - **"测试：脱下"** - 脱下指定部位
   - **"测试：脱下所有"** - 脱下所有装备

---

## 代码使用示例

### 基础换装

```csharp
// 获取装备管理器
GameObjectEquipmentManager manager = GetComponent<GameObjectEquipmentManager>();

// 装备背包1（索引0）
manager.Equip(GameObjectEquipmentType.Bag, 0);

// 装备上衣2（索引1）
manager.Equip(GameObjectEquipmentType.Top, 1);

// 脱下鞋子
manager.Unequip(GameObjectEquipmentType.Shoes);

// 脱下所有装备
manager.UnequipAll();
```

### 通过名称装备

```csharp
// 通过 GameObject 名称装备
manager.EquipByName(GameObjectEquipmentType.Bag, "Bag_5");
manager.EquipByName(GameObjectEquipmentType.Hair, "Hair_3");
```

### 批量装备

```csharp
// 创建装备配置
var equipmentSetup = new Dictionary<GameObjectEquipmentType, int>
{
    { GameObjectEquipmentType.Bag, 2 },
    { GameObjectEquipmentType.Top, 1 },
    { GameObjectEquipmentType.Bottom, 3 },
    { GameObjectEquipmentType.Shoes, 0 }
};

// 批量装备
manager.EquipMultiple(equipmentSetup);
```

### 监听装备变化

```csharp
private void Start()
{
    var manager = GetComponent<GameObjectEquipmentManager>();
    manager.OnEquipmentChanged += OnEquipmentChanged;
}

private void OnEquipmentChanged(GameObjectEquipmentType equipmentType, int index)
{
    Debug.Log($"装备变化：{equipmentType} 切换到索引 {index}");

    // 在这里可以处理装备变化的逻辑
    // 例如：更新UI、保存配置、触发动画等
}
```

---

## 角色模型要求

为了使用这个系统，你的角色模型需要满足以下结构：

```
Character_114
├── RotateNode
├── Body (可选)
├── Bone
└── Parts ← 装备根节点
    ├── Bag ← 装备部位
    │   ├── Bag_1 ← 装备选项
    │   ├── Bag_2
    │   └── Bag_3
    ├── Top
    │   ├── Top_1
    │   ├── Top_2
    │   └── Top_3
    ├── Bottom
    │   ├── Bottom_1
    │   └── Bottom_2
    └── ... (其他装备部位)
```

**关键点：**
- 必须有一个 `Parts` 父节点（名字可以不同，但需要在设置时指定）
- 每个装备部位是 `Parts` 的直接子节点
- 每个装备选项是装备部位节点的子 GameObject
- 所有装备选项都应该有 `SkinnedMeshRenderer` 或 `MeshRenderer` 组件

---

## 与现有系统的区别

### 旧系统（Mesh 替换）
```csharp
// 通过替换 SkinnedMeshRenderer 的 Mesh
skinnedMeshRenderer.sharedMesh = newMesh;
```

### 新系统（GameObject 激活）
```csharp
// 通过激活/禁用 GameObject
gameObject.SetActive(true/false);
```

**优势：**
- ✅ 更简单直观
- ✅ 支持复杂的装备结构（多个子对象）
- ✅ 不需要手动管理 Mesh 资源
- ✅ 更容易在编辑器中预览和调试

---

## 常见问题

### Q: 如何添加新的装备部位？

A:
1. 在 `GameObjectEquipmentType.cs` 中添加新的枚举值
2. 在角色模型的 `Parts` 节点下创建对应的子节点
3. 重新运行 "自动设置装备槽位"

### Q: 装备索引是如何确定的？

A: 装备索引就是该装备在父节点中的子对象索引（从0开始）。例如：
- `Bag_1` 是索引 0
- `Bag_2` 是索引 1
- 以此类推

### Q: 可以同时装备多个同类装备吗？

A: 不可以。每个装备部位同一时间只能装备一个选项。装备新的会自动脱下旧的。

### Q: 如何保存装备配置？

A: 你可以保存当前装备的索引配置：
```csharp
Dictionary<GameObjectEquipmentType, int> currentSetup = new Dictionary<GameObjectEquipmentType, int>();
foreach (GameObjectEquipmentType type in Enum.GetValues(typeof(GameObjectEquipmentType)))
{
    int index = manager.GetCurrentEquipmentIndex(type);
    if (index >= 0)
        currentSetup[type] = index;
}
// 将 currentSetup 序列化保存
```

---

## 技术支持

如有问题或建议，请联系开发团队。
