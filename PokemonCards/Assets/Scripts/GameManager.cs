using PockemonCards.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System.Linq;

namespace PockemonCards
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;
        //action to be invoked when fetching all the pokemon info is completed
        public Action<List<PokemonDto>> OnPokemonInfoFetched;

        private WebRequestClient _webRequestClient = new WebRequestClient();
        private string[] _ownedPokemonNames = { "Bulbasaur", "Charmander", "Piplup", "Squirtle"};
        

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
        /// Will fetch all the info for all the pokemons from the _ownedPokemonNames list
        /// </summary>
        private async void FetchAllPokemonInfo()
        {
            //there are no pokemon owned so we can return
            if (_ownedPokemonNames.Length == 0)
                return;

            var stopwatch = Stopwatch.StartNew();
            List<string> urls = new List<string>();
            //creating the urls and converting all names to lowercase as the API is case sensitive
            foreach (var name in _ownedPokemonNames)
            {
                urls.Add(string.Format(Endpoints.POKEMON, name.ToLower()));
            }

            IEnumerable<PokemonDto> response = await _webRequestClient.GetAllAsync<PokemonDto>(urls);
            foreach (var pokemon in response)
            {
                if (!ReferenceEquals(pokemon, null))
                    UnityEngine.Debug.Log("base_experience: " + pokemon.base_experience + " name: " + pokemon.name);
            }

            OnPokemonInfoFetched?.Invoke(response.ToList());

            stopwatch.Stop();
            UnityEngine.Debug.Log("calls took: " + stopwatch.ElapsedMilliseconds);
        }
    }
}
