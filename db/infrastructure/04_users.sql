-- ============================================================
-- users table
--
-- Stores registered user accounts. email is unique and used
-- as the login identifier. password_hash stores the BCrypt
-- hash — never the plaintext password.
--
-- last_login_at is nullable: null means the user has never
-- logged in after registering.
-- ============================================================

CREATE TABLE IF NOT EXISTS users (
    id            UUID         NOT NULL DEFAULT gen_random_uuid(),
    email         TEXT         NOT NULL,
    password_hash TEXT         NOT NULL,
    full_name     TEXT         NOT NULL,
    created_at    TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    last_login_at TIMESTAMPTZ,

    CONSTRAINT pk_users       PRIMARY KEY (id),
    CONSTRAINT uq_users_email UNIQUE (email)
);
