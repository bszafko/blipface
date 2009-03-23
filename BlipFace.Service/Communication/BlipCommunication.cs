﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Http;
using System.Runtime.Serialization.Json;
using System.Net;
using BlipFace.Service.Entities;
using System.Runtime.Serialization;

namespace BlipFace.Service.Communication
{

    /// <summary>
    /// Główna klasa służąca do komunikacji z api blip.pl
    /// 
    /// Na razie zastanawiamy się czy to nie będzie singleton,
    /// a może same metody statyczne?
    /// </summary>
    public class BlipCommunication
    {


        public event EventHandler<StatusesLoadingEventArgs> StatusesLoaded;

        /// <summary>
        /// Klasa z WCF Rest Starter Kit (http://msdn.microsoft.com/netframework/cc950529(en-us).aspx)
        /// </summary>
        private HttpClient blipHttpClient = new HttpClient("http://api.blip.pl/");


        private string userName;
        private string password;

        /// <summary>
        /// Konstruktor, ustawia dane do autentykacji, oraz niezbędne
        /// nagłówki do komunikacji z blipem
        /// </summary>
        /// <param name="_userName">nazwa użytkownika</param>
        /// <param name="_password">hasło</param>
        public BlipCommunication(string _userName, string _password)
        {

            userName = _userName;
            password = _password;

            //potrzeba dodać obowiązkowy nagłówek gdy korzystamy z api blip'a
            blipHttpClient.DefaultHeaders.Add("X-Blip-API", "0.02");

            //także wymagane przez blipa
            blipHttpClient.DefaultHeaders.Accept.Add(
                new Microsoft.Http.Headers.StringWithOptionalQuality("application/json"));

            //trzeba zakodować w base64 login:hasło - tak każe blip
            byte[] credentialBuffer = new UTF8Encoding().GetBytes(
                string.Format("{0}:{1}", userName, password));
            string authHeader = "Basic " + Convert.ToBase64String(credentialBuffer);

            //nagłówek autoryzacja - zakodowane w base64
            blipHttpClient.DefaultHeaders.Add("Authorization", authHeader);

            //ustawienie nagłówka UserAgent - po tym blip rozpoznaje transport
            blipHttpClient.DefaultHeaders.UserAgent.Add(
                new Microsoft.Http.Headers.ProductOrComment("BlipFace"));


        }


        /// <summary>
        /// Pobiera listę statusów, w sposób synchroniczny
        /// </summary>
        /// <param name="limit">limit statusów</param>
        /// <returns></returns>
        public IList<BlipStatus> GetUpdates(int limit)
        {

            string query = string.Format("updates?include=user,user[avatar]&amp;limit={0}", limit.ToString());
            HttpResponseMessage resp = blipHttpClient.Get(query);
            //sprawdzamy czy komunikacja się powiodła
            resp.EnsureStatusIsSuccessful();


            var statuses = resp.Content.ReadAsJsonDataContract<StatusesList>();


            return statuses;
        }


        /// <summary>
        /// Pobiera statusy asynchronicznie, gdy już pobierze to zgłasza że pobrał
        /// i w callbacku ustawia statusy w widoku
        /// </summary>
        /// <param name="limi"></param>
        public void GetUpdatesAsync(int limit)
        {
            string query = string.Format("updates?include=user,user[avatar]&amp;limit={0}", limit.ToString());


            //jako state przekazujemy cały obiekt,aby można było pobrać później z niego ResponseMessage
            blipHttpClient.BeginSend(
                new HttpRequestMessage("GET", query),new AsyncCallback(AfterGetUpdates), blipHttpClient);
            
            //blipHttpClient.SendCompleted+=new EventHandler<SendCompletedEventArgs>(blipHttpClient_SendCompleted);
            //blipHttpClient.SendAsync(new HttpRequestMessage("GET",query);
        }

        /// <summary>
        /// Metoda asynchronicznie pobiera statusy z dashboardu użytkownika, gdy 
        /// zostaną pobrane zgłaszane jest zdarzenie 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="limit"></param>
        public void GetUserDashboard(string user, int limit)
        {
            string query = string.Format("/users/{0}/dashboard?include=user,user[avatar]&amp;limit={1}",user, limit.ToString());


            //jako state przekazujemy cały obiekt,aby można było pobrać później z niego ResponseMessage
            blipHttpClient.BeginSend(
                new HttpRequestMessage("GET", query), new AsyncCallback(AfterGetUpdates), blipHttpClient);
            
        }

        private void AfterGetUpdates(IAsyncResult result)
        {

            //pobieramy obiekt HttpClient, dzięki któremu został wysłany request
            //przekazaliśmy ten obiekt jako state
            var client = result.AsyncState as HttpClient;
            
            //pobieramy odpowiedź
            var resp = client.EndSend(result);

            //deserializujemy z json
            var statuses = resp.Content.ReadAsJsonDataContract<StatusesList>();

            //zgłaszamy zdarzenie że dane załadowaliśmy, przekazując nasze parametry zgłosznie wraz z statusami
            StatusesLoaded(this, new StatusesLoadingEventArgs(statuses));
        }



        public void AddStatusAsync(string content)
        {
            string query = "/updates"; 
            
            
            blipHttpClient.Post(query,HttpContent.Create(string.Format(@"body={0}",content)) );
            //jako state przekazujemy cały obiekt,aby można było pobrać później z niego ResponseMessage
          //  blipHttpClient.BeginSend(
          //      new HttpRequestMessage("POST", query), new AsyncCallback(AfterGetUpdates), blipHttpClient);
            
        }
    }



    /// <summary>
    /// klasa reprezentująca Statusy przekazane jako argumenty wywołania zdarzenia
    /// </summary>
    public class StatusesLoadingEventArgs : EventArgs
    {
        public IList<BlipStatus> Statuses { get; private set; }

        public StatusesLoadingEventArgs(IList<BlipStatus> statuses)
        {
            Statuses = statuses;
        }
    }
}
