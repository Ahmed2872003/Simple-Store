open simpleStore

open Domain
open Services

[<EntryPoint>]
let main argv =
    let initialState = {
        Catalog = Catalog.initCatalog()
        Cart = []
    }
    
    printfn "Welcome to the F# Simple Store!"
    
    UI.mainLoop initialState
    
    0