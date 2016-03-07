using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UserDB
{
    public interface IRepository<X>
    {
        X Create(X entity);
        X Read(int id);
        void Update(X entity);
        void Delete(X entity);


    }
}