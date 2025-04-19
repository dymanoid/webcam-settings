using System.Collections.Generic;
using SharpYaml.Serialization;

namespace WebcamSettings;

internal sealed class Camera
{
    [YamlMember(0)]
    public required string DevicePath { get; init; }

    public Dictionary<string, CameraSetting> CameraControls { get; } = [];

    public Dictionary<string, CameraSetting> VideoControls { get; } = [];
}
