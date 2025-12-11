module simpleStore.Domain

open System

type Product = {
    Id: int
    Name: string
    Price: decimal
    Category: string
}

type CartItem = {
    Product: Product
    Quantity: int
}

type Receipt = { Date: DateTime; Items: CartItem list; Total: decimal }

type StoreState = {
    Catalog: Map<int, Product>
    Cart: CartItem list
}