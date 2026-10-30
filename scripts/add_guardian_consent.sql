CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    migration_id character varying(150) NOT NULL,
    product_version character varying(32) NOT NULL,
    CONSTRAINT pk___ef_migrations_history PRIMARY KEY (migration_id)
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE TABLE user_account (
        id uuid NOT NULL,
        email character varying(254) NOT NULL,
        phone character varying(30),
        password_hash character varying(200) NOT NULL,
        status character varying(20) NOT NULL,
        primary_role character varying(20) NOT NULL,
        email_confirmed_at_utc timestamp with time zone,
        created_at_utc timestamp with time zone NOT NULL,
        updated_at_utc timestamp with time zone NOT NULL,
        CONSTRAINT pk_user_account PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE TABLE transporter (
        id uuid NOT NULL,
        user_account_id uuid NOT NULL,
        name character varying(150) NOT NULL,
        document_type character varying(10) NOT NULL,
        document_number character varying(20) NOT NULL,
        license_number character varying(20),
        license_category character varying(5),
        license_expiry date,
        business_phone character varying(30),
        created_at_utc timestamp with time zone NOT NULL,
        updated_at_utc timestamp with time zone NOT NULL,
        deleted_at_utc timestamp with time zone,
        CONSTRAINT pk_transporter PRIMARY KEY (id),
        CONSTRAINT fk_transporter_user_accounts_user_account_id FOREIGN KEY (user_account_id) REFERENCES user_account (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE TABLE assistant (
        id uuid NOT NULL,
        transporter_id uuid NOT NULL,
        user_account_id uuid NOT NULL,
        name character varying(150) NOT NULL,
        created_at_utc timestamp with time zone NOT NULL,
        updated_at_utc timestamp with time zone NOT NULL,
        deleted_at_utc timestamp with time zone,
        CONSTRAINT pk_assistant PRIMARY KEY (id),
        CONSTRAINT fk_assistant_transporters_transporter_id FOREIGN KEY (transporter_id) REFERENCES transporter (id) ON DELETE RESTRICT,
        CONSTRAINT fk_assistant_user_accounts_user_account_id FOREIGN KEY (user_account_id) REFERENCES user_account (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE TABLE event_log (
        id uuid NOT NULL,
        transporter_id uuid NOT NULL,
        occurred_at_utc timestamp with time zone NOT NULL,
        event_type character varying(100) NOT NULL,
        aggregate_type character varying(60) NOT NULL,
        aggregate_id uuid NOT NULL,
        actor_user_id uuid,
        payload jsonb NOT NULL,
        CONSTRAINT pk_event_log PRIMARY KEY (id),
        CONSTRAINT fk_event_log_transporters_transporter_id FOREIGN KEY (transporter_id) REFERENCES transporter (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE TABLE guardian (
        id uuid NOT NULL,
        transporter_id uuid NOT NULL,
        user_account_id uuid NOT NULL,
        name character varying(150) NOT NULL,
        phone character varying(30),
        mobile character varying(30),
        whatsapp character varying(30),
        contact_email character varying(254),
        address_street character varying(200),
        address_number character varying(20),
        address_complement character varying(100),
        address_district character varying(100),
        address_city character varying(100),
        address_state character varying(2),
        address_postal_code character varying(12),
        address_latitude double precision,
        address_longitude double precision,
        created_at_utc timestamp with time zone NOT NULL,
        updated_at_utc timestamp with time zone NOT NULL,
        deleted_at_utc timestamp with time zone,
        CONSTRAINT pk_guardian PRIMARY KEY (id),
        CONSTRAINT fk_guardian_transporters_transporter_id FOREIGN KEY (transporter_id) REFERENCES transporter (id) ON DELETE RESTRICT,
        CONSTRAINT fk_guardian_user_accounts_user_account_id FOREIGN KEY (user_account_id) REFERENCES user_account (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE TABLE school (
        id uuid NOT NULL,
        transporter_id uuid NOT NULL,
        name character varying(150) NOT NULL,
        address_street character varying(200),
        address_number character varying(20),
        address_complement character varying(100),
        address_district character varying(100),
        address_city character varying(100),
        address_state character varying(2),
        address_postal_code character varying(12),
        address_latitude double precision,
        address_longitude double precision,
        created_at_utc timestamp with time zone NOT NULL,
        updated_at_utc timestamp with time zone NOT NULL,
        deleted_at_utc timestamp with time zone,
        CONSTRAINT pk_school PRIMARY KEY (id),
        CONSTRAINT fk_school_transporters_transporter_id FOREIGN KEY (transporter_id) REFERENCES transporter (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE TABLE vehicle (
        id uuid NOT NULL,
        transporter_id uuid NOT NULL,
        plate character varying(10) NOT NULL,
        model character varying(80),
        capacity integer,
        active boolean NOT NULL,
        created_at_utc timestamp with time zone NOT NULL,
        updated_at_utc timestamp with time zone NOT NULL,
        deleted_at_utc timestamp with time zone,
        CONSTRAINT pk_vehicle PRIMARY KEY (id),
        CONSTRAINT fk_vehicle_transporter_transporter_id FOREIGN KEY (transporter_id) REFERENCES transporter (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE TABLE student (
        id uuid NOT NULL,
        transporter_id uuid NOT NULL,
        user_account_id uuid,
        name character varying(150) NOT NULL,
        birth_date date NOT NULL,
        grade character varying(20),
        school_id uuid,
        created_at_utc timestamp with time zone NOT NULL,
        updated_at_utc timestamp with time zone NOT NULL,
        deleted_at_utc timestamp with time zone,
        CONSTRAINT pk_student PRIMARY KEY (id),
        CONSTRAINT fk_student_school_school_id FOREIGN KEY (school_id) REFERENCES school (id) ON DELETE RESTRICT,
        CONSTRAINT fk_student_transporters_transporter_id FOREIGN KEY (transporter_id) REFERENCES transporter (id) ON DELETE RESTRICT,
        CONSTRAINT fk_student_user_accounts_user_account_id FOREIGN KEY (user_account_id) REFERENCES user_account (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE TABLE transport_group (
        id uuid NOT NULL,
        transporter_id uuid NOT NULL,
        name character varying(120) NOT NULL,
        shift character varying(40),
        vehicle_id uuid,
        assistant_id uuid,
        created_at_utc timestamp with time zone NOT NULL,
        updated_at_utc timestamp with time zone NOT NULL,
        deleted_at_utc timestamp with time zone,
        CONSTRAINT pk_transport_group PRIMARY KEY (id),
        CONSTRAINT fk_transport_group_assistant_assistant_id FOREIGN KEY (assistant_id) REFERENCES assistant (id) ON DELETE RESTRICT,
        CONSTRAINT fk_transport_group_transporters_transporter_id FOREIGN KEY (transporter_id) REFERENCES transporter (id) ON DELETE RESTRICT,
        CONSTRAINT fk_transport_group_vehicles_vehicle_id FOREIGN KEY (vehicle_id) REFERENCES vehicle (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE TABLE guardian_student (
        id uuid NOT NULL,
        guardian_id uuid NOT NULL,
        student_id uuid NOT NULL,
        relationship character varying(20) NOT NULL,
        is_primary boolean NOT NULL,
        can_pickup boolean NOT NULL,
        active boolean NOT NULL,
        started_at_utc timestamp with time zone NOT NULL,
        ended_at_utc timestamp with time zone,
        created_at_utc timestamp with time zone NOT NULL,
        updated_at_utc timestamp with time zone NOT NULL,
        CONSTRAINT pk_guardian_student PRIMARY KEY (id),
        CONSTRAINT fk_guardian_student_guardian_guardian_id FOREIGN KEY (guardian_id) REFERENCES guardian (id) ON DELETE RESTRICT,
        CONSTRAINT fk_guardian_student_students_student_id FOREIGN KEY (student_id) REFERENCES student (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE TABLE attendance_session (
        id uuid NOT NULL,
        transporter_id uuid NOT NULL,
        transport_group_id uuid NOT NULL,
        session_type character varying(15) NOT NULL,
        session_date date NOT NULL,
        status character varying(20) NOT NULL,
        opened_at_utc timestamp with time zone,
        closed_at_utc timestamp with time zone,
        vehicle_id uuid,
        assistant_id uuid,
        created_by uuid NOT NULL,
        created_at_utc timestamp with time zone NOT NULL,
        updated_at_utc timestamp with time zone NOT NULL,
        CONSTRAINT pk_attendance_session PRIMARY KEY (id),
        CONSTRAINT ck_attendance_session_status CHECK (status IN ('Open','Closed','Canceled')),
        CONSTRAINT ck_attendance_session_type CHECK (session_type IN ('ToSchool','FromSchool')),
        CONSTRAINT fk_attendance_session_transport_groups_transport_group_id FOREIGN KEY (transport_group_id) REFERENCES transport_group (id) ON DELETE RESTRICT,
        CONSTRAINT fk_attendance_session_transporters_transporter_id FOREIGN KEY (transporter_id) REFERENCES transporter (id) ON DELETE RESTRICT,
        CONSTRAINT fk_attendance_session_user_accounts_created_by FOREIGN KEY (created_by) REFERENCES user_account (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE TABLE enrollment (
        id uuid NOT NULL,
        student_id uuid NOT NULL,
        transport_group_id uuid NOT NULL,
        active boolean NOT NULL,
        started_at_utc timestamp with time zone NOT NULL,
        ended_at_utc timestamp with time zone,
        created_at_utc timestamp with time zone NOT NULL,
        updated_at_utc timestamp with time zone NOT NULL,
        CONSTRAINT pk_enrollment PRIMARY KEY (id),
        CONSTRAINT fk_enrollment_students_student_id FOREIGN KEY (student_id) REFERENCES student (id) ON DELETE RESTRICT,
        CONSTRAINT fk_enrollment_transport_groups_transport_group_id FOREIGN KEY (transport_group_id) REFERENCES transport_group (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE TABLE attendance_record (
        id uuid NOT NULL,
        attendance_session_id uuid NOT NULL,
        student_id uuid NOT NULL,
        transporter_id uuid NOT NULL,
        status character varying(30) NOT NULL,
        picked_up_by_guardian_id uuid,
        justification character varying(500),
        justified_by uuid,
        school_id uuid,
        recorded_at_utc timestamp with time zone NOT NULL,
        recorded_by uuid NOT NULL,
        created_at_utc timestamp with time zone NOT NULL,
        updated_at_utc timestamp with time zone NOT NULL,
        CONSTRAINT pk_attendance_record PRIMARY KEY (id),
        CONSTRAINT ck_attendance_record_status CHECK (status IN ('Present','Absent','Late','PickedUpByGuardian','Justified')),
        CONSTRAINT fk_attendance_record_attendance_sessions_attendance_session_id FOREIGN KEY (attendance_session_id) REFERENCES attendance_session (id) ON DELETE RESTRICT,
        CONSTRAINT fk_attendance_record_guardians_justified_by FOREIGN KEY (justified_by) REFERENCES guardian (id) ON DELETE RESTRICT,
        CONSTRAINT fk_attendance_record_guardians_picked_up_by_guardian_id FOREIGN KEY (picked_up_by_guardian_id) REFERENCES guardian (id) ON DELETE RESTRICT,
        CONSTRAINT fk_attendance_record_students_student_id FOREIGN KEY (student_id) REFERENCES student (id) ON DELETE RESTRICT,
        CONSTRAINT fk_attendance_record_transporters_transporter_id FOREIGN KEY (transporter_id) REFERENCES transporter (id) ON DELETE RESTRICT,
        CONSTRAINT fk_attendance_record_user_accounts_recorded_by FOREIGN KEY (recorded_by) REFERENCES user_account (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE INDEX ix_assistant_transporter ON assistant (transporter_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_assistant_user_account ON assistant (user_account_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE INDEX ix_attendance_record_justified_by ON attendance_record (justified_by);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE INDEX ix_attendance_record_picked_up ON attendance_record (transporter_id, recorded_at_utc) WHERE status = 'PickedUpByGuardian';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE INDEX ix_attendance_record_picked_up_by_guardian_id ON attendance_record (picked_up_by_guardian_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE INDEX ix_attendance_record_recorded_by ON attendance_record (recorded_by);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_attendance_record_session_student ON attendance_record (attendance_session_id, student_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE INDEX ix_attendance_record_student_date ON attendance_record (student_id, recorded_at_utc);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE INDEX ix_attendance_session_created_by ON attendance_session (created_by);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_attendance_session_group_date_type ON attendance_session (transport_group_id, session_date, session_type);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE INDEX ix_attendance_session_transporter_date ON attendance_session (transporter_id, session_date);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_enrollment_active ON enrollment (student_id, transport_group_id) WHERE active;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE INDEX ix_enrollment_student ON enrollment (student_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE INDEX ix_enrollment_transport_group ON enrollment (transport_group_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE INDEX ix_event_log_aggregate ON event_log (aggregate_type, aggregate_id, occurred_at_utc);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE INDEX ix_event_log_transporter_date ON event_log (transporter_id, occurred_at_utc);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE INDEX ix_guardian_transporter ON guardian (transporter_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE INDEX ix_guardian_user_account ON guardian (user_account_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_guardian_student_pair ON guardian_student (guardian_id, student_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_guardian_student_primary ON guardian_student (student_id) WHERE is_primary AND active;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE INDEX ix_school_transporter ON school (transporter_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_school_transporter_name ON school (transporter_id, name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE INDEX ix_student_school ON student (school_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE INDEX ix_student_transporter ON student (transporter_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE INDEX ix_student_user_account_id ON student (user_account_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE INDEX ix_transport_group_assistant_id ON transport_group (assistant_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE INDEX ix_transport_group_transporter ON transport_group (transporter_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE INDEX ix_transport_group_vehicle_id ON transport_group (vehicle_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_transporter_document ON transporter (document_number);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_transporter_user_account ON transporter (user_account_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_user_account_email ON user_account (email);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE INDEX ix_vehicle_transporter ON vehicle (transporter_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_vehicle_transporter_plate ON vehicle (transporter_id, plate);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722130906_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260722130906_InitialCreate', '10.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722153532_EnrollmentActiveGroupIndex') THEN
    CREATE INDEX ix_enrollment_active_group ON enrollment (transport_group_id) WHERE active;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260722153532_EnrollmentActiveGroupIndex') THEN
    INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260722153532_EnrollmentActiveGroupIndex', '10.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260827124719_AddCalendarDay') THEN
    CREATE TABLE calendar_day (
        id uuid NOT NULL,
        transporter_id uuid NOT NULL,
        date date NOT NULL,
        type character varying(15) NOT NULL,
        description character varying(200),
        created_at_utc timestamp with time zone NOT NULL,
        updated_at_utc timestamp with time zone NOT NULL,
        CONSTRAINT pk_calendar_day PRIMARY KEY (id),
        CONSTRAINT ck_calendar_day_type CHECK (type IN ('SchoolDay','Holiday')),
        CONSTRAINT fk_calendar_day_transporters_transporter_id FOREIGN KEY (transporter_id) REFERENCES transporter (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260827124719_AddCalendarDay') THEN
    CREATE UNIQUE INDEX ix_calendar_day_transporter_date ON calendar_day (transporter_id, date);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260827124719_AddCalendarDay') THEN
    INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260827124719_AddCalendarDay', '10.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20261029210510_AddGuardianConsent') THEN
    CREATE TABLE guardian_consent (
        id uuid NOT NULL,
        transporter_id uuid NOT NULL,
        guardian_id uuid NOT NULL,
        student_id uuid NOT NULL,
        terms_version character varying(20) NOT NULL,
        granted_at_utc timestamp with time zone NOT NULL,
        revoked_at_utc timestamp with time zone,
        CONSTRAINT pk_guardian_consent PRIMARY KEY (id),
        CONSTRAINT fk_guardian_consent_guardian_guardian_id FOREIGN KEY (guardian_id) REFERENCES guardian (id) ON DELETE RESTRICT,
        CONSTRAINT fk_guardian_consent_students_student_id FOREIGN KEY (student_id) REFERENCES student (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20261029210510_AddGuardianConsent') THEN
    CREATE UNIQUE INDEX ix_guardian_consent_active ON guardian_consent (guardian_id, student_id) WHERE revoked_at_utc IS NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20261029210510_AddGuardianConsent') THEN
    CREATE INDEX ix_guardian_consent_student_id ON guardian_consent (student_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20261029210510_AddGuardianConsent') THEN
    INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20261029210510_AddGuardianConsent', '10.0.10');
    END IF;
END $EF$;
COMMIT;

