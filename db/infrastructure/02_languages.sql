-- ============================================================
-- languages reference table
--
-- Primary key is the BCP-47 code (e.g. 'en', 'es', 'pt').
-- Using the code as PK avoids integer-to-string mapping and
-- keeps queries self-documenting (language_code = 'es' reads
-- clearly without a join).
--
-- is_enabled controls which languages the backend actively
-- serves translations for. All others are stored but silently
-- fall back to English.
-- ============================================================

CREATE TABLE IF NOT EXISTS languages (
    code        VARCHAR(8)   NOT NULL,
    name        VARCHAR(64)  NOT NULL,          -- English display name
    native_name VARCHAR(64)  NOT NULL,          -- Name in that language itself
    is_enabled  BOOLEAN      NOT NULL DEFAULT FALSE,

    -- Audit columns — set by the application layer on writes;
    -- updated_at is kept current automatically by the trigger below.
    created_at  TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    updated_at  TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    created_by  VARCHAR(255) NOT NULL DEFAULT 'system',
    updated_by  VARCHAR(255) NOT NULL DEFAULT 'system',

    CONSTRAINT pk_languages PRIMARY KEY (code)
);

-- Auto-maintain updated_at on every UPDATE
CREATE OR REPLACE TRIGGER trg_languages_updated_at
    BEFORE UPDATE ON languages
    FOR EACH ROW EXECUTE FUNCTION set_updated_at();
