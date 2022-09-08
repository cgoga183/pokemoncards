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
        private const int BATCH_SIZE = 100;
        //it will track all the failed request to be used for processing errors
        private List<string> _failedUrls = new List<string>();
        
        /// <summary>
        /// Returns a list of all failed requests that happend during get
        /// </summary>
        /// <returns></returns>
        public List<string> GetFailedUrls()
        {
            return _failedUrls;
        }

        /// <summary>
        /// It will return true if there were any failed requests
        /// </summary>
        /// <returns></returns>
        public bool HadFailedRequests()
        {
            return _failedUrls.Count > 0;
        }

        /// <summary>
        /// Starts a GET request and converts the response into the generic TResultType
        /// For every failed request or error it will add the url to the _failedUrls list
        /// </summary>
        /// <typeparam name="TResultType">The generic type used for conversion</typeparam>
        /// <param name="url">The base url used for the GET request</param>
        /// <returns></returns>
        public async Task<TResultType> GetAsync<TResultType>(string url)
        {
            using var www = UnityWebRequest.Get(url);

            www.SetRequestHeader("Content-Type", "application/json");
            www.timeout = 1;
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
                Debug.Log($"Success: {www.downloadHandler.text}");
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
        /// Starts multiple GET requests that will run in parallel and in batches of BATCH_SIZE
        /// The results will be converted into the generic TResultType. 
        /// A UnityEvent will be triggered when a batch of requests is completed
        /// </summary>
        /// <typeparam name="TResultType"></typeparam>
        /// <param name="urls">The list of request urls</param>
        /// <param name="updateProgress">A UnityEvent that will accept as params the number of completed request and the total number of requets</param>
        /// <returns></returns>
        public async Task<IEnumerable<TResultType>> GetAllAsync<TResultType>(IEnumerable<string> urls, UnityEvent<int, int> updateProgress)
        {
            _failedUrls.Clear();

            var results = new List<TResultType>();
            int totalUrlCount = urls.Count();

            int numberOfBatches = (int)Math.Ceiling((double)totalUrlCount / BATCH_SIZE);

            for (int i = 0; i < numberOfBatches; i++)
            {
                var currentUrls = urls.Skip(i * BATCH_SIZE).Take(BATCH_SIZE);
                var tasks = currentUrls.Select(url => GetAsync<TResultType>(url));
                results.AddRange(await Task.WhenAll(tasks));
                updateProgress?.Invoke(results.Count, totalUrlCount);
            }

            return results;
        }
    }
}
