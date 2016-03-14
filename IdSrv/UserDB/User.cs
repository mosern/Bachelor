using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UserDB
{
    /// <summary>
    /// Entity Framework model for users, intentionally with minimal information
    /// 
    /// Written by: Andreas Mosvoll
    /// </summary>
    public class User : BaseDbModel
    {
        [Required, MinLength(5),MaxLength(40)]
        public string Username { get; set; }
        [Required, MinLength(10), MaxLength(100)]
        public string Password { get; set; }
    }
}