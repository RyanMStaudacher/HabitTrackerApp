using System.Data.Common;
using System.Globalization;
using Microsoft.Data.Sqlite;

namespace HabitTracker
{
    class Program
    {
        static string connectionString = @"Data Source=habit-tracker.db";

        static void Main(string[] args)
        {
            Console.Clear();
            using(var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var tableCmd = connection.CreateCommand();

                tableCmd.CommandText = @"CREATE TABLE IF NOT EXISTS drinking_water (Id INTEGER PRIMARY KEY AUTOINCREMENT, 
                Date TEXT, Quantity INTEGER)";

                tableCmd.ExecuteNonQuery();

                connection.Close();
            }

            GetUserInput();
        }

        static void GetUserInput()
        {
            //Console.Clear();
            bool closeApp = false;
            while (closeApp == false)
            {
                Console.WriteLine("MAIN MENU");
                Console.WriteLine("\nWhat would you like to do?");
                Console.WriteLine("\nType 0 to close the application.");
                Console.WriteLine("Type 1 to view all records.");
                Console.WriteLine("Type 2 to insert record.");
                Console.WriteLine("Type 3 to delete record.");
                Console.WriteLine("Type 4 to update record.");
                Console.WriteLine("------------------------------------");

                string commandInput = Console.ReadLine();

                switch(commandInput)
                {
                    case "0":
                        Console.Clear();
                        Console.WriteLine("\nGoodbye!");
                        System.Threading.Thread.Sleep(1500);
                        Console.Clear();
                        closeApp = true;
                        Environment.Exit(0);
                    break;
                    case "1":
                        GetAllRecords();
                    break;
                    case "2":
                        Insert();
                    break;
                    case "3":
                        Delete();
                    break;
                    case "4":
                        Update();
                    break;
                    default:
                        Console.WriteLine("\nInvalid command. Please type a number from 0 to 4.\n");
                    break;
                }
            }
        }

        private static void GetAllRecords()
        {
            //Console.Clear();
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText = $"SELECT * FROM drinking_water ";

                List<DrinkingWater> tableData = new List<DrinkingWater>();

                SqliteDataReader reader = tableCmd.ExecuteReader();

                if(reader.HasRows)
                {
                    while(reader.Read())
                    {
                        tableData.Add(new DrinkingWater
                        {
                            Id = reader.GetInt32(0),
                            Date = DateTime.ParseExact(reader.GetString(1), "dd-MM-yy", new CultureInfo("en-US")),
                            Quantity = reader.GetInt32(2)
                        });
                    }
                }
                else
                {
                    Console.WriteLine("No rows found");
                }

                connection.Close();

                Console.WriteLine("--------------------------------------------");
                foreach (DrinkingWater dw in tableData)
                {
                    Console.WriteLine($"{dw.Id} - {dw.Date.ToString("dd-MMM-yyyy")} - Quantity: {dw.Quantity}");
                }
                Console.WriteLine("--------------------------------------------\n");

                Console.WriteLine("Press enter to continue...");
                Console.ReadLine();
                Console.Clear();
            }
        }

        private static void Insert()
        {
            Console.Clear();
            string date = GetDateInput();

            int quantity = GetNumberInput("Please insert number of glasses or other measure of your choice (no decimals allowed)");

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText = $"INSERT INTO drinking_water(date, quantity) VALUES('{date}', {quantity})";

                tableCmd.ExecuteNonQuery();

                connection.Close();
            }

            Console.Clear();
        }

        private static void Delete()
        {
            Console.Clear();
            GetAllRecords();

            var recordId = GetNumberInput("\nPlease type the Id of the record you want to delete or type 0 to go back to the main menu.");

            using(var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var tableCmd = connection.CreateCommand();
                //tableCmd.CommandText = $"DELETE from drinking_water WHERE Id = '{recordId}'";

                tableCmd.CommandText = $"Select COUNT(1) from drinking_water Where Id = '{recordId}'";

                var commandOutput = tableCmd.ExecuteScalar().ToString();

                if(commandOutput == "1")
                {
                    tableCmd.CommandText = $"DELETE from drinking_water WHERE Id = '{recordId}'";
                    tableCmd.ExecuteNonQuery();
                }
                else if(commandOutput == "0")
                {
                    Console.WriteLine($"Record with Id {recordId} doesn't exist.\nPress enter to continue...");
                    Console.ReadLine();
                    Delete();
                }
            }

            Console.WriteLine($"Record with Id {recordId} was deleted.\nPress enter to continue...");
            Console.ReadLine();
            Console.Clear();

            GetUserInput();
        }

        private static void Update()
        {
            Console.Clear();
            GetAllRecords();

            var recordId = GetNumberInput("\n\nPlease type the Id of the record you would like to update. Type 0 to return to the main menu.");

            using( var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var checkCmd = connection.CreateCommand();
                checkCmd.CommandText = $"SELECT EXISTS(SELECT 1 FROM drinking_water WHERE Id = {recordId})";
                int checkQuery = Convert.ToInt32(checkCmd.ExecuteScalar());

                if(checkQuery == 0)
                {
                    Console.WriteLine($"\n\nRecord with Id {recordId} doesn't exist.\n\n");
                    connection.Close();
                    Update();
                }

                string date = GetDateInput();

                int quantity = GetNumberInput("\nPlease insert number of glasses or other measure of your choice. (no decimals allowed)");

                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText = $"UPDATE drinking_water SET date = '{date}', quantity = {quantity} WHERE Id = {recordId}";

                tableCmd.ExecuteNonQuery();

                connection.Close();
            }
        }

        internal static string GetDateInput()
        {
            Console.WriteLine("\nPlease insert the date: (Format: dd-mm-yy). Type 0 to return to main menu.");

            string dateInput = Console.ReadLine();

            if(dateInput == "0")
            {
                GetUserInput();
            }

            while(!DateTime.TryParseExact(dateInput, "dd-MM-yy", new CultureInfo("en-US"), DateTimeStyles.None, out _))
            {
                Console.WriteLine("\n\nInvalid date. (Format: dd-MM-yy). Type 0 to return to main menu or try again.\n\n");
                dateInput = Console.ReadLine();
            }

            Console.WriteLine();
            
            return dateInput;
        }

        internal static int GetNumberInput(string message)
        {
            Console.WriteLine(message);

            string numberInput = Console.ReadLine();

            if(numberInput == "0")
            {
                Console.Clear();
                GetUserInput();
            }

            while(!Int32.TryParse(numberInput, out _) || Convert.ToInt32(numberInput) < 0)
            {
                Console.WriteLine("\n\nInvalid number. Try again.\n\n");
                numberInput = Console.ReadLine();
            }

            int finalInput = Convert.ToInt32(numberInput);

            Console.WriteLine();

            return finalInput;
        }
    }

    public class DrinkingWater
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int Quantity { get; set; }
    }
}