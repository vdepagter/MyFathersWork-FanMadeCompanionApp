namespace MyFathersWorkWebApp;

public static partial class ATimeOfWar
{
    // TODO ATOW Gen II: temporary entry point (port map 2.2). Replace with IntroMartial-Monarchists "Plagued by Conflict".
    private static void IntroMartialMonarchists(GlobalData globalData)
    {
        globalData.SaveToUndo();
        NotYetPorted(globalData);
    }

    // TODO ATOW Gen II: temporary entry point (port map 2.2). Replace with IntroMartial-Separatists "Down with the Monarchy".
    private static void IntroMartialSeparatists(GlobalData globalData)
    {
        globalData.SaveToUndo();
        NotYetPorted(globalData);
    }
}
