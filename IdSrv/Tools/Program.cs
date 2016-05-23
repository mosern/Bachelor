using IdentityServer3.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    /// <summary>
    /// Console program that generates password hash, user for testing.
    /// 
    /// Wiritten by: Andreas Mosvoll
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            string[] tohash = 
            {
                "101419822364482428767",
                "10154276036884453",
                "870a87c4e5c11bbd",
                "101431235647359298200",
                "105064990027889097789",
                "111915463313837390278",
                "507c7e6c81f30f81"
            };

            Console.WriteLine("Raw = Sha512");

            foreach (string raw in tohash)
            {
                Console.WriteLine(raw + " = " + raw.Sha512());
            }
            
            Console.ReadLine();
        }
    }
}
