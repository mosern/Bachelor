using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserDB
{
    /// <summary>
    /// Entity Framework model for Providers, eks. google and facebook
    /// 
    /// Written by: Andreas Mosvoll
    /// </summary>
    public class Provider : BaseDbModel
    {
        [Required]
        public string Name { get; set; }
    }
}
