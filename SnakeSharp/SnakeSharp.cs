using System;
using System.Timers;
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

namespace SnakeSharp
{
    internal class SnakeSharp
    {
        private Direction _direction = Direction.None;
        private Food _food;
        private bool _init;
        private float _lastHealth = float.MaxValue;
        private Map _map;
        private Timer _onTickTimer;
        private bool _pause = true;
        private bool _pauseWait;
        private Score _score;
        private Snake _snake;

        public SnakeSharp()
        {
            Game.OnGameStart += OnGameStart;
            Game.OnGameUpdate += OnGameUpdate;
            Game.OnWndProc += OnWndProc;
            Drawing.OnDraw += DrawingOnDraw;
        }

        private void OnGameStart(EventArgs args)
        {
            _pause = true;
        }

        private void OnGameUpdate(EventArgs args)
        {
            try
            {
                Obj_AI_Hero player = ObjectManager.Player;
                if (!player.IsDead)
                {
                    if (player.Health < _lastHealth - 10)
                    {
                        _pause = true;
                    }
                    _lastHealth = player.Health;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void OnWndProc(WndEventArgs args)
        {
            try
            {
                switch (args.WParam)
                {
                    case (uint) Key.VK_UP:
                        if (args.Msg == (uint) Key.WM_KEYDOWN)
                        {
                            _direction = Direction.Up;
                            _pauseWait = false;
                        }
                        break;
                    case (uint) Key.VK_RIGHT:
                        if (args.Msg == (uint) Key.WM_KEYDOWN)
                        {
                            _direction = Direction.Right;
                            _pauseWait = false;
                        }
                        break;
                    case (uint) Key.VK_DOWN:
                        if (args.Msg == (uint) Key.WM_KEYDOWN)
                        {
                            _direction = Direction.Down;
                            _pauseWait = false;
                        }
                        break;
                    case (uint) Key.VK_LEFT:
                        if (args.Msg == (uint) Key.WM_KEYDOWN)
                        {
                            _direction = Direction.Left;
                            _pauseWait = false;
                        }
                        break;
                    case (uint) Key.VK_PLUS:
                        if (args.Msg == (uint) Key.WM_KEYUP)
                        {
                            _onTickTimer.Interval = _onTickTimer.Interval <= 50 ? 25 : _onTickTimer.Interval - 25;
                        }
                        break;
                    case (uint) Key.VK_MINUS:
                        if (args.Msg == (uint) Key.WM_KEYUP)
                        {
                            _onTickTimer.Interval = _onTickTimer.Interval >= 475 ? 500 : _onTickTimer.Interval + 25;
                        }
                        break;
                    case (uint) Key.VK_NUMPAD5:
                        if (args.Msg == (uint) Key.WM_KEYUP)
                        {
                            if (!_init)
                            {
                                _map = new Map(30, 30);
                                _snake = new Snake(_map);
                                _food = new Food(_map, _snake);
                                _score = new Score(_map, _food);
                                _onTickTimer = new Timer(200);
                                _onTickTimer.Elapsed += OnTick;
                                _onTickTimer.Start();
                                _init = true;
                            }
                            if (_pause)
                                _pauseWait = true;
                            _pause = !_pause;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void OnTick(object state, ElapsedEventArgs elapsedEventArgs)
        {
            try
            {
                if (!_pause && !_pauseWait)
                {
                    if (!_snake.DoMove(_direction) || _snake.Win())
                    {
                        _pauseWait = true;
                        _snake.Reset();
                        _food.Reset();
                        _score.Reset();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void DrawingOnDraw(EventArgs args)
        {
            try
            {
                if (!_pause || _pauseWait)
                {
                    _map.Draw();
                    _food.Draw();
                    _snake.Draw();
                    _score.Draw();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}