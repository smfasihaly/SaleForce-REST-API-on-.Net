/*
  ~ Created By : Syed Fasih Ali
  ~ Dated : 21 January 2020

*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
namespace C_APISALESFORCE
{


    public class sObject
    /* SO_Object Class 
   * API return to thing a BOOL named done and List of records name records when we get records by Querry 
   * when we get from Sobject name API list of record as recentItems and List of metadata of object as describe
       I'm not using describe list here... 
   */
    {
        public bool done;
        public List<Records> records;
        public List<Records> recentItems; // recent item only contain Id and Name of object

    }

    public class Records
    // Records get from API these are only for Account object you have to change them according to object 
    {
        public string Id { get; set; }

        public string Name { get; set; }
        public string ShippingCity { get; set; }

        public string Phone { get; set; }

    }
 public class SalesforceClient
    {/*
        SalesforceClient access API
    */
        private const string LOGIN_ENDPOINT = "https://login.salesforce.com/services/oauth2/token";
        private const string API_ENDPOINT = "/services/data/v47.0/";

        public string Username { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string AuthToken { get; set; }
        public string InstanceUrl { get; set; }

        static SalesforceClient()
        {
            // SF requires TLS 1.1 or 1.2
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;
        }

        public void Login()
        {
            String jsonResponse;
            using (var client = new HttpClient())
            {
                var request = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"grant_type", "password"},
                    {"client_id", ClientId},
                    {"client_secret", ClientSecret},
                    {"username", Username},
                    {"password", Password + Token}
                }
                );
                request.Headers.Add("X-PrettyPrint", "1");
                var response = client.PostAsync(LOGIN_ENDPOINT, request).Result;
                jsonResponse = response.Content.ReadAsStringAsync().Result;
            }
            Console.WriteLine($"Response: {jsonResponse}");
            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonResponse);
            AuthToken = values["access_token"];
            InstanceUrl = values["instance_url"];
        }


        public string GetByID(string sObject, string Id)
        {
            using (var client = new HttpClient())
            {
                string restQuery = InstanceUrl + API_ENDPOINT + "sobjects/" + sObject + "/" + Id;
                var request = new HttpRequestMessage(HttpMethod.Get, restQuery);
                request.Headers.Add("Authorization", "Bearer " + AuthToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Add("X-PrettyPrint", "1");
                var response = client.SendAsync(request).Result;
                return response.Content.ReadAsStringAsync().Result;
            }
        }
        public string GetRecentItem(string sObject)
        {
            using (var client = new HttpClient())
            {
                string restQuery = InstanceUrl + API_ENDPOINT + "sobjects/" + sObject;
                var request = new HttpRequestMessage(HttpMethod.Get, restQuery);
                request.Headers.Add("Authorization", "Bearer " + AuthToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Add("X-PrettyPrint", "1");
                var response = client.SendAsync(request).Result;
                return response.Content.ReadAsStringAsync().Result;
            }
        }

        public string Query(string soqlQuery)
        {
            using (var client = new HttpClient())
            {
                string restRequest = InstanceUrl + API_ENDPOINT + "query/?q=" + soqlQuery;
                var request = new HttpRequestMessage(HttpMethod.Get, restRequest);
                request.Headers.Add("Authorization", "Bearer " + AuthToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Add("X-PrettyPrint", "1");
                var response = client.SendAsync(request).Result;
                return response.Content.ReadAsStringAsync().Result;
            }
        }
        public bool Delete(string sObject)
        {
            using (var client = new HttpClient())
            {
                string restQuery = InstanceUrl + API_ENDPOINT + "sobjects/" + sObject;
                var request = new HttpRequestMessage(HttpMethod.Delete, restQuery);
                request.Headers.Add("Authorization", "Bearer " + AuthToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = client.SendAsync(request).Result;
                return response.IsSuccessStatusCode;
            }
        }
        public string Add(string sObject, string JSON)
        {

            string restQuery = InstanceUrl + API_ENDPOINT + "sobjects/" + sObject;
            var request = HttpWebRequest.Create(restQuery);
            // request.ContentType = "application/json";
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Headers.Add("X-PrettyPrint", "1");
            // Attach the access token and JSON to the request to Salesforce.
            request.Headers.Add("Authorization: OAuth " + AuthToken);
            using (var requestWriter = new StreamWriter(request.GetRequestStream()))
            {
                requestWriter.Write(JSON);
                requestWriter.Flush();
                requestWriter.Close();
            }

            // Send the object to Salesforce
            var response = request.GetResponse();
            var data = "";
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                data = reader.ReadToEnd();
            }
            return data;

        }
        public bool UpdateRecord(string sObject, string JSON)
        {


            string restQuery = InstanceUrl + API_ENDPOINT + "sobjects/" + sObject;
            var request = HttpWebRequest.Create(restQuery);
            // request.ContentType = "application/json";
            request.Method = "Patch";
            request.ContentType = "application/json";
            request.Headers.Add("X-PrettyPrint", "1");
            // Attach the access token and JSON to the request to Salesforce.
            request.Headers.Add("Authorization: OAuth " + AuthToken);
            using (var requestWriter = new StreamWriter(request.GetRequestStream()))
            {
                requestWriter.Write(JSON);
                requestWriter.Flush();
                requestWriter.Close();
            }

            // Send the object to Salesforce
            try
            {
                var response = request.GetResponse();
                return true;
            }
            catch
            {
                return false;
            }

        }
    }
    public class Program
    {
        private static SalesforceClient CreateClient()
        {
            /*
                Credential of Your Salesforce
            */
            return new SalesforceClient
            {
                Username = "Your User Name",
                Password = "youPass",
                Token = "Your Token",
                ClientId = "Your ClientID",
                ClientSecret = "Your clientSecret",
            };
        }

        static void Main(string[] args)
        {
            // CreateClient() method to Create new client with your id, password, ClientId and Secret from your App 
            var client = CreateClient();
            // Login Method after login you'll get AuthToken and Instance Url
            client.Login();





            #region  Get from Querry
            // ============= To get Record From Query Query() will return string of json then  we desrialize it in our Sobject =========================            
            string json = client.Query("select Name, Id,ShippingCity From account");
            sObject values = JsonConvert.DeserializeObject<sObject>(json);
            foreach (Records account in values.records)
            {
                System.Console.WriteLine($"Name Of Record: {account.Name} and Shippping city : {account.ShippingCity}");
            }
            #endregion




            #region  Get from GetRecentItem(Sobject)
            /* ============= To get recentRecord of specific Object run GetRecentItem() will return string of json then  we deserialize 
                             it in our Sobject  object=========================  */
            string json_2 = client.GetRecentItem("account");
            sObject values_2 = JsonConvert.DeserializeObject<sObject>(json_2);
            foreach (Records account in values_2.recentItems)
            {
                System.Console.WriteLine($"Name Of Record: {account.Name} and Id : {account.Id}");
            }

            #endregion



            #region  Get from GetByID(SObject,ID)
            /* ============= To get recentRecord of specific Object run GetByID() will return string of json then  we deserialize 
                             it in our records object =========================  */
            string json_3 = client.GetByID("account", "0012w000002gPPWAA2");
            Records Record = JsonConvert.DeserializeObject<Records>(json_3);

            System.Console.WriteLine($"Name Of Record: {Record.Name} and Id : {Record.Id}");


            #endregion




            #region Adding a new record
            /* ============= for adding new record run Add method  and pass sobject and json of your object
                            return json {id , bool Success and Error[] if any}=========================  */
            Records newRecord = new Records() { Name = "new record from .net by S Fasih Ali" };
            string toJson = JsonConvert.SerializeObject(newRecord);
            // string _Json = client.Add("account", toJson);
            // System.Console.WriteLine(_Json);   
            #endregion



            #region Updating a record
            /* ============= for updating  a record run update method  and pass sobject/+Id and json of your object 
                            without ID only those properties you want to change
                            return no response }=========================  */
            Records UpdatableRecord = new Records() { Name = "updatted name", ShippingCity = "islamabad" };

            var id = "0012w000002gPPYAA2";
            string toJsonU = JsonConvert.SerializeObject(UpdatableRecord);
            client.UpdateRecord("account/" + id, toJsonU);
            #endregion



            #region Deleting a record
            /* ============= for deleting  a record run delete method  and pass sobject/+Id
                         return no response }=========================  */
            var idToDel = "0012w000002gPPUAA2";
            client.Delete("account/" + idToDel);
            #endregion
            Console.ReadLine();
        }
    }
   
}
