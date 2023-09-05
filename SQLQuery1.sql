exec SpTax_GeneratePandL_TDS '2023-07-01','2023-08-08',1291714950
exec SpGetTDSCalculated '2023-08-08','1291714950','2023-2024'
select * from foclient where clientid in (select clientid from client (Nolock) where Rtrim(curlocation)+Rtrim(tradecode) = 'TKN060' ) 
truncate table Tax_Profit_Details_Cash_TDS
truncate table tax_Profit_Buynotfound_TDS
truncate table TDS_JournalEntry
select * from tax_Profit_Buynotfound_TDS 
select * from Tax_Profit_Details_Cash_TDS  
select * from TDS_JournalEntry
select * from TaxComputationStatement
select * from Tax_Daily_Profit_Summary

update TDS_JournalEntry set Client=1291145960 where TransId=1

EXEC SpGetDailyTransactions '2023-08-08',1291714950







