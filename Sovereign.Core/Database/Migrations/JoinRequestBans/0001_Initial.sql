CREATE TABLE JoinRequestDeclineHistory (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    BanId INTEGER NOT NULL,
    Domain TEXT NOT NULL,
    GroupId INTEGER NOT NULL,
    UserId INTEGER NOT NULL,
    Time TEXT NOT NULL
)