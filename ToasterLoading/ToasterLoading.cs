using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Timers;
using LeagueSharp;

namespace ToasterLoading
{
    internal class ToasterLoading
    {
        private const int SecondsToWait = 250;

        private readonly MemoryStream _packet;
        private bool _escaped;
        private Timer _timer;

        public ToasterLoading()
        {
            _packet = new MemoryStream();
            Game.OnGameSendPacket += OnGameSendPacket;
            Drawing.OnDraw += OnDraw;
        }

        private void OnDraw(EventArgs args)
        {
            try
            {
                if (_escaped)
                    return;

                Drawing.DrawText(10, 10, Color.Green, Assembly.GetExecutingAssembly().GetName().Name);
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
                if (args.PacketData[0] == 189 && !_escaped)
                {
                    args.Process = false;
                    _packet.Write(args.PacketData, 0, args.PacketData.Length);
                    _timer = new Timer(SecondsToWait*1000);
                    _timer.Elapsed += OnTimedEvent;
                    _timer.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            try
            {
                if (!_escaped)
                {
                    _escaped = true;
                    Game.SendPacket(_packet.ToArray(), PacketChannel.C2S, PacketProtocolFlags.Reliable);
                    _packet.Close();
                }
                _timer.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}