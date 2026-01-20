-- Insert default currencies if not exists
IF NOT EXISTS (SELECT 1 FROM Currencies WHERE Code = 'EUR')
BEGIN
    INSERT INTO Currencies ([Name], Code, Symbol, ExchangeRate, IsDefault, CreatedAt, IsDeleted)
    VALUES 
    (N'Euro', N'EUR', N'€', 1.0, 1, GETDATE(), 0),
    (N'United States Dollar', N'USD', N'$', 1.0, 0, GETDATE(), 0),
    (N'Russia Ruble', N'RUR', N'₽', 1.0, 0, GETDATE(), 0);
    
    PRINT 'Currencies inserted successfully';
END
ELSE
BEGIN
    PRINT 'Currencies already exist';
END
GO

SELECT * FROM Currencies;
GO
