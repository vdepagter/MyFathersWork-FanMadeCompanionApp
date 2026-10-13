namespace MyFathersWorkWebApp;

public static partial class FearOfTheUnknown
{
    public static void Foreigners(GlobalData globalData)
    {
        // Round 4 -> Foreign
        // Round 5 -> Foreign2
        // Round 6 -> Foreign3
        FearOfTheUnknownVars vars = Vars(globalData);

        vars.HubId           = FearOfTheUnknownHubId.Foreigners;
        globalData.ActiveHub = new GameplayHub(globalData);
        globalData.ActiveHub.SetDefaultTitle();
        globalData.ActiveHub.SetSubtitle(globalData.Years);

        const string       witchingHourSection = "WitchingHour";
        GameplayHubSection witchingHour        = globalData.ActiveHub.AddSection(witchingHourSection, true);
        witchingHour.ReplaceShouldShow(() => globalData.Years is Years.Early or Years.Middle);
        if (globalData.Years is Years.Early or Years.Middle) witchingHour.AddDefaultContent(witchingHourSection, content => content.FormatWithReplacement(0, HexedLocation(globalData)));

        const string       witchingHourLateSection = "WitchingHourLate";
        GameplayHubSection witchingHourLate        = globalData.ActiveHub.AddSection(witchingHourLateSection, true);
        witchingHourLate.ReplaceShouldShow(() => globalData.Years == Years.Late && vars.Cursed == 1);
        witchingHourLate.AddDefaultContent(witchingHourLateSection);

        const string       rumorMillSection = "RumorMill";
        GameplayHubSection rumorMill        = globalData.ActiveHub.AddSection(rumorMillSection);
        rumorMill.ReplaceShouldShow(() => globalData.Years is Years.Early or Years.Middle);

        if (globalData.Years == Years.Early && HexOf(globalData, Witch(globalData), false) == 0)
        {
            rumorMill.AddNextContent(1, rumorMillSection); // the Church itself is hexed
        }
        else
        {
            rumorMill.AddDefaultContent(rumorMillSection);
            rumorMill.AddSpecialClickHere(rumorMillSection, RumorMill);
            rumorMill.AddNextContent(2, rumorMillSection, true);
        }

        const string       townStrengthSection = "TownStrength";
        GameplayHubSection townStrength        = globalData.ActiveHub.AddSection(townStrengthSection, true);
        townStrength.AddDefaultContent(townStrengthSection, content => content.FormatWithReplacement(0, BrickValue(globalData).ToString()));
        townStrength.AddSpecialClickHere(townStrengthSection, TownHallExplanation);

        const string       keyToTheCitySection = "KeyToTheCity";
        GameplayHubSection keyToTheCity        = globalData.ActiveHub.AddSection(keyToTheCitySection);
        keyToTheCity.AddDefaultContent(keyToTheCitySection, content => content.FormatWithCondition(0, () => globalData.PlayersNum >= 4));

        const string       bankLibrarySection = "BankLibrary";
        GameplayHubSection bankLibrary        = globalData.ActiveHub.AddSection(bankLibrarySection);
        bankLibrary.ReplaceShouldShow(() => globalData.Years == Years.Early);
        bankLibrary.AddDefaultContent(bankLibrarySection);
        bankLibrary.AddSpecialClickHere(bankLibrarySection, BankLibraryExplanation);

        const string       accusationsSection = "Accusations";
        GameplayHubSection accusations        = globalData.ActiveHub.AddSection(accusationsSection);
        accusations.ReplaceShouldShow(() => globalData.Years is Years.Early or Years.Middle);
        accusations.AddDefaultContent(accusationsSection);

        const string       pressingForwardSection = "PressingForward";
        GameplayHubSection pressingForward        = globalData.ActiveHub.AddSection(pressingForwardSection);
        pressingForward.ReplaceShouldShow(() => !vars.RevengeResolved);
        pressingForward.AddDefaultContent(pressingForwardSection);
        pressingForward.AddSpecialClickHere(pressingForwardSection, data =>
        {
            if (Vars(data).RevengePlot == ExtendedBool.True) Fortune35(data);
            else Misfortune35(data);
        });

        GameplayHubSection nextRound = globalData.ActiveHub.AddSection(string.Empty);
        nextRound.ReplaceShouldShow(() => globalData.Years is Years.Early or Years.Middle);
        nextRound.AddClickHereContinueNextRound(Foreigners_0);

        globalData.ActiveHub.AddEndOfGenerationSection(Foreigners_0);
    }

    private static void Foreigners_0(GlobalData globalData)
    {
        globalData.ShowEndOfRoundPopUp(globalData.Years switch
        {
            Years.Early  => WallEvent,
            Years.Middle => Accusations,
            Years.Late   => KeyToTheCityAward,
            _            => _ => { }
        });
    }

    #region Draws

    private static int BrickValue(GlobalData globalData) => Vars(globalData).RandomElement([1, 2], _RND_BRICK_VALUE);

    // The cursed cousin: with 2 or 3 players the Apprentice may be the witch (drawn when the first hex is revealed)
    private static string Witch(GlobalData globalData)
    {
        List<string> players = globalData.GetActivePlayers().ToList();

        List<string> candidates = globalData.PlayersNum switch
        {
            2 => [..players, ..players, FearOfTheUnknownVars.SUSPECT],
            3 => [..players, ..players, FearOfTheUnknownVars.SUSPECT],
            _ => players
        };

        return Vars(globalData).RandomElement(candidates, _RND_WITCH);
    }

    // Hex choice of a player, or the Apprentice's (drawn when it is revealed)
    private static int HexOf(GlobalData globalData, string player, bool secondHex)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        if (player == FearOfTheUnknownVars.SUSPECT)
        {
            return vars.RandomElement([0, 1, 2], secondHex ? _RND_SUSPECT_HEX2 : _RND_SUSPECT_HEX1);
        }

        return secondHex ? vars.Hex2[player] : vars.Hex1[player];
    }

    private static string HexLocationName(GlobalData globalData, int location, bool secondHex)
    {
        return Tag(globalData, secondHex ? "HexLocation2" : "HexLocation1").FormatWithIndex(0, location);
    }

    // Location hexed by the witch this round
    private static string HexedLocation(GlobalData globalData)
    {
        bool secondHex = globalData.Years != Years.Early;
        return HexLocationName(globalData, HexOf(globalData, Witch(globalData), secondHex), secondHex);
    }

    #endregion Draws

    #region Intro

    private static void ForeignersIntro(GlobalData globalData)
    {
        globalData.SaveToUndo();
        Vars(globalData).Path = FearOfTheUnknownHubId.Foreigners;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.SetVoiceOver("ForeignIntro");
        globalData.ActiveWindow.AddGameplayTitle(GlobalTags.Gameplay_Generation_II);
        globalData.ActiveWindow.AddDefaultSubtitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, RandomPlayer(globalData)));
        globalData.ActiveWindow.AddClickHereToContinue(StrengtheningTheTown);
    }

    private static void StrengtheningTheTown(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, RandomPlayer(globalData)).FormatWithReplacement(1, globalData.TownName));
        globalData.ActiveWindow.AddClickHereToContinue(StrengtheningTheTown_0);
    }

    private static void StrengtheningTheTown_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S2_BrickTokens, PopUpButton.Confirm, Obsessions,
            content => content.FormatWithReplacement(0, BrickValue(globalData).ToString()).FormatWithCondition(1, () => globalData.PlayersNum >= 4));
    }

    // Originally RevengePlot35 - the player with the least VP secretly chooses the effect of reaching 35VP
    private static void RevengePlot(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddOnceYouAreReady(RevengePlotChoice);
    }

    private static void RevengePlotChoice(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultBaseTitle();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [RevengeFortune, RevengeMisfortune]);
    }

    private static void RevengeFortune(GlobalData globalData)
    {
        globalData.SaveToUndo();
        Vars(globalData).RevengePlot = ExtendedBool.True;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultBaseTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(RevengePlot_0);
    }

    private static void RevengeMisfortune(GlobalData globalData)
    {
        globalData.SaveToUndo();
        Vars(globalData).RevengePlot = ExtendedBool.False;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultBaseTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(RevengePlot_0);
    }

    private static void RevengePlot_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.StorybookToken, PopUpButton.Confirm, WitchIntro);
    }

    private static void WitchIntro(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content
                                                             .FormatWithReplacement(0, RandomPlayer(globalData))
                                                             .FormatWithReplacement(2, Suspect(globalData))
                                                             .FormatWithCondition(1, () => SuspectInPlay(globalData)));
        globalData.ActiveWindow.AddClickHereToContinue(GameOfHexes);
    }

    private static void GameOfHexes(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content
                                                             .FormatWithReplacement(2, Suspect(globalData))
                                                             .FormatWithReplacement(3, globalData.TownName)
                                                             .FormatWithCondition(0, () => SuspectInPlay(globalData))
                                                             .FormatWithCondition(1, () => SuspectInPlay(globalData)));
        globalData.ActiveWindow.AddClickHereToContinue(GameOfHexes_0);
    }

    private static void GameOfHexes_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.VillageChronicleCover, PopUpButton.Confirm, data =>
        {
            Vars(data).HexIndex = 0;
            FirstHex(data);
        }, content => content.FormatWithCondition(0, () => Vars(globalData).Victim == _VICTIM_BUTCHER));
    }

    #endregion Intro

    #region Hexes

    // Originally Witchwolf1a - e: every player secretly chooses a location to hex
    private static void FirstHex(GlobalData globalData)
    {
        string player = globalData.GetPlayerNameByIndex(Vars(globalData).HexIndex);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, player));
        globalData.ActiveWindow.AddNextContentWithLinks(1, [data => ChooseHex(0, false, data), data => ChooseHex(1, false, data), data => ChooseHex(2, false, data)], true);
    }

    // Originally Witchwolf2a - e
    private static void SecondHex(GlobalData globalData)
    {
        string player = globalData.GetPlayerNameByIndex(Vars(globalData).HexIndex);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, player));
        globalData.ActiveWindow.AddNextContentWithLinks(1, [data => ChooseHex(0, true, data), data => ChooseHex(1, true, data), data => ChooseHex(2, true, data)], true);
    }

    private static void ChooseHex(int location, bool secondHex, GlobalData globalData)
    {
        FearOfTheUnknownVars vars   = Vars(globalData);
        string               player = globalData.GetPlayerNameByIndex(vars.HexIndex);

        if (secondHex) vars.Hex2[player] = location;
        else vars.Hex1[player] = location;

        vars.HexIndex += 1;

        if (vars.HexIndex < globalData.PlayersNum)
        {
            if (secondHex) SecondHex(globalData);
            else FirstHex(globalData);
            return;
        }

        if (secondHex) SecondHexRevealed(globalData);
        else FirstHexRevealed(globalData);
    }

    // Originally Witchwolf1Hex
    private static void FirstHexRevealed(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S2_WitchHexToken, PopUpButton.Confirm, ForeignersSetup,
            content => content.FormatWithReplacement(0, HexedLocation(globalData)));
    }

    // Originally Witchwolf2Hex
    private static void SecondHexRevealed(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S2_WitchHexToken, PopUpButton.Confirm, Foreigners,
            content => content.FormatWithReplacement(0, HexedLocation(globalData)));
    }

    private static void ForeignersSetup(GlobalData globalData)
    {
        globalData.SaveToUndo();

        FearOfTheUnknownVars vars = Vars(globalData);
        if (globalData.PlayersNum == 4) vars.Tracker += 1;

        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.AngryMobSetup1, PopUpButton.Confirm, Foreigners, content => content
                                                                                                                                                         .FormatWithIndex(1, globalData.LocalizedPlayerNumberIndex())
                                                                                                                                                         .FormatWithCondition(0, () => globalData.PlayersNum <= 3)
                                                                                                                                                         .FormatWithReplacement(2, vars.Tracker.ToString())
                                                                                                                                                         .FormatWithCondition(3, () => globalData.PlayersNum == 3));
    }

    #endregion Hexes

    #region Hub Events

    // Originally CheckW1 / CheckW2: spy on the hex chosen by a player
    private static void RumorMill(GlobalData globalData)
    {
        bool secondHex = globalData.Years != Years.Early;

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddNextContent(secondHex ? 2 : 1, false, content => Either(globalData, 3, 2, "RumorMill")(content)
                                                                                   .FormatWithReplacement(0, Newspaper(globalData))
                                                                                   .FormatWithReplacement(1, RandomDate(globalData, "RumorMill" + globalData.Years, "Flavor_NewsDate"))
                                                                                   .FormatWithReplacement(2, HexedLocation(globalData)));
        globalData.ActiveWindow.AddNextContent(3, true);

        List<string> targets = globalData.GetActivePlayers().ToList();
        if (SuspectInPlay(globalData)) targets.Add(FearOfTheUnknownVars.SUSPECT);

        foreach (string target in targets)
        {
            globalData.ActiveWindow.AddCustomLink(Name(globalData, target), data =>
            {
                Vars(data).RumorTarget = target;
                Rumor(data);
            });
        }
    }

    private static void Rumor(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddCustomContent(Tag(globalData, "Secret_Content"));
        globalData.ActiveWindow.AddOnceYouAreReady(RumorRevealed);
    }

    private static void RumorRevealed(GlobalData globalData)
    {
        string target    = Vars(globalData).RumorTarget;
        bool   secondHex = globalData.Years != Years.Early;

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content
                                                             .FormatWithReplacement(0, Name(globalData, target))
                                                             .FormatWithReplacement(1, HexLocationName(globalData, HexOf(globalData, target, secondHex), secondHex)));
        globalData.ActiveWindow.AddClickHereToContinue(Foreigners);
    }

    private static void TownHallExplanation(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, BrickValue(globalData).ToString()).FormatWithCondition(1, () => globalData.PlayersNum >= 4));
        globalData.ActiveWindow.AddClickHereToContinue(Foreigners);
    }

    private static void BankLibraryExplanation(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithCondition(0, () => Vars(globalData).Victim == _VICTIM_BLACKSMITH));
        globalData.ActiveWindow.AddClickHereToContinue(Foreigners);
    }

    private static void Fortune35(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(Fortune35_0);
    }

    private static void Fortune35_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.AdvanceJournalTrack, PopUpButton.Confirm, data =>
        {
            Vars(data).RevengeResolved = true;
            Foreigners(data);
        });
    }

    private static void Misfortune35(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(Misfortune35_0);
    }

    private static void Misfortune35_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.Servant, PopUpButton.Confirm, data =>
        {
            Vars(data).RevengeResolved = true;
            Foreigners(data);
        });
    }

    #endregion Hub Events

    #region End of the Early Years - The Memorial

    private static void WallEvent(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddCustomContent(LetterHeader(globalData));
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(WallVote);
    }

    private static void WallVote(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.SetCue(SoundCue.SpecialEvent);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereAfterVote(WallVoteResolved);
    }

    private static void WallVoteResolved(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [data => WallChosen(ExtendedBool.True, data), data => WallChosen(ExtendedBool.False, data)], true);
    }

    private static void WallChosen(ExtendedBool strength, GlobalData globalData)
    {
        Vars(globalData).Walls = strength;
        VaguePromise(globalData);
    }

    private static void VaguePromise(GlobalData globalData)
    {
        bool strength = Vars(globalData).Walls == ExtendedBool.True;

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContent(strength ? 1 : 2, true);
        globalData.ActiveWindow.AddClickHereToContinue(VaguePromise_0);
    }

    private static void VaguePromise_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S2_BrickTokens, PopUpButton.Confirm, data =>
        {
            Vars(data).HexIndex = 0;
            SecondHex(data);
        }, content => content.FormatWithCondition(0, () => Vars(globalData).Walls == ExtendedBool.True));
    }

    #endregion End of the Early Years - The Memorial

    #region End of the Middle Years - Accusations

    // Originally Witchwolfresolution
    private static void Accusations(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddCustomContent(LetterHeader(globalData));
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(Accuse);
    }

    private static void Accuse(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content
                                                             .FormatWithReplacement(1, Suspect(globalData))
                                                             .FormatWithCondition(0, () => SuspectInPlay(globalData)));
        globalData.ActiveWindow.AddNextContentWithLinks(1, [DeathAndResolution], true);
    }

    // Originally Witchres1
    private static void DeathAndResolution(GlobalData globalData)
    {
        FearOfTheUnknownVars vars  = Vars(globalData);
        string               witch = Witch(globalData);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Name(globalData, witch)));

        if (SuspectInPlay(globalData))
        {
            string pointed = vars.RandomElement(globalData.GetActivePlayers().ToList(), _RND_SUSPECT_POINT);
            globalData.ActiveWindow.AddNextContent(1, false, content => Either(globalData, 2, 2, "DeathAndResolution")(content)
                                                                       .FormatWithReplacement(0, Suspect(globalData))
                                                                       .FormatWithReplacement(1, pointed));
        }

        globalData.ActiveWindow.AddNextContent(2);

        List<string> accused = globalData.GetActivePlayers().ToList();
        if (SuspectInPlay(globalData)) accused.Add(FearOfTheUnknownVars.SUSPECT);

        foreach (string player in accused)
        {
            globalData.ActiveWindow.AddCustomLink(Name(globalData, player), data =>
            {
                if (player == Witch(data))
                {
                    CursedRemoval(data);
                    return;
                }

                Vars(data).FalseWitch = player;
                CurseContinues(data);
            });
        }
    }

    // Originally Witchres2
    private static void CursedRemoval(GlobalData globalData)
    {
        string witch  = Witch(globalData);
        string author = RandomPlayer(globalData, nameof(Accusations));

        globalData.SaveToUndo();
        Vars(globalData).Cursed = 1;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, author));
        globalData.ActiveWindow.AddNextContent(witch == author ? 1 : witch == FearOfTheUnknownVars.SUSPECT ? 2 : 3, false,
            content => content.FormatWithReplacement(0, Name(globalData, witch)));
        globalData.ActiveWindow.AddClickHereToContinue(CursedRemoval_0);
    }

    private static void CursedRemoval_0(GlobalData globalData)
    {
        string witch = Witch(globalData);

        globalData.SaveToUndo();

        if (witch == FearOfTheUnknownVars.SUSPECT)
        {
            // Originally hexAI: the Apprentice hexes the rival of their trade or the Laborer's Union
            int location = Vars(globalData).RandomElement([0, 1], _RND_SUSPECT_LAST_HEX);
            globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S2_WitchHexToken, PopUpButton.Confirm, CaravanCheck, string.Empty);
            globalData.ActivePopup.ReplaceDescription(Tag(globalData, "CursedRemoval_0_Content1")
                                                     .FormatWithReplacement(0, Suspect(globalData))
                                                     .FormatWithIndex(1, location == 1 ? 2 : Vars(globalData).Victim - 1));
            return;
        }

        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S2_WitchHexToken, PopUpButton.Confirm, CaravanCheck, string.Empty);
        globalData.ActivePopup.ReplaceDescription(Tag(globalData, "CursedRemoval_0_Content2").FormatWithReplacement(0, witch));
    }

    // Originally Witchres3
    private static void CurseContinues(GlobalData globalData)
    {
        string accused = Vars(globalData).FalseWitch;
        string author  = RandomPlayer(globalData, nameof(Accusations));

        globalData.SaveToUndo();
        Vars(globalData).Cursed = 2;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, author));
        globalData.ActiveWindow.AddNextContent(accused == author ? 1 : accused == FearOfTheUnknownVars.SUSPECT ? 2 : 3, false,
            content => content.FormatWithReplacement(0, Name(globalData, accused)));
        globalData.ActiveWindow.AddClickHereToContinue(CurseContinues_0);
    }

    private static void CurseContinues_0(GlobalData globalData)
    {
        string accused = Vars(globalData).FalseWitch;

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.Self, PopUpButton.Confirm, CaravanCheck, string.Empty);
        globalData.ActivePopup.ReplaceDescription(accused == FearOfTheUnknownVars.SUSPECT
                                                      ? Tag(globalData, "CurseContinues_0_Content1").FormatWithReplacement(0, Suspect(globalData))
                                                      : Tag(globalData, "CurseContinues_0_Content2").FormatWithReplacement(0, accused));
    }

    #endregion End of the Middle Years - Accusations

    #region End of Generation II - Foreigners

    // Originally S4IsoBrick
    private static void KeyToTheCityAward(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Newspaper(globalData)));
        globalData.ActiveWindow.AddClickHereToContinue(KeyToTheCityAward_0);
    }

    private static void KeyToTheCityAward_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S2_BrickTokens, PopUpButton.Confirm, Gen2ForeignersEnd, content => content
                                                                                                                                                         .FormatWithReplacement(0, Vars(globalData).RandomInRange(2, 5, _RND_KEY_VP).ToString())
                                                                                                                                                         .FormatWithCondition(1, () => globalData.PlayersNum >= 4));
    }

    private static void Gen2ForeignersEnd(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.EndOfGeneration, PopUpIcon.MFWlogo, PopUpButton.Confirm,
            data => Vars(data).RandomElement([ConstructionMatters, ProtectedAgainstWitchcraft], _RND_GEN2_FOREIGN_FATE).Invoke(data),
            content => content.FormatWithCondition(0, () => !Vars(globalData).RevengeResolved));
    }

    // Originally S4Foreign1: enough bricks build the Memorial Wall
    private static void ConstructionMatters(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        globalData.SaveToUndo();
        int bricks = globalData.PlayersNum switch
        {
            2 => vars.RandomInRange(2, 5, _RND_WALL_BRICKS),
            3 => vars.RandomInRange(2, 6, _RND_WALL_BRICKS) + 2,
            _ => vars.RandomInRange(3, 6, _RND_WALL_BRICKS) + 3
        };

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Newspaper(globalData)).FormatWithReplacement(1, globalData.TownName));
        globalData.ActiveWindow.AddNextContentWithLinks(1, [IsolationIntro, TensionIntro], true, content => content.FormatWithReplacement(0, bricks.ToString()));
    }

    // Originally S4Foreign3: removing the witch ends the Wall project
    private static void ProtectedAgainstWitchcraft(GlobalData globalData)
    {
        bool removed = Vars(globalData).Cursed == 1;

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Newspaper(globalData)));
        globalData.ActiveWindow.AddNextContentWithLinks(removed ? 1 : 2, [removed ? TensionIntro : IsolationIntro], true, content => content
                                                                                                                                   .FormatWithReplacement(0, Name(globalData, Witch(globalData)))
                                                                                                                                   .FormatWithReplacement(1, globalData.TownName)
                                                                                                                                   .FormatWithReplacement(2, Newspaper(globalData)));
    }

    #endregion End of Generation II - Foreigners

    #region Caravans

    // Originally Caravancheck: after some rounds the Romani caravan may visit (players with a Market token can buy goods)
    private static void CaravanCheck(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);
        Action<GlobalData>   next = CaravanNext(globalData);

        int index = (vars.Path, globalData.Years) switch
        {
            (FearOfTheUnknownHubId.Foreigners, _)           => _RND_CARAVAN,
            (FearOfTheUnknownHubId.Isolation, Years.Middle) => _RND_CARAVAN + 1,
            (FearOfTheUnknownHubId.Isolation, _)            => _RND_CARAVAN + 2,
            (FearOfTheUnknownHubId.Tension, Years.Middle)   => _RND_CARAVAN + 3,
            _                                               => _RND_CARAVAN + 4
        };

        // 1 in 4 after the Foreigners' Middle Years, 1 in 3 in Generation III
        List<Action<GlobalData>> outcomes = vars.Path == FearOfTheUnknownHubId.Foreigners ? [next, next, next, Caravansery] : [next, next, Caravansery];
        vars.RandomElement(outcomes, index).Invoke(globalData);
    }

    // Where the story continues after the caravan check (Years is already the next round)
    private static Action<GlobalData> CaravanNext(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        return (vars.Path, globalData.Years) switch
        {
            (FearOfTheUnknownHubId.Foreigners, _)           => Foreigners,
            (FearOfTheUnknownHubId.Isolation, Years.Middle) => SmugglersOffer,
            (FearOfTheUnknownHubId.Isolation, _)            => IsolationLateSetup,
            (FearOfTheUnknownHubId.Tension, Years.Middle)   => TensionMiddleSetup,
            _                                               => Tension
        };
    }

    private static void Caravansery(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddCustomContent(JournalHeader(globalData, "Caravansery" + Vars(globalData).CaravanVisits));
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(Caravansery_0);
    }

    private static void Caravansery_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S2_MarketToken, PopUpButton.Confirm, data =>
        {
            Vars(data).CaravanVisits += 1;
            CaravanNext(data).Invoke(data);
        });
    }

    #endregion Caravans
}
