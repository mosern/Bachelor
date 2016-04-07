using Api.Classes;
using Api.Models.EF;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Web;
using UserDB;

namespace Api.Models.Api
{
    public class UserInfo
    {
        static LocationRepository<UserLocation> userLocRepo = new LocationRepository<UserLocation>();

        private UserInfo(int id, string username, IEnumerable<object> location)
        {
            Id = id;
            Username = username;
            Location = location;
        }

        public UserInfo(User user)
        {
            Id = user.Id;
            Username = user.Username;
            Location = UserLocationInfo.List(userLocRepo.List().Where(u => u.UserId == user.Id));
        }

        public static object Shape(User user, List<string> fields)
        {
            object usrInf = user;

            if (fields.Any(f => 
            f.Equals("location", StringComparison.OrdinalIgnoreCase)||
            f.Equals("coordinate", StringComparison.OrdinalIgnoreCase)||
            f.Equals("name", StringComparison.OrdinalIgnoreCase)||
            f.Equals("locNr", StringComparison.OrdinalIgnoreCase)||
            f.Equals("hits", StringComparison.OrdinalIgnoreCase)
            ))
            {
                usrInf = new UserInfo(user);
                usrInf = new UserInfo(((UserInfo)usrInf).Id, ((UserInfo)usrInf).Username, 
                                        UserLocationInfo.ShapeList(userLocRepo.List().Where(u => u.UserId == user.Id).AsEnumerable(), fields));

                if(!fields.Any(f => f.Equals("location", StringComparison.OrdinalIgnoreCase)))
                {
                    fields.Add("location");
                }
            }
            
            ExpandoObject toReturn = new ExpandoObject();

            foreach(var field in fields)
            {
                try {
                    var value = usrInf.GetType().GetProperty(field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(usrInf);
                    ((IDictionary<String, Object>)toReturn).Add(field, value);
                }
                catch (NullReferenceException)
                {
                    
                }               
            }

            return toReturn;
        }

        public static IEnumerable<UserInfo> List(IEnumerable<User> users)
        {
            List<UserInfo> UsrInf = new List<UserInfo>();

            foreach(User user in users)
            {
                UsrInf.Add(new UserInfo(user));
            }

            return UsrInf as IEnumerable<UserInfo>;
        }

        public static IEnumerable<Object> ShapeList(IEnumerable<User> users, List<string> fields)
        {
            List<object> toReturn = new List<object>();

            foreach(User user in users)
            {
                toReturn.Add(UserInfo.Shape(user, fields));
            }

            return toReturn;
        }

        public int Id { get; }
        public string Username { get; }
        public IEnumerable<object> Location { get; }
    }
}