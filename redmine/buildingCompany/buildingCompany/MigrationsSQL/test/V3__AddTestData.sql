-- V3__AddTestData.sql
-- Добавляем тестовый тип работы для PROD окружения

INSERT INTO "WorkTypes" ("Title", "PricePerHour") 
VALUES ('Тестовый тип работ test', 500.00);