using System.ComponentModel;
using SharpYaml.Serialization;

namespace WebcamSettings
{
    internal sealed class CameraSetting
    {
        [YamlMember(0)]
        public int Value { get; init; }

        [YamlMember(1)]
        [DefaultValue(false)]
        public bool IsAuto { get; init; }
    }
}
