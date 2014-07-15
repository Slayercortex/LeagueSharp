using LeagueSharp;

namespace JungleTimers
{
    internal class JungleMinion
    {
        public JungleMinion(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
        public bool Dead { get; set; }
        public GameObject Unit { get; set; }
    }
}