using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CSCore.Codecs;
using CSCore.CoreAudioAPI;
using CSCore.SoundOut;

namespace SoundMachine
{
    static class SoundPlaying
    {
        private static readonly ConcurrentDictionary<ISoundOut, string> ActiveMediaPlayers = new ConcurrentDictionary<ISoundOut, string>();

        public static void DisposeAll()
        {
            MuteAll();
            var players = ActiveMediaPlayers.Keys.ToArray();
            foreach (var pl in players)
                pl.Dispose();
        }

        public static void MuteAll()
        {
            // Stop will change CD, so make a copy first
            var players = ActiveMediaPlayers.Keys.ToArray();
            foreach (var pl in players)
                pl.Stop();
        }

        public static void Play(this Sound sound)
        {
            try
            {
                var soundOut = new WasapiOut(true, AudioClientShareMode.Shared, 5) {Device = BestDevice};
                soundOut.Initialize(CodecFactory.Instance.GetCodec(sound.Path));
                soundOut.Stopped += (sender, args) =>
                {
                    string _;
                    var player = (ISoundOut) sender;
                    if (ActiveMediaPlayers.TryRemove(player, out _))
                        player.Dispose();
                };
                ActiveMediaPlayers.TryAdd(soundOut, sound.Path);
                soundOut.Play();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), e.Message);
            }
            
        }

        private static IEnumerable<MMDevice> EnumerateWasapiDevices()
        {
            using (var enumerator = new MMDeviceEnumerator())
            {
                return enumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active);
            }
        }

        private static MMDevice BestDevice
        {
            get
            {
                return EnumerateWasapiDevices().FirstOrDefault(mmd => mmd.FriendlyName.Contains("Speakers")) ??
                       EnumerateWasapiDevices().FirstOrDefault();
            }
        }
    }
}