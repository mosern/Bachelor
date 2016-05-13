using Api.Models.EF;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;

namespace Api.Classes
{
    /// <summary>
    /// I was inspired by the following site and so the code is similar and some places copied.
    /// http://www.codeproject.com/Articles/814768/CRUD-Operations-Using-the-Generic-Repository-Patte
    /// 
    /// Written by: Andreas Mosvoll
    /// </summary>
    /// <typeparam name="X"></typeparam>
    public class LocationRepository<X> : ILocationRepository<X> where X : BaseModel
    {
        private Context context;
        private DbSet<X> entities = null;
        private string errorMessage = string.Empty;

        public LocationRepository()
        {
            context = new Context();
        }

        #region CRUD
        public X Create(X entity)
        {
            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException("entity");
                }
                Entities.Add(entity);
                context.SaveChanges();

                return Entities.Find(entity.Id);
            }
            catch (DbEntityValidationException dbEx)
            {

                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        errorMessage += string.Format("Property: {0} Error: {1}",
                        validationError.PropertyName, validationError.ErrorMessage) + Environment.NewLine;
                    }
                }
                throw new Exception(errorMessage, dbEx);
            }
        }

        public X Read(int id)
        {
            return Entities.Find(id);
        }

        public void Update(X entity)
        {
            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException("entity");
                }
                X Original = Entities.Find(entity.Id);

                if (Original != null)
                {
                    context.Entry(Original).CurrentValues.SetValues(entity);
                    context.SaveChanges();
                }
                else
                {
                    throw new Exception(entity + " not found");
                }
            }
            catch (DbEntityValidationException dbEx)
            {

                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        errorMessage += string.Format("Property: {0} Error: {1}",
                        validationError.PropertyName, validationError.ErrorMessage) + Environment.NewLine;
                    }
                }
                throw new Exception(errorMessage, dbEx);
            }

        }

        public void Delete(X entity)
        {
            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException("entity");
                }
                Entities.Remove(entity);
                context.SaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {

                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        errorMessage += string.Format("Property: {0} Error: {1}",
                        validationError.PropertyName, validationError.ErrorMessage) + Environment.NewLine;
                    }
                }
                throw new Exception(errorMessage, dbEx);
            }
        }

        #endregion

        public DbSet<X> List()
        {
            return Entities;
        }

        private DbSet<X> Entities
        {
            get
            {
                if (entities == null)
                {
                    entities = context.Set<X>();
                }
                return entities;
            }
        }

        public void Dispose()
        {
            context.Dispose();
        }
    }
}