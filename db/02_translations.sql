-- ============================================================
-- translations table — generic content translation store
--
-- Stores translated field values for any entity type (recipe,
-- tag, playlist, etc.) without requiring a separate table per
-- entity. The unique index enforces one translation per
-- (entity, field, language) combination.
--
-- language_code and source_language both reference languages(code)
-- so only valid, known language codes can be stored.
-- ============================================================

CREATE TABLE IF NOT EXISTS translations (
    id              UUID         NOT NULL DEFAULT gen_random_uuid(),
    entity_type     VARCHAR(64)  NOT NULL,   -- e.g. 'recipe', 'tag'
    entity_id       UUID         NOT NULL,
    field_name      VARCHAR(64)  NOT NULL,   -- e.g. 'title', 'description'
    language_code   VARCHAR(8)   NOT NULL,
    value           TEXT         NOT NULL,
    source_language VARCHAR(8)   NOT NULL DEFAULT 'en',
    translated_by   VARCHAR(16)  NOT NULL DEFAULT 'manual',

    -- Audit columns
    created_at      TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    created_by      VARCHAR(255) NOT NULL DEFAULT 'system',
    updated_by      VARCHAR(255) NOT NULL DEFAULT 'system',

    CONSTRAINT pk_translations            PRIMARY KEY (id),
    CONSTRAINT fk_translations_lang       FOREIGN KEY (language_code)   REFERENCES languages(code),
    CONSTRAINT fk_translations_src_lang   FOREIGN KEY (source_language) REFERENCES languages(code),
    CONSTRAINT chk_translations_xlated_by CHECK (translated_by IN ('manual', 'auto'))
);

-- One translation per entity-field-language (enforces uniqueness for upserts)
CREATE UNIQUE INDEX IF NOT EXISTS uix_translations_entity_field_lang
    ON translations (entity_type, entity_id, field_name, language_code);

-- Fast retrieval of all translated fields for a given entity + language
CREATE INDEX IF NOT EXISTS ix_translations_entity_lang
    ON translations (entity_type, entity_id, language_code);

-- Auto-maintain updated_at on every UPDATE
CREATE OR REPLACE TRIGGER trg_translations_updated_at
    BEFORE UPDATE ON translations
    FOR EACH ROW EXECUTE FUNCTION set_updated_at();
