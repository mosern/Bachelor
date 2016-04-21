using AdmAspNet.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AdmAspNet.Helpers
{
    public class LocationBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelType == typeof(LocationViewModel))
            {
                HttpRequestBase request = controllerContext.HttpContext.Request;
                string name = request.Form.Get("Name");
                string locNr = request.Form.Get("LocNr");
                string desc = request.Form.Get("Desc"); 
                //Gather everything necessary 
                double lng,lat,alt;
                if (!double.TryParse(request.Form.Get("Coordinate.Lng"), out lng))
                {
                    bindingContext.ModelState.AddModelError("Coordinate.Lng", "Breddegrad må være en gyldig double"); 
                }
                if (!double.TryParse(request.Form.Get("Coordinate.Lat"), out lat))
                {
                    bindingContext.ModelState.AddModelError("Coordinate.Lat", "Lengdegrad må være en gyldig double"); 
                }
                if (!double.TryParse(request.Form.Get("Coordinate.Alt"), out alt))
                {
                    bindingContext.ModelState.AddModelError("Coordinate.Alt", "Høyde må være en gyldig double"); 
                }

                int typeId; 
                if (!int.TryParse(request.Form.Get("Type.Id"), out typeId)) {
                    bindingContext.ModelState.AddModelError("Type.Id", "ID må være ett tall"); 
                }
                return new LocationViewModel
                {
                    Name = name,
                    LocNr = locNr,
                    Desc = desc,
                    Coordinate = new CoordinateViewModel {Lng = lng,Lat = lat, Alt = alt},
                    Type = new TypeViewModel { Id = typeId}
                }; 
            }
            else
            {
                return base.BindModel(controllerContext, bindingContext); 
            }
        }
    }
}