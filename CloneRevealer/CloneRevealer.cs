using System;
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

namespace CloneRevealer
{
    internal class CloneRevealer
    {
        private readonly string[] _champions =
        {
            "Shaco", "LeBlanc", "MonkeyKing", "Yorick"
        };

        private Menu _menu;

        public CloneRevealer()
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        private void OnGameLoad(EventArgs args)
        {
            try
            {
                _menu = new Menu(Assembly.GetExecutingAssembly().GetName().Name,
                    Assembly.GetExecutingAssembly().GetName().Name, true);
                _menu.AddSubMenu(new Menu("Misc", "Misc"));
                _menu.SubMenu("Misc").AddItem(new MenuItem("CircleLag", "Lag Free Circles").SetValue(true));
                _menu.SubMenu("Misc")
                    .AddItem(new MenuItem("CircleQuality", "Circles Quality").SetValue(new Slider(30, 100, 10)));
                _menu.SubMenu("Misc")
                    .AddItem(new MenuItem("CircleThickness", "Circles Thickness").SetValue(new Slider(2, 10, 1)));
                _menu.AddToMainMenu();

                Game.PrintChat(
                    string.Format(
                        "<font color='#F7A100'>{0} v{1} loaded.</font>",
                        Assembly.GetExecutingAssembly().GetName().Name,
                        Assembly.GetExecutingAssembly().GetName().Version
                        )
                    );

                Drawing.OnDraw += OnDraw;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void OnDraw(EventArgs args)
        {
            try
            {
                foreach (
                    Obj_AI_Hero hero in
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(hero => hero.IsValid && hero.IsEnemy && !hero.IsDead)
                            .Where(hero => _champions.Contains(hero.Name)))
                {
                    if (_menu.Item("CircleLag").GetValue<bool>())
                    {
                        Utility.DrawCircle(hero.Position, hero.BoundingRadius + 30, Color.Yellow,
                            _menu.Item("CircleThickness").GetValue<Slider>().Value,
                            _menu.Item("CircleQuality").GetValue<Slider>().Value);
                    }
                    else
                    {
                        Drawing.DrawCircle(hero.Position, hero.BoundingRadius + 30, Color.Yellow);
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