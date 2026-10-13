namespace MyFathersWorkWebApp;

public static partial class FearOfTheUnknown
{
    // Originally PEWitch2: every player receives an Obsession - the one of their Generation I milestone, or a random one
    private static void Obsessions(GlobalData globalData)
    {
        bool creature = Vars(globalData).Path == FearOfTheUnknownHubId.Creature;

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle(content => content.FormatWithCondition(0, () => creature));
        globalData.ActiveWindow.AddCustomContent(LetterHeader(globalData));
        globalData.ActiveWindow.AddNextContent(creature ? 1 : 2);
        globalData.ActiveWindow.AddClickHereToContinue(Obsessions_0);
    }

    private static void Obsessions_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S2_Obsession, PopUpButton.Confirm, data =>
        {
            Vars(data).ObsessionIndex = 0;
            Obsession(data);
        });
    }

    // Originally PEWitch2PlayerA - E
    private static void Obsession(GlobalData globalData)
    {
        FearOfTheUnknownVars vars   = Vars(globalData);
        string               player = globalData.GetPlayerNameByIndex(vars.ObsessionIndex);

        globalData.SaveToUndo();
        int fate = DrawFate(globalData, player);

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle(content => content.FormatWithReplacement(0, player));
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, player).FormatWithReplacement(1, RandomDate(globalData, "Obsession" + player)));
        globalData.ActiveWindow.AddNextContent(fate);
        globalData.ActiveWindow.AddClickHereToContinue(Obsession_0);
    }

    private static void Obsession_0(GlobalData globalData)
    {
        FearOfTheUnknownVars vars   = Vars(globalData);
        string               player = globalData.GetPlayerNameByIndex(vars.ObsessionIndex);

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S2_Obsession, PopUpButton.Confirm, data =>
        {
            Vars(data).ObsessionIndex += 1;

            if (Vars(data).ObsessionIndex < data.PlayersNum) Obsession(data);
            else ObsessionExplanation(data);
        }, content => content.FormatWithReplacement(0, player).FormatWithIndex(1, vars.Fate[player] - 1));
    }

    // Players without a milestone draw from the Obsessions nobody claimed (originally FPFateHub), in turn order
    private static int DrawFate(GlobalData globalData, string player)
    {
        FearOfTheUnknownVars vars = Vars(globalData);
        if (vars.Fate[player] != 0) return vars.Fate[player];

        List<int> remaining = [1, 2, 3, 4, 5, 6];

        for (int x = 0; x < vars.Milestones.Length; ++x)
        {
            if (vars.Milestones[x]) remaining.Remove(x + 1);
        }

        foreach (string other in globalData.GetActivePlayers()) remaining.Remove(vars.Fate[other]);

        vars.Fate[player] = vars.RandomElement(remaining, _RND_OBSESSION + PlayerIndex(globalData, player));
        return vars.Fate[player];
    }

    // Originally PEExplanation1
    private static void ObsessionExplanation(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(Vars(globalData).Path == FearOfTheUnknownHubId.Creature ? CreatureSetup : RevengePlot);
    }
}
