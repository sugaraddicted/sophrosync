-- Create one dedicated user per service (principle of least privilege)
CREATE USER svc_clients       WITH PASSWORD 'changeme';
CREATE USER svc_consent       WITH PASSWORD 'changeme';
CREATE USER svc_notes         WITH PASSWORD 'changeme';
CREATE USER svc_notifications WITH PASSWORD 'changeme';
CREATE USER svc_reporting     WITH PASSWORD 'changeme';
CREATE USER svc_schedule      WITH PASSWORD 'changeme';

-- Create one database per service
CREATE DATABASE sophrosync_clients       OWNER svc_clients;
CREATE DATABASE sophrosync_consent       OWNER svc_consent;
CREATE DATABASE sophrosync_notes         OWNER svc_notes;
CREATE DATABASE sophrosync_notifications OWNER svc_notifications;
CREATE DATABASE sophrosync_reporting     OWNER svc_reporting;
CREATE DATABASE sophrosync_schedule      OWNER svc_schedule;

-- Revoke default public access
REVOKE ALL ON DATABASE sophrosync_clients       FROM PUBLIC;
REVOKE ALL ON DATABASE sophrosync_consent       FROM PUBLIC;
REVOKE ALL ON DATABASE sophrosync_notes         FROM PUBLIC;
REVOKE ALL ON DATABASE sophrosync_notifications FROM PUBLIC;
REVOKE ALL ON DATABASE sophrosync_reporting     FROM PUBLIC;
REVOKE ALL ON DATABASE sophrosync_schedule      FROM PUBLIC;
