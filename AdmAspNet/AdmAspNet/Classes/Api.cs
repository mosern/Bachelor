﻿using AdmAspNet.Models.DataContracts;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;

namespace AdmAspNet.Classes
{
    /// <summary>
    /// A class for doing all calls to the API 
    /// </summary>
    public class Api
    {
        /// <summary>
        /// The base address for the API
        /// </summary>
        private static string apiBaseAddress = ConfigurationManager.AppSettings["apiBaseAddress"];
        
        /// <summary>
        /// Bearer token
        /// </summary>
        private string token; 

        /// <summary>
        /// An overloaded constructor that accepts a token
        /// </summary>
        /// <param name="_token">Bearer token of the current session</param>
        public Api(string _token)
        {
            token = _token;  
        }

        /// <summary>
        /// Standard constructor
        /// </summary>
        public Api()
        {
            token = null; 
        }
        #region LocationMethods
        /// <summary>
        /// Returns all the locations (rooms and similar) in the database 
        /// </summary>
        /// <returns>A list of all locations</returns>
        public List<Location> GetAllLocations()
        {
            string url = ConfigurationManager.AppSettings["apiLocations"] + "/?asObject=false";
            List<Location> returnList = CallApi<List<Location>>(url);
            return returnList; 
        }

        /// <summary>
        /// Gets a location by id 
        /// </summary>
        /// <param name="id">ID of the location</param>
        /// <returns>The location object that matches the same ID</returns>
        public Location GetLocationById(int id)
        {
            string url = ConfigurationManager.AppSettings["apiLocations"] + "/"+id;
            Location returnObject = CallApi<Location>(url); 
            return returnObject; 
        }

        /// <summary>
        /// Post a location to the API
        /// </summary>
        /// <param name="location">The location to be added</param>
        /// <returns>True if added, false if not</returns>
        public bool PostLocation(Location location)
        {
            string url = ConfigurationManager.AppSettings["apiLocations"];
            return PostApi(url, location);
        }

        public bool UpdateLocation(int id, Location location)
        {
            string url = ConfigurationManager.AppSettings["apiLocations"]+"/"+id;
            return PatchApi(url, location); 
        }
        #endregion

        #region UserMethods
        /// <summary>
        /// Get a user by id
        /// </summary>
        /// <param name="id">The id of the user</param>
        /// <returns>The user that matches the ID</returns>
        public User GetUserById(int id)
        {
            string url = ConfigurationManager.AppSettings["apiUsers"] + "/" + id;
            User returnObject = CallApi<User>(url);
            return returnObject; 
        }

        /// <summary>
        /// Returns all the users in the database
        /// </summary>
        /// <returns>A list of all users</returns>
        public List<User> GetAllUsers()
        {
            string url = ConfigurationManager.AppSettings["apiUsers"] + "/?asObject=false";
            List<User> returnList = CallApi<List<User>>(url);
            return returnList;
        }
        #endregion

        #region TypeMethods
        /// <summary>
        /// Return a list with all types from the API
        /// </summary>
        /// <returns>A list with all types</returns>
        public List<Models.DataContracts.Type> GetAllTypes()
        {
            string url = ConfigurationManager.AppSettings["apiTypes"] + "/?asObject=false";
            List<Models.DataContracts.Type> returnList = CallApi<List<Models.DataContracts.Type>>(url);
            return returnList; 
        }
        #endregion

        #region HelperMethods
        /// <summary>
        /// Handle GET calls to the API with a generic type
        /// </summary>
        /// <typeparam name="T">Indicates which type of data to return. Usually a DataContract class.</typeparam>
        /// <param name="url">The URL to call in the API</param>
        /// <returns>Returns the parameter specified in the typeparam</returns>
        private T CallApi<T>(string url) where T : class 
        {
            using (var client = SetupClient())
            {
                HttpResponseMessage response = client.GetAsync(url).Result;
                if (response.IsSuccessStatusCode)
                {
                    DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(T));
                    Stream stream = response.Content.ReadAsStreamAsync().Result;
                    object objResponse = jsonSerializer.ReadObject(stream);
                    T jsonResponse = objResponse as T;
                    return jsonResponse; 

                }

            }
            return null; 
        }

        /// <summary>
        /// Handle POST calls to the API with a generic type
        /// </summary>
        /// <typeparam name="T">The type to post</typeparam>
        /// <param name="url">The URL to post to</param>
        /// <param name="data">The data to post</param>
        /// <returns>True if the data was posted, false if not</returns>
        private bool PostApi<T>(string url,T data) where T : class
        {
            using (var client = SetupClient())
            {
                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(T));
                using (MemoryStream ms = new MemoryStream())
                {
                    jsonSerializer.WriteObject(ms, data);
                    string jsonData = Encoding.Default.GetString(ms.ToArray());
                    StringContent stringContent = new StringContent(jsonData, Encoding.Default,"application/json");
                    HttpResponseMessage response = client.PostAsync(url, stringContent).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        return true; 
                    }
                }
            }
            return false;
        }

        private bool PatchApi<T>(string url, T data) where T : class {
            using (var client = SetupClient())
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(T));
                    jsonSerializer.WriteObject(ms, data);
                    string jsonData = Encoding.Default.GetString(ms.ToArray());
                    StringContent stringContent = new StringContent(jsonData, Encoding.Default, "application/json");
                    HttpResponseMessage response = client.PutAsync(url, stringContent).Result; 
                    if (response.IsSuccessStatusCode)
                    {
                        return true; 
                    }
                }
            }
            return false; 
        }
        /// <summary>
        /// Handle the deletion of an object that 
        /// </summary>
        /// <param name="url">The url to delete</param>
        /// <returns>True if the data was deleted, false if not</returns>
        private bool DeleteObject(string url)
        {
            using (var client = SetupClient())
            {
                HttpResponseMessage response = client.DeleteAsync(url).Result; 
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            return false; 
        }

        /// <summary>
        /// Sets all the standard setting for the HTTP client
        /// </summary>
        /// <returns>A HTTP client with the standard settings</returns>
        private HttpClient SetupClient()
        {
            HttpClient client = new HttpClient(); 
                if (token != null)
                {
                    client.SetBearerToken(token);
                }
                client.BaseAddress = new Uri(apiBaseAddress);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                return client; 
        }
        #endregion

    }
}