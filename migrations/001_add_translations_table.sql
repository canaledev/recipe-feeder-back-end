-- Migration 001: Generic translations table
-- Stores translated field values for any entity type.
-- The unique index ensures one translation per (entity, field, language).

CREATE TABLE IF NOT EXISTS translations (
    id              UUID         NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    entity_type     VARCHAR(64)  NOT NULL,  -- e.g. 'recipe', 'tag', 'playlist'
    entity_id       UUID         NOT NULL,
    field_name      VARCHAR(64)  NOT NULL,  -- e.g. 'title', 'description'
    language_code   VARCHAR(8)   NOT NULL,  -- BCP 47: 'en', 'es', 'pt', 'fr', 'hi'
    value           TEXT         NOT NULL,
    source_language VARCHAR(8)   NOT NULL DEFAULT 'en',
    translated_by   VARCHAR(16)  NOT NULL DEFAULT 'manual' CHECK (translated_by IN ('manual', 'auto')),
    translated_at   TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

CREATE UNIQUE INDEX IF NOT EXISTS uix_translations_entity_field_lang
    ON translations (entity_type, entity_id, field_name, language_code);

CREATE INDEX IF NOT EXISTS ix_translations_entity_lang
    ON translations (entity_type, entity_id, language_code);
