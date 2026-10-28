CREATE TABLE identities(provider TEXT NOT NULL,subject TEXT NOT NULL,user_id TEXT NOT NULL REFERENCES users(id),PRIMARY KEY(provider,subject));
CREATE TABLE oauth_flows(flow_hash TEXT PRIMARY KEY,device_code TEXT NOT NULL,expires_at INTEGER NOT NULL,interval_seconds INTEGER NOT NULL,next_poll INTEGER NOT NULL);
