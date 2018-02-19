open System

type Symbol = string

type Price = {
    price: decimal
    excluded: bool
}

type Volume = {
    volume : decimal
    excluded: bool
}

type PercetangeChange = PercetangeChange of decimal | UknownPercentage

type Supply = Minable of int64 | NotMinable of int64 | UknownSupply

type Coin = {    
    name: string
    symbol:Symbol  
    marketCap:Price
    price: Price
    circulatingSupply: Supply
    volume: Volume
    hourlyPercentChange: PercetangeChange
    dailyPercentChange: PercetangeChange
    weeklyPercentChange: PercetangeChange
}

type Pair = {
    exchangeName: string
    // the first currency in the pair is called base
    baseCurrency: Symbol
    quoteCurrency: Symbol
    pairPrice: Price
    volume: Volume    
}

type CoinName = CoinName of string

type CoinSymbol = CoinSymbol of string
type CoinVolume = CoinVolume of decimal
// type
type BasicInfo = CoinName * CoinSymbol * CoinVolume

type CodeAndProfile = CodeAndProfile of string * int

type PairDB = {
    // ID: Guid
    Date: DateTime
    PriceUSD: decimal
    Exchange: string
    Code: string
    Volume: decimal
}