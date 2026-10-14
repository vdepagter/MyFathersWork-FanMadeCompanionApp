namespace MyFathersWorkWebApp;

public static partial class ATimeOfWar
{
    // TODO ATOW Gen III: temporary entry point (port map 2.6). Replace with WarningIntro "Future Consequences".
    private static void WarningIntro(GlobalData globalData)
    {
        globalData.SaveToUndo();
        NotYetPorted(globalData);
    }
}
