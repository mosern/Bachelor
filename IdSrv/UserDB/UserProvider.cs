using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserDB
{
    /// <summary>
    /// Entity Framework Model that contains information on which providers the users is registered with and the provider identifier
    /// 
    /// Written by: Andreas Mosvoll
    /// </summary>
    public class UserProvider : BaseDbModel
    {
        [Required]
        public string Identifier { get; set; }

        public int ProviderId { get; set; }
        public virtual Provider Provider { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }
    }
}
