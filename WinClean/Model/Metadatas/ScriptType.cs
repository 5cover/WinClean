using Scover.WinClean.Resources;

namespace Scover.WinClean.Model.Metadatas;

public sealed class ScriptType : Metadata
{
    private readonly int _order;

    private ScriptType(int order, string resourceName, bool isMutable) : base(new ResourceTextProvider(ScriptTypes.ResourceManager, resourceName))
        => (_order, IsMutable) = (order, isMutable);

    public static ScriptType Custom { get; } = new(1, nameof(ScriptTypes.Custom), true);
    public static ScriptType Default { get; } = new(0, nameof(ScriptTypes.Default), false);
    public bool IsMutable { get; }

    public override int CompareTo(Metadata? other) => _order.CompareTo((other as ScriptType)?._order);
}