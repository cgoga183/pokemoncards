using System;

namespace PockemonCards.Network
{
    public class PokemonDto : IComparable<PokemonDto>
    {
        public int base_experience;
        public string name;
        
        public int CompareTo(PokemonDto other)
        {
            return other.base_experience.CompareTo(this.base_experience);
        }
    }


}
