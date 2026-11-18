using System.Globalization;
using System.Text;

namespace Mang_Inasar_Kiosk_ConsoleApp
{
    internal class Program
    {
        class PaidOrder
        {
            public string OrderId = "";
            public string Status = "PAID";
            public List<string> ItemNames = new List<string>();
            public List<decimal> UnitPrices = new List<decimal>();
            public List<int> Quantities = new List<int>();
            public List<decimal> Subtotals = new List<decimal>();
            public decimal Total;
            public decimal Cash;
            public decimal Change;
        }

        /// <summary>Thrown when the keyboard input stream is closed (Ctrl+Z / Ctrl+D).</summary>
        class InputClosedException : Exception
        {
        }

        enum PaymentResult
        {
            Paid,
            Cancelled,
            BackToOrder
        }

        class Program
        {
            // =====================================================================
            //  1. SETTINGS AND MENU DATA
            // =====================================================================
            const string RecordsFilePath = @"C:\Users\AJ\Downloads\MangInsar.csv";

            // General settings
            const string RestaurantName = "MANG INASAR";
            const string Currency = "PHP";
            const int MaxQuantity = 99;
            const decimal MaxCash = 9999999.99m;

            // Cashier login
            const string CashierUsername = "MangInasar123";
            const string CashierPassword = "MangInasar111326";
            const int MaxLoginAttempts = 3;

            // Screen layout
            const int BoxInner = 62;        // characters between the box borders

            // CSV format
            const string CsvHeader = "OrderId,Status,ItemName,UnitPrice,Quantity,Subtotal,Total,Cash,Change";
            static readonly string[] categoryNames =
            {
            "IHAW-SARAP SAVERS With Drinks",
            "GRILLED PORK With Drinks",
            "WHAT'S NEW? Solo Fiesta / AlmuSOLB With Drinks",
            "CHICKEN INASAL With Drinks",
            "PALABOK",
            "HALO-HALO"
        };

            static readonly string[] menuNames =
            {
            "Chicken Inasal Regular",       //  Ihaw-Sarap Savers
            "Pork Sisig",                   //  Ihaw-Sarap Savers
            "Bangus Sisig",                 //  Ihaw-Sarap Savers
            "Pork BBQ",                     //  Grilled Pork 
            "Grilled Liempo",               //  Grilled Pork 
            "Solo Fiesta: 2pc Pork BBQ",    //  What's New 
            "Chicken Inasal Paa",           //  What's New
            "Paa Large Solo",               //  Chicken Inasal
            "Pecho Large",                  //  Chicken Inasal
            "Family Size",                  //  Palabok 
            "Extra Creamy Halo-Halo",       //  Halo-Halo 
            "Crema de Leche Halo-Halo"      //  Halo-Halo
        };

            static readonly decimal[] menuPrices =
            {
            124.00m, //  Ihaw-Sarap Savers
            124.00m, //  Ihaw-Sarap Savers
            154.00m, //  Ihaw-Sarap Savers
            124.00m, //  Grilled Pork
            154.00m, //  Grilled Pork
            214.00m, //  What's New 
            219.00m, //  What's New 
            154.00m, //  Chicken Inasal
            179.00m, //  Chicken Inasal
            305.00m, //  Palabok
            39.00m,  //  Halo-Halo
            39.00m   //  Halo-Halo
        };

            // Category index (into categoryNames) for each menu item.
            static readonly int[] menuCategory = { 0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 5, 5 };

            // Cart quantity for each menu item (same index as the menu arrays).
            static readonly int[] cartQuantities = new int[menuNames.Length];
        }

        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            try
            {
                RunMainMenu();
            }
            catch (InputClosedException)
            {
                Console.WriteLine();
                Console.WriteLine(" Input closed. Goodbye!");
            }
        }

        static void RunMainMenu()
        {
            bool running = true;
            string message = "";

            while (running)
            {
                ShowMainMenu(message);
                message = "";

                string choice = ReadInput("Enter your choice").ToUpperInvariant();

                switch (choice)
                {
                    case "1":
                        RunCustomerView();
                        break;
                    case "2":
                        if (CashierLogin())
                        {
                            RunCashierView();
                        }
                        break;
                    case "3":
                        running = false;
                        break;
                    default:
                        message = Error("Invalid choice. Please enter 1, 2 or 3.");
                        break;
                }
            }

            ShowMessageBox("Thank you! Goodbye.", waitForEnter: false);
        }

        static void ShowMainMenu(string message)
        {
            BeginScreen();
            DrawBanner(RestaurantName, "Order here. Ihaw Sarap Unli Sarap!");
            DrawOptions(
                "   [1]  Customer View",
                "   [2]  Cashier View",
                "   [3]  Exit");
            EndScreen(message);
        }


        static void RunCustomerView()
        {
            ClearCart();
            bool inCustomerView = true;
            string message = "";

            while (inCustomerView)
            {
                ShowCategoryScreen(message);
                message = "";

                string choice = ReadInput("Enter category number").ToUpperInvariant();

                if (choice == "C")
                {
                    // ReviewOrder returns true when the customer is finished (paid or cancelled).
                    inCustomerView = !ReviewOrder();
                }
                else if (choice == "X")
                {
                    // An empty cart has nothing to lose, so no confirmation is needed.
                    if (CountCartItems() == 0 || ConfirmCancel())
                    {
                        inCustomerView = false;
                    }
                }
                else if (TryGetCategoryIndex(choice, out int categoryIndex))
                {
                    inCustomerView = !RunCategory(categoryIndex);
                }
                else
                {
                    message = Error("Invalid choice. Enter a category number 1-" + categoryNames.Length
                                    + ", C to review, or X to cancel.");
                }
            }
        }

        /// <summary>
        /// Item list for one category. Returns true when the customer is finished
        /// (paid or cancelled), false to go back to the category list.
        /// </summary>
        static bool RunCategory(int categoryIndex)
        {
            string message = "";

            while (true)
            {
                ShowItemScreen(categoryIndex, message);
                message = "";

                string choice = ReadInput("Enter item number").ToUpperInvariant();

                if (choice == "B")
                {
                    return false;
                }

                if (choice == "C")
                {
                    if (ReviewOrder())
                    {
                        return true;
                    }
                }
                else if (TryGetItemInCategory(choice, categoryIndex, out int itemIndex))
                {
                    message = AddItemToCart(itemIndex);
                }
                else
                {
                    message = Error("Invalid choice. Enter one of the item numbers shown, B to go back, or C to review.");
                }
            }
        }

        static void ShowCategoryScreen(string message)
        {
            BeginScreen();
            DrawHeader("SELECT A CATEGORY", CartLabel());
            DrawBlank();

            for (int i = 0; i < categoryNames.Length; i++)
            {
                DrawLine(string.Format("  [{0}]  {1}", i + 1, categoryNames[i]));
            }

            DrawBlank();
            DrawRow("Current total:", FormatMoney(CalculateTotal()));
            DrawActions("  [C] Review order / checkout      [X] Cancel order");
            EndScreen(message);
        }

        static void ShowItemScreen(int categoryIndex, string message)
        {
            BeginScreen();
            DrawHeader(categoryNames[categoryIndex], CartLabel());
            DrawBlank();

            for (int i = 0; i < menuNames.Length; i++)
            {
                if (menuCategory[i] == categoryIndex)
                {
                    DrawLine(string.Format("  [{0,2}]  {1,-24}{2,16}", i + 1, menuNames[i], FormatMoney(menuPrices[i])));
                }
            }

            DrawBlank();
            DrawRow("Current total:", FormatMoney(CalculateTotal()));
            DrawActions("  [B] Back to categories           [C] Review order");
            EndScreen(message);
        }

        /// <summary>Asks for a quantity and adds it to the cart. Returns a message for the menu screen.</summary>
        static string AddItemToCart(int itemIndex)
        {
            while (true)
            {
                string input = ReadInput("Quantity of " + menuNames[itemIndex] + " (1-" + MaxQuantity + ", B = back)");

                if (input.Equals("B", StringComparison.OrdinalIgnoreCase))
                {
                    return "";
                }

                if (!TryParseQuantity(input, out int quantity))
                {
                    Console.WriteLine(Error("Invalid quantity. Enter a whole number from 1 to " + MaxQuantity + "."));
                    continue;
                }

                if (cartQuantities[itemIndex] + quantity > MaxQuantity)
                {
                    Console.WriteLine(Error("You already have " + cartQuantities[itemIndex] + " of "
                                            + menuNames[itemIndex] + ". The most allowed is " + MaxQuantity + "."));
                    continue;
                }

                cartQuantities[itemIndex] += quantity;
                return Info("Added " + quantity + " x " + menuNames[itemIndex] + ".");
            }
        }

        /// <summary>
        /// Order review loop. Returns true when the customer is finished
        /// (order paid or cancelled), false to go back to the menu.
        /// </summary>
        static bool ReviewOrder()
        {
            string message = "";

            while (true)
            {
                if (CountCartItems() == 0)
                {
                    // Empty orders can never reach payment.
                    ShowMessageBox("Your order is empty. Please add at least one item first.");
                    return false;
                }

                ShowReviewScreen(message);
                message = "";

                string choice = ReadInput("Enter your choice").ToUpperInvariant();

                switch (choice)
                {
                    case "A":
                        return false;

                    case "P":
                        PaymentResult result = ProcessPayment();
                        if (result == PaymentResult.Paid || result == PaymentResult.Cancelled)
                        {
                            return true;
                        }
                        break;      // BackToOrder: show the review again

                    case "X":
                        if (ConfirmCancel())
                        {
                            return true;
                        }
                        break;

                    default:
                        message = Error("Invalid choice. Enter A, P or X.");
                        break;
                }
            }
        }

        static void ShowReviewScreen(string message)
        {
            PaidOrder preview = BuildOrderFromCart(0m, 0m);

            BeginScreen();
            DrawHeader("YOUR ORDER");
            DrawOrderTable(preview);
            DrawRow("ORDER TOTAL", FormatMoney(preview.Total));
            DrawActions(
                "  [A] Add more items",
                "  [P] Confirm and proceed to payment",
                "  [X] Cancel order");
            EndScreen(message);
        }
        static PaymentResult ProcessPayment()
        {
            decimal total = CalculateTotal();
            string message = "";

            while (true)
            {
                ShowPaymentScreen(total, message);
                message = "";

                string input = ReadInput("Enter cash amount");

                if (input.Equals("X", StringComparison.OrdinalIgnoreCase))
                {
                    if (ConfirmCancel())
                    {
                        return PaymentResult.Cancelled;
                    }
                    continue;
                }

                if (input.Equals("B", StringComparison.OrdinalIgnoreCase))
                {
                    return PaymentResult.BackToOrder;
                }

                if (!TryParseCash(input, out decimal cash, out string cashError))
                {
                    message = Error(cashError);
                    continue;
                }

                if (cash < total)
                {
                    // Rejected attempt: nothing is remembered, the full amount must be re-entered.
                    message = Error("Cash is not enough. You still need " + FormatMoney(total - cash)
                                    + ". Enter the full amount again, or X to cancel.");
                    continue;
                }

                PaidOrder order = BuildOrderFromCart(cash, cash - total);

                if (SaveWithRetry(order))
                {
                    ShowReceiptScreen(order, "Thank you! Please take your receipt.");
                    ClearCart();    // ready for the next customer
                    return PaymentResult.Paid;
                }

                // Saving failed and the customer chose not to retry.
                return PaymentResult.BackToOrder;
            }
        }

        static void ShowPaymentScreen(decimal total, string message)
        {
            BeginScreen();
            DrawHeader("PAYMENT (CASH)");
            DrawBlank();
            DrawRow("Amount due:", FormatMoney(total));
            DrawBlank();
            DrawLine(" Enter the cash you are handing over, in pesos");
            DrawLine(" (up to 2 decimal places, e.g. 300 or 300.50).");
            DrawBlank();
            DrawActions("  [B] Back to order        [X] Cancel order");
            EndScreen(message);
        }

        /// <summary>
        /// Tries to save the order. On failure shows the error and lets the user retry
        /// or go back to the order. Returns true only when the order was saved.
        /// A failed attempt writes nothing, so a retry cannot create duplicate records.
        /// </summary>
        static bool SaveWithRetry(PaidOrder order)
        {
            while (true)
            {
                if (TrySaveOrder(order, out string saveError))
                {
                    return true;
                }

                ShowSaveFailedScreen(saveError);

                while (true)
                {
                    string choice = ReadInput("Enter your choice").ToUpperInvariant();

                    if (choice == "R")
                    {
                        break;      // outer loop tries to save again
                    }
                    if (choice == "B")
                    {
                        return false;
                    }
                    Console.WriteLine(Error("Invalid choice. Enter R or B."));
                }
            }
        }

        static void ShowSaveFailedScreen(string saveError)
        {
            BeginScreen();
            DrawHeader("! PAYMENT NOT COMPLETED");
            DrawLine(" The order could NOT be saved, so the sale was not recorded.");
            DrawLine(" Reason: " + Fit(saveError, 50));
            DrawBlank();
            DrawLine("  [R] Retry saving");
            DrawLine("  [B] Back to order (nothing was saved)");
            EndScreen();
        }

        /// <summary>Builds a PAID order from the current cart.</summary>
        static PaidOrder BuildOrderFromCart(decimal cash, decimal change)
        {
            PaidOrder order = new PaidOrder();

            for (int i = 0; i < menuNames.Length; i++)
            {
                if (cartQuantities[i] > 0)
                {
                    order.ItemNames.Add(menuNames[i]);
                    order.UnitPrices.Add(menuPrices[i]);
                    order.Quantities.Add(cartQuantities[i]);
                    order.Subtotals.Add(CalculateSubtotal(menuPrices[i], cartQuantities[i]));
                }
            }

            order.Total = CalculateTotal();
            order.Cash = cash;
            order.Change = change;
            return order;
        }

        static void ShowReceiptScreen(PaidOrder order, string footer)
        {
            BeginScreen();
            DrawBanner(RestaurantName, "OFFICIAL RECEIPT");
            DrawRow("Order ID: " + order.OrderId, "Status: " + order.Status);
            DrawDivider();
            DrawOrderTable(order);
            DrawRow("TOTAL", FormatMoney(order.Total));
            DrawRow("CASH", FormatMoney(order.Cash));
            DrawRow("CHANGE", FormatMoney(order.Change));
            DrawDivider();
            DrawCentered(footer);
            EndScreen();
            Pause();
        }

        static bool CashierLogin()
        {
            string message = "";

            for (int attempt = 1; attempt <= MaxLoginAttempts; attempt++)
            {
                ShowLoginScreen(attempt, message);
                message = "";

                string username = ReadInput("Username");

                if (username.Equals("B", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                string password = ReadPassword("Password");

                if (username == CashierUsername && password == CashierPassword)
                {
                    return true;
                }

                message = Error("Incorrect username or password.");
            }

            ShowMessageBox("Too many failed attempts. Returning to the main menu.");
            return false;
        }

        static void ShowLoginScreen(int attempt, string message)
        {
            BeginScreen();
            DrawHeader("CASHIER LOGIN");
            DrawBlank();
            DrawLine(" Enter your username and password to continue.");
            DrawBlank();
            DrawActions("  Attempt " + attempt + " of " + MaxLoginAttempts + "      (type B as username to go back)");
            EndScreen(message);
        }

        static void RunCashierView()
        {
            bool inCashierView = true;
            string message = "";

            while (inCashierView)
            {
                ShowCashierMenu(message);
                message = "";

                string choice = ReadInput("Enter your choice").ToUpperInvariant();

                switch (choice)
                {
                    case "1":
                        FindOrder();
                        break;
                    case "2":
                        ShowSales();
                        break;
                    case "B":
                        inCashierView = false;
                        break;
                    default:
                        message = Error("Invalid choice. Enter 1, 2 or B.");
                        break;
                }
            }
        }

        static void ShowCashierMenu(string message)
        {
            BeginScreen();
            DrawHeader("CASHIER VIEW");
            DrawOptions(
                "   [1]  Find an order by Order ID",
                "   [2]  View sales report",
                "   [B]  Back to main menu");
            EndScreen(message);
        }

        static void FindOrder()
        {
            string message = "";

            while (true)
            {
                ShowFindOrderScreen(message);
                message = "";

                string input = ReadInput("Enter Order ID").ToUpperInvariant();

                if (input == "B")
                {
                    return;
                }

                if (input.Length == 0)
                {
                    message = Error("Order ID cannot be blank. Example: ORD0001.");
                    continue;
                }

                if (!TryLoadOrders(out List<PaidOrder> orders, out _, out string loadError))
                {
                    message = Error("Could not read the records: " + loadError);
                    continue;
                }

                PaidOrder found = FindOrderById(orders, input);

                if (found == null)
                {
                    message = Error("Order not found: " + Fit(input, 30));
                    continue;
                }

                ShowReceiptScreen(found, "Saved record (read from file)");
            }
        }

        static void ShowFindOrderScreen(string message)
        {
            BeginScreen();
            DrawHeader("FIND ORDER");
            DrawBlank();
            DrawLine(" Type an Order ID such as ORD0001 (not case-sensitive).");
            DrawBlank();
            DrawActions("  [B] Back to cashier menu");
            EndScreen(message);
        }

        /// <summary>Returns the order with the given ID, or null when there is none.</summary>
        static PaidOrder FindOrderById(List<PaidOrder> orders, string orderId)
        {
            foreach (PaidOrder order in orders)
            {
                if (order.OrderId.Equals(orderId, StringComparison.OrdinalIgnoreCase))
                {
                    return order;
                }
            }
            return null;
        }

        static void ShowSales()
        {
            BeginScreen();
            DrawHeader("SALES REPORT");
            DrawBlank();

            if (!TryLoadOrders(out List<PaidOrder> orders, out int skipped, out string loadError))
            {
                DrawLine(" ! Could not read the records:");
                DrawLine("   " + Fit(loadError, 56));
                DrawBlank();
                EndScreen();
                Pause();
                return;
            }

            // Sales = sum of order totals (not cash or change).
            decimal totalSales = 0m;
            foreach (PaidOrder order in orders)
            {
                totalSales += order.Total;
            }

            DrawRow("Completed orders:", orders.Count.ToString(CultureInfo.InvariantCulture));
            DrawRow("Total sales:", FormatMoney(totalSales));
            DrawBlank();

            if (orders.Count == 0)
            {
                DrawCentered("No sales yet");
                DrawBlank();
            }

            if (skipped > 0)
            {
                DrawLine(" Note: " + skipped + " unreadable line(s) in the file were skipped.");
                DrawBlank();
            }

            EndScreen();
            Pause();
        }
    }
}

