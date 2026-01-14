# OpenFindBearings API Specification

> **Version**: v1
> **Base URL**: `https://api.openfindbearings.example.com`
> **Auth**: Bearer Token (JWT) in `Authorization` header, except for `/auth/login`

---

## 通用响应格式

所有接口返回 JSON，结构如下：

```json
{
  "code": 200,
  "msg": "success",
  "data": { }
}
```

* `code`: 200 表示成功，非 200 表示错误（如 400、401、500）
* `msg`: 简要描述
* `data`: 业务数据（可能为对象、数组或 null）

---

## 1. 用户认证

### POST /api/auth/login

**微信小程序登录，获取访问令牌。**

​**Request Body (JSON)**​**:**

```json
{
  "code": "wx_login_code_from_miniprogram"
}
```

​**Success Response (200)**​**:**

```json
{
  "code": 200,
  "msg": "success",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.xxxxx",
    "user": {
      "id": 1001,
      "nickname": "轴承老王",
      "avatar": "https://example.com/avatar.jpg",
      "role": 1
    }
  }
}
```

​**Errors**​**:**

* `400`: 微信 code 无效
* `500`: 微信接口调用失败

---

## 2. 供求消息

### POST /api/messages

**发布一条求购或供应消息。**

​**Headers**​**:**

* `Authorization: Bearer <token>`

​**Request Body (JSON)**​**:**

```json
{
  "title": "求购深沟球轴承6201",
  "content": "需要1000套，价格合适长期合作",
  "type": 0,
  "spec": "6201"
}
```

* `type`: `0` **= 求购,** `1` **= 供应**

​**Success Response (200)**​**:**

```json
{
  "code": 200,
  "msg": "发布成功",
  "data": {
    "id": 501
  }
}
```

### GET /api/messages

**获取消息列表（分页）。**

​**Query Parameters**​**:**

* `page` **(int, default=1)**
* `pageSize` **(int, default=10, max=50)**
* `type` **(int, optional): 0=求购, 1=供应, omit=all**
* `keyword` **(string, optional): 标题或内容关键词**

​**Success Response (200)**​**:**

```json
{
  "code": 200,
  "msg": "success",
  "data": {
    "items": [
      {
        "id": 501,
        "userId": 1001,
        "title": "求购深沟球轴承6201",
        "content": "需要1000套...",
        "type": 0,
        "viewCount": 12,
        "createTime": "2026-01-14T10:30:00Z"
      }
    ],
    "total": 125,
    "page": 1,
    "pageSize": 10
  }
}
```

### GET /api/messages/ {id}

**获取消息详情。**

​**Success Response (200)**​**:**

```json
{
  "code": 200,
  "msg": "success",
  "data": {
    "id": 501,
    "userId": 1001,
    "userNickname": "轴承老王",
    "userAvatar": "https://example.com/avatar.jpg",
    "title": "求购深沟球轴承6201",
    "content": "需要1000套...",
    "type": 0,
    "viewCount": 13,
    "createTime": "2026-01-14T10:30:00Z"
  }
}
```

---

## 3. 用户行为

### POST /api/actions

**提交用户对某条消息的行为。**

​**Headers**​**:**

* `Authorization: Bearer <token>`

​**Request Body (JSON)**​**:**

```json
{
  "messageId": 501,
  "actionType": 2
}
```

* `actionType`:
  * `1` **= 浏览（通常前端自动上报）**
  * `2` **= 感兴趣（右滑）**
  * `3` **= 不感兴趣（左滑）**
  * `4` **= 拨打电话**

​**Success Response (200)**​**:**

```json
1{ "code": 200, "msg": "ok", "data": null }
```

---

## 4. 智能与数据产品（演进接口）

> **⚠️ 起步阶段可返回 mock 数据或简单聚合结果。**

### GET /api/recommend/feed

**获取个性化推荐流。**

​**Query Parameters**​**:**

* `page`, `pageSize`

​**Response**​**: 同** `/api/messages` **列表格式。**

---

### GET /api/pricing/index

**获取某型号的价格指数。**

​**Query Parameters**​**:**

* `model` **(string, required): 如 "6201"**

​**Success Response (200)**​**:**

```json
{
  "code": 200,
  "msg": "success",
  "data": {
    "model": "6201",
    "avgPrice": 2.85,
    "minPrice": 2.5,
    "maxPrice": 3.2,
    "quoteCount": 24,
    "lastUpdateTime": "2026-01-14T00:00:00Z"
  }
}
```

---

### GET /api/stats/rankings

**获取热门榜单。**

​**Query Parameters**​**:**

* `type`: "hot" (default), "new"
* `days`: 7 (default)

​**Success Response (200)**​**:**

```json
{
  "code": 200,
  "msg": "success",
  "data": [
    { "model": "6201", "count": 120 },
    { "model": "6304", "count": 98 },
    { "model": "NU205", "count": 76 }
  ]
}
```

---

## 错误码参考

**表格**

| **Code** | **含义**                       |
| ---------- | --------------------------- |
| **400**  | **请求参数错误**               |
| **401**  | **未授权（Token 缺失或无效）**  |
| **404**  | **资源不存在**                 |
| **500**  | **服务器内部错误**              |
