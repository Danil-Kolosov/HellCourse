-- V2__AddRowVersion.sql
-- Добавление поля RowVersion для оптимистичной блокировки

-- Добавляем поле RowVersion в таблицу Employees
ALTER TABLE "Employees" ADD COLUMN "RowVersion" BLOB;

-- Обновляем существующие записи (устанавливаем значение по умолчанию)
UPDATE "Employees" SET "RowVersion" = randomblob(8) WHERE "RowVersion" IS NULL;
