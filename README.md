[README.txt](https://github.com/user-attachments/files/35037119/README.txt)
==============================================================
  MANG INASAR SELF-SERVICE KIOSK  (C# Console App)
==============================================================
A console-based simulation of a Mang Inasar self-service
ordering kiosk (classroom project, C# / .NET).

Customers browse the menu, build an order, pay with cash, and
get a receipt. Cashiers log in to look up past orders and view
a sales report. All completed orders are saved to a CSV file.


FEATURES
--------------------------------------------------------------
Customer View
  - Browse the menu by 6 categories (Ihaw-Sarap Savers, Grilled
    Pork, What's New, Chicken Inasal, Palabok, Halo-Halo)
  - Add items with a quantity (1-99 per item)
  - Live cart counter and running total on every screen
  - Review the order, add more items, or cancel (Y/N confirm)
  - Cash payment with change computation
  - Official receipt shown after payment
  - Empty orders can never reach payment

Cashier View (password protected)
  - Find an order by Order ID (e.g. ORD0001, not case-sensitive)
    and reprint its receipt
  - Sales report: number of completed orders and total sales

Data and Safety
  - Orders are saved to a CSV file with sequential Order IDs
    (ORD0001, ORD0002, ...)
  - If saving fails, the sale is NOT counted. The user can retry
    or go back (no duplicate records)
  - All input is validated (quantity, cash, menu choices, Y/N)
  - Cashier login is limited to 3 attempts; the password is
    masked with *
  - Handles closed input (Ctrl+Z / Ctrl+D) without crashing


MENU
--------------------------------------------------------------
 #   Item                          Category             Price (PHP)
 --  ----------------------------  -------------------  -----------
 1   Chicken Inasal Regular        Ihaw-Sarap Savers         124.00
 2   Pork Sisig                    Ihaw-Sarap Savers         124.00
 3   Bangus Sisig                  Ihaw-Sarap Savers         154.00
 4   Pork BBQ                      Grilled Pork              124.00
 5   Grilled Liempo                Grilled Pork              154.00
 6   Solo Fiesta: 2pc Pork BBQ     What's New                214.00
 7   Chicken Inasal Paa            What's New                219.00
 8   Paa Large Solo                Chicken Inasal            154.00
 9   Pecho Large                   Chicken Inasal            179.00
 10  Family Size                   Palabok                   305.00
 11  Extra Creamy Halo-Halo        Halo-Halo                  39.00
 12  Crema de Leche Halo-Halo      Halo-Halo                  39.00


GETTING STARTED
--------------------------------------------------------------
Requirements
  - .NET SDK 6.0 or newer (uses implicit usings)
  - Visual Studio 2022 OR any terminal

Run with Visual Studio
  1. Open the solution (.sln) file.
  2. Press F5 (or click Start).

Run from the terminal
  cd Mang_Inasar_Kiosk_ConsoleApp
  dotnet run

Set the records file location
  Orders are saved to the path defined at the top of Program.cs:

  const string RecordsFilePath = @"C:\Users\AJ\Downloads\MangInsar.csv";

  Change this to a folder that exists on your computer. The
  folder is created automatically if it is missing, and the file
  is created on the first saved order.


HOW TO USE
--------------------------------------------------------------
Main Menu
  [1]  Customer View
  [2]  Cashier View
  [3]  Exit

As a Customer
  1. Choose 1 from the main menu.
  2. Enter a category number (1-6).
  3. Enter an item number, then the quantity.
  4. Repeat to add more items. Use B to go back to categories.
  5. Press C to review your order.
  6. In the review screen:
       A = add more items
       P = proceed to payment
       X = cancel the order
  7. Enter the cash amount (e.g. 500 or 500.50). If it is not
     enough, enter the full amount again.
  8. Your receipt is displayed, and the order is saved.

  Keys:
    C   Review order / checkout
    X   Cancel order (asks for confirmation if cart has items)
    B   Go back

As a Cashier
  1. Choose 2 from the main menu.
  2. Log in (3 attempts allowed; type B as the username to go
     back).

       Username: MangInasar123
       Password: MangInasar111326

  3. Pick an option:
       1 = Find an order by ID and view its receipt
       2 = View the sales report
       B = Back to the main menu

  NOTE: The credentials are hard-coded for classroom demo
  purposes only. A real system would never store passwords in
  source code.


SAVED RECORDS FORMAT
--------------------------------------------------------------
Each item in an order is one row. Rows with the same Order ID
belong to the same order.

OrderId,Status,ItemName,UnitPrice,Quantity,Subtotal,Total,Cash,Change
ORD0001,PAID,Pork BBQ,124.00,2,248.00,287.00,300.00,13.00
ORD0001,PAID,Extra Creamy Halo-Halo,39.00,1,39.00,287.00,300.00,13.00

  - Money values are plain numbers (no PHP label or commas).
  - Only rows with status PAID count toward the sales report.
  - Unreadable lines are skipped, and the report tells you how
    many were skipped.


CODE OVERVIEW
--------------------------------------------------------------
Everything lives in Program.cs, organized into sections:

  Entry and main menu   Main, RunMainMenu, ShowMainMenu
  Customer flow         RunCustomerView, RunCategory,
                        AddItemToCart, ReviewOrder
  Payment               ProcessPayment, SaveWithRetry,
                        BuildOrderFromCart, ShowReceiptScreen
  Cashier flow          CashierLogin, RunCashierView, FindOrder,
                        ShowSales
  File handling         TrySaveOrder, TryLoadOrders,
                        GetNextOrderId, IsFileError
  Calculations          CalculateSubtotal, CalculateTotal,
                        CountCartItems
  Input and validation  ReadInput, ReadPassword, TryParseQuantity,
                        TryParseCash, TryGetCategoryIndex,
                        TryGetItemInCategory

Data structures
  - Parallel arrays hold the menu: menuNames, menuPrices,
    menuCategory
  - cartQuantities[] stores the quantity of each menu item (same
    index as the menu arrays)
  - PaidOrder class represents a completed order (items, prices,
    quantities, total, cash, change)
  - PaymentResult enum: Paid, Cancelled, BackToOrder
  - InputClosedException: custom exception for closed input

Design notes
  - Screens are redrawn each step using helper methods
    (BeginScreen, DrawHeader, DrawRow, EndScreen, ...) for a
    clean kiosk-style UI.
  - File operations use try/catch with a filter
    (when IsFileError(ex)) so only expected file errors are
    caught.
  - A failed save writes nothing, so retrying cannot create
    duplicate records.


CUSTOMIZING
--------------------------------------------------------------
  To change...            Edit...
  ----------------------  ------------------------------------
  Menu items / prices     menuNames, menuPrices, menuCategory
                          (keep all three the same length)
  Categories              categoryNames
  Max quantity per item   MaxQuantity
  Cashier login           CashierUsername, CashierPassword,
                          MaxLoginAttempts
  Records file            RecordsFilePath
