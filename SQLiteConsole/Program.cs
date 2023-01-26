using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace SQLiteConsole
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try {
                var db = new SQLiteConnection("test.db");

            } catch (Exception e) {
                Console.Write("Failed " + e.Message);
            }
            
        }
    }
}
