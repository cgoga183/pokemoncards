using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using UnityEngine.Events;

namespace PockemonCards.Network
{
    public class WebRequestClient
    {
        //the maximum size of batched request started at once
        private const int BATCH_SIZE = 50;
        //it will track all the failed request
        private List<string> _failedUrls = new List<string>();

        /// <summary>
        /// Returns a list of all failed requests that happend when triggering multiple requests in parallel
        /// The list will reset when the public GetAllAsync or GetAnyAsync calls will be triggered
        /// </summary>
        /// <returns></returns>
        public List<string> GetFailedUrls()
        {
            return _failedUrls;
        }

        /// <summary>
        /// It will return true if there were any failed requests when triggering multiple requests in parallel
        /// </summary>
        /// <returns></returns>
        public bool HadFailedRequests()
        {
            return _failedUrls.Count > 0;
        }

        /// <summary>
        /// Starts a GET request and converts the response into the generic TResultType
        /// If the request will fail or trigger an error the url will be added to the _failedUrls list
        /// </summary>
        /// <typeparam name="TResultType">The generic type used for conversion</typeparam>
        /// <param name="url">The base url used for the GET request</param>
        /// <param name="timeout">The maximum timeout for the request. The default is set to 1 second.</param>
        /// <returns>The reponse converted into TResultType</returns>
        private async Task<TResultType> GetAsync<TResultType>(string url, int timeout = 1)
        {
            using UnityWebRequest www = UnityWebRequest.Get(url);

            www.SetRequestHeader("Content-Type", "application/json");
            www.timeout = timeout;
            var operation = www.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log($"Failed: {www.error}");
                _failedUrls.Add(url);
                return default;
            }
            
            var jsonResponse = www.downloadHandler.text;
            try
            {
                var result = JsonConvert.DeserializeObject<TResultType>(jsonResponse);
               // Debug.Log($"Success: {www.downloadHandler.text}");
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{this} Could not parse response {jsonResponse} {ex.Message}");
                _failedUrls.Add(url);
                return default;
            }
        }

        /// <summary>
        /// Starts multiple GET requests that will run in parallel and in batches of BATCH_SIZE using Task.WhenAll
        /// The results will be converted into a generic TResultType list and only the succesful ones will be added.
        /// A UnityEvent will be triggered for every succesful request
        /// </summary>
        /// <typeparam name="TResultType">he generic type used for conversion</typeparam>
        /// <param name="urls">The list of request urls</param>
        /// <param name="onRequestSuccesful">The Unity event that will be triggered on a succesful request</param>
        /// <returns>A list of succesful respones</returns>
        public async Task<IEnumerable<TResultType>> GetAllAsync<TResultType>(IEnumerable<string> urls, UnityEvent onRequestSuccesful)
        {
            _failedUrls.Clear();

            var results = new List<TResultType>();
            int numberOfBatches = (int)Math.Ceiling((double)urls.Count() / BATCH_SIZE);

            for (int i = 0; i < numberOfBatches; i++)
            {
                var currentUrls = urls.Skip(i * BATCH_SIZE).Take(BATCH_SIZE);
                var tasks = currentUrls.Select(url => GetAsync<TResultType>(url));
                results.AddRange(await Task.WhenAll(tasks));
                //only adding the succesful requests in the result list. if the result is null it means it failed
                foreach (var result in results)
                {
                    if (result != null)
                        onRequestSuccesful?.Invoke();
                }
            }

            return results;
        }

        /// <summary>
        /// Starts multiple GET requests that will run in parallel and in batches of BATCH_SIZE using Task.WhenAny
        /// The results will be converted into a generic TResultType list and only the succesful ones will be added.
        /// A UnityEvent will be triggered for every succesful request
        /// </summary>
        /// <typeparam name="TResultType">he generic type used for conversion</typeparam>
        /// <param name="urls">The list of request urls</param>
        /// <param name="onRequestSuccesful">The Unity event that will be triggered on a succesful request</param>
        /// <returns>A list of succesful respones</returns>
        public async Task<IEnumerable<TResultType>> GetAnyAsync<TResultType>(IEnumerable<string> urls, UnityEvent onRequestSuccesful)
        {
            _failedUrls.Clear();

            List<TResultType> results = new List<TResultType>();
            int numberOfBatches = (int)Math.Ceiling((double)urls.Count() / BATCH_SIZE);

            for (int i = 0; i < numberOfBatches; i++)
            {
                var currentUrls = urls.Skip(i * BATCH_SIZE).Take(BATCH_SIZE);
                IEnumerable<Task<TResultType>> tasksQuery = currentUrls.Select(url => GetAsync<TResultType>(url));
                List<Task<TResultType>> tasksList = tasksQuery.ToList();
                
                while (tasksList.Count > 0)
                {
                    Task<TResultType> finishedTask = await Task.WhenAny(tasksList);
                    tasksList.Remove(finishedTask);
                    //only adding the succesful requests in the result list. if the result is null it means it failed
                    if (finishedTask.Result != null)
                    {
                        results.Add(finishedTask.Result);
                        onRequestSuccesful?.Invoke();
                    }
                }
            }

            return results;
        }
    }
}
