using PockemonCards.Network;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace PockemonCards
{
    public class GameManager : MonoBehaviour
    {
        private WebRequestClient _webRequestClient = new WebRequestClient();
        private string[] ownedPokemonNames = { "Bulbasaur", "Charmander", "Piplup", "Squirtle"};
        
        // Start is called before the first frame update
        void Start()
        {
            FetchAllPokemonInfo();
           
        }

        private async void FetchAllPokemonInfo()
        {
            var stopwatch = Stopwatch.StartNew();
            List<string> urls = new List<string>();
            foreach (var name in ownedPokemonNames)
            {
                urls.Add(string.Format(Endpoints.POKEMON, name));
            }

            IEnumerable<PokemonDto> response = await _webRequestClient.GetAllAsync<PokemonDto>(urls);
            foreach (var pokemon in response)
            {
                UnityEngine.Debug.Log("base_experience: " + pokemon.base_experience + " name: " + pokemon.name);
            }
            stopwatch.Stop();
            UnityEngine.Debug.Log("calls took: " + stopwatch.ElapsedMilliseconds);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
