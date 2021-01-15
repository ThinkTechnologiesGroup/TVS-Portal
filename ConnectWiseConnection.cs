using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using RestSharp;

using ThinkVoip.Models;

using ThinkVoipTool;

namespace ThinkVoip
{
    public class ConnectWiseConnection : ConnectWiseModel
    {

        private const string InitialUrl = "https://cw.think-team.com/login/companyinfo/think";
        

        public ConnectWiseConnection(string User, string Pass)
        {
            var restClient = new RestClient(InitialUrl);
            var restRequest = new RestRequest(Method.GET);
            var restResponse = restClient.Execute(restRequest);
            JsonConvert.PopulateObject(restResponse.Content, this);
            ApiUrl = "https://" + SiteUrl + "/" + Codebase + "apis/3.0/";
            AuthKey = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{CompanyName}+{User}:{Pass}"));
        }


        private string AuthKey { get; }
        private string ApiUrl { get; }


       
        public async Task<string> GetRestResponse(Method method, string query)
        {
            var newRequest = new RestRequest(method);
            var restClient = new RestClient(ApiUrl + query);
            newRequest.AddHeader("Content-Type", "application/json");
            newRequest.AddHeader("Authorization", "Basic " + AuthKey);
            newRequest.AddHeader("Accept", "application/json");
            newRequest.AddHeader("clientId", "8f1701b8-e616-49a4-8bac-51349804e5de");
            var result = await restClient.ExecuteAsync(newRequest).ConfigureAwait(false);
            return result.Content;
        }

        public async Task<List<CompanyModel.Agreement>> GetAllTvsVoIpClients()
        {
            var newRequest = new RestRequest(Method.GET);
            var restClient = new RestClient(ApiUrl +
                                            "finance/agreements?conditions=department/name=\"TVS - Cloud VoIP Services\" AND agreementStatus = \"Active\" AND noEndingDateFlag = true");
            newRequest.AddHeader("Content-Type", "application/json");
            newRequest.AddHeader("Authorization", "Basic " + AuthKey);
            newRequest.AddHeader("Accept", "application/json");
            newRequest.AddHeader("clientId", "8f1701b8-e616-49a4-8bac-51349804e5de");
            var response = await restClient.ExecuteAsync(newRequest).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<List<CompanyModel.Agreement>>(response.Content);
        }

        public async Task<List<CompanyModel.Agreement>> GetAllThinkVoIpClients()
        {
            var newRequest = new RestRequest(Method.GET);
            var restClient =
                new RestClient(ApiUrl +
                               "finance/agreements?conditions=department/name=\"TTG - VoIP Services\" AND agreementStatus = \"Active\" AND noEndingDateFlag = true");
            newRequest.AddHeader("Content-Type", "application/json");
            newRequest.AddHeader("Authorization", "Basic " + AuthKey);
            newRequest.AddHeader("Accept", "application/json");
            newRequest.AddHeader("clientId", "8f1701b8-e616-49a4-8bac-51349804e5de");
            var response = await restClient.ExecuteAsync(newRequest).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<List<CompanyModel.Agreement>>(response.Content);
        }
        public async Task<List<CompanyModel.Agreement>> GetAllVoIpClients()
        {
            var tvs = await GetAllThinkVoIpClients();
            var ttg = await GetAllThinkVoIpClients();
            return new List<CompanyModel.Agreement>()
            {
                //tvs,
                //ttg
            };
        }

        public async Task<CompanyModel> GetCompany(int id)
        {
            var newRequest = new RestRequest(Method.GET);
            var restClient = new RestClient(ApiUrl + "company/companies/" + id.ToString());
            newRequest.AddHeader("Content-Type", "application/json");
            newRequest.AddHeader("Authorization", "Basic " + AuthKey);
            newRequest.AddHeader("Accept", "application/json");
            newRequest.AddHeader("clientId", "8f1701b8-e616-49a4-8bac-51349804e5de");
            var response = await restClient.ExecuteAsync(newRequest).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<CompanyModel>(response.Content);
        }
    }
}