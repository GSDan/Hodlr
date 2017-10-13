﻿using Newtonsoft.Json;
using RestSharp.Portable;
using RestSharp.Portable.HttpClient;
using System;
using System.Threading.Tasks;

namespace Hodlr
{
    public class ServerResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }

    public static class Comms
    {
        public static string BlockChainApi = "https://blockchain.info/";
        public static string ConverterApi = "http://api.fixer.io/";

        public static async Task<ServerResponse<T>> Get<T>(string apiBase, string route)
        {
            try
            {
                RestClient restClient = new RestClient(apiBase);
                IRestResponse resp = await restClient.Execute(new RestRequest(route, Method.GET));

                ServerResponse<T> toRet = new ServerResponse<T>
                {
                    Success = resp.IsSuccess,
                    Message = resp.StatusDescription
                };

                if (toRet.Success && typeof(T) != typeof(string))
                {
                    toRet.Data = JsonConvert.DeserializeObject<T>(resp.Content);
                }

                return toRet;
            }
            catch (Exception e)
            {
                return new ServerResponse<T>
                {
                    Success = false,
                    Message = e.Message
                };
            }
        }

    }
}
