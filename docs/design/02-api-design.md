# API 设计文档

## API 规范

### 基础 URL

```
开发环境: http://localhost:5000/api
生产环境: https://api.openfindbearings.com/api
```

### 请求头

```
Content-Type: application/json
Authorization: Bearer {access_token}
```

### 统一响应格式

```json
{
  "success": true,
  "message": "操作成功",
  "data": {},
  "errorCode": null,
  "timestamp": "2024-01-01T00:00:00Z"
}
```

## API 端点列表

### 1. 认证服务 (Auth Service)

#### 1.1 用户注册

```http
POST /api/auth/register
Content-Type: application/json

{
  "username": "string",
  "password": "string",
  "phoneNumber": "string",
  "email": "string",
  "fullName": "string"
}
```

#### 1.2 用户登录

```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "string",
  "password": "string"
}
```

响应：
```json
{
  "success": true,
  "data": {
    "accessToken": "string",
    "refreshToken": "string",
    "expiresIn": 3600,
    "user": {
      "id": 0,
      "username": "string",
      "fullName": "string"
    }
  }
}
```

#### 1.3 刷新令牌

```http
POST /api/auth/refresh-token
Content-Type: application/json

{
  "refreshToken": "string"
}
```

#### 1.4 微信登录

```http
POST /api/auth/wechat-login
Content-Type: application/json

{
  "code": "string"
}
```

---

### 2. 用户服务 (User Service)

#### 2.1 获取当前用户信息

```http
GET /api/users/profile
Authorization: Bearer {token}
```

#### 2.2 更新用户信息

```http
PUT /api/users/profile
Authorization: Bearer {token}
Content-Type: application/json

{
  "fullName": "string",
  "avatarUrl": "string"
}
```

#### 2.3 创建企业信息

```http
POST /api/users/companies
Authorization: Bearer {token}
Content-Type: application/json

{
  "companyName": "string",
  "unifiedSocialCreditCode": "string",
  "contactPerson": "string",
  "contactPhone": "string",
  "contactEmail": "string",
  "province": "string",
  "city": "string",
  "address": "string"
}
```

#### 2.4 企业认证申请

```http
POST /api/users/companies/certify
Authorization: Bearer {token}
Content-Type: multipart/form-data

{
  "businessLicense": "file",
  "taxCertificate": "file"
}
```

#### 2.5 获取企业信用评级

```http
GET /api/users/credit/{companyId}
```

---

### 3. 轴承服务 (Bearing Service)

#### 3.1 获取轴承列表

```http
GET /api/bearings?pageIndex=1&pageSize=20&keyword=6204&brandId=1
```

响应：
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "modelNumber": "6204",
      "brand": {
        "id": 1,
        "name": "SKF"
      },
      "innerDiameter": 20,
      "outerDiameter": 47,
      "width": 14
    }
  ],
  "pageIndex": 1,
  "pageSize": 20,
  "totalCount": 100,
  "totalPages": 5
}
```

#### 3.2 获取轴承详情

```http
GET /api/bearings/{id}
```

#### 3.3 搜索建议

```http
GET /api/bearings/suggest?keyword=620
```

#### 3.4 获取等效型号

```http
GET /api/bearings/{id}/equivalent
```

---

### 4. 库存服务 (Inventory Service)

#### 4.1 获取库存列表

```http
GET /api/inventory?pageIndex=1&pageSize=20&bearingModelId=1
```

#### 4.2 添加库存

```http
POST /api/inventory
Authorization: Bearer {token}
Content-Type: application/json

{
  "bearingModelId": 1,
  "quantity": 100,
  "price": 25.50,
  "qualityLevel": "Original",
  "origin": "Sweden",
  "warehouseLocation": "上海"
}
```

#### 4.3 更新库存

```http
PUT /api/inventory/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "quantity": 150,
  "price": 26.00
}
```

#### 4.4 批量导入库存

```http
POST /api/inventory/batch-import
Authorization: Bearer {token}
Content-Type: multipart/form-data

{
  "file": "inventory.xlsx"
}
```

#### 4.5 导出库存

```http
GET /api/inventory/export
Authorization: Bearer {token}
```

---

### 5. 询价交易服务 (Inquiry Service)

#### 5.1 获取询价列表

```http
GET /api/inquiries?pageIndex=1&pageSize=20&status=Open
```

#### 5.2 发布寻货询价

```http
POST /api/inquiries
Authorization: Bearer {token}
Content-Type: application/json

{
  "bearingModelId": 1,
  "quantity": 50,
  "targetPrice": 30.00,
  "qualityLevel": "Original",
  "urgent": false,
  "requiredDate": "2024-12-31",
  "description": "急需一批SKF 6204轴承"
}
```

#### 5.3 获取询价详情

```http
GET /api/inquiries/{id}
```

#### 5.4 报价

```http
POST /api/inquiries/{inquiryId}/quotations
Authorization: Bearer {token}
Content-Type: application/json

{
  "inventoryId": 10,
  "unitPrice": 28.00,
  "quantity": 50,
  "availableQuantity": 100,
  "validityDays": 7,
  "deliveryPeriod": 3,
  "paymentTerms": "款到发货",
  "description": "现货，3天内发货"
}
```

#### 5.5 获取报价列表

```http
GET /api/inquiries/{inquiryId}/quotations
```

#### 5.6 接受报价

```http
PUT /api/quotations/{id}/accept
Authorization: Bearer {token}
```

#### 5.7 创建订单

```http
POST /api/orders
Authorization: Bearer {token}
Content-Type: application/json

{
  "quotationId": 1
}
```

#### 5.8 获取订单列表

```http
GET /api/orders?pageIndex=1&pageSize=20&status=Pending
```

#### 5.9 获取订单详情

```http
GET /api/orders/{id}
```

---

### 6. 匹配服务 (Match Service)

#### 6.1 为询价匹配供应商

```http
POST /api/match/inquiry
Authorization: Bearer {token}
Content-Type: application/json

{
  "inquiryId": 1
}
```

响应：
```json
{
  "success": true,
  "data": [
    {
      "companyId": 10,
      "companyName": "上海轴承有限公司",
      "inventoryId": 100,
      "score": 95.5,
      "availableQuantity": 200,
      "price": 28.00,
      "reason": "库存充足，价格优惠"
    }
  ]
}
```

#### 6.2 获取推荐列表

```http
GET /api/match/recommendations
Authorization: Bearer {token}
```

---

## 错误码

| 错误码 | 说明 |
|--------|------|
| 200 | 成功 |
| 400 | 请求参数错误 |
| 401 | 未授权 |
| 403 | 禁止访问 |
| 404 | 资源不存在 |
| 409 | 资源冲突 |
| 500 | 服务器内部错误 |
| 1001 | 用户名或密码错误 |
| 1002 | 用户已存在 |
| 1003 | Token 无效 |
| 1004 | Token 已过期 |
| 2001 | 库存不足 |
| 2002 | 轴承型号不存在 |
| 3001 | 询价已关闭 |
| 3002 | 已报价 |
