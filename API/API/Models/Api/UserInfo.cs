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
        LocationRepository<UserLocation> userLocRepo = new LocationRepository<UserLocation>();
        public UserInfo(User user)
        {
            Id = user.Id;
            Username = user.Username;
            Location = UserLocationInfo.List(userLocRepo.List().Where(u => u.UserId == user.Id));
        }

        public static object Shape(User user, List<string> fields)
        {
            UserInfo usrInf = new UserInfo(user);
            ExpandoObject toReturn = new ExpandoObject();

            foreach(var field in fields)
            {
                try {
                    var value = usrInf.GetType().GetProperty(field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(usrInf);
                    ((IDictionary<String, Object>)toReturn).Add(field, value);
                }
                catch (NullReferenceException)
                {
                    break;
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
        public IEnumerable<UserLocationInfo> Location { get; }
    }
}