using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendanceManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_account",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    password_hash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    primary_role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    email_confirmed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_account", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "transporter",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    document_type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    document_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    license_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    license_category = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    license_expiry = table.Column<DateOnly>(type: "date", nullable: true),
                    business_phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transporter", x => x.id);
                    table.ForeignKey(
                        name: "fk_transporter_user_accounts_user_account_id",
                        column: x => x.user_account_id,
                        principalTable: "user_account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "assistant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    transporter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_assistant", x => x.id);
                    table.ForeignKey(
                        name: "fk_assistant_transporters_transporter_id",
                        column: x => x.transporter_id,
                        principalTable: "transporter",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_assistant_user_accounts_user_account_id",
                        column: x => x.user_account_id,
                        principalTable: "user_account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "event_log",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    transporter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    occurred_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    event_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    aggregate_type = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    aggregate_id = table.Column<Guid>(type: "uuid", nullable: false),
                    actor_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    payload = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_event_log", x => x.id);
                    table.ForeignKey(
                        name: "fk_event_log_transporters_transporter_id",
                        column: x => x.transporter_id,
                        principalTable: "transporter",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "guardian",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    transporter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    mobile = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    whatsapp = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    contact_email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    address_street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    address_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    address_complement = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    address_district = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    address_city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    address_state = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    address_postal_code = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    address_latitude = table.Column<double>(type: "double precision", nullable: true),
                    address_longitude = table.Column<double>(type: "double precision", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_guardian", x => x.id);
                    table.ForeignKey(
                        name: "fk_guardian_transporters_transporter_id",
                        column: x => x.transporter_id,
                        principalTable: "transporter",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_guardian_user_accounts_user_account_id",
                        column: x => x.user_account_id,
                        principalTable: "user_account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "school",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    transporter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    address_street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    address_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    address_complement = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    address_district = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    address_city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    address_state = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    address_postal_code = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    address_latitude = table.Column<double>(type: "double precision", nullable: true),
                    address_longitude = table.Column<double>(type: "double precision", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_school", x => x.id);
                    table.ForeignKey(
                        name: "fk_school_transporters_transporter_id",
                        column: x => x.transporter_id,
                        principalTable: "transporter",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vehicle",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    transporter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plate = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    model = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    capacity = table.Column<int>(type: "integer", nullable: true),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vehicle", x => x.id);
                    table.ForeignKey(
                        name: "fk_vehicle_transporter_transporter_id",
                        column: x => x.transporter_id,
                        principalTable: "transporter",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "student",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    transporter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_account_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: false),
                    grade = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    school_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_student", x => x.id);
                    table.ForeignKey(
                        name: "fk_student_school_school_id",
                        column: x => x.school_id,
                        principalTable: "school",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_student_transporters_transporter_id",
                        column: x => x.transporter_id,
                        principalTable: "transporter",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_student_user_accounts_user_account_id",
                        column: x => x.user_account_id,
                        principalTable: "user_account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "transport_group",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    transporter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    shift = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: true),
                    assistant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transport_group", x => x.id);
                    table.ForeignKey(
                        name: "fk_transport_group_assistant_assistant_id",
                        column: x => x.assistant_id,
                        principalTable: "assistant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transport_group_transporters_transporter_id",
                        column: x => x.transporter_id,
                        principalTable: "transporter",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transport_group_vehicles_vehicle_id",
                        column: x => x.vehicle_id,
                        principalTable: "vehicle",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "guardian_student",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    guardian_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    relationship = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false),
                    can_pickup = table.Column<bool>(type: "boolean", nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    started_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ended_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_guardian_student", x => x.id);
                    table.ForeignKey(
                        name: "fk_guardian_student_guardian_guardian_id",
                        column: x => x.guardian_id,
                        principalTable: "guardian",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_guardian_student_students_student_id",
                        column: x => x.student_id,
                        principalTable: "student",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "attendance_session",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    transporter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    transport_group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_type = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    session_date = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    opened_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    closed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: true),
                    assistant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_attendance_session", x => x.id);
                    table.CheckConstraint("ck_attendance_session_status", "status IN ('Open','Closed','Canceled')");
                    table.CheckConstraint("ck_attendance_session_type", "session_type IN ('ToSchool','FromSchool')");
                    table.ForeignKey(
                        name: "fk_attendance_session_transport_groups_transport_group_id",
                        column: x => x.transport_group_id,
                        principalTable: "transport_group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_attendance_session_transporters_transporter_id",
                        column: x => x.transporter_id,
                        principalTable: "transporter",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_attendance_session_user_accounts_created_by",
                        column: x => x.created_by,
                        principalTable: "user_account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "enrollment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    transport_group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    started_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ended_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_enrollment", x => x.id);
                    table.ForeignKey(
                        name: "fk_enrollment_students_student_id",
                        column: x => x.student_id,
                        principalTable: "student",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_enrollment_transport_groups_transport_group_id",
                        column: x => x.transport_group_id,
                        principalTable: "transport_group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "attendance_record",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    attendance_session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    transporter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    picked_up_by_guardian_id = table.Column<Guid>(type: "uuid", nullable: true),
                    justification = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    justified_by = table.Column<Guid>(type: "uuid", nullable: true),
                    school_id = table.Column<Guid>(type: "uuid", nullable: true),
                    recorded_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    recorded_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_attendance_record", x => x.id);
                    table.CheckConstraint("ck_attendance_record_status", "status IN ('Present','Absent','Late','PickedUpByGuardian','Justified')");
                    table.ForeignKey(
                        name: "fk_attendance_record_attendance_sessions_attendance_session_id",
                        column: x => x.attendance_session_id,
                        principalTable: "attendance_session",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_attendance_record_guardians_justified_by",
                        column: x => x.justified_by,
                        principalTable: "guardian",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_attendance_record_guardians_picked_up_by_guardian_id",
                        column: x => x.picked_up_by_guardian_id,
                        principalTable: "guardian",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_attendance_record_students_student_id",
                        column: x => x.student_id,
                        principalTable: "student",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_attendance_record_transporters_transporter_id",
                        column: x => x.transporter_id,
                        principalTable: "transporter",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_attendance_record_user_accounts_recorded_by",
                        column: x => x.recorded_by,
                        principalTable: "user_account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_assistant_transporter",
                table: "assistant",
                column: "transporter_id");

            migrationBuilder.CreateIndex(
                name: "ix_assistant_user_account",
                table: "assistant",
                column: "user_account_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_attendance_record_justified_by",
                table: "attendance_record",
                column: "justified_by");

            migrationBuilder.CreateIndex(
                name: "ix_attendance_record_picked_up",
                table: "attendance_record",
                columns: new[] { "transporter_id", "recorded_at_utc" },
                filter: "status = 'PickedUpByGuardian'");

            migrationBuilder.CreateIndex(
                name: "ix_attendance_record_picked_up_by_guardian_id",
                table: "attendance_record",
                column: "picked_up_by_guardian_id");

            migrationBuilder.CreateIndex(
                name: "ix_attendance_record_recorded_by",
                table: "attendance_record",
                column: "recorded_by");

            migrationBuilder.CreateIndex(
                name: "ix_attendance_record_session_student",
                table: "attendance_record",
                columns: new[] { "attendance_session_id", "student_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_attendance_record_student_date",
                table: "attendance_record",
                columns: new[] { "student_id", "recorded_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_attendance_session_created_by",
                table: "attendance_session",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_attendance_session_group_date_type",
                table: "attendance_session",
                columns: new[] { "transport_group_id", "session_date", "session_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_attendance_session_transporter_date",
                table: "attendance_session",
                columns: new[] { "transporter_id", "session_date" });

            migrationBuilder.CreateIndex(
                name: "ix_enrollment_active",
                table: "enrollment",
                columns: new[] { "student_id", "transport_group_id" },
                unique: true,
                filter: "active");

            migrationBuilder.CreateIndex(
                name: "ix_enrollment_student",
                table: "enrollment",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "ix_enrollment_transport_group",
                table: "enrollment",
                column: "transport_group_id");

            migrationBuilder.CreateIndex(
                name: "ix_event_log_aggregate",
                table: "event_log",
                columns: new[] { "aggregate_type", "aggregate_id", "occurred_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_event_log_transporter_date",
                table: "event_log",
                columns: new[] { "transporter_id", "occurred_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_guardian_transporter",
                table: "guardian",
                column: "transporter_id");

            migrationBuilder.CreateIndex(
                name: "ix_guardian_user_account",
                table: "guardian",
                column: "user_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_guardian_student_pair",
                table: "guardian_student",
                columns: new[] { "guardian_id", "student_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_guardian_student_primary",
                table: "guardian_student",
                column: "student_id",
                unique: true,
                filter: "is_primary AND active");

            migrationBuilder.CreateIndex(
                name: "ix_school_transporter",
                table: "school",
                column: "transporter_id");

            migrationBuilder.CreateIndex(
                name: "ix_school_transporter_name",
                table: "school",
                columns: new[] { "transporter_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_student_school",
                table: "student",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "ix_student_transporter",
                table: "student",
                column: "transporter_id");

            migrationBuilder.CreateIndex(
                name: "ix_student_user_account_id",
                table: "student",
                column: "user_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_transport_group_assistant_id",
                table: "transport_group",
                column: "assistant_id");

            migrationBuilder.CreateIndex(
                name: "ix_transport_group_transporter",
                table: "transport_group",
                column: "transporter_id");

            migrationBuilder.CreateIndex(
                name: "ix_transport_group_vehicle_id",
                table: "transport_group",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "ix_transporter_document",
                table: "transporter",
                column: "document_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_transporter_user_account",
                table: "transporter",
                column: "user_account_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_account_email",
                table: "user_account",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_transporter",
                table: "vehicle",
                column: "transporter_id");

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_transporter_plate",
                table: "vehicle",
                columns: new[] { "transporter_id", "plate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "attendance_record");

            migrationBuilder.DropTable(
                name: "enrollment");

            migrationBuilder.DropTable(
                name: "event_log");

            migrationBuilder.DropTable(
                name: "guardian_student");

            migrationBuilder.DropTable(
                name: "attendance_session");

            migrationBuilder.DropTable(
                name: "guardian");

            migrationBuilder.DropTable(
                name: "student");

            migrationBuilder.DropTable(
                name: "transport_group");

            migrationBuilder.DropTable(
                name: "school");

            migrationBuilder.DropTable(
                name: "assistant");

            migrationBuilder.DropTable(
                name: "vehicle");

            migrationBuilder.DropTable(
                name: "transporter");

            migrationBuilder.DropTable(
                name: "user_account");
        }
    }
}
