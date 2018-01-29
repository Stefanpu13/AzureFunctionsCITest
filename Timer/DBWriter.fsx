
#if INTERACTIVE
#I @"C:\Users\stefan.uzunov.SCALEFOCUS\.nuget\packages\dapper\1.50.4\lib\net451\"

#r "Dapper.dll"
#endif

#load "types.fsx"
#load "utils.fsx"

open Utils
open Types

open System
open System.Data.SqlClient
open Dapper
open System.Collections.Generic

module DB = 


    [<Literal>]    
    let AzureConnectionString = @"
    Server=tcp:cryptospu.database.windows.net,1433;
    Initial Catalog=Crypto;Persist Security Info=False;
    User ID=crypto_db;Password=Stefan@2;
    MultipleActiveResultSets=False;
    Encrypt=True;
    TrustServerCertificate=False;
    Connection Timeout=30;
    "

    let insertPairCmd = @"
    INSERT INTO dbo.Pairs (Date, PriceUSD, Exchange,Code, Volume)
    "

    let MAX_SQL_INSERTS_COUNT = 1000
    let singleQoutes s = "'" + s + "'"

    let toValuePart p = 
        "(" + 
        (
            join [
                singleQoutes (p.Date.ToString("yyyy-MM-dd HH:mm:ss"))
                p.PriceUSD.ToString()
                singleQoutes p.Exchange
                singleQoutes p.Code
                p.Volume.ToString() 
            ] ","
        ) + 
        ")"

    let toPairDb p =          
        {
            PriceUSD = p.pairPrice.price
            Exchange = p.exchangeName
            Date = DateTime.UtcNow
            Code = p.baseCurrency.ToLower() + "/" + p.quoteCurrency.ToLower()
            Volume = p.volume.volume
        }

    let writeToAzureDb (pairsInExchanges: PairDB list ) =
        let batches = 
            pairsInExchanges            
            |> Seq.map toValuePart            
            |> Seq.chunkBySize MAX_SQL_INSERTS_COUNT            

        let conn = new SqlConnection(AzureConnectionString)   
        conn.Open()
        use trans = conn.BeginTransaction()       
          
        let affectedRows =       
            Seq.fold (fun affectedRows valuesBatch ->
                let batchAffectedRows = 
                    conn.Execute(
                        insertPairCmd + " Values " + join valuesBatch ",", 
                        transaction = trans
                    )
                
                affectedRows + batchAffectedRows
            ) 0 batches
        
        trans.Commit();

        affectedRows