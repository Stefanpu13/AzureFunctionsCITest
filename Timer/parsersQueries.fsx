
#load "types.fsx"
#load "parsers.fsx"

open Types
open Parsers
open IsThisCoinAScam


// Take list of high profile coins with daily volume more than 5 000 000
let highVolumeCoins = 
    IsThisCoinAScam.coinsWithDailyVolumeInInterval 5000000.M 1_000_000_000_000.M

let highVolumeCoinsCodes = 
    highVolumeCoins 
    |> Seq.map (fun (_, CoinSymbol c, _) -> c.ToLower())
    |> Set.ofSeq

let highProfileHighVolumeCoins = 
    coinsCodeAndProfile
    |> Seq.filter(fun (CodeAndProfile (c, p)) -> 
        p >= 80 && Set.contains (c.ToLower()) highVolumeCoinsCodes
    )

highProfileHighVolumeCoins |> List.ofSeq |> Seq.length

let allCoinsInTop25Exchanges =
    CoinMarketCap.coinsPerExchanges (CoinMarketCap.getTop25Exchanges())

let uniqueCoinPairs  = 
    allCoinsInTop25Exchanges
    |> Seq.map snd
    |> Seq.collect id
    |> Seq.distinctBy (fun p -> (p.baseCurrency, p.quoteCurrency))

// unique pairs in top 25 echanges are less than all recorded pairs
// for example btc/usdt is traded in more than one exchange
uniqueCoinPairs |> Seq.length

let dailyRecordsCount = 
    allCoinsInTop25Exchanges
    |> Seq.map snd
    |> Seq.collect id
    |> Seq.length

// get unique coinPairs where the base qurrency is high volume and high profile

let uniqueHighVolumeAndProfilePairs = 
    uniqueCoinPairs 
    |> Seq.filter (fun p -> 
        Set.contains (p.baseCurrency.ToLower()) highVolumeCoinsCodes 
)


uniqueCoinPairs 
|> Seq.filter (fun p -> 
    Set.contains (p.baseCurrency.ToLower()) highVolumeCoinsCodes 
)
|> Seq.map (fun p -> p.baseCurrency, p.quoteCurrency)
|> List.ofSeq
|> Seq.length

CoinMarketCap.coinsInExchange "gate-io" 
|> snd
|> Seq.map (fun p -> "gate-io", p.baseCurrency + "/" + p.quoteCurrency)
|> List.ofSeq


// get list of coins with 50 or more million volume
let majorCoins = 
    IsThisCoinAScam.coinsWithBiggerDailyVolume 50_000_000.M
    |> List.ofSeq


// which of the has lost the most the last week
// which has lost the least the last week

// #time

let allCoinsData = 
    CoinMarketCap.loadAllCoinsPage ()
    |> CoinMarketCap.getAllCoinsRows
    |> Seq.map CoinMarketCap.getCoinRow


allCoinsData
|> Seq.filter(fun c -> 
    majorCoins
    |> List.exists (fun (_, CoinSymbol s, _) -> 
        s.ToLower() = c.symbol.ToLower()
    )
)
|> Seq.sortBy (fun c -> c.weeklyPercentChange)
|> Seq.map (fun c ->
    c. name, c. symbol, c.weeklyPercentChange
)
|> List.ofSeq
|> List.iter (fun (n, s, w) -> 
    let p = 
        match w with
        | PercetangeChange p -> float p
        | UknownPercentage -> 0.0

    printfn "Name: %A;  Symbol: %A; WeeklyChange: %A%%" n s p                
)

CoinMarketCap.getTopExchangesWithVolume ()

CoinMarketCap.coinsInExchange "upbit"
|> snd
|> Seq.filter (fun p -> 
    List.exists (fun (_, CoinSymbol s, _) -> 
        p.baseCurrency.ToLower() = s.ToLower()         
    ) majorCoins
)
|> Seq.map (fun p -> p.baseCurrency, p.quoteCurrency)
|> Seq.toList
|> List.filter (fun (_, q) ->    
    q.ToLower() <> "usdt" && 
    q.ToLower() <> "bnb"
)