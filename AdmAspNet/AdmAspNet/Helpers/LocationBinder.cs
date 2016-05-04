using AdmAspNet.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
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
                if (!double.TryParse(request.Form.Get("Coordinate.Lng"),NumberStyles.Any,CultureInfo.GetCultureInfo("nb-NO"),out lng))
                {
                    bindingContext.ModelState.AddModelError("Coordinate.Lng", "Breddegrad må være en gyldig double"); 
                }
                if (!double.TryParse(request.Form.Get("Coordinate.Lat"),NumberStyles.Any,CultureInfo.GetCultureInfo("nb-NO"),out lat))
                {
                    bindingContext.ModelState.AddModelError("Coordinate.Lat", "Lengdegrad må være en gyldig double"); 
                }
                if (!double.TryParse(request.Form.Get("Coordinate.Alt"),NumberStyles.Any,CultureInfo.GetCultureInfo("nb-NO"),out alt))
                {
                    bindingContext.ModelState.AddModelError("Coordinate.Alt", "Høyde må være en gyldig double"); 
                }

                int typeId, neighbourId;
                double distance;  
                if (!int.TryParse(request.Form.Get("Type.Id"), out typeId)) {
                    bindingContext.ModelState.AddModelError("Type.Id", "ID må være ett tall"); 
                }
                if (!int.TryParse(request.Form.Get("NeighbourId"),out neighbourId)) 
                {
                    bindingContext.ModelState.AddModelError("NeighbourId", "NeighbourId må være ett tall"); 
                }
                if (!double.TryParse(request.Form.Get("Distance"),NumberStyles.Any,CultureInfo.GetCultureInfo("nb-NO"),out distance))
                {
                    bindingContext.ModelState.AddModelError("Distance", "Distance må være ett tall"); 
                }
                return new LocationViewModel
                {
                    Name = name,
                    LocNr = locNr,
                    Desc = desc,
                    NeighbourId = neighbourId,
                    Distance = distance
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