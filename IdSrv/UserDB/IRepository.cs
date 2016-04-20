using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UserDB
{
    /// <summary>
    /// Repository interface
    /// 
    /// Written by: Andreas Mosvoll
    /// </summary>
    /// <typeparam name="X"></typeparam>
    public interface IRepository<X> : IDisposable
    {
        X Create(X entity);
        X Read(int id);
        void Update(X entity);
        void Delete(X entity);
        IQueryable<X> List();


    }
}