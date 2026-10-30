namespace MyFathersWorkApp;

public static partial class FearOfTheUnknown
{
    // Round of Generation III that just ended (ShowEndOfRoundPopUp advances Years - and Generation after the last round)
    private static Years EndedYears(GlobalData globalData)
    {
        return globalData.Generation > Generation.Third ? Years.Late : globalData.Years - 1;
    }

    private static int PathIndex(GlobalData globalData) => Vars(globalData).Path switch
    {
        FearOfTheUnknownHubId.Isolation  => 0,
        FearOfTheUnknownHubId.Tension    => 1,
        FearOfTheUnknownHubId.Privatized => 2,
        _                                => 3 // Liberal
    };

    #region Obsessions

    // Originally PEWitch3Intro
    private static void ObsessionsResolve(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S2_Obsession, PopUpButton.Confirm, ObsessionsAltered);
    }

    // Originally PEWitch3
    private static void ObsessionsAltered(GlobalData globalData)
    {
        int path = PathIndex(globalData);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle(content => content.FormatWithIndex(0, path));
        globalData.ActiveWindow.AddCustomContent(LetterHeader(globalData));
        globalData.ActiveWindow.AddNextContent(path + 1);
        globalData.ActiveWindow.AddClickHereToContinue(ObsessionsAltered_0);
    }

    private static void ObsessionsAltered_0(GlobalData globalData)
    {
        FearOfTheUnknownHubId path = Vars(globalData).Path;

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.MaladjustmentBack, PopUpButton.Confirm, ObsessionCards,
            content => content.FormatWithCondition(0, () => path is FearOfTheUnknownHubId.Isolation or FearOfTheUnknownHubId.Privatized));
    }

    // Originally PEWitch3b
    private static void ObsessionCards(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S2_Obsession, PopUpButton.Confirm, ObsessionPenalty, string.Empty);

        string content = string.Empty;

        foreach (string player in globalData.GetActivePlayers().OrderBy(player => vars.Fate[player]))
        {
            content += Tag(globalData, "ObsessionCards_Content").FormatWithReplacement(0, player).FormatWithIndex(1, vars.Fate[player] - 1);
        }

        content += Tag(globalData, "ObsessionCards_Content1");
        globalData.ActivePopup.ReplaceDescription(content.TrimTrailingBreaks());
    }

    // Originally PEExplanation2
    private static void ObsessionPenalty(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(Vars(globalData).Path switch
        {
            FearOfTheUnknownHubId.Isolation  => IsolationSetup,
            FearOfTheUnknownHubId.Tension    => TensionSetup,
            FearOfTheUnknownHubId.Privatized => PrivatizedSetup,
            _                                => LiberalSetup
        });
    }

    // Originally PEWitchEnding
    private static void ObsessionResolution(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S2_Obsession, PopUpButton.Confirm, Scoring, string.Empty);

        string content = Tag(globalData, "ObsessionResolution_Content");
        if (vars.Stole) content += Tag(globalData, "ObsessionResolution_Content1").FormatWithReplacement(0, ObsessionVp(globalData).ToString());

        foreach (string player in globalData.GetActivePlayers().Where(player => vars.Crossroad[player]))
        {
            content += Tag(globalData, "ObsessionResolution_Content2").FormatWithReplacement(0, player);
        }

        globalData.ActivePopup.ReplaceDescription(content.TrimTrailingBreaks());
    }

    #endregion Obsessions

    #region Crossroads

    // Originally Gen3Token2SignIn
    private static void Crossroads(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => Either(globalData, 1, 2, "Crossroads" + globalData.Years)(content)
                                                             .FormatWithReplacement(0, RandomDate(globalData, "Crossroads" + globalData.Years)));
        globalData.ActiveWindow.AddAllPlayersNamesAsOptions(player =>
        {
            Vars(globalData).SelectedPlayer = player;
            LifeForsaken(globalData);
        }, showPlayer: player => !Vars(globalData).CrossroadVisited[player]);
    }

    // Originally Gen3Token2
    private static void LifeForsaken(GlobalData globalData)
    {
        string player = Vars(globalData).SelectedPlayer;

        globalData.SaveToUndo();
        Vars(globalData).CrossroadVisited[player] = true;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => Either(globalData, 2, 2, "LifeForsaken" + player)(content)
                                                             .FormatWithReplacement(0, player)
                                                             .FormatWithReplacement(1, RandomDate(globalData, "LifeForsaken" + player)));
        globalData.ActiveWindow.AddNextContentWithLinks(1, [Conviction, RejectingMyFate], true);
    }

    // Originally Gen3Token2a
    private static void Conviction(GlobalData globalData)
    {
        string player = Vars(globalData).SelectedPlayer;

        globalData.SaveToUndo();
        Vars(globalData).Crossroad[player] = true;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, player).FormatWithReplacement(1, RandomDate(globalData, "Conviction" + player)));
        globalData.ActiveWindow.AddClickHereToContinue(ReturnToHub);
    }

    // Originally Gen3Token2b
    private static void RejectingMyFate(GlobalData globalData)
    {
        string player = Vars(globalData).SelectedPlayer;

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => Either(globalData, 2, 2, "RejectingMyFate" + player)(content)
                                                             .FormatWithReplacement(0, player)
                                                             .FormatWithReplacement(1, RandomDate(globalData, "RejectingMyFate" + player)));
        globalData.ActiveWindow.AddClickHereToContinue(RejectingMyFate_0);
    }

    private static void RejectingMyFate_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.CompulsionBack, PopUpButton.Confirm, ReturnToHub);
    }

    #endregion Crossroads

    #region The Count's Debts

    // Originally CountBlessing / PrivateCountCure / LibCountBlessing
    private static void DebtRepaid(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        if (vars.Cure != ExtendedBool.True)
        {
            AfterDebtRepaid(globalData);
            return;
        }

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, vars.CurePlayer));
        globalData.ActiveWindow.AddClickHereToContinue(DebtRepaid_0);
    }

    private static void DebtRepaid_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S2_BlessingOfTheStrigoi, PopUpButton.Confirm, AfterDebtRepaid,
            content => content.FormatWithReplacement(0, Vars(globalData).CurePlayer));
    }

    private static void AfterDebtRepaid(GlobalData globalData)
    {
        if (Vars(globalData).Path == FearOfTheUnknownHubId.Privatized) HouseForPrivateUse(globalData);
        else UnderstandingAndSupport(globalData);
    }

    // Originally PrivateHomeTile / LiberalHomeTile: the investor who bought the Count's house
    private static void HouseForPrivateUse(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        if (vars.HouseOnTheHill == string.Empty)
        {
            AfterHouseForPrivateUse(globalData);
            return;
        }

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content
                                                             .FormatWithReplacement(0, vars.HouseOnTheHill)
                                                             .FormatWithReplacement(1, RandomDate(globalData, "HouseForPrivateUse")));
        globalData.ActiveWindow.AddClickHereToContinue(HouseForPrivateUse_0);
    }

    private static void HouseForPrivateUse_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S2_HouseOnTheHillTile, PopUpButton.Confirm, AfterHouseForPrivateUse,
            content => content.FormatWithReplacement(0, Vars(globalData).HouseOnTheHill));
    }

    private static void AfterHouseForPrivateUse(GlobalData globalData)
    {
        if (Vars(globalData).Path == FearOfTheUnknownHubId.Privatized) CountInvestment(globalData);
        else DebtRepaid(globalData);
    }

    // Originally PaymentOrDeathA - E: the Count returns for the money still owed
    private static void StartDeathCollection(GlobalData globalData)
    {
        Vars(globalData).PaymentIndex = 0;
        NextDeathCollection(globalData);
    }

    private static void NextDeathCollection(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        while (vars.PaymentIndex < globalData.PlayersNum && vars.Debt[globalData.GetPlayerNameByIndex(vars.PaymentIndex)] == 0) vars.PaymentIndex += 1;

        if (vars.PaymentIndex < globalData.PlayersNum)
        {
            UnwelcomeReturn(globalData);
            return;
        }

        if (vars.Path == FearOfTheUnknownHubId.Privatized) Privatized(globalData);
        else Liberal(globalData);
    }

    private static void UnwelcomeReturn(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDoNotOtherPlayersToSee(globalData.GetPlayerNameByIndex(Vars(globalData).PaymentIndex), UnwelcomeReturnRevealed);
    }

    private static void UnwelcomeReturnRevealed(GlobalData globalData)
    {
        FearOfTheUnknownVars vars   = Vars(globalData);
        string               player = globalData.GetPlayerNameByIndex(vars.PaymentIndex);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultBaseTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, player).FormatWithReplacement(1, vars.Debt[player].ToString()));
        globalData.ActiveWindow.AddNextContentWithLinks(1, [PaymentReceived, PrematureEnd], true);
    }

    // Originally Payment2ThanksA - E
    private static void PaymentReceived(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, globalData.GetPlayerNameByIndex(Vars(globalData).PaymentIndex)));
        globalData.ActiveWindow.AddClickHereToContinue(PaymentReceived_0);
    }

    private static void PaymentReceived_0(GlobalData globalData)
    {
        FearOfTheUnknownVars vars   = Vars(globalData);
        string               player = globalData.GetPlayerNameByIndex(vars.PaymentIndex);

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.Money_Icon, PopUpButton.Confirm, data =>
        {
            Vars(data).Debt[player]  =  0;
            Vars(data).PaymentIndex += 1;
            NextDeathCollection(data);
        }, content => content.FormatWithReplacement(0, vars.Debt[player].ToString()));
    }

    // Originally Payment2NoA - E
    private static void PrematureEnd(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, globalData.GetPlayerNameByIndex(Vars(globalData).PaymentIndex)));
        globalData.ActiveWindow.AddClickHereToContinue(PrematureEnd_0);
    }

    private static void PrematureEnd_0(GlobalData globalData)
    {
        string player = globalData.GetPlayerNameByIndex(Vars(globalData).PaymentIndex);

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.Self, PopUpButton.Confirm, data =>
        {
            Vars(data).Debt[player]  =  0;
            Vars(data).PaymentIndex += 1;
            NextDeathCollection(data);
        });
    }

    #endregion The Count's Debts

    #region The Hunt

    // Originally HuntExplanation
    private static void HuntExplanation(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(ReturnToHub);
    }

    private static bool HuntActive(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);
        return vars.CreatureMade == _CREATURE_NOT_CREATED && !vars.Expedition;
    }

    // Originally Expedition1: at the end of each round the players with the most Hunt tokens hunt the Creature
    private static void Expedition(GlobalData globalData)
    {
        Years ended = EndedYears(globalData);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddCustomContent(JournalHeader(globalData, "Expedition" + ended));
        globalData.ActiveWindow.AddDefaultContent(content => Either(globalData, 1, 3, "Expedition" + ended)(content).FormatWithIndex(0, (int)ended));
        globalData.ActiveWindow.AddClickHereToContinue(ExpeditionTokens);
    }

    // Originally Expedition2
    private static void ExpeditionTokens(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [ExpeditionResult], true);
    }

    // Originally Expedition3: the number of Clues needed shrinks each round
    private static void ExpeditionResult(GlobalData globalData)
    {
        Years ended = EndedYears(globalData);

        globalData.SaveToUndo();
        int clues = Vars(globalData).RandomInRange(ended switch
        {
            Years.Early  => 3,
            Years.Middle => 2,
            _            => 1
        }, 6, _RND_EXPEDITION + (int)ended);

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, clues.ToString()));
        globalData.ActiveWindow.AddNextContentWithLinks(1, [CreatureKilled, CreatureEscapes], true);
    }

    // Originally ExpYes
    private static void CreatureKilled(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(CreatureKilled_0);
    }

    private static void CreatureKilled_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S2_HuntTokenFRONT, PopUpButton.Confirm, data =>
        {
            Vars(data).Expedition = true;

            if (Vars(data).Path == FearOfTheUnknownHubId.Privatized) HostileTakeover(data);
            else if (EndedYears(data) == Years.Late) MenagerieAward(data);
            else EssentialSalts(data);
        });
    }

    // Originally ExpNo
    private static void CreatureEscapes(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(CreatureEscapes_0);
    }

    private static void CreatureEscapes_0(GlobalData globalData)
    {
        Years ended = EndedYears(globalData);

        // The hunt continues next round in Acceptance, except after the last round (source asked to reshuffle then too)
        bool huntContinues = Vars(globalData).Path == FearOfTheUnknownHubId.Liberal && ended != Years.Late;

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S2_HuntTokenFRONT, PopUpButton.Confirm, data =>
        {
            if (Vars(data).Path == FearOfTheUnknownHubId.Privatized) HostileTakeover(data);
            else if (EndedYears(data) == Years.Early) AidToTheInstitute(data);
            else if (EndedYears(data) == Years.Middle) Round20Event(data);
            else MenagerieAward(data);
        }, content => content.FormatWithCondition(0, () => huntContinues));
    }

    #endregion The Hunt

    #region Scoring

    private static void Scoring(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        if (Vars(globalData).CreatureCard) globalData.ActiveWindow.AddNextContent(1);
        globalData.ActiveWindow.AddNextContent(2);
        globalData.ActiveWindow.AddNextContentWithLinks(3, [FinalScoring.Start], true);
    }

    public static void FinalEnding(GlobalData globalData)
    {
        FearOfTheUnknownVars vars   = Vars(globalData);
        string               winner = vars.FamilyWinner ? Tag(globalData, "FinalEnding_Family") : vars.Winner;

        // The winner's Battledome Warrior (the first player's when the family shares the win, as in the source)
        string warrior = vars.WarriorName[vars.FamilyWinner || !vars.WarriorName.ContainsKey(vars.Winner) ? globalData.PlayerAName : vars.Winner];

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.MarkFinalEnding();
        globalData.ActiveWindow.AddDefaultTitle(content => content
                                                           .FormatWithReplacement(1, globalData.TownName)
                                                           .FormatWithReplacement(2, Warden(globalData))
                                                           .FormatWithIndex(0, vars.Ending));
        globalData.ActiveWindow.AddNextContent(vars.Ending, false, content => content
                                                                             .FormatWithReplacement(0, winner)
                                                                             .FormatWithReplacement(1, globalData.TownName)
                                                                             .FormatWithReplacement(2, Warden(globalData))
                                                                             .FormatWithReplacement(3, warrior)
                                                                             .FormatWithReplacement(4, vars.ProgramName)
                                                                             .FormatWithCondition(5, () => vars.CreatureMade == _CREATURE_CREATED)
                                                                             .FormatWithCondition(6, () => vars.Cure == ExtendedBool.True));
        globalData.ActiveWindow.AddNextContent(10, true);
    }

    #endregion Scoring
}
