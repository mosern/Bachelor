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
            string password = "Hemmelig!";
            Console.WriteLine(password.Sha512());
            Console.ReadLine();
        }
    }
}
