using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Api.Classes
{
    /// <summary>
    /// Repository interface
    /// 
    /// Written by: Andreas Mosvoll
    /// </summary>
    /// <typeparam name="X"></typeparam>
    public interface ILocationRepository<X>
    {
        X Create(X entity);
        X Read(int id);
        void Update(X entity);
        void Delete(X entity);


    }
}