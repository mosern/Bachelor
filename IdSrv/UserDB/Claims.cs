using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserDB
{
    public class Claims : BaseDbModel
    {
        [Required]
        public string Type { get; set; }
        [Required]
        public string Value { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }
    }
}
