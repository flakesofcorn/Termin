using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Reflection.Metadata.Ecma335;
using System.Security;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

class Program
{

    static string connectionString = "Data Source=DESKTOP-EVCQ0V1\\SQLEXPRESS;Initial Catalog=users_db;Integrated Security=True";



    static void Main()
    {
        Console.WriteLine("welcome for command help type help, to continue loggin in with 'login'");
        Console.Write("$$- ");
        string input = Console.ReadLine();

        switch (input)
        {
            case "help":
                help();
                break;
            case "login":
                Console.Write("username: ");
                string username = Console.ReadLine();

                Console.Write("password: ");
                string password = GetMaskedInput();

                bool valid = CheckUser(username, password);

                if (valid)
                {
                    Console.Write("\n");
                    Console.WriteLine("logged in");
                    index();
                }
                else
                {
                    Console.Write('\n');
                    Console.WriteLine("wrong username or password please double check your credentials ");
                    Main();
                }
                break;
            case "mkusr":
                Console.Write("username: ");
                string new_username = Console.ReadLine();

                Console.Write("password: ");
                string new_password = GetMaskedInput();

                Console.Write("email: ");
                string email = Console.ReadLine();

                create_user(new_username, new_password, email);


                break;
            case "quit":
                System.Environment.Exit(0);
                break;
            case "":
                Main();
                break;
            case "clear":
                Console.Clear();
                break;
            default:
                Main();
                break;
        }

    }



    static string GetMaskedInput()
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


    static void help()
    {
        Console.WriteLine("-login       |       to login to appllication requires valid username and password.");
        Console.WriteLine("-mkusr       |       use to create a new user.");
        Console.WriteLine("-quit        |       exits the program.");
        Console.WriteLine("-users       |       displays all users currently registered(only accesible by admin users)");

        Main();
    }

    static void create_user(string username, string password, string email)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {


            byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);
            Console.WriteLine($"salt: {Convert.ToBase64String(salt)}");

            string hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password!,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));



            string query = "INSERT INTO AUsers_table (username, hash, admin, email, salt) VALUES (@username, @password, @admin, @email, @salt)";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@Password", hash);
                command.Parameters.AddWithValue("@email", email);
                command.Parameters.AddWithValue("@salt", SqlDbType.VarBinary).Value = salt;
                command.Parameters.AddWithValue("@admin", false);
                try
                {
                    connection.Open();
                    Console.WriteLine(connection);
                    Console.WriteLine(hash);
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



    static bool CheckUser(string username, string password)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();




            string query = "SELECT hash, salt FROM AUsers_table WHERE Username = @Username";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Username", username);


                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string hash = reader.GetString(0);
                        byte[] salt = (byte[])reader["salt"];


                        if (salt != null)
                        {
                            string hash2 = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                                password: password!,
                                salt: salt,
                                prf: KeyDerivationPrf.HMACSHA256,
                                iterationCount: 100000,
                                numBytesRequested: 256 / 8));

                            if (hash2 != hash)
                            {

                                return false;
                            }
                            else
                            {
                                return true;
                            }



                        }


                    }

                }





                int count = (int)command.ExecuteScalar();


                connection.Close();
                return count > 0;

            }
        }
    }

    static void index()
    {
        Console.Write("\n");

        Console.Write("$$- ");

        string input = Console.ReadLine();

        switch (input)
        {
            case "help":
                help();
                break;

            case "logout":
                Main();

                break;

            case "users":
                display_users();

                break;
            case "quit":

                System.Environment.Exit(0);
                break;
            case "":
                index();
                break;
            case "mkusr":
                Console.Write("username: ");
                string new_username = Console.ReadLine();

                Console.Write("password: ");
                string new_password = GetMaskedInput();

                Console.Write("email: ");
                string email = Console.ReadLine();

                create_user(new_username, new_password, email);


                break;
            case "clear":
                Console.Clear();
                index();
                break;
            default:
                index();
                break;
        }


    }
    static void display_users()
    {
        Console.WriteLine("please type in your username");
        string username = Console.ReadLine();
        bool valid = Checkadm(username);
        if (valid)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();


                string query = "SELECT * FROM AUsers_table";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine("----------------------------------------------");
                            Console.WriteLine($"    |{reader["id"]}  |   {reader["username"]}  |   {reader["email"]}  |   {reader["admin"]}    |");

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
            Console.WriteLine("he user youre currently using does not have access to this command");
            index();
        }
    }

    static bool Checkadm(string username)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();




            string query = "SELECT admin FROM AUsers_table WHERE Username = @Username";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Username", username);


                using (SqlDataReader reader = command.ExecuteReader())
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
        using (SqlConnection connection = new SqlConnection(connectionString))
        {





            string query = "INSERT INTO products (SerialNumber, Name, SizeInLitres) VALUES (@snn, @name, @size)";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                Console.WriteLine("input the serialnumber of the product");
                string snn = Console.ReadLine();
                Console.WriteLine("input the name of the product");
                string name = Console.ReadLine();
                Console.WriteLine("input the size(litres) of the product");
                string size = Console.ReadLine();

                command.Parameters.AddWithValue("@snn", snn);
                command.Parameters.AddWithValue("@name" name);
                command.Parameters.AddWithValue("@size", size);

                try
                {
                    connection.Open();
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
}