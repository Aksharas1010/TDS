alter Procedure SpGetTDSCalculated  
 @to_date DATE ,    
 @clientid int =null,  
 @FiscYear varchar(12)  
As                                                           
begin  

 CREATE TABLE [#TDS_SaleTrans]                  
 (                                                              
  [clientid] [int] NOT NULL,                                    
  [security] [char](10) NOT NULL   ,    
  [Type] [varchar](20) NOT NULL     
 )         
 CREATE TABLE Client_Profit_Sums (  
    Client VARCHAR(50),  
	TransSaleDate [datetime] NULL,     
    Sum_Short_Term_Profit DECIMAL(10, 2),  
    Sum_Long_Term_Profit DECIMAL(10, 2),  
	DailySetOffST DECIMAL(10,2),
	DailySetOffLT Decimal(10,2),
	Profit DECIMAL(10, 2),  
	OpeningBalST Decimal(10,2),  
	OpeningBalLT Decimal(10,2),
	Adjusted_Short_Term DECIMAL(10, 2),  
    Adjusted_Long_Term DECIMAL(10, 2),
		ClosingBalST Decimal(10,2),  
	ClosingBalLT Decimal(10,2),
	TaxableGain DECIMAL(10, 2),  
	ST_Tax DECIMAL(10, 2),  
	LT_Tax DECIMAL(10, 2),   
	ST_TaxPercentage DECIMAL(10, 2),  
	LT_TaxPercentage DECIMAL(10, 2) ,

);   

 DECLARE @input_date DATE = @to_date    
 DECLARE @quarter int = (DATEPART(QUARTER, @input_date));    
 DECLARE @quarter_start_date DATE = CAST(CAST(YEAR(@input_date) AS VARCHAR(4)) + '-' + CAST((3 * @quarter - 2) AS VARCHAR(2)) + '-01' AS DATE);    
 DECLARE @quarter_end_date DATE = DATEADD(MONTH, 3,cast(@quarter_start_date as varchar(50))) - 1;    
 DECLARE @month1_start_date DATE = @quarter_start_date;    
 DECLARE @month1_end_date DATE = DATEADD(MONTH, 1, cast(@quarter_start_date as varchar(50))) - 1;    
 DECLARE @month2_start_date DATE = DATEADD(MONTH, 1, cast(@quarter_start_date as varchar(50)));    
 DECLARE @month2_end_date DATE = DATEADD(MONTH, 2, cast(@quarter_start_date as varchar(50))) - 1;    
 DECLARE @month3_start_date DATE = DATEADD(MONTH, 2, cast(@quarter_start_date as varchar(50)));    
 DECLARE @month3_end_date DATE = @quarter_end_date; 
 INSERT INTO Client_Profit_Sums (Client,TransSaleDate, Sum_Short_Term_Profit, Sum_Long_Term_Profit)  
 SELECT clientid,TranDateSale,  
 SUM(CASE WHEN Type = 'Short Term' THEN (Profit) ELSE 0 END) AS Sum_Short_Term_Profit,  
 SUM(CASE WHEN Type = 'Long Term' THEN (Profit) ELSE 0 END) AS Sum_Long_Term_Profit  
 FROM Tax_Profit_Details_Cash_TDS 
 WHERE
 TranDateSale >= @quarter_start_date and TranDateSale <= @to_date
 and clientid=@clientid   
 GROUP BY clientid,TranDateSale order by TranDateSale ;

DECLARE @Client VARCHAR(50);  
DECLARE @TransSaleDate DATETIME;  
DECLARE @Sum_Short_Term_Profit DECIMAL(10, 2);  
DECLARE @Sum_Long_Term_Profit DECIMAL(10, 2);  
DECLARE @ClosingBal_ST DECIMAL(10, 2)=0;  
DECLARE @ClosingBal_LT DECIMAL(10, 2)=0; 
DECLARE @DailySetOffLT decimal(10,2);
DECLARE @DailySetOffST decimal(10,2);
DECLARE @PrevOpeningBal DECIMAL(10, 2) = 0;  -- Initialize previous day's opening balance  
DECLARE @PrevAdjusted_Long_Term DECIMAL(10, 2) = 0; -- Initialize previous day's adjusted long-term value  
DECLARE @PrevAdjusted_Short_Term DECIMAL(10, 2) = 0; -- Initialize previous day's adjusted long-term value  
DECLARE @OpeningBalST DECIMAL(10, 2)  
DECLARE @OpeningBalLT DECIMAL(10, 2)
declare @previoustrandate date
DECLARE profit_cursor CURSOR FOR  
SELECT  Client, TransSaleDate,  Sum_Short_Term_Profit,  Sum_Long_Term_Profit FROM  Client_Profit_Sums  where client=@clientid  order by TransSaleDate ---- for specific client  
OPEN profit_cursor;    
FETCH NEXT FROM profit_cursor INTO @Client, @TransSaleDate, @Sum_Short_Term_Profit, @Sum_Long_Term_Profit;   
WHILE @@FETCH_STATUS = 0  
BEGIN  
  ----------------------------------Opening Bal---------------------------------------------------------------------
 print @previoustrandate
 set @OpeningBalST  =  CASE
        WHEN MONTH(@previoustrandate) <> MONTH(@TransSaleDate) THEN
            CASE
                WHEN @ClosingBal_ST < 0 THEN @ClosingBal_ST 
                ELSE 0
            END
        ELSE @ClosingBal_ST
    END
 set @OpeningBalLT  =  CASE
        WHEN MONTH(@previoustrandate) <> MONTH(@TransSaleDate) THEN
            CASE
                WHEN @ClosingBal_LT < 0 THEN @ClosingBal_LT 
                ELSE 0
            END
        ELSE @ClosingBal_LT
    END

  --------------------------------------BuyNotFound--------------------------------------------------------------------------
   declare @BuyNotFound decimal(10,2)
   select @BuyNotFound=Sum(SellValue) from Tax_Profit_Buynotfound_TDS where Clientid=@Client and Trandate=@TransSaleDate
   if @BuyNotFound>0
   begin
   set @Sum_Short_Term_Profit=@Sum_Short_Term_Profit+@BuyNotFound;
   update Client_Profit_Sums set Sum_Short_Term_Profit=@Sum_Short_Term_Profit where Client=@Client  and TransSaleDate=@TransSaleDate
   end
  ------------------------------------ Daily AdjustMent------------------------------------------------------------------------------------------------ 
  
  DECLARE @Adjusted_Short_Term DECIMAL(10, 2) = CASE  
  WHEN @Sum_Short_Term_Profit < 0 and @Sum_Long_Term_Profit >0 THEN  
  CASE  
  WHEN (@Sum_Long_Term_Profit - ABS(@Sum_Short_Term_Profit)) > 0 THEN 0  
  ELSE @Sum_Long_Term_Profit -ABS(@Sum_Short_Term_Profit)  
  END  
  ELSE   
  @Sum_Short_Term_Profit  
  END;   
  DECLARE @Adjusted_Long_Term DECIMAL(10, 2) = CASE  
  WHEN @Sum_Short_Term_Profit < 0 and @Sum_Long_Term_Profit >0 THEN   
  CASE  
  WHEN (@Sum_Long_Term_Profit - ABS(@Sum_Short_Term_Profit)) < 0 THEN 0  
  ELSE @Sum_Long_Term_Profit - ABS(@Sum_Short_Term_Profit)  
  END  
  ELSE   
  @Sum_Long_Term_Profit  
  END;  
  set @DailySetOffLT=@Adjusted_Long_Term;
  set @DailySetOffST=@Adjusted_Short_Term;
  -----------------------------------------Set Offf------------------------------- -------------------------------------------------------------------------------------------------- 
  DECLARE @Profit DECIMAL(10, 2) = @Sum_Short_Term_Profit + @Sum_Long_Term_Profit;  

  DECLARE @Difference DECIMAL(10, 2)  
  
  -- Check for specific condition between previous day's and today's Adjusted_Long_Term values  
  IF @OpeningBalLT >= 0 AND @Adjusted_Long_Term < 0   
  BEGIN  
   set @Difference = @Adjusted_Long_Term + @OpeningBalLT;  
   IF @Difference >= 0  
   BEGIN  
    SET @Adjusted_Long_Term = 0;  
    if @Adjusted_Short_Term<0 and @Adjusted_Short_Term+@OpeningBalST < 0  
    begin  
		declare @shorttermdiff decimal(10,2)=@Adjusted_Short_Term+@OpeningBalST
		set @Adjusted_Short_Term=case when @Difference+@shorttermdiff<0 then @Difference+@shorttermdiff else 0 end
		Set @Adjusted_Long_Term=case when @Difference+@shorttermdiff>0 then @Difference+@shorttermdiff else 0 end
    end 
	else if @Adjusted_Short_Term<0 and @Adjusted_Short_Term+@OpeningBalST > 0 
	begin
		declare @shorttermdiff2 decimal(10,2)=@Adjusted_Short_Term+@OpeningBalST
		set @Adjusted_Short_Term=@shorttermdiff2;
		set @OpeningBalST=0
	 end
   END  
   ELSE  
   BEGIN     
    SET @Adjusted_Long_Term = @Difference;  
	set @ClosingBal_LT=@Adjusted_Long_Term
   END;  
  END;  
  IF @OpeningBalLT < 0 AND @Adjusted_Long_Term > 0 or @OpeningBalLT < 0 AND @Adjusted_Long_Term < 0  
  BEGIN  
    set @Difference  = @Adjusted_Long_Term + @PrevAdjusted_Long_Term;  
    SET @Adjusted_Long_Term = @Difference;   
	set @ClosingBal_LT=@Adjusted_Long_Term
  END;  
  
  IF @OpeningBalST < 0 AND @Adjusted_Short_Term > 0 or @OpeningBalST < 0 AND @Adjusted_Short_Term < 0 or @OpeningBalST >= 0 AND @Adjusted_Short_Term < 0   
  BEGIN 
  print 'inside'
  print @Adjusted_Short_Term
    set @Difference  = @Adjusted_Short_Term + @OpeningBalST;  
    SET @Adjusted_Short_Term = @Difference;   
	--print 'Before settung'
	--print @Adjusted_Short_Term
	set @ClosingBal_ST=@Adjusted_Short_Term
	--print 'After settung'
	--print @ClosingBal_ST  
	END;  
  
  if @OpeningBalLT>=0 and @Adjusted_Long_Term>=0 and @DailySetOffLT>0
  begin 
    set @ClosingBal_LT=@OpeningBalLT+@Adjusted_Long_Term
  end
    if @OpeningBalST>=0 and @Adjusted_Short_Term>=0 and @DailySetOffST>0
  begin 
    set @ClosingBal_ST=@OpeningBalST+@Adjusted_Short_Term
  end
   
  --print 'closing bal'
  --print @ClosingBal_LT
  --print @ClosingBal_ST
   ----------------------- Update the table with adjusted values-------------------------------------------------------------  
   UPDATE Client_Profit_Sums  
        SET Adjusted_Short_Term = @Adjusted_Short_Term,  
            Adjusted_Long_Term = @Adjusted_Long_Term,  
			DailySetOffLT=@DailySetOffLT,
			DailySetOffST=@DailySetOffST,
			
   Profit = @Profit,  
   OpeningBalST  = @OpeningBalST,
   OpeningBalLT=@OpeningBalLT,
		 ClosingBalLT=@ClosingBal_LT ,
		  ClosingBalST=@ClosingBal_ST ,

   TaxableGain=@Adjusted_Short_Term+@Adjusted_Long_Term,  
   ST_Tax=case when @Adjusted_Short_Term>0 then   
   CASE  
   WHEN  @Adjusted_Short_Term > 5000000 THEN     
     @Adjusted_Short_Term * (  
     SELECT Total_tax_rate / 100 FROM TaxratesMaster  
     WHERE Gain_type = 'Short Term' AND YTD_gain_range = '> 50,00,000' AND finyear = @FiscYear  
                )  
   WHEN  @Adjusted_Short_Term <= 5000000 THEN   
    @Adjusted_Short_Term * (  
     SELECT Total_tax_rate / 100 FROM TaxratesMaster  
     WHERE Gain_type = 'Short Term' AND YTD_gain_range = '<= 50,00,000' AND finyear = @FiscYear  
                )  
   end  
   else 
   0
   end,  
   LT_Tax=case when @Adjusted_Long_Term>0 then   
   CASE  
   WHEN  @Adjusted_Long_Term > 5000000 THEN  
   @Adjusted_Long_Term * (  
     SELECT Total_tax_rate / 100 FROM TaxratesMaster  
     WHERE Gain_type = 'Long Term' AND YTD_gain_range = '> 50,00,000' AND finyear = @FiscYear  
                )  
   WHEN  @Adjusted_Long_Term <= 5000000 THEN   
   @Adjusted_Long_Term * (  
     SELECT Total_tax_rate / 100 FROM TaxratesMaster  
     WHERE Gain_type = 'Long Term' AND YTD_gain_range = '<= 50,00,000' AND finyear = @FiscYear  
                )  
   end  
   else 0  
   end,  
   ST_TaxPercentage=case when @Adjusted_Short_Term>0 then   
   CASE  
   WHEN  @Adjusted_Short_Term > 5000000 THEN   
    (  
     SELECT Total_tax_rate FROM TaxratesMaster  
     WHERE Gain_type = 'Short Term' AND YTD_gain_range = '> 50,00,000' AND finyear = @FiscYear  
                )  
   WHEN  @Adjusted_Short_Term <= 5000000 THEN    
      (  
     SELECT Total_tax_rate FROM TaxratesMaster  
     WHERE Gain_type = 'Short Term' AND YTD_gain_range = '<= 50,00,000' AND finyear = @FiscYear  
                )  
   end  
   else 0  
   end,  
   LT_TaxPercentage=case when @Adjusted_Long_Term>0 then   
   CASE  
   WHEN  @Adjusted_Long_Term > 5000000 THEN   
    (  
     SELECT Total_tax_rate FROM TaxratesMaster  
     WHERE Gain_type = 'Long Term' AND YTD_gain_range = '> 50,00,000' AND finyear = @FiscYear  
                )  
   WHEN  @Adjusted_Long_Term <= 5000000 THEN   
    (  
     SELECT Total_tax_rate FROM TaxratesMaster  
     WHERE Gain_type = 'Long Term' AND YTD_gain_range = '<= 50,00,000' AND finyear = @FiscYear  
        )  
   end  
   else 0  
   end  
        WHERE  
  --Client = @Client   
  Client=@clientid -- for specific client  
  AND TransSaleDate = @TransSaleDate;  
  ------------end------------------------------------------------------------------------------------------------
  
  SET @PrevAdjusted_Long_Term = @Adjusted_Long_Term; -- Update previous day's adjusted long-term value  
  SET @PrevAdjusted_Short_Term = @Adjusted_Short_Term; 
  set @previoustrandate=@TransSaleDate


  	---------------------------------------Reverse entry------------------------------------------------------------------------------------
	print 'reverse'
    declare @PreviousDate datetime;
	declare @Sum_Short_Term_Profit_today decimal(10,2),@Sum_Long_Term_Profit_today decimal(10,2),@Previous_OpeningST decimal(10,2),
	@Previous_OpeningLT decimal(10,2)
	SELECT @Sum_Short_Term_Profit_today=DailySetOffST,@Sum_Long_Term_Profit_today=DailySetOffLT FROM Client_Profit_Sums WHERE TransSaleDate = @TransSaleDate;
	set @PreviousDate=DATEADD(DAY, -1, @to_date) 
	SELECT @Previous_OpeningST=OpeningBalST,@Previous_OpeningLT=OpeningBalLT 
	FROM Client_Profit_Sums
	WHERE TransSaleDate = @TransSaleDate;
	print 'long'
	print @Previous_OpeningLT
	print @Sum_Long_Term_Profit_today
	print 'short'
	print @Previous_OpeningST
	print @Sum_Short_Term_Profit_today
	if(@Previous_OpeningLT>0 and @Sum_Long_Term_Profit_today<0)
	begin
		declare @totalLT_tax decimal(10,2);
		print 'reverse entry'
			select @totalLT_tax=case when @OpeningBalLT>0 then  
			CASE  WHEN  @OpeningBalLT > 5000000 THEN  @OpeningBalLT * (SELECT Total_tax_rate / 100 FROM TaxratesMaster  WHERE Gain_type = 'Long Term' AND YTD_gain_range = '> 50,00,000' AND finyear = @FiscYear)  
			WHEN  @OpeningBalLT <= 5000000 THEN @OpeningBalLT * (SELECT Total_tax_rate / 100 FROM TaxratesMaster WHERE Gain_type = 'Long Term' AND YTD_gain_range = '<= 50,00,000' AND finyear = @FiscYear  )  
			end  else 0  end
			UPDATE Client_Profit_Sums
			SET ST_Tax = -ABS((@totalLT_tax-(SELECT ST_Tax FROM Client_Profit_Sums WHERE TransSaleDate = @TransSaleDate)))
			WHERE TransSaleDate = @TransSaleDate;
	end
	if(@Previous_OpeningST>0 and @Sum_Short_Term_Profit_today<0)
	 begin
	 print 'reverse entry short'
			declare @totalST_tax decimal(10,2);
			select @totalST_tax=case when @OpeningBalST>0 then   
			   CASE  WHEN  @OpeningBalST > 5000000 THEN  @OpeningBalST * (SELECT Total_tax_rate / 100 FROM TaxratesMaster WHERE Gain_type = 'Short Term' AND YTD_gain_range = '> 50,00,000' AND finyear = @FiscYear)
			   WHEN  @OpeningBalST <= 5000000 THEN @OpeningBalST * ( SELECT Total_tax_rate / 100 FROM TaxratesMaster  WHERE Gain_type = 'Short Term' AND YTD_gain_range = '<= 50,00,000' AND finyear = @FiscYear)  
			   end else  0 end
			   print @totalSt_tax
			UPDATE Client_Profit_Sums
			SET ST_Tax =-((@totalST_tax-(SELECT ST_Tax FROM Client_Profit_Sums WHERE TransSaleDate = @TransSaleDate)))
			WHERE TransSaleDate = @TransSaleDate;
	 end
	--if(select SUM(ST_Tax+LT_Tax+@BuyNotFoundTax) from Client_Profit_Sums where TransSaleDate=@to_date)>0
	
	--------------------------------------end----------------------------------------------------------------------------------------------------

  FETCH NEXT FROM profit_cursor INTO @Client, @TransSaleDate, @Sum_Short_Term_Profit, @Sum_Long_Term_Profit;  
 END;  
  
 CLOSE profit_cursor;  
 DEALLOCATE profit_cursor;  


 --------------------Journal Entry----------------------------------------------------------------------------------------------------
 if 
	exists(SELECT 1 FROM Client_Profit_Sums WHERE TransSaleDate = @to_date AND ST_Tax > 0 AND LT_Tax > 0)
	begin
	INSERT INTO TDS_JournalEntry (Client, ClientCode, TransSaleDate, TotalTax, Description)  
	SELECT  
    t.Client,  
    RTRIM(c.curlocation) + RTRIM(c.tradecode),  
    t.TransSaleDate,  
    SUM(t.ST_Tax+t.LT_Tax),  
    CASE  
        WHEN CAST( t.ST_TaxPercentage AS INT) > 0 AND CAST( t.LT_TaxPercentage AS INT) > 0 THEN  
            'on Long term capital gain tax ' + CONVERT(VARCHAR(10), t.LT_TaxPercentage) +  
            ' and on Short term capital gain tax ' + CONVERT(VARCHAR(10), t.ST_TaxPercentage)  
        WHEN CAST( t.LT_TaxPercentage AS INT) > 0 THEN  
            'on Long term capital gain tax ' + CONVERT(VARCHAR(10), t.LT_TaxPercentage)  
        WHEN CAST( t.ST_TaxPercentage AS INT) > 0 THEN  
            'on Short term capital gain tax ' + CONVERT(VARCHAR(10), t.ST_TaxPercentage)  
        ELSE  
            NULL  
    END AS Description  
FROM  
    Client c  
JOIN  
    Client_Profit_Sums t ON c.ClientID = t.Client  
WHERE  
    t.Client = @clientid  
    AND t.TransSaleDate = @to_date  
GROUP BY  
    t.CLIENT, c.CURLOCATION, c.TRADECODE, t.TransSaleDate, t.ST_TaxPercentage, t.LT_TaxPercentage;  
	end   
 if exists(SELECT 1 FROM Client_Profit_Sums WHERE TransSaleDate = @to_date AND ST_Tax < 0 AND LT_Tax >= 0) or
	exists(SELECT 1 FROM Client_Profit_Sums WHERE TransSaleDate = @to_date AND ST_Tax >= 0 AND LT_Tax < 0)
	
	begin
	if(select ST_Tax from Client_Profit_Sums WHERE TransSaleDate = @to_date)!=0
	begin 
	INSERT INTO TDS_JournalEntry (Client, ClientCode, TransSaleDate, TotalTax, Description)  
	SELECT  
    t.Client,  
    RTRIM(c.curlocation) + RTRIM(c.tradecode),  
    t.TransSaleDate,  
    SUM(t.ST_Tax),  
    'on Short term capital gain tax ' + CONVERT(VARCHAR(10), t.ST_TaxPercentage)  
	FROM  
    Client c  
	JOIN  
    Client_Profit_Sums t ON c.ClientID = t.Client  
	WHERE  
    t.Client = @clientid  
    AND t.TransSaleDate= @to_date  
	GROUP BY  
    t.CLIENT, c.CURLOCATION, c.TRADECODE, t.TransSaleDate, t.ST_TaxPercentage, t.LT_TaxPercentage; 
	end
	if(select LT_Tax from Client_Profit_Sums WHERE TransSaleDate = @to_date)!=0
	begin 
	INSERT INTO TDS_JournalEntry (Client, ClientCode, TransSaleDate, TotalTax, Description)  
	SELECT  
    t.Client,  
    RTRIM(c.curlocation) + RTRIM(c.tradecode),  
    t.TransSaleDate,  
    SUM(t.LT_Tax),  
    'on Long term capital gain tax ' + CONVERT(VARCHAR(10), t.LT_TaxPercentage)  
	FROM  
    Client c  
	JOIN  
    Client_Profit_Sums t ON c.ClientID = t.Client  
	WHERE  
    t.Client = @clientid  
    AND t.TransSaleDate = @to_date  
	GROUP BY  
    t.CLIENT, c.CURLOCATION, c.TRADECODE, t.TransSaleDate, t.ST_TaxPercentage, t.LT_TaxPercentage;
	end
	end
	
 --------------------------end-----------------------------------------------------------------------------------------------------
select * from Client_Profit_Sums where TransSaleDate between @to_date and @to_date  

delete from Tax_Daily_Profit_Summary
--insert into Tax_Daily_Profit_Summary(Client,TransSaleDate,
--Sum_Short_Term_Profit,Sum_Long_Term_Profit,DailySetOffLT ,DailySetOffST,Profit,OpeningBalLT,OpeningBalST,Adjusted_Short_Term,Adjusted_Long_Term,
--TaxableGain,ST_Tax,LT_Tax,ST_TaxPercentage,LT_TaxPercentage)
-- select  Client,TransSaleDate,
--Sum_Short_Term_Profit,Sum_Long_Term_Profit,DailySetOffLT ,DailySetOffST,Profit,OpeningBalLT,OpeningBalST,Adjusted_Short_Term,Adjusted_Long_Term,
--TaxableGain,ST_Tax,LT_Tax,ST_TaxPercentage,LT_TaxPercentage from Client_Profit_Sums where TransSaleDate= @to_date
 

 insert into Tax_Daily_Profit_Summary(Client,TransSaleDate,
Sum_Short_Term_Profit,Sum_Long_Term_Profit,DailySetOffLT ,DailySetOffST,Profit,OpeningBalLT,OpeningBalST,Adjusted_Short_Term,Adjusted_Long_Term,
TaxableGain,ST_Tax,LT_Tax,ST_TaxPercentage,LT_TaxPercentage,ClosingBalLT,ClosingBalST)
 select  Client,TransSaleDate,
Sum_Short_Term_Profit,Sum_Long_Term_Profit,DailySetOffLT ,DailySetOffST,Profit,OpeningBalLT,OpeningBalST,Adjusted_Short_Term,Adjusted_Long_Term,
TaxableGain,ST_Tax,LT_Tax,ST_TaxPercentage,LT_TaxPercentage,ClosingBalLT,ClosingBalST from Client_Profit_Sums
 
    
 drop table Client_Profit_Sums  
end