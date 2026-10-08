using Styx;
using Styx.Helpers;
using Styx.Logic.Pathing;
using Styx.Plugins.PluginClass;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;

using System;
using System.Diagnostics;
using System.IO;

namespace PluginHotspotRecorder
{
    public class HotspotRecorder : HBPlugin
    {
        public override string Name { get { return "Hotspot Auto Recorder"; } }
        public override string Author { get { return "User"; } }
        public override Version Version { get { return new Version(1, 0, 0, 0); } }

        // Distance threshold in yards before saving a new hotspot
        private const float MinDistanceYards = 10.0f; //change for distance between hotspots
        private const string OutputFileName = "RecordedHotspots.xml";

        private WoWPoint _lastHotspotPos = WoWPoint.Zero;
        private Stopwatch _timer = new Stopwatch();

        public override void Pulse()
        {
            if (!StyxWoW.IsInGame || ObjectManager.Me == null)
                return;

            if (!_timer.IsRunning)
                _timer.Start();

            // Throttle distance check to once every second
            if (_timer.ElapsedMilliseconds >= 1000)
            {
                WoWPoint currentPos = ObjectManager.Me.Location;

                if (_lastHotspotPos == WoWPoint.Zero || currentPos.Distance(_lastHotspotPos) >= MinDistanceYards)
                {
                    SaveHotspot(currentPos);
                    _lastHotspotPos = currentPos;
                }

                _timer.Restart();
            }
        }

        private void SaveHotspot(WoWPoint point)
        {
            try
            {
                // Force full path to CopilotBuddy root directory
                string rootPath = AppDomain.CurrentDomain.BaseDirectory;
                string fullFilePath = Path.Combine(rootPath, OutputFileName);

                // Format X, Y, Z with F6 precision to match "GENERATE HOTSPOT" output
                string xmlLine = string.Format("<Hotspot X=\"{0}\" Y=\"{1}\" Z=\"{2}\" />", 
                    point.X.ToString("F6"), 
                    point.Y.ToString("F6"), 
                    point.Z.ToString("F6"));

                File.AppendAllText(fullFilePath, xmlLine + Environment.NewLine);

                Logging.Write("[Hotspot Recorder]: Saved to {0}", fullFilePath);
            }
            catch (Exception ex)
            {
                Logging.Write("[Hotspot Recorder Error]: {0}", ex.Message);
            }
        }
    }
}
