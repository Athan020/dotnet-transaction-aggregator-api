-- Create Extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_stat_statements";

-- Create Schemas
CREATE SCHEMA IF NOT EXISTS transactions;
CREATE SCHEMA IF NOT EXISTS audit;

-- Transaction Sources Table (Normalized)
CREATE TABLE transactions.sources (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(255) NOT NULL UNIQUE,
    description TEXT,
    source_type VARCHAR(50) NOT NULL, -- 'card', 'prepaid', 'reward'
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create Accounts Table
CREATE TABLE transactions.accounts (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    account_number VARCHAR(50) NOT NULL UNIQUE,
    account_holder_name VARCHAR(255) NOT NULL,
    account_type VARCHAR(50), -- 'savings', 'checking', 'prepaid'
    source_id UUID NOT NULL REFERENCES transactions.sources(id) ON DELETE CASCADE,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_accounts_source_id ON transactions.accounts(source_id);

-- Create Categories Table
CREATE TABLE transactions.categories (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(255) NOT NULL UNIQUE,
    description TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Categorization rules table (drives the dynamic categorization engine)
CREATE TABLE IF NOT EXISTS transactions.categorization_rules (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    rule_name TEXT NOT NULL UNIQUE,
    category TEXT NOT NULL,
    description_contains TEXT[] NOT NULL,
    priority INT NOT NULL,
    enabled BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_categorization_rules_priority 
    ON transactions.categorization_rules(priority);

-- optional sample rules
INSERT INTO transactions.categorization_rules (rule_name, category, description_contains, priority)
VALUES
    ('groceries-rule', 'Groceries', ARRAY['grocery','supermarket','market'], 10),
    ('coffee-rule', 'Coffee', ARRAY['coffee','starbucks'], 20)
ON CONFLICT (rule_name) DO NOTHING;

-- Create Transactions Table (Normalized)
CREATE TABLE transactions.transactions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    transaction_id VARCHAR(255) NOT NULL,
    account_id UUID NOT NULL REFERENCES transactions.accounts(id) ON DELETE CASCADE,
    category_id UUID REFERENCES transactions.categories(id),
    description VARCHAR(500),
    amount DECIMAL(19, 4) NOT NULL,
    currency VARCHAR(3) DEFAULT 'USD',
    transaction_type VARCHAR(50), -- 'debit', 'credit'
    merchant VARCHAR(255),
    location VARCHAR(255),
    reference_number VARCHAR(255),
    transaction_timestamp TIMESTAMP NOT NULL,
    processed_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(account_id, transaction_id, transaction_timestamp)
);

CREATE INDEX idx_transactions_account_id ON transactions.transactions(account_id);
CREATE INDEX idx_transactions_category_id ON transactions.transactions(category_id);
CREATE INDEX idx_transactions_timestamp ON transactions.transactions(transaction_timestamp);
CREATE INDEX idx_transactions_created_at ON transactions.transactions(created_at);

-- Create Transaction Metadata Table (for extensibility)
CREATE TABLE transactions.transaction_metadata (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    transaction_id UUID NOT NULL REFERENCES transactions.transactions(id) ON DELETE CASCADE,
    metadata_key VARCHAR(255),
    metadata_value TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_transaction_metadata_transaction_id ON transactions.transaction_metadata(transaction_id);

-- Create Audit Log Table
CREATE TABLE audit.transaction_changes (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    transaction_id UUID NOT NULL,
    operation VARCHAR(50), -- 'INSERT', 'UPDATE', 'DELETE'
    old_values JSONB,
    new_values JSONB,
    changed_by VARCHAR(255),
    changed_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_audit_transaction_id ON audit.transaction_changes(transaction_id);
CREATE INDEX idx_audit_changed_at ON audit.transaction_changes(changed_at);

-- Create Sync Status Table (for tracking Kafka to DB sync)
CREATE TABLE transactions.sync_status (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    kafka_topic VARCHAR(255),
    kafka_partition INTEGER,
    "offset" BIGINT,
    last_synced_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    status VARCHAR(50), -- 'pending', 'completed', 'failed'
    error_message TEXT,
    UNIQUE(kafka_topic, kafka_partition)
);

CREATE INDEX idx_sync_status_status ON transactions.sync_status(status);

-- Create Aggregation Cache Table
CREATE TABLE transactions.aggregation_cache (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    cache_key VARCHAR(500) NOT NULL UNIQUE,
    cache_value JSONB NOT NULL,
    expires_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_aggregation_cache_expires_at ON transactions.aggregation_cache(expires_at);

-- Insert default sources
INSERT INTO transactions.sources (name, description, source_type) VALUES 
    ('Card Source', 'Credit and Debit Card Transactions', 'card'),
    ('Prepaid Source', 'Prepaid Card Transactions', 'prepaid'),
    ('Reward Source', 'Rewards and Loyalty Transactions', 'reward')
ON CONFLICT DO NOTHING;

-- Insert default categories
INSERT INTO transactions.categories (name, description) VALUES 
    ('Groceries', 'Grocery and food purchases'),
    ('Utilities', 'Utility bills and services'),
    ('Entertainment', 'Entertainment and leisure'),
    ('Transportation', 'Transport and fuel'),
    ('Healthcare', 'Healthcare and medical'),
    ('Shopping', 'General shopping and retail'),
    ('Dining', 'Restaurants and dining'),
    ('Other', 'Other transactions')
ON CONFLICT DO NOTHING;

-- Create Materialized View for Transaction Summary
CREATE MATERIALIZED VIEW transactions.transaction_summary AS
SELECT 
    DATE(t.transaction_timestamp) as transaction_date,
    a.source_id,
    s.name as source_name,
    c.name as category_name,
    t.transaction_type,
    COUNT(*) as transaction_count,
    SUM(t.amount) as total_amount,
    AVG(t.amount) as avg_amount,
    MIN(t.amount) as min_amount,
    MAX(t.amount) as max_amount
FROM transactions.transactions t
JOIN transactions.accounts a ON t.account_id = a.id
JOIN transactions.sources s ON a.source_id = s.id
LEFT JOIN transactions.categories c ON t.category_id = c.id
GROUP BY 
    DATE(t.transaction_timestamp),
    a.source_id,
    s.name,
    c.name,
    t.transaction_type;

-- Create Indexes for Performance
CREATE INDEX idx_transactions_account_timestamp ON transactions.transactions(account_id, transaction_timestamp DESC);
CREATE INDEX idx_transactions_category_date ON transactions.transactions(category_id, transaction_timestamp DESC);
CREATE INDEX idx_cache_key_expires ON transactions.aggregation_cache(cache_key, expires_at);

-- Grant permissions
GRANT USAGE ON SCHEMA transactions TO public;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA transactions TO admin;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA transactions TO admin;

-- Create function for updating updated_at timestamp
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

-- Create triggers for updated_at
CREATE TRIGGER update_sources_updated_at BEFORE UPDATE ON transactions.sources
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_accounts_updated_at BEFORE UPDATE ON transactions.accounts
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_transactions_updated_at BEFORE UPDATE ON transactions.transactions
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
