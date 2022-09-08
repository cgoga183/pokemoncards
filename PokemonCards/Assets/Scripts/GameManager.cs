using PockemonCards.Network;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace PockemonCards
{
    public class GameManager : MonoBehaviour
    {
        //if some requests will fail this is the max number of retries we are doing
        public const int NO_OF_RETRIES = 3;
        
        public static GameManager Instance;

        //event to be triggered when fetching all the pokemon info is completed
        public UnityEvent<List<PokemonDto>> OnPokemonInfoFetched;
        //event to be triggered to updated the progress bar as we fetch pokemon info
        public UnityEvent<int, int> UpdateProgressBar;

        //used for displaying errors or successes
        public ProgressContainer progressContainer;
        
        private WebRequestClient _webRequestClient = new WebRequestClient();
        private string[] _ownedPokemonNames = { "bulbasaur", "Charmander", "Piplup", "squirtle", "bulbasaur", "Charmander", "Piplup", "squirtle", "bulbasaur", "Charmander", "Piplup", "squirtle", "bulbasaur", "Charmander", "Piplup", "squirtle", "bulbasaur", "Charmander", "Piplup", "squirtle", "bulbasaur", "Charmander", "Piplup", "squirtle", "bulbasaur", "Charmander", "Piplup", "squirtle", "bulbasaur", "Charmander", "Piplup", "squirtle", "bulbasaur", "Charmander", "Piplup", "squirtle" , "bulbasaur", "Charmander", "Piplup", "squirtle" , "bulbasaur", "Charmander", "Piplup", "squirtle" , "bulbasaur", "Charmander", "Piplup", "squirtle" , "bulbasaur", "Charmander", "Piplup", "squirtle" , "bulbasaur", "Charmander", "Piplup", "squirtle" , "bulbasaur", "Charmander", "Piplup", "squirtle" , "bulbasaur", "Charmander", "Piplup", "squirtle" , "bulbasaur", "Charmander", "Piplup", "squirtle" , "bulbasaur", "Charmander", "Piplup", "squirtle" , "bulbasaur", "Charmander", "Piplup", "squirtle" , "bulbasaur", "Charmander", "Piplup", "squirtle" , "bulbasaur", "Charmander", "Piplup", "squirtle" , "bulbasaur", "Charmander", "Piplup", "squirtle", "bulbasaur", "Charmander", "Piplup", "squirtle", "bulbasaur", "Charmander", "Piplup", "squirtle", "bulbasaur", "Charmander", "Piplup", "squirtle", "bulbasaur", "Charmander", "Piplup", "squirtle", "bulbasaur", "Charmander", "Piplup", "squirtle", "bulbasaur", "Charmander", "Piplup", "squirtle", "bulbasaur", "Charmander", "Piplup", "squirtle", "bulbasaur", "Charmander", "Piplup", "squirtle", "bulbasaur", "Charmander", "Piplup", "squirtle", "bulbasaur", "Charmander", "Piplup", "squirtle", "bulbasaur", "Charmander", "Piplup", "squirtle" };
        //private string[] _ownedPokemonNames = { "bulbasaur", "Charmander", "Piplup", "squirtle"};
        //private string[] _ownedPokemonNames = { };


        private void Awake()
        {
            //create the singleton and make sure it is unique
            if (ReferenceEquals(Instance, null))
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            FetchAllPokemonInfo();
           
        }

        /// <summary>
        /// Will try fetching all owned pokemon info and handling errors.If there are some errors it will retry for the NO_OF_RETRIES size.
        /// It notify the progressContainer to display the correct errors or success states
        /// </summary>
        private async void FetchAllPokemonInfo()
        {
            //there are no pokemon owned so we can return
            if (ReferenceEquals(_ownedPokemonNames, null) || _ownedPokemonNames.Length == 0)
            {
                progressContainer.SetState(ProgressContainer.ProgressStates.STATE_NOTHING_TO_FETCH);
                return;
            }

            List<string> urls = new List<string>();
            //creating the urls and converting all names to lowercase as the API is case sensitive
            foreach (var name in _ownedPokemonNames)
            {
                urls.Add(string.Format(Endpoints.POKEMON, name.ToLower()));
            }
            
            var stopwatch = Stopwatch.StartNew();

            List<PokemonDto> pokemonList = new List<PokemonDto>();
            progressContainer.SetState(ProgressContainer.ProgressStates.STATE_FETCHING);

            int noOfRetries = NO_OF_RETRIES;

            bool errors = await PopulateListWithResultsAsync(urls, pokemonList);
            noOfRetries--;
            
            //if there were errors on sending the request we will retry
            while (errors && noOfRetries > 0)
            {
                urls.Clear();
                urls.AddRange(_webRequestClient.GetFailedUrls());
                progressContainer.SetState(ProgressContainer.ProgressStates.STATE_RETRY_FAILED_FETCH, urls.Count);
                errors = await PopulateListWithResultsAsync(urls, pokemonList);
                noOfRetries--;
            }
            
            OnPokemonInfoFetched?.Invoke(pokemonList);

            if (errors)
            {
                int failedUrls = _webRequestClient.GetFailedUrls().Count;
                if (failedUrls == 0)
                {
                    progressContainer.SetState(ProgressContainer.ProgressStates.STATE_FINISH_FETCH_WITH_UNKOWN_ERROR);
                }
                else
                {
                    progressContainer.SetState(ProgressContainer.ProgressStates.STATE_FINISH_FETCH_WITH_FAILED, urls.Count);
                }
            }
            else
            {
                progressContainer.SetState(ProgressContainer.ProgressStates.STATE_FETCHED_SUCCESFULLY);
            }

            stopwatch.Stop();
            UnityEngine.Debug.Log("calls took when all: " + stopwatch.ElapsedMilliseconds);

        }

        /// <summary>
        /// It will start fetching the info from all the ulrs request list and populate and stort the final list with the non null results.
        /// The final list must be initialized and not null othwerise this will return with error.
        /// The urls request list must be initialized and have at least one itme otherwise it will return with error
        /// </summary>
        /// <param name="urls">List of ulrs requests</param>
        /// <param name="finalList">The list that will be populated with the results</param>
        /// <returns></returns>
        private async Task<bool> PopulateListWithResultsAsync(List<string> urls, List<PokemonDto> finalList)
        {
            if (ReferenceEquals(finalList, null))
            {
                UnityEngine.Debug.LogError("The result list must be initialized");
                return true;
            }

            if (ReferenceEquals(urls, null) || urls.Count == 0)
            {
                UnityEngine.Debug.LogError("The url request list is empty");
                return true;
            }

            var responses = await _webRequestClient.GetAllAsync<PokemonDto>(urls, UpdateProgressBar);
            bool errors = _webRequestClient.HadFailedRequests();
            //if there were requests that failed we need to remove the null responses from the list
            if (errors)
            {
                foreach (var response in responses)
                {
                    if (!ReferenceEquals(response, null))
                        finalList.Add(response);
                }
            }
            else
            {
                finalList.AddRange(responses);
            }
            finalList.Sort();

            return errors;
        }
    }
}
