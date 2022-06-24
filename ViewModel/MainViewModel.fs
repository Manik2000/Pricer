namespace ViewModel

open System
open System.Collections.ObjectModel
open LiveCharts;
open LiveCharts.Wpf;


type MainViewModel() = 
    inherit ViewModelBase()

    let options = ObservableCollection<OptionViewModel>()
    let data = ObservableCollection<ConfigurationViewModel>()
    let calculationParameters = ObservableCollection<ConfigurationViewModel>()
    
    let getDataConfiguration () = data |> Seq.map (fun conf -> (conf.Key , conf.Value)) |> Map.ofSeq
    let getCalculationConfiguration () = calculationParameters |> Seq.map (fun conf -> (conf.Key , conf.Value)) |> Map.ofSeq
    

    (* add some dummy data rows *)
    do
        data.Add(ConfigurationViewModel { Key = "r"             ; Value = "0.05" }) 
        data.Add(ConfigurationViewModel { Key = "volatility"    ; Value = "0.2"  })
        data.Add(ConfigurationViewModel { Key = "current price" ; Value = "1000" })

        calculationParameters.Add(ConfigurationViewModel { Key = "monteCarlo::runs" ; Value = "1000" })
        calculationParameters.Add(ConfigurationViewModel { Key = "Number of paths"  ; Value = "5" })


    let calculateFun _ = do
            options |> Seq.iter(fun option -> option.Calculate(getDataConfiguration (), getCalculationConfiguration ()))

    let calculate = SimpleCommand calculateFun

    let addOption = SimpleCommand(fun _ -> 
            let currentConfig = getCalculationConfiguration ()
            OptionRecord.Random currentConfig |> OptionViewModel |> options.Add
            )

    let removeOption = SimpleCommand(fun option -> options.Remove (option :?> OptionViewModel) |> ignore)
    let clearOptions = SimpleCommand(fun _ -> options.Clear () )


    (* charting *)
    let chartSeries = SeriesCollection()
    
    let getPlottingData () = 
        let plottingData = getDataConfiguration ()
        let plottingParams = getCalculationConfiguration ()
        let N = if plottingParams.ContainsKey "Number of paths"  then int plottingParams["Number of paths"] else 5
        let S0 = if plottingData.ContainsKey "current price" then float plottingData["current price"] else 1000.0
        let r = if plottingData.ContainsKey "r" then float plottingData["r"] else 0.05
        let vol = if plottingData.ContainsKey "volatility" then float plottingData["volatility"] else 0.2
        vol, r, S0, N


    let addChartSeriesFun _ = do 
        chartSeries.Clear ()

        let plotParams = getPlottingData ()
        let one_path = OptionPricer().geometricBrownian 

        let time = 0.5
        let (n, (vol, r, S0, N)) = (int(time * 180.0), plotParams)

        let addLine () = 
            let ls = LineSeries()
            ls.Values <- ChartValues<float> (one_path n S0 r vol time)
            chartSeries.Add(ls)
            
        seq { 1 .. int N } |> Seq.iter (fun _ -> addLine () |> ignore)
        
    let addChartSeries = SimpleCommand addChartSeriesFun


    (* add a few series for a good measure *)
    do
        addChartSeriesFun ()


    (* market data commands *)
    let addMarketDataRecord = SimpleCommand (fun _ -> data.Add(ConfigurationViewModel { Key = ""; Value = "" }))
    let removeMarketDataRecord = SimpleCommand (fun record -> data.Remove(record :?> ConfigurationViewModel) |> ignore)
    let clearMarketDataRecord = SimpleCommand (fun _ -> data.Clear ())


    (* calculation parameters commands *)
    let addCalcParameterRecord = SimpleCommand (fun _ -> calculationParameters.Add(ConfigurationViewModel { Key = ""; Value = "" }))
    let removeCalcParameterRecord = SimpleCommand (fun record -> calculationParameters.Remove(record :?> ConfigurationViewModel) |> ignore)
    let clearCalcParameterRecord = SimpleCommand (fun _ -> calculationParameters.Clear ())


    (* automatically update summary when dependency data changes (entries added/removed)  *)
    do
        options.CollectionChanged.Add calculateFun
        data.CollectionChanged.Add calculateFun
        calculationParameters.CollectionChanged.Add calculateFun

    (* commands *)
    member this.AddOption = addOption 
    member this.RemoveOption = removeOption
    member this.ClearOptions = clearOptions
    member this.Calculate = calculate

    member this.AddMarketData = addMarketDataRecord
    member this.RemoveMarketData = removeMarketDataRecord
    member this.ClearMarketData = clearMarketDataRecord
    
    member this.AddCalcParameter = addCalcParameterRecord 
    member this.RemoveCalcParameter = removeCalcParameterRecord 
    member this.ClearCalcParameter = clearCalcParameterRecord 


    (* data fields *)
    member this.Options = options
    member this.Data = data
    member this.CalculationParameters = calculationParameters

    
    (* charting *)
    member this.ChartSeries = chartSeries
    member this.AddChartSeries = addChartSeries
