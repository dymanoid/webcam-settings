using System;
using CommandLine;

namespace WebcamSettings
{
    internal static class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                return Parser.Default.ParseArguments<StoreOptions, SetOptions, int>(args).MapResult(
                    (StoreOptions store) => SettingsProcessor.Store(store.TargetFile, store.CameraIndex),
                    (SetOptions set) => SettingsProcessor.Set(set.SourceFile),
                    _ => 1);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Critical error occurred: '{e.Message}'");
                return 1;
            }
        }
    }
}
