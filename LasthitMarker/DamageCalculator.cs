using System.Linq;
using LeagueSharp;

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

namespace LasthitMarker
{
    internal class DamageCalculator
    {
        public static double Calculate(Obj_AI_Hero player, Obj_AI_Base target)
        {
            //no % armor/magic penetration calculation
            //can't find magic resist??
            //no masteries yet
            double targetArmor = target.Armor + target.FlatArmorMod;
            double targetSpellbock = target.SpellBlock + target.FlatSpellBlockMod;
            double attackDamage = player.BaseAttackDamage + player.FlatPhysicalDamageMod;
            double magicDamage = player.BaseAbilityDamage + player.FlatMagicDamageMod;

            targetArmor -= player.FlatArmorPenetrationMod;
            targetSpellbock -= player.FlatMagicPenetrationMod;

            double totalDamage = attackDamage*(100/(100 + targetArmor));

            if (HasMasteryArcaneBlade())
            {
                totalDamage += (magicDamage*0.05)*(100/(100 + targetSpellbock));
            }
            if (HasMasteryDoubleEdgedSword())
            {
                if (player.AttackRange > 250)
                {
                    totalDamage *= 1.02;
                }
                else
                {
                    totalDamage *= 1.015;
                }
            }
            if (HasMasteryHavoc())
            {
                totalDamage *= 1.03;
            }
            if (target.IsMinion & HasMasteryButcher())
            {
                totalDamage += 2;
            }
            return totalDamage;
        }

        public static bool HasMasteryDoubleEdgedSword()
        {
            return
                ObjectManager.Player.Masteries.Where(mastery => mastery.Page == MasteryPage.Offense)
                    .Any(mastery => mastery.Id == 65 && mastery.Points == 1);
        }

        public static bool HasMasteryButcher()
        {
            return
                ObjectManager.Player.Masteries.Where(mastery => mastery.Page == MasteryPage.Offense)
                    .Any(mastery => mastery.Id == 68 && mastery.Points == 1);
        }

        public static bool HasMasteryArcaneBlade()
        {
            return
                ObjectManager.Player.Masteries.Where(mastery => mastery.Page == MasteryPage.Offense)
                    .Any(mastery => mastery.Id == 132 && mastery.Points == 1);
        }

        public static bool HasMasteryHavoc()
        {
            return
                ObjectManager.Player.Masteries.Where(mastery => mastery.Page == MasteryPage.Offense)
                    .Any(mastery => mastery.Id == 146 && mastery.Points == 1);
        }
    }
}