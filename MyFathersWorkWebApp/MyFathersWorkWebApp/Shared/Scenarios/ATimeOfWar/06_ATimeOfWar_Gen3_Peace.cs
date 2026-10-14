namespace MyFathersWorkWebApp;

public static partial class ATimeOfWar
{
    // TODO ATOW Gen III: temporary entry point (port map 2.5). Replace with PeaceIntro "Peace Through War".
    private static void PeaceIntro(GlobalData globalData)
    {
        globalData.SaveToUndo();
        NotYetPorted(globalData);
    }
}
