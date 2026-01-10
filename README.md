# openfindbearings 概述

## 核心功能：
1. 轴承供应商或厂家登录，上传自己轴承库存信息和接收寻货询价并应答（供应轴承）、发布寻货询价（需求轴承）；
2. 后台实现海量轴承信息的存储和检索，通过采购和供应信息自动充实轴承数据库信息，通过轴承需求信息检索各供应商的库存状态发布信息给有库存的供应商。
## Core Functions: 
1. Bearing suppliers or manufacturers can log in to upload their bearing inventory information, receive inquiries for sourcing and respond (supply bearings), and post sourcing inquiries (demand for bearings); 
2. The backend achieves storage and retrieval of massive bearing information, automatically enriches the bearing database through procurement and supply information, and retrieves suppliers' inventory status based on bearing demand information to provide information to suppliers with available stock.

# 🏗️ OpenFindBearings 系统架构

请参阅 [系统架构](docs/architecture.md) 获取详细内容。

# 🤝 贡献指南

## 1. 开发环境设置
1. 安装 .NET 10 SDK
2. 安装 Docker Desktop
3. 安装 Dapr CLI
4. 安装 PostgreSQL 16
5. 配置开发证书
## 2. 代码规范
- 遵循 C# 编码规范
- 使用 EditorConfig 统一格式
- 编写单元测试和集成测试
- API 文档与代码同步更新
## 3. 提交规范
- 使用 Conventional Commits
- 关联 Issue 编号
- 提供详细的变更说明
- 通过 CI 测试和代码审查

---

**版本**: 1.0.0  
**技术栈**: .NET 10 + PostgreSQL 16 + Dapr  
**维护者**: OpenFindBearings Team  
**许可证**: MIT
**最后更新**: 2024-01-15  