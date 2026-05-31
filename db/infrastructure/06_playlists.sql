-- Playlist catalog: playlists and their ordered recipe items.

CREATE TABLE IF NOT EXISTS playlists (
    id               UUID         NOT NULL DEFAULT gen_random_uuid(),
    title            TEXT         NOT NULL,
    description      TEXT         NOT NULL DEFAULT '',
    cover_image_url  TEXT         NOT NULL DEFAULT '',
    author           TEXT         NOT NULL DEFAULT '',
    difficulty       TEXT         NOT NULL DEFAULT 'Easy',
    rating           NUMERIC(3,1) NOT NULL DEFAULT 0.0,
    created_at       TIMESTAMPTZ  NOT NULL DEFAULT NOW(),

    CONSTRAINT pk_playlists PRIMARY KEY (id)
);

CREATE TABLE IF NOT EXISTS playlist_items (
    id             UUID NOT NULL DEFAULT gen_random_uuid(),
    playlist_id    UUID NOT NULL,
    recipe_id      UUID NOT NULL,
    ordinal_index  INT  NOT NULL DEFAULT 0,

    CONSTRAINT pk_playlist_items          PRIMARY KEY (id),
    CONSTRAINT uq_playlist_items_order    UNIQUE (playlist_id, ordinal_index),
    CONSTRAINT fk_playlist_items_playlist FOREIGN KEY (playlist_id)
        REFERENCES playlists(id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ix_playlist_items_playlist_id ON playlist_items (playlist_id);
