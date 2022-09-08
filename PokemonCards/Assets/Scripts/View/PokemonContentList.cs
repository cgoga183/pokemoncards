using PockemonCards.Network;
using System.Collections.Generic;
using UnityEngine;

namespace PockemonCards.View
{
    public class PokemonContentList : MonoBehaviour
    {
        public PokemonTemplate pokemonTemplatePrefab;

        /// <summary>
        /// Will populate the list and instantiate each pokemon using the pokemonTemplatePrefab
        /// </summary>
        /// <param name="pokemonInfoList"></param>
        public void OnPokemonInfoFetched(List<PokemonDto> pokemonInfoList)
        {
            foreach(var pokemonInfo in pokemonInfoList)
            {
                if (ReferenceEquals(pokemonInfo, null))
                    continue;
                
                PokemonTemplate pokemonObject = Instantiate(pokemonTemplatePrefab);
                pokemonObject.SetName(pokemonInfo.name);
                pokemonObject.SetBaseExperience(pokemonInfo.base_experience);
                pokemonObject.gameObject.SetActive(true);

                pokemonObject.transform.SetParent(gameObject.transform, false);
            }
        }
    }
}
