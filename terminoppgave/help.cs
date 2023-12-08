using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace terminoppgave
{
    public class help : Program
    {
        public static void help_()
        {
            Console.WriteLine("list of all commands and their uses: \n");
            Console.WriteLine("-login       |       to login to appllication requires valid username and password.");
            Console.WriteLine("-logout      |       loggs out back to start screen");
            Console.WriteLine("-mkusr       |       use to create a new user.");
            Console.WriteLine("-quit        |       exits the program.");
            Console.WriteLine("-users       |       displays all users currently registered");
            Console.WriteLine("-clear       |       clears the console");
            Console.WriteLine("-mkprod      |       add new product to databse");
            Console.WriteLine("-listprod    |       lists all products saved in the databse");
            Console.WriteLine("-editusr     |       allows admin to edit and delete registered users");
            Console.WriteLine("-mkorder     |       creates an order and save it to the database\n");
            if (!Program.logged_in)
            {
                Program.Main();

            }
            else
            {
                Program.index();
            }
        }
    }
}
