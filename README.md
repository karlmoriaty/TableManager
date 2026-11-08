# TableManager - 企业级 ERP/CRM 桌面管理系统

面向中小企业的高效内部管理桌面应用，集库存管理、客户关系管理、订单管理于一体。

## 功能特性

- **仪表盘**：数据概览与统计图表
- **商品管理**：商品 CRUD、库存查询、库存盘点
- **客户管理**：客户信息、跟进记录、联系人管理
- **订单管理**：销售订单创建、状态管理、查询统计
- **库存管理**：入库、出库、库存预警
- **数据导出**：Excel 报表导出

## 技术栈

- **C# 12** + **WPF** + **.NET 8.0**
- **SQLite**（Microsoft.Data.Sqlite）嵌入式数据库
- **CommunityToolkit.Mvvm**（MVVM 框架）
- **EPPlus**（Excel 导出）

## 项目结构

```
TableManager/
├── TableManager.sln
├── TableManager.UI/          # WPF 界面层（View + ViewModel + Converter）
├── TableManager.BLL/         # 业务逻辑层（Services）
├── TableManager.DAL/         # 数据访问层（Database + Models + Repositories）
└── README.md
```

## 快速开始

```bash
# 克隆仓库后，用 Visual Studio 打开 TableManager.sln
# 或命令行：
dotnet build TableManager.sln
dotnet run --project TableManager.UI/TableManager.UI.csproj
```

首次运行会自动创建 SQLite 数据库文件（位于程序运行目录下的 `tablemanager.db`）。
