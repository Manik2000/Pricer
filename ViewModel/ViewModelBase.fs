namespace ViewModel

open System.ComponentModel

//WPF-friendly ViewModelBase
type ViewModelBase () =
    let propertyChanged = Event<PropertyChangedEventHandler,PropertyChangedEventArgs>()
    member this.Notify propertyName = propertyChanged.Trigger(this,PropertyChangedEventArgs(propertyName))
    interface INotifyPropertyChanged with
        [<CLIEvent>]
        member x.PropertyChanged = propertyChanged.Publish
        