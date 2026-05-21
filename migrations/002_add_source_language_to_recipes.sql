-- Migration 002: Add source_language to recipes
-- Tracks the language in which recipe content was originally authored.
-- Used by TranslationService as the second step in the fallback chain.

ALTER TABLE recipes
    ADD COLUMN IF NOT EXISTS source_language VARCHAR(8) NOT NULL DEFAULT 'en';
