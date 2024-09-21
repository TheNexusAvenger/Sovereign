CREATE TABLE BanEntries (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    TargetRobloxUserId INTEGER NOT NULL,
    Domain TEXT NOT NULL,
    Action TEXT NOT NULL,
    ExcludeAltAccounts BOOLEAN NOT NULL,
    StartTime TEXT NOT NULL,
    EndTime TEXT,
    ActingRobloxUserId INTEGER NOT NULL,
    DisplayReason TEXT NOT NULL,
    PrivateReason TEXT NOT NULL
);

CREATE TABLE ExternalAccountLinks (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Domain TEXT NOT NULL,
    RobloxUserId INTEGER NOT NULL,
    LinkMethod TEXT NOT NULL,
    LinkData TEXT NOT NULL
);