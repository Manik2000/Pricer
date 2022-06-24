namespace ViewModel

open System
open FSharp.Stats.Distributions


type PricerResult = 
    {
        EuropeanOption: float
        AsianOption: float
        Delta: float
    }


(* Model for option pricing. *)
type OptionRecord =
    {
        OptionName    : string
        OptionType    : string
        Maturity      : DateTime
        Strike        : float
    }
    
    (* Simple utility method for creating a random payment. *)
    static member sysRandom = System.Random()
    static member Random(configuration : CalculationConfiguration) = 
        
        {
            OptionName   = sprintf "Option%04d" (OptionRecord.sysRandom.Next(9999))
            OptionType   = [|"call"; "put"|][OptionRecord.sysRandom.Next(2)]
            Maturity     = (DateTime.Now.AddMonths (OptionRecord.sysRandom.Next(1, 6))).Date
            Strike       = float (OptionRecord.sysRandom.Next(800, 1200))
        }


type OptionValuationInputs = 
    {
        Options : OptionRecord
        Data : DataConfiguration
        CalculationsParameters: CalculationConfiguration
    }


(* Different methods of options pricing *)
type OptionPricer() = 
    
    let normal = Continuous.normal 0.0 1.0

    member this.geometricBrownian n S0 r vol years = 
        let (normals, indexes, m) = (Array.init n (fun _ -> normal.Sample()), [|1 .. n|], float n)
        (S0, indexes) 
        ||>  Array.scan (fun x y -> x * exp ((r - vol*vol / 2.0) * years / m + vol * sqrt (years / m) * normals[y - 1]))    

    member this.callAsianOptionPrice M S0 K time r vol = 
        let n = int (time * 300.0)
        let payoff array =  Array.average array - K |> max 0.0               
        [|1 .. M|]
        |> Array.map (fun _ -> payoff (this.geometricBrownian n S0 r vol time))
        |> Array.average

    member this.putAsianOptionPrice M S0 K time r vol =
        let n = int (time * 300.0)
        let payoff array =  K - Array.average array |> max 0.0
        [|1 .. M|]
        |> Array.map (fun _ -> payoff (this.geometricBrownian n S0 r vol time))
        |> Array.average  

    member this.d S0 K r vol T = (log(S0 / K) + (r + vol*vol / 2.0) * T) / (vol * sqrt T)

    member this.callEuropeanOptionPrice S0 K r T vol =
        let d1 = this.d S0 K r vol T
        S0 * normal.CDF(d1) - K * exp(-r * T) * normal.CDF(d1 - vol * sqrt T)

    member this.putEuropeanOptionPrice S0 K r T vol = 
        let d1 = this.d S0 K r vol T
        K * exp(-r * T) * normal.CDF(vol * sqrt T - d1) - S0 * normal.CDF(-d1)

    member this.callDelta S0 K r vol T= 
        (this.d S0 K r vol T) |> normal.CDF     

    member this.putDelta S0 K r vol T= 
        - (this.d S0 K r vol T) |> fun x -> -normal.CDF(x)    


type OptionValuationModel(inputs: OptionValuationInputs) = 
    member this.Calculate() : PricerResult =     
        let r = if inputs.Data.ContainsKey "r" then float inputs.Data["r"] else 0.05
        let T = (float (inputs.Options.Maturity - DateTime.Now).Days) / 365.0 
        let K = inputs.Options.Strike
        let type_ = inputs.Options.OptionType        
        let S0 = if inputs.Data.ContainsKey "current price" then float inputs.Data["current price"] else 1000.0              
        let vol = if inputs.Data.ContainsKey "volatility" then float inputs.Data["volatility"] else 0.2

        let MonteCarloRuns = if inputs.CalculationsParameters.ContainsKey "monteCarlo::runs" 
                                then int inputs.CalculationsParameters["monteCarlo::runs"]
                             else 1000

        let pricer = OptionPricer ()

        let europeanOptionPrice = match type_ with
                                  | "call" -> pricer.callEuropeanOptionPrice S0 K r T vol
                                  | _      -> pricer.putEuropeanOptionPrice S0 K r T vol

        let asianOptionPrice = match type_ with
                               | "call" -> pricer.callAsianOptionPrice MonteCarloRuns S0 K T r vol
                               | _      -> pricer.putAsianOptionPrice MonteCarloRuns S0 K T r vol

        let delta = match type_ with
                    | "call" -> pricer.callDelta S0 K r vol T
                    | _ -> pricer.putDelta S0 K r vol T

        { EuropeanOption = europeanOptionPrice ; Delta = delta; AsianOption = asianOptionPrice }
