namespace ViewModel

open System.Windows.Input
open System

//WPF-friendly simple command, can always execute
type SimpleCommand(action :  obj -> unit) = 
    let canExecuteChanged = new Event<EventHandler, EventArgs>()
    interface ICommand with
        member this.CanExecute(parameter: obj): bool = true
        [<CLIEvent>]
        member this.CanExecuteChanged: IEvent<System.EventHandler,System.EventArgs> = canExecuteChanged.Publish
        member this.Execute(parameter: obj): unit = action parameter