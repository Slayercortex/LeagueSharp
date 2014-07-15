using System.Linq;
using LeagueSharp;
using SharpDX;

namespace QuickSmite
{
    internal class BigMinions
    {
        private static readonly string[] MinionNames =
        {
            "Worm", "Dragon", "LizardElder", "AncientGolem"
        };

        public static Obj_AI_Minion GetNearest(Vector3 pos)
        {
            double? shortest = null;
            Obj_AI_Minion sMinion = null;
            foreach (Obj_AI_Minion minion in ObjectManager.Get<Obj_AI_Minion>())
            {
                if (MinionNames.Any(name => minion.Name.StartsWith(name)))
                {
                    double distance = Vector3.Distance(pos, minion.Position);
                    shortest = shortest == null || shortest > distance ? distance : shortest;
                    sMinion = minion;
                }
            }
            return sMinion;
        }
    }
}