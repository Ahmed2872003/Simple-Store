module simpleStore.UI

open System
open System.Windows.Forms
open System.Drawing
open Domain
open Services

// 1. DEFINE HELPER TYPES AT THE TOP
// This wrapper helps display the receipt nicely in the ListBox
type ReceiptDisplayItem = {
    Date: DateTime
    Items: CartItem list
    Display: string
}

// 2. STYLING HELPER
let styleGrid (grid: DataGridView) =
    grid.BackgroundColor <- Color.WhiteSmoke
    grid.BorderStyle <- BorderStyle.None
    grid.CellBorderStyle <- DataGridViewCellBorderStyle.SingleHorizontal
    grid.ColumnHeadersBorderStyle <- DataGridViewHeaderBorderStyle.None
    grid.ColumnHeadersDefaultCellStyle.BackColor <- Color.FromArgb(20, 20, 20)
    grid.ColumnHeadersDefaultCellStyle.ForeColor <- Color.White
    grid.ColumnHeadersDefaultCellStyle.Font <- new Font("Segoe UI", 9.0f, FontStyle.Bold)
    grid.EnableHeadersVisualStyles <- false
    grid.SelectionMode <- DataGridViewSelectionMode.FullRowSelect
    grid.MultiSelect <- false
    grid.RowHeadersVisible <- false
    grid.AutoSizeColumnsMode <- DataGridViewAutoSizeColumnsMode.Fill
    grid.AlternatingRowsDefaultCellStyle.BackColor <- Color.FromArgb(238, 239, 249)
    grid.DefaultCellStyle.SelectionBackColor <- Color.DarkTurquoise
    grid.DefaultCellStyle.SelectionForeColor <- Color.WhiteSmoke
    grid.DefaultCellStyle.Font <- new Font("Segoe UI", 9.0f)
    grid.ReadOnly <- true
    grid.AllowUserToAddRows <- false

// 3. MAIN WINDOW
let createMainWindow (initialState: StoreState) =
    let form = new Form(Text = "Simple Store Professional", Width = 1000, Height = 700)
    form.Font <- new Font("Segoe UI", 9.0f)
    
    // Use 'ref' to hold state
    let currentState = ref initialState

    // --- MAIN TABS ---
    let tabControl = new TabControl(Dock = DockStyle.Fill)
    let tabStore = new TabPage("Store")
    let tabReceipts = new TabPage("Order History")
    tabControl.Controls.Add(tabStore)
    tabControl.Controls.Add(tabReceipts)
    form.Controls.Add(tabControl)

    // =================================================================================
    // TAB 1: STORE INTERFACE
    // =================================================================================
    
    let splitContainer = new SplitContainer(Dock = DockStyle.Fill, SplitterDistance = 600)
    tabStore.Controls.Add(splitContainer)

    // --- LEFT SIDE: CATALOG ---
    let panelCatalog = new Panel(Dock = DockStyle.Fill, Padding = Padding(10))
    splitContainer.Panel1.Controls.Add(panelCatalog)

    // Search Bar
    let panelSearch = new Panel(Dock = DockStyle.Top, Height = 40)
    let txtSearch = new TextBox(PlaceholderText = "Search products...", Width = 300, Top = 5)
    let btnSearch = new Button(Text = "Search", Left = 310, Width = 80, Top = 4, BackColor = Color.LightGray)
    panelSearch.Controls.Add(txtSearch)
    panelSearch.Controls.Add(btnSearch)

    // Catalog Grid
    let gridCatalog = new DataGridView(Dock = DockStyle.Fill)
    styleGrid gridCatalog
    gridCatalog.Columns.Add("Id", "ID") |> ignore
    gridCatalog.Columns.Add("Name", "Product Name") |> ignore
    gridCatalog.Columns.Add("Category", "Category") |> ignore
    gridCatalog.Columns.Add("Price", "Price") |> ignore
    gridCatalog.Columns.[0].Width <- 50 

    // Add Button
    let panelAdd = new Panel(Dock = DockStyle.Bottom, Height = 50, Padding = Padding(0, 10, 0, 0))
    let btnAdd = new Button(Text = "Add to Cart", Dock = DockStyle.Right, Width = 150, BackColor = Color.MediumSeaGreen, ForeColor = Color.White, Font = new Font("Segoe UI", 10.0f, FontStyle.Bold))
    panelAdd.Controls.Add(btnAdd)

    panelCatalog.Controls.Add(gridCatalog)
    panelCatalog.Controls.Add(panelSearch)
    panelCatalog.Controls.Add(panelAdd)
    panelAdd.SendToBack()
    panelSearch.SendToBack()

    // --- RIGHT SIDE: CART ---
    let panelCart = new Panel(Dock = DockStyle.Fill, Padding = Padding(10))
    splitContainer.Panel2.Controls.Add(panelCart)

    let lblCartTitle = new Label(Text = "Shopping Cart", Dock = DockStyle.Top, Height = 30, Font = new Font("Segoe UI", 12.0f, FontStyle.Bold))
    
    // Cart Grid
    let gridCart = new DataGridView(Dock = DockStyle.Fill)
    styleGrid gridCart
    gridCart.Columns.Add("Name", "Item") |> ignore
    gridCart.Columns.Add("Price", "Price") |> ignore
    gridCart.Columns.Add("Qty", "Qty") |> ignore
    gridCart.Columns.Add("Total", "Subtotal") |> ignore
    gridCart.Columns.[2].Width <- 40

    // Cart Footer
    let panelCartFooter = new Panel(Dock = DockStyle.Bottom, Height = 100)
    let lblTotal = new Label(Text = "$0.00", Dock = DockStyle.Top, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 14.0f, FontStyle.Bold))
    
    let btnRemove = new Button(Text = "Remove Item", Width = 100, Height = 35, Top = 40, Left = 0, BackColor = Color.IndianRed, ForeColor = Color.White)
    let btnCheckout = new Button(Text = "Checkout", Width = 120, Height = 35, Top = 40, Left = 110, BackColor = Color.DodgerBlue, ForeColor = Color.White)
    
    btnCheckout.Left <- panelCartFooter.Width - btnCheckout.Width
    btnRemove.Left <- btnCheckout.Left - btnRemove.Width - 10
    btnCheckout.Anchor <- AnchorStyles.Top ||| AnchorStyles.Right
    btnRemove.Anchor <- AnchorStyles.Top ||| AnchorStyles.Right

    panelCartFooter.Controls.Add(btnRemove)
    panelCartFooter.Controls.Add(btnCheckout)
    panelCartFooter.Controls.Add(lblTotal)

    panelCart.Controls.Add(gridCart)
    panelCart.Controls.Add(lblCartTitle)
    panelCart.Controls.Add(panelCartFooter)
    panelCartFooter.SendToBack()


    // =================================================================================
    // TAB 2: RECEIPTS INTERFACE
    // =================================================================================
    
    let splitReceipts = new SplitContainer(Dock = DockStyle.Fill, SplitterDistance = 300)
    tabReceipts.Controls.Add(splitReceipts)

    // Left: List
    let groupReceiptList = new GroupBox(Text = "Past Receipts", Dock = DockStyle.Fill, Padding = Padding(10))
    let listReceipts = new ListBox(Dock = DockStyle.Fill, FormattingEnabled = true)
    listReceipts.Font <- new Font("Consolas", 10.0f) 
    groupReceiptList.Controls.Add(listReceipts)
    splitReceipts.Panel1.Controls.Add(groupReceiptList)

    // Right: Details
    let groupReceiptDetails = new GroupBox(Text = "Receipt Details", Dock = DockStyle.Fill, Padding = Padding(10))
    let gridReceiptItems = new DataGridView(Dock = DockStyle.Fill)
    styleGrid gridReceiptItems
    gridReceiptItems.Columns.Add("Name", "Item") |> ignore
    gridReceiptItems.Columns.Add("Category", "Category") |> ignore
    gridReceiptItems.Columns.Add("Qty", "Qty") |> ignore
    gridReceiptItems.Columns.Add("Price", "Price") |> ignore
    
    let lblReceiptTotal = new Label(Text = "Total: $0.00", Dock = DockStyle.Bottom, Height = 30, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 11.0f, FontStyle.Bold))
    
    groupReceiptDetails.Controls.Add(gridReceiptItems)
    groupReceiptDetails.Controls.Add(lblReceiptTotal)
    splitReceipts.Panel2.Controls.Add(groupReceiptDetails)

    // =================================================================================
    // LOGIC
    // =================================================================================

    // --- REFRESH STORE TAB ---
    let refreshStoreUI () =
        gridCart.Rows.Clear()
        currentState.Value.Cart |> List.iter (fun item -> 
            let subtotal = item.Product.Price * (decimal item.Quantity)
            gridCart.Rows.Add(item.Product.Name, item.Product.Price, item.Quantity, subtotal) |> ignore
        )
        let total = Pricing.calculateTotal currentState.Value.Cart
        lblTotal.Text <- sprintf "Total: $%.2M" total

    // --- LOAD CATALOG ---
    let loadCatalog products =
        gridCatalog.Rows.Clear()
        products |> List.iter (fun p ->
            let rowIndex = gridCatalog.Rows.Add(p.Id, p.Name, p.Category, p.Price)
            gridCatalog.Rows.[rowIndex].Tag <- p
        )

    currentState.Value.Catalog |> Map.toList |> List.map snd |> loadCatalog

    // --- LOAD RECEIPTS ---
    let refreshReceiptsUI () =
        listReceipts.Items.Clear()
        try 
            let receipts = FileIO.loadReceipts "./receipts.json"
            
            // Filter out any completely null receipts just in case
            let validReceipts = receipts |> List.filter (fun r -> box r <> null)
            let sorted = validReceipts |> List.sortByDescending (fun r -> r.Date)
            
            sorted |> List.iter (fun r -> 
                // SAFETY CHECK: If Items is null, treat count as 0
                let count = if box r.Items = null then 0 else r.Items.Length
                
                let displayStr = sprintf "%s - (%d items)" (r.Date.ToString("yyyy-MM-dd HH:mm")) count
                
                // Ensure we don't pass null Items to the display wrapper
                let safeItems = if box r.Items = null then [] else r.Items
                
                let item = { Date = r.Date; Items = safeItems; Display = displayStr }
                listReceipts.Items.Add(item) |> ignore
            )
            listReceipts.DisplayMember <- "Display"
        with
        | ex -> 
            MessageBox.Show("Error loading receipts: " + ex.Message) |> ignore

    // --- EVENT HANDLERS ---

    btnSearch.Click.Add(fun _ -> 
        let term = txtSearch.Text
        let results = Search.searchProducts term currentState.Value.Catalog
        loadCatalog results
    )

    btnAdd.Click.Add(fun _ ->
        if gridCatalog.SelectedRows.Count > 0 then
            // Use .Item(0) for safety
            let selectedRow = gridCatalog.SelectedRows.Item(0)
            // Explicit cast with parens to avoid syntax error
            let product = (selectedRow.Tag :?> Product)
            
            let newCart = Cart.addToCart product currentState.Value.Cart

            FileIO.saveCart(newCart, "./cart.json") 
            currentState.Value <- { currentState.Value with Cart = newCart }
            refreshStoreUI()
        else
            MessageBox.Show("Please select a product.") |> ignore
    )

    btnRemove.Click.Add(fun _ ->
        if gridCart.SelectedRows.Count > 0 then
            let index = gridCart.SelectedRows.[0].Index
            // Bounds check
            if index < currentState.Value.Cart.Length then
                let item = currentState.Value.Cart.[index]
                let newCart = Cart.removeFromCart item.Product.Id currentState.Value.Cart
                FileIO.saveCart(newCart, "./cart.json") 
                currentState.Value <- { currentState.Value with Cart = newCart }
                refreshStoreUI()
        else
            MessageBox.Show("Select an item to remove.") |> ignore
    )

    btnCheckout.Click.Add(fun _ ->
        let total = Pricing.calculateTotal currentState.Value.Cart
        if total > 0.0m then
            FileIO.saveReceipt currentState.Value.Cart total "./receipts.json"
            FileIO.saveCart([], "./cart.json") 
            MessageBox.Show(sprintf "Payment Successful!\nReceipt generated for $%.2M" total) |> ignore
            
            currentState.Value <- { currentState.Value with Cart = [] }
            refreshStoreUI()
            refreshReceiptsUI()
        else
            MessageBox.Show("Cart is empty.") |> ignore
    )

    listReceipts.SelectedIndexChanged.Add(fun _ ->
        if listReceipts.SelectedItem <> null then
            // Use safe cast with 'match' or 'try cast' if preferred, but direct cast works here
            match listReceipts.SelectedItem with
            | :? ReceiptDisplayItem as receipt ->
                gridReceiptItems.Rows.Clear()
                let mutable total = 0.0m
                
                receipt.Items |> List.iter (fun item ->
                    gridReceiptItems.Rows.Add(item.Product.Name, item.Product.Category, item.Quantity, item.Product.Price) |> ignore
                    total <- total + (decimal item.Quantity * item.Product.Price)
                )
                lblReceiptTotal.Text <- sprintf "Total: $%.2M" total
            | _ -> ()
    )

    refreshStoreUI()
    refreshReceiptsUI()
    Application.Run(form)