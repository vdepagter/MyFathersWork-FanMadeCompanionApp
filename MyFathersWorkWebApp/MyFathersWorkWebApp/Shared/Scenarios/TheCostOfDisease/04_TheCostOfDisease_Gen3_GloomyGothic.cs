namespace MyFathersWorkWebApp;

public static partial class TheCostOfDisease
{
    private const int _RND_HUNT_VP            = 59;
    private const int _RND_CHARITY_PENALTY    = 60;
    private const int _RND_TIERED_REWARDS_1   = 61; // 61 - 63
    private const int _RND_HUNT_NUMBER        = 64;
    private const int _RND_CONFRONTATION_FAIL = 65;
    private const int _RND_TAXES_NEXT         = 66;
    private const int _RND_TIERED_REWARDS_2   = 67; // 67 - 69

    public static void GloomyGothic(GlobalData globalData)
    {
        // Round 10 -> GloomyGothic1
        // Round 11 -> GloomyGothic2
        // Round 12 -> GloomyGothic3
        TheCostOfDiseaseVars vars = globalData.TheCostOfDiseaseVars;

        vars.HubId           = CostOfDiseaseHubId.GloomyGothic;
        globalData.ActiveHub = new GameplayHub(globalData);
        globalData.ActiveHub.SetDefaultTitle();
        globalData.ActiveHub.SetSubtitle(globalData.Years);

        bool Hunters() => vars.Society == Society.FraternityOfHunters;
        bool HuntersSafe() => Hunters() && ((globalData.Years == Years.Middle && vars.Confront != ExtendedBool.True) || (globalData.Years == Years.Late && vars.Taxes != ExtendedBool.False));
        bool HuntersTaxation() => Hunters() && (globalData.Years == Years.Early || (globalData.Years == Years.Middle && vars.Confront != ExtendedBool.True));
        bool OrderSpawn() => !Hunters() && !(globalData.Years == Years.Late && vars.VialUse == ExtendedBool.True);

        // Fraternity of Hunters
        const string       safeSection = "Safe";
        GameplayHubSection safe        = globalData.ActiveHub.AddSection(safeSection, true);
        safe.ReplaceShouldShow(HuntersSafe);
        safe.AddDefaultContent(safeSection, content => content.FormatWithCondition(0, () => globalData.Years == Years.Middle));

        const string       huntersRestSection = "HuntersRest";
        GameplayHubSection huntersRest        = globalData.ActiveHub.AddSection(huntersRestSection, true);
        huntersRest.ReplaceShouldShow(() => HuntersTaxation() || HuntersSafe());
        huntersRest.AddDefaultContent(huntersRestSection);

        const string       taxationSection = "Taxation";
        GameplayHubSection taxation        = globalData.ActiveHub.AddSection(taxationSection);
        taxation.ReplaceShouldShow(HuntersTaxation);
        taxation.AddDefaultContent(taxationSection);

        const string       finalTaxationSection = "FinalTaxation";
        GameplayHubSection finalTaxation        = globalData.ActiveHub.AddSection(finalTaxationSection);
        finalTaxation.ReplaceShouldShow(() => Hunters() && globalData.Years == Years.Late && vars.Taxes != ExtendedBool.False);
        finalTaxation.AddDefaultContent(finalTaxationSection);

        const string       huntIsOnSection = "HuntIsOn";
        GameplayHubSection huntIsOn        = globalData.ActiveHub.AddSection(huntIsOnSection, true);
        huntIsOn.ReplaceShouldShow(() => Hunters() && globalData.Years == Years.Middle && vars.Confront == ExtendedBool.True);
        huntIsOn.AddDefaultContent(huntIsOnSection, content => content.FormatWithReplacement(0, vars.HuntVp.ToString()));

        const string       huntersHavenSection = "HuntersHaven";
        GameplayHubSection huntersHaven        = globalData.ActiveHub.AddSection(huntersHavenSection, true);
        huntersHaven.ReplaceShouldShow(() => Hunters() && globalData.Years == Years.Late && vars.Taxes == ExtendedBool.False);
        huntersHaven.AddDefaultContent(huntersHavenSection);

        // Order of St. Hubertus
        const string       masterworkSection = "Masterwork";
        GameplayHubSection masterwork        = globalData.ActiveHub.AddSection(masterworkSection, true);
        masterwork.ReplaceShouldShow(OrderSpawn);
        masterwork.AddDefaultContent(masterworkSection);
        masterwork.AddSpecialClickHere(masterworkSection, MWTokenResolve);

        const string       monsterSpawnSection = "MonsterSpawn";
        GameplayHubSection monsterSpawn        = globalData.ActiveHub.AddSection(monsterSpawnSection);
        monsterSpawn.ReplaceShouldShow(OrderSpawn);
        monsterSpawn.AddDefaultContent(monsterSpawnSection);

        const string       schoolSection = "School";
        GameplayHubSection school        = globalData.ActiveHub.AddSection(schoolSection, true);
        school.ReplaceShouldShow(() => !Hunters() && globalData.Years == Years.Late && vars.VialUse == ExtendedBool.True);
        school.AddDefaultContent(schoolSection);

        const string       giftOfSpawningSection = "GiftOfSpawning";
        GameplayHubSection giftOfSpawning        = globalData.ActiveHub.AddSection(giftOfSpawningSection);
        giftOfSpawning.ReplaceShouldShow(() => !Hunters());
        giftOfSpawning.AddDefaultContent(giftOfSpawningSection);

        GameplayHubSection nextRound = globalData.ActiveHub.AddSection(string.Empty);
        nextRound.ReplaceShouldShow(() => globalData.Years is Years.Early or Years.Middle);
        nextRound.AddClickHereContinueNextRound(GloomyGothic_0);

        globalData.ActiveHub.AddEndOfGenerationSection(GloomyGothic_0);
    }

    private static void GloomyGothic_0(GlobalData globalData)
    {
        TheCostOfDiseaseVars vars    = globalData.TheCostOfDiseaseVars;
        bool                 hunters = vars.Society == Society.FraternityOfHunters;

        if (globalData.Years == Years.Late)
        {
            vars.Ending = hunters
                              ? vars.Taxes == ExtendedBool.False ? CostOfDiseaseEnding.HuntersEvil1 : CostOfDiseaseEnding.HuntersEvil2
                              : vars.VialUse == ExtendedBool.True ? CostOfDiseaseEnding.WolvesEvil1 : CostOfDiseaseEnding.WolvesEvil2;
        }

        globalData.ShowEndOfRoundPopUp(globalData.Years switch
        {
            Years.Early  => hunters ? GloomyPenalty1 : WolvesEvil1,
            Years.Middle => hunters ? vars.Confront == ExtendedBool.True ? HunterConfrontation : TaxesEventNoConfrontation : WolvesVote,
            Years.Late   => hunters ? vars.Taxes == ExtendedBool.False ? Scoring : GloomyPenalty3 : AwardSpawningPods,
            _            => _ => { }
        });
    }

    #region Setup

    private static string BuildingTilesText(GlobalData globalData, string tag, Func<int, bool> placeBuilding)
    {
        string content = string.Empty;

        for (int x = 0; x < globalData.TheCostOfDiseaseVars.Gen2Buildings.Length; ++x)
        {
            if (!placeBuilding(globalData.TheCostOfDiseaseVars.BuildingsExposeValue[x])) continue;
            content += globalData.GetScenarioLocalizedTag(tag)
                                 .FormatWithIndex(0, (int)globalData.TheCostOfDiseaseVars.Gen2Buildings[x])
                                 .FormatWithIndex(1, x);
        }

        return content;
    }

    private static void GloomyGothicSetup(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.TheCostOfDiseaseVars.Gen3SetupShown = true;
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.StartPlayerToken, PopUpButton.Confirm, GloomyGothic, string.Empty);

        // Source compares the building expose value with <= 1 in Generation III (unlike > 0 at the end of Generation II)
        string content = globalData.GetScenarioLocalizedTag("GloomyGothicSetup_Content")
                                   .FormatWithCondition(0, () => globalData.TheCostOfDiseaseVars.Society == Society.FraternityOfHunters)
                                   .FormatWithCondition(1, () => globalData.PlayersNum == 3);
        content += BuildingTilesText(globalData, "GloomyGothicSetup_Content1", value => value <= 1);
        content += globalData.GetScenarioLocalizedTag("GloomyGothicSetup_Content2");
        globalData.ActivePopup.ReplaceDescription(content);
    }

    private static void GloomyGothicLate(GlobalData globalData)
    {
        TheCostOfDiseaseVars vars = globalData.TheCostOfDiseaseVars;

        bool huntersDefeated = vars.Society == Society.FraternityOfHunters && vars.Taxes == ExtendedBool.False;
        bool orderCleansed   = vars.Society == Society.OrderOfStHubertus && vars.VialUse == ExtendedBool.True;

        if (huntersDefeated || orderCleansed) GloomyGothicLateSetup(globalData);
        else GloomyGothic(globalData);
    }

    private static void GloomyGothicLateSetup(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S1_Suspicious_Building, PopUpButton.Confirm, GloomyGothic, string.Empty);

        string content = globalData.GetScenarioLocalizedTag("GloomyGothicLateSetup_Content")
                                   .FormatWithCondition(0, () => globalData.TheCostOfDiseaseVars.Society == Society.FraternityOfHunters);
        content += BuildingTilesText(globalData, "GloomyGothicLateSetup_Content1", value => value <= 1);
        globalData.ActivePopup.ReplaceDescription(content);
    }

    #endregion Setup

    #region Intro

    private static void GloomyHunterIntro(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.TheCostOfDiseaseVars.HuntVp = globalData.TheCostOfDiseaseVars.RandomElement([4, 5], _RND_HUNT_VP);

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddGameplayTitle(GlobalTags.Gameplay_Generation_III);
        globalData.ActiveWindow.AddDefaultSubtitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(EvilConsequences);
    }

    private static void GloomyWolvesIntro(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddGameplayTitle(GlobalTags.Gameplay_Generation_III);
        globalData.ActiveWindow.AddDefaultSubtitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(EvilConsequences);
    }

    private static Faction SocietyFaction(GlobalData globalData)
    {
        return globalData.TheCostOfDiseaseVars.Society == Society.FraternityOfHunters ? Faction.Hunters : Faction.Wolves;
    }

    private static void EvilConsequences(GlobalData globalData)
    {
        Faction faction    = SocietyFaction(globalData);
        bool    allAllied  = globalData.GetActivePlayers().All(player => globalData.TheCostOfDiseaseVars.Ally[player] == faction);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithCondition(0, () => faction == Faction.Hunters));
        globalData.ActiveWindow.AddClickHereToContinue(allAllied ? EvilMayor : EvilConsequences_0);
    }

    private static void EvilConsequences_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.Creepy_Icon, PopUpButton.Confirm, EvilMayor, string.Empty);

        Faction faction = SocietyFaction(globalData);
        string  content = string.Empty;

        foreach (string player in globalData.GetActivePlayers())
        {
            if (globalData.TheCostOfDiseaseVars.Ally[player] == faction) continue;
            content += globalData.GetScenarioLocalizedTag("EvilConsequences_0_Content1").FormatWithReplacement(0, player);
        }

        globalData.ActivePopup.ReplaceDescription(content);
    }

    private static void EvilMayor(GlobalData globalData)
    {
        if (globalData.TheCostOfDiseaseVars.Building != BankOrLibrary.Bank)
        {
            MayorLibraryEvil(globalData);
            return;
        }

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddHandStorybookToNoJump(globalData.TheCostOfDiseaseVars.Mayor + " III");
        globalData.ActiveWindow.AddDefaultContent(content => content
                                                            .FormatWithReplacement(0, globalData.TheCostOfDiseaseVars.Mayor)
                                                            .FormatWithIndex(1, (int)globalData.TheCostOfDiseaseVars.Society));
        globalData.ActiveWindow.AddClickHereToContinue(EvilMayor_0);
    }

    private static void EvilMayor_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S1_MayorCoin, PopUpButton.Confirm, EvilMayorNext);
    }

    private static void MayorLibraryEvil(GlobalData globalData)
    {
        if (globalData.TheCostOfDiseaseVars.Building != BankOrLibrary.Library)
        {
            EvilMayorNext(globalData);
            return;
        }

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddHandStorybookToNoJump(globalData.TheCostOfDiseaseVars.Mayor + " III");
        globalData.ActiveWindow.AddDefaultContent(content => content
                                                            .FormatWithReplacement(0, globalData.TheCostOfDiseaseVars.Mayor)
                                                            .FormatWithReplacement(1, globalData.TownName)
                                                            .FormatWithCondition(2, () => globalData.TheCostOfDiseaseVars.Society == Society.FraternityOfHunters));
        globalData.ActiveWindow.AddClickHereToContinue(MayorLibraryEvil_0);
    }

    private static void MayorLibraryEvil_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S1_MayorCoin, PopUpButton.Confirm, EvilMayorNext,
            content => content.FormatWithReplacement(0, globalData.TheCostOfDiseaseVars.Mayor));
    }

    private static void EvilMayorNext(GlobalData globalData)
    {
        if (globalData.TheCostOfDiseaseVars.HCount == 0 && globalData.TheCostOfDiseaseVars.Society == Society.OrderOfStHubertus) EvilWolvesEventStart(globalData);
        else LosingOrderAid(globalData);
    }

    private static void LosingOrderAid(GlobalData globalData)
    {
        TheCostOfDiseaseVars vars = globalData.TheCostOfDiseaseVars;

        if (vars.Society == Society.OrderOfStHubertus)
        {
            if (vars.HCount == 0) EvilWolvesEventStart(globalData);
            else LosingOrderAidHunters(globalData);
        }
        else
        {
            if (vars.WCount == 0) TaxesEventStart(globalData);
            else LosingOrderAidWolves(globalData);
        }
    }

    private static bool AnyAlly(GlobalData globalData, Faction faction)
    {
        return globalData.GetActivePlayers().Any(player => globalData.TheCostOfDiseaseVars.Ally[player] == faction);
    }

    private static string AlliesList(GlobalData globalData, Faction faction, string tag)
    {
        string content = string.Empty;

        foreach (string player in globalData.GetActivePlayers())
        {
            if (globalData.TheCostOfDiseaseVars.Ally[player] != faction) continue;
            content += globalData.GetScenarioLocalizedTag(tag).FormatWithReplacement(0, player);
        }

        return content;
    }

    private static void LosingOrderAidHunters(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(AnyAlly(globalData, Faction.Hunters) ? LosingOrderAidHunters_0 : EvilWolvesEventStart);
    }

    private static void LosingOrderAidHunters_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.GainServantFromLost, PopUpButton.Confirm, EvilWolvesEventStart, string.Empty);
        globalData.ActivePopup.ReplaceDescription(AlliesList(globalData, Faction.Hunters, "LosingOrderAid_0_Content1"));
    }

    private static void LosingOrderAidWolves(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(AnyAlly(globalData, Faction.Wolves) ? LosingOrderAidWolves_0 : TaxesEventStart);
    }

    private static void LosingOrderAidWolves_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.GainServantFromLost, PopUpButton.Confirm, TaxesEventStart, string.Empty);
        globalData.ActivePopup.ReplaceDescription(AlliesList(globalData, Faction.Wolves, "LosingOrderAid_0_Content1"));
    }

    private static void EvilWolvesEventStart(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(VialCharity);
    }

    private static void VialCharity(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDoNotOtherPlayersToSee(globalData.TheCostOfDiseaseVars.Charity, VialCharity_0);
    }

    private static void VialCharity_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, globalData.TheCostOfDiseaseVars.Charity));
        globalData.ActiveWindow.AddClickHereToContinue(VialCharity_1);
    }

    private static void VialCharity_1(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S1_VialToken, PopUpButton.Confirm, LycanthropicMessage);
    }

    private static void LycanthropicMessage(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [LycanthropicMessage_Yes, WolvesSetupGen3]);
    }

    private static void LycanthropicMessage_Yes(GlobalData globalData)
    {
        globalData.TheCostOfDiseaseVars.Lycan = true;
        LycanEvil(globalData);
    }

    private static void LycanEvil(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(LycanEvil_0);
    }

    private static void LycanEvil_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S1_MWUpdateLycanthropic, PopUpButton.Confirm, WolvesSetupGen3);
    }

    private static void WolvesSetupGen3(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.StorybookToken, PopUpButton.Confirm, GloomyGothicSetup);
    }

    private static void TaxesEventStart(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(CharityNegCons);
    }

    private static void CharityNegCons(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.TheCostOfDiseaseVars.CharityPenalty = globalData.TheCostOfDiseaseVars.RandomElement([1, 2], _RND_CHARITY_PENALTY);

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, globalData.TheCostOfDiseaseVars.Charity));
        globalData.ActiveWindow.AddClickHereToContinue(CharityNegCons_0);
    }

    private static void CharityNegCons_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S1_HeartToken, PopUpButton.Confirm, GloomyGothicSetup,
            content => content
                      .FormatWithReplacement(0, globalData.TheCostOfDiseaseVars.Charity)
                      .FormatWithCondition(1, () => globalData.TheCostOfDiseaseVars.CharityPenalty == 1));
    }

    #endregion Intro

    #region Fraternity of Hunters Events

    private static void GloomyPenalty1(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(GloomyPenalty1_0);
    }

    private static void GloomyPenalty1_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S1_HunterToken, PopUpButton.Confirm, Preposterous);
    }

    private static void Preposterous(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithIndex(0, globalData.LocalizedPlayerNumberIndex()));
        globalData.ActiveWindow.AddClickHereToContinue(globalData.PlayersNum == 2 ? EvilHunter1Event2_Yes : EvilHunter1Event);
    }

    private static void EvilHunter1Event(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, globalData.TownName));
        globalData.ActiveWindow.AddNextContentWithLinks(1, [EvilHunter1Event2]);
    }

    private static void EvilHunter1Event2(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [EvilHunter1Event2_Yes, EvilHunter1Event2_No]);
    }

    private static void EvilHunter1Event2_Yes(GlobalData globalData)
    {
        globalData.TheCostOfDiseaseVars.Confront = ExtendedBool.True;
        EvilHunter1EventYes(globalData);
    }

    private static void EvilHunter1Event2_No(GlobalData globalData)
    {
        globalData.TheCostOfDiseaseVars.Confront = ExtendedBool.False;
        GloomyGothic(globalData);
    }

    private static void EvilHunter1EventYes(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(GloomyGothic);
    }

    private static void HunterConfrontation(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, globalData.TownName));
        globalData.ActiveWindow.AddClickHereToContinue(CannotParticipate);
    }

    private static void CannotParticipate(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(CannotParticipate_0);
    }

    private static void CannotParticipate_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S1_DiseaseExperiment, PopUpButton.Confirm, HunterConf2);
    }

    private static void HunterConf2(GlobalData globalData)
    {
        globalData.SaveToUndo();

        TheCostOfDiseaseVars vars = globalData.TheCostOfDiseaseVars;
        vars.HuntNumber = globalData.PlayersNum switch
        {
            2 => vars.RandomInRange(3, 7, _RND_HUNT_NUMBER),
            3 => vars.RandomInRange(8, 12, _RND_HUNT_NUMBER),
            _ => vars.RandomInRange(11, 15, _RND_HUNT_NUMBER) // 4
        };

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, vars.HuntNumber.ToString()));
        globalData.ActiveWindow.AddNextContentWithLinks(1, [HunterConf3]);
    }

    private static void HunterConf3(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [OhYesTheyDead, ConfrontationFail], false,
            content => content.FormatWithReplacement(0, globalData.TheCostOfDiseaseVars.HuntNumber.ToString()));
    }

    private static void ConfrontationFail(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.TheCostOfDiseaseVars.Taxes = ExtendedBool.True;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, globalData.TheCostOfDiseaseVars.HuntVp.ToString()));
        globalData.ActiveWindow.AddClickHereToContinue(ConfrontationFail_0);
    }

    private static void ConfrontationFail_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.AngryMob_Icon, PopUpButton.Confirm, GloomyGothicLate,
            content => content.FormatWithIndex(0, globalData.TheCostOfDiseaseVars.RandomElement([0, 1], _RND_CONFRONTATION_FAIL)));
    }

    private static void OhYesTheyDead(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.TheCostOfDiseaseVars.Taxes = ExtendedBool.False;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, globalData.TownName));
        globalData.ActiveWindow.AddClickHereToContinue(OhYesTheyDead_0);
    }

    private static void OhYesTheyDead_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.ScoreTrackMarker, PopUpButton.Confirm, GloomyGothicLate,
            content => content.FormatWithReplacement(0, globalData.TheCostOfDiseaseVars.HuntVp.ToString()));
    }

    private static void TaxesEventNoConfrontation(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(TaxesEventNoConfrontation_0);
    }

    private static void TaxesEventNoConfrontation_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S1_HunterToken, PopUpButton.Confirm,
            globalData.TheCostOfDiseaseVars.RandomElement([TaxesEventNoConfrontation2, GloomyGothicLate], _RND_TAXES_NEXT));
    }

    private static void TaxesEventNoConfrontation2(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, globalData.TownName));
        globalData.ActiveWindow.AddClickHereToContinue(TaxesEventNoConfrontation2_0);
    }

    private static void TaxesEventNoConfrontation2_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.Creepy_Icon, PopUpButton.Confirm, GloomyGothicLate);
    }

    private static void GloomyPenalty3(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(GloomyPenalty3_0);
    }

    private static void GloomyPenalty3_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S1_HunterToken, PopUpButton.Confirm, Scoring);
    }

    #endregion Fraternity of Hunters Events

    #region Order of St. Hubertus Events

    private static void MWTokenResolve(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(MWTokenResolve_0);
    }

    private static void MWTokenResolve_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.ScoreTrackMarker, PopUpButton.Confirm, GloomyGothic);
    }

    private static string TieredRewardsText(GlobalData globalData, string tag, int randomIndex)
    {
        TheCostOfDiseaseVars vars = globalData.TheCostOfDiseaseVars;
        return globalData.GetScenarioLocalizedTag(tag)
                         .FormatWithIndex(0, vars.RandomElement([0, 1, 2, 3], randomIndex))
                         .FormatWithIndex(1, vars.RandomElement([0, 1, 2],    randomIndex + 1))
                         .FormatWithIndex(2, vars.RandomElement([0, 1],       randomIndex + 2));
    }

    private static void WolvesEvil1(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(TieredRewards1);
    }

    private static void TieredRewards1(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(TieredRewards1_0);
    }

    private static void TieredRewards1_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S1_WolfToken, PopUpButton.Confirm, GloomyGothic, string.Empty);
        globalData.ActivePopup.ReplaceDescription(TieredRewardsText(globalData, "TieredRewards1_0_Content", _RND_TIERED_REWARDS_1));
    }

    private static void WolvesVote(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [WolvesVote2]);
    }

    private static void WolvesVote2(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [WolvesVoteCheck, TheVialUse]);
    }

    private static void WolvesVoteCheck(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [WolvesVoteChange, TheVialUse]);
    }

    private static void WolvesVoteChange(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.TheCostOfDiseaseVars.VialUse = ExtendedBool.True;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, globalData.TownName));
        globalData.ActiveWindow.AddClickHereToContinue(VialSoldforLess);
    }

    private static void VialSoldforLess(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(VialSoldforLess_0);
    }

    private static void VialSoldforLess_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.Money_Icon, PopUpButton.Confirm, GloomyGothicLate,
            content => content.FormatWithReplacement(0, globalData.TheCostOfDiseaseVars.Charity));
    }

    private static void TheVialUse(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, globalData.TheCostOfDiseaseVars.Charity));
        globalData.ActiveWindow.AddNextContentWithLinks(1, [VialChanged, VialSold]);
    }

    private static void VialChanged(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.TheCostOfDiseaseVars.VialUse = ExtendedBool.True;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, globalData.TownName));
        globalData.ActiveWindow.AddClickHereToContinue(VialChanged_0);
    }

    private static void VialChanged_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S1_VialToken, PopUpButton.Confirm, GloomyGothicLate,
            content => content.FormatWithReplacement(0, globalData.TheCostOfDiseaseVars.Charity));
    }

    private static void VialSold(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.TheCostOfDiseaseVars.VialUse = ExtendedBool.False;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, globalData.TheCostOfDiseaseVars.Charity));
        globalData.ActiveWindow.AddClickHereToContinue(VialSold_0);
    }

    private static void VialSold_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.Money_Icon, PopUpButton.Confirm, WolvesEvil2,
            content => content.FormatWithReplacement(0, globalData.TheCostOfDiseaseVars.Charity));
    }

    private static void WolvesEvil2(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(TieredRewards2);
    }

    private static void TieredRewards2(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.Creepy_Icon, PopUpButton.Confirm, GloomyGothicLate, string.Empty);
        globalData.ActivePopup.ReplaceDescription(TieredRewardsText(globalData, "TieredRewards2_Content", _RND_TIERED_REWARDS_2));
    }

    private static void AwardSpawningPods(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithCondition(0, () => globalData.TheCostOfDiseaseVars.VialUse == ExtendedBool.True));
        globalData.ActiveWindow.AddClickHereToContinue(AwardSpawningPods_0);
    }

    private static void AwardSpawningPods_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.MFWlogo, PopUpButton.Confirm, Scoring);
    }

    #endregion Order of St. Hubertus Events
}
