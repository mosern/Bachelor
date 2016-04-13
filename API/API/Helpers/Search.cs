using Api.Factories;
using Api.Models.Api;
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

        public static SearchViewModel Location(string searchString)
        {
            SearchViewModel result = new SearchViewModel();
            IQueryable<LocationViewModel> locations = null;
            IQueryable<PeopleViewModel> people = null;

            switch (getTypeLocation(searchString))
            {
                case 1 : locations = locationById(Convert.ToInt32(searchString)); break;
                case 2 : locations = locationByLocNr(searchString); break;
                case 3 : locations = locationByName(searchString); break;
            }

            switch (getTypePeople(searchString))
            {
                case 1: people = new List<PeopleViewModel>().AsQueryable(); break;
                case 2: people = new List<PeopleViewModel>().AsQueryable(); break;
                case 3: people = new List<PeopleViewModel>().AsQueryable(); break;
            }


            result.LocationViewModel = locations;
            result.PeopleViewModel = people;

            return result;

        }

        private static int getTypeLocation(string searchString)
        {
            if (isLocId(searchString))
            {
                return 1;
            }
            else if (isLocNr(searchString))
            {
                return 2;
            }
            else if (isLocName(searchString))
            {
                return 3;
            }
            else
            {
                return 0;
            }
        }

        private static int getTypePeople(string searchString)
        {
            if (isLocId(searchString))
            {
                return 1;
            }
            else if (isLocNr(searchString))
            {
                return 2;
            }
            else if (isLocName(searchString))
            {
                return 3;
            }
            else
            {
                return 0;
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

        private static IQueryable<LocationViewModel> locationByLocNr(string locNr)
        {
            var loc = LocRepo.List().Where(l => l.LocNr.Contains(locNr));
            if (loc != null)
            {
                return ConversionFactory.queryLocationToViewModel(loc);
            }
            else
            {
                return new List<LocationViewModel>().AsQueryable();
            }
        }

        private static IQueryable<LocationViewModel> locationByName(string name)
        {
            var loc = LocRepo.List().Where(l => l.Name.Contains(name));
            if (loc != null)
            {
                return ConversionFactory.queryLocationToViewModel(loc); ;
            }
            else
            {
                return new List<LocationViewModel>().AsQueryable();
            }
        }

        private static IQueryable<LocationViewModel> locationById(int id)
        {
            var loc = LocRepo.Read(id);
            if(loc != null)
            {
                var list = new List<Location>();
                list.Add(loc);

                return ConversionFactory.queryLocationToViewModel(list.AsQueryable());
            }
            else
            {
                return new List<LocationViewModel>().AsQueryable();
            }
            
        }

    }
}