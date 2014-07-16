using System;
using System.Linq;
using System.Reflection;
using LeagueSharp;
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

        private readonly Action _onLoadAction;

        public TowerRange()
        {
            _onLoadAction = new CallOnce().A(OnLoad);
            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
        }

        private void OnLoad()
        {
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
                    Obj_Turret tower in
                        ObjectManager.Get<Obj_Turret>()
                            .Where(tower => tower.IsValid && !tower.IsDead)
                            .Where(
                                tower => Vector3.Distance(ObjectManager.Player.Position, tower.Position) <= MaxDistance)
                    )
                {
                    Drawing.DrawCircle(tower.Position, TurretRange, tower.IsAlly ? Color.Green : Color.Red);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}