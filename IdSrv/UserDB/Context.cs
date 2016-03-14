using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserDB
{
    /// <summary>
    /// Entity Framework DbContext for user tables 
    /// 
    /// Written by: Andreas Mosvoll
    /// </summary>
    public class Context : DbContext
    {
        public Context() : base("Bachelor")
        {

        }

        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<Provider> Provider { get; set; }
        public virtual DbSet<UserProvider> UserProvider { get; set; }
        public virtual DbSet<Claims> Claims { get; set; }

    }
}
