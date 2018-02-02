(*
    A list of daily tasks that will be usefull to catch trends:
    1. Pairs:
        1. A new pair is added to an exchange(s)
            a) Reasoning: adding pair to echange is positive for the pair price
            Especially if the exchange is bigger and has smaller number of pairs
            b) Expected price behaviour
                - Rise in the following days to week, 
                but depends on overall market sentiment
            c) Why this is useful: 
                - Short term rallies can be profitable,
                and are more likely, due to the coin gaining popularity
            d) What can go wrong:
                - Price might already be pumped and sell off follows 
                the listing of the coin on an exchange
                - The coin might not go up, as the general market is bearish
                (This is also a good thing - opportunity to buy at low price?)
                -Volativity might be big and in two directions
            e) What data to look for:            
                - Which is the pair
                - Which is the exchange and which is it position 
                    * is it a top 10 by volume exchange - more potensial buyers
                    * does it have small number of pairs - more likely ro have
                    serious scrutiny rules                    
                    * Are there are multiple exchanges
        2. A pair is removed from exchange
            a) Reasoning: Number of reasons, most of them very negative:
                - Pair is forbidden from regulations (its a scam, ponzy, etc)
                - Exchange is having problems with pair - Pair trading is suspended
                due to investigations or wild price swings that treaten the exchange
                finances
            b) Expected price behviour:
                - Instant, very sharp decline
            c) Why this is usefull:
                Sell indicator:
                - If one pair is removed, other pairs can be removed too, 
                for similar reasons (
                Reasons can be: regulators; problems with exchange)
            d) What can go wrong
                - I will loose if invested and, 
                If overinvested I can have a huge loss
                - Negative effect to other currencies (including mine) 
                might be immediate                
                (For example, regulators say: ICO is scam and must be removed, 
                other removals might follow)
                - Exchange can have problems with solvency(This assumes that
                exchange pays customers with its money):
                    A lot of users are overinvested and incurring losses, try to 
                    withdraw money at once. The exchange becomes thin on money. Then,
                    people holding other currencies try to cash in, but there is not
                    enough money
                     )
                - regulator bans the currency or, a class of currencies - that leads to
                huge sell pressure
            e) What data to look for: 
                - Pair
                    * Which is it
                    * Is a pair in top 100 by volume

                - Exchange
                    * Which is it
                    * Are there others

        3. A pair is excluded on "coinmarketcap"
*)

#I @"C:\Users\stefan.uzunov.SCALEFOCUS\.nuget\packages\sqlprovider\1.1.28\lib\net451"
#r "FSharp.Data.SQLProvider.dll"
open FSharp.Data.Sql
open System

[<Literal>]
let AzureConnectionString = @"
Server=tcp:cryptospu.database.windows.net,1433;
Initial Catalog=Crypto;
Persist Security Info=False;
User ID=crypto_db;
Password=Stefan@2;
MultipleActiveResultSets=False;
Encrypt=True;
TrustServerCertificate=False;
Connection Timeout=30;
"

type Sql = SqlDataProvider< 
              ConnectionString = AzureConnectionString,
              DatabaseVendor = Common.DatabaseProviderTypes.MSSQLSERVER,                   
              CaseSensitivityChange = Common.CaseSensitivityChange.ORIGINAL,       
              UseOptionTypes = true>

let ctx = Sql.GetDataContext()

// #time

let toExchangeAndCode (pairs: seq<Sql.dataContext.``dbo.PairsEntity``>) = 
    pairs
    |> Seq.map(fun p -> p.Exchange, p.Code)
    |> Set.ofSeq

let toPairsPerExchange s = 
    s 
    |> Seq.groupBy fst
    |> Seq.map (fun (gr, s) -> gr, (Seq.map snd >> Seq.toList) s)    
    |> List.ofSeq


// get pairs for specific hour/datetime

let stripMinutesAndHours (utcDate: DateTime) = 
    DateTime(
        utcDate.Year,
        utcDate.Month,
        utcDate.Day, 
        utcDate.Hour, 
        0, 
        0,
        DateTimeKind.Utc
    )

let getPairsForHourOfDay (date: DateTime) =
    let d = date.Day
    let h = date.Hour   
    query {
        for pair in ctx.Dbo.Pairs  do
            where (
                pair.Date >= date && 
                d = pair.Date.Day &&
                h = pair.Date.Hour
            )
            select pair
    }
    |> Seq.toList

let getPairsForHour = stripMinutesAndHours >> getPairsForHourOfDay 

let atBeginningOfHour (pairs: seq<Sql.dataContext.``dbo.PairsEntity``>) =
    pairs
    // Note: records are not always done at 0, 20, 40 min
    |> Seq.filter (fun p -> p.Date.Minute < 20)


let getPairsExchangeAndCode = 
    getPairsForHour 
    >> atBeginningOfHour 
    >> toExchangeAndCode

let yesterdayPairs = 
    DateTime.UtcNow.AddDays(-1.0)
    |> getPairsExchangeAndCode

let latestPairs = 
    DateTime.UtcNow
    |> getPairsExchangeAndCode

let addedPairs = 
    Set.difference latestPairs yesterdayPairs
    |> toPairsPerExchange

let removedPairs = 
    Set.difference yesterdayPairs latestPairs
    |> toPairsPerExchange


let lastHourPairs = 
    DateTime.UtcNow.AddHours(-1.0)
    |> getPairsExchangeAndCode

Set.difference latestPairs lastHourPairs
|> toPairsPerExchange

Set.difference lastHourPairs latestPairs
|> toPairsPerExchange


Set.difference latestPairs yesterdayPairs
|> toPairsPerExchange

Set.difference yesterdayPairs latestPairs
|> toPairsPerExchange
// |> Seq.find (fun (e, _) -> e = "gate-io")
// |> snd
// |> List.length

latestPairs 
|> Set.filter(fun (e, _ )-> e ="gate-io")