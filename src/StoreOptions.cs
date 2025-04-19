using CommandLine;

namespace WebcamSettings;

[Verb("store", HelpText = "Show camera settings dialog and then store camera settings into specified file.")]
internal sealed class StoreOptions(string targetFile, int cameraIndex)
{
    [Value(0, MetaName = "target file", Required = true, HelpText = "Target file path to save camera settings to.")]
    public string TargetFile { get; } = targetFile;

    [Option('c', "camera", Default = 0, HelpText = "Camera index to store settings of, defaults to first detected camera.")]
    public int CameraIndex { get; } = cameraIndex;
}
