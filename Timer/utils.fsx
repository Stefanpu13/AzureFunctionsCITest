open System

let (|ValidUSDPrice|_|) (price: string) = 
   let mutable v = 0.M
   let priceStr = price.Trim().Substring(1).Replace(",", "")
   if Decimal.TryParse(priceStr, &v) then Some(v)
   else None   


let (|ValidPrice|ExcludedPrice|InvalidPrice|) (price: string) = 
    let mutable v = 0.M
     
    let priceStr = price.Replace("$","").Replace(",", "").Replace(" ", "")
    if priceStr.Contains("*") then
        let excludedPriceStr = priceStr.Replace("*", "")

        if Decimal.TryParse(excludedPriceStr, &v) 
        then ExcludedPrice v
        else InvalidPrice   
    else
        if Decimal.TryParse(priceStr, &v) 
        then ValidPrice v
        else InvalidPrice   

let (|MinableSupply|NotMinableSupply|InvalidSupply|) (supply: string) = 
    let mutable v = 0L
     
    let priceStr = supply.Replace(",", "").Replace(" ", "")
    if priceStr.Contains("*") then
        let excludedPriceStr = priceStr.Replace("*", "")

        if Int64.TryParse(excludedPriceStr, &v) 
        then NotMinableSupply v
        else InvalidSupply   
    else
        if Int64.TryParse(priceStr, &v) 
        then MinableSupply v
        else InvalidSupply  

let (|ValidPersentage|InvalidPercentage|) (input: string) = 
    let mutable v = 0.M
    let percentageStr = input.Replace(" ", "").Replace("%", "")

    if Decimal.TryParse (percentageStr, &v) 
    then ValidPersentage v
    else InvalidPercentage


    

let join l separator : string = 
    Seq.reduce (fun r el -> r + separator + el) l
