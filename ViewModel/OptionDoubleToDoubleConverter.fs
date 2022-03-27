namespace ViewModel

open System
open System.Windows.Data
open System.Globalization
    
(* 
   Converter class to change the way how optional values are displayed. 
   Here we want to show empty string if the value is missing, and the string rendering of the value otherwise.

   What we don't want is to display Some .../None, and this class is handling this.
*)
type OptionDoubleToDoubleConverter() =
    interface IValueConverter with
        member this.Convert (value : obj, _ : Type, _ : obj, _ : CultureInfo) =
            match value with
             | null -> null
             | :? option<Money> as ovalue -> 
                match ovalue with 
                | Some v -> box v 
                | None -> null

        member this.ConvertBack (_ : obj, _ : Type, _ : obj, _ : CultureInfo) =
            raise( new NotImplementedException("OptionDoubleToDoubleConverter.ConvertBack not implemented"))