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
    internal class HeroSpell
    {
        public bool Available = false;
        public string Name = string.Empty;
        public float Range = 0;
        public SpellSlot Slot = SpellSlot.Unknown;
        public float TrueRange = 0;

        public HeroSpell(string name, SpellSlot slot, float range)
        {
            Name = name;
            Slot = slot;
            Range = range;
            TrueRange = range + ObjectManager.Player.AttackRange;
            Available = ObjectManager.Player.BaseSkinName == Name;
        }

        public bool CanUseSpell()
        {
            return Available && !ObjectManager.Player.IsDead && !ObjectManager.Player.IsStunned &&
                   Slot != SpellSlot.Unknown && ObjectManager.Player.Spellbook.CanUseSpell(Slot) == SpellState.Ready;
        }

        public bool CanUseSpell(Obj_AI_Minion target)
        {
            return Available && CanUseSpell() && target != null && target.IsValid && !target.IsInvulnerable &&
                   !target.IsAlly && IsInRange(target);
        }

        public bool IsInRange(Obj_AI_Minion target)
        {
            return target != null && target.IsValid &&
                   Vector3.Distance(ObjectManager.Player.Position, target.Position) <= TrueRange;
        }

        public bool CastSpell(Obj_AI_Minion target)
        {
            if (!CanUseSpell(target))
                return false;
            ObjectManager.Player.Spellbook.CastSpell(Slot, target);
            return true;
        }

        public double CalculateDamage()
        {
            switch (Name)
            {
                case "Nunu":
                    return 250 + 150*ObjectManager.Player.Spellbook.GetSpell(Slot).Level;
                case "Chogath":
                    return 1000 + 0.7*(ObjectManager.Player.BaseAbilityDamage + ObjectManager.Player.FlatMagicDamageMod);
                case "Olaf":
                    return 25 + 45*ObjectManager.Player.Spellbook.GetSpell(Slot).Level +
                           0.4*(ObjectManager.Player.BaseAttackDamage + ObjectManager.Player.FlatPhysicalDamageMod);
                default:
                    return 0;
            }
        }
    }
}