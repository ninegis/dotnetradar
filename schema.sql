CREATE TABLE "AlarmHandleRecords" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AlarmHandleRecords" PRIMARY KEY AUTOINCREMENT,
    "HandleId" TEXT NOT NULL,
    "Photo" TEXT NOT NULL,
    "Video" TEXT NOT NULL,
    "HandleDescription" TEXT NOT NULL,
    "HandleTime" TEXT NOT NULL,
    "Handler" TEXT NOT NULL,
    "CreateTime" TEXT NOT NULL,
    "UpdateTime" TEXT NOT NULL,
    "IsDeleted" INTEGER NOT NULL
);


CREATE TABLE "AlarmRecords" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AlarmRecords" PRIMARY KEY AUTOINCREMENT,
    "HandleId" TEXT NOT NULL,
    "RuleId" TEXT NOT NULL,
    "ProjectId" TEXT NOT NULL,
    "Timestamp" TEXT NOT NULL,
    "AlarmStatus" INTEGER NOT NULL,
    "AlarmLevel" INTEGER NOT NULL,
    "AlarmContent" TEXT NOT NULL,
    "HandleStatus" TEXT NOT NULL,
    "ScanStatus" TEXT NOT NULL,
    "CreateTime" TEXT NOT NULL,
    "UpdateTime" TEXT NOT NULL,
    "IsDeleted" INTEGER NOT NULL
);


CREATE TABLE "Devices" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Devices" PRIMARY KEY AUTOINCREMENT,
    "DeviceId" TEXT NOT NULL,
    "ProjectId" TEXT NOT NULL,
    "DeviceName" TEXT NOT NULL,
    "DeviceType" TEXT NOT NULL,
    "DeviceTypeCode" INTEGER NOT NULL,
    "Status" TEXT NOT NULL,
    "Location" TEXT NOT NULL,
    "IpAddress" TEXT NOT NULL,
    "Port" INTEGER NOT NULL,
    "MqttTopic" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "LastUpdateTime" TEXT NOT NULL,
    "CreateTime" TEXT NOT NULL,
    "UpdateTime" TEXT NOT NULL,
    "IsDeleted" INTEGER NOT NULL
);


CREATE TABLE "Projects" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Projects" PRIMARY KEY AUTOINCREMENT,
    "ProjectId" TEXT NOT NULL,
    "ProjectName" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "Location" TEXT NOT NULL,
    "Status" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "StoragePath" TEXT NOT NULL,
    "ContactPerson" TEXT NOT NULL,
    "ContactPhone" TEXT NOT NULL,
    "ContactEmail" TEXT NOT NULL,
    "Longitude" REAL NOT NULL,
    "Latitude" REAL NOT NULL,
    "Elevation" REAL NOT NULL,
    "StartDate" TEXT NOT NULL,
    "EndDate" TEXT NULL,
    "CreateTime" TEXT NOT NULL,
    "UpdateTime" TEXT NOT NULL,
    "IsDeleted" INTEGER NOT NULL,
    CONSTRAINT "AK_Projects_ProjectId" UNIQUE ("ProjectId")
);


CREATE TABLE "RadarData" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_RadarData" PRIMARY KEY AUTOINCREMENT,
    "DeviceId" TEXT NOT NULL,
    "ProjectId" TEXT NOT NULL,
    "Timestamp" TEXT NOT NULL,
    "DataType" TEXT NOT NULL,
    "Sequence" INTEGER NOT NULL,
    "FileName" TEXT NOT NULL,
    "Duration" INTEGER NOT NULL,
    "Status" TEXT NOT NULL,
    "TaskId" INTEGER NOT NULL,
    "ImageData" BLOB NOT NULL,
    "RangeResolution" REAL NOT NULL,
    "AngleResolution" REAL NOT NULL,
    "RangeMin" REAL NOT NULL,
    "AngleMin" REAL NOT NULL,
    "RangeNumber" INTEGER NOT NULL,
    "AngleNumber" INTEGER NOT NULL,
    "CreateTime" TEXT NOT NULL,
    "UpdateTime" TEXT NOT NULL,
    "IsDeleted" INTEGER NOT NULL
);


CREATE TABLE "users" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_users" PRIMARY KEY,
    "Username" TEXT NOT NULL,
    "PasswordHash" TEXT NOT NULL,
    "Email" TEXT NULL,
    "Phone" TEXT NULL,
    "RealName" TEXT NULL,
    "Role" TEXT NULL,
    "IsActive" INTEGER NOT NULL,
    "LastLoginTime" TEXT NULL,
    "CreatedTime" TEXT NOT NULL,
    "UpdatedTime" TEXT NULL,
    "IsDeleted" INTEGER NOT NULL
);


CREATE TABLE "alarm_rules" (
    "id" TEXT NOT NULL CONSTRAINT "PK_alarm_rules" PRIMARY KEY,
    "project_id" TEXT NOT NULL,
    "rule_name" TEXT NOT NULL,
    "rule_description" TEXT NULL,
    "alarm_content" TEXT NULL,
    "alarm_rule" TEXT NOT NULL,
    "alarm_level" INTEGER NOT NULL,
    "enable" INTEGER NOT NULL,
    "alarm_threshold" REAL NOT NULL,
    "devices_json" TEXT NULL,
    "geo_mark_array_json" TEXT NULL,
    "data_source" TEXT NULL,
    "target_type" TEXT NULL,
    "mode" TEXT NULL,
    "create_time" TEXT NOT NULL,
    "update_time" TEXT NULL,
    "is_deleted" INTEGER NOT NULL,
    CONSTRAINT "FK_alarm_rules_Projects_project_id" FOREIGN KEY ("project_id") REFERENCES "Projects" ("ProjectId") ON DELETE RESTRICT
);


CREATE TABLE "color_settings" (
    "id" TEXT NOT NULL CONSTRAINT "PK_color_settings" PRIMARY KEY,
    "project_id" TEXT NOT NULL,
    "setting_type" TEXT NOT NULL,
    "type" INTEGER NOT NULL,
    "min_value" REAL NOT NULL,
    "max_value" REAL NOT NULL,
    "hsl_h_start" INTEGER NOT NULL,
    "hsl_h_end" INTEGER NOT NULL,
    "hsl_direction" INTEGER NOT NULL,
    "filter_enable" INTEGER NOT NULL,
    "filter_min" REAL NULL,
    "filter_max" REAL NULL,
    "filter_alpha" REAL NULL,
    "hsl_s" REAL NOT NULL,
    "hsl_l" REAL NOT NULL,
    "value_array_json" TEXT NULL,
    "color_array_json" TEXT NULL,
    "auto_mode" INTEGER NOT NULL,
    "create_time" TEXT NOT NULL,
    "update_time" TEXT NULL,
    CONSTRAINT "FK_color_settings_Projects_project_id" FOREIGN KEY ("project_id") REFERENCES "Projects" ("ProjectId") ON DELETE RESTRICT
);


CREATE TABLE "geo_marks" (
    "id" TEXT NOT NULL CONSTRAINT "PK_geo_marks" PRIMARY KEY,
    "project_id" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "type" TEXT NOT NULL,
    "coordinates_json" TEXT NULL,
    "description" TEXT NULL,
    "color" TEXT NULL,
    "icon" TEXT NULL,
    "create_time" TEXT NOT NULL,
    "update_time" TEXT NULL,
    "is_deleted" INTEGER NOT NULL,
    CONSTRAINT "FK_geo_marks_Projects_project_id" FOREIGN KEY ("project_id") REFERENCES "Projects" ("ProjectId") ON DELETE RESTRICT
);


CREATE TABLE "image_analysis_configs" (
    "id" TEXT NOT NULL CONSTRAINT "PK_image_analysis_configs" PRIMARY KEY,
    "project_id" TEXT NOT NULL,
    "standard_image_side_pixel" INTEGER NOT NULL,
    "compress_image_side_pixel" INTEGER NOT NULL,
    "matrix_tile_rng_num" INTEGER NOT NULL,
    "matrix_tile_ang_num" INTEGER NOT NULL,
    "gen_defo" INTEGER NOT NULL,
    "gen_scat" INTEGER NOT NULL,
    "gen_speed" INTEGER NOT NULL,
    "gen_acceleration" INTEGER NOT NULL,
    "config_json" TEXT NULL,
    "create_time" TEXT NOT NULL,
    "update_time" TEXT NULL,
    CONSTRAINT "FK_image_analysis_configs_Projects_project_id" FOREIGN KEY ("project_id") REFERENCES "Projects" ("ProjectId") ON DELETE RESTRICT
);


CREATE TABLE "image_marks" (
    "id" TEXT NOT NULL CONSTRAINT "PK_image_marks" PRIMARY KEY,
    "project_id" TEXT NOT NULL,
    "image_id" TEXT NULL,
    "name" TEXT NOT NULL,
    "mark_type" TEXT NOT NULL,
    "coordinates_json" TEXT NULL,
    "description" TEXT NULL,
    "color" TEXT NULL,
    "create_time" TEXT NOT NULL,
    "update_time" TEXT NULL,
    "is_deleted" INTEGER NOT NULL,
    CONSTRAINT "FK_image_marks_Projects_project_id" FOREIGN KEY ("project_id") REFERENCES "Projects" ("ProjectId") ON DELETE RESTRICT
);


CREATE TABLE "panel_configs" (
    "id" TEXT NOT NULL CONSTRAINT "PK_panel_configs" PRIMARY KEY,
    "project_id" TEXT NOT NULL,
    "panel_type" TEXT NOT NULL,
    "config_json" TEXT NOT NULL,
    "create_time" TEXT NOT NULL,
    "update_time" TEXT NULL,
    CONSTRAINT "FK_panel_configs_Projects_project_id" FOREIGN KEY ("project_id") REFERENCES "Projects" ("ProjectId") ON DELETE RESTRICT
);


CREATE INDEX "IX_alarm_rules_project_id" ON "alarm_rules" ("project_id");


CREATE INDEX "IX_alarm_rules_project_id_enable_is_deleted" ON "alarm_rules" ("project_id", "enable", "is_deleted");


CREATE INDEX "IX_alarm_rules_rule_name" ON "alarm_rules" ("rule_name");


CREATE INDEX "IX_AlarmHandleRecords_HandleId" ON "AlarmHandleRecords" ("HandleId");


CREATE INDEX "IX_AlarmRecords_ProjectId_Timestamp" ON "AlarmRecords" ("ProjectId", "Timestamp");


CREATE INDEX "IX_AlarmRecords_RuleId_Timestamp" ON "AlarmRecords" ("RuleId", "Timestamp");


CREATE INDEX "IX_AlarmRecords_Timestamp" ON "AlarmRecords" ("Timestamp");


CREATE INDEX "IX_color_settings_project_id" ON "color_settings" ("project_id");


CREATE INDEX "IX_color_settings_project_id_setting_type" ON "color_settings" ("project_id", "setting_type");


CREATE UNIQUE INDEX "IX_Devices_DeviceId" ON "Devices" ("DeviceId");


CREATE INDEX "IX_Devices_ProjectId" ON "Devices" ("ProjectId");


CREATE INDEX "IX_Devices_ProjectId_DeviceId" ON "Devices" ("ProjectId", "DeviceId");


CREATE INDEX "IX_geo_marks_name" ON "geo_marks" ("name");


CREATE INDEX "IX_geo_marks_project_id" ON "geo_marks" ("project_id");


CREATE INDEX "IX_geo_marks_project_id_is_deleted" ON "geo_marks" ("project_id", "is_deleted");


CREATE UNIQUE INDEX "IX_image_analysis_configs_project_id" ON "image_analysis_configs" ("project_id");


CREATE INDEX "IX_image_marks_image_id" ON "image_marks" ("image_id");


CREATE INDEX "IX_image_marks_project_id" ON "image_marks" ("project_id");


CREATE INDEX "IX_image_marks_project_id_is_deleted" ON "image_marks" ("project_id", "is_deleted");


CREATE INDEX "IX_panel_configs_project_id" ON "panel_configs" ("project_id");


CREATE UNIQUE INDEX "IX_panel_configs_project_id_panel_type" ON "panel_configs" ("project_id", "panel_type");


CREATE INDEX "IX_Projects_ProjectName" ON "Projects" ("ProjectName");


CREATE INDEX "IX_Projects_Status" ON "Projects" ("Status");


CREATE INDEX "IX_RadarData_DeviceId_Timestamp" ON "RadarData" ("DeviceId", "Timestamp");


CREATE INDEX "IX_RadarData_ProjectId_Timestamp" ON "RadarData" ("ProjectId", "Timestamp");


CREATE INDEX "IX_RadarData_Timestamp" ON "RadarData" ("Timestamp");


CREATE INDEX "IX_users_Email" ON "users" ("Email");


CREATE UNIQUE INDEX "IX_users_Username" ON "users" ("Username");


CREATE INDEX "IX_users_Username_IsDeleted" ON "users" ("Username", "IsDeleted");


