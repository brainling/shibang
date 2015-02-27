﻿CREATE TABLE IF NOT EXISTS Games(
	Id INTEGER PRIMARY KEY AUTOINCREMENT,
	Name TEXT NOT NULL,
	IsActivelyPlaying TINYINT NOT NULL,
	IsPersistent TINYINT NOT NULL,
	Rating INTEGER,
	GiantBombId INTEGER,
	GiantBombUrl INTEGER,
	SteamId TEXT,
	MediumImageUrl TEXT,
	SmallImageUrl TEXT,
	LastUpdated DATETIME
);
