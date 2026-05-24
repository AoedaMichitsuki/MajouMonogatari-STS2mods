using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;

namespace MajouMonogatari_STS2mods.Shared.Hand;

public sealed class ShiftResult
{
    public ShiftResult(
        CardModel card,
        bool moved,
        int originalIndex,
        int newIndex,
        int primaryStepsMoved,
        IReadOnlyDictionary<CardModel, int> beforeIndices,
        IReadOnlyDictionary<CardModel, int> afterIndices)
    {
        Card = card;
        Moved = moved;
        OriginalIndex = originalIndex;
        NewIndex = newIndex;
        PrimaryStepsMoved = primaryStepsMoved;
        BeforeIndices = beforeIndices;
        AfterIndices = afterIndices;
    }

    public CardModel Card { get; }
    public bool Moved { get; }
    public int OriginalIndex { get; }
    public int NewIndex { get; }
    public int PrimaryStepsMoved { get; }
    public IReadOnlyDictionary<CardModel, int> BeforeIndices { get; }
    public IReadOnlyDictionary<CardModel, int> AfterIndices { get; }
}
