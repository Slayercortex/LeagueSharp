using System.Linq;
using LeagueSharp;
using SharpDX;

/*
    Copyright (C) 2014 Nikita Bernthaler

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

namespace QuickSmite
{
    internal class Smite
    {
        public readonly int Range = 750;
        public readonly string SummonerName = "SummonerSmite";
        public bool Available = false;
        public SpellSlot Slot = SpellSlot.Unknown;

        public Smite()
        {
            var spells = ObjectManager.Player.SummonerSpellbook.Spells;
            foreach (var spell in spells.Where(spell => spell.Name.ToLower() == SummonerName.ToLower()))
            {
                Available = true;
                Slot = spell.Slot;
            }
        }

        public double CalculateDamage()
        {
            int level = ObjectManager.Player.Level;
            int[] stages =
            {
                20*level + 370,
                30*level + 330,
                40*level + 240,
                50*level + 100
            };
            return stages.Max();
        }

        public bool CanUseSpell()
        {
            return Available && !ObjectManager.Player.IsDead && !ObjectManager.Player.IsStunned &&
                   Slot != SpellSlot.Unknown &&
                   ObjectManager.Player.SummonerSpellbook.CanUseSpell(Slot) == SpellState.Ready;
        }

        public bool CanUseSpell(Obj_AI_Minion target)
        {
            return Available && CanUseSpell() && target != null && target.IsValid && !target.IsInvulnerable &&
                   !target.IsAlly && IsInRange(target);
        }

        public bool IsInRange(Obj_AI_Minion target)
        {
            return target != null && target.IsValid &&
                   Vector3.Distance(ObjectManager.Player.Position, target.Position) <= Range;
        }

        public bool CastSpell(Obj_AI_Minion target)
        {
            if (!CanUseSpell(target))
                return false;
            ObjectManager.Player.SummonerSpellbook.CastSpell(Slot, target);
            return true;
        }
    }
}
