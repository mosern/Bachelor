using Api.Factories;
using Api.Helpers;
using Api.Models.Api;
using Api.Models.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using UserDB;

namespace Api.Classes
{
    /// <summary>
    /// Handles search fuctionality
    /// 
    /// Written by: Andreas Mosvoll
    /// </summary>
    public class Search
    {
        /// <summary>
        /// Takes a search stirng and returns the result
        /// </summary>
        /// <param name="searchString"></param>
        /// <returns>SearchViewModel, contains the result in form of a list LocationViewModel and a list of PeopleViewModel</returns>
        public static SearchViewModel Location(string searchString, IPrincipal user)
        {
            SearchViewModel result = new SearchViewModel();
            List<LocationViewModel> locations = new List<LocationViewModel>();
            IQueryable<PeopleViewModel> people = null;

            //switch (getTypeLocation(searchString))
            //{
            //    case 0 : locations = new List<LocationViewModel>().AsQueryable(); break;
            //    case 1 : locations = locationByLocNr(searchString); break;
            //    case 2 : locations = locationByName(searchString); break;
            //}

            locations.AddRange(locationByLocNr(searchString));

            var byName = locationByName(searchString);

            foreach(LocationViewModel loc in byName)
            {
                if (!locations.Contains(loc))
                    locations.Add(loc);
            }

            if(locations.Count == 1)
            {
                var loc = locations.First();
                loc.Hits += 1;
                using (var repo = new LocationRepository<Location>())
                    repo.Update(AutoMapConfig.getMapper().Map<LocationViewModel, Location>(loc));

                if (user.Identity.IsAuthenticated)
                {   
                    using(var userRepo = new Repository<User>())     
                    using (var repo = new LocationRepository<UserLocation>())
                    {
                        var userC = user as ClaimsPrincipal;
                        string username = userC.FindFirst("username").Value;
                        var dbUser = userRepo.List().Where(u => u.Username == username).FirstOrDefault();
                        var userLoc = repo.List().Where(u => u.LocationId == loc.Id && u.User.Id == dbUser.Id).FirstOrDefault();

                        if (userLoc == null)
                        {
                            repo.Create(new UserLocation { LocationId = loc.Id.Value, UserId = dbUser.Id, Hits = 1 });
                        }
                        else
                        {
                            userLoc.Hits += 1;
                            repo.Update(userLoc);
                        }
                            
                    }

                }
            }

            switch (getTypePeople(searchString))
            {
                case 0: people = new List<PeopleViewModel>().AsQueryable(); break;
                case 1: people = peopleByName(searchString); break;
            }

            result.LocationViewModel = locations.AsQueryable();
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
                //peo = repo.List().Where(p => p.Name.Contains(name)).ToList();
                peo = repo.List().Where(p => p.Name.StartsWith(name)).ToList();

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
        /// <param name="locNr"></param>
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
                //loc = repo.List().Where(l => l.Name.Contains(name));
                loc = repo.List().Where(l => l.Name.StartsWith(name));

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
        /// <param name="id"></param>
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