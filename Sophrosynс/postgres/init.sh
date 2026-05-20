#!/bin/bash
set -e

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    CREATE USER svc_clients       WITH PASSWORD '$CLIENTS_DB_PASSWORD';
    CREATE USER svc_consent       WITH PASSWORD '$CONSENT_DB_PASSWORD';
    CREATE USER svc_identity      WITH PASSWORD '$IDENTITY_DB_PASSWORD';
    CREATE USER svc_notes         WITH PASSWORD '$NOTES_DB_PASSWORD';
    CREATE USER svc_notifications WITH PASSWORD '$NOTIFICATIONS_DB_PASSWORD';
    CREATE USER svc_reporting     WITH PASSWORD '$REPORTING_DB_PASSWORD';
    CREATE USER svc_schedule      WITH PASSWORD '$SCHEDULE_DB_PASSWORD';

    CREATE DATABASE sophrosync_clients       OWNER svc_clients;
    CREATE DATABASE sophrosync_consent       OWNER svc_consent;
    CREATE DATABASE sophrosync_identity      OWNER svc_identity;
    CREATE DATABASE sophrosync_notes         OWNER svc_notes;
    CREATE DATABASE sophrosync_notifications OWNER svc_notifications;
    CREATE DATABASE sophrosync_reporting     OWNER svc_reporting;
    CREATE DATABASE sophrosync_schedule      OWNER svc_schedule;

    REVOKE ALL ON DATABASE sophrosync_clients       FROM PUBLIC;
    REVOKE ALL ON DATABASE sophrosync_consent       FROM PUBLIC;
    REVOKE ALL ON DATABASE sophrosync_identity      FROM PUBLIC;
    REVOKE ALL ON DATABASE sophrosync_notes         FROM PUBLIC;
    REVOKE ALL ON DATABASE sophrosync_notifications FROM PUBLIC;
    REVOKE ALL ON DATABASE sophrosync_reporting     FROM PUBLIC;
    REVOKE ALL ON DATABASE sophrosync_schedule      FROM PUBLIC;
EOSQL
