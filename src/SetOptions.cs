﻿using CommandLine;

namespace WebcamSettings;

[Verb("set", HelpText = "Set camera settings from specified file.")]
internal sealed class SetOptions(string sourceFile)
{
    [Value(0, MetaName = "source file", Required = true, HelpText = "Source file path to apply camera settings from.")]
    public string SourceFile { get; } = sourceFile;
}
