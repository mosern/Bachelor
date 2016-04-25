using Api.Factories;
using Api.Helpers;
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
    /// <summary>
    /// Handles search fuctionality
    /// </summary>
    public class Search
    {
        /// <summary>
        /// Takes a search stirng and returns the result
        /// </summary>
        /// <param name="searchString"></param>
        /// <returns>SearchViewModel, contains the result in form of a list LocationViewModel and a list of PeopleViewModel</returns>
        public static SearchViewModel Location(string searchString)
        {
            SearchViewModel result = new SearchViewModel();
            IQueryable<LocationViewModel> locations = null;
            IQueryable<PeopleViewModel> people = null;

            switch (getTypeLocation(searchString))
            {
                case 0 : locations = new List<LocationViewModel>().AsQueryable(); break;
                case 1 : locations = locationByLocNr(searchString); break;
                case 2 : locations = locationByName(searchString); break;
            }

            switch (getTypePeople(searchString))
            {
                case 0: people = new List<PeopleViewModel>().AsQueryable(); break;
                case 1: people = peopleByName(searchString); break;
            }

            //TODO Clean this up!
            //List<int> locid = new List<int>();

            //foreach(PeopleViewModel peo in people)
            //{
            //    Location tempLoc;
            //    using(var repo = new LocationRepository<Location>())
            //        tempLoc = repo.List().Where(l => l.Id == peo.Location.Id).FirstOrDefault();

            //    LocationViewModel loc = null;

            //    if(tempLoc != null)
            //    {
            //        loc = AutoMapConfig.getMapper().Map<Location, LocationViewModel>(tempLoc);
            //    }
            //    else
            //    {
            //        break;
            //    }

            //    locid.Add(loc.Id.Value);

               

            //    if (!locations.Contains(loc, new CompareEF<LocationViewModel>()))
            //    {
            //        var temp = locations.ToList();
            //        temp.Add(loc);

            //        locations = temp.AsQueryable();
            //    }
            //}

            //foreach (LocationViewModel loc in locations)
            //{
            //    if (!locid.Contains(loc.Id.Value))
            //    {
            //        People tempPeo;
            //        using (var repo = new LocationRepository<People>())
            //            tempPeo = repo.List().Where(p => p.LocationId == loc.Id).FirstOrDefault();

            //        PeopleViewModel peo = null;

            //        if (tempPeo != null)
            //        {
            //            peo = AutoMapConfig.getMapper().Map<People, PeopleViewModel>(tempPeo);
            //        }
            //        else
            //        {
            //            break;
            //        }

            //        if (!people.Contains(peo))
            //        {
            //            var temp = people.ToList();
            //            temp.Add(peo);

            //            people = temp.AsQueryable();
            //        }
            //    }
            //}


            result.LocationViewModel = locations;
            result.PeopleViewModel = people;

            return result;

        }

        /// <summary>
        /// Checks what the search string contains
        /// </summary>
        /// <param name="searchString"></param>
        /// <returns>int based on type of input. 1 for Loc number, 2 for LocName, 0 for unknown</returns>
        private static int getTypeLocation(string searchString)
        {
            if (isLocNr(searchString))
            {
                return 1;
            }
            else if (isLocName(searchString))
            {
                return 2;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Checks what the search string contains
        /// </summary>
        /// <param name="searchString"></param>
        /// <returns>int based on type of input. 1 for people name, 0 for unknown</returns>
        private static int getTypePeople(string searchString)
        {

            if (isPeoName(searchString))
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Checks if search string is a locNr
        /// </summary>
        /// <param name="checkString"></param>
        /// <returns>bool</returns>
        private static bool isLocNr(string checkString)
        {
            if(checkString.Length > 5)
            {
                return false;
            }

            try
            {
                if(checkString.Length <= 4)
                {
                    Convert.ToInt32(checkString);
                    return true;
                }
            }
            catch
            {

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

        /// <summary>
        /// Checks if search string is a locName
        /// </summary>
        /// <param name="checkString"></param>
        /// <returns>bool</returns>
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

        //TODO Remove?
        //private static bool isLocId(string checkString)
        //{
        //    try
        //    {
        //        Convert.ToInt32(checkString);
        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }

        //}

        /// <summary>
        /// Checks if search string is a people name
        /// </summary>
        /// <param name="checkString"></param>
        /// <returns>bool</returns>
        private static bool isPeoName(string checkString)
        {
            if (checkString.Length <= 20)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// gets people objects where name contains search string
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static IQueryable<PeopleViewModel> peopleByName(string name)
        {
            IEnumerable<People> peo;
            using (var repo = new LocationRepository<People>())
            {
                peo = repo.List().Where(p => p.Name.Contains(name)).ToList();

                if (peo != null)
                {
                    return AutoMapConfig.getMapper().Map<IEnumerable<People>, IEnumerable<PeopleViewModel>>(peo).AsQueryable();
                }
                else
                {
                    return new List<PeopleViewModel>().AsQueryable();
                }
            }
                
        }

        /// <summary>
        /// gets location objects where locNr contains search string
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static IQueryable<LocationViewModel> locationByLocNr(string locNr)
        {
            IEnumerable<Location> loc;
            using (var repo = new LocationRepository<Location>())
            {
                loc = repo.List().Where(l => l.LocNr.Contains(locNr));

                if (loc != null)
                {
                    return AutoMapConfig.getMapper().Map<IEnumerable<Location>, IEnumerable<LocationViewModel>>(loc).AsQueryable();
                }
                else
                {
                    return new List<LocationViewModel>().AsQueryable();
                }
            }             
        }

        /// <summary>
        /// gets location objects where name contains search string
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static IQueryable<LocationViewModel> locationByName(string name)
        {
            IEnumerable<Location> loc;
            using (var repo = new LocationRepository<Location>())
            {
                loc = repo.List().Where(l => l.Name.Contains(name));

                if (loc != null)
                {
                    return AutoMapConfig.getMapper().Map<IEnumerable<Location>, IEnumerable<LocationViewModel>>(loc).AsQueryable();
                }
                else
                {
                    return new List<LocationViewModel>().AsQueryable();
                }
            }

        }

        /// <summary>
        /// gets location objects where id equals search string
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static IQueryable<LocationViewModel> locationById(int id)
        {
            Location loc;
            using (var repo = new LocationRepository<Location>())
            {
                loc = repo.Read(id);

                if (loc != null)
                {
                    var list = new List<Location>();
                    list.Add(loc);

                    return AutoMapConfig.getMapper().Map<IEnumerable<Location>, IEnumerable<LocationViewModel>>(list).AsQueryable();
                }
                else
                {
                    return new List<LocationViewModel>().AsQueryable();
                }
            }            
        }

    }
}