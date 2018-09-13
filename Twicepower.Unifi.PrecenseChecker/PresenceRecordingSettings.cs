using System;
using System.Collections.Generic;
using System.Text;

namespace TwicePower.Unifi.PrecenseChecker
{
    public class PresenceRecordingSettings
    {
        public string[] PresenceIndicationMACs { get; set; }
        public string[] CameraIdsToSetToMotionRecordingIfNoOneIsPresent { get; set; }

        public string SOCKS { get; set; }
        public bool VerifySsl { get; set; } = true;
        public bool EnableNightRecordingIfAtHome { get; internal set; }
    }
}
