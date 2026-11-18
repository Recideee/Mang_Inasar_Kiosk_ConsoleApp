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
    }
}

