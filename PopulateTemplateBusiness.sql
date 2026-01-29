-- ============================================
-- Populate Template Business (BusinessId = 1)
-- with reference data for import
-- ============================================

-- Insert Currencies
INSERT OR IGNORE INTO Currencies (Id, Code, Name, Symbol, ExchangeRate, IsDefault, BusinessId, CreatedAt, UpdatedAt)
VALUES
(1, 'EUR', 'Euro', '€', 1.0, 1, 1, datetime('now'), datetime('now')),
(2, 'USD', 'US Dollar', '$', 1.1, 0, 1, datetime('now'), datetime('now')),
(3, 'CHF', 'Swiss Franc', 'CHF', 1.05, 0, 1, datetime('now'), datetime('now')),
(4, 'GBP', 'British Pound', '£', 1.17, 0, 1, datetime('now'), datetime('now')),
(5, 'RUB', 'Russian Ruble', '₽', 0.01, 0, 1, datetime('now'), datetime('now')),
(6, 'PLN', 'Polish Zloty', 'zł', 0.25, 0, 1, datetime('now'), datetime('now')),
(7, 'CZK', 'Czech Koruna', 'Kč', 0.045, 0, 1, datetime('now'), datetime('now')),
(8, 'HUF', 'Hungarian Forint', 'Ft', 0.0028, 0, 1, datetime('now'), datetime('now')),
(9, 'SEK', 'Swedish Krona', 'kr', 0.1, 0, 1, datetime('now'), datetime('now')),
(10, 'NOK', 'Norwegian Krone', 'kr', 0.095, 0, 1, datetime('now'), datetime('now')),
(11, 'DKK', 'Danish Krone', 'kr', 0.135, 0, 1, datetime('now'), datetime('now')),
(12, 'JPY', 'Japanese Yen', '¥', 0.0075, 0, 1, datetime('now'), datetime('now')),
(13, 'CNY', 'Chinese Yuan', '¥', 0.15, 0, 1, datetime('now'), datetime('now')),
(14, 'AUD', 'Australian Dollar', 'A$', 0.72, 0, 1, datetime('now'), datetime('now')),
(15, 'CAD', 'Canadian Dollar', 'C$', 0.8, 0, 1, datetime('now'), datetime('now'));

-- Insert Tax Rates
INSERT OR IGNORE INTO TaxRates (Id, Name, Rate, IsDefault, BusinessId, CreatedAt, UpdatedAt)
VALUES
(1, 'Standard VAT (AT)', 20.0, 1, 1, datetime('now'), datetime('now')),
(2, 'Reduced VAT 10% (AT)', 10.0, 0, 1, datetime('now'), datetime('now')),
(3, 'Reduced VAT 13% (AT)', 13.0, 0, 1, datetime('now'), datetime('now')),
(4, 'VAT Free Export (AT)', 0.0, 0, 1, datetime('now'), datetime('now')),
(5, 'Reverse Charge', 0.0, 0, 1, datetime('now'), datetime('now')),
(6, 'Standard VAT (DE)', 19.0, 0, 1, datetime('now'), datetime('now')),
(7, 'Reduced VAT (DE)', 7.0, 0, 1, datetime('now'), datetime('now')),
(8, 'Standard VAT (CH)', 8.1, 0, 1, datetime('now'), datetime('now')),
(9, 'Reduced VAT (CH)', 2.6, 0, 1, datetime('now'), datetime('now')),
(10, 'Standard VAT (GB)', 20.0, 0, 1, datetime('now'), datetime('now')),
(11, 'Reduced VAT (GB)', 5.0, 0, 1, datetime('now'), datetime('now'));

-- Insert Client Areas
INSERT OR IGNORE INTO ClientAreas (Id, Code, Name, Description, BusinessId, CreatedAt, UpdatedAt)
VALUES
(1, 'INLAND', 'Inland - Austria', 'Customers within Austria', 1, datetime('now'), datetime('now')),
(2, 'EU', 'European Union', 'EU customers with valid VAT ID', 1, datetime('now'), datetime('now')),
(3, 'EXPORT', 'Third Countries (Export)', 'Non-EU export customers', 1, datetime('now'), datetime('now')),
(4, 'DE', 'Germany', 'German customers', 1, datetime('now'), datetime('now')),
(5, 'CH', 'Switzerland', 'Swiss customers', 1, datetime('now'), datetime('now')),
(6, 'GB', 'Great Britain', 'UK customers', 1, datetime('now'), datetime('now')),
(7, 'BENELUX', 'Benelux', 'Belgium, Netherlands, Luxembourg', 1, datetime('now'), datetime('now')),
(8, 'NORDIC', 'Nordic Countries', 'Scandinavia region', 1, datetime('now'), datetime('now')),
(9, 'EASTEU', 'Eastern Europe', 'Poland, Czech Republic, Hungary, etc.', 1, datetime('now'), datetime('now'));

-- Insert Client Types
INSERT OR IGNORE INTO ClientTypes (Id, Code, Name, Description, BusinessId, CreatedAt, UpdatedAt)
VALUES
(1, 'B2B', 'Business to Business', 'Corporate customers', 1, datetime('now'), datetime('now')),
(2, 'B2C', 'Business to Consumer', 'Individual consumers', 1, datetime('now'), datetime('now')),
(3, 'GOV', 'Government', 'Government agencies and institutions', 1, datetime('now'), datetime('now')),
(4, 'NGO', 'Non-Profit Organization', 'Non-profit and charity organizations', 1, datetime('now'), datetime('now')),
(5, 'RETAIL', 'Retail Customer', 'Walk-in retail customers', 1, datetime('now'), datetime('now')),
(6, 'WHSALE', 'Wholesale Customer', 'Wholesale buyers', 1, datetime('now'), datetime('now')),
(7, 'RESELLER', 'Reseller', 'Authorized resellers', 1, datetime('now'), datetime('now'));

-- Insert Units of Measure
INSERT OR IGNORE INTO Units (Id, ShortName, Name, BusinessId, CreatedAt, UpdatedAt)
VALUES
(1, 'pcs', 'Pieces', 1, datetime('now'), datetime('now')),
(2, 'kg', 'KG', 1, datetime('now'), datetime('now')),
(3, 'Karton', 'Karton', 1, datetime('now'), datetime('now')),
(4, 'l', 'Liter', 1, datetime('now'), datetime('now')),
(5, 'm', 'Meter', 1, datetime('now'), datetime('now')),
(6, 'Pauschal', 'Pauschalbetrag', 1, datetime('now'), datetime('now')),
(7, 'Stk', 'Stück', 1, datetime('now'), datetime('now')),
(8, 't', 'Tonne', 1, datetime('now'), datetime('now')),
(9, 'm²', 'Square Meter', 1, datetime('now'), datetime('now')),
(10, 'm³', 'Cubic Meter', 1, datetime('now'), datetime('now')),
(11, 'h', 'Hour', 1, datetime('now'), datetime('now')),
(12, 'day', 'Day', 1, datetime('now'), datetime('now')),
(13, 'box', 'Box', 1, datetime('now'), datetime('now')),
(14, 'pal', 'Pallet', 1, datetime('now'), datetime('now')),
(15, 'set', 'Set', 1, datetime('now'), datetime('now'));

-- Insert Payment Methods
INSERT OR IGNORE INTO PaymentMethods (Id, Name, BusinessId, CreatedAt, UpdatedAt)
VALUES
(1, 'Bank Transfer', 1, datetime('now'), datetime('now')),
(2, 'Cash', 1, datetime('now'), datetime('now')),
(3, 'Credit Card', 1, datetime('now'), datetime('now')),
(4, 'Debit Card', 1, datetime('now'), datetime('now')),
(5, 'PayPal', 1, datetime('now'), datetime('now')),
(6, 'Direct Debit (SEPA)', 1, datetime('now'), datetime('now')),
(7, 'Check', 1, datetime('now'), datetime('now')),
(8, 'Invoice (Net 30)', 1, datetime('now'), datetime('now')),
(9, 'Invoice (Net 14)', 1, datetime('now'), datetime('now')),
(10, 'Prepayment', 1, datetime('now'), datetime('now')),
(11, 'Cash on Delivery', 1, datetime('now'), datetime('now'));

-- Insert Chart of Accounts
INSERT OR IGNORE INTO Accounts (Id, AccountNumber, Name, AccountCode, BusinessId, CreatedAt, UpdatedAt)
VALUES
(1, '4000', 'Erlöse 20% USt', 'REVENUE_20', 1, datetime('now'), datetime('now')),
(2, '4010', 'Erlöse 10% USt', 'REVENUE_10', 1, datetime('now'), datetime('now')),
(3, '4030', 'Erlöse 13% USt', 'REVENUE_13', 1, datetime('now'), datetime('now')),
(4, '4100', 'Erlöse steuerfrei (Export)', 'REVENUE_EXPORT', 1, datetime('now'), datetime('now')),
(5, '4112', 'Erlöse 13% USt Inland', 'REVENUE_13_DOM', 1, datetime('now'), datetime('now')),
(6, '4662', 'Zinsertrag', 'INTEREST_INC', 1, datetime('now'), datetime('now')),
(7, '5000', 'Aufwendungen für Material', 'COGS_MATERIAL', 1, datetime('now'), datetime('now')),
(8, '5100', 'Aufwendungen für Waren', 'COGS_GOODS', 1, datetime('now'), datetime('now')),
(9, '5700', 'Personalaufwand', 'PERSONNEL_EXP', 1, datetime('now'), datetime('now')),
(10, '6000', 'Raumkosten', 'RENT_EXP', 1, datetime('now'), datetime('now')),
(11, '6500', 'KFZ-Kosten', 'VEHICLE_EXP', 1, datetime('now'), datetime('now')),
(12, '7000', 'Werbekosten', 'MARKETING_EXP', 1, datetime('now'), datetime('now')),
(13, '7300', 'Reisekosten', 'TRAVEL_EXP', 1, datetime('now'), datetime('now')),
(14, '7500', 'Bürokosten', 'OFFICE_EXP', 1, datetime('now'), datetime('now')),
(15, '8000', 'Abschreibungen', 'DEPRECIATION', 1, datetime('now'), datetime('now'));

SELECT 'Template Business Data Populated Successfully!' as Result;
SELECT COUNT(*) as Currencies FROM Currencies WHERE BusinessId = 1;
SELECT COUNT(*) as TaxRates FROM TaxRates WHERE BusinessId = 1;
SELECT COUNT(*) as ClientAreas FROM ClientAreas WHERE BusinessId = 1;
SELECT COUNT(*) as ClientTypes FROM ClientTypes WHERE BusinessId = 1;
SELECT COUNT(*) as Units FROM Units WHERE BusinessId = 1;
SELECT COUNT(*) as PaymentMethods FROM PaymentMethods WHERE BusinessId = 1;
SELECT COUNT(*) as Accounts FROM Accounts WHERE BusinessId = 1;
