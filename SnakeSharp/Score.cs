using System;
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

namespace SnakeSharp
{
    internal class Score
    {
        private readonly Map _map;

        public Score(Map map, Food food)
        {
            Points = 0;
            _map = map;
            food.Eat += FoodOnEat;
        }

        public int Points { get; private set; }

        private void FoodOnEat(object sender, EventArgs eventArgs)
        {
            Points++;
        }

        public void Reset()
        {
            Points = 0;
        }

        public void Draw()
        {
            Vector2 position = _map.Tile2Positon(new Tile(0, -1));
            Drawing.DrawText(position.X, position.Y, Color.GreenYellow, string.Format("Score: {0}", Points));
        }
    }
}