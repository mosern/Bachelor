using AdmAspNet.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AdmAspNet.Helpers
{
    /// <summary>
    /// A custom binder to prevent overposting when posting accesspoint
    /// </summary>
    public class AccessPointBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelType == typeof(AccessPointViewModel))
            {
                HttpRequestBase request = controllerContext.HttpContext.Request;
                string macAddress = request.Form.Get("MacAddress"); 
                string desc = request.Form.Get("Desc");
                //Parse
                double lng, lat, alt;
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

                //Create the object with necessary fields
                return new AccessPointViewModel
                {
                    Desc = desc,
                    MacAddress = macAddress,
                    Coordinate = new CoordinateViewModel { Lng = lng, Lat = lat, Alt = alt }
                };
            }
            else
            {
                return base.BindModel(controllerContext, bindingContext);
            }
        }
    }
}