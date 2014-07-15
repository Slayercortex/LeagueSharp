using System.Collections.Generic;
using LeagueSharp;

namespace AutoPotion
{
    internal class Potion
    {
        public string Name { get; set; }
        public int MinCharges { get; set; }
        public ItemId ItemId { get; set; }
        public int Priority { get; set; }
        public List<PotionType> TypeList { get; set; }
    }
}