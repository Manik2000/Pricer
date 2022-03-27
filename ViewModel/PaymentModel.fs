namespace ViewModel

open System

(* A type representing given amount of money in specific currency. Very bare bones, could be extended in various ways. Some examples:
1. Multiplication by float so that $1 * 100 = $100.
2. Addition to other Money instance so that $1 + $2 = $3, but 1 zl + $1 = <exception thrown> *)
type Money =
    {
        Value : float
        Currency : string
    }

    override this.ToString() = sprintf "%.2f (%s)" this.Value this.Currency

(* Model for Payment trade. *)
type PaymentRecord =
    {
        TradeName : string
        Expiry    : DateTime
        Currency  : string
        Principal : int64
    }
    
    (* Simple utility method for creating a random payment. *)
    static member sysRandom = System.Random()
    static member Random(configuration : CalculationConfiguration) = 
        (* We pick a random currency either from given short list, or from valuation::knownCurrencies config key *)
        let knownCurrenciesDefault = [| "EUR"; "USD"; "PLN"; |]
        
        let knownCurrencies = if configuration.ContainsKey "valuation::knownCurrencies" 
                              then configuration.["valuation::knownCurrencies"].Split([|' '|])
                              else knownCurrenciesDefault
        
        {
            TradeName = sprintf "Payment%04d" (PaymentRecord.sysRandom.Next(9999))
            Expiry    = (DateTime.Now.AddMonths (PaymentRecord.sysRandom.Next(1, 6))).Date
            Currency  = knownCurrencies.[ PaymentRecord.sysRandom.Next(knownCurrencies.Length) ]
            Principal = int64 (PaymentRecord.sysRandom.Next())
        }

(* Complete set of data required for valuation *)
type PaymentValuationInputs = 
    {
        Trade : PaymentRecord
        Data : DataConfiguration
        CalculationsParameters: CalculationConfiguration
    }

(* The valuation model for Payment. We may have multiple valuation models implementations per given trade type, or have a valuation model that handles multiple trade types. *)
type PaymentValuationModel(inputs: PaymentValuationInputs) = 
    (* Calculate() method returns a value of given trade. This one is very simple, yet demonstrates some concepts.
    
    It will try to return the result in the global default currency as configured by valuation::baseCurrency key.

    If the valuation::baseCurrency is not defined or we are unable to obtain the FX rate FX::<targetCcy><tradeCcy>, 
    we simply return the value using the trade currency.

    *)
    member this.Calculate() : Money = 
        let tradeCcy = inputs.Trade.Currency

        let targetCcy = match inputs.CalculationsParameters.TryFind "valuation::baseCurrency" with
                         | Some ccy -> ccy
                         | None -> tradeCcy

        let fxRateKey = sprintf "FX::%s%s" targetCcy tradeCcy

        let fxRate = if inputs.Data.ContainsKey fxRateKey then float inputs.Data.[ fxRateKey ] else 1.0 // lookup FX rate
        let finalCcy = if inputs.Data.ContainsKey fxRateKey then targetCcy else tradeCcy
        (* changes here - accumulation of the principal value *)
        let r = if inputs.Data.ContainsKey "r" then float inputs.Data["r"] else 0.05
        let T = (float (inputs.Trade.Expiry - DateTime.Now).Days) / 365.0 

        { Value = (float inputs.Trade.Principal)  * exp (r * T) / fxRate; Currency = finalCcy }
        
