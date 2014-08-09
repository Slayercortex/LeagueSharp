using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using LeagueSharp;
using LeagueSharp.Common;

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
    internal class QuickSmite
    {
        private Obj_AI_Minion _currentMinion;

        private HeroSpell _heroSpell;
        private List<HeroSpell> _heroSpells;
        private Menu _menu;

        private Smite _smite;

        public QuickSmite()
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        private void OnGameLoad(EventArgs args)
        {
            try
            {
                _menu = new Menu(Assembly.GetExecutingAssembly().GetName().Name,
                    Assembly.GetExecutingAssembly().GetName().Name, true);
                _menu.AddItem(
                    new MenuItem("Enabled", "Enabled").SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle,
                        true)));

                var spellsMenu = new Menu("Spells", "Spells");
                spellsMenu.AddItem(new MenuItem("Smite", "Use Smite").SetValue(true));
                spellsMenu.AddItem(new MenuItem("Nunu", "Use Nunu Q").SetValue(true));
                spellsMenu.AddItem(new MenuItem("Chogath", "Use Cho'Gath R").SetValue(true));
                spellsMenu.AddItem(new MenuItem("Olaf", "Use Olaf E").SetValue(true));

                var miscMenu = new Menu("Misc", "Misc");
                miscMenu.AddItem(new MenuItem("CircleLag", "Lag Free Circles").SetValue(true));
                miscMenu.AddItem(new MenuItem("CircleQuality", "Circles Quality").SetValue(new Slider(30, 100, 10)));
                miscMenu.AddItem(new MenuItem("CircleThickness", "Circles Thickness").SetValue(new Slider(2, 10, 1)));

                _menu.AddSubMenu(spellsMenu);
                _menu.AddSubMenu(miscMenu);
                _menu.AddToMainMenu();

                Game.PrintChat(
                    string.Format(
                        "<font color='#F7A100'>{0} v{1} loaded.</font>",
                        Assembly.GetExecutingAssembly().GetName().Name,
                        Assembly.GetExecutingAssembly().GetName().Version
                        )
                    );

                _smite = new Smite();
                _heroSpells = new List<HeroSpell>
                {
                    new HeroSpell("Nunu", SpellSlot.Q, 125),
                    new HeroSpell("Chogath", SpellSlot.R, 175),
                    new HeroSpell("Olaf", SpellSlot.E, 325)
                };
                _heroSpell = _heroSpells.FirstOrDefault(s => s.Available);

                Game.OnGameUpdate += OnGameUpdate;
                Drawing.OnDraw += OnDraw;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void OnGameUpdate(EventArgs args)
        {
            try
            {
                if (!_menu.Item("Enabled").GetValue<KeyBind>().Active)
                    return;
                if (!SmiteEnabled() && !HeroSpellEnabled())
                    return;
                _currentMinion = BigMinions.GetNearest(ObjectManager.Player.Position);
                if (_currentMinion != null && _currentMinion.Health <= CalculateDamage(_currentMinion))
                {
                    KillMinion(_currentMinion);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void KillMinion(Obj_AI_Minion minion)
        {
            if (HeroSpellEnabled() && _heroSpell.CanUseSpell(minion))
                _heroSpell.CastSpell(minion);
            if (SmiteEnabled() && _smite.CanUseSpell(minion))
                _smite.CastSpell(minion);
        }

        private double CalculateDamage(Obj_AI_Minion target)
        {
            double damage = 0;
            if (SmiteEnabled() && _smite.CanUseSpell(target))
                damage += _smite.CalculateDamage();
            if (HeroSpellEnabled() && _heroSpell.CanUseSpell(target))
                damage += _heroSpell.CalculateDamage();
            return damage;
        }

        private bool HeroSpellEnabled()
        {
            return _heroSpell != null && _heroSpell.Available &&
                   (_heroSpell.Name == "Nunu" && _menu.Item("Nunu").GetValue<bool>() ||
                    _heroSpell.Name == "Olaf" && _menu.Item("Olaf").GetValue<bool>() ||
                    _heroSpell.Name == "Chogath" && _menu.Item("Chogath").GetValue<bool>());
        }

        private bool SmiteEnabled()
        {
            return _smite.Available && _menu.Item("Smite").GetValue<bool>();
        }

        private void OnDraw(EventArgs args)
        {
            try
            {
                if (!_menu.Item("Enabled").GetValue<KeyBind>().Active)
                    return;
                if (!SmiteEnabled() && !HeroSpellEnabled())
                    return;

                if (_menu.Item("CircleLag").GetValue<bool>())
                {
                    if (SmiteEnabled())
                    {
                        Utility.DrawCircle(ObjectManager.Player.Position, _smite.Range,
                            _smite.CanUseSpell(_currentMinion) ? Color.Blue : Color.Gray,
                            _menu.Item("CircleThickness").GetValue<Slider>().Value,
                            _menu.Item("CircleQuality").GetValue<Slider>().Value);
                    }
                    if (HeroSpellEnabled())
                    {
                        Utility.DrawCircle(ObjectManager.Player.Position,
                            _heroSpell.TrueRange,
                            _heroSpell.CanUseSpell(_currentMinion) ? Color.Blue : Color.Gray,
                            _menu.Item("CircleThickness").GetValue<Slider>().Value,
                            _menu.Item("CircleQuality").GetValue<Slider>().Value);
                    }
                }
                else
                {
                    if (SmiteEnabled())
                    {
                        Drawing.DrawCircle(ObjectManager.Player.Position, _smite.Range,
                            _smite.CanUseSpell() && _smite.IsInRange(_currentMinion) ? Color.Blue : Color.Gray);
                    }
                    if (HeroSpellEnabled())
                    {
                        Drawing.DrawCircle(ObjectManager.Player.Position,
                            _heroSpell.TrueRange,
                            _heroSpell.CanUseSpell() && _heroSpell.IsInRange(_currentMinion) ? Color.Blue : Color.Gray);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}