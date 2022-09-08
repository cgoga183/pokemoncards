using PockemonCards.Network;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace PockemonCards.View
{
    public class PokemonContentList : MonoBehaviour
    {
        public PokemonTemplate pokemonTemplatePrefab;
        public TextMeshProUGUI loadingText;

        void Awake()
        {
            loadingText.gameObject.SetActive(true);
            loadingText.text = Texts.FETCHING_POKEMONS;
        }

        /// <summary>
        /// Will populate the list and instantiate each pokemon using the pokemonTemplatePrefab
        /// </summary>
        /// <param name="pokemonInfoList"></param>
        public void OnPokemonInfoFetched(List<PokemonDto> pokemonInfoList)
        {
            //making sure we are not receiving a null list
            if (ReferenceEquals(pokemonInfoList, null))
            {
                return;
            }
            loadingText.gameObject.SetActive(false);

            foreach(var pokemonInfo in pokemonInfoList)
            {
                //ignore any null items if any
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
