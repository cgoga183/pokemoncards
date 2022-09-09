using TMPro;
using UnityEngine;

namespace PockemonCards.View
{
    public class PokemonTemplate : MonoBehaviour
    {
        public TextMeshProUGUI pokeName;
        public TextMeshProUGUI pokeBaseExperience;

        public void SetName(string name)
        {
            pokeName.text = string.Format(Texts.POKEMON_NAME, name);
        }

        public void SetBaseExperience(int baseExperience)
        {
            pokeBaseExperience.text = string.Format(Texts.POKEMON_BASE_XP, baseExperience);
        }
    }
}
