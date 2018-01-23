#if INTERACTIVE
open System
#I @"C:/Users/stefan.uzunov.SCALEFOCUS/AppData/Roaming/nvm/v8.7.0/node_modules/azure-functions-core-tools/bin/"

#r "Microsoft.Azure.Webjobs.Host.dll" // Tracer
#r "Microsoft.Azure.WebJobs.Extensions.dll" // timer
open Microsoft.Azure.WebJobs.Host
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Timers
#endif

type Log (level) =
    inherit TraceWriter(level:System.Diagnostics.TraceLevel)
    new () = Log(Diagnostics.TraceLevel.Verbose)
    override this.Trace(event) = printfn "%A" event

let Run(myTimer: TimerInfo, log: TraceWriter) =
    log.Info(
        sprintf "F# Timer trigger function executed at: %s edited" 
            (DateTime.Now.ToString()))


// Run (null, Log())