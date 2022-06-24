namespace ViewModel
 
//Representation of a Payment to the UI
type OptionViewModel(input : OptionRecord) = 
    inherit ViewModelBase()

    let mutable userInput = input
    let mutable european_value : float option = None
    let mutable asian_value : float option = None
    let mutable delta : float option = None

    member this.OptionName 
        with get() = userInput.OptionName
        and set(x) = 
            userInput <- {userInput with OptionName = x}
            base.Notify("OptionName")

    member this.OptionType
        with get() = userInput.OptionType
        and set(x) = 
            userInput <- {userInput with OptionType = x}
            base.Notify("OptionType")

    member this.Maturity
        with get() = userInput.Maturity
        and set(x) = 
            userInput <- {userInput with Maturity = x}
            base.Notify("Maturity")

    member this.Strike
        with get() = userInput.Strike
        and set(x) = 
            userInput <- {userInput with Strike = x}
            base.Notify("Strike")

    member this.EuropeanOption
        with get() = european_value
        and set(x) = 
            european_value <- x
            base.Notify("EuropeanOption")

    member this.AsianOption
        with get() = asian_value
        and set(x) = 
            asian_value <- x
            base.Notify("AsianOption")

    member this.Delta
        with get() = delta
        and set(x) = 
            delta <- x
            base.Notify("Delta")

    // Invoke the valuation based on user input
    member this.Calculate(data : DataConfiguration, calculationParameters : CalculationConfiguration) = 
        
        //capture inputs
        let paymentInputs : OptionValuationInputs = 
            {
                Options = 
                         {
                            OptionName    = this.OptionName
                            OptionType    = this.OptionType
                            Maturity      = this.Maturity
                            Strike        = this.Strike
                         }
                Data = data
                CalculationsParameters = calculationParameters
            }

        //calculate
        let calc = OptionValuationModel(paymentInputs).Calculate()

        //present to the user
        this.EuropeanOption <- Option.Some (calc.EuropeanOption)

        this.AsianOption <- Option.Some (calc.AsianOption)

        this.Delta <- Option.Some (calc.Delta)

