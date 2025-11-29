module simpleStore.Services

open System
open System.IO
open System.Text.Json
open Domain 

module Catalog =
    let initialProducts = [
        { Id = 1; Name = "Gaming Laptop"; Price = 1200.00m; Category = "Electronics" }
        { Id = 2; Name = "Mechanical Keyboard"; Price = 85.50m; Category = "Electronics" }
        { Id = 3; Name = "F# Programming Book"; Price = 45.00m; Category = "Books" }
        { Id = 4; Name = "Coffee Mug"; Price = 12.00m; Category = "Home" }
        { Id = 5; Name = "USB-C Hub"; Price = 25.00m; Category = "Electronics" }
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

    let saveReceipt (cart: CartItem list) (total: decimal) =
        let receipt = { Date = DateTime.Now; Items = cart; Total = total }
        let options = JsonSerializerOptions(WriteIndented = true)
        let json = JsonSerializer.Serialize(receipt, options)
        File.WriteAllText("receipt.json", json)