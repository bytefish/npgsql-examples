# npgsql-examples #

This is a repository for PostgreSQL and Npgsql samples.

## notify-listen ##

Shows how to use the `NOTIFY` and `LISTEN` features with Npgsql.

### Start the PostgreSQL database ###
 
Start by switching to `notify-listen/docker` and run:

```
docker compose up
```

It starts a local PostgreSQL 16 instance listening on `localhost:5432` using username `postgres` and password `password`.

### Start the .NET Application ###

Start the .NET application, the connection string (given in the `appsettings.json`) already points to the local Postgres instance.

### Using psql to update the data ###

We switch over to a Terminal and start `psql` to update some data:

```
> psql -h localhost -U postgres -W
Password:

psql (15.2, server 16.6 (Debian 16.6-1.pgdg120+1))
WARNING: psql major version 15, server major version 16.
         Some psql features might not work.
WARNING: Console code page (437) differs from Windows code page (1252)
         8-bit characters might not work correctly. See psql reference
         page "Notes for Windows users" for details.
Type "help" for help.

postgres=# SELECT * FROM gitclub.repository;
 repository_id |      name      | organization_id | last_edited_by
---------------+----------------+-----------------+----------------
             1 | Tooling        |               1 |              1
             2 | Specifications |               1 |              1
(2 rows)

postgres=# UPDATE gitclub.repository SET name = 'New Tooling' WHERE repository_id = 1;
UPDATE 1
```

### Enjoying the received Notification ###

And in the Console we will see the incoming notification generated by the PostgreSQL `NOTIFY` command:

```
dbug: Microsoft.Extensions.Hosting.Internal.Host[2]
      Hosting started
dbug: NpgsqlNotifyListen.Infrastructure.LoggingPostgresNotificationHandler[0]
      PostgresNotification (PID = 137, Channel = core_db_event, Payload = {"timestamp" : "2025-01-25T08:38:47.781178+00:00", "row_version" : "752", "operation" : "UPDATE", "schema" : "gitclub", "table" : "repository", "payload" : {"repository_id":1,"name":"New Tooling","organization_id":1,"last_edited_by":1}}
```

## logical-replication-outbox-pattern ##

Shows how to use PostgreSQL Logical Replication feature to implement the Transactional Outbox Pattern.

### Start the PostgreSQL database ###
 
Start by switching to the directory `logical-replication-outbox-pattern/docker` and run:

```
docker compose up
```

This starts a local PostgreSQL 16 instance listening `localhost:5432` using username `postgres` and password `password`.

### Start the .NET Application ###

Open the `GitClub.sln` solution and run the `GitClub` server. 

The connection string already points to the local Postgres instance, if you want to change it use the `appsettings.json`.

Now open the `GitClub.http` file and send the Login Request:

```
### Login as "philipp" and pass the "Administrator" Role
POST {{GitClub_HostAddress}}/Auth/login
Content-Type: application/json

{
    "email": "philipp@bytefish.de",
    "rememberMe": true,
    "roles": [ "Administrator", "User" ]
}
```

Now send a HTTP POST Request for creating a new `Team`:

```
### Create new Team "Rockstar Developers"
POST {{GitClub_HostAddress}}/Teams
Content-Type: application/json

{
    "name": "Rockstar Developers #11",
    "organizationId": 1,
    "lastEditedBy": 1
}
```

This creates a new `OutboxEvent` in the `gitclub.outbox_event` table and notifies all subscribers.

### Enjoying the received OutboxEvents ###

And in the console we will see the replicated outbox event generated by the PostgreSQL Logical Replication feature:

```
[10:25:12 INF] Now listening on: https://localhost:5000
info: GitClub.Hosted.PostgresOutboxEventProcessor[0]
      Processing OutboxEvent (Id = 38192)
[10:25:12 INF] Processing OutboxEvent (Id = 38192)
info: GitClub.Infrastructure.Consumer.OutboxEventConsumer[0]
      Processing Event of Type GitClub.Messages.TeamCreatedMessage and Content {"teamId": 38190, "organizationId": 1}
[10:25:14 INF] Processing Event of Type GitClub.Messages.TeamCreatedMessage and Content {"teamId": 38190, "organizationId": 1}
dbug: GitClub.Hosted.PostgresOutboxEventProcessor[0]
      Received Postgres WAL Message (Type = CommitMessage, ServerClock = 01/26/2025 09:25:12, WalStart = 0/159E368, WalEnd = 0/159E368)
```