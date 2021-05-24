using System.Collections.Generic;
using SharpYaml.Serialization;

namespace WebcamSettings
{
    internal sealed class Camera
    {
        [YamlMember(0)]
        public string DevicePath { get; init; }

        public Dictionary<string, CameraSetting> CameraControls { get; private set; } = new();

        public Dictionary<string, CameraSetting> VideoControls { get; private set; } = new();
    }
}
