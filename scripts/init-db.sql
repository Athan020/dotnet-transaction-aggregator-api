-- Create Extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_stat_statements";

-- Create Schemas
CREATE SCHEMA IF NOT EXISTS transactions;



-- SourceId
-- ExternalId
-- Amount
-- Currency
-- Timestamp
-- Categorization_Status


-- Create Tables

CREATE TABLE IF NOT EXISTS transactions.transactions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    source_id VARCHAR(50) NOT NULL,
    account_id BIGINT NOT NULL,
    external_id VARCHAR(50) NOT NULL,
    amount DECIMAL(18, 2) NOT NULL,
    currency VARCHAR(3) NOT NULL,
    transaction_date TIMESTAMP NOT NULL,
    description TEXT,
    category_id UUID REFERENCES transactions.categories(id),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);


CREATE TABLE IF NOT EXISTS transactions.categorization (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    transaction_id UUID NOT NULL,
    status TEXT NOT NULL,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (transaction_id) REFERENCES transactions.transactions(id) ON DELETE CASCADE
);


-- Create Categories Table
CREATE TABLE transactions.categories (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(255) NOT NULL UNIQUE,
    description TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_transactions_account_id_date ON transactions.transactions(account_id, transaction_date);
CREATE INDEX IF NOT EXISTS idx_transactions_category_id ON transactions.transactions(category_id);

CREATE INDEX IF NOT EXISTS idx_categorization_transaction_id ON transactions.categorization(transaction_id);







