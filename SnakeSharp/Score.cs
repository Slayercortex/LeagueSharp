using System;
using LeagueSharp;
using SharpDX;
using Color = System.Drawing.Color;

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