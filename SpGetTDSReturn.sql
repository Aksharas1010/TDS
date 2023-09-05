
create Procedure SpGetTDSReturn  
@fromdate VARCHAR(20),
@todate varchar(20),
@clientid int= null,
@FiscYear varchar(max)
As                                                       
begin                                                                                                                                        
  Set NoCount On   
   
--select * from Tax_Daily_Profit_Summary
--declare @FiscYear varchar(max)='2023-2024'
select Client,
sum(case when TransSaleDate between '2023-08-01' and '2023-08-08' then Adjusted_Short_Term else 0 end) as sum_Adjusted_Short_Term,
sum(case when TransSaleDate between '2023-08-01' and '2023-08-08' then Adjusted_Long_Term else 0 end) as sum_Adjusted_Long_Term 
, CASE 
  WHEN  sum(case when TransSaleDate between '2023-08-01' and '2023-08-08' then Adjusted_Long_Term else 0 end) > 5000000 
  THEN sum(case when TransSaleDate between '2023-08-01' and '2023-08-08' then Adjusted_Long_Term else 0 end) * 
  (SELECT Tax_rate/100 FROM TaxratesMaster WHERE Gain_type = 'Long Term' AND YTD_gain_range = '> 50,00,000' AND finyear = @FiscYear) 
  else 
   sum(case when TransSaleDate between '2023-08-01' and '2023-08-08' then Adjusted_Long_Term else 0 end) * 
  (SELECT Tax_rate/100 FROM TaxratesMaster WHERE Gain_type = 'Long Term' AND YTD_gain_range = '<= 50,00,000' AND finyear = @FiscYear) 
  end as LGTax 
,case 
	when sum(case when TransSaleDate between '2023-08-01' and '2023-08-08' then Adjusted_Long_Term else 0 end) > 5000000 
	THEN sum(case when TransSaleDate between '2023-08-01' and '2023-08-08' then Adjusted_Long_Term else 0 end) * 
   (SELECT Tax_rate/100 FROM TaxratesMaster WHERE Gain_type = 'Long Term' AND YTD_gain_range = '> 50,00,000' AND finyear = @FiscYear) *
  (SELECT Cess/100 FROM TaxratesMaster WHERE Gain_type = 'Long Term' AND YTD_gain_range = '> 50,00,000' AND finyear = @FiscYear)
  else
    sum(case when TransSaleDate between '2023-08-01' and '2023-08-08' then Adjusted_Long_Term else 0 end) * 
	 (SELECT Tax_rate/100 FROM TaxratesMaster WHERE Gain_type = 'Long Term' AND YTD_gain_range = '<= 50,00,000' AND finyear = @FiscYear) *
  (SELECT Cess/100 FROM TaxratesMaster WHERE Gain_type = 'Long Term' AND YTD_gain_range = '<= 50,00,000' AND finyear = @FiscYear) 
  end as LGCess
,case when sum(case when TransSaleDate between '2023-08-01' and '2023-08-08' then Adjusted_Long_Term else 0 end) > 5000000 
  THEN sum(case when TransSaleDate between '2023-08-01' and '2023-08-08' then Adjusted_Long_Term else 0 end) * 
   (SELECT Tax_rate/100 FROM TaxratesMaster WHERE Gain_type = 'Long Term' AND YTD_gain_range = '> 50,00,000' AND finyear = @FiscYear) *
  (SELECT Surcharge/100 FROM TaxratesMaster WHERE Gain_type = 'Long Term' AND YTD_gain_range = '> 50,00,000' AND finyear = @FiscYear) 
  else 
   sum(case when TransSaleDate between '2023-08-01' and '2023-08-08' then Adjusted_Long_Term else 0 end) * 
    (SELECT Tax_rate/100 FROM TaxratesMaster WHERE Gain_type = 'Long Term' AND YTD_gain_range = '<= 50,00,000' AND finyear = @FiscYear) *
  (SELECT Surcharge/100 FROM TaxratesMaster WHERE Gain_type = 'Long Term' AND YTD_gain_range = '<= 50,00,000' AND finyear = @FiscYear) 
  end as LGSurcharge 

  , CASE 
  WHEN  sum(case when TransSaleDate between '2023-08-01' and '2023-08-08' then Adjusted_Short_Term else 0 end) > 5000000 
  THEN sum(case when TransSaleDate between '2023-08-01' and '2023-08-08' then Adjusted_Short_Term else 0 end) * 
  (SELECT Tax_rate/100 FROM TaxratesMaster WHERE Gain_type = 'Short Term' AND YTD_gain_range = '> 50,00,000' AND finyear = @FiscYear) 
  else 
   sum(case when TransSaleDate between '2023-08-01' and '2023-08-08' then Adjusted_Short_Term else 0 end) * 
  (SELECT Tax_rate/100 FROM TaxratesMaster WHERE Gain_type = 'Short Term' AND YTD_gain_range = '<= 50,00,000' AND finyear = @FiscYear) 
  end as STTax 
,case 
	when sum(case when TransSaleDate between '2023-08-01' and '2023-08-08' then Adjusted_Short_Term else 0 end) > 5000000 
	THEN sum(case when TransSaleDate between '2023-08-01' and '2023-08-08' then Adjusted_Short_Term else 0 end) * 
   (SELECT Tax_rate/100 FROM TaxratesMaster WHERE Gain_type = 'Short Term' AND YTD_gain_range = '> 50,00,000' AND finyear = @FiscYear) *
  (SELECT Cess/100 FROM TaxratesMaster WHERE Gain_type = 'Short Term' AND YTD_gain_range = '> 50,00,000' AND finyear = @FiscYear)
  else
    sum(case when TransSaleDate between '2023-08-01' and '2023-08-08' then Adjusted_Short_Term else 0 end) * 
	 (SELECT Tax_rate/100 FROM TaxratesMaster WHERE Gain_type = 'Short Term' AND YTD_gain_range = '<= 50,00,000' AND finyear = @FiscYear) *
  (SELECT Cess/100 FROM TaxratesMaster WHERE Gain_type = 'Short Term' AND YTD_gain_range = '<= 50,00,000' AND finyear = @FiscYear) 
  end as STCess
,case when sum(case when TransSaleDate between '2023-08-01' and '2023-08-08' then Adjusted_Short_Term else 0 end) > 5000000 
  THEN sum(case when TransSaleDate between '2023-08-01' and '2023-08-08' then Adjusted_Short_Term else 0 end) * 
   (SELECT Tax_rate/100 FROM TaxratesMaster WHERE Gain_type = 'Short Term' AND YTD_gain_range = '> 50,00,000' AND finyear = @FiscYear) *
  (SELECT Surcharge/100 FROM TaxratesMaster WHERE Gain_type = 'Short Term' AND YTD_gain_range = '> 50,00,000' AND finyear = @FiscYear) 
  else 
   sum(case when TransSaleDate between '2023-08-01' and '2023-08-08' then Adjusted_Short_Term else 0 end) * 
    (SELECT Tax_rate/100 FROM TaxratesMaster WHERE Gain_type = 'Short Term' AND YTD_gain_range = '<= 50,00,000' AND finyear = @FiscYear) *
  (SELECT Surcharge/100 FROM TaxratesMaster WHERE Gain_type = 'Short Term' AND YTD_gain_range = '<= 50,00,000' AND finyear = @FiscYear) 
  end as STSurcharge 


from Tax_Daily_Profit_Summary
group by Client

End 
