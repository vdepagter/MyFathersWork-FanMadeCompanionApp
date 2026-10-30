namespace MyFathersWorkApp;

// Scoring and the endings (port map 2.8 / 2.9, section 8). Scoring is the shared end of every Generation III branch after
// its own end bonuses (Reign: BenevolenceBonus, Peace: FinalBattleComplete, Warning: the Late end-of-round popup,
// Paradox: ParadoxEvent). FinalScoring (shared) handles score entry, tie breakers and rankings, then calls FinalEnding.
public static partial class ATimeOfWar
{
    // The Time Machine Masterwork reminder only exists on the Time Travel path, where the question was asked (port map 7,
    // B30). Branch bonuses were shown in their own passages before.
    private static void Scoring(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        if (Vars(globalData).TimeMachineMasterwork) globalData.ActiveWindow.AddNextContent(1);
        globalData.ActiveWindow.AddNextContent(2);
        globalData.ActiveWindow.AddNextContentWithLinks(3, [FinalScoring.Start], true);
    }

    // Originally VarEndingsPassage -> ATOW-End1 ... ATOW-End8 ($ending, set by ParadoxEvent, WarningIntro / Stasis,
    // ShadowBonus, FinalBattleComplete). The endings use the ranking name ("The family" on a shared win), not the regal
    // title of Heavy is the Crown.
    public static void FinalEnding(GlobalData globalData)
    {
        ATimeOfWarVars vars   = Vars(globalData);
        int            ending = vars.Ending;
        string         winner = vars.FamilyWinner ? globalData.GetScenarioLocalizedTag("FinalEnding_Family") : vars.Winner;

        // Flavor: the giant invention of ending 1 (3 names), the weapon of ending 7 (9 names)
        int flavor = ending switch
        {
            1 => vars.Flavor("FinalEnding#1", 3),
            7 => vars.Flavor("FinalEnding#7", 9),
            _ => 0
        };

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.MarkFinalEnding();
        globalData.ActiveWindow.AddDefaultTitle(title => title.FormatWithIndex(0, ending));

        // Inner placeholders are formatted before the conditions that contain them (ending 5: 7 inside 6 inside 8; ending
        // 8: 11 inside 10)
        globalData.ActiveWindow.AddNextContent(ending, false, content => content
                                                                       .FormatWithReplacement(0, winner)
                                                                       .FormatWithReplacement(1, globalData.TownName)
                                                                       .FormatWithReplacement(2, Winner(globalData))
                                                                       .FormatWithReplacement(3, vars.NewMeat)
                                                                       .FormatWithIndex(4, flavor)
                                                                       .FormatWithCondition(5, () => vars.Releases >= 1)
                                                                       .FormatWithCondition(7, () => vars.BuildingsDestroyed > 0)
                                                                       .FormatWithCondition(6, () => vars.WarWinner == WarFaction.Separatists)
                                                                       .FormatWithCondition(8, () => vars.Barracks == ExtendedBool.True)
                                                                       .FormatWithCondition(9, () => vars.Benevolent == ExtendedBool.True)
                                                                       .FormatWithCondition(11, () => vars.Gas == ExtendedBool.True)
                                                                       .FormatWithCondition(10, () => vars.Meat != (vars.Gas == ExtendedBool.True)));
        globalData.ActiveWindow.AddNextContent(10, true);
    }
}
