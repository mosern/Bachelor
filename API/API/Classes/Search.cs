using Api.Models.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Api.Classes
{
    public class Search
    {
        static LocationRepository<Location> LocRepo = new LocationRepository<Location>();

        public static IEnumerable<Location> Location(string searchString)
        {
            if (isLocId(searchString))
            {
                return locationById(Convert.ToInt32(searchString));
            }
            else if (isLocNr(searchString))
            {
                return locationByLocNr(searchString);
            }
            else if(isLocName(searchString)){
                return locationByName(searchString);
            }
            else
            {
                return null;
            } 
        }

        private static bool isLocNr(string checkString)
        {
            if(checkString.Length > 5)
            {
                return false;
            }

            if(checkString.Length == 1)
            {
                if (char.IsLetter(Convert.ToChar(checkString)))
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }

            try
            {
                char ch = Convert.ToChar(checkString.Substring(0, 1));
                int num = Convert.ToInt32(checkString.Substring(1, checkString.Length-1));
                return true;
            }
            catch
            {
                return false;
            }
                
        }

        private static bool isLocName(string checkString)
        {
            if(checkString.Length <= 20)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool isLocId(string checkString)
        {
            try
            {
                Convert.ToInt32(checkString);
                return true;
            }
            catch
            {
                return false;
            }

        }

        private static IEnumerable<Location> locationByLocNr(string locNr)
        {
            var loc = LocRepo.List().Where(l => l.LocNr.Contains(locNr));
            if (loc != null)
            {
                return loc;
            }
            else
            {
                return null;
            }
        }

        private static IEnumerable<Location> locationByName(string name)
        {
            var loc = LocRepo.List().Where(l => l.Name.Contains(name));
            if (loc != null)
            {
                return loc;
            }
            else
            {
                return null;
            }
        }

        private static IEnumerable<Location> locationById(int id)
        {
            var loc = LocRepo.Read(id);
            if(loc != null)
            {
                return new[] { loc };
            }
            else
            {
                return null;
            }
            
        }
    }
}