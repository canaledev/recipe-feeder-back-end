-- ============================================================
-- Seed: sample translations
--
-- These are representative rows that exercise the full fallback
-- chain (en, es, pt, fr, hi). Replace the UUIDs with real recipe
-- IDs once the recipes table is populated.
--
-- Format:
--   entity_type : 'recipe' | 'tag' | 'playlist'
--   entity_id   : UUID of the entity being translated
--   field_name  : 'title' | 'description'
--   language_code / source_language : must exist in languages table
-- ============================================================

-- Sample recipe: Rice Pudding (source authored in English)
WITH sample_recipe AS (
    SELECT '00000000-0000-0000-0000-000000000001'::UUID AS id
)
INSERT INTO translations
    (entity_type, entity_id, field_name, language_code, value, source_language, translated_by, created_by, updated_by)
SELECT
    'recipe', id, field_name, language_code, value, 'en', 'manual', 'system', 'system'
FROM sample_recipe
CROSS JOIN (VALUES
    ('title',       'en', 'Rice Pudding'),
    ('title',       'es', 'Arroz con leche'),
    ('title',       'pt', 'Arroz doce'),
    ('title',       'fr', 'Riz au lait'),
    ('title',       'hi', 'चावल की खीर'),
    ('description', 'en', 'A creamy dessert made with rice simmered in milk and sugar.'),
    ('description', 'es', 'Un postre cremoso de arroz cocido en leche con azúcar.'),
    ('description', 'pt', 'Uma sobremesa cremosa de arroz cozido em leite com açúcar.'),
    ('description', 'fr', 'Un dessert crémeux à base de riz cuit dans du lait sucré.'),
    ('description', 'hi', 'दूध और चीनी में पके चावल से बना मलाईदार मिठाई।')
) AS t(field_name, language_code, value)
ON CONFLICT (entity_type, entity_id, field_name, language_code) DO NOTHING;

-- Sample recipe: Guacamole (source authored in Spanish)
WITH sample_recipe AS (
    SELECT '00000000-0000-0000-0000-000000000002'::UUID AS id
)
INSERT INTO translations
    (entity_type, entity_id, field_name, language_code, value, source_language, translated_by, created_by, updated_by)
SELECT
    'recipe', id, field_name, language_code, value, 'es', 'manual', 'system', 'system'
FROM sample_recipe
CROSS JOIN (VALUES
    ('title',       'es', 'Guacamole'),
    ('title',       'en', 'Guacamole'),
    ('title',       'pt', 'Guacamole'),
    ('title',       'fr', 'Guacamole'),
    ('title',       'hi', 'गुआकामोले'),
    ('description', 'es', 'Dip cremoso de aguacate con limón, cilantro y chile.'),
    ('description', 'en', 'Creamy avocado dip with lime, coriander and chili.'),
    ('description', 'pt', 'Pasta cremosa de abacate com limão, coentro e pimenta.'),
    ('description', 'fr', 'Sauce crémeuse à base d''avocat, citron vert, coriandre et piment.'),
    ('description', 'hi', 'नींबू, धनिया और मिर्च के साथ मलाईदार एवोकाडो डिप।')
) AS t(field_name, language_code, value)
ON CONFLICT (entity_type, entity_id, field_name, language_code) DO NOTHING;

-- Sample tags
INSERT INTO translations
    (entity_type, entity_id, field_name, language_code, value, source_language, translated_by, created_by, updated_by)
VALUES
    ('tag', '00000000-0000-0000-0001-000000000001', 'name', 'en', 'Vegetarian',    'en', 'manual', 'system', 'system'),
    ('tag', '00000000-0000-0000-0001-000000000001', 'name', 'es', 'Vegetariano',   'en', 'manual', 'system', 'system'),
    ('tag', '00000000-0000-0000-0001-000000000001', 'name', 'pt', 'Vegetariano',   'en', 'manual', 'system', 'system'),
    ('tag', '00000000-0000-0000-0001-000000000001', 'name', 'fr', 'Végétarien',    'en', 'manual', 'system', 'system'),
    ('tag', '00000000-0000-0000-0001-000000000001', 'name', 'hi', 'शाकाहारी',      'en', 'manual', 'system', 'system'),
    ('tag', '00000000-0000-0000-0001-000000000002', 'name', 'en', 'Quick',         'en', 'manual', 'system', 'system'),
    ('tag', '00000000-0000-0000-0001-000000000002', 'name', 'es', 'Rápido',        'en', 'manual', 'system', 'system'),
    ('tag', '00000000-0000-0000-0001-000000000002', 'name', 'pt', 'Rápido',        'en', 'manual', 'system', 'system'),
    ('tag', '00000000-0000-0000-0001-000000000002', 'name', 'fr', 'Rapide',        'en', 'manual', 'system', 'system'),
    ('tag', '00000000-0000-0000-0001-000000000002', 'name', 'hi', 'त्वरित',        'en', 'manual', 'system', 'system')
ON CONFLICT (entity_type, entity_id, field_name, language_code) DO NOTHING;
