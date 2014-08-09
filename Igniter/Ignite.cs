using System;
using System.Collections.Generic;
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

namespace Igniter
{
    public class Ignite
    {
        private const string Name = "SummonerDot";
        private const int Range = 600;
        private const int HealReducion = 50;
        private const int Duration = 5;
        private const int Ticks = 5;

        private readonly Action _onLoadAction;

        private readonly List<Potion> _potions = new List<Potion>
        {
            new Potion
            {
                Name = "ItemCrystalFlask",
                HP5 = 10.0
            },
            new Potion
            {
                Name = "RegenerationPotion",
                HP5 = 10.0
            },
            new Potion
            {
                Name = "ItemMiniRegenPotion",
                HP5 = 10.0
            }
        };

        private SpellDataInst _ignite;

        public Ignite()
        {
            _onLoadAction = new CallOnce().A(OnLoad);
            Game.OnGameUpdate += OnGameUpdate;
        }

        public event EventHandler<IgniteEventArgs> CanKillEnemies;

        public event EventHandler<IgniteEventArgs> CanKillstealEnemies;

        private void OnLoad()
        {
            _ignite = GetSpell();
        }

        protected virtual void OnCanKillEnemies(IgniteEventArgs e)
        {
            EventHandler<IgniteEventArgs> handler = CanKillEnemies;
            if (handler != null) handler(this, e);
        }

        protected virtual void OnCanKillstealEnemies(IgniteEventArgs e)
        {
            EventHandler<IgniteEventArgs> handler = CanKillstealEnemies;
            if (handler != null) handler(this, e);
        }

        private void OnGameUpdate(EventArgs args)
        {
            try
            {
                _onLoadAction();
                List<Obj_AI_Hero> killable =
                    EnemiesInRange().Where(enemy => CalculateHeroHealth(enemy) <= CalculateDamage()).ToList();
                if (killable.Count > 0)
                {
                    OnCanKillEnemies(new IgniteEventArgs {Enemies = killable});
                    List<Obj_AI_Hero> killsteal =
                        EnemiesInRange().Where(enemy => enemy.Health <= (CalculateDamage()/Ticks)).ToList();
                    if (killsteal.Count > 0)
                    {
                        OnCanKillstealEnemies(new IgniteEventArgs {Enemies = killsteal});
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public bool Cast(Obj_AI_Hero enemy)
        {
            if (!enemy.IsValid || !enemy.IsVisible || !enemy.IsTargetable || enemy.IsDead)
            {
                return false;
            }
            SpellDataInst ignite = GetSpell();
            if (ignite != null && ignite.Slot != SpellSlot.Unknown && ignite.State == SpellState.Ready &&
                ObjectManager.Player.CanCast)
            {
                ObjectManager.Player.SummonerSpellbook.CastSpell(ignite.Slot, enemy);
                return true;
            }
            return false;
        }

        public bool CanKill(Obj_AI_Hero enemy)
        {
            return CalculateHeroHealth(enemy) <= CalculateDamage();
        }

        public bool CanKillsteal(Obj_AI_Hero enemy)
        {
            return enemy.Health <= (CalculateDamage()/Ticks);
        }

        public SpellDataInst GetSpell()
        {
            if (_ignite != null)
            {
                return _ignite;
            }
            SpellDataInst[] spells = ObjectManager.Player.SummonerSpellbook.Spells;
            return spells.FirstOrDefault(spell => spell.Name == Name);
        }

        public double CalculateDamage()
        {
            return ObjectManager.Player.Level*20 + 50;
        }

        public double CalculateHeroHealth(Obj_AI_Hero hero)
        {
            double healthReg = 0;
            double HP5 = hero.HPRegenRate;
            Potion cPot = GetActivePotion(hero);
            BuffInstance buff = GetActivePotionBuff(hero);
            if (cPot != null && buff != null)
            {
                HP5 -= cPot.HP5;
            }
            healthReg += (HP5/5)*Duration;
            if (cPot != null && buff != null)
            {
                int remaining = Convert.ToInt32(buff.EndTime - Game.Time);
                healthReg += remaining > Duration ? (cPot.HP5/5)*Duration : (cPot.HP5/5)*remaining;
            }
            return hero.Health + ((healthReg*HealReducion)/100);
        }

        private IEnumerable<Obj_AI_Hero> EnemiesInRange()
        {
            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(
                        hero =>
                            hero.IsValid && hero.IsTargetable && hero.IsVisible && hero.IsEnemy &&
                            Vector3.Distance(ObjectManager.Player.Position, hero.Position) <= Range)
                    .ToList();
        }

        private Potion GetActivePotion(Obj_AI_Hero hero)
        {
            return
                (from potion in _potions
                    from buff in hero.Buffs
                    where buff.Name == potion.Name && buff.IsActive
                    select potion).FirstOrDefault();
        }

        private BuffInstance GetActivePotionBuff(Obj_AI_Hero hero)
        {
            return
                (from potion in _potions
                    from buff in hero.Buffs
                    where buff.Name == potion.Name && buff.IsActive
                    select buff).FirstOrDefault();
        }
    }
}