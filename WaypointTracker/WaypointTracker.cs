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

namespace WaypointTracker
{
    internal class WaypointTracker
    {
        private Menu _menu;

        public WaypointTracker()
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
                foreach (Obj_AI_Hero hero in from hero in ObjectManager.Get<Obj_AI_Hero>()
                    where hero.IsValid && !hero.IsDead && !hero.IsBot && hero.Path.Length != 0
                    where _menu.Item("DrawAlly").GetValue<Boolean>() || !hero.IsAlly
                    where _menu.Item("DrawEnemy").GetValue<Boolean>() || !hero.IsEnemy
                    select hero)
                {
                    float[] lastPathPos =
                        Drawing.WorldToScreen(hero.Path[hero.Path.Length > 1 ? hero.Path.Length - 1 : 0]);
                    float[] heroPos = Drawing.WorldToScreen(hero.Position);
                    for (int index = 0; index < hero.Path.Length; index++)
                    {
                        float[] curr = Drawing.WorldToScreen(hero.Path[index]);
                        if (index > 0)
                        {
                            float[] prev = Drawing.WorldToScreen(hero.Path[index - 1]);
                            Drawing.DrawLine(prev[0], prev[1], curr[0], curr[1], 3,
                                hero.IsAlly ? Color.Green : Color.Red);
                        }
                        else
                        {
                            Drawing.DrawLine(heroPos[0], heroPos[1], curr[0], curr[1], 3,
                                hero.IsAlly ? Color.Green : Color.Red);
                        }
                    }
                    Drawing.DrawText(lastPathPos[0], lastPathPos[1], Color.Orange, hero.BaseSkinName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}