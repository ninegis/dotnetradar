using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RadarSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddConfigurationTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlarmHandleRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HandleId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Photo = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Video = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    HandleDescription = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    HandleTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Handler = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlarmHandleRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AlarmRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HandleId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    RuleId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ProjectId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AlarmStatus = table.Column<bool>(type: "INTEGER", nullable: false),
                    AlarmLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    AlarmContent = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    HandleStatus = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ScanStatus = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CreateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlarmRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeviceId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ProjectId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DeviceName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DeviceType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DeviceTypeCode = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    IpAddress = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Port = table.Column<int>(type: "INTEGER", nullable: false),
                    MqttTopic = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    LastUpdateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProjectId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ProjectName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    StoragePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ContactPerson = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ContactPhone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ContactEmail = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Longitude = table.Column<double>(type: "REAL", nullable: false),
                    Latitude = table.Column<double>(type: "REAL", nullable: false),
                    Elevation = table.Column<double>(type: "REAL", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                    table.UniqueConstraint("AK_Projects_ProjectId", x => x.ProjectId);
                });

            migrationBuilder.CreateTable(
                name: "RadarData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeviceId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ProjectId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataType = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Sequence = table.Column<int>(type: "INTEGER", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Duration = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    TaskId = table.Column<int>(type: "INTEGER", nullable: false),
                    ImageData = table.Column<byte[]>(type: "BLOB", nullable: false),
                    RangeResolution = table.Column<float>(type: "REAL", nullable: false),
                    AngleResolution = table.Column<float>(type: "REAL", nullable: false),
                    RangeMin = table.Column<float>(type: "REAL", nullable: false),
                    AngleMin = table.Column<float>(type: "REAL", nullable: false),
                    RangeNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    AngleNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RadarData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Username = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    RealName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Role = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastLoginTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "alarm_rules",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    project_id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    rule_name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    rule_description = table.Column<string>(type: "TEXT", nullable: true),
                    alarm_content = table.Column<string>(type: "TEXT", nullable: true),
                    alarm_rule = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    alarm_level = table.Column<int>(type: "INTEGER", nullable: false),
                    enable = table.Column<bool>(type: "INTEGER", nullable: false),
                    alarm_threshold = table.Column<double>(type: "REAL", nullable: false),
                    devices_json = table.Column<string>(type: "TEXT", nullable: true),
                    geo_mark_array_json = table.Column<string>(type: "TEXT", nullable: true),
                    data_source = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    target_type = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    mode = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    create_time = table.Column<DateTime>(type: "TEXT", nullable: false),
                    update_time = table.Column<DateTime>(type: "TEXT", nullable: true),
                    is_deleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alarm_rules", x => x.id);
                    table.ForeignKey(
                        name: "FK_alarm_rules_Projects_project_id",
                        column: x => x.project_id,
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "color_settings",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    project_id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    setting_type = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    type = table.Column<int>(type: "INTEGER", nullable: false),
                    min_value = table.Column<double>(type: "REAL", nullable: false),
                    max_value = table.Column<double>(type: "REAL", nullable: false),
                    hsl_h_start = table.Column<int>(type: "INTEGER", nullable: false),
                    hsl_h_end = table.Column<int>(type: "INTEGER", nullable: false),
                    hsl_direction = table.Column<int>(type: "INTEGER", nullable: false),
                    filter_enable = table.Column<bool>(type: "INTEGER", nullable: false),
                    filter_min = table.Column<double>(type: "REAL", nullable: true),
                    filter_max = table.Column<double>(type: "REAL", nullable: true),
                    filter_alpha = table.Column<double>(type: "REAL", nullable: true),
                    hsl_s = table.Column<double>(type: "REAL", nullable: false),
                    hsl_l = table.Column<double>(type: "REAL", nullable: false),
                    value_array_json = table.Column<string>(type: "TEXT", nullable: true),
                    color_array_json = table.Column<string>(type: "TEXT", nullable: true),
                    auto_mode = table.Column<bool>(type: "INTEGER", nullable: false),
                    create_time = table.Column<DateTime>(type: "TEXT", nullable: false),
                    update_time = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_color_settings", x => x.id);
                    table.ForeignKey(
                        name: "FK_color_settings_Projects_project_id",
                        column: x => x.project_id,
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "geo_marks",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    project_id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    type = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    coordinates_json = table.Column<string>(type: "TEXT", nullable: true),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    color = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    icon = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    create_time = table.Column<DateTime>(type: "TEXT", nullable: false),
                    update_time = table.Column<DateTime>(type: "TEXT", nullable: true),
                    is_deleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_geo_marks", x => x.id);
                    table.ForeignKey(
                        name: "FK_geo_marks_Projects_project_id",
                        column: x => x.project_id,
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "image_analysis_configs",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    project_id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    standard_image_side_pixel = table.Column<int>(type: "INTEGER", nullable: false),
                    compress_image_side_pixel = table.Column<int>(type: "INTEGER", nullable: false),
                    matrix_tile_rng_num = table.Column<int>(type: "INTEGER", nullable: false),
                    matrix_tile_ang_num = table.Column<int>(type: "INTEGER", nullable: false),
                    gen_defo = table.Column<bool>(type: "INTEGER", nullable: false),
                    gen_scat = table.Column<bool>(type: "INTEGER", nullable: false),
                    gen_speed = table.Column<bool>(type: "INTEGER", nullable: false),
                    gen_acceleration = table.Column<bool>(type: "INTEGER", nullable: false),
                    config_json = table.Column<string>(type: "TEXT", nullable: true),
                    create_time = table.Column<DateTime>(type: "TEXT", nullable: false),
                    update_time = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_image_analysis_configs", x => x.id);
                    table.ForeignKey(
                        name: "FK_image_analysis_configs_Projects_project_id",
                        column: x => x.project_id,
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "image_marks",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    project_id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    image_id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    mark_type = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    coordinates_json = table.Column<string>(type: "TEXT", nullable: true),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    color = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    create_time = table.Column<DateTime>(type: "TEXT", nullable: false),
                    update_time = table.Column<DateTime>(type: "TEXT", nullable: true),
                    is_deleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_image_marks", x => x.id);
                    table.ForeignKey(
                        name: "FK_image_marks_Projects_project_id",
                        column: x => x.project_id,
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "panel_configs",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    project_id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    panel_type = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    config_json = table.Column<string>(type: "TEXT", nullable: false),
                    create_time = table.Column<DateTime>(type: "TEXT", nullable: false),
                    update_time = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_panel_configs", x => x.id);
                    table.ForeignKey(
                        name: "FK_panel_configs_Projects_project_id",
                        column: x => x.project_id,
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_alarm_rules_project_id",
                table: "alarm_rules",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_alarm_rules_project_id_enable_is_deleted",
                table: "alarm_rules",
                columns: new[] { "project_id", "enable", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_alarm_rules_rule_name",
                table: "alarm_rules",
                column: "rule_name");

            migrationBuilder.CreateIndex(
                name: "IX_AlarmHandleRecords_HandleId",
                table: "AlarmHandleRecords",
                column: "HandleId");

            migrationBuilder.CreateIndex(
                name: "IX_AlarmRecords_ProjectId_Timestamp",
                table: "AlarmRecords",
                columns: new[] { "ProjectId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_AlarmRecords_RuleId_Timestamp",
                table: "AlarmRecords",
                columns: new[] { "RuleId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_AlarmRecords_Timestamp",
                table: "AlarmRecords",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_color_settings_project_id",
                table: "color_settings",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_color_settings_project_id_setting_type",
                table: "color_settings",
                columns: new[] { "project_id", "setting_type" });

            migrationBuilder.CreateIndex(
                name: "IX_Devices_DeviceId",
                table: "Devices",
                column: "DeviceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Devices_ProjectId",
                table: "Devices",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_ProjectId_DeviceId",
                table: "Devices",
                columns: new[] { "ProjectId", "DeviceId" });

            migrationBuilder.CreateIndex(
                name: "IX_geo_marks_name",
                table: "geo_marks",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_geo_marks_project_id",
                table: "geo_marks",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_geo_marks_project_id_is_deleted",
                table: "geo_marks",
                columns: new[] { "project_id", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_image_analysis_configs_project_id",
                table: "image_analysis_configs",
                column: "project_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_image_marks_image_id",
                table: "image_marks",
                column: "image_id");

            migrationBuilder.CreateIndex(
                name: "IX_image_marks_project_id",
                table: "image_marks",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_image_marks_project_id_is_deleted",
                table: "image_marks",
                columns: new[] { "project_id", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_panel_configs_project_id",
                table: "panel_configs",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_panel_configs_project_id_panel_type",
                table: "panel_configs",
                columns: new[] { "project_id", "panel_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ProjectName",
                table: "Projects",
                column: "ProjectName");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Status",
                table: "Projects",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_RadarData_DeviceId_Timestamp",
                table: "RadarData",
                columns: new[] { "DeviceId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_RadarData_ProjectId_Timestamp",
                table: "RadarData",
                columns: new[] { "ProjectId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_RadarData_Timestamp",
                table: "RadarData",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_users_Username",
                table: "users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_Username_IsDeleted",
                table: "users",
                columns: new[] { "Username", "IsDeleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alarm_rules");

            migrationBuilder.DropTable(
                name: "AlarmHandleRecords");

            migrationBuilder.DropTable(
                name: "AlarmRecords");

            migrationBuilder.DropTable(
                name: "color_settings");

            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropTable(
                name: "geo_marks");

            migrationBuilder.DropTable(
                name: "image_analysis_configs");

            migrationBuilder.DropTable(
                name: "image_marks");

            migrationBuilder.DropTable(
                name: "panel_configs");

            migrationBuilder.DropTable(
                name: "RadarData");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "Projects");
        }
    }
}
