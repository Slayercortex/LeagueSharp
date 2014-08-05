using System;
using System.Linq;
using System.Reflection;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

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

namespace TowerRange
{
    internal class TowerRange
    {
        private const float TurretRange = 910;
        private const double MaxDistance = 1450;

        private Menu _menu;

        public TowerRange()
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        private void OnGameLoad(EventArgs args)
        {
            try
            {
                _menu = new Menu(Assembly.GetExecutingAssembly().GetName().Name,
                    Assembly.GetExecutingAssembly().GetName().Name, true);
                _menu.AddItem(new MenuItem("DrawAlly", "Draw Ally").SetValue(true));
                _menu.AddItem(new MenuItem("DrawEnemy", "Draw Enemy").SetValue(true));
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
                foreach (Obj_Turret tower in ObjectManager.Get<Obj_Turret>()
                    .Where(tower => tower.IsValid && !tower.IsDead)
                    .Where(
                        tower => Vector3.Distance(ObjectManager.Player.Position, tower.Position) <= MaxDistance)
                    .Where(tower => _menu.Item("DrawAlly").GetValue<bool>() || !tower.IsAlly)
                    .Where(tower => _menu.Item("DrawEnemy").GetValue<bool>() || !tower.IsEnemy))
                {
                    if (_menu.Item("CircleLag").GetValue<bool>())
                    {
                        Utility.DrawCircle(tower.Position, TurretRange, tower.IsAlly ? Color.Green : Color.Red,
                            _menu.Item("CircleThickness").GetValue<Slider>().Value,
                            _menu.Item("CircleQuality").GetValue<Slider>().Value);
                    }
                    else
                    {
                        Drawing.DrawCircle(tower.Position, TurretRange, tower.IsAlly ? Color.Green : Color.Red);
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