using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.IdentityModel.Claims;
using System.IdentityModel.Tokens;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace AdmAspNet
{
    public partial class Startup
    {
        private string ClientId = ConfigurationManager.AppSettings["ClientId"];
        private string Authority = ConfigurationManager.AppSettings["Authority"];
        private string RedirectURI = ConfigurationManager.AppSettings["RedirectURI"];

        //Inspired by the Pluralsight course "Building and Securing a RESTful API for Multiple Clients in ASP.NET", my code may be similar to the code in the exercise-files.
        //https://app.pluralsight.com/library/courses/building-securing-restful-api-aspdotnet/exercise-files
        public void ConfigureAuth(IAppBuilder app)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "Cookies"
            });

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
                {
                    ClientId = ClientId,
                    Authority = Authority,
                    RedirectUri = RedirectURI,
                    SignInAsAuthenticationType = "Cookies",
                    ResponseType="id_token",

                    Notifications = new OpenIdConnectAuthenticationNotifications()
                    {
                        MessageReceived = async n =>
                        {
                            DecodeAndWrite(n.ProtocolMessage.IdToken);
                        }
                    }

                });
        }

        //Writen by Kevin Dockx as a part of the Pluralsight course "Building and Securing a RESTful API for Multiple Clients in ASP.NET"
        //https://app.pluralsight.com/library/courses/building-securing-restful-api-aspdotnet/exercise-files
        public static void DecodeAndWrite(string token)
        {
            try
            {


                var parts = token.Split('.');

                string partToConvert = parts[1];
                partToConvert = partToConvert.Replace('-', '+');
                partToConvert = partToConvert.Replace('_', '/');
                switch (partToConvert.Length % 4)
                {
                    case 0:
                        break;
                    case 2:
                        partToConvert += "==";
                        break;
                    case 3:
                        partToConvert += "=";
                        break;
                    default:
                        break;
                }

                var partAsBytes = Convert.FromBase64String(partToConvert);
                var partAsUTF8String = Encoding.UTF8.GetString(partAsBytes, 0, partAsBytes.Count());

                // Json .NET
                var jwt = JObject.Parse(partAsUTF8String);

                // Write to output
                Debug.Write(jwt.ToString());

            }
            catch (Exception ex)
            {
                // something went wrong
                Debug.Write(ex.Message);

            }
        }
    }
}
