using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using SharpDX;
using Color = System.Drawing.Color;

namespace SnakeSharp
{
    internal class Food
    {
        private readonly Map _map;
        private readonly Snake _snake;

        public Food(Map map, Snake snake)
        {
            _map = map;
            _snake = snake;
            _snake.Move += SnakeOnMove;
            Generate();
        }

        public Tile Position { get; private set; }

        private void SnakeOnMove(object sender, EventArgs eventArgs)
        {
            Tile head = _snake.Body.FirstOrDefault();
            if (head != null && head.X == Position.X && head.Y == Position.Y)
            {
                _snake.Add();
                OnEat(new EventArgs());
                Generate();
            }
        }

        public void Reset()
        {
            Generate();
        }

        public void Generate()
        {
            Position = GetFreeTiles().OrderBy(x => Guid.NewGuid()).FirstOrDefault();
        }

        public void Draw()
        {
            Vector2 position = _map.Tile2Positon(Position);
            Drawing.DrawLine(position.X, position.Y, position.X + _map.TileSize, position.Y, _map.TileSize, Color.Yellow);
        }

        private IEnumerable<Tile> GetFreeTiles()
        {
            var freeTiles = new List<Tile>();
            for (int x = 0; x < _map.X; x++)
            {
                for (int y = 0; y < _map.Y; y++)
                {
                    if (!_snake.Body.Any(s => s.X == x && s.Y == y))
                    {
                        freeTiles.Add(new Tile(x, y));
                    }
                }
            }
            return freeTiles;
        }

        public event EventHandler<EventArgs> Eat;

        protected virtual void OnEat(EventArgs e)
        {
            EventHandler<EventArgs> handler = Eat;
            if (handler != null) handler(this, e);
        }
    }
}