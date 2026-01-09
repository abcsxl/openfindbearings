-- ============================================================================
-- OpenFindBearings 轴承交易平台 - 数据库初始化脚本
-- ============================================================================
-- 说明：此脚本创建所有微服务所需的数据库、表结构和索引
-- 数据库：PostgreSQL 15+
-- ============================================================================

-- ============================================================================
-- 第一部分：创建数据库
-- ============================================================================

CREATE DATABASE IF NOT EXISTS auth_db;
CREATE DATABASE IF NOT EXISTS user_db;
CREATE DATABASE IF NOT EXISTS bearing_db;
CREATE DATABASE IF NOT EXISTS inventory_db;
CREATE DATABASE IF NOT EXISTS inquiry_db;
CREATE DATABASE IF NOT EXISTS match_db;
CREATE DATABASE IF NOT EXISTS message_db;

-- ============================================================================
-- 第二部分：认证数据库 (auth_db)
-- ============================================================================

\c auth_db;

-- 角色表
CREATE TABLE IF NOT EXISTS roles (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) UNIQUE NOT NULL,
    description VARCHAR(200),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 用户表
CREATE TABLE IF NOT EXISTS users (
    id BIGSERIAL PRIMARY KEY,
    username VARCHAR(50) UNIQUE NOT NULL,
    phone_number VARCHAR(20) UNIQUE,
    email VARCHAR(100) UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    wechat_openid VARCHAR(100) UNIQUE,
    avatar_url VARCHAR(500),
    full_name VARCHAR(100),
    is_active BOOLEAN DEFAULT true,
    is_phone_verified BOOLEAN DEFAULT false,
    last_login_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT chk_has_contact CHECK (phone_number IS NOT NULL OR email IS NOT NULL OR wechat_openid IS NOT NULL)
);

-- 用户角色关联表
CREATE TABLE IF NOT EXISTS user_roles (
    user_id BIGINT REFERENCES users(id) ON DELETE CASCADE,
    role_id INTEGER REFERENCES roles(id) ON DELETE CASCADE,
    assigned_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (user_id, role_id)
);

-- 刷新令牌表
CREATE TABLE IF NOT EXISTS refresh_tokens (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token VARCHAR(500) UNIQUE NOT NULL,
    expires_at TIMESTAMP NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    revoked_at TIMESTAMP,
    replaced_by_token VARCHAR(500),
    device_info VARCHAR(200)
);

-- 创建索引
CREATE INDEX IF NOT EXISTS idx_users_phone ON users(phone_number);
CREATE INDEX IF NOT EXISTS idx_users_email ON users(email);
CREATE INDEX IF NOT EXISTS idx_users_wechat ON users(wechat_openid);
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_user ON refresh_tokens(user_id);
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_expires ON refresh_tokens(expires_at);

-- 插入默认角色
INSERT INTO roles (name, description) VALUES
    ('Supplier', '供应商'),
    ('Admin', '系统管理员'),
    ('SuperAdmin', '超级管理员')
ON CONFLICT (name) DO NOTHING;

-- ============================================================================
-- 第三部分：用户数据库 (user_db)
-- ============================================================================

\c user_db;

-- 企业信息表
CREATE TABLE IF NOT EXISTS companies (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL,
    company_name VARCHAR(200) NOT NULL,
    unified_social_credit_code VARCHAR(50) UNIQUE,
    business_license_url VARCHAR(500),
    contact_person VARCHAR(50),
    contact_phone VARCHAR(20),
    contact_email VARCHAR(100),
    province VARCHAR(50),
    city VARCHAR(50),
    address VARCHAR(500),
    business_scope TEXT,
    certification_status VARCHAR(20) DEFAULT 'Pending',
    certification_at TIMESTAMP,
    certified_by BIGINT,
    reject_reason TEXT,
    credit_score INTEGER DEFAULT 100,
    total_orders INTEGER DEFAULT 0,
    completed_orders INTEGER DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 企业认证文件表
CREATE TABLE IF NOT EXISTS company_documents (
    id BIGSERIAL PRIMARY KEY,
    company_id BIGINT NOT NULL REFERENCES companies(id) ON DELETE CASCADE,
    document_type VARCHAR(50) NOT NULL,
    document_url VARCHAR(500) NOT NULL,
    file_name VARCHAR(200),
    file_size BIGINT,
    upload_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 信用记录表
CREATE TABLE IF NOT EXISTS credit_records (
    id BIGSERIAL PRIMARY KEY,
    company_id BIGINT NOT NULL REFERENCES companies(id) ON DELETE CASCADE,
    score_change INTEGER NOT NULL,
    reason VARCHAR(200),
    order_id BIGINT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 创建索引
CREATE INDEX IF NOT EXISTS idx_companies_user ON companies(user_id);
CREATE INDEX IF NOT EXISTS idx_companies_cert_status ON companies(certification_status);
CREATE INDEX IF NOT EXISTS idx_companies_location ON companies(province, city);
CREATE INDEX IF NOT EXISTS idx_credit_records_company ON credit_records(company_id);

-- ============================================================================
-- 第四部分：轴承数据库 (bearing_db)
-- ============================================================================

\c bearing_db;

-- 品牌表
CREATE TABLE IF NOT EXISTS brands (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) UNIQUE NOT NULL,
    name_en VARCHAR(100),
    country VARCHAR(50),
    logo_url VARCHAR(500),
    website VARCHAR(200),
    description TEXT,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 分类表
CREATE TABLE IF NOT EXISTS categories (
    id SERIAL PRIMARY KEY,
    parent_id INTEGER REFERENCES categories(id),
    name VARCHAR(100) NOT NULL,
    name_en VARCHAR(100),
    level INTEGER DEFAULT 1,
    path VARCHAR(500),
    sort_order INTEGER DEFAULT 0,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 轴承型号主表
CREATE TABLE IF NOT EXISTS bearing_models (
    id BIGSERIAL PRIMARY KEY,
    model_number VARCHAR(100) NOT NULL,
    brand_id INTEGER REFERENCES brands(id),
    category_id INTEGER REFERENCES categories(id),
    inner_diameter DECIMAL(10,2),
    outer_diameter DECIMAL(10,2),
    width DECIMAL(10,2),
    weight DECIMAL(10,3),
    material VARCHAR(100),
    cage_material VARCHAR(100),
    seal_type VARCHAR(50),
    precision_level VARCHAR(20),
    clearance VARCHAR(20),
    description TEXT,
    keywords TEXT,
    specifications JSONB,
    is_standard BOOLEAN DEFAULT true,
    equivalent_models TEXT[],
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uk_brand_model UNIQUE (brand_id, model_number)
);

-- 轴承别名表
CREATE TABLE IF NOT EXISTS bearing_aliases (
    id BIGSERIAL PRIMARY KEY,
    bearing_model_id BIGINT NOT NULL REFERENCES bearing_models(id) ON DELETE CASCADE,
    alias VARCHAR(100) NOT NULL,
    source VARCHAR(50),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 创建索引
CREATE INDEX IF NOT EXISTS idx_bearing_models_number ON bearing_models(model_number);
CREATE INDEX IF NOT EXISTS idx_bearing_models_brand ON bearing_models(brand_id);
CREATE INDEX IF NOT EXISTS idx_bearing_models_category ON bearing_models(category_id);
CREATE INDEX IF NOT EXISTS idx_bearing_models_specs ON bearing_models USING GIN(specifications);
CREATE INDEX IF NOT EXISTS idx_bearing_models_keywords ON bearing_models USING GIN(to_tsarray('simple', keywords));
CREATE INDEX IF NOT EXISTS idx_bearing_aliases_model ON bearing_aliases(bearing_model_id);
CREATE INDEX IF NOT EXISTS idx_bearing_aliases_alias ON bearing_aliases(alias);
CREATE INDEX IF NOT EXISTS idx_categories_parent ON categories(parent_id);
CREATE INDEX IF NOT EXISTS idx_categories_path ON categories(path);

-- ============================================================================
-- 第五部分：库存数据库 (inventory_db)
-- ============================================================================

\c inventory_db;

-- 库存表
CREATE TABLE IF NOT EXISTS inventory_items (
    id BIGSERIAL PRIMARY KEY,
    company_id BIGINT NOT NULL,
    bearing_model_id BIGINT NOT NULL,
    quantity INTEGER NOT NULL DEFAULT 0,
    reserved_quantity INTEGER DEFAULT 0,
    available_quantity INTEGER GENERATED ALWAYS AS (quantity - reserved_quantity) STORED,
    price DECIMAL(12,2),
    min_quantity INTEGER DEFAULT 1,
    price_negotiable BOOLEAN DEFAULT false,
    quality_level VARCHAR(20) DEFAULT 'Original',
    origin VARCHAR(100),
    production_date DATE,
    batch_number VARCHAR(100),
    warehouse_location VARCHAR(200),
    description TEXT,
    images TEXT[],
    is_active BOOLEAN DEFAULT true,
    is_verified BOOLEAN DEFAULT false,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_price_update_at TIMESTAMP
);

-- 库存变动记录表
CREATE TABLE IF NOT EXISTS inventory_records (
    id BIGSERIAL PRIMARY KEY,
    inventory_id BIGINT NOT NULL REFERENCES inventory_items(id) ON DELETE CASCADE,
    change_type VARCHAR(20) NOT NULL,
    quantity_before INTEGER NOT NULL,
    quantity_change INTEGER NOT NULL,
    quantity_after INTEGER NOT NULL,
    reference_type VARCHAR(50),
    reference_id BIGINT,
    remark TEXT,
    created_by BIGINT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 价格历史表
CREATE TABLE IF NOT EXISTS price_history (
    id BIGSERIAL PRIMARY KEY,
    inventory_id BIGINT NOT NULL REFERENCES inventory_items(id) ON DELETE CASCADE,
    old_price DECIMAL(12,2),
    new_price DECIMAL(12,2) NOT NULL,
    change_reason VARCHAR(200),
    changed_by BIGINT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 创建索引
CREATE INDEX IF NOT EXISTS idx_inventory_company ON inventory_items(company_id);
CREATE INDEX IF NOT EXISTS idx_inventory_bearing ON inventory_items(bearing_model_id);
CREATE INDEX IF NOT EXISTS idx_inventory_quantity ON inventory_items(quantity) WHERE quantity > 0;
CREATE INDEX IF NOT EXISTS idx_inventory_updated ON inventory_items(updated_at);
CREATE INDEX IF NOT EXISTS idx_inventory_records_inventory ON inventory_records(inventory_id);
CREATE INDEX IF NOT EXISTS idx_inventory_records_created ON inventory_records(created_at);
CREATE INDEX IF NOT EXISTS idx_price_history_inventory ON price_history(inventory_id);

-- ============================================================================
-- 第六部分：询价交易数据库 (inquiry_db)
-- ============================================================================

\c inquiry_db;

-- 寻货询价表
CREATE TABLE IF NOT EXISTS inquiries (
    id BIGSERIAL PRIMARY KEY,
    buyer_company_id BIGINT NOT NULL,
    bearing_model_id BIGINT,
    custom_model VARCHAR(100),
    brand_id INTEGER,
    quantity INTEGER NOT NULL,
    target_price DECIMAL(12,2),
    quality_level VARCHAR(20),
    urgent BOOLEAN DEFAULT false,
    required_date DATE,
    description TEXT,
    status VARCHAR(20) DEFAULT 'Open',
    valid_until TIMESTAMP,
    view_count INTEGER DEFAULT 0,
    quotation_count INTEGER DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    closed_at TIMESTAMP
);

-- 报价表
CREATE TABLE IF NOT EXISTS quotations (
    id BIGSERIAL PRIMARY KEY,
    inquiry_id BIGINT NOT NULL REFERENCES inquiries(id) ON DELETE CASCADE,
    supplier_company_id BIGINT NOT NULL,
    inventory_id BIGINT,
    unit_price DECIMAL(12,2) NOT NULL,
    total_price DECIMAL(12,2) GENERATED ALWAYS AS (unit_price * quantity) STORED,
    quantity INTEGER NOT NULL,
    available_quantity INTEGER NOT NULL,
    validity_days INTEGER DEFAULT 7,
    delivery_period INTEGER,
    payment_terms VARCHAR(200),
    delivery_terms VARCHAR(200),
    description TEXT,
    attachments TEXT[],
    status VARCHAR(20) DEFAULT 'Pending',
    reject_reason TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    expires_at TIMESTAMP,
    CONSTRAINT uk_quotation UNIQUE (inquiry_id, supplier_company_id)
);

-- 订单表
CREATE TABLE IF NOT EXISTS orders (
    id BIGSERIAL PRIMARY KEY,
    order_number VARCHAR(50) UNIQUE NOT NULL,
    quotation_id BIGINT REFERENCES quotations(id),
    buyer_company_id BIGINT NOT NULL,
    supplier_company_id BIGINT NOT NULL,
    bearing_model_id BIGINT,
    model_number VARCHAR(100) NOT NULL,
    brand_id INTEGER,
    quantity INTEGER NOT NULL,
    unit_price DECIMAL(12,2) NOT NULL,
    total_amount DECIMAL(12,2) NOT NULL,
    delivery_period INTEGER,
    delivery_address TEXT,
    payment_terms VARCHAR(200),
    invoice_type VARCHAR(50),
    status VARCHAR(20) DEFAULT 'Pending',
    payment_status VARCHAR(20) DEFAULT 'Unpaid',
    ship_status VARCHAR(20) DEFAULT 'NotShipped',
    confirmed_at TIMESTAMP,
    shipped_at TIMESTAMP,
    delivered_at TIMESTAMP,
    completed_at TIMESTAMP,
    cancelled_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 订单状态历史表
CREATE TABLE IF NOT EXISTS order_status_history (
    id BIGSERIAL PRIMARY KEY,
    order_id BIGINT NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    old_status VARCHAR(20),
    new_status VARCHAR(20) NOT NULL,
    remark TEXT,
    created_by BIGINT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 创建索引
CREATE INDEX IF NOT EXISTS idx_inquiries_buyer ON inquiries(buyer_company_id);
CREATE INDEX IF NOT EXISTS idx_inquiries_bearing ON inquiries(bearing_model_id);
CREATE INDEX IF NOT EXISTS idx_inquiries_status ON inquiries(status);
CREATE INDEX IF NOT EXISTS idx_inquiries_created ON inquiries(created_at DESC);
CREATE INDEX IF NOT EXISTS idx_inquiries_urgent ON inquiries(urgent) WHERE urgent = true;
CREATE INDEX IF NOT EXISTS idx_quotations_inquiry ON quotations(inquiry_id);
CREATE INDEX IF NOT EXISTS idx_quotations_supplier ON quotations(supplier_company_id);
CREATE INDEX IF NOT EXISTS idx_quotations_status ON quotations(status);
CREATE INDEX IF NOT EXISTS idx_quotations_created ON quotations(created_at DESC);
CREATE INDEX IF NOT EXISTS idx_orders_number ON orders(order_number);
CREATE INDEX IF NOT EXISTS idx_orders_buyer ON orders(buyer_company_id);
CREATE INDEX IF NOT EXISTS idx_orders_supplier ON orders(supplier_company_id);
CREATE INDEX IF NOT EXISTS idx_orders_status ON orders(status);
CREATE INDEX IF NOT EXISTS idx_orders_created ON orders(created_at DESC);
CREATE INDEX IF NOT EXISTS idx_order_status_history_order ON order_status_history(order_id);

-- ============================================================================
-- 第七部分：匹配数据库 (match_db)
-- ============================================================================

\c match_db;

-- 匹配记录表
CREATE TABLE IF NOT EXISTS match_records (
    id BIGSERIAL PRIMARY KEY,
    inquiry_id BIGINT NOT NULL,
    company_id BIGINT NOT NULL,
    inventory_id BIGINT,
    match_score DECIMAL(5,2) NOT NULL,
    match_reasons JSONB,
    has_stock BOOLEAN NOT NULL,
    available_quantity INTEGER,
    price DECIMAL(12,2),
    status VARCHAR(20) DEFAULT 'Pending',
    notified_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 推荐记录表
CREATE TABLE IF NOT EXISTS recommendations (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL,
    recommendation_type VARCHAR(50) NOT NULL,
    item_id BIGINT NOT NULL,
    score DECIMAL(5,2) NOT NULL,
    reason TEXT,
    is_clicked BOOLEAN DEFAULT false,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 创建索引
CREATE INDEX IF NOT EXISTS idx_match_records_inquiry ON match_records(inquiry_id);
CREATE INDEX IF NOT EXISTS idx_match_records_company ON match_records(company_id);
CREATE INDEX IF NOT EXISTS idx_match_records_score ON match_records(match_score DESC);
CREATE INDEX IF NOT EXISTS idx_recommendations_user ON recommendations(user_id);
CREATE INDEX IF NOT EXISTS idx_recommendations_type ON recommendations(recommendation_type);

-- ============================================================================
-- 第八部分：消息数据库 (message_db)
-- ============================================================================

\c message_db;

-- 消息会话表
CREATE TABLE IF NOT EXISTS message_threads (
    id BIGSERIAL PRIMARY KEY,
    subject VARCHAR(200),
    related_type VARCHAR(50),
    related_id BIGINT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 消息表
CREATE TABLE IF NOT EXISTS messages (
    id BIGSERIAL PRIMARY KEY,
    thread_id BIGINT NOT NULL REFERENCES message_threads(id) ON DELETE CASCADE,
    sender_id BIGINT NOT NULL,
    receiver_id BIGINT NOT NULL,
    content TEXT NOT NULL,
    attachments TEXT[],
    is_read BOOLEAN DEFAULT false,
    read_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 创建索引
CREATE INDEX IF NOT EXISTS idx_messages_thread ON messages(thread_id);
CREATE INDEX IF NOT EXISTS idx_messages_sender ON messages(sender_id);
CREATE INDEX IF NOT EXISTS idx_messages_receiver ON messages(receiver_id);
CREATE INDEX IF NOT EXISTS idx_messages_unread ON messages(receiver_id) WHERE is_read = false;

-- ============================================================================
-- 完成
-- ============================================================================

-- 输出初始化完成信息
SELECT 'Database initialization completed successfully!' AS status;
