using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UserDB
{
    /// <summary>
    /// Entity Framework Model, this is the base for all my EF models
    /// Written by: Andreas Mosvoll
    /// </summary>
    public class BaseDbModel
    {
        public int Id { get; set; }
    }
}