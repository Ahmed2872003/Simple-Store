open simpleStore

open Domain
open Services
open UI

[<EntryPoint>]
let main argv =
    let initialState = {
        Catalog = Catalog.initCatalog()
        Cart = FileIO.loadData<CartItem> "./cart.json"
    }
    
    printfn "Welcome to the F# Simple Store!"
    
    UI.createMainWindow initialState
    
    0