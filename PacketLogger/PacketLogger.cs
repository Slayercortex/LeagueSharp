using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using LeagueSharp;
using Microsoft.CSharp;
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

namespace PacketLogger
{
    internal class PacketLogger
    {
        private const int WM_KEYDOWN = 0x100;
        private const int WM_KEYUP = 0x101;
        private const int Activate = 0x09; // Tab
        private const int SwitchActive = 0x20; // Space
        private const int ClearConsole = 0x60; // Numpad 0
        private const int SwitcPacketSend = 0x61; // Numpad 1
        private const int SwitcPacketProcess = 0x62; // Numpad 2
        private const int CursorOverDraw = 0x63; // Numpad 3
        private const int CursorOverInfo = 0x65; // Numpad 5
        private const int PlayerNetworkId = 0x67; // Numpad 7
        private const int PlayerPosition = 0x68; // Numpad 8
        private const int ConsoleForeground = 0x6B; // Numpad +

        private const int CursorOverInfoRange = 300;
        private const int CursorDrawingRadius = 25;

        private readonly Action _onLoadAction;
        private bool _active = true;
        private bool _cursorDrawCircles;
        private bool _packetProcess;
        private bool _packetSend = true;
        private bool _switchActive;

        public PacketLogger()
        {
            _onLoadAction = new CallOnce().A(OnLoad);
            Drawing.OnDraw += OnDraw;
            Game.OnGameUpdate += OnGameUpdate;
            Game.OnGameInput += OnGameInput;
            Game.OnWndProc += OnWndProc;
            Game.OnGameSendPacket += OnGameSendPacket;
            Game.OnGameProcessPacket += OnGameProcessPacket;
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
                Drawing.DrawText(10, 10, Color.Yellow, "Packet Active:");
                Drawing.DrawText(10, 30, Color.Yellow, "Packet Send:");
                Drawing.DrawText(10, 50, Color.Yellow, "Packet Process:");
                Drawing.DrawText(10, 70, Color.Yellow, "Drawing Circles:");

                Drawing.DrawText(135, 10, _active ? Color.Green : Color.Red, _active ? "Enabled" : "Disabled");
                Drawing.DrawText(135, 30, _packetSend ? Color.Green : Color.Red, _packetSend ? "Enabled" : "Disabled");
                Drawing.DrawText(135, 50, _packetProcess ? Color.Green : Color.Red,
                    _packetProcess ? "Enabled" : "Disabled");
                Drawing.DrawText(135, 70, _cursorDrawCircles ? Color.Green : Color.Red,
                    _cursorDrawCircles ? "Enabled" : "Disabled");

                if (_cursorDrawCircles)
                {
                    IEnumerable<Obj_AI_Base> list = GetMonstersNearCursor();
                    foreach (Obj_AI_Base obj in list.Where(obj => obj.IsValid))
                    {
                        Drawing.DrawCircle(obj.Position, obj.BoundingRadius + CursorDrawingRadius, Color.Gray);
                    }
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
                    case ClearConsole:
                        if (args.Msg == WM_KEYUP)
                        {
                            Console.Clear();
                            Game.PrintChat(
                                string.Format(
                                    "<font color='#F7A100'>{0}: {1}</font>",
                                    Assembly.GetExecutingAssembly().GetName().Name,
                                    "Console cleared"
                                    )
                                );
                        }
                        break;
                    case SwitcPacketSend:
                        if (args.Msg == WM_KEYUP)
                        {
                            _packetSend = !_packetSend;
                        }
                        break;
                    case SwitcPacketProcess:
                        if (args.Msg == WM_KEYUP)
                        {
                            _packetProcess = !_packetProcess;
                        }
                        break;
                    case Activate:
                        if (args.Msg == WM_KEYUP)
                        {
                            _active = !_active;
                        }
                        break;
                    case SwitchActive:
                        if (args.Msg == WM_KEYDOWN)
                        {
                            if (!_switchActive)
                            {
                                _switchActive = true;
                                _active = !_active;
                            }
                        }
                        if (args.Msg == WM_KEYUP)
                        {
                            _switchActive = false;
                            _active = !_active;
                        }
                        break;
                    case PlayerNetworkId:
                        if (args.Msg == WM_KEYUP)
                        {
                            Console.WriteLine("{0}{3}Byte: {1}{3}Int: {2}{3}{3}", "Player NetworkId",
                                (byte) ObjectManager.Player.NetworkId, ObjectManager.Player.NetworkId,
                                Environment.NewLine);
                        }
                        break;
                    case PlayerPosition:
                        if (args.Msg == WM_KEYUP)
                        {
                            Console.WriteLine("{0}{4}X: {1}{4}Y: {2}{4}Z: {3}{4}{4}", "Player Positon",
                                ObjectManager.Player.Position.X, ObjectManager.Player.Position.Y,
                                ObjectManager.Player.Position.Z, Environment.NewLine);
                        }
                        break;
                    case CursorOverDraw:
                        if (args.Msg == WM_KEYUP)
                        {
                            _cursorDrawCircles = !_cursorDrawCircles;
                        }
                        break;
                    case CursorOverInfo:
                        if (args.Msg == WM_KEYUP)
                        {
                            IEnumerable<Obj_AI_Base> list = GetMonstersNearCursor();
                            foreach (Obj_AI_Base obj in list)
                            {
                                Console.WriteLine(
                                    "Name: {0}{7}Type: {1}{7}NetworkId: {2}{7}NetworkId(byte): {3}{7}Position: x: {4} y: {5} z: {6}{7}{7}",
                                    obj.Name, obj.Type, obj.NetworkId, (byte) obj.NetworkId, obj.Position.X,
                                    obj.Position.Y,
                                    obj.Position.Z, Environment.NewLine);
                            }
                        }
                        break;
                    case ConsoleForeground:
                        if (args.Msg == WM_KEYUP)
                        {
                            IntPtr handle = WindowServices.FindWindowByCaption(IntPtr.Zero, Console.Title);
                            if (handle != IntPtr.Zero)
                            {
                                WindowServices.SetForegroundWindow(handle);
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void OnGameSendPacket(GamePacketEventArgs args)
        {
            try
            {
                if (_active && _packetSend)
                {
                    LogPacket(args);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void OnGameProcessPacket(GamePacketEventArgs args)
        {
            try
            {
                if (_active && _packetProcess)
                {
                    LogPacket(args);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void OnGameInput(GameInputEventArgs args)
        {
            try
            {
                Match match = Regex.Match(args.Input, @"return\[(.*)\]");
                if (match.Success)
                {
                    var includeAssemblies = new[]
                    {
                        "System.dll",
                        "System.Core.dll",
                        "System.Drawing.dll",
                        @"C:\Users\Lizzaran\Desktop\LeagueSharp\Assemblies\System\LeagueSharp.dll",
                        @"C:\Users\Lizzaran\Desktop\LeagueSharp\Assemblies\System\SharpDX.dll"
                    };
                    using (var codedom = new CSharpCodeProvider())
                    {
                        CompilerResults res = codedom.CompileAssemblyFromSource(
                            new CompilerParameters(includeAssemblies)
                            {
                                GenerateInMemory = true
                            },
                                "using System;using System.Collections.Generic;using System.IO;using System.Linq;using LeagueSharp;using Microsoft.CSharp;using SharpDX;using Color = System.Drawing.Color; public class Codedom { public " + match.Groups[1].Value + " Execute() { {" + args.Input.Remove(0, ("return[" + match.Groups[1].Value + "] ").Length) + "} }}"
                            );
                        Type type = res.CompiledAssembly.GetType("Codedom");

                        object obj = Activator.CreateInstance(type);
                        object output = type.GetMethod("Execute").Invoke(obj, new object[] {});
                        Game.PrintChat("PacketLogger: " + args.Input.Remove(0, ("return[" + match.Groups[1].Value + "] ").Length));
                        if (match.Groups[1].Value.ToLower() != "void")
                            Console.WriteLine(output);
                    }
                    args.Process = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private IEnumerable<Obj_AI_Base> GetMonstersNearCursor()
        {
            return ObjectManager.Get<Obj_AI_Base>()
                .Where(obj => obj.IsValid)
                .Where(
                    obj =>
                        Vector2.Distance(new Vector2(Game.CursorPos.X, Game.CursorPos.Y),
                            new Vector2(obj.Position.X, obj.Position.Y)) <= CursorOverInfoRange)
                .ToList();
        }

        private void LogPacket(GamePacketEventArgs args)
        {
            Console.WriteLine("Channel: {0}{3}Flag: {1}{3}Data: {2}{3}{3}", args.Channel, args.ProtocolFlag,
                args.PacketData.Aggregate(string.Empty,
                    (current, d) => current + (d.ToString(CultureInfo.InvariantCulture) + " ")), Environment.NewLine);
        }
    }
}