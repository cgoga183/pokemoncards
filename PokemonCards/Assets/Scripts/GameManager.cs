using PockemonCards.Network;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;
using PockemonCards.View;

namespace PockemonCards
{
    public class GameManager : MonoBehaviour
    {
        //if some requests will fail this is the max number of retries we are doing
        public const int NO_OF_RETRIES = 3;
        
        public static GameManager Instance;

        //event to be triggered when fetching all the pokemon info is completed
        public UnityEvent<List<PokemonDto>> OnPokemonInfoFetched;
        //event to be triggered everytime a request will fnish succesfully
        public UnityEvent OnRequestSuccesful;

        //used for displaying errors or successes
        public ProgressContainer progressContainer;
        
        private WebRequestClient _webRequestClient = new WebRequestClient();
        //private string[] _ownedPokemonNames = { "asgdasgsagasdgasdgasgasdgsadgsa23516161234632743273272347237__", "Charmander", "Piplup", "squirtle", "bulbasaur", "Charmander", "Piplup", "squirtle", "bulbasaur", "Charmander", "Piplup", "squirtle", "bulbasaur", "Charmander", "Piplup", "squirtle", "bulbasaur", "Charmander", "Piplup", "squirtle", "bulbasaur", "Charmander", "Piplup", "squirtle", "bulbasaur", "Charmander", "Piplup", "squirtle", "bulbasaur", "Charmander", "Piplup", "squirtle", "bulbasaur", "Charmander", "Piplup", "squirtle" , "bulbasaur", "Charmander", "Piplup", "squirtle" , "bulbasaur", "Charmander", "Piplup", "squirtle" , "bulbasaur", "Charmander", "Piplup", "squirtle" , "bulbasaur", "Charmander", "Piplup", "squirtle" , "bulbasaur", "Charmander", "Piplup", "squirtle" , "bulbasaur", "Charmander", "Piplup", "squirtle" , "bulbasaur", "Charmander", "Piplup", "squirtle" , "bulbasaur", "Charmander", "Piplup", "squirtle" , "bulbasaur", "Charmander", "Piplup", "squirtle" , "bulbasaur", "Charmander", "Piplup", "squirtle" , "bulbasaur", "Charmander", "Piplup", "squirtle" , "bulbasaur", "Charmander", "Piplup", "squirtle" , "bulbasaur", "Charmander", "Piplup", "squirtle", "bulbasaur", "Charmander", "Piplup", "squirtle", "bulbasaur", "Charmander", "Piplup", "squirtle", "bulbasaur", "Charmander", "Piplup", "squirtle", "bulbasaur", "Charmander", "Piplup", "squirtle", "bulbasaur", "Charmander", "Piplup", "squirtle", "bulbasaur", "Charmander", "Piplup", "squirtle", "bulbasaur", "Charmander", "Piplup", "squirtle", "bulbasaur", "Charmander", "Piplup", "squirtle", "bulbasaur", "Charmander", "Piplup", "squirtle", "bulbasaur", "Charmander", "Piplup", "squirtle", "bulbasaur", "Charmander", "Piplup", "squirtle" };
        //private string[] _ownedPokemonNames = { "bulbasaur", "Charmander", "Piplup", "squirtle"};
        private string[] _ownedPokemonNames = { "raticate", "rattata", "pidgeot", "pidgeotto", "pidgey", "beedrill", "kakuna", "weedle", "butterfree", "metapod", "caterpie", "blastoise", "wartortle", "squirtle", "charizard", "charmeleon", "charmander", "venusaur", "wigglytuff", "jigglypuff", "ninetales", "vulpix", "clefable", "clefairy", "nidoking", "nidorino", "nidoran-m", "nidoqueen", "nidorina", "nidoran-f", "sandslash", "sandshrew", "raichu", "pikachu", "arbok", "ekans", "fearow", "spearow"};
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
        /// Will try fetching all owned pokemon info and handling errors.
        /// If there are some errors it will retry for the NO_OF_RETRIES size.
        /// It notifies the progressContainer to display the correct errors or success states.
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
            List<PokemonDto> pokemonList = new List<PokemonDto>();
            //creating the request url list
            foreach (var name in _ownedPokemonNames)
            {
                //I'm assuming the API expects the names in lower case although is not clearly specified in their docs
                string nameEndpoint = string.Format(Endpoints.POKEMON, name.ToLower());
                //making sure we are not adding duplicate so all requests will be unique
                if (!urls.Contains(nameEndpoint))
                {
                    urls.Add(nameEndpoint);
                }
            }
            
            //reseting the progress bar
            progressContainer.ResetProgressBar(urls.Count);

            //using this for debug purposes to test how long the requests will take
            var stopwatch = Stopwatch.StartNew();
            
            progressContainer.SetState(ProgressContainer.ProgressStates.STATE_FETCHING);

            int noOfRetries = NO_OF_RETRIES;
            var responses = await _webRequestClient.GetAnyAsync<PokemonDto>(urls, OnRequestSuccesful);

            bool errors = _webRequestClient.HadFailedRequests();
            pokemonList.AddRange(responses);
            pokemonList.Sort();
            noOfRetries--;
            
            //if there were errors on sending the request we will retry
            while (errors && noOfRetries > 0)
            {
                //will reset the url list and populate it only with the failed requests
                urls.Clear();
                urls.AddRange(_webRequestClient.GetFailedUrls());
                progressContainer.SetState(ProgressContainer.ProgressStates.STATE_RETRY_FAILED_FETCH, urls.Count);

                responses = await _webRequestClient.GetAnyAsync<PokemonDto>(urls, OnRequestSuccesful);
                errors = _webRequestClient.HadFailedRequests();
                pokemonList.AddRange(responses);
                pokemonList.Sort();
                noOfRetries--;
            }
            
            //trigger the event for notifying anyone listening that we have fetched all data
            OnPokemonInfoFetched?.Invoke(pokemonList);

            //error handling
            if (errors)
            {
                int failedUrlsCount = _webRequestClient.GetFailedUrls().Count;
                progressContainer.SetState(ProgressContainer.ProgressStates.STATE_FINISH_FETCH_WITH_FAILED, failedUrlsCount);
            }
            else
            {
                progressContainer.SetState(ProgressContainer.ProgressStates.STATE_FETCHED_SUCCESFULLY);
            }

            stopwatch.Stop();
            UnityEngine.Debug.Log("Calls took: " + stopwatch.ElapsedMilliseconds);
        }
    }
}
