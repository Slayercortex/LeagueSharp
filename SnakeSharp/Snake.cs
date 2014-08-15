using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using SharpDX;
using Color = System.Drawing.Color;

namespace SnakeSharp
{
    internal class Snake
    {
        private readonly Map _map;
        private Direction _lastDirection = Direction.None;
        private Tile _lastTail;

        public Snake(Map map)
        {
            _map = map;
            Body = new List<Tile> {new Tile(1, Convert.ToInt32(_map.Y/2))};
        }

        public List<Tile> Body { get; private set; }

        public void Draw()
        {
            foreach (Tile tile in Body)
            {
                Vector2 position = _map.Tile2Positon(tile);
                Drawing.DrawLine(position.X, position.Y, position.X + _map.TileSize, position.Y, _map.TileSize,
                    Color.White);
            }
        }

        public bool Win()
        {
            return Body.Count == _map.X*_map.Y;
        }

        public bool DoMove(Direction direction)
        {
            Tile tail = Body.LastOrDefault();
            Tile head = Body.FirstOrDefault();
            if (tail != null && head != null)
            {
                switch (direction)
                {
                    case Direction.Up:
                        if (_lastDirection == Direction.Down)
                            direction = Direction.Down;
                        break;
                    case Direction.Right:
                        if (_lastDirection == Direction.Left)
                            direction = Direction.Left;
                        break;
                    case Direction.Down:
                        if (_lastDirection == Direction.Up)
                            direction = Direction.Up;
                        break;
                    case Direction.Left:
                        if (_lastDirection == Direction.Right)
                            direction = Direction.Right;
                        break;
                }
                int x = direction == Direction.Right ? 1 : (direction == Direction.Left ? -1 : 0);
                int y = direction == Direction.Down ? 1 : (direction == Direction.Up ? -1 : 0);
                var newPos = new Tile(head.X + x, head.Y + y);
                if (newPos.X < _map.X && newPos.X >= 0 && newPos.Y < _map.Y && newPos.Y >= 0 &&
                    !Body.Any(s => s.X == newPos.X && s.Y == newPos.Y))
                {
                    _lastTail = new Tile(tail.X, tail.Y);
                    Body.RemoveAt(Body.Count - 1);
                    Body.Insert(0, newPos);
                    _lastDirection = direction;
                    OnMove(new EventArgs());
                    return true;
                }
            }
            return false;
        }

        public void Add()
        {
            Body.Add(_lastTail);
        }

        public void Reset()
        {
            _lastDirection = Direction.None;
            Body = new List<Tile> {new Tile(1, Convert.ToInt32(_map.Y/2))};
        }

        public event EventHandler<EventArgs> Move;

        protected virtual void OnMove(EventArgs e)
        {
            EventHandler<EventArgs> handler = Move;
            if (handler != null) handler(this, e);
        }
    }
}