# HHPortal - 现代化权限管理系统

一个基于ASP.NET Core 8.0和Bootstrap 5的现代化权限管理系统，提供完整的用户认证、角色管理和权限控制功能。支持前后端分离架构，具备完善的二次开发机制。

## ✨ 功能特性

### 🔐 用户认证
- JWT令牌认证
- 用户注册与登录
- 访问令牌与刷新令牌机制
- 密码加密存储

### 👥 用户管理
- 用户CRUD操作
- 角色分配
- 密码修改
- 用户状态管理

### 🎭 角色管理
- 角色CRUD操作
- 权限分配
- 角色层级管理
- 用户角色关联

### 🔑 权限管理
- 权限CRUD操作
- 树形权限结构
- 角色权限关联
- 资源类型分类

### 🖥️ 前端界面
- 响应式Web界面
- 现代化UI设计
- 实时数据展示
- 用户友好的操作体验

## 🛠️ 技术栈

### 后端
- **框架**: ASP.NET Core 8.0
- **数据库**: PostgreSQL 12+
- **ORM**: Entity Framework Core 8.0
- **身份认证**: ASP.NET Core Identity
- **JWT**: System.IdentityModel.Tokens.Jwt
- **API文档**: Swagger/OpenAPI

### 前端
- **框架**: 纯HTML5 + JavaScript
- **UI框架**: Bootstrap 5
- **图标库**: Font Awesome 6
- **HTTP客户端**: Fetch API

## 📁 项目结构

```
HHPortal/
├── src/
│   ├── backend/
│   │   ├── Controllers/          # API控制器
│   │   ├── Data/                 # 数据库上下文和数据初始化
│   │   ├── DTOs/                 # 数据传输对象
│   │   ├── Models/               # 数据模型
│   │   ├── Services/             # 业务逻辑服务
│   │   ├── appsettings.json      # 配置文件
│   │   └── Program.cs            # 应用入口
│   └── html/
│       ├── index.html            # 主页面
│       └── app.js                # 前端JavaScript
├── doc/
│   └── product_design.md         # 产品设计文档
├── README.md
└── .gitignore
```

## 🚀 快速开始

### 环境要求
- .NET 8.0 SDK
- PostgreSQL 12+
- Node.js 16+ (开发环境)

### 安装步骤

1. **克隆项目**
```bash
git clone [repository-url]
cd hhportal
```

2. **配置数据库**
```bash
# 创建数据库
createdb hhportal

# 更新连接字符串
# 编辑 src/backend/appsettings.json
```

3. **配置JWT密钥**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=hhportal;Username=postgres;Password=yourpassword"
  },
  "Jwt": {
    "Issuer": "HHPortal",
    "Audience": "HHPortalUsers",
    "SecretKey": "your-secret-key-here-must-be-at-least-32-characters-long",
    "ExpiresIn": 60
  }
}
```

4. **运行后端服务**
```bash
cd src/backend
dotnet restore
dotnet run
```

5. **访问应用**
- 后端API: http://localhost:5000
- Swagger文档: http://localhost:5000/swagger
- 前端界面: 打开 src/html/index.html

### 默认管理员账户
- 用户名: admin
- 密码: Admin@123456

## 📊 API端点

### 认证相关
- `POST /api/auth/login` - 用户登录
- `POST /api/auth/refresh` - 刷新令牌
- `POST /api/auth/logout` - 用户登出
- `GET /api/auth/me` - 获取当前用户信息

### 用户管理
- `GET /api/users` - 获取用户列表
- `POST /api/users` - 创建用户
- `GET /api/users/{id}` - 获取用户详情
- `PUT /api/users/{id}` - 更新用户
- `DELETE /api/users/{id}` - 删除用户
- `POST /api/users/{id}/change-password` - 修改密码

### 角色管理
- `GET /api/roles` - 获取角色列表
- `POST /api/roles` - 创建角色
- `GET /api/roles/{id}` - 获取角色详情
- `PUT /api/roles/{id}` - 更新角色
- `DELETE /api/roles/{id}` - 删除角色

### 权限管理
- `GET /api/permissions` - 获取权限列表
- `POST /api/permissions` - 创建权限
- `GET /api/permissions/{id}` - 获取权限详情
- `PUT /api/permissions/{id}` - 更新权限
- `DELETE /api/permissions/{id}` - 删除权限

### 角色权限管理
- `GET /api/rolepermissions/roles/{roleId}/permissions` - 获取角色权限
- `POST /api/rolepermissions/roles/{roleId}/permissions` - 分配权限给角色
- `DELETE /api/rolepermissions/roles/{roleId}/permissions/{permissionId}` - 移除角色权限
- `GET /api/rolepermissions/users/{userId}/permissions` - 获取用户权限

## 🔧 开发指南

### 数据库迁移
```bash
# 创建迁移
dotnet ef migrations add InitialCreate

# 应用迁移
dotnet ef database update
```

### 添加新功能
1. 创建对应的DTO类
2. 实现服务层逻辑
3. 创建控制器
4. 更新前端界面
5. 添加权限控制

### 权限配置
系统使用基于角色的访问控制(RBAC)，权限分为以下类型：
- System: 系统管理权限
- Content: 内容管理权限
- File: 文件管理权限

## 📝 开发规范

### 代码规范
- 使用C#命名规范
- 添加必要的注释
- 遵循RESTful API设计
- 使用异步编程

### 数据库规范
- 使用Guid作为主键
- 添加适当的索引
- 使用迁移管理数据库变更
- 保持数据完整性

## 🐛 常见问题

### 数据库连接失败
- 检查PostgreSQL服务是否运行
- 确认连接字符串配置正确
- 检查防火墙设置

### JWT认证失败
- 检查SecretKey长度(至少32字符)
- 确认JWT配置正确
- 检查令牌有效期

### 跨域问题
- 确保CORS配置正确
- 检查前端API地址配置

## 🤝 贡献指南

1. Fork项目
2. 创建功能分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 创建Pull Request

## 📄 许可证

本项目采用MIT许可证 - 查看 [LICENSE](LICENSE) 文件了解详情

## 🙋‍♂️ 联系方式

如有问题或建议，请通过以下方式联系：
- 提交Issue
- 发送邮件至: admin@hhportal.com

## 🔄 更新日志

### v1.0.0 (2024-01-01)
- 初始版本发布
- 用户认证系统
- 用户管理功能
- 角色管理功能
- 权限管理功能
- 前端管理界面

### 文档
详细设计文档请参见 [产品设计方案](doc/product_design.md)