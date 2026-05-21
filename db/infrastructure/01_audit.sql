-- ============================================================
-- Shared audit trigger function
-- Automatically keeps updated_at current on every UPDATE.
-- Apply to each table via CREATE TRIGGER (see table scripts).
-- ============================================================

CREATE OR REPLACE FUNCTION set_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;
