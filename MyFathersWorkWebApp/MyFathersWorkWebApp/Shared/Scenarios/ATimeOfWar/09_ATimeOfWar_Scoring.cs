namespace MyFathersWorkWebApp;

public static partial class ATimeOfWar
{
    // TODO ATOW Scoring: temporary (port map 2.8). Replace with the endings 1 - 8 (FotU FinalEnding pattern). Called by
    // GlobalData.ShowFinalEnding after FinalScoring; unreachable until Generation III is ported.
    public static void FinalEnding(GlobalData globalData)
    {
        globalData.SaveToUndo();
        NotYetPorted(globalData);
        globalData.ActiveWindow!.MarkFinalEnding();
    }
}
