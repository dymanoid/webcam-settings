using System;
using System.IO;
using SharpYaml.Serialization;

namespace WebcamSettings
{
    internal sealed class SettingsProcessor
    {
        public static int Store(string targetFile, int cameraIndex)
        {
            if (string.IsNullOrWhiteSpace(targetFile))
            {
                throw new ArgumentException($"Target file cannot be null or whitespace.", nameof(targetFile));
            }

            if (cameraIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cameraIndex), "Camera index cannot be negative.");
            }

            CheckDirectory(targetFile);

            var cameraController = new CameraController();
            var cameraSettings = cameraController.GetCameraSettings(cameraIndex);

            var serializer = new Serializer(new SerializerSettings { EmitShortTypeName = true });

            using var outputFile = new FileStream(targetFile, FileMode.Create, FileAccess.Write);
            serializer.Serialize(outputFile, cameraSettings);

            return 0;
        }

        public static int Set(string sourceFile)
        {
            if (string.IsNullOrWhiteSpace(sourceFile))
            {
                throw new ArgumentException($"Source file cannot be null or whitespace.", nameof(sourceFile));
            }

            if (!File.Exists(sourceFile))
            {
                throw new InvalidOperationException($"File '{sourceFile}' does not exist.");
            }

            using var inputFile = new FileStream(sourceFile, FileMode.Open, FileAccess.Read);

            var serializer = new Serializer();
            var camera = serializer.Deserialize<Camera>(inputFile);

            var cameraController = new CameraController();
            cameraController.ApplySettings(camera);

            return 0;
        }

        private static void CheckDirectory(string targetFile)
        {
            var directory = Path.GetDirectoryName(targetFile);
            if (directory is not null && !Directory.Exists(directory))
            {
                throw new InvalidOperationException($"Directory '{directory}' does not exist.");
            }
        }
    }
}
