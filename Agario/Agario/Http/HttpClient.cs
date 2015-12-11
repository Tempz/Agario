/*
	Copyright (C) 2015 Tempz@users.noreply.github.com

	This file is part of https://github.com/Tempz/Agario

	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using Newtonsoft.Json.Linq;

namespace Agario.Http
{
    internal class HttpClient
    {
        private readonly CookieContainer sessionCookieContainer;

        private readonly IDictionary<string, string> sharedCustomHeaders = new Dictionary<string, string>();

        public HttpClient()
        {
            sessionCookieContainer = new CookieContainer();
        }

        public HttpResponseInfo SendGet(Uri url, Dictionary<string, string> customHeaders = null)
        {
            var request = InitRequest(url, customHeaders);
            return InitResponse((HttpWebResponse)request.GetResponse(), null, null, "GET", customHeaders);
        }

        public HttpResponseInfo SendPost(Uri url, Dictionary<string, string> postParameters, Dictionary<string, string> customHeaders = null )
        {
            return SendPost(url, InitPostParameters(postParameters), "application/x-www-form-urlencoded", customHeaders);
        }

        public HttpResponseInfo SendPost(Uri url, JObject json, Dictionary<string, string> customHeaders = null )
        {
            return SendPost(url, json.ToString(), "application/json", customHeaders);
        }

        public HttpResponseInfo SendPost(Uri url, string postData, string contentTypeHeader, Dictionary<string, string> customHeaders = null )
        {
            var request = InitRequest(url, customHeaders);
            SetupPostRequest(request, postData, contentTypeHeader, "POST");
            return InitResponse((HttpWebResponse)request.GetResponse(), postData, contentTypeHeader, "POST", customHeaders);
        }

        public HttpResponseInfo SendPut(Uri url, Dictionary<string, string> customHeaders = null)
        {
            var request = InitRequest(url, customHeaders);
            request.Method = "PUT";
            return InitResponse((HttpWebResponse)request.GetResponse(), null, null, "PUT", customHeaders);
        }

        public HttpResponseInfo SendPut(Uri url, JObject json, Dictionary<string, string> customHeaders = null)
        {
            return SendPut(url, json.ToString(), "application/json", customHeaders);
        }

        public HttpResponseInfo SendPut(Uri url, string putData, string contentTypeHeader, Dictionary<string, string> customHeaders = null)
        {
            var request = InitRequest(url, customHeaders);
            SetupPostRequest(request, putData, contentTypeHeader, "PUT");
            return InitResponse((HttpWebResponse)request.GetResponse(), putData, contentTypeHeader, "PUT", customHeaders);
        }

        public void UpdateSharedCustomHeader(string key, string value)
        {
            sharedCustomHeaders[key] = value;
        }

        private string InitPostParameters(Dictionary<string, string> postParameters)
        {
            string postData = "";

            foreach (string key in postParameters.Keys)
            {
                postData += HttpUtility.UrlEncode(key) + "="
                      + HttpUtility.UrlEncode(postParameters[key]) + "&";
            }
            return postData;
        }

        private HttpResponseInfo InitResponse(HttpWebResponse response,string postData, string method, string contentTypeHeader, Dictionary<string,string> customHeaders = null)
        {
            HttpWebRequest request;
            while (response.StatusCode == HttpStatusCode.Found || response.StatusCode == HttpStatusCode.MovedPermanently)
            {
                response.Close();
                request = InitRequest(new Uri(response.Headers["Location"]), customHeaders);
                if (!string.IsNullOrEmpty(postData))
                {
                    SetupPostRequest(request, postData, contentTypeHeader, method);
                }
                response = (HttpWebResponse)request.GetResponseNoThrow();
            }

            var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            var result = reader.ReadToEnd();
            reader.Dispose();

            return new HttpResponseInfo()
            {
                ResponseData = result,
                StatusCode = response.StatusCode,
                ResponseUrl = response.ResponseUri,
                ResponseHeaders = response.Headers.GetHeaders()
            };
        }

        private void SetupPostRequest(HttpWebRequest request, string postData, string contentTypeHeader, string method)
        {
            request.Method = method;
            request.ContentType = contentTypeHeader;
            var postBytes = Encoding.UTF8.GetBytes(postData);
            request.ContentLength = postBytes.Length;
            var stream = request.GetRequestStream();
            stream.Write(postBytes, 0, postBytes.Length);
            stream.Close();
        }

        private HttpWebRequest InitRequest(Uri url, Dictionary<string, string> customHeaders = null)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            
            request.CookieContainer = sessionCookieContainer;
            request.AllowAutoRedirect = false;

            if(customHeaders != null) 
                foreach (var customHeader in customHeaders)
                {
                    request.Headers.Add(customHeader.Key, customHeader.Value);
                }
            foreach (var customHeader in sharedCustomHeaders)
            {
                request.Headers.Add(customHeader.Key, customHeader.Value);
            }
            return request;
        }
    }
}