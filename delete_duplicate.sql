-- Удаление дубликата клиента с VAT ATU81077969
-- Оставляем только один (с меньшим ID)

-- Проверяем дубликаты
SELECT Id, CompanyName, VatNumber, CreatedAt 
FROM Clients 
WHERE VatNumber = 'ATU81077969' AND IsDeleted = 0
ORDER BY Id;

-- Удаляем дубликат (мягкое удаление)
-- Замените @DuplicateId на ID второго клиента из результата выше
DECLARE @DuplicateId INT;

-- Найти второй (дубликат) ID
SELECT TOP 1 @DuplicateId = Id 
FROM Clients 
WHERE VatNumber = 'ATU81077969' AND IsDeleted = 0
ORDER BY Id DESC;

-- Мягко удалить дубликат
UPDATE Clients 
SET IsDeleted = 1, UpdatedAt = GETUTCDATE()
WHERE Id = @DuplicateId;

-- Проверка результата
SELECT Id, CompanyName, VatNumber, IsDeleted 
FROM Clients 
WHERE VatNumber = 'ATU81077969';
