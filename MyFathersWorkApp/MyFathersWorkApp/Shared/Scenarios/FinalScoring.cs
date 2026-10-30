namespace MyFathersWorkApp;

// Score entry, tie breakers and rankings at the end of Generation III - the same for every scenario. The texts use the
// tags below in each scenario's CSV (ScoreEntry_Content, TieBreaker1_Title, ...). Ends in the scenario's FinalEnding.
// Methods calling SaveToUndo must stay unique by name across FinalScoring and the scenario classes.
public static class FinalScoring
{
    public static void Start(GlobalData globalData)
    {
        globalData.ScenarioVars.ScoreIndex = 0;
        ScoreEntry(globalData);
    }

    private static bool IsValidScore(string value)
    {
        return int.TryParse(value, out int score) && score is >= 0 and <= 999;
    }

    private static void ScoreEntry(GlobalData globalData)
    {
        ScenarioVars vars   = globalData.ScenarioVars;
        string       player = globalData.GetPlayerNameByIndex(vars.ScoreIndex);

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
        ScenarioVars vars     = globalData.ScenarioVars;
        string[]     players  = globalData.GetActivePlayers();
        int          topScore = players.Max(player => vars.Scores[player]);
        return players.Where(player => vars.Scores[player] == topScore).ToArray();
    }

    private static void ScoreResult(GlobalData globalData)
    {
        ScenarioVars vars = globalData.ScenarioVars;
        string[]     tied = TopScorers(globalData);

        if (tied.Length == 1)
        {
            vars.TiedPlayers    = [];
            vars.TieMasterworks = [];
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
        globalData.ScenarioVars.Winner       = winner;
        globalData.ScenarioVars.FamilyWinner = string.IsNullOrEmpty(winner);
        Rankings(globalData);
    }

    private static void TieBreaker1(GlobalData globalData)
    {
        ScenarioVars vars   = globalData.ScenarioVars;
        string       player = vars.TiedPlayers[vars.TieIndex];

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, player));
        globalData.ActiveWindow.AddYesNo(_ => TieBreaker1Answer(true, globalData), _ => TieBreaker1Answer(false, globalData));
    }

    private static void TieBreaker1Answer(bool completedMasterwork, GlobalData globalData)
    {
        ScenarioVars vars = globalData.ScenarioVars;

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
        ScenarioVars vars   = globalData.ScenarioVars;
        string       player = vars.TieMasterworks[vars.TieIndex];

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
        ScenarioVars vars = globalData.ScenarioVars;

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();

        // Tie-breakers only separate the tied top scorers: Masterwork first, then Estate Upgrades (when that was asked)
        bool upgradesAsked = vars.TieMasterworks.Length > 1 && vars.TieUpgrades.Length == vars.TieMasterworks.Length;

        (int score, int masterwork, int upgrades) Key(string player)
        {
            int masterworkIndex = Array.IndexOf(vars.TieMasterworks, player);
            return (vars.Scores[player], masterworkIndex >= 0 ? 1 : 0, upgradesAsked && masterworkIndex >= 0 ? vars.TieUpgrades[masterworkIndex] : 0);
        }

        string[] ranking = globalData.GetActivePlayers().OrderByDescending(Key).ToArray();

        for (int x = 0; x < ranking.Length; ++x)
        {
            string player = ranking[x];
            int    place  = ranking.Count(other => Key(other).CompareTo(Key(player)) > 0); // equal players share the place
            globalData.ActiveWindow.AddNextContent(1, false, content => content
                                                                       .FormatWithIndex(0, place)
                                                                       .FormatWithReplacement(1, player)
                                                                       .FormatWithReplacement(2, vars.Scores[player].ToString()));
        }

        globalData.ActiveWindow.AddNextContent(2, true, content => content
                                                                  .FormatWithReplacement(1, ranking[0])
                                                                  .FormatWithCondition(0, () => vars.FamilyWinner));
        globalData.ActiveWindow.AddClickHereToContinue(data => data.ShowFinalEnding());
    }
}
