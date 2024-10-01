# Sovereign
Sovereign is a ban system built on Roblox's ban APIs to handle bans
across multiple games and groups with a single ban list.

## Design
### Modules
Sovereign runs multiple applications with specific purposes, mainly
to keep the logs separate for each role. The modules that run include:
- `Sovereign.Api.Bans`: API for handling bans and unbans.
- `Sovereign.Bans.Games`: Applies bans/unbans to games.
- `Sovereign.Bans.JoinRequests`: Automatically declines join requests
  from banned users.
- `Sovereign.Discord`: Discord bot to assign bans and unbans.

Once Open Cloud APIs exist to kick or ban from groups, a new module
will be created for that purpose.

### Domains
Sovereign is able to support multiple sets of games and groups (called
"domains" in Sovereign), and each domain can contain multiple games
(handled by `Sovereign.Bans.Games`) and groups. Each domain is separate
to allow multiple tenants to share an instance of Sovereign.

### Discord Bot
A Discord bot is provided to manage bans/unbans and view them.
1 bot can control multiple domains and multiple servers can control
1 domain, but each server can only control 1 domain. The configuration
is done on the server and can't be re-configured in the Discord server.

4 commands are provided:
- `/startlink`: Allows for linking a Discord account to a Roblox account
  in order to authorize and store bans.
  - Currently, only putting a random message in the profile description
    is supported. OAuth2 could be supported, but would require Sovereign
    to be exposed to the internet with a static IP.
  - If the Roblox user id provided is not authorized to ban, the link
    request will be prevented.
- `/startban`: Starts the process for banning users.
  - If multiple options for ban templates are configured for the domain,
    a dropdown menu will be present with the options. Otherwise, the
    only configured template will be presented as a modal.
  - Having a linked account is required to run the command.
- `/startunban`: Starts the process for unbanning users.
  - Having a linked account is required to run the command.
- `/viewban`: Displays the latest ban/unban (if any) with the given
  user and adds page buttons to see older bans.
  - Having a linked account is **not** required to run the command.
    Make sure to lock down access to Sovereign commands if used in a
    public server.

All messages displayed with the Discord bot are displayed only to the
user running the commands.

## API
Examples of the ban API can be found in [`TestRequests.http`](./Sovereign.Api.Bans/TestRequests.http)
under `Sovereign.Api.Bans`.

All examples use an `ApiKey`-based `Authorization` headers, but
`Signature`-based `Authorization` headers are recommended. They function
like `ApiKey` headers, except the `Authorization` headers are only
valid for those contents of the request. Except for `GET` requests,
the header should be in the format of `Signature base64Value`, where
`base64Value` is the HMACSHA256 of the request JSON body using
the secret key stored in the bans API's configuration. For `GET`
requests, use the request query (including the leading question mark)
instead.

## Setup
### Roblox Open Cloud
Sovereign supports banning from games and auto-declining join requests
from groups. Both are configured in the [Roblox Open Cloud dashboard](https://create.roblox.com/dashboard/credentials).

#### Game Bans
Under "Access Permissions" for a new or existing API key,
`user-restrictions` needs to be added with the desired games to handle
bans. Both `read` and `write` permissions need to be granted.

#### Group Join Request Bans
Under **a user** (NOT group), the `group` permission needs to be added
with both `read` and `write`. At this time, there is no way to restrict
the API key to specific groups, so an alt account for the specific groups
is recommended.

### Configurations
The intended setup for Sovereign is to be run with Docker Compose.
When used, all configurations will be stored in the `configurations`
directory.

When an application is run for the first time, a default configuration
is created. On Linux with Docker, the file permissions might be `root:root`,
so running `sudo chown` will be required.

#### `api.bans.json` (`Sovereign.Api.Bans`)
Entries:
- `Domains` *(Required)*: List of domains that are accepted for ban
  and unban requests.
  - `Name` *(Required)*: Unique name of the domain that will be used
    in requests.
  - `ApiKeys` *(Optional)*: List of accepted API keys for authorization
    headers. If not provided, `SecretKeys` is required.
  - `SecretKeys` *(Optional)*: List of accepted secret keys used for
    authorization haeders using HMACSHA256. This is recommended over
    `ApiKeys`, but not usable within Roblox servers. However, if
    `Sovereign.Discord` is used, at least 1 secret key is required.
  - `Rules` *(Required)*: List of rules to determine who can ban and
    unban. [See Bouncer's README about rule definitions.](https://github.com/TheNexusAvenger/Bouncer/blob/master/README.md)
    - `Name` *(Optional)*: Display name of the rule shown in logs.
    - `Rule` *(Required)*: Rule definition.
    - `Action` *(Required)*: Either `Allow` to allow the user to ban/unban,
      or `Deny` to prevent them. `Deny` rules should be before `Allow`
      rules if a rule is meant to prevent a subset of an `Allow` rule.
  - `GroupIdRankChecks` *(Optional)*: List of group ids to compare
    banning users with who they are banning. This is recommended for
    groups to avoid lower ranks from banning higher ranks.
- `Logging` *(Required)*: Logging configuration.
  - `MinimumLogLevel` *(Required)*: Minimum log level to display.
    Can be `Trace`, `Debug`, `Information`, `Warning`, `Error`,
    or `None`.

All parts of the configuration support updating without restarting the
application.

Example configuration:
```json
{
  "Domains": [
    {
      "Name": "MyGroup",
      "SecretKeys": [
        "MySecretKey"
      ],
      "Rules": [
        {
          "Name": "Deny Special Rank",
          "Rule": "GroupRankIs(12345, \"EqualTo\", 230)",
          "Action": "Deny"
        },
        {
          "Name": "Allow Group Moderators",
          "Rule": "GroupRankIs(12345, \"AtLeast\", 200)",
          "Action": "Allow"
        }
      ],
      "GroupIdRankChecks": [
        12345
      ]
    }
  ],
  "Logging": {
    "MinimumLogLevel": "Information"
  }
}
```

The example configuration allows for controlling the domain `MyGroup`
with the secret key `MySecretKey` used for `Signature`-based
`Authorization` headers. Rank 230 in group 12345 is not allowed
to ban/unban, while anyone else at least rank 200 can. The group
ranks in group 12345 are checked before banning to prevent lower
ranks in the group from banning higher ranks.

#### `bans.games.json` (`Sovereign.Bans.Games`)
Entries:
- `Games` *(Required)*: List of games to apply bans/unbans to.
  - `Domain` *(Required)*: Domain of bans to apply to the game.
  - `GameId` *(Required)*: Game id (NOT place id) to apply bans/unbans.
  - `ApiKey` *(Required)*: Roblox Open Cloud API key for updating
    and getting user restrictions.
  - `DryRun` *(Optional)*: If `true`, the logic for bans will be
    run, but no bans or unbans will be sent to Roblox.
- `Logging` *(Required)*: Logging configuration.
  - `MinimumLogLevel` *(Required)*: Minimum log level to display.
    Can be `Trace`, `Debug`, `Information`, `Warning`, `Error`,
    or `None`.

All parts of the configuration support updating without restarting the
application.

Example configuration:
```json
{
  "Games": [
    {
      "Domain": "MyGroup",
      "GameId": 123456789,
      "ApiKey": "OpenCloudApiKey",
      "DryRun": false
    },
    {
      "Domain": "MyGroup",
      "GameId": 234567890,
      "ApiKey": "OpenCloudApiKey",
      "DryRun": false
    }
  ],
  "Logging": {
    "MinimumLogLevel": "Information"
  }
}
```

The example configuration applies the bans/unbans in the `MyGroup`
domain to games `123456789` and `234567890`.

#### `bans.joinrequests.json` (`Sovereign.Bans.JoinRequests`)
Entries:
- `Groups` *(Required)*: List of groups to deny join requests for.
  - `Domain` *(Required)*: Domain of bans to deny join requests for.
  - `GroupId` *(Required)*: Group id to deny join requests for.
  - `ApiKey` *(Required)*: Roblox Open Cloud API key for fetching
    and denying group join requests.
  - `LoopDelaySeconds` *(Optional)*: Delay (in seconds) between
    checking for join requests. If not specified, 30 seconds is
    used by default.
  - `DryRun` *(Optional)*: If `true`, the logic for fetching and
    denying join requests will be run, but no join request actions
    will be sent to Roblox.
- `Logging` *(Required)*: Logging configuration.
  - `MinimumLogLevel` *(Required)*: Minimum log level to display.
    Can be `Trace`, `Debug`, `Information`, `Warning`, `Error`,
    or `None`.

All parts of the configuration support updating without restarting the
application.

Example configuration:
```json
{
  "Groups": [
    {
      "Domain": "MyGroup",
      "GroupId": 12345,
      "ApiKey": "OpenCloudApiKey",
      "LoopDelaySeconds": 15,
      "DryRun": false
    },
    {
      "Domain": "MyGroup",
      "GroupId": 23456,
      "ApiKey": "OpenCloudApiKey",
      "LoopDelaySeconds": 15,
      "DryRun": false
    }
  ],
  "Logging": {
    "MinimumLogLevel": "Information"
  }
}
```

The example configuration denies join requests for groups 12345
and 23456 for those banned in domain the `MyGroup`. The join requests
are checked every 15 seconds.

#### `discord.json` (`Sovereign.Discord`)
Entries:
- `Discord` *(Required)*: Configuration for the Discord bot.
  - `Token` *(Required)*: Discord token for the bot.
  - `Servers` *(Required)*: List of servers controlled whitelisted
    for the bot with their domain.
    - `Id` *(Required)*: Id of the Discord server.
    - `Domain` *(Required)*: Domain of the Discord server to control.
      Multiple servers can control 1 domain, but each server can
      only control 1 domain.
- `Domains` *(Required)*: List of domains controlled by the servers.
  - `Name` *(Required)*: Name of the domain.
  - `ApiSecretKey` *(Required)*: Secret key used to create
    `Signature`-based `Authorization` headers. It must match at least
    1 secret key in the `SecretKeys` for the domain in the bans API.
  - `BanOptions` *(Required)*: List of ban option templates for creating
    bans.
    - `Name` *(Required)*: Name of the template to display in the dropdown
      menu when running `/startban`.
    - `Description` *(Optional)*: Description to display in the dropdown
      menu when running `/startban`.
    - `DefaultDisplayReason` *(Optional)*: Default display reason to fill
      when presenting the ban modal.
    - `DefaultPrivateReason` *(Optional)*: Default private reason to fill
      when presenting the ban modal.
- `Logging` *(Required)*: Logging configuration.
  - `MinimumLogLevel` *(Required)*: Minimum log level to display.
    Can be `Trace`, `Debug`, `Information`, `Warning`, `Error`,
    or `None`.

All parts of the configuration except for `Discord.Token` support updating
without restarting the application. Changes to the Discord token will require
a restart of the application.

Example configuration:
```json
{
  "Discord": {
    "Token": "DiscordToken",
    "Servers": [
      {
        "Id": 123456789,
        "Domain": "MyGroup"
      },
      {
        "Id": 234567890,
        "Domain": "MyGroup"
      }
    ]
  },
  "Domains": [
    {
      "Name": "MyGroup",
      "ApiSecretKey": "MySecretKey",
      "BanOptions": [
        {
          "Name": "Exploiting",
          "Description": "Please specify details in the private reason.",
          "DefaultDisplayReason": "Banned for exploiting. See the game's description.",
          "DefaultPrivateReason": "No exploit information stored."
        },
        {
          "Name": "Other",
          "DefaultDisplayReason": "You are banned. See the Training Facility's social links to appeal."
        }
      ]
    }
  ],
  "Logging": {
    "MinimumLogLevel": "Information"
  }
}
```

The example configuration sets up servers 123456789 and 234567890 to
control the ban domain `MyGroup`. `/startban` will present a dropdown with
`Exploiting` and `Other`.

#### `docker-compose.overrides.yml`
When using Docker Compose, a `docker-compose.overrides.yml` is strongly
recommended to change the internal secret used for internal webhooks (used
for faster ban responses). All of them must match to work.

```yml
services:
  sovereign-api-bans:
    environment:
      - INTERNAL_WEBHOOK_SECRET_KEY=MyCustomSecret

  sovereign-bans-games:
    environment:
      - INTERNAL_WEBHOOK_SECRET_KEY=MyCustomSecret

  sovereign-bans-join-requests:
    environment:
      - INTERNAL_WEBHOOK_SECRET_KEY=MyCustomSecret
```

`docker-compose.overrides.yml` can also be set to disable services from
starting if you don't need the full functionality. The example below
disables the Discord bot, disables the join request banning, and opens
port 8000 of the bans API for use with other applications.

```yml
services:
  sovereign-api-bans:
    environment:
      - INTERNAL_WEBHOOK_SECRET_KEY=MyCustomSecret
    ports:
      - 8000:8000

  sovereign-bans-games:
    environment:
      - INTERNAL_WEBHOOK_SECRET_KEY=MyCustomSecret

  sovereign-bans-join-requests:
    profiles:
      - donotstart

  sovereign-bans-discord:
    profiles:
      - donotstart
```

## Running
Sovereign can be compiled and run using .NET 8. Native AOT is planned,
but not in use due to incompatibilities with Entity Framework. It is highly
recommended to use Docker if possible for running. Using the
`docker-compose.yml` on a system set up with Docker, the server can
be built (or updated) and started with the following when run in the
root directory of the project.

```bash
docker compose up -d --build
```

Stopping the server is done with the following:
```bash
docker compose down
```

## License
Sovereign is available under the terms of the GNU Lesser General Public
License. See [LICENSE](LICENSE) for details.