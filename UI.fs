module simpleStore.UI

open System
open Domain
open Services

let printCatalog (products: Product list) =
    printfn "\n--- PRODUCT CATALOG ---"
    printfn "%-5s %-25s %-15s %-10s" "ID" "Name" "Category" "Price"
    printfn "%s" (String.replicate 60 "-")
    products |> List.iter (fun p ->
        printfn "%-5d %-25s %-15s $%.2f" p.Id p.Name p.Category p.Price
    )

let printCart (cart: CartItem list) =
    printfn "\n--- YOUR CART ---"
    if cart.IsEmpty then
        printfn "Your cart is empty."
    else
        cart |> List.iter (fun item ->
            printfn "%dx %-25s ($%.2f each)" item.Quantity item.Product.Name item.Product.Price
        )
        let total = Pricing.calculateTotal cart
        printfn "%s" (String.replicate 40 "-")
        printfn "TOTAL: $%.2f" total

let rec mainLoop (state: StoreState) =
    printfn "\nOptions: (1) View All (2) Search (3) Add (4) Remove (5) View Cart (6) Checkout (q) Quit"
    printf "> "
    match Console.ReadLine() with
    | "1" ->
        let allProducts = state.Catalog |> Map.toList |> List.map snd
        printCatalog allProducts
        mainLoop state
    
    | "2" ->
        printf "Enter search term: "
        let term = Console.ReadLine()
        let results = Search.searchProducts term state.Catalog
        printCatalog results
        mainLoop state

    | "3" ->
        printf "Enter Product ID to ADD: "
        match Int32.TryParse(Console.ReadLine()) with
        | true, id ->
            match Catalog.getProduct id state.Catalog with
            | Some prod ->
                let newCart = Cart.addToCart prod state.Cart
                printfn "Added %s to cart." prod.Name
                mainLoop { state with Cart = newCart }
            | None ->
                printfn "Product not found."
                mainLoop state
        | _ -> mainLoop state

    | "4" ->
        printf "Enter Product ID to REMOVE: "
        match Int32.TryParse(Console.ReadLine()) with
        | true, id ->
            let newCart = Cart.removeFromCart id state.Cart
            printfn "Updated cart."
            mainLoop { state with Cart = newCart }
        | _ -> mainLoop state

    | "5" ->
        printCart state.Cart
        mainLoop state

    | "6" ->
        printCart state.Cart
        let total = Pricing.calculateTotal state.Cart
        FileIO.saveReceipt state.Cart total
        printfn "Thank you for shopping! Exiting..."
        () // Stop recursion

    | "q" -> 
        printfn "Goodbye!"
        ()

    | _ -> 
        printfn "Invalid option."
        mainLoop state