using Scover.WinClean.Resources;

namespace Scover.WinClean.Model.Metadatas;

public sealed class ScriptType : ScriptResourceMetadata
{
    private ScriptType(string resourceName, bool isMutable) : base(ScriptTypes.ResourceManager, resourceName, resourceName + "Description")
            => IsMutable = isMutable;

    public static ScriptType Custom { get; } = new(nameof(ScriptTypes.Custom), true);
    public static ScriptType Default { get; } = new(nameof(ScriptTypes.Default), false);
    public bool IsMutable { get; }
}