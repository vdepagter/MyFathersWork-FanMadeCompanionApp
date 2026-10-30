namespace MyFathersWorkWebApp;

public static partial class FearOfTheUnknown
{
    public static void Tension(GlobalData globalData)
    {
        // Round 13 -> Tension
        // Round 14 -> Tension2
        // Round 15 -> Tension3
        FearOfTheUnknownVars vars = Vars(globalData);

        bool early = globalData.Years == Years.Early;
        bool good  = vars.TensionGood == ExtendedBool.True;

        vars.HubId           = FearOfTheUnknownHubId.Tension;
        globalData.ActiveHub = new GameplayHub(globalData);
        globalData.ActiveHub.SetDefaultTitle(content => content.FormatWithIndex(0, early ? 0 : good ? 2 : 1));
        globalData.ActiveHub.SetSubtitle(globalData.Years);

        const string       specialRuleSection = "SpecialRule";
        GameplayHubSection specialRule        = globalData.ActiveHub.AddSection(specialRuleSection, true);
        specialRule.ReplaceShouldShow(() => globalData.Years == Years.Early && !vars.TenseResolved);
        specialRule.AddDefaultContent(specialRuleSection);

        const string       resolutionSection = "Resolution";
        GameplayHubSection resolution        = globalData.ActiveHub.AddSection(resolutionSection, true);
        resolution.ReplaceShouldShow(() => globalData.Years == Years.Early && !vars.TenseResolved);
        resolution.AddDefaultContent(resolutionSection);
        resolution.AddSpecialClickHere(resolutionSection, CalmBeforeTheTempest);

        const string       enemySection = "Enemy";
        GameplayHubSection enemy        = globalData.ActiveHub.AddSection(enemySection, true);
        enemy.ReplaceShouldShow(() => globalData.Years != Years.Early && vars.TensionGood == ExtendedBool.False);
        enemy.AddDefaultContent(enemySection);

        const string       crossroadsSection = "Crossroads";
        GameplayHubSection crossroads        = globalData.ActiveHub.AddSection(crossroadsSection, true);
        crossroads.ReplaceShouldShow(() => globalData.Years != Years.Early && vars.TensionGood == ExtendedBool.True
                                            && globalData.GetActivePlayers().Any(player => !vars.CrossroadVisited[player]));
        if (!early && good) crossroads.AddDefaultContent(crossroadsSection, content => content.FormatWithReplacement(0, CrossroadsSpace(globalData).ToString()));
        crossroads.AddSpecialClickHere(crossroadsSection, Crossroads);

        const string       plotExpansionSection = "PlotExpansion";
        GameplayHubSection plotExpansion        = globalData.ActiveHub.AddSection(plotExpansionSection);
        plotExpansion.ReplaceShouldShow(() => vars.Walls == ExtendedBool.False);
        plotExpansion.AddDefaultContent(plotExpansionSection);

        const string       occultSection = "Occult";
        GameplayHubSection occult        = globalData.ActiveHub.AddSection(occultSection);
        occult.AddDefaultContent(occultSection, content => content.FormatWithCondition(0, () => globalData.PlayersNum >= 4));

        const string       churchSection = "Church";
        GameplayHubSection church        = globalData.ActiveHub.AddSection(churchSection);
        church.AddSpecialClickHere(churchSection, Church);
        church.AddDefaultContent(churchSection);

        // Source showed this section when $tensebad == 0 and the penalty at the end when $tensebad > 1, the opposite of the
        // vote it belongs to: it applies after the family voted to steal (see WaysToEaseTension)
        const string       penaltySection = "Penalty";
        GameplayHubSection penalty        = globalData.ActiveHub.AddSection(penaltySection);
        penalty.ReplaceShouldShow(() => globalData.Years == Years.Late && vars.Stole);
        if (vars.Stole) penalty.AddDefaultContent(penaltySection, content => content.FormatWithReplacement(0, ObsessionVp(globalData).ToString()));

        GameplayHubSection nextRound = globalData.ActiveHub.AddSection(string.Empty);
        nextRound.ReplaceShouldShow(() => globalData.Years is Years.Early or Years.Middle);
        nextRound.AddClickHereContinueNextRound(Tension_0);

        globalData.ActiveHub.AddEndOfGenerationSection(Tension_0);
    }

    private static void Tension_0(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        globalData.ShowEndOfRoundPopUp(globalData.Years switch
        {
            Years.Early  => vars.TenseResolved ? Comeuppance : CalmBeforeTheTempestEnd,
            Years.Middle => vars.TensionGood == ExtendedBool.True ? CaravanCheck : WaysToEaseTension,
            Years.Late   => CabinetOfCuriosities,
            _            => _ => { }
        });
    }

    private static int CrossroadsSpace(GlobalData globalData) => Vars(globalData).RandomElement([7, 8, 9], _RND_CROSSROADS);

    private static int ObsessionVp(GlobalData globalData) => Vars(globalData).RandomElement([4, 5, 6], _RND_OBSESSION_VP);

    #region Intro

    private static void TensionIntro(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        globalData.SaveToUndo();
        vars.Path          = FearOfTheUnknownHubId.Tension;
        vars.TenseResolved = false;
        vars.TensionGood   = ExtendedBool.None;
        vars.Stole         = false;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.SetVoiceOver("TensionIntro");
        globalData.ActiveWindow.AddGameplayTitle(GlobalTags.Gameplay_Generation_III);
        globalData.ActiveWindow.AddDefaultSubtitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, RandomPlayer(globalData)));
        globalData.ActiveWindow.AddClickHereToContinue(TwoSides);
    }

    // Originally TensionSetting
    private static void TwoSides(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddCustomContent(JournalHeader(globalData));
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, globalData.TownName));
        globalData.ActiveWindow.AddClickHereToContinue(TwoSides_0);
    }

    private static void TwoSides_0(GlobalData globalData)
    {
        globalData.SaveToUndo();

        // Source: WallCheck recounts the bricks only when Gen2fate == "witch", which is never set - skipped as in the official app
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S2_RomaniToken, PopUpButton.Confirm, MinorReward);
    }

    // Originally TenseBricksRes: the family supported Acceptance
    private static void MinorReward(GlobalData globalData)
    {
        if (Vars(globalData).Walls != ExtendedBool.False)
        {
            Reconciliation(globalData);
            return;
        }

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddCustomContent(LetterHeader(globalData));
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(MinorReward_0);
    }

    // Source shows the plot expansion rule under a bold SPECIAL RULE line in the letter; it is a popup here
    private static void MinorReward_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialRule, PopUpIcon.S2_BrickTokens, PopUpButton.Confirm, Reconciliation);
    }

    // Originally WitchTensionResolution: the heir of an executed witch
    private static void Reconciliation(GlobalData globalData)
    {
        FearOfTheUnknownVars vars  = Vars(globalData);
        string               witch = Witch(globalData);

        if (vars.Cursed != 1 || witch == FearOfTheUnknownVars.SUSPECT)
        {
            InSupportOfOthers(globalData);
            return;
        }

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, witch).FormatWithCondition(1, () => vars.FamilyPlot[witch]));
        globalData.ActiveWindow.AddClickHereToContinue(Reconciliation_0);
    }

    private static void Reconciliation_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.GainServantFromLost, PopUpButton.Confirm, InSupportOfOthers,
            content => content.FormatWithReplacement(0, Witch(globalData)));
    }

    // Originally Creepy1stPlayerB
    private static void InSupportOfOthers(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(InSupportOfOthers_0);
    }

    private static void InSupportOfOthers_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.Creepy_Icon, PopUpButton.Confirm, ObsessionsResolve);
    }

    private static void TensionSetup(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S2_EstateUpgradeBACK, PopUpButton.Confirm, Tension, content => content
                                                                                                                                                         .FormatWithCondition(0, () => Vars(globalData).Walls == ExtendedBool.True)
                                                                                                                                                         .FormatWithCondition(1, () => globalData.PlayersNum == 3));
    }

    private static void TensionMiddleSetup(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.VillageChronicleCover, PopUpButton.Confirm, Tension,
            content => content.FormatWithCondition(0, () => Vars(globalData).TensionGood == ExtendedBool.True));
    }

    #endregion Intro

    #region The Special Event

    // Originally TenseConfirm, reached when the Angry Mob overtakes the Romani token during the Early Years
    private static void CalmBeforeTheTempest(GlobalData globalData)
    {
        globalData.SaveToUndo();
        Vars(globalData).TenseResolved = true;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddCustomContent(LetterHeader(globalData));
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(CalmBeforeTheTempest_0);
    }

    private static void CalmBeforeTheTempest_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S2_RomaniToken, PopUpButton.Confirm, CalmBeforeTheTempest_1);
    }

    // Source shows this rule under a bold SPECIAL RULE line in the setup above; it is its own popup here
    private static void CalmBeforeTheTempest_1(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialRule, PopUpIcon.AngryMob_Icon, PopUpButton.Confirm, Tension);
    }

    // Originally TenseConfirm at the end of the Early Years without an overtake. Source only reached its "move the Angry Mob
    // to the Romani token" branch after a second click on the end of round link; it is shown directly here.
    private static void CalmBeforeTheTempestEnd(GlobalData globalData)
    {
        globalData.SaveToUndo();
        Vars(globalData).TenseResolved = true;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultBaseTitle();
        globalData.ActiveWindow.AddCustomContent(LetterHeader(globalData, nameof(CalmBeforeTheTempest)));
        globalData.ActiveWindow.AddStoryContent("CalmBeforeTheTempest_Content");
        globalData.ActiveWindow.AddClickHereToContinue(CalmBeforeTheTempestEnd_0);
    }

    private static void CalmBeforeTheTempestEnd_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.AngryMob_Icon, PopUpButton.Confirm, Comeuppance);
    }

    // Originally ConsequencesTense: the family's choices through the generations decide the fate of the town
    private static void Comeuppance(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        bool butcher  = vars.Victim == _VICTIM_BUTCHER;
        bool cursed   = vars.Cursed == 2;
        bool strength = vars.Walls == ExtendedBool.True;
        int  bad      = (butcher ? 1 : 0) + (cursed ? 1 : 0) + (strength ? 1 : 0);

        globalData.SaveToUndo();
        vars.TensionGood = bad > 3 - bad ? ExtendedBool.False : ExtendedBool.True;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddCustomContent(JournalHeader(globalData));
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, globalData.TownName));
        globalData.ActiveWindow.AddNextContent(butcher ? 1 : 2);
        globalData.ActiveWindow.AddNextContent(cursed ? 3 : 4, true);
        globalData.ActiveWindow.AddNextContent(strength ? 5 : 6, true);
        globalData.ActiveWindow.AddNextContent(7, true);
        globalData.ActiveWindow.AddClickHereToContinue(vars.TensionGood == ExtendedBool.True ? GoodNeighbors : ComeToAnAgreement);
    }

    // Originally ThisTenseGood
    private static void GoodNeighbors(GlobalData globalData)
    {
        globalData.SaveToUndo();
        Vars(globalData).Ending = 3;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, RandomPlayer(globalData)));
        globalData.ActiveWindow.AddClickHereToContinue(GoodNeighbors_0);
    }

    private static void GoodNeighbors_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.AngryMob_Icon, PopUpButton.Confirm, TheCrossroads);
    }

    // Originally TenseGoodEvent1
    private static void TheCrossroads(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, RandomPlayer(globalData, nameof(GoodNeighbors))));
        globalData.ActiveWindow.AddClickHereToContinue(TheCrossroads_0);
    }

    private static void TheCrossroads_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.StorybookToken, PopUpButton.Confirm, CaravanCheck,
            content => content.FormatWithReplacement(0, CrossroadsSpace(globalData).ToString()));
    }

    // Originally ThistenseBad
    private static void ComeToAnAgreement(GlobalData globalData)
    {
        globalData.SaveToUndo();
        Vars(globalData).Ending = 4;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, RandomPlayer(globalData)));
        globalData.ActiveWindow.AddClickHereToContinue(ComeToAnAgreement_0);
    }

    private static void ComeToAnAgreement_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S2_RomaniToken, PopUpButton.Confirm, CaravanCheck);
    }

    #endregion The Special Event

    #region End of the Middle Years - Stealing Resources

    // Originally TenseEventBad2
    private static void WaysToEaseTension(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddCustomContent(LetterHeader(globalData));
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(globalData.PlayersNum == 2 ? StealBid : StealVote);
    }

    // Originally 2PTenseEvent1
    private static void StealBid(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.SetCue(SoundCue.SpecialEvent);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, ObsessionVp(globalData).ToString()));
        globalData.ActiveWindow.AddClickHereAfterBid(StealBidResolved);
    }

    // Originally 2PTenseEvent2
    private static void StealBidResolved(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [GoodsProcured, EthicsPrevail], true);
    }

    // Originally TenseEventBad2b
    private static void StealVote(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.SetCue(SoundCue.SpecialEvent);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, ObsessionVp(globalData).ToString()));
        globalData.ActiveWindow.AddClickHereAfterVote(StealVoteResolved);
    }

    // Originally TenseEventBad2Res
    private static void StealVoteResolved(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [GoodsProcured, EthicsPrevail], true);
    }

    // Originally TenseEventBad2Yes
    private static void GoodsProcured(GlobalData globalData)
    {
        globalData.SaveToUndo();
        Vars(globalData).Stole = true;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, RandomPlayer(globalData, nameof(WaysToEaseTension))));
        globalData.ActiveWindow.AddClickHereToContinue(GoodsProcured_0);
    }

    private static void GoodsProcured_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.MFWlogo, PopUpButton.Confirm, CaravanCheck,
            content => content.FormatWithCondition(0, () => globalData.PlayersNum == 2).FormatWithCondition(1, () => globalData.PlayersNum >= 3));
    }

    // Originally TenseEventBad2No
    private static void EthicsPrevail(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content
                                                             .FormatWithReplacement(0, RandomPlayer(globalData, nameof(WaysToEaseTension)))
                                                             .FormatWithReplacement(1, globalData.TownName));
        globalData.ActiveWindow.AddClickHereToContinue(EthicsPrevail_0);
    }

    private static void EthicsPrevail_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.AngryMob_Icon, PopUpButton.Confirm, CaravanCheck,
            content => content.FormatWithReplacement(0, (globalData.PlayersNum + 1).ToString()).FormatWithCondition(1, () => globalData.PlayersNum >= 3));
    }

    #endregion End of the Middle Years - Stealing Resources

    #region End of Generation III - Tension

    // Originally TensionBonus
    private static void CabinetOfCuriosities(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithCondition(0, () => globalData.PlayersNum >= 4));
        globalData.ActiveWindow.AddNextContentWithLinks(1, [Neglected], true);
    }

    // Originally TensionCaravanResolve (shown even when the caravan visited, as in the source)
    private static void Neglected(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Newspaper(globalData)).FormatWithReplacement(1, globalData.TownName));
        globalData.ActiveWindow.AddClickHereToContinue(Neglected_0);
    }

    private static void Neglected_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S2_MarketToken, PopUpButton.Confirm, ObsessionResolution);
    }

    #endregion End of Generation III - Tension
}
