-- V1__InitialCreate.sql
-- Создание таблиц для строительной компании

-- Создание таблицы строительных площадок
CREATE TABLE IF NOT EXISTS "BuildingSites" (
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Address" TEXT NOT NULL
);

-- Создание таблицы сотрудников
CREATE TABLE IF NOT EXISTS "Employees" (
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT,
    "FullName" TEXT NOT NULL,
    "Position" TEXT NOT NULL
);

-- Создание таблицы типов работ
CREATE TABLE IF NOT EXISTS "WorkTypes" (
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT,
    "Title" TEXT NOT NULL,
    "PricePerHour" TEXT NOT NULL  -- TEXT для decimal в SQLite
);

-- Создание таблицы журнала работ
CREATE TABLE IF NOT EXISTS "WorkLogs" (
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT,
    "BuildingSiteId" INTEGER NOT NULL,
    "WorkTypeId" INTEGER NOT NULL,
    "EmployeeId" INTEGER NOT NULL,
    "HoursWorked" INTEGER NOT NULL,
    "Status" TEXT NOT NULL,
    FOREIGN KEY ("BuildingSiteId") REFERENCES "BuildingSites"("Id") ON DELETE RESTRICT,
    FOREIGN KEY ("WorkTypeId") REFERENCES "WorkTypes"("Id") ON DELETE RESTRICT,
    FOREIGN KEY ("EmployeeId") REFERENCES "Employees"("Id") ON DELETE RESTRICT
);

-- Создание индексов для оптимизации запросов
--CREATE INDEX IF NOT EXISTS "IX_WorkLogs_BuildingSiteId" ON "WorkLogs" ("BuildingSiteId");
--CREATE INDEX IF NOT EXISTS "IX_WorkLogs_EmployeeId" ON "WorkLogs" ("EmployeeId");
--CREATE INDEX IF NOT EXISTS "IX_WorkLogs_WorkTypeId" ON "WorkLogs" ("WorkTypeId");