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
            pokeName.text = "Name: " + name;
        }

        public void SetBaseExperience(int baseExperience)
        {
            pokeBaseExperience.text = "Base XP: " + baseExperience;
        }
    }
}
