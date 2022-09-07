using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

namespace PockemonCards.Network
{
    public class WebRequestClient
    {
        private const int BATCH_SIZE = 100;

        /// <summary>
        /// Starts a GET request and converts the response into the generic TResultType
        /// </summary>
        /// <typeparam name="TResultType">The generic type used for conversion</typeparam>
        /// <param name="url">The base url used for the GET request</param>
        /// <returns></returns>
        public async Task<TResultType> GetAsync<TResultType>(string url)
        {
            using var www = UnityWebRequest.Get(url);

            www.SetRequestHeader("Content-Type", "application/json");

            var operation = www.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log($"Failed: {www.error}");
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
                return default;
            }
        }

        /// <summary>
        /// Starts multiple GET requests that will run in parallel and in batches of BATCH_SIZE
        /// The results will be converted into the generic TResultType
        /// </summary>
        /// <typeparam name="TResultType">The generic type used for conversion</typeparam>
        /// <param name="urls">The list of base urls used for the GET request</param>
        /// <returns></returns>
        public async Task<IEnumerable<TResultType>> GetAllAsync<TResultType>(IEnumerable<string> urls)
        {
            var results = new List<TResultType>();

            int numberOfBatches = (int)Math.Ceiling((double)urls.Count() / BATCH_SIZE);

            for (int i = 0; i < numberOfBatches; i++)
            {
                var currentUrls = urls.Skip(i * BATCH_SIZE).Take(BATCH_SIZE);
                var tasks = currentUrls.Select(url => GetAsync<TResultType>(url));
                results.AddRange(await Task.WhenAll(tasks));
            }

            return results;
        }
    }
}
