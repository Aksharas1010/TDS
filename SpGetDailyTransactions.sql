--exec SpGetDailyTransactions '2023-08-08',1290543694
create Procedure SpGetDailyTransactions
@date date null,
@clientid int null
As                                                       
begin                                                                                                                                        
  Set NoCount On   
   -- select * from foclient where clientid in (select clientid from client (Nolock) where Rtrim(curlocation)+Rtrim(tradecode) = 'TKN060' ) 

	--select c.NAME as ClientName,c.TRADECODE as TradeCode,Rtrim(curlocation)+Rtrim(tradecode) as ClientCode,c.CLIENTID, t.BuyQty,t.BuyValue,t.TranDateBuy,t.TranDateSale,t.TransID,
	--case when t.Type='Long Term' then 'LG' else 'SG' end as Type,
	--T.SaleQty,T.SaleValue,T.ISIN,t.Security,s.PRODUCT
	--from
	--Tax_Profit_Details_Cash_TDS t

	--inner join Client c on c.CLIENTID=t.Clientid
	--inner join Sauda s on s.CLIENTID=t.Clientid and t.TranDateSale=s.TRANDATE and t.Security=s.SECURITY
	--where TranDateSale=@date and t.clientid=@clientid

	SELECT
    c.NAME as ClientName,
    c.TRADECODE as TradeCode,
    RTRIM(curlocation) + RTRIM(tradecode) as ClientCode,
    c.CLIENTID,
    t.BuyQty,
    t.BuyValue,
    t.TranDateBuy,
    t.TranDateSale,
    t.TransID,
    CASE WHEN t.Type='Long Term' THEN 'LG' ELSE 'SG' END as Type,
    T.SaleQty,
    T.SaleValue,
    T.ISIN,
    t.Security,
    MAX(s.PRODUCT) as PRODUCT -- Using MAX to pick one PRODUCT name
FROM Tax_Profit_Details_Cash_TDS t
INNER JOIN Client c ON c.CLIENTID = t.Clientid
LEFT JOIN Sauda s ON s.CLIENTID = t.Clientid AND t.TranDateSale = s.TRANDATE AND t.Security = s.SECURITY
WHERE TranDateSale = @date AND t.clientid = @clientid
GROUP BY
    c.NAME,
    c.TRADECODE,
    RTRIM(curlocation) + RTRIM(tradecode),
    c.CLIENTID,
    t.BuyQty,
    t.BuyValue,
    t.TranDateBuy,
    t.TranDateSale,
    t.TransID,
    CASE WHEN t.Type='Long Term' THEN 'LG' ELSE 'SG' END,
    T.SaleQty,
    T.SaleValue,
    T.ISIN,
    t.Security;

End 
