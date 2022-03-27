namespace ViewModel

type ConfigurationRecord = 
    {
        Key : string
        Value : string
    }
    
type DataConfiguration = Map<string, string>
type CalculationConfiguration = Map<string, string>

type ConfigurationViewModel( configRec : ConfigurationRecord) = 
    inherit ViewModelBase()

    let mutable configRec = configRec

    member this.Value
        with get() = configRec.Value
        and set(x) = 
            configRec <- {configRec with Value = x }
            base.Notify("Value")
    
    member this.Key
        with get() = configRec.Key
        and set(x) = 
            configRec <- {configRec with Key = x }
            base.Notify("Key") 

