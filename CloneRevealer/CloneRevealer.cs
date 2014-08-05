using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
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

namespace CloneRevealer
{
    internal class CloneRevealer
    {
        private readonly string[] _champions =
        {
            "Shaco", "LeBlanc", "MonkeyKing", "Yorick"
        };

        private readonly Action _onLoadAction;

        public CloneRevealer()
        {
            _onLoadAction = new CallOnce().A(OnLoad);
            Game.OnGameUpdate += OnGameUpdate;
        }

        private void OnLoad()
        {
            Drawing.OnDraw += OnDraw;
            Game.PrintChat(
                string.Format(
                    "<font color='#F7A100'>{0} v{1} loaded.</font>",
                    Assembly.GetExecutingAssembly().GetName().Name,
                    Assembly.GetExecutingAssembly().GetName().Version
                    )
                );
        }

        private void OnGameUpdate(EventArgs args)
        {
            try
            {
                _onLoadAction();
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
                    Drawing.DrawCircle(hero.Position, hero.BoundingRadius + 25, Color.Yellow);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}