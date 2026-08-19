namespace SaveHub.WinForms;

/// <summary>A game shown in the Edit/Manage pickers: the title id plus a friendly display label.</summary>
internal sealed record GameOption(string Id, string Display)
{
    public override string ToString()
    {
        return Display;
    }
}
