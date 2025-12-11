module simpleStore.Services

open System
open System.IO
open System.Text.Json
open Domain 

module Catalog =
    let initialProducts = [
        // Electronics
        { Id = 1; Name = "Gaming Laptop"; Price = 1200.00m; Category = "Electronics" }
        { Id = 2; Name = "Mechanical Keyboard"; Price = 85.50m; Category = "Electronics" }
        { Id = 3; Name = "Wireless Mouse"; Price = 45.99m; Category = "Electronics" }
        { Id = 4; Name = "27-inch Monitor"; Price = 299.00m; Category = "Electronics" }
        { Id = 5; Name = "USB-C Hub"; Price = 25.00m; Category = "Electronics" }
        { Id = 6; Name = "Noise Cancelling Headphones"; Price = 199.99m; Category = "Electronics" }
        { Id = 7; Name = "Smartphone Stand"; Price = 15.00m; Category = "Electronics" }
        { Id = 8; Name = "Webcam 1080p"; Price = 59.99m; Category = "Electronics" }
        { Id = 9; Name = "HDMI Cable (2m)"; Price = 12.50m; Category = "Electronics" }
        { Id = 10; Name = "External SSD 1TB"; Price = 110.00m; Category = "Electronics" }
        { Id = 11; Name = "Smart Watch"; Price = 250.00m; Category = "Electronics" }
        { Id = 12; Name = "Tablet Pro"; Price = 800.00m; Category = "Electronics" }
        { Id = 13; Name = "Graphics Card RTX 4060"; Price = 350.00m; Category = "Electronics" }
        { Id = 14; Name = "32GB RAM Kit"; Price = 120.00m; Category = "Electronics" }
        
        // Books
        { Id = 15; Name = "F# Programming Book"; Price = 45.00m; Category = "Books" }
        { Id = 16; Name = "Clean Code"; Price = 40.00m; Category = "Books" }
        { Id = 17; Name = "Introduction to Algorithms"; Price = 85.00m; Category = "Books" }
        { Id = 18; Name = "The Pragmatic Programmer"; Price = 38.00m; Category = "Books" }
        { Id = 19; Name = "Design Patterns"; Price = 52.00m; Category = "Books" }
        { Id = 20; Name = "Sci-Fi Novel"; Price = 19.99m; Category = "Books" }

        // Home & Office
        { Id = 21; Name = "Coffee Mug"; Price = 12.00m; Category = "Home" }
        { Id = 22; Name = "Ergonomic Office Chair"; Price = 150.00m; Category = "Home" }
        { Id = 23; Name = "LED Desk Lamp"; Price = 35.00m; Category = "Home" }
        { Id = 24; Name = "Stainless Steel Water Bottle"; Price = 22.00m; Category = "Home" }
        { Id = 25; Name = "Moleskine Notebook"; Price = 18.00m; Category = "Home" }
        { Id = 26; Name = "Gel Pen Set (12-pack)"; Price = 10.50m; Category = "Home" }
        { Id = 27; Name = "Desk Mat"; Price = 25.00m; Category = "Home" }

        // Gaming
        { Id = 28; Name = "Gaming Controller"; Price = 60.00m; Category = "Gaming" }
        { Id = 29; Name = "VR Headset"; Price = 400.00m; Category = "Gaming" }
        { Id = 30; Name = "Gaming Chair"; Price = 180.00m; Category = "Gaming" }
    ]
    let initCatalog () =
        initialProducts
        |> List.map (fun p -> (p.Id, p))
        |> Map.ofList

    let getProduct id catalog = Map.tryFind id catalog


module Cart =
    let addToCart (product: Product) (cart: CartItem list) =

        match cart |> List.tryFind (fun item -> item.Product.Id = product.Id) with
        | Some existingItem ->
            let newItem = { existingItem with Quantity = existingItem.Quantity + 1 }
            cart |> List.map (fun item -> if item.Product.Id = product.Id then newItem else item)
        | None ->
            { Product = product; Quantity = 1 } :: cart



    let removeFromCart (productId: int) (cart: CartItem list) =
        match cart |> List.tryFind (fun item -> item.Product.Id = productId) with
        | Some item when item.Quantity > 1 ->
            let newItem = { item with Quantity = item.Quantity - 1 }
            cart |> List.map (fun i -> if i.Product.Id = productId then newItem else i)
        | Some _ ->
            cart |> List.filter (fun item -> item.Product.Id <> productId)
        | None -> cart

module Pricing = 
    let calculateTotal (cart: CartItem list) =
        cart |> List.sumBy (fun item -> item.Product.Price * decimal item.Quantity)

module Search =
    let searchProducts (query: string) (catalog: Map<int, Product>) =
        catalog
        |> Map.toList
        |> List.map snd
        |> List.filter (fun p -> 
            p.Name.ToLower().Contains(query.ToLower()) || 
            p.Category.ToLower().Contains(query.ToLower())
        )

module FileIO =
    type Receipt = { Date: DateTime; Items: CartItem list; Total: decimal }

    let loadData<'T> (filePath: string) : 'T list =
        if File.Exists(filePath) then
            try
                let json = File.ReadAllText(filePath)
                
                if String.IsNullOrWhiteSpace(json) then 
                    [] 
                else 
                    JsonSerializer.Deserialize<'T list>(json)
            with
            | ex -> 
                printfn "Error loading from %s: %s" filePath ex.Message
                []
        else
            []

    let saveData<'T> (data: 'T list) (filePath: string) =
        try
            let options = JsonSerializerOptions(WriteIndented = true)
            let json = JsonSerializer.Serialize(data, options)
            File.WriteAllText(filePath, json)
        with
        | ex -> printfn "Error saving to %s: %s" filePath ex.Message


    let appendToFile<'T> (newItem: 'T) (filePath: string) =
        let existingItems = loadData<'T> filePath
        
        let updatedList = existingItems @ [newItem]
        
        saveData updatedList filePath



