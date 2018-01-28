open System

type Code = string

type Coin = {    
    code:Code  
}

type Price = {
    price: decimal
    excluded: bool
}

type Volume = {
    volume : decimal
    excluded: bool
}

type Pair = {
    exchangeName: string
    // the first currency in the pair is called base
    baseCurrency: Code
    quoteCurrency: Code
    pairPrice: Price
    volume: Volume    
}

type CoinName = CoinName of string

type CoinCode = CoinCode of string
type CoinVolume = CoinVolume of decimal
// type
type BasicInfo = CoinName * CoinCode * CoinVolume

type CodeAndProfile = CodeAndProfile of string * int

type PairDB = {
    // ID: Guid
    Date: DateTime
    PriceUSD: decimal
    Exchange: string
    Code: string
    Volume: decimal
}