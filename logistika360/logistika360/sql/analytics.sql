-- Logistika 360 analytics (SQL Server). Parameters: @From, @To (DATE)
DECLARE @From DATE=DATEADD(DAY,-30,CAST(GETDATE() AS DATE)), @To DATE=CAST(GETDATE() AS DATE);
-- 1 Top 10 products sold in last 30 days
SELECT TOP 10 p.Sku,p.Name,SUM(oi.Quantity) Sold FROM OrderItems oi JOIN Orders o ON o.Id=oi.OrderId JOIN Products p ON p.Id=oi.ProductId
WHERE o.CreatedAt>=@From AND o.CreatedAt<DATEADD(DAY,1,@To) AND o.Status NOT IN('Draft','Cancelled') GROUP BY p.Sku,p.Name ORDER BY Sold DESC;
-- 2 Free stock per warehouse
SELECT w.Name,SUM(s.Quantity-s.ReservedQuantity) FreeStock FROM Stock s JOIN Warehouses w ON w.Id=s.WarehouseId GROUP BY w.Name;
-- 3 Order totals by status
SELECT Status,COUNT(*) Orders,SUM(TotalAmount) Total FROM Orders GROUP BY Status;
-- 4 Delayed deliveries
SELECT d.Id,d.OrderId,d.ScheduledAt,d.Status,DATEDIFF(HOUR,d.ScheduledAt,COALESCE(d.CompletedAt,GETDATE())) HoursLate FROM Deliveries d
WHERE d.ScheduledAt<COALESCE(d.CompletedAt,GETDATE()) AND d.Status<>'Failed';
-- 5 Frequent customers (HAVING)
SELECT c.Id,c.Name,COUNT(*) Orders FROM Orders o JOIN Customers c ON c.Id=o.CustomerId WHERE o.CreatedAt>=@From GROUP BY c.Id,c.Name HAVING COUNT(*)>=3 ORDER BY Orders DESC;
-- 6 Low-stock alerts (CASE)
SELECT p.Sku,s.WarehouseId,s.Quantity-s.ReservedQuantity Available,CASE WHEN s.Quantity-s.ReservedQuantity<=0 THEN 'OUT' WHEN s.Quantity-s.ReservedQuantity<=5 THEN 'LOW' ELSE 'OK' END Level
FROM Stock s JOIN Products p ON p.Id=s.ProductId WHERE s.Quantity-s.ReservedQuantity<=5;
-- 7 Daily sales with running total (CTE + window)
WITH daily AS(SELECT CAST(CreatedAt AS DATE) Day,SUM(TotalAmount) Sales FROM Orders WHERE CreatedAt>=@From AND Status NOT IN('Draft','Cancelled') GROUP BY CAST(CreatedAt AS DATE))
SELECT Day,Sales,SUM(Sales) OVER(ORDER BY Day) RunningTotal FROM daily ORDER BY Day;
-- 8 Product rank within category (window)
WITH sold AS(SELECT p.Category,p.Name,SUM(oi.Quantity) Qty FROM OrderItems oi JOIN Products p ON p.Id=oi.ProductId GROUP BY p.Category,p.Name)
SELECT Category,Name,Qty,RANK() OVER(PARTITION BY Category ORDER BY Qty DESC) Rnk FROM sold;
-- 9 On-time delivery rate
SELECT CAST(100.0*SUM(CASE WHEN CompletedAt<=ScheduledAt THEN 1 ELSE 0 END)/NULLIF(COUNT(*),0) AS DECIMAL(5,1)) OnTimeRate FROM Deliveries WHERE Status='Delivered';
-- 10 Recommended indexes
-- CREATE INDEX IX_Orders_Status_Created ON Orders(Status,CreatedAt); CREATE INDEX IX_Stock_Prod_Wh ON Stock(ProductId,WarehouseId);
