namespace ViewModel

open System
open System.Collections.ObjectModel
open LiveCharts;
open LiveCharts.Wpf;

//Strating point of the viewmodel that drives the UI
//It aggregates all relevant parts of the UI, and exposes them via properties
type MainViewModel() = 
    inherit ViewModelBase()

    let trades = ObservableCollection<PaymentViewModel>()
    let data = ObservableCollection<ConfigurationViewModel>()
    let calculationParameters = ObservableCollection<ConfigurationViewModel>()

    let getDataConfiguration () = data |> Seq.map (fun conf -> (conf.Key , conf.Value)) |> Map.ofSeq
    let getCalculationConfiguration () = calculationParameters |> Seq.map (fun conf -> (conf.Key , conf.Value)) |> Map.ofSeq
    
    (* add some dummy data rows *)
    do
        data.Add(ConfigurationViewModel { Key = "FX::USDPLN"; Value = "3.76" })
        data.Add(ConfigurationViewModel { Key = "FX::USDEUR"; Value = "0.87" })
        data.Add(ConfigurationViewModel { Key = "FX::EURGBP"; Value = "0.90" })
        data.Add(ConfigurationViewModel { Key = "r"; Value = "0.05" }) // changed here

        calculationParameters.Add(ConfigurationViewModel { Key = "monteCarlo::runs"; Value = "100" })
        calculationParameters.Add(ConfigurationViewModel { Key = "valuation::baseCurrency"; Value = "USD" })
        calculationParameters.Add(ConfigurationViewModel { Key = "valuation::knownCurrencies"; Value = "USD PLN EUR GBP" })
        calculationParameters.Add(ConfigurationViewModel { Key = "methodology::bumpRisk"; Value = "True" })
        calculationParameters.Add(ConfigurationViewModel { Key = "methodology::bumpSize"; Value = "0.0001" })

    let summary = ObservableCollection<SummaryRow>()

    (* trade commands *)
    let refreshSummary() = 
        summary.Clear()
        
        trades 
        |> Seq.choose(fun t -> t.Value) // find correctly evaluated trades
        |> Seq.groupBy(fun m -> m.Currency)  // group by currency
        |> Seq.map(fun (ccy, v) -> { Currency = ccy; Value = v |> Seq.map (fun m -> m.Value) |> Seq.sum }) // extract values, calculate a sum
        |> Seq.iter(summary.Add) // add to summary page

    let calculateFun _ = do
            trades |> Seq.iter(fun trade -> trade.Calculate(getDataConfiguration (), getCalculationConfiguration ()))
            refreshSummary()

    let calculate = SimpleCommand calculateFun

    let addTrade = SimpleCommand(fun _ -> 
            let currentConfig = getCalculationConfiguration ()
            PaymentRecord.Random currentConfig |> PaymentViewModel |> trades.Add
            )

    let removeTrade = SimpleCommand(fun trade -> trades.Remove (trade :?> PaymentViewModel) |> ignore)
    let clearTrades = SimpleCommand(fun _ -> trades.Clear () )

    (* charting *)
    
    let chartSeries = SeriesCollection()

    let predefinedChartFunctions = [| (fun x -> sin x); (fun x -> x); (fun x -> x*x) |] 

    let addChartSeriesFun _ = do
                let ls = LineSeries()
                let multiplier = System.Random().NextDouble()
                let mapFun = predefinedChartFunctions.[ System.Random().Next(predefinedChartFunctions.Length) ]
                ls.Title <- sprintf "Test series %0.2f" multiplier
                let series = seq { for i in 1 .. 100 do yield (0.01 * multiplier * double i) }
                ls.Values <- ChartValues<float> (Seq.map mapFun series)
                chartSeries.Add(ls)

    let addChartSeries = SimpleCommand addChartSeriesFun

    (* add a few series for a good measure *)
    do
        addChartSeriesFun ()
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
        trades.CollectionChanged.Add calculateFun
        data.CollectionChanged.Add calculateFun
        calculationParameters.CollectionChanged.Add calculateFun

    (* commands *)
    member this.AddTrade = addTrade 
    member this.RemoveTrade = removeTrade
    member this.ClearTrades = clearTrades
    member this.Calculate = calculate

    member this.AddMarketData = addMarketDataRecord
    member this.RemoveMarketData = removeMarketDataRecord
    member this.ClearMarketData = clearMarketDataRecord
    
    member this.AddCalcParameter = addCalcParameterRecord 
    member this.RemoveCalcParameter = removeCalcParameterRecord 
    member this.ClearCalcParameter = clearCalcParameterRecord 


    (* data fields *)
    member this.Trades = trades
    member this.Data = data
    member this.CalculationParameters = calculationParameters
    member this.Summary = summary

    (* charting *)

    member this.ChartSeries = chartSeries
    member this.AddChartSeries = addChartSeries