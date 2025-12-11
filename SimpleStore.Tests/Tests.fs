namespace SimpleStore.Tests

open System
open System.IO
open Xunit
open Xunit.Abstractions
open FsUnit.Xunit
open simpleStore.Domain
open simpleStore.Services




// -----------------------------------------------------------
// 2. CLEAN TEST CLASS
// -----------------------------------------------------------
// The constructor now handles ONLY the output helper, satisfying xUnit.
type ServiceTests(output: ITestOutputHelper) =

    // Test Data
    let p1 = { Id = 1; Name = "Test Laptop"; Price = 1000.0m; Category = "Electronics" }
    let p2 = { Id = 2; Name = "Test Mouse"; Price = 50.0m; Category = "Electronics" }
    let p3 = { Id = 3; Name = "Coffee"; Price = 5.0m; Category = "Home" }

    // Helper to print to console
    member this.log (message: string) = 
        output.WriteLine(sprintf "   [LOG] %s" message)

    member this.pass (message: string) =
        output.WriteLine(sprintf " ✅ PASS: %s" message)

    // =========================================================
    // CART TESTS
    // =========================================================

    [<Fact>]
    member this.``Cart: Add new item should create entry with Quantity 1`` () =
        this.log "Starting Add Item Test..."
        
        let cart = []
        // Access data via the module name 'TestData'
        let updatedCart = Cart.addToCart p1 cart

        updatedCart |> should haveLength 1
        updatedCart.Head.Quantity |> should equal 1
        updatedCart.Head.Product.Name |> should equal "Test Laptop"
        
        this.pass "Item added correctly"

    [<Fact>]
    member this.``Cart: Add existing item should increment Quantity`` () =
        this.log "Starting Increment Item Test..."

        let cart = [{ Product = p1; Quantity = 1 }]

        let updatedCart = Cart.addToCart p1 cart

        updatedCart |> should haveLength 1
        updatedCart.Head.Quantity |> should equal 2
        
        this.pass "Quantity incremented to 2"

    [<Fact>]
    member this.``Cart: Remove item with Qty > 1 should decrement`` () =
        this.log "Starting Decrement Test..."

        let cart = [{ Product = p2; Quantity = 2 }]

        let updatedCart = Cart.removeFromCart 2 cart

        updatedCart |> should haveLength 1
        updatedCart.Head.Quantity |> should equal 1
        
        this.pass "Quantity decremented from 2 to 1"

    [<Fact>]
    member this.``Cart: Remove item with Qty 1 should remove from list`` () =
        this.log "Starting Removal Test..."

        let cart = [{ Product = p2; Quantity = 1 }]

        let updatedCart = Cart.removeFromCart 2 cart

        updatedCart |> should be Empty
        
        this.pass "Item completely removed from cart"

    // =========================================================
    // PRICING TESTS
    // =========================================================

    [<Fact>]
    member this.``Pricing: Calculate Total sums strictly correctly`` () =
        this.log "Starting Pricing Calculation..."

        let cart = [
            { Product = p1; Quantity = 1 }
            { Product = p2; Quantity = 2 }
        ]

        let total = Pricing.calculateTotal cart

        total |> should equal 1100.0m
        
        this.pass "Total calculated as 1100.0"

    // =========================================================
    // SEARCH TESTS
    // =========================================================

    [<Fact>]
    member this.``Search: Finds product by Name (Case Insensitive)`` () =
        this.log "Starting Search Test..."

        let catalog = [ (p1.Id, p1); (p2.Id, p2); (p3.Id, p3) ] |> Map.ofList

        let results = Search.searchProducts "laptop" catalog

        results |> should haveLength 1
        results.Head.Name |> should equal "Test Laptop"
        
        this.pass "Found 'Test Laptop' using query 'laptop'"

    [<Fact>]
    member this.``Search: Finds product by Category`` () =
        this.log "Starting Category Search..."

        let catalog = [ (p1.Id, p1); (p2.Id, p2); (p3.Id, p3) ] |> Map.ofList

        let results = Search.searchProducts "Home" catalog

        results |> should haveLength 1
        results.Head.Name |> should equal "Coffee"
        
        this.pass "Found 'Coffee' in 'Home' category"

    // =========================================================
    // FILE IO TESTS
    // =========================================================
    
    [<Fact>]
    member this.``FileIO: Save and Load Receipts`` () =
        this.log "Starting File IO Test..."

        let receiptFilePath = "receipts.json"
        // Clean up previous test runs
        if File.Exists(receiptFilePath) then File.Delete(receiptFilePath)

        let testCart = [{ Product = p3; Quantity = 5 }]
        let testTotal = 25.0m

        let newReceipt = { Date = DateTime.Now; Items = testCart; Total = testTotal }
        
        // Save
        this.log "Saving receipt to disk..."
        // FIX: Use 'appendToFile' because 'newReceipt' is a single item, not a list
        FileIO.appendToFile newReceipt receiptFilePath

        // Load
        this.log "Reading receipts from disk..."
        let loadedReceipts = FileIO.loadData<Receipt> receiptFilePath

        loadedReceipts |> should not' (be Empty)
        
        let lastReceipt = List.last loadedReceipts
        
        lastReceipt.Items |> should haveLength 1
        lastReceipt.Items.Head.Product.Name |> should equal "Coffee"
        
        this.pass "File Save & Load cycle successful"

    [<Fact>]
    member this.``FileIO: Save and Load Cart`` () =
        this.log "Starting File IO Cart Test..."

        let cartFilePath = "cart.json"
        
        if File.Exists(cartFilePath) then File.Delete(cartFilePath)

        // Arrange: Create a cart with 2 Laptops and 1 Mouse
        let testCart = [
            { Product = p1; Quantity = 2 }
            { Product = p2; Quantity = 1 }
        ]
        
        // Act 1: Save
        this.log "Saving cart to disk..."

        // FIX: Remove ( ) and , because F# functions are space-separated (Curried)
        FileIO.saveData testCart cartFilePath

        // Act 2: Load
        this.log "Reading cart from disk..."
        let loadedCart = FileIO.loadData<CartItem> cartFilePath

        // Assert
        loadedCart |> should not' (be Empty)
        loadedCart |> should haveLength 2
        
        let laptopEntry = loadedCart |> List.find (fun i -> i.Product.Id = p1.Id)
        laptopEntry.Quantity |> should equal 2
        
        this.pass "Cart Save & Load cycle successful"