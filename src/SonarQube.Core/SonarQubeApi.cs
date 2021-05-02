using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace SonarQube.Core
{
    public class SonarQubeApi
    {
        private string _apiUrl;
        private string _userName;
        private string _password;

        public SonarQubeApi(string apiUrl, string userName, string password)
        {
            _apiUrl = apiUrl;
            _userName = userName;
            _password = password;
        }

        public string LastErrorMessage { get; set; }

        public virtual T ExecuteRequest<T>(string action)
        {
            using (var client = new HttpClient())
            {
                var byteArray = Encoding.ASCII.GetBytes($"{_userName}:{_password}");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var fullUrl = _apiUrl + action;

                using (var response = client.GetAsync(fullUrl).Result)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        using (var content = response.Content)
                        {
                            string result = content.ReadAsStringAsync().Result;
                            var info = JsonConvert.DeserializeObject<T>(result);
                            return info;
                        }
                    }
                    else
                    {
                        LastErrorMessage = $"{response.StatusCode} - {response.ReasonPhrase} : {fullUrl}";
                        return default(T);
                    }
                }
            }
        }
    }    
}