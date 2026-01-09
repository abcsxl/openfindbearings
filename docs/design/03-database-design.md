# 数据库设计文档

## 数据库列表

| 数据库 | 说明 | 连接字符串 |
|--------|------|------------|
| auth_db | 用户认证数据库 | `Host=localhost;Database=auth_db;Username=bearing;Password=xxx` |
| user_db | 用户信息数据库 | `Host=localhost;Database=user_db;Username=bearing;Password=xxx` |
| bearing_db | 轴承主数据库 | `Host=localhost;Database=bearing_db;Username=bearing;Password=xxx` |
| inventory_db | 库存管理数据库 | `Host=localhost;Database=inventory_db;Username=bearing;Password=xxx` |
| inquiry_db | 询价交易数据库 | `Host=localhost;Database=inquiry_db;Username=bearing;Password=xxx` |
| match_db | 匹配记录数据库 | `Host=localhost;Database=match_db;Username=bearing;Password=xxx` |
| message_db | 消息系统数据库 | `Host=localhost;Database=message_db;Username=bearing;Password=xxx` |

---

## 1. auth_db - 用户认证数据库

### 1.1 roles - 角色表

| 字段 | 类型 | 说明 | 约束 |
|------|------|------|------|
| id | SERIAL | 主键 | PRIMARY KEY |
| name | VARCHAR(50) | 角色名称 | UNIQUE, NOT NULL |
| description | VARCHAR(200) | 角色描述 | |
| created_at | TIMESTAMP | 创建时间 | DEFAULT CURRENT_TIMESTAMP |

默认角色：
- Supplier - 供应商
- Admin - 系统管理员
- SuperAdmin - 超级管理员

### 1.2 users - 用户表

| 字段 | 类型 | 说明 | 约束 |
|------|------|------|------|
| id | BIGSERIAL | 主键 | PRIMARY KEY |
| username | VARCHAR(50) | 用户名 | UNIQUE, NOT NULL |
| phone_number | VARCHAR(20) | 手机号 | UNIQUE |
| email | VARCHAR(100) | 邮箱 | UNIQUE |
| password_hash | VARCHAR(255) | 密码哈希 | NOT NULL |
| wechat_openid | VARCHAR(100) | 微信OpenID | UNIQUE |
| avatar_url | VARCHAR(500) | 头像URL | |
| full_name | VARCHAR(100) | 姓名 | |
| is_active | BOOLEAN | 是否激活 | DEFAULT true |
| is_phone_verified | BOOLEAN | 手机是否验证 | DEFAULT false |
| last_login_at | TIMESTAMP | 最后登录时间 | |
| created_at | TIMESTAMP | 创建时间 | DEFAULT CURRENT_TIMESTAMP |
| updated_at | TIMESTAMP | 更新时间 | DEFAULT CURRENT_TIMESTAMP |

约束：
- `chk_has_contact`: 手机号、邮箱、微信OpenID 至少一个不为空

索引：
- `idx_users_phone`: phone_number
- `idx_users_email`: email
- `idx_users_wechat`: wechat_openid

### 1.3 user_roles - 用户角色关联表

| 字段 | 类型 | 说明 | 约束 |
|------|------|------|------|
| user_id | BIGINT | 用户ID | PRIMARY KEY, FOREIGN KEY -> users(id) ON DELETE CASCADE |
| role_id | INTEGER | 角色ID | PRIMARY KEY, FOREIGN KEY -> roles(id) ON DELETE CASCADE |
| assigned_at | TIMESTAMP | 分配时间 | DEFAULT CURRENT_TIMESTAMP |

### 1.4 refresh_tokens - 刷新令牌表

| 字段 | 类型 | 说明 | 约束 |
|------|------|------|------|
| id | BIGSERIAL | 主键 | PRIMARY KEY |
| user_id | BIGINT | 用户ID | FOREIGN KEY -> users(id) ON DELETE CASCADE |
| token | VARCHAR(500) | 令牌 | UNIQUE, NOT NULL |
| expires_at | TIMESTAMP | 过期时间 | NOT NULL |
| created_at | TIMESTAMP | 创建时间 | DEFAULT CURRENT_TIMESTAMP |
| revoked_at | TIMESTAMP | 撤销时间 | |
| replaced_by_token | VARCHAR(500) | 替换令牌 | |
| device_info | VARCHAR(200) | 设备信息 | |

索引：
- `idx_refresh_tokens_user`: user_id
- `idx_refresh_tokens_expires`: expires_at

---

## 2. user_db - 用户信息数据库

### 2.1 companies - 企业信息表

| 字段 | 类型 | 说明 | 约束 |
|------|------|------|------|
| id | BIGSERIAL | 主键 | PRIMARY KEY |
| user_id | BIGINT | 用户ID | NOT NULL |
| company_name | VARCHAR(200) | 企业名称 | NOT NULL |
| unified_social_credit_code | VARCHAR(50) | 统一社会信用代码 | UNIQUE |
| business_license_url | VARCHAR(500) | 营业执照URL | |
| contact_person | VARCHAR(50) | 联系人 | |
| contact_phone | VARCHAR(20) | 联系电话 | |
| contact_email | VARCHAR(100) | 联系邮箱 | |
| province | VARCHAR(50) | 省份 | |
| city | VARCHAR(50) | 城市 | |
| address | VARCHAR(500) | 详细地址 | |
| business_scope | TEXT | 经营范围 | |
| certification_status | VARCHAR(20) | 认证状态 | DEFAULT 'Pending' (Pending/Approved/Rejected) |
| certification_at | TIMESTAMP | 认证时间 | |
| certified_by | BIGINT | 认证人ID | |
| reject_reason | TEXT | 拒绝原因 | |
| credit_score | INTEGER | 信用评分 | DEFAULT 100 |
| total_orders | INTEGER | 总订单数 | DEFAULT 0 |
| completed_orders | INTEGER | 完成订单数 | DEFAULT 0 |
| created_at | TIMESTAMP | 创建时间 | DEFAULT CURRENT_TIMESTAMP |
| updated_at | TIMESTAMP | 更新时间 | DEFAULT CURRENT_TIMESTAMP |

索引：
- `idx_companies_user`: user_id
- `idx_companies_cert_status`: certification_status
- `idx_companies_location`: province, city

### 2.2 company_documents - 企业认证文件表

| 字段 | 类型 | 说明 | 约束 |
|------|------|------|------|
| id | BIGSERIAL | 主键 | PRIMARY KEY |
| company_id | BIGINT | 企业ID | FOREIGN KEY -> companies(id) ON DELETE CASCADE |
| document_type | VARCHAR(50) | 文件类型 | NOT NULL (BusinessLicense/TaxCertificate/etc.) |
| document_url | VARCHAR(500) | 文件URL | NOT NULL |
| file_name | VARCHAR(200) | 文件名 | |
| file_size | BIGINT | 文件大小 | |
| upload_at | TIMESTAMP | 上传时间 | DEFAULT CURRENT_TIMESTAMP |

### 2.3 credit_records - 信用记录表

| 字段 | 类型 | 说明 | 约束 |
|------|------|------|------|
| id | BIGSERIAL | 主键 | PRIMARY KEY |
| company_id | BIGINT | 企业ID | FOREIGN KEY -> companies(id) ON DELETE CASCADE |
| score_change | INTEGER | 分数变化 | NOT NULL |
| reason | VARCHAR(200) | 原因 | |
| order_id | BIGINT | 关联订单ID | |
| created_at | TIMESTAMP | 创建时间 | DEFAULT CURRENT_TIMESTAMP |

索引：
- `idx_credit_records_company`: company_id

---

## 3. bearing_db - 轴承主数据库

### 3.1 brands - 品牌表

| 字段 | 类型 | 说明 | 约束 |
|------|------|------|------|
| id | SERIAL | 主键 | PRIMARY KEY |
| name | VARCHAR(100) | 品牌名称 | UNIQUE, NOT NULL |
| name_en | VARCHAR(100) | 英文名称 | |
| country | VARCHAR(50) | 国家 | |
| logo_url | VARCHAR(500) | Logo URL | |
| website | VARCHAR(200) | 官网 | |
| description | TEXT | 描述 | |
| is_active | BOOLEAN | 是否启用 | DEFAULT true |
| created_at | TIMESTAMP | 创建时间 | DEFAULT CURRENT_TIMESTAMP |

### 3.2 categories - 分类表

| 字段 | 类型 | 说明 | 约束 |
|------|------|------|------|
| id | SERIAL | 主键 | PRIMARY KEY |
| parent_id | INTEGER | 父分类ID | FOREIGN KEY -> categories(id) |
| name | VARCHAR(100) | 分类名称 | NOT NULL |
| name_en | VARCHAR(100) | 英文名称 | |
| level | INTEGER | 层级 | DEFAULT 1 |
| path | VARCHAR(500) | 路径 | 如 /1/2/3 |
| sort_order | INTEGER | 排序 | DEFAULT 0 |
| is_active | BOOLEAN | 是否启用 | DEFAULT true |
| created_at | TIMESTAMP | 创建时间 | DEFAULT CURRENT_TIMESTAMP |

索引：
- `idx_categories_parent`: parent_id
- `idx_categories_path`: path

### 3.3 bearing_models - 轴承型号主表

| 字段 | 类型 | 说明 | 约束 |
|------|------|------|------|
| id | BIGSERIAL | 主键 | PRIMARY KEY |
| model_number | VARCHAR(100) | 型号 | NOT NULL |
| brand_id | INTEGER | 品牌ID | FOREIGN KEY -> brands(id) |
| category_id | INTEGER | 分类ID | FOREIGN KEY -> categories(id) |
| inner_diameter | DECIMAL(10,2) | 内径 | |
| outer_diameter | DECIMAL(10,2) | 外径 | |
| width | DECIMAL(10,2) | 宽度 | |
| weight | DECIMAL(10,3) | 重量 | |
| material | VARCHAR(100) | 材质 | |
| cage_material | VARCHAR(100) | 保持架材质 | |
| seal_type | VARCHAR(50) | 密封类型 | |
| precision_level | VARCHAR(20) | 精度等级 | P0/P6/P5/P4/P2 |
| clearance | VARCHAR(20) | 游隙 | C2/CN/C3/C4/C5 |
| description | TEXT | 描述 | |
| keywords | TEXT | 搜索关键词 | |
| specifications | JSONB | 其他规格参数 | |
| is_standard | BOOLEAN | 是否标准件 | DEFAULT true |
| equivalent_models | TEXT[] | 等效型号数组 | |
| created_at | TIMESTAMP | 创建时间 | DEFAULT CURRENT_TIMESTAMP |
| updated_at | TIMESTAMP | 更新时间 | DEFAULT CURRENT_TIMESTAMP |

约束：
- `uk_brand_model`: (brand_id, model_number) UNIQUE

索引：
- `idx_bearing_models_number`: model_number
- `idx_bearing_models_brand`: brand_id
- `idx_bearing_models_category`: category_id
- `idx_bearing_models_specs`: specifications (GIN)
- `idx_bearing_models_keywords`: keywords (GIN)

### 3.4 bearing_aliases - 轴承别名表

| 字段 | 类型 | 说明 | 约束 |
|------|------|------|------|
| id | BIGSERIAL | 主键 | PRIMARY KEY |
| bearing_model_id | BIGINT | 轴承ID | FOREIGN KEY -> bearing_models(id) ON DELETE CASCADE |
| alias | VARCHAR(100) | 别名 | NOT NULL |
| source | VARCHAR(50) | 别名来源 | |
| created_at | TIMESTAMP | 创建时间 | DEFAULT CURRENT_TIMESTAMP |

索引：
- `idx_bearing_aliases_model`: bearing_model_id
- `idx_bearing_aliases_alias`: alias

---

## 4. inventory_db - 库存管理数据库

### 4.1 inventory_items - 库存表

| 字段 | 类型 | 说明 | 约束 |
|------|------|------|------|
| id | BIGSERIAL | 主键 | PRIMARY KEY |
| company_id | BIGINT | 企业ID | NOT NULL |
| bearing_model_id | BIGINT | 轴承型号ID | NOT NULL |
| quantity | INTEGER | 库存数量 | DEFAULT 0 |
| reserved_quantity | INTEGER | 已预留数量 | DEFAULT 0 |
| available_quantity | INTEGER | 可用数量 | GENERATED (quantity - reserved_quantity) |
| price | DECIMAL(12,2) | 单价 | |
| min_quantity | INTEGER | 最小起订量 | DEFAULT 1 |
| price_negotiable | BOOLEAN | 价格可议 | DEFAULT false |
| quality_level | VARCHAR(20) | 质量等级 | DEFAULT 'Original' (Original/OEM/Compatible) |
| origin | VARCHAR(100) | 产地 | |
| production_date | DATE | 生产日期 | |
| batch_number | VARCHAR(100) | 批号 | |
| warehouse_location | VARCHAR(200) | 仓库位置 | |
| description | TEXT | 描述 | |
| images | TEXT[] | 图片URL数组 | |
| is_active | BOOLEAN | 是否启用 | DEFAULT true |
| is_verified | BOOLEAN | 是否已验证 | DEFAULT false |
| created_at | TIMESTAMP | 创建时间 | DEFAULT CURRENT_TIMESTAMP |
| updated_at | TIMESTAMP | 更新时间 | DEFAULT CURRENT_TIMESTAMP |
| last_price_update_at | TIMESTAMP | 价格更新时间 | |

索引：
- `idx_inventory_company`: company_id
- `idx_inventory_bearing`: bearing_model_id
- `idx_inventory_quantity`: quantity WHERE quantity > 0
- `idx_inventory_updated`: updated_at

### 4.2 inventory_records - 库存变动记录表

| 字段 | 类型 | 说明 | 约束 |
|------|------|------|------|
| id | BIGSERIAL | 主键 | PRIMARY KEY |
| inventory_id | BIGINT | 库存ID | FOREIGN KEY -> inventory_items(id) ON DELETE CASCADE |
| change_type | VARCHAR(20) | 变更类型 | NOT NULL (In/Out/Adjust/Reserve/Release) |
| quantity_before | INTEGER | 变更前数量 | NOT NULL |
| quantity_change | INTEGER | 变更数量 | NOT NULL |
| quantity_after | INTEGER | 变更后数量 | NOT NULL |
| reference_type | VARCHAR(50) | 关联类型 | Order/Manual/etc. |
| reference_id | BIGINT | 关联ID | |
| remark | TEXT | 备注 | |
| created_by | BIGINT | 创建人ID | |
| created_at | TIMESTAMP | 创建时间 | DEFAULT CURRENT_TIMESTAMP |

索引：
- `idx_inventory_records_inventory`: inventory_id
- `idx_inventory_records_created`: created_at

### 4.3 price_history - 价格历史表

| 字段 | 类型 | 说明 | 约束 |
|------|------|------|------|
| id | BIGSERIAL | 主键 | PRIMARY KEY |
| inventory_id | BIGINT | 库存ID | FOREIGN KEY -> inventory_items(id) ON DELETE CASCADE |
| old_price | DECIMAL(12,2) | 原价格 | |
| new_price | DECIMAL(12,2) | 新价格 | NOT NULL |
| change_reason | VARCHAR(200) | 变更原因 | |
| changed_by | BIGINT | 变更人ID | |
| created_at | TIMESTAMP | 创建时间 | DEFAULT CURRENT_TIMESTAMP |

索引：
- `idx_price_history_inventory`: inventory_id

---

## 5. inquiry_db - 询价交易数据库

### 5.1 inquiries - 寻货询价表

| 字段 | 类型 | 说明 | 约束 |
|------|------|------|------|
| id | BIGSERIAL | 主键 | PRIMARY KEY |
| buyer_company_id | BIGINT | 买家企业ID | NOT NULL |
| bearing_model_id | BIGINT | 轴承型号ID | |
| custom_model | VARCHAR(100) | 自定义型号 | |
| brand_id | INTEGER | 品牌ID | |
| quantity | INTEGER | 数量 | NOT NULL |
| target_price | DECIMAL(12,2) | 目标价格 | |
| quality_level | VARCHAR(20) | 质量等级 | |
| urgent | BOOLEAN | 是否紧急 | DEFAULT false |
| required_date | DATE | 要求交货日期 | |
| description | TEXT | 描述 | |
| status | VARCHAR(20) | 状态 | DEFAULT 'Open' (Open/Closed/Cancelled/Expired) |
| valid_until | TIMESTAMP | 有效期 | |
| view_count | INTEGER | 浏览次数 | DEFAULT 0 |
| quotation_count | INTEGER | 报价次数 | DEFAULT 0 |
| created_at | TIMESTAMP | 创建时间 | DEFAULT CURRENT_TIMESTAMP |
| updated_at | TIMESTAMP | 更新时间 | DEFAULT CURRENT_TIMESTAMP |
| closed_at | TIMESTAMP | 关闭时间 | |

索引：
- `idx_inquiries_buyer`: buyer_company_id
- `idx_inquiries_bearing`: bearing_model_id
- `idx_inquiries_status`: status
- `idx_inquiries_created`: created_at DESC
- `idx_inquiries_urgent`: urgent WHERE urgent = true

### 5.2 quotations - 报价表

| 字段 | 类型 | 说明 | 约束 |
|------|------|------|------|
| id | BIGSERIAL | 主键 | PRIMARY KEY |
| inquiry_id | BIGINT | 询价ID | FOREIGN KEY -> inquiries(id) ON DELETE CASCADE |
| supplier_company_id | BIGINT | 供应商企业ID | NOT NULL |
| inventory_id | BIGINT | 库存ID | |
| unit_price | DECIMAL(12,2) | 单价 | NOT NULL |
| total_price | DECIMAL(12,2) | 总价 | GENERATED (unit_price * quantity) |
| quantity | INTEGER | 数量 | NOT NULL |
| available_quantity | INTEGER | 可用数量 | NOT NULL |
| validity_days | INTEGER | 报价有效期(天) | DEFAULT 7 |
| delivery_period | INTEGER | 交货周期(天) | |
| payment_terms | VARCHAR(200) | 付款方式 | |
| delivery_terms | VARCHAR(200) | 交货条款 | |
| description | TEXT | 描述 | |
| attachments | TEXT[] | 附件URL数组 | |
| status | VARCHAR(20) | 状态 | DEFAULT 'Pending' (Pending/Accepted/Rejected/Cancelled/Expired) |
| reject_reason | TEXT | 拒绝原因 | |
| created_at | TIMESTAMP | 创建时间 | DEFAULT CURRENT_TIMESTAMP |
| updated_at | TIMESTAMP | 更新时间 | DEFAULT CURRENT_TIMESTAMP |
| expires_at | TIMESTAMP | 过期时间 | |

约束：
- `uk_quotation`: (inquiry_id, supplier_company_id) UNIQUE

索引：
- `idx_quotations_inquiry`: inquiry_id
- `idx_quotations_supplier`: supplier_company_id
- `idx_quotations_status`: status
- `idx_quotations_created`: created_at DESC

### 5.3 orders - 订单表

| 字段 | 类型 | 说明 | 约束 |
|------|------|------|------|
| id | BIGSERIAL | 主键 | PRIMARY KEY |
| order_number | VARCHAR(50) | 订单号 | UNIQUE, NOT NULL |
| quotation_id | BIGINT | 报价ID | FOREIGN KEY -> quotations(id) |
| buyer_company_id | BIGINT | 买家企业ID | NOT NULL |
| supplier_company_id | BIGINT | 供应商企业ID | NOT NULL |
| bearing_model_id | BIGINT | 轴承型号ID | |
| model_number | VARCHAR(100) | 型号 | NOT NULL |
| brand_id | INTEGER | 品牌ID | |
| quantity | INTEGER | 数量 | NOT NULL |
| unit_price | DECIMAL(12,2) | 单价 | NOT NULL |
| total_amount | DECIMAL(12,2) | 总金额 | NOT NULL |
| delivery_period | INTEGER | 交货周期(天) | |
| delivery_address | TEXT | 交货地址 | |
| payment_terms | VARCHAR(200) | 付款方式 | |
| invoice_type | VARCHAR(50) | 发票类型 | |
| status | VARCHAR(20) | 订单状态 | DEFAULT 'Pending' |
| payment_status | VARCHAR(20) | 支付状态 | DEFAULT 'Unpaid' |
| ship_status | VARCHAR(20) | 发货状态 | DEFAULT 'NotShipped' |
| confirmed_at | TIMESTAMP | 确认时间 | |
| shipped_at | TIMESTAMP | 发货时间 | |
| delivered_at | TIMESTAMP | 交付时间 | |
| completed_at | TIMESTAMP | 完成时间 | |
| cancelled_at | TIMESTAMP | 取消时间 | |
| created_at | TIMESTAMP | 创建时间 | DEFAULT CURRENT_TIMESTAMP |
| updated_at | TIMESTAMP | 更新时间 | DEFAULT CURRENT_TIMESTAMP |

订单状态：
- Pending - 待确认
- Confirmed - 已确认
- InProduction - 生产中
- Shipped - 已发货
- Delivered - 已交付
- Completed - 已完成
- Cancelled - 已取消

索引：
- `idx_orders_number`: order_number
- `idx_orders_buyer`: buyer_company_id
- `idx_orders_supplier`: supplier_company_id
- `idx_orders_status`: status
- `idx_orders_created`: created_at DESC

### 5.4 order_status_history - 订单状态历史表

| 字段 | 类型 | 说明 | 约束 |
|------|------|------|------|
| id | BIGSERIAL | 主键 | PRIMARY KEY |
| order_id | BIGINT | 订单ID | FOREIGN KEY -> orders(id) ON DELETE CASCADE |
| old_status | VARCHAR(20) | 旧状态 | |
| new_status | VARCHAR(20) | 新状态 | NOT NULL |
| remark | TEXT | 备注 | |
| created_by | BIGINT | 操作人ID | |
| created_at | TIMESTAMP | 创建时间 | DEFAULT CURRENT_TIMESTAMP |

索引：
- `idx_order_status_history_order`: order_id

---

## 6. match_db - 匹配记录数据库

### 6.1 match_records - 匹配记录表

| 字段 | 类型 | 说明 | 约束 |
|------|------|------|------|
| id | BIGSERIAL | 主键 | PRIMARY KEY |
| inquiry_id | BIGINT | 询价ID | NOT NULL |
| company_id | BIGINT | 企业ID | NOT NULL |
| inventory_id | BIGINT | 库存ID | |
| match_score | DECIMAL(5,2) | 匹配度评分 | NOT NULL (0-100) |
| match_reasons | JSONB | 匹配原因详情 | |
| has_stock | BOOLEAN | 有库存 | NOT NULL |
| available_quantity | INTEGER | 可用数量 | |
| price | DECIMAL(12,2) | 价格 | |
| status | VARCHAR(20) | 状态 | DEFAULT 'Pending' (Pending/Notified/Rejected) |
| notified_at | TIMESTAMP | 通知时间 | |
| created_at | TIMESTAMP | 创建时间 | DEFAULT CURRENT_TIMESTAMP |

索引：
- `idx_match_records_inquiry`: inquiry_id
- `idx_match_records_company`: company_id
- `idx_match_records_score`: match_score DESC

### 6.2 recommendations - 推荐记录表

| 字段 | 类型 | 说明 | 约束 |
|------|------|------|------|
| id | BIGSERIAL | 主键 | PRIMARY KEY |
| user_id | BIGINT | 用户ID | NOT NULL |
| recommendation_type | VARCHAR(50) | 推荐类型 | NOT NULL (Inquiry/Supply/Bearing) |
| item_id | BIGINT | 项目ID | NOT NULL |
| score | DECIMAL(5,2) | 推荐分数 | NOT NULL |
| reason | TEXT | 推荐原因 | |
| is_clicked | BOOLEAN | 是否点击 | DEFAULT false |
| created_at | TIMESTAMP | 创建时间 | DEFAULT CURRENT_TIMESTAMP |

索引：
- `idx_recommendations_user`: user_id
- `idx_recommendations_type`: recommendation_type

---

## 7. message_db - 消息系统数据库

### 7.1 message_threads - 消息会话表

| 字段 | 类型 | 说明 | 约束 |
|------|------|------|------|
| id | BIGSERIAL | 主键 | PRIMARY KEY |
| subject | VARCHAR(200) | 主题 | |
| related_type | VARCHAR(50) | 关联类型 | Inquiry/Order/etc. |
| related_id | BIGINT | 关联ID | |
| created_at | TIMESTAMP | 创建时间 | DEFAULT CURRENT_TIMESTAMP |
| updated_at | TIMESTAMP | 更新时间 | DEFAULT CURRENT_TIMESTAMP |

### 7.2 messages - 消息表

| 字段 | 类型 | 说明 | 约束 |
|------|------|------|------|
| id | BIGSERIAL | 主键 | PRIMARY KEY |
| thread_id | BIGINT | 会话ID | FOREIGN KEY -> message_threads(id) ON DELETE CASCADE |
| sender_id | BIGINT | 发送人ID | NOT NULL |
| receiver_id | BIGINT | 接收人ID | NOT NULL |
| content | TEXT | 内容 | NOT NULL |
| attachments | TEXT[] | 附件URL数组 | |
| is_read | BOOLEAN | 是否已读 | DEFAULT false |
| read_at | TIMESTAMP | 已读时间 | |
| created_at | TIMESTAMP | 创建时间 | DEFAULT CURRENT_TIMESTAMP |

索引：
- `idx_messages_thread`: thread_id
- `idx_messages_sender`: sender_id
- `idx_messages_receiver`: receiver_id
- `idx_messages_unread`: receiver_id WHERE is_read = false

---

## 数据字典

### 质量等级 (quality_level)

| 值 | 说明 |
|----|------|
| Original | 原装正品 |
| OEM | OEM生产 |
| Compatible | 替代品 |

### 认证状态 (certification_status)

| 值 | 说明 |
|----|------|
| Pending | 待审核 |
| Approved | 已通过 |
| Rejected | 已拒绝 |

### 精度等级 (precision_level)

| 值 | 说明 |
|----|------|
| P0 | 普通级 |
| P6 | 高级 |
| P5 | 精密级 |
| P4 | 超精密级 |
| P2 | 超超精密级 |

### 游隙 (clearance)

| 值 | 说明 |
|----|------|
| C2 | 小于普通游隙 |
| CN | 普通游隙 |
| C3 | 大于普通游隙 |
| C4 | 较大游隙 |
| C5 | 特大游隙 |
