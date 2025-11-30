open simpleStore

open Domain
open Services
open UI

[<EntryPoint>]
let main argv =
    let initialState = {
        Catalog = Catalog.initCatalog()
        Cart = []
    }
    
    printfn "Welcome to the F# Simple Store!"
    
    UI.createMainWindow initialState
    
    0