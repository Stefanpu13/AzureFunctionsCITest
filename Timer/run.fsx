#if INTERACTIVE
open System
#I @"C:/Users/stefan.uzunov.SCALEFOCUS/AppData/Roaming/nvm/v8.7.0/node_modules/azure-functions-core-tools/bin/"
#I @"C:/Users/stefan.uzunov.SCALEFOCUS/.nuget/packages/fsharp.data/2.4.4/lib/net45/"

#I @"C:\Users\stefan.uzunov.SCALEFOCUS\.nuget\packages\fsharp.data.typeproviders\5.0.0.2\lib\net40"
#r "Microsoft.Azure.Webjobs.Host.dll" // Tracer
#r "Microsoft.Azure.WebJobs.Extensions.dll" // timer

#r "FSharp.Data.dll"
#r "FSharp.Data.DesignTime.dll"

#r "FSharp.Data.TypeProviders.dll"


open Microsoft.Azure.WebJobs.Host
open Microsoft.Azure.WebJobs
#endif

// #r "System.Data.Linq.dll"

open FSharp.Data
// open System.Data
// open System.Data.Linq
// open FSharp.Data.TypeProviders



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

// type SQL = SqlDataConnection<AzureConnectionString>

// let ctx = SQL.GetDataContext()

// ctx.
// let readFromAzureDb () =    
//     ctx.Pairs 
//     |> Seq.take 2
//     |> Seq.toArray 
//     |> Seq.map (fun p -> p.Exchange)

type Log (level) =
    inherit TraceWriter(level:System.Diagnostics.TraceLevel)
    new () = Log(Diagnostics.TraceLevel.Verbose)
    override this.Trace(event) = printfn "%A" event

let Run(myTimer: TimerInfo, log: TraceWriter) =
    let exchangesUri = "https://coinmarketcap.com/exchanges/volume/24-hour/all/"
    let mutable exchangesPage = HtmlDocument.Load(exchangesUri)
    let mutable exchangesInfoRows = exchangesPage.CssSelect(".table.table-condensed > tr")   

    let getTop25Exchanges () =             
        exchangesInfoRows
        |> Seq.map(fun el -> el, el.AttributeValue("id"))
        |> Seq.filter(fun (_, attr) -> not <| String.IsNullOrEmpty(attr))
        |> Seq.map snd
        |> Seq.take 25

    let topExchange  = getTop25Exchanges() |> Seq.item 0 

    log.Info(
        sprintf "Top exchange: %s" topExchange           
        )
    
    // readFromAzureDb () 
    // |> Seq.map (fun exch -> sprintf "Top exchange: %s" exch)
    // |> Seq.reduce (+)
    // |> log.Info    

// Run (null, Log())