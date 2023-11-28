using Scover.WinClean.Resources;

namespace Scover.WinClean.Model.Metadatas;

public sealed class ScriptType : OrderedMetadata
{
    private ScriptType(string resourceName, int order, bool isMutable) : base(new ResourceTextProvider(ScriptTypes.ResourceManager, resourceName), order)
        => IsMutable = isMutable;

    public static ScriptType Custom { get; } = new(nameof(ScriptTypes.Custom), 1, true);
    public static ScriptType Default { get; } = new(nameof(ScriptTypes.Default), 0, false);
    public bool IsMutable { get; }
}