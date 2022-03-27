namespace ViewModel
 
//Representation of a Payment to the UI
type PaymentViewModel(input : PaymentRecord) = 
    inherit ViewModelBase()

    let mutable userInput = input
    let mutable value : Money option = None

    member this.TradeName 
        with get() = userInput.TradeName
        and set(x) = 
            userInput <- {userInput with TradeName = x }
            base.Notify("TradeName")

    member this.Expiry 
        with get() = userInput.Expiry
        and set(x) = 
            userInput <- {userInput with Expiry = x }
            base.Notify("Expiry")

    member this.Currency 
        with get() = userInput.Currency
        and set(x) = 
            userInput <- {userInput with Currency = x }
            base.Notify("Currency")

    member this.Principal 
        with get() = userInput.Principal
        and set(x) = 
            userInput <- {userInput with Principal = x }
            base.Notify("Principal")
    
    member this.Value
        with get() = value
        and set(x) = 
            value <- x
            base.Notify("Value")

    // Invoke the valuation based on user input
    member this.Calculate(data : DataConfiguration, calculationParameters : CalculationConfiguration) = 
        
        //capture inputs
        let paymentInputs : PaymentValuationInputs = 
            {
                Trade = 
                         {
                             TradeName = this.TradeName
                             Expiry    = this.Expiry
                             Currency  = this.Currency
                             Principal = this.Principal
                         }
                Data = data
                CalculationsParameters = calculationParameters
            }
        //calculate
        let calc = PaymentValuationModel(paymentInputs).Calculate()

        //present to the user
        this.Value <- Option.Some (calc)

(* summary row. there is little functionality here, so this is very brief. *)
type SummaryRow = 
    {
        Currency: string
        Value : float
    }