using System;
using System.Collections.Generic;
using System.Text;

namespace TwicePower.Unifi.PrecenseChecker
{
    public class PresenceRecordingSettings
    {
        public string[] PresenceIndicationMACs { get; set; }
        public string[] CameraIdsToSetToMotionRecordingIfNoOneIsPresent { get; set; }
    }
}
