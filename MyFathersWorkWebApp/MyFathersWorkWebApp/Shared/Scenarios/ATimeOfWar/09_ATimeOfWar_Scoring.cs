namespace MyFathersWorkWebApp;

public static partial class ATimeOfWar
{
    // TODO ATOW Scoring: temporary (port map 2.8 / 2.9). Shared entry of every Generation III branch after its end bonuses
    // (Reign: BenevolenceBonus, Peace: FinalBattleComplete). Replace with the Scoring passage -> FinalScoring.Start.
    private static void Scoring(GlobalData globalData)
    {
        globalData.SaveToUndo();
        NotYetPorted(globalData);
    }

    // TODO ATOW Scoring: temporary (port map 2.8). Replace with the endings 1 - 8 (FotU FinalEnding pattern). Called by
    // GlobalData.ShowFinalEnding after FinalScoring; unreachable until Scoring is ported.
    public static void FinalEnding(GlobalData globalData)
    {
        globalData.SaveToUndo();
        NotYetPorted(globalData);
        globalData.ActiveWindow!.MarkFinalEnding();
    }
}
