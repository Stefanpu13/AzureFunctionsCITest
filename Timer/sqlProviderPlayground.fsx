#I @"C:\Users\stefan.uzunov.SCALEFOCUS\.nuget\packages\sqlprovider\1.1.28\lib\net451"
#r "FSharp.Data.SQLProvider.dll"
open FSharp.Data.Sql

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

type sql = SqlDataProvider< 
              ConnectionString = AzureConnectionString,
              DatabaseVendor = Common.DatabaseProviderTypes.MSSQLSERVER,              
              UseOptionTypes = true >

let ctx = sql.GetDataContext()

// #time
let allRecords = ctx.Dbo.Pairs |> List.ofSeq

let groupedByTimeOfRecording = 
    allRecords    
    |> Seq.groupBy (fun p -> p.Date)    


groupedByTimeOfRecording 
|> Seq.sortBy fst
|> Seq.map (snd >> Seq.length)
|> Seq.distinct
|> Seq.toList


groupedByTimeOfRecording
|> Seq.map (fun (h, ps) -> h.ToString("yyyy-MM-dd HH:mm"), Seq.length ps)
|> Seq.toList

groupedByTimeOfRecording
|> Seq.sortByDescending fst
|> Seq.map (fun (h, ps) -> h.ToString("yyyy-MM-dd HH:mm"), Seq.length ps)
|> Seq.toList

allRecords |> Seq.length