# 企业级管理后台系统 - 第一阶段概要设计文档

## 1. 系统概述
本文档详细描述企业级管理后台系统第一阶段（用户、角色与权限核心模块）的概要设计，基于phase1_requirements.md需求文档编制。

## 2. 总体架构
### 2.1 技术栈选择
- **后端**：Asp.Net Core 8.0
- **数据库**：PostgreSQL 16
- **ORM框架**：Entity Framework Core 8.0
- **认证授权**：ASP.NET Core Identity + JWT
- **前端**：React 18 + TypeScript + Ant Design
- **状态管理**：Redux Toolkit
- **API文档**：Swashbuckle.AspNetCore (OpenAPI 3.0)

### 2.2 系统分层架构
1. **表示层**：React前端应用，提供用户界面与交互
2. **应用层**：ASP.NET Core Web API，处理HTTP请求与响应
3. **领域层**：业务逻辑处理，包含核心业务规则与实体模型
4. **数据访问层**：通过Entity Framework Core实现数据持久化

## 3. 模块设计
### 3.1 用户管理模块
#### 3.1.1 功能架构
- 用户认证（登录/注销）
- 用户信息CRUD
- 密码重置与安全策略
- 用户状态管理

#### 3.1.2 核心实体
```
User {
  Id: Guid (PK)
  Username: string
  Email: string
  PasswordHash: string (使用ASP.NET Core Identity加密)
  FullName: string
  Status: Enum (Active/Inactive)
  CreatedAt: DateTime
  UpdatedAt: DateTime
}
```

### 3.2 角色管理模块
#### 3.2.1 功能架构
- 角色CRUD操作
- 用户-角色多对多关联
- 预设角色模板（超级管理员、系统管理员、普通用户）

#### 3.2.2 核心实体
```
Role {
  Id: Guid (PK)
  Name: string
  Description: string
  CreatedAt: DateTime
  UpdatedAt: DateTime
}

UserRole {
  UserId: Guid (FK)
  RoleId: Guid (FK)
  (复合主键)
}
```

### 3.3 权限管理模块
#### 3.3.1 功能架构
- RBAC3权限模型实现
- 权限粒度控制（菜单/按钮/接口）
- 角色-权限分配机制

#### 3.3.2 核心实体
```
Permission {
  Id: Guid (PK)
  Name: string
  Code: string (唯一标识)
  Description: string
  ResourceType: Enum (Menu/Button/Api)
  ResourcePath: string
}

RolePermission {
  RoleId: Guid (FK)
  PermissionId: Guid (FK)
  (复合主键)
}
```

## 4. 数据库设计
### 4.1 实体关系图(ERD)
```
User 1──* UserRole *──1 Role 1──* RolePermission *──1 Permission
```

### 4.2 详细表结构
#### User表
| 字段名 | 类型 | 约束 | 说明 |
|--------|------|------|------|
| Id | Guid | PK | 用户ID |
| Username | varchar(50) | 唯一,非空 | 用户名 |
| Email | varchar(100) | 唯一,非空 | 邮箱 |
| PasswordHash | varchar(255) | 非空 | 密码哈希 |
| FullName | varchar(100) | | 姓名 |
| Status | smallint | 非空 | 状态(0-禁用,1-启用) |
| LastLoginTime | datetime | | 最后登录时间 |
| CreatedAt | datetime | 非空 | 创建时间 |
| UpdatedAt | datetime | 非空 | 更新时间 |

#### Role表
| 字段名 | 类型 | 约束 | 说明 |
|--------|------|------|------|
| Id | Guid | PK | 角色ID |
| Name | varchar(50) | 唯一,非空 | 角色名称 |
| Description | text | | 描述 |
| CreatedAt | datetime | 非空 | 创建时间 |
| UpdatedAt | datetime | 非空 | 更新时间 |

#### Permission表
| 字段名 | 类型 | 约束 | 说明 |
|--------|------|------|------|
| Id | Guid | PK | 权限ID |
| Name | varchar(100) | 非空 | 权限名称 |
| Code | varchar(100) | 唯一,非空 | 权限编码 |
| ResourceType | smallint | 非空 | 资源类型(1-菜单,2-按钮,3-接口) |
| ResourcePath | varchar(255) | | 资源路径 |
| ParentId | Guid | FK | 父权限ID |

### 4.3 索引策略
- User表：Username(唯一), Email(唯一), Status
- Role表：Name(唯一)
- Permission表：Code(唯一), ResourceType, ParentId

## 5. API接口设计
### 5.1 接口规范
- RESTful设计风格
- 基础路径：/api/v1
- 统一响应格式：
```json
{
  "code": 200,
  "message": "success",
  "data": {}
}
```

### 5.2 核心接口示例
#### 认证模块
- POST /api/v1/auth/login - 用户登录
  请求体: {"username":"string","password":"string","rememberMe":boolean}
  响应: {"token":"string","refreshToken":"string","expiresIn":3600}

- POST /api/v1/auth/refresh - 刷新令牌
- POST /api/v1/auth/logout - 用户注销

#### 用户管理
- GET /api/v1/users - 获取用户列表(分页)
  参数: page=1&size=10&keyword=xxx
- GET /api/v1/users/{id} - 获取用户详情
- POST /api/v1/users - 创建用户
- PUT /api/v1/users/{id} - 更新用户
- DELETE /api/v1/users/{id} - 删除用户
- PUT /api/v1/users/{id}/status - 修改用户状态

#### 角色管理
- GET /api/v1/roles - 获取角色列表
- POST /api/v1/roles - 创建角色
- PUT /api/v1/roles/{id} - 更新角色
- DELETE /api/v1/roles/{id} - 删除角色
- GET /api/v1/roles/{id}/permissions - 获取角色权限
- PUT /api/v1/roles/{id}/permissions - 分配角色权限

#### 权限管理
- GET /api/v1/permissions - 获取权限列表
- POST /api/v1/permissions - 创建权限
- PUT /api/v1/permissions/{id} - 更新权限

## 6. 安全设计
### 6.1 认证机制
- JWT令牌有效期：默认2小时
- 刷新令牌机制：支持令牌续期
- 记住登录状态：最长30天

### 6.2 密码安全
- 加密存储：ASP.NET Core Identity哈希算法
- 复杂度要求：≥8位，包含大小写字母、数字和特殊字符
- 历史密码校验：防止重复使用最近5次密码

### 6.3 访问控制
- 基于角色的权限验证
- API请求频率限制
- 敏感操作日志记录

## 7. 性能优化
### 7.1 数据库优化
- 合理使用索引
- 查询语句优化
- 分页查询实现

### 7.2 应用层优化
- 接口数据缓存(Redis)
- 前端资源压缩与CDN
- 异步处理长时间任务

## 8. 部署与运维
### 8.1 开发环境配置
- .NET 8 SDK
- Node.js 16+
- PostgreSQL 16
- Redis 6+

### 8.2 构建流程
- 前端：npm run build
- 后端：dotnet build -c Release

## 9. 风险评估
### 9.1 潜在风险
- 数据安全风险：敏感信息泄露
- 性能风险：高并发下响应延迟
- 兼容性风险：浏览器适配问题

### 9.2 应对策略
- 数据加密存储与传输
- 性能测试与瓶颈优化
- 多浏览器兼容性测试

## 8. 验收标准
- 功能覆盖率：100%覆盖需求文档功能点
- 接口响应时间：≤300ms
- 安全合规：符合OWASP Top 10安全标准