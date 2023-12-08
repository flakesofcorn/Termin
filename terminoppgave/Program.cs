using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Reflection.Metadata.Ecma335;
using System.Security;
using System.Security.Cryptography;
using System.Xml;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using MySqlConnector;

// 
//
//
//
//

namespace terminoppgave
{

    public class Program
    {

        public static string connectionString = "Data Source=10.100.1.105;uid=remote1;pwd=admin;Initial Catalog=userdb;";
        static bool running = true;
        public static bool logged_in = false;


        public static void Main()
        {
            while (running)
            {
                //Console.ForegroundColor = ConsoleColor.Blue;

                Console.WriteLine("welcome for command help type help, to continue with 'login'");
                Console.Write("$$- ");
                string input = Console.ReadLine();

                switch (input)
                {
                    case "help":
                        help.help_();
                        break;
                    case "blue":
                        Console.ForegroundColor = ConsoleColor.Blue;
                        break;
                    case "login":
                        try
                        {

                            Console.Write("username: ");
                            string username = Console.ReadLine();

                            Console.Write("password: ");
                            string password = GetMaskedInput();

                            checkuser.User(username);

                            bool valid = checkuser.CheckUser(username, password);

                            if (valid)
                            {
                                Console.Write("\n");
                                Console.WriteLine("logged in");
                                logged_in = true;
                                if (checkuser.Guser.admin)
                                {
                                    Console.WriteLine("admin true");
                                }


                                index();
                            }
                            else
                            {
                                Console.Write('\n');
                                Console.WriteLine("wrong username or password please double check your credentials ");
                                Main();
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            Console.Write(ex.StackTrace);
                        }

                        break;

                    case "mkusr":
                        Console.Write("username: ");
                        string new_username = Console.ReadLine();

                        Console.Write("password: ");
                        string new_password = GetMaskedInput();

                        Console.Write("email: ");
                        string email = Console.ReadLine();
                        try
                        {
                            create_user(new_username, new_password, email);

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                            Main();
                        }



                        break;
                    case "quit":
                        logged_in = false;
                        running = false;
                        System.Environment.Exit(0);
                        break;
                    case "":
                        Main();
                        break;
                    case "clear":
                        Console.Clear();
                        break;
                    default:
                        Console.WriteLine($"{input}, command not recognized");
                        Main();
                        break;
                }
            }

        }



        static string GetMaskedInput() // converts text to * to hide input of sensetive infomation
        {
            string input = "";
            ConsoleKeyInfo keyInfo;

            do
            {
                keyInfo = Console.ReadKey(true);

                if (keyInfo.Key != ConsoleKey.Enter)
                {
                    input += keyInfo.KeyChar;
                    Console.Write("*");
                }
            }

            while (keyInfo.Key != ConsoleKey.Enter);

            return input;
        }






        static void create_user(string username, string password, string email)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {


                byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);
                Console.WriteLine($"salt: {Convert.ToBase64String(salt)}");

                string hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: password!,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 100000,
                    numBytesRequested: 256 / 8));



                string query = "INSERT INTO users (username, hash, admin, email, salt) VALUES (@username, @password, @admin, @email, @salt)";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Password", hash);
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@salt", SqlDbType.VarBinary).Value = salt;
                    command.Parameters.AddWithValue("@admin", true);
                    try
                    {
                        connection.Open();
                        Console.WriteLine(connection);
                        command.ExecuteNonQuery();

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }


                    connection.Close();



                }
                Main();
            }
        }





        public static void index()
        {
            while (running)
            {

                Console.Write("\n");

                Console.Write("$$- ");

                string input = Console.ReadLine();

                switch (input)
                {
                    case "help":
                        help.help_();
                        break;

                    case "logout":
                        logged_in = false;
                        Main();
                        break;

                    case "users":
                        display_users();
                        break;

                    case "quit":
                        running = false;
                        System.Environment.Exit(0);
                        break;
                    case "":
                        index();
                        break;
                    case "mkusr":
                        try
                        {
                            Console.Write("username: ");
                            string new_username = Console.ReadLine();

                            Console.Write("password: ");
                            string new_password = GetMaskedInput();

                            Console.Write("email: ");
                            string email = Console.ReadLine();

                            create_user(new_username, new_password, email);

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }

                        break;
                    case "clear":
                        clearconsole();
                        break;
                    case "mkprod":
                        registerproduct();
                        break;
                    case "listprod":
                        display_prodcuts();
                        break;
                    case "editusr":
                        Console.WriteLine("enter id of user you wish to edit");
                        int id = int.Parse(Console.ReadLine());
                        edit_usr(id);
                        break;
                    case "mkorder":
                        Create_order();
                        break;
                    case "delorder":
                        Console.Write("order id: ");
                        int order_id = int.Parse(Console.ReadLine());
                        Del_order(order_id);
                        break;
                    case "orders":
                        Display_orders();
                        break;
                    default:

                        index();
                        break;
                }
            }


        }
        static void display_users()
        {

            if (checkuser.Guser.admin)
            {

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();


                    string query = "SELECT * FROM users";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {

                        using (MySqlDataReader r = command.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                Console.WriteLine("\n");
                                Console.WriteLine($"id: '{r["id"]}' | username: '{r["username"]}' | email: '{r["email"]}' | admin: '{r["admin"]}'\n");


                            }

                        }

                        command.ExecuteNonQuery();
                        connection.Close();
                    }

                }




                index();
            }
            else
            {
                Console.WriteLine("the user youre currently using does not have access to this command");
                index();
            }
        }

        static bool verify() // verifies user action
        {
            Console.WriteLine("are you sure y for yes n for no");
            string input = Console.ReadLine();

            switch (input)
            {
                case "y":
                    return true;
                case "n":
                    return false;
                default:
                    return false;
            }
        }

        static void edit_usr(int usr_id)
        {


            if (checkuser.Guser.admin)
            {

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT * FROM users WHERE id = @id;";
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@id", usr_id);
                        using (MySqlDataReader r = command.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                Console.WriteLine($"\nid: {r["id"]} | username: {r["username"]} | email: {r["email"]} | admin: {r["admin"]}");
                            }
                        }
                    }

                    Console.WriteLine("to delte user type: 'del'.\n to edit users admin status type 'admin t/f'. \n to edit username type 'usrn'. \n to edit email addr type 'email' ");
                    string input = Console.ReadLine();
                    switch (input)
                    {
                        default:
                            index();
                            break;
                        case "del": // deletes user from database
                            if (verify())
                            {
                                string query = "DELETE FROM users WHERE id = @id";
                                using (MySqlCommand command = new MySqlCommand(query, connection))
                                {
                                    command.Parameters.AddWithValue("@id", usr_id);
                                    try
                                    {
                                        command.ExecuteNonQuery();
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"ex: {ex.Message}, {ex.StackTrace}");
                                    }
                                }

                            }
                            else
                            {
                                return;
                            }
                            break;
                        case "admin t": // change the admin status of the user to true
                            if (!verify()) { return; }
                            else
                            {
                                string queryA = "UPDATE users SET admin = @admin WHERE id = @id";
                                using (MySqlCommand command = new MySqlCommand(queryA, connection))
                                {
                                    command.Parameters.AddWithValue("@admin", true);
                                    command.Parameters.AddWithValue("@id", usr_id);
                                    try
                                    {
                                        command.ExecuteNonQuery();
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"ex: {ex.Message}, {ex.StackTrace}");
                                    }

                                }
                            }
                            break;
                        case "admin f": // change the admin status of the user to false
                            if (!verify()) { return; }
                            else
                            {
                                string queryB = "UPDATE users SET admin = @admin WHERE id = @id";
                                using (MySqlCommand command = new MySqlCommand(queryB, connection))
                                {
                                    command.Parameters.AddWithValue("@admin", false);
                                    command.Parameters.AddWithValue("@id", usr_id);
                                    try
                                    {
                                        command.ExecuteNonQuery();
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"ex: {ex.Message}, {ex.StackTrace}");
                                    }

                                }
                            }
                            break;
                        case "usrn": // change the username for the user
                            Console.Write("New Username: ");
                            string n_username = Console.ReadLine();

                            if (!verify()) { return; }
                            else
                            {
                                string queryC = "UPDATE users SET username = @newusername WHERE id = @id";
                                using (MySqlCommand command = new MySqlCommand(queryC, connection))
                                {
                                    command.Parameters.AddWithValue("@newusername", n_username);
                                    command.Parameters.AddWithValue("@id", usr_id);
                                    try
                                    {
                                        command.ExecuteNonQuery();
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"ex: {ex.Message}, {ex.StackTrace}");
                                    }

                                }
                            }

                            break;
                        case "email": // change email on the user
                            Console.Write("New Email: ");
                            string n_Email = Console.ReadLine();

                            if (!verify()) { return; }
                            else
                            {
                                string queryC = "UPDATE users SET email = @newemail WHERE id = @id";
                                using (MySqlCommand command = new MySqlCommand(queryC, connection))
                                {
                                    command.Parameters.AddWithValue("@newemail", n_Email);
                                    command.Parameters.AddWithValue("@id", usr_id);
                                    try
                                    {
                                        command.ExecuteNonQuery();
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"ex: {ex.Message}, {ex.StackTrace}");
                                    }

                                }
                            }
                            break;


                    }
                }


            }
        }

        static void clearconsole()
        {
            Console.Clear();
        }

        static bool Checkadm(string username)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();




                string query = "SELECT admin FROM users WHERE Username = @Username";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);


                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (reader["admin"] is true)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }


                        }

                    }


                    int count = (int)command.ExecuteScalar();


                    connection.Close();
                    return count > 0;

                }
            }
        }

        static void registerproduct()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {





                string query = "INSERT INTO products (SerialNumber, Name, SizeInLitres) VALUES (@snn, @name, @size)";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    Console.WriteLine("input the serialnumber of the product");
                    string snn_T = Console.ReadLine();
                    int snn = int.Parse(snn_T);
                    Console.WriteLine("input the name of the product");
                    string name = Console.ReadLine();
                    Console.WriteLine("input the size(litres) of the product");
                    string size = Console.ReadLine();

                    command.Parameters.AddWithValue("@snn", snn);
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@size", size);

                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                        Console.WriteLine($"{name}, item sucessfully added into 'products'");

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        Console.WriteLine(ex.StackTrace);
                    }


                    connection.Close();



                }
                if (!logged_in)
                {
                    Main();
                }
                else
                {
                    index();

                }
            }
        }

        static void display_prodcuts()
        {

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {



                string query = "SELECT * FROM products";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    connection.Open();
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine("----------------------------------------------");
                            Console.WriteLine($"    |{reader["SerialNumber"]}  |   {reader["Name"]}  |   {reader["SizeInLitres"]}    |");

                        }

                    }
                    try
                    {
                        command.ExecuteNonQuery();
                        connection.Close();

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }

                }

            }
        }

        static void Create_order()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                int SNN = 0; // Initialize variables
                string name = string.Empty;
                int PID = 0;

                connection.Open();

                // Query to select products
                string query1 = "SELECT * FROM products";

                using (MySqlCommand command = new MySqlCommand(query1, connection))
                {
                    using (MySqlDataReader r = command.ExecuteReader())
                    {
                        Console.WriteLine("list of product name and serialnumbers: ");
                        while (r.Read())
                        {
                            Console.WriteLine($" serialnumber: {r["SerialNumber"]} | name: {r["Name"]}");
                        }
                    }
                }




                string query2 = "SELECT * FROM AUsers_table";

                using (MySqlCommand command = new MySqlCommand(query2, connection))
                {
                    using (MySqlDataReader r = command.ExecuteReader())
                    {
                        Console.WriteLine("id for users: \n");
                        while (r.Read())
                        {
                            Console.WriteLine($"{r["id"]} | {r["email"]}");
                        }
                    }
                }

                Console.Write("Serialnumber: ");
                SNN = int.Parse(Console.ReadLine());
                Console.Write("user id: ");
                PID = int.Parse(Console.ReadLine());
                Console.Write("amount(in litres): ");
                double amount = double.Parse(Console.ReadLine());



                string query3 = "INSERT INTO orders (productSerialNumber, OrderDate, Quantity, TotalPrice, UserID) " +
                                "VALUES (@snn, @date, @size, @price, @pid)";

                using (MySqlCommand command = new MySqlCommand(query3, connection))
                {
                    DateTime currentDateTime = DateTime.Now;

                    double price = 299.99 * amount;

                    // Setting parameter values
                    command.Parameters.AddWithValue("@snn", SNN);
                    command.Parameters.AddWithValue("@date", currentDateTime);
                    command.Parameters.AddWithValue("@size", amount);
                    command.Parameters.AddWithValue("@price", price);
                    command.Parameters.AddWithValue("@pid", PID);


                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        Console.WriteLine("Order created successfully.");
                    }
                    else
                    {
                        Console.WriteLine("Failed to create order.");
                    }
                }
            }
        }

        static void Del_order(int order_id)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {



                string query = $"DELETE FROM orders WHERE id = {order_id}";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    if (verify())
                    {
                        try
                        {
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                    else
                    {
                        index();
                    }
                }

            }
        }

        static void Display_orders()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {



                string query = "SELECT * FROM orders";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    connection.Open();
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine($"    userid: {reader["UserID"]}    |    orderid: {reader["OrderID"]}  |   serialnumber: {reader["ProductSerialNumber"]}  |   orderdate: {reader["OrderDate"]}    |   Quantity(L): {reader["Quantity"]}    |   price(Kr): {reader["TotalPrice"]}\n");

                        }

                    }
                    try
                    {
                        command.ExecuteNonQuery();
                        connection.Close();

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            }
        }
    }
}
