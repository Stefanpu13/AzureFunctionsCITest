#if INTERACTIVE

// Scalefocus computer
#I @"C:/Users/stefan.uzunov.SCALEFOCUS/AppData/Roaming/nvm/v8.7.0/node_modules/azure-functions-core-tools/bin/"
#I @"C:/Users/stefan.uzunov.SCALEFOCUS/.nuget/packages/fsharp.data/2.4.4/lib/net45/"
#I @"C:\Users\stefan.uzunov.SCALEFOCUS\.nuget\packages\dapper\1.50.4\lib\net451\"

#r "Microsoft.Azure.Webjobs.Host.dll" // Tracer
#r "Microsoft.Azure.WebJobs.Extensions.dll" // timer

#r "FSharp.Data.dll"
#r "FSharp.Data.DesignTime.dll"
#r "Dapper.dll"

open System
open Microsoft.Azure.WebJobs.Host
open Microsoft.Azure.WebJobs
#endif

#load "parsers.fsx"
#load "DBWriter.fsx"

open Parsers 
open DBWriter

let AzureConnectionString = @"
    Server=tcp:cryptospu.database.windows.net,1433;
    Initial Catalog=Crypto;Persist Security Info=False;
    User ID=crypto_db;Password=Stefan@2;
    MultipleActiveResultSets=False;
    Encrypt=True;
    TrustServerCertificate=False;
    Connection Timeout=30;
"

type Log (level) =
    inherit TraceWriter(level:System.Diagnostics.TraceLevel)
    new () = Log(Diagnostics.TraceLevel.Verbose)
    override this.Trace(event) = printfn "%A" event

let measureTime m f (log: TraceWriter) = 
    let timer = System.Diagnostics.Stopwatch()
    timer.Start()
    let res =  f()
    log.Info (sprintf "Elapsed Time during %s: %i seconds" m (timer.ElapsedMilliseconds / 1000L))
    res

let Run(myTimer: TimerInfo, log: TraceWriter) =
    let getPairsFn () = 
        CoinMarketCap.coinsPerExchanges (
            CoinMarketCap.getTop25Exchanges ()
            // ["binance"]
            )
    let pairsToWrite =  
        measureTime  "getting pairs" getPairsFn log
        |> Seq.map snd
        |> Seq.collect id
        |> Seq.map DB.toPairDb
        |> List.ofSeq
    
    let recordsAffected = 
        measureTime 
            "writing to db" 
            (fun _ -> DB.writeToAzureDb []) 
            log
    
    log.Info (sprintf "Records Affected: %A" recordsAffected)
    ()

// Run (null, Log())

