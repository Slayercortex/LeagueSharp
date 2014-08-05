using System;
using System.Collections.Generic;
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

namespace LasthitMarker
{
    internal class LasthitMarker
    {
        private const int MaxMinionDistance = 1000;

        private readonly Action _onLoadAction;
        private List<Obj_AI_Minion> _killableMinions = new List<Obj_AI_Minion>();

        public LasthitMarker()
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
                _killableMinions = (from minion in ObjectManager.Get<Obj_AI_Minion>()
                    where minion.IsValid && minion.IsVisible && minion.IsEnemy && !minion.IsDead
                    where Vector3.Distance(ObjectManager.Player.Position, minion.Position) <= MaxMinionDistance
                    where minion.Health <= DamageCalculator.Calculate(ObjectManager.Player, minion)
                    select minion).ToList();
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
                foreach (Obj_AI_Minion minion in _killableMinions)
                {
                    Drawing.DrawCircle(minion.Position, minion.BoundingRadius + 25, Color.Gray);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}