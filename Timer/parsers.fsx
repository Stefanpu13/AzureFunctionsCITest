(*
    What data do i need to save for each exchange?
        - name
        - country of operation(or registration)
        - traded currencies (as pairs)
        - hourly/daily volume for each pair
        - hourly/daily exchange rates for each pair
    
    What other data is valuable?
        - added/removed currencies pairs
            * Adding a currency, especially on large and reputable exchange,
            is positive sign for the currency(although it will probably intially drop
            as early investors exit)
            * Removing currency is bad sign and will cause sell-off
        - differences in prices accross exchanges - market is decentralized and fragmented.
        That means that in the coming months/years, different things will affect individual 
        exchanges:
            * theft
            * regulations
            * bans
        The info will not be available immediately in the form of news. However,
        Informed people will do actions that will cause price distortions. For example,
            * Upcoming negative events will cause increased sells at particular
            echange. As result the prices of major coins will be lower than on other
            exchanges
                ** If prices are for a specific currency only, that might indicate
                problem with the currency (for exmaple, its about to be removed)
                ** IF prices are for several currencies, 
                than maybe the exchange has problems
*)

#if INTERACTIVE
#I @"C:/Users/stefan.uzunov.SCALEFOCUS/.nuget/packages/fsharp.data/2.4.4/lib/net45/"
// my pc
#I @"C:/Users/stefan/.nuget/packages/fsharp.data/2.4.4/lib/net45/"

#r "FSharp.Data.dll"
#r "FSharp.Data.DesignTime.dll"
#endif

open System
open FSharp.Data

#load "utils.fsx"
#load "types.fsx"

open Utils
open Types    


module CoinMarketCap = 
    let allCoinsUri = "https://coinmarketcap.com/all/views/all/"
    let exchangesUri = "https://coinmarketcap.com/exchanges/volume/24-hour/all/"
    let mutable exchangesPage = HtmlDocument.Load(exchangesUri)
    let mutable exchangesInfoRows = exchangesPage.CssSelect(".table.table-condensed > tr")    
    let count = 25   

    let reInit(uri: string) =         
        exchangesPage <- HtmlDocument.Load(uri)
        exchangesInfoRows <- exchangesPage.CssSelect(".table.table-condensed > tr")

    let innerText (children: HtmlNode []) pos = 
        children.[pos]
        |> (fun el -> el.InnerText())

    let getTop25Exchanges () =             
        exchangesInfoRows
        |> Seq.map(fun el -> el, el.AttributeValue("id"))
        |> Seq.filter(fun (_, attr) -> not <| String.IsNullOrEmpty(attr))
        |> Seq.map snd
        |> Seq.take count            

    let getTopExchangesWithVolume () = 
        let topExchanges = 
            getTop25Exchanges ()
            |> List.ofSeq

        let topExchangesVolume = 
            exchangesInfoRows
            |> Seq.filter(fun row -> 
                let ems = row.CssSelect("td > em")

                if ems.IsEmpty 
                then false
                else ems.Head.InnerText() = "Total"
            )
            |> Seq.map( fun volumeRow -> 
                volumeRow.Elements()
                |> Seq.item 1
                |> (fun td -> td.InnerText())
            )
            |> Seq.take count
            |> List.ofSeq

        List.zip topExchanges topExchangesVolume

    let getExchangePairsRows (exchangePage:HtmlDocument) = 
        exchangePage.CssSelect(".table-responsive tr") 
        // skip the header row
        |> Seq.skip 1 

    let parseExchangePairRow exchangeName (row: HtmlNode) =     
        let children = row.Elements()
        let pair = 
            children
            |> Seq.item 2
            |> (fun td -> td.InnerText())
        let baseAndQuote = pair.Split([|'/'|])               
        let baseCurr, quoteCurr = baseAndQuote.[0], baseAndQuote.[1]
        let pairVolume = 
            children 
            |> Seq.item 3
            |> (fun td -> td.InnerText())
            |> (fun volume -> 
                match volume with
                | ValidPrice p -> {volume =p;Volume.excluded=false} 
                | ExcludedPrice p  -> {volume=p;excluded=true} 
                | InvalidPrice ->  {volume=0.M;excluded=false}
            )
        let pairPriceUSD = 
            children
            |> Seq.item 4
            |> (fun td -> td.InnerText())
            |> (fun pairPrice -> 
                match pairPrice with
                | ValidPrice p -> {price=p;excluded=false} 
                | ExcludedPrice p  -> {price=p;excluded=true} 
                | InvalidPrice ->  {price=0.M;excluded=false}
            )
        {
            exchangeName=exchangeName
            baseCurrency= baseCurr
            quoteCurrency= quoteCurr
            pairPrice=pairPriceUSD
            volume=pairVolume
        }            

    let coinsInExchange exchangeName =         
        let exchangePage = 
            HtmlDocument.Load("https://coinmarketcap.com/exchanges/" + exchangeName)
        let listedCoinsRows = 
            getExchangePairsRows exchangePage
            |> Seq.map (parseExchangePairRow exchangeName)
            |> Set.ofSeq          
        
        exchangeName, listedCoinsRows  

    let coinsPerExchanges exchanges = 
        exchanges
        |> Seq.map coinsInExchange
        |> List.ofSeq

    // List all top 20 exchanges where given coin is traded(is base currency)

    let exchangesOfCoin (coinCode: string) =             
        getTop25Exchanges ()            
        |> coinsPerExchanges
        |> Seq.filter( fun coinsOnExchange ->
            coinsOnExchange
            |> snd
            |> Set.exists(fun c -> c.baseCurrency.ToLower() = coinCode.ToLower())
        )

    let loadAllCoinsPage () = 
        HtmlDocument.Load(allCoinsUri)
    let getAllCoinsRows (allCoinsPage: HtmlDocument) = 
        allCoinsPage.CssSelect(".table.js-summary-table tr")
    let getCoinRow (row: HtmlNode) = 
        let children = row.Elements() |> Seq.toArray
        let childInnerText  = innerText children

        let name = 
            children.[1]
            |> (fun td -> 
                td.Elements()
                |> Seq.last
                |> (fun a -> a.InnerText())                 
            ) 
        let symbol = childInnerText 2
        let marketCap = 
            childInnerText 3
            |> (fun marketCap -> 
                match marketCap with
                | ValidPrice p -> {price=p;excluded=false} 
                | ExcludedPrice p  -> {price=p;excluded=true} 
                | InvalidPrice ->  {price=0.M;excluded=false}
            )
        let price = 
            childInnerText 4
            |> (fun price -> 
                match price with
                | ValidPrice p -> {price=p;excluded=false} 
                | ExcludedPrice p  -> {price=p;excluded=true} 
                | InvalidPrice ->  {price=0.M;excluded=false}
            )      
        let circulatingSupply = 
            childInnerText 5
            |> (fun supply -> 
                match supply with
                | MinableSupply p -> Minable p 
                | NotMinableSupply p -> NotMinable p
                | InvalidSupply ->  UknownSupply
            )      

        let volume = 
            childInnerText 6
            |> (fun volume -> 
                match volume with
                | ValidPrice p -> {volume=p;excluded=false} 
                | ExcludedPrice p  -> {volume=p;excluded=true} 
                | InvalidPrice ->  {volume=0.M;excluded=false}
            )                 
        
        let hourlyPercentChange = 
             childInnerText 7
             |> (fun t -> 
                match t with
                | ValidPersentage p -> PercetangeChange p
                | InvalidPercentage -> UknownPercentage
             )

        let dailyPercentChange = 
             childInnerText 8
             |> (fun t -> 
                match t with
                | ValidPersentage p -> PercetangeChange p
                | InvalidPercentage -> UknownPercentage
             )

        let weeklyPercentChange = 
             childInnerText 9
             |> (fun t -> 
                match t with
                | ValidPersentage p -> PercetangeChange p
                | InvalidPercentage -> UknownPercentage
             )         

        {
            name=name
            symbol=symbol
            marketCap=marketCap
            price=price
            circulatingSupply=circulatingSupply
            volume=volume
            hourlyPercentChange=hourlyPercentChange
            dailyPercentChange=dailyPercentChange
            weeklyPercentChange=weeklyPercentChange
        }


module IsThisCoinAScam =
    let allCoinsPage = HtmlDocument.Load("https://coinmarketcap.com/all/views/all/")

    let coinRows = allCoinsPage.CssSelect("#currencies-all > tbody > tr")

    // page displays a table but before javascript is executed all coins are
    // displayed in the html
    let allCoinsProfilesPage = HtmlDocument.Load("https://isthiscoinascam.com/#")

    let allCoinsProfiles = allCoinsProfilesPage.CssSelect("#example > tbody > tr")

    allCoinsProfiles |> Seq.head |> (fun row ->                 
        row.Elements() 
        |> Seq.last 
        |>  (fun td -> td.InnerText())
    )

    let mutable coinsBasicInfo = Seq.empty

    let getInnerText (parent: HtmlNode) selector = 
        match Seq.tryHead (parent.CssSelect(selector)) with
        |Some  el ->  el.InnerText() 
        |None -> "Element does not exist"

    let coinsCodeAndProfile = 
        allCoinsProfiles |> Seq.map (fun row ->
            let code = row.Elements() |> Seq.item 1 |> (fun el -> el.InnerText())
            let profileValue = 
                row.Elements() 
                |> Seq.last 
                |> (fun el -> el.InnerText())
                |> int

            CodeAndProfile (code, profileValue)        
        )    

    let getCoinsBasicInfo () = 
        coinRows 
        |> Seq.map(fun coinRow -> 
            let coinName = getInnerText coinRow ".currency-name-container"        
            let coinSymbol = getInnerText coinRow ".col-symbol"        
            let coinVolume = getInnerText coinRow ".volume"
                
            (
                CoinName coinName, 
                CoinSymbol coinSymbol, 
                CoinVolume (
                    match coinVolume with
                    | ValidUSDPrice p -> p 
                    |_ -> 0.M
                )
            )    
        )

    let biggerThanVolume vol = fun (_,_,CoinVolume v) -> v >= vol 
    let volumeInInterval low high = 
        fun (_,_,CoinVolume v) -> low <= v && v <= high  

    let coinsWithDailyVolumeInInterval low high = 
        Seq.filter (volumeInInterval low high) coinsBasicInfo
        
    let coinsWithBiggerDailyVolume vol = 
        Seq.filter (biggerThanVolume vol) coinsBasicInfo    

    let profileCoins profile coins ((_,CoinSymbol code,_)) =
        Seq.exists(fun (CodeAndProfile(c:string, pr )) -> 
            c.ToLower() = code.ToLower() && pr >= profile
        ) coins
    
    
    coinsBasicInfo <- getCoinsBasicInfo ()

