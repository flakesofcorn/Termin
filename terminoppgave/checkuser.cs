using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Reflection.Metadata.Ecma335;
using System.Security;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using MySqlConnector;

namespace terminoppgave
{
    public class checkuser
    {


        public static bool CheckUser(string username, string password)
        {
            using (MySqlConnection connection = new MySqlConnection(Program.connectionString))
            {
                connection.Open();




                string query = "SELECT hash, salt FROM users WHERE Username = @Username";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);


                    using (MySqlDataReader reader = command.ExecuteReader())
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
                            else
                            {
                                return false;
                            }
                        }
                    }

                    try
                    {
                        int count = (int)command.ExecuteScalar();
                        connection.Close();
                        return count > 0;
                    }
                    catch (Exception ex)
                    {
                        // Console.WriteLine(ex.ToString());
                        // Console.WriteLine(ex.StackTrace);
                        connection.Close();
                        return false;
                    }

                }
            }
        }
        public class user
        {
            public string username;
            public bool admin;
        }

        public static user Guser = new user();
        public static void User(string username)
        {
            using (MySqlConnection connection = new MySqlConnection(Program.connectionString))
            {
                connection.Open();




                string query = "SELECT admin, username FROM users WHERE Username = @Username";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);


                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Guser.username = (string)reader["username"];
                            Guser.admin = (bool)reader["admin"];

                            Console.WriteLine(Guser.admin);
                        }

                    }
                }
            }
        }

    }






}