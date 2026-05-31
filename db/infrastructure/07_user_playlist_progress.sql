-- User subscription state and per-item completion tracking.
-- Unsubscribing is a soft delete (unsubscribed_at); completion history is preserved.

CREATE TABLE IF NOT EXISTS user_playlist_subscriptions (
    id               UUID        NOT NULL DEFAULT gen_random_uuid(),
    user_id          UUID        NOT NULL,
    playlist_id      UUID        NOT NULL,
    subscribed_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    unsubscribed_at  TIMESTAMPTZ,

    CONSTRAINT pk_user_playlist_subscriptions      PRIMARY KEY (id),
    CONSTRAINT uq_user_playlist_subscriptions_pair UNIQUE (user_id, playlist_id),
    CONSTRAINT fk_user_playlist_subscriptions_user FOREIGN KEY (user_id)
        REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT fk_user_playlist_subscriptions_playlist FOREIGN KEY (playlist_id)
        REFERENCES playlists(id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ix_user_playlist_subscriptions_user_id ON user_playlist_subscriptions (user_id);

-- completion_state: never_done | current_done | previous_done | historical_done | skipped
CREATE TABLE IF NOT EXISTS user_playlist_items (
    id                UUID        NOT NULL DEFAULT gen_random_uuid(),
    user_id           UUID        NOT NULL,
    playlist_id       UUID        NOT NULL,
    recipe_id         UUID        NOT NULL,
    completion_state  TEXT        NOT NULL DEFAULT 'never_done',
    updated_at        TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT pk_user_playlist_items         PRIMARY KEY (id),
    CONSTRAINT uq_user_playlist_items_triple  UNIQUE (user_id, playlist_id, recipe_id),
    CONSTRAINT fk_user_playlist_items_user    FOREIGN KEY (user_id)
        REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT fk_user_playlist_items_playlist FOREIGN KEY (playlist_id)
        REFERENCES playlists(id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ix_user_playlist_items_user_playlist ON user_playlist_items (user_id, playlist_id);
