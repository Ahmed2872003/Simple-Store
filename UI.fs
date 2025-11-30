module simpleStore.UI

open System
open System.Windows.Forms
open System.Drawing
open Domain
open Services

let createMainWindow (initialState: StoreState) =
    let form = new Form(Text = "Simple Store Simulator", Width = 800, Height = 600)
    
    // Use 'ref' to hold state
    let currentState = ref initialState

    // --- CONTROLS ---
    let splitContainer = new SplitContainer(Dock = DockStyle.Fill)
    form.Controls.Add(splitContainer)

    // Left Side: Catalog
    let lblCatalog = new Label(Text = "Product Catalog", Dock = DockStyle.Top, Height = 30)
    let listCatalog = new ListBox(Dock = DockStyle.Fill)
    let txtSearch = new TextBox(PlaceholderText = "Search...", Dock = DockStyle.Bottom)
    let btnSearch = new Button(Text = "Search", Dock = DockStyle.Bottom)
    let btnAdd = new Button(Text = "Add to Cart ->", Dock = DockStyle.Bottom, Height = 40, BackColor = Color.LightGreen)
    
    splitContainer.Panel1.Controls.Add(listCatalog)
    splitContainer.Panel1.Controls.Add(btnAdd)
    splitContainer.Panel1.Controls.Add(txtSearch) 
    splitContainer.Panel1.Controls.Add(btnSearch)
    splitContainer.Panel1.Controls.Add(lblCatalog)

    // Right Side: Cart
    let lblCart = new Label(Text = "Your Cart", Dock = DockStyle.Top, Height = 30)
    let listCart = new ListBox(Dock = DockStyle.Fill)
    let lblTotal = new Label(Text = "Total: $0.00", Dock = DockStyle.Bottom, Font = new Font("Arial", 12.0f, FontStyle.Bold))
    let btnRemove = new Button(Text = "Remove Selected", Dock = DockStyle.Bottom)
    let btnCheckout = new Button(Text = "Checkout & Save", Dock = DockStyle.Bottom, Height = 50, BackColor = Color.LightSkyBlue)

    splitContainer.Panel2.Controls.Add(listCart)
    splitContainer.Panel2.Controls.Add(btnRemove)
    splitContainer.Panel2.Controls.Add(lblTotal)
    splitContainer.Panel2.Controls.Add(btnCheckout)
    splitContainer.Panel2.Controls.Add(lblCart)

    // --- HELPER FUNCTIONS ---

    let formatProduct (p: Product) =
        sprintf "[%d] %s - $%0.2M (%s)" p.Id p.Name p.Price p.Category

    let formatCartItem (item: CartItem) =
        sprintf "%dx %s ($%0.2M)" item.Quantity item.Product.Name item.Product.Price

    // REFRESH FUNCTION
    let refreshUI () =
        listCart.Items.Clear()
        // FIX: Use .Value instead of !
        currentState.Value.Cart |> List.iter (fun item -> 
            listCart.Items.Add(formatCartItem item) |> ignore
        )
        
        let total = Pricing.calculateTotal currentState.Value.Cart
        lblTotal.Text <- sprintf "Total: $%.2M" total

    // Setup Catalog ListBox
    listCatalog.DisplayMember <- "Name" 
    
    listCatalog.Format.Add(fun (e: ListControlConvertEventArgs) ->
        match e.ListItem with
        | :? Product as p -> e.Value <- formatProduct p
        | _ -> ()
    )
    listCatalog.FormattingEnabled <- true

    // Load initial data
    // FIX: Use .Value instead of !
    currentState.Value.Catalog |> Map.iter (fun _ p -> listCatalog.Items.Add(p) |> ignore)

    // --- EVENT HANDLERS ---

    // 1. Search Logic
    btnSearch.Click.Add(fun _ -> 
        let term = txtSearch.Text
        let results = Search.searchProducts term currentState.Value.Catalog
        listCatalog.Items.Clear()
        results |> List.iter (fun p -> listCatalog.Items.Add(p) |> ignore)
    )

    // 2. Add to Cart Logic
    btnAdd.Click.Add(fun _ ->
        if listCatalog.SelectedItem <> null then
            let selectedProduct = listCatalog.SelectedItem :?> Product
            // FIX: Use .Value instead of !
            let newCart = Cart.addToCart selectedProduct currentState.Value.Cart
            
            // FIX: Use .Value <- ... instead of :=
            currentState.Value <- { currentState.Value with Cart = newCart }
            refreshUI()
        else
            MessageBox.Show("Please select a product first.") |> ignore
    )

    // 3. Remove from Cart Logic
    btnRemove.Click.Add(fun _ ->
        if listCart.SelectedIndex <> -1 then
            let item = currentState.Value.Cart.[listCart.SelectedIndex]
            let newCart = Cart.removeFromCart item.Product.Id currentState.Value.Cart
            
            // FIX: Use .Value <- ... instead of :=
            currentState.Value <- { currentState.Value with Cart = newCart }
            refreshUI()
        else
            MessageBox.Show("Please select an item in the cart to remove.") |> ignore
    )

    // 4. Checkout Logic
    btnCheckout.Click.Add(fun _ ->
        let total = Pricing.calculateTotal currentState.Value.Cart
        
        if total > 0.0m then
            FileIO.saveReceipt currentState.Value.Cart total
            MessageBox.Show(sprintf "Receipt Saved!\nTotal Paid: $%.2M" total) |> ignore
            
            // FIX: Use .Value <- ... instead of :=
            currentState.Value <- { currentState.Value with Cart = [] }
            refreshUI()
        else
            MessageBox.Show("Cart is empty.") |> ignore
    )

    refreshUI()
    Application.Run(form)