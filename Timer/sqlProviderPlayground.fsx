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

let allRecords = ctx.Dbo.Pairs |> List.ofSeq

let groupedByTimeOfRecording = 
    allRecords
    |> Seq.sortBy (fun p -> p.Date)
    |> Seq.groupBy (fun p -> p.Date.ToString("yyyy-MM-dd HH:mm"))


groupedByTimeOfRecording
|> Seq.map (snd >> Seq.length)
|> Seq.distinct
|> Seq.toList


groupedByTimeOfRecording
|> Seq.map (fun (h, ps) -> h, Seq.length ps)
|> Seq.toList
