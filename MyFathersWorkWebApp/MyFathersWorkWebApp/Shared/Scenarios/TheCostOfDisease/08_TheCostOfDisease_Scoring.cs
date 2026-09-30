namespace MyFathersWorkWebApp;

public static partial class TheCostOfDisease
{
    #region Scoring

    private static void Scoring(GlobalData globalData)
    {
        TheCostOfDiseaseVars vars = globalData.TheCostOfDiseaseVars;

        globalData.SaveToUndo();
        vars.ScoreIndex = 0;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        if (vars.Lycan) globalData.ActiveWindow.AddNextContent(1);
        if (vars.Immort) globalData.ActiveWindow.AddNextContent(2);
        globalData.ActiveWindow.AddNextContentWithLinks(3, [ScoreEntry], true);
    }

    private static bool IsValidScore(string value)
    {
        return int.TryParse(value, out int score) && score is >= 0 and <= 999;
    }

    private static void ScoreEntry(GlobalData globalData)
    {
        TheCostOfDiseaseVars vars   = globalData.TheCostOfDiseaseVars;
        string               player = globalData.GetPlayerNameByIndex(vars.ScoreIndex);

        globalData.SaveToUndo();
        globalData.ActiveInputPopup = new GameplayInputPopup(globalData, "0", PopUpButton.Confirm, IsValidScore, value =>
        {
            vars.Scores[player] =  int.Parse(value);
            vars.ScoreIndex     += 1;

            if (vars.ScoreIndex < globalData.PlayersNum) ScoreEntry(globalData);
            else ScoreResult(globalData);
        }, false, content => content.FormatWithReplacement(0, player));
    }

    private static string[] TopScorers(GlobalData globalData)
    {
        TheCostOfDiseaseVars vars     = globalData.TheCostOfDiseaseVars;
        string[]             players  = globalData.GetActivePlayers();
        int                  topScore = players.Max(player => vars.Scores[player]);
        return players.Where(player => vars.Scores[player] == topScore).ToArray();
    }

    private static void ScoreResult(GlobalData globalData)
    {
        TheCostOfDiseaseVars vars = globalData.TheCostOfDiseaseVars;
        string[]             tied = TopScorers(globalData);

        if (tied.Length == 1)
        {
            DeclareWinner(tied[0], globalData);
            return;
        }

        vars.TiedPlayers    = tied;
        vars.TieMasterworks = [];
        vars.TieIndex       = 0;
        TieBreaker1(globalData);
    }

    // Rulebook, End of the Game: most points wins. Ties go to the player who completed their Masterwork,
    // then to the player with the most Estate Upgrades. If still tied, the family shares the win.
    private static void DeclareWinner(string winner, GlobalData globalData)
    {
        globalData.TheCostOfDiseaseVars.Winner       = winner;
        globalData.TheCostOfDiseaseVars.FamilyWinner = string.IsNullOrEmpty(winner);
        Rankings(globalData);
    }

    private static void TieBreaker1(GlobalData globalData)
    {
        TheCostOfDiseaseVars vars   = globalData.TheCostOfDiseaseVars;
        string               player = vars.TiedPlayers[vars.TieIndex];

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, player));
        globalData.ActiveWindow.AddYesNo(_ => TieBreaker1Answer(true, globalData), _ => TieBreaker1Answer(false, globalData));
    }

    private static void TieBreaker1Answer(bool completedMasterwork, GlobalData globalData)
    {
        TheCostOfDiseaseVars vars = globalData.TheCostOfDiseaseVars;

        if (completedMasterwork) vars.TieMasterworks = [..vars.TieMasterworks, vars.TiedPlayers[vars.TieIndex]];
        vars.TieIndex += 1;

        if (vars.TieIndex < vars.TiedPlayers.Length)
        {
            TieBreaker1(globalData);
            return;
        }

        switch (vars.TieMasterworks.Length)
        {
            case 0:
            {
                DeclareWinner(string.Empty, globalData);
                break;
            }
            case 1:
            {
                DeclareWinner(vars.TieMasterworks[0], globalData);
                break;
            }
            default:
            {
                vars.TieIndex    = 0;
                vars.TieUpgrades = new int[vars.TieMasterworks.Length];
                TieBreaker2(globalData);
                break;
            }
        }
    }

    private static void TieBreaker2(GlobalData globalData)
    {
        TheCostOfDiseaseVars vars   = globalData.TheCostOfDiseaseVars;
        string               player = vars.TieMasterworks[vars.TieIndex];

        globalData.SaveToUndo();
        globalData.ActiveInputPopup = new GameplayInputPopup(globalData, "0", PopUpButton.Confirm, IsValidScore, value =>
        {
            vars.TieUpgrades[vars.TieIndex] =  int.Parse(value);
            vars.TieIndex                   += 1;

            if (vars.TieIndex < vars.TieMasterworks.Length)
            {
                TieBreaker2(globalData);
                return;
            }

            int      mostUpgrades = vars.TieUpgrades.Max();
            string[] leaders      = vars.TieMasterworks.Where((_, index) => vars.TieUpgrades[index] == mostUpgrades).ToArray();
            DeclareWinner(leaders.Length == 1 ? leaders[0] : string.Empty, globalData);
        }, false, content => content.FormatWithReplacement(0, player));
    }

    private static void Rankings(GlobalData globalData)
    {
        TheCostOfDiseaseVars vars = globalData.TheCostOfDiseaseVars;

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();

        string[] ranking = globalData.GetActivePlayers().OrderByDescending(player => vars.Scores[player]).ThenByDescending(player => player == vars.Winner).ToArray();

        for (int x = 0; x < ranking.Length; ++x)
        {
            string player = ranking[x];
            int    place  = x;
            globalData.ActiveWindow.AddNextContent(1, false, content => content
                                                                       .FormatWithIndex(0, place)
                                                                       .FormatWithReplacement(1, player)
                                                                       .FormatWithReplacement(2, vars.Scores[player].ToString()));
        }

        globalData.ActiveWindow.AddNextContent(2, true, content => content
                                                                  .FormatWithReplacement(1, ranking[0])
                                                                  .FormatWithCondition(0, () => vars.FamilyWinner));
        globalData.ActiveWindow.AddClickHereToContinue(FinalEnding);
    }

    #endregion Scoring

    #region Endings

    private static void FinalEnding(GlobalData globalData)
    {
        TheCostOfDiseaseVars vars = globalData.TheCostOfDiseaseVars;

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle(content => content.FormatWithIndex(0, (int)vars.Ending));
        globalData.ActiveWindow.AddNextContent((int)vars.Ending, false, content => content
                                                                                  .FormatWithReplacement(0, globalData.TownName)
                                                                                  .FormatWithIndex(1, globalData.LocalizedPlayerNumberIndex())
                                                                                  .FormatWithCondition(2, () => vars.Uni == ExtendedBool.True)
                                                                                  .FormatWithCondition(3, () => vars.Electricity != ExtendedBool.False)
                                                                                  .FormatWithCondition(4, () => vars.Cured == ExtendedBool.True)
                                                                                  .FormatWithCondition(5, () => vars.Ultimate));
        globalData.ActiveWindow.AddNextContent(10, true);
    }

    #endregion Endings
}
