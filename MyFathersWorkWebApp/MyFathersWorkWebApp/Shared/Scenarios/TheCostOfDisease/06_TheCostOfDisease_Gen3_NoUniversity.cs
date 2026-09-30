namespace MyFathersWorkWebApp;

public static partial class TheCostOfDisease
{
    private const int _RND_MASTERWORK_COST      = 131; // 131 - 134 (one per player)
    private const int _RND_MASTERWORK_ADJECTIVE = 135; // 135 - 138 (one per player)
    private const int _RND_BAR_INTRO            = 140; // 140 - 149
    private const int _RND_BAR                  = 150; // 150 - 159
    private const int _RND_BAR_VARIANT          = 160; // 160 - 169
    private const int _RND_BAR_CASE_A           = 170; // 170 - 179
    private const int _RND_BAR_CASE_B           = 180; // 180 - 189
    private const int _RND_BAR_INTRO_VALUE      = 190; // 190 - 199
    private const int _RND_MASTERWORK_COMPLETE  = 200; // 200 - 203 (one per player)
    private const int _BAR_RANDOM_SIZE          = 10;

    public static void NoUniversity(GlobalData globalData)
    {
        // Round 16 -> NoUni1
        // Round 17 -> NoUni2
        // Round 18 -> NoUni3
        TheCostOfDiseaseVars vars = globalData.TheCostOfDiseaseVars;

        vars.HubId           = CostOfDiseaseHubId.NoUniversity;
        globalData.ActiveHub = new GameplayHub(globalData);
        globalData.ActiveHub.SetDefaultTitle();
        globalData.ActiveHub.SetSubtitle(globalData.Years);

        const string       likeUsSection = "LikeUs";
        GameplayHubSection likeUs        = globalData.ActiveHub.AddSection(likeUsSection, true);
        likeUs.ReplaceShouldShow(() => globalData.Years == Years.Late && vars.Electricity == ExtendedBool.False);
        likeUs.AddDefaultContent(likeUsSection);

        const string       stepForwardSection = "StepForward";
        GameplayHubSection stepForward        = globalData.ActiveHub.AddSection(stepForwardSection, true);
        stepForward.ReplaceShouldShow(() => globalData.Years == Years.Late && vars.Electricity == ExtendedBool.True);
        stepForward.AddDefaultContent(stepForwardSection);

        const string       electricitySection = "Electricity";
        GameplayHubSection electricity        = globalData.ActiveHub.AddSection(electricitySection);
        electricity.ReplaceShouldShow(() => globalData.Years == Years.Late && vars.Electricity == ExtendedBool.True);
        electricity.AddDefaultContent(electricitySection);

        const string       pubSection = "Pub";
        GameplayHubSection pub        = globalData.ActiveHub.AddSection(pubSection, true);
        pub.AddDefaultContent(pubSection);
        pub.AddSpecialClickHere(pubSection, Barventures);

        const string       masterworkSection = "Masterwork";
        GameplayHubSection masterwork        = globalData.ActiveHub.AddSection(masterworkSection);
        masterwork.AddDefaultContent(masterworkSection);

        const string       recipeSection = "Recipe";
        GameplayHubSection recipe        = globalData.ActiveHub.AddSection(recipeSection);
        recipe.AddDefaultContent(recipeSection);
        recipe.AddSpecialClickHere(recipeSection, MWTemp);

        const string       completedSection = "Completed";
        GameplayHubSection completed        = globalData.ActiveHub.AddSection(completedSection);
        completed.AddDefaultContent(completedSection);
        completed.AddSpecialClickHere(completedSection, MWCompleteName);

        const string       knowledgeSection = "Knowledge";
        GameplayHubSection knowledge        = globalData.ActiveHub.AddSection(knowledgeSection);
        knowledge.ReplaceShouldShow(() => globalData.Years is Years.Early or Years.Middle);
        knowledge.AddDefaultContent(knowledgeSection);

        GameplayHubSection nextRound = globalData.ActiveHub.AddSection(string.Empty);
        nextRound.ReplaceShouldShow(() => globalData.Years is Years.Early or Years.Middle);
        nextRound.AddClickHereContinueNextRound(NoUniversity_0);

        globalData.ActiveHub.AddEndOfGenerationSection(NoUniversity_0);
    }

    private static void NoUniversity_0(GlobalData globalData)
    {
        TheCostOfDiseaseVars vars = globalData.TheCostOfDiseaseVars;

        if (globalData.Years == Years.Late) vars.Ending = CostOfDiseaseEnding.NoUniGood;

        globalData.ShowEndOfRoundPopUp(globalData.Years switch
        {
            Years.Early or Years.Middle => RoundEndKnowledge,
            Years.Late                  => vars.Electricity == ExtendedBool.False ? NoUni3b : Scoring,
            _                           => _ => { }
        });
    }

    #region Setup

    private static void NoUniversitySetup(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.TheCostOfDiseaseVars.Gen3SetupShown = true;
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.AngryMobSetup1, PopUpButton.Confirm, NoUniversity,
            content => content
                      .FormatWithIndex(0, globalData.LocalizedPlayerNumberIndex())
                      .FormatWithCondition(1, () => globalData.PlayersNum == 3));
    }

    private static void NoUniversityLate(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.Creepy_Icon, PopUpButton.Confirm, NoUniversity);
    }

    #endregion Setup

    #region Intro

    private static void NoUniversityIntro(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddGameplayTitle(GlobalTags.Gameplay_Generation_III);
        globalData.ActiveWindow.AddDefaultSubtitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(CreepyStar);
    }

    private static void CreepyStar(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(CreepyStar_0);
    }

    private static void CreepyStar_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.Creepy_Icon, PopUpButton.Confirm,
            globalData.TheCostOfDiseaseVars.LifeCount == 0 ? PanaceaUnleashCons1 : ImmortalNoUni);
    }

    private static void ImmortalNoUni(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(ImmortalNoUni_0);
    }

    private static void ImmortalNoUni_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.GainCaretakerFromLost, PopUpButton.Confirm, PanaceaUnleashCons1, string.Empty);
        globalData.ActivePopup.ReplaceDescription(ImmortalCaretakersText(globalData, "ImmortalNoUni_0_Content"));
    }

    private static void PanaceaUnleashCons1(GlobalData globalData)
    {
        if (globalData.TheCostOfDiseaseVars.Pana != PanaceaVal.Unleash)
        {
            NewMaster(globalData);
            return;
        }

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(PanaceaUnleashCons1_0);
    }

    private static void PanaceaUnleashCons1_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.Servant, PopUpButton.Confirm, NewMaster);
    }

    #endregion Intro

    #region New Masterwork

    private static string NewMasterPlayer(GlobalData globalData)
    {
        return globalData.GetPlayerNameByIndex(globalData.TheCostOfDiseaseVars.NewMasterIndex);
    }

    private static void NewMaster(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.TheCostOfDiseaseVars.NewMasterIndex = 0;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, globalData.TownName));
        globalData.ActiveWindow.AddNextContentWithLinks(1, [NewMasterHand]);
    }

    private static void NewMasterHand(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDoNotOtherPlayersToSee(NewMasterPlayer(globalData), NewMasterDiscipline);
    }

    private static void NewMasterDiscipline(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle(content => content.FormatWithReplacement(0, NewMasterPlayer(globalData)));
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [
            _ => NewMasterSetDiscipline(MasterworkDiscipline.Biology,     globalData),
            _ => NewMasterSetDiscipline(MasterworkDiscipline.Engineering, globalData),
            _ => NewMasterSetDiscipline(MasterworkDiscipline.Chemistry,   globalData),
            _ => NewMasterSetDiscipline(MasterworkDiscipline.Occult,      globalData)
        ]);
    }

    private static void NewMasterSetDiscipline(MasterworkDiscipline discipline, GlobalData globalData)
    {
        globalData.TheCostOfDiseaseVars.MwDiscipline[NewMasterPlayer(globalData)] = discipline;
        NewMasterType(globalData);
    }

    private static void NewMasterType(GlobalData globalData)
    {
        TheCostOfDiseaseVars vars       = globalData.TheCostOfDiseaseVars;
        string               player     = NewMasterPlayer(globalData);
        MasterworkDiscipline discipline = vars.MwDiscipline[player];

        globalData.SaveToUndo();

        vars.MwCost[player] = -1; // drawn when the card is shown (MasterworkCost)

        int disciplineIndex = (int)discipline - 1;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithIndex(0, disciplineIndex));
        globalData.ActiveWindow.AddNextContentWithLinks(1, [
            _ => NewMasterSetType((MasterworkType)(disciplineIndex * 2 + 1), globalData),
            _ => NewMasterSetType((MasterworkType)(disciplineIndex * 2 + 2), globalData)
        ], false, content => content.FormatWithIndex(0, disciplineIndex).FormatWithIndex(1, disciplineIndex));
    }

    private static void NewMasterSetType(MasterworkType type, GlobalData globalData)
    {
        globalData.TheCostOfDiseaseVars.MwType[NewMasterPlayer(globalData)] = type;
        NewMasterName(globalData);
    }

    private static string SanitizeMasterworkName(string name)
    {
        return new string(name.Where(character => character is not ('|' or '{' or '}' or '<' or '>' or ';')).ToArray()).Trim();
    }

    private static void NewMasterName(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveInputPopup = new GameplayInputPopup(globalData, "NewMasterName_Placeholder", PopUpButton.Confirm,
            name => SanitizeMasterworkName(name).Length is > 0 and <= 40,
            name =>
            {
                globalData.TheCostOfDiseaseVars.MwName[NewMasterPlayer(globalData)] = SanitizeMasterworkName(name);
                NewMasterResult(globalData);
            }, true);
    }

    private static void NewMasterResult(GlobalData globalData)
    {
        TheCostOfDiseaseVars vars   = globalData.TheCostOfDiseaseVars;
        string               player = NewMasterPlayer(globalData);

        globalData.SaveToUndo();
        vars.MwAdjective[player] = vars.RandomElement([0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10], _RND_MASTERWORK_ADJECTIVE + vars.NewMasterIndex);

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle(content => content
                                                          .FormatWithIndex(0, vars.MwAdjective[player])
                                                          .FormatWithReplacement(1, vars.MwName[player]));
        globalData.ActiveWindow.AddCustomContent(MasterworkCard(globalData, player));
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(NewMasterNext);
    }

    private static void NewMasterNext(GlobalData globalData)
    {
        globalData.TheCostOfDiseaseVars.NewMasterIndex += 1;

        if (globalData.TheCostOfDiseaseVars.NewMasterIndex < globalData.PlayersNum) NewMasterHand(globalData);
        else NoUniversitySetup(globalData);
    }

    private static string MasterworkReward(GlobalData globalData, string player)
    {
        return globalData.GetScenarioLocalizedTag("MasterworkReward_Content" + (int)globalData.TheCostOfDiseaseVars.MwType[player]);
    }

    private static int MasterworkCost(GlobalData globalData, string player)
    {
        TheCostOfDiseaseVars vars = globalData.TheCostOfDiseaseVars;
        if (vars.MwCost[player] >= 0) return vars.MwCost[player];

        // Science index: 0 - Engineering, 1 - Chemistry, 2 - Biology
        List<int> costs = vars.MwDiscipline[player] switch
        {
            MasterworkDiscipline.Biology     => [0, 1],
            MasterworkDiscipline.Engineering => [2, 1],
            MasterworkDiscipline.Chemistry   => [0, 2],
            _                                => [0, 1, 2]
        };
        vars.MwCost[player] = vars.RandomElement(costs, _RND_MASTERWORK_COST + Array.IndexOf(globalData.GetActivePlayers(), player));
        return vars.MwCost[player];
    }

    private static string MasterworkCard(GlobalData globalData, string player)
    {
        TheCostOfDiseaseVars vars = globalData.TheCostOfDiseaseVars;

        return globalData.GetScenarioLocalizedTag("MasterworkCard_Content" + (int)vars.MwType[player])
                         .FormatWithReplacement(0, vars.MwName[player])
                         .FormatWithIndex(1, MasterworkCost(globalData, player))
                         .FormatWithIndex(2, MasterworkCost(globalData, player))
               + MasterworkReward(globalData, player)
               + globalData.GetScenarioLocalizedTag("MasterworkCard_Note");
    }

    #endregion New Masterwork

    #region Masterwork Hub Actions

    private static void MWTemp(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddAllPlayersNamesAsOptions(playerName =>
        {
            globalData.TheCostOfDiseaseVars.MasterworkCheck = playerName;
            MWCheck(globalData);
        }, PlayerFormatterTag.Third);
    }

    private static void MWCheck(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddCustomContent(MasterworkCard(globalData, globalData.TheCostOfDiseaseVars.MasterworkCheck));
        globalData.ActiveWindow.AddNextContentWithLinks(2, [NoUniversity], true);
    }

    private static void MWCompleteName(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddAllPlayersNamesAsOptions(playerName =>
        {
            globalData.TheCostOfDiseaseVars.MasterworkCheck = playerName;
            MWCheckComplete(globalData);
        }, PlayerFormatterTag.Third);
    }

    private static void MWCheckComplete(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddCustomContent(MasterworkCard(globalData, globalData.TheCostOfDiseaseVars.MasterworkCheck), true);
        globalData.ActiveWindow.AddNextContentWithLinks(2, [MWComplete, NoUniversity], true);
    }

    private static void MWComplete(GlobalData globalData)
    {
        TheCostOfDiseaseVars vars        = globalData.TheCostOfDiseaseVars;
        string               player      = vars.MasterworkCheck;
        int                  playerIndex = Array.IndexOf(globalData.GetActivePlayers(), player);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle(content => content
                                                          .FormatWithIndex(0, vars.RandomElement([0, 1, 2, 3, 4, 5], _RND_MASTERWORK_COMPLETE + Math.Max(playerIndex, 0)))
                                                          .FormatWithReplacement(1, player));
        globalData.ActiveWindow.AddNextContent((int)vars.MwType[player], false, content => content
                                                                                          .FormatWithReplacement(0, vars.MwName[player])
                                                                                          .FormatWithReplacement(1, player));
        globalData.ActiveWindow.AddNextContent(10, true, content => content + MasterworkReward(globalData, player));
        globalData.ActiveWindow.AddNextContentWithLinks(11, [NoUniversity], true);
    }

    #endregion Masterwork Hub Actions

    #region Bar-ventures

    private static int BarventureIndex(GlobalData globalData) => Math.Min(Math.Max(globalData.TheCostOfDiseaseVars.BarCount - 1, 0), _BAR_RANDOM_SIZE - 1);

    // 1 - Blacksmith, 2 - Farmer's Market, 3 - Fortune Teller, 4 - General Store, 5 - Hospital, 6 - Laborer's Union, 7 - Cemetery
    private static int BarventureBar(GlobalData globalData)
    {
        List<int> bars = globalData.Years == Years.Late ? [1, 2, 3, 4, 6, 7] : [1, 2, 3, 4, 5, 6, 7];
        return globalData.TheCostOfDiseaseVars.RandomElement(bars, _RND_BAR + BarventureIndex(globalData));
    }

    private static void Barventures(GlobalData globalData)
    {
        TheCostOfDiseaseVars vars = globalData.TheCostOfDiseaseVars;

        globalData.SaveToUndo();
        vars.BarCount += 1;

        int index = BarventureIndex(globalData);
        int bar   = BarventureBar(globalData);

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content
                                                            .FormatWithIndex(1, vars.RandomElement([0, 1, 2], _RND_BAR_INTRO_VALUE + index))
                                                            .FormatWithIndex(0, vars.RandomElement([0, 1, 2], _RND_BAR_INTRO + index)));
        globalData.ActiveWindow.AddNextContent(bar, true, content => content
                                                                    .FormatWithReplacement(0, vars.RandomInRange(1, 999, _RND_BAR_CASE_A + index).ToString())
                                                                    .FormatWithReplacement(1, vars.RandomInRange(1, 999, _RND_BAR_CASE_B + index).ToString())
                                                                    .FormatWithCondition(2, () => vars.RandomBool(_RND_BAR_VARIANT + index)));
        globalData.ActiveWindow.AddClickHereToContinue(Barventures_0);
    }

    private static void Barventures_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.Creepy_Icon, PopUpButton.Confirm, NoUniversity,
            content => content.FormatWithIndex(0, BarventureBar(globalData) - 1));
    }

    #endregion Bar-ventures

    #region Round Events

    private static void RoundEndKnowledge(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithCondition(0, () => globalData.Years == Years.Middle));
        globalData.ActiveWindow.AddClickHereToContinue(RoundEndKnowledge_0);
    }

    private static void RoundEndKnowledge_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.Creepy_Icon, PopUpButton.Confirm,
            globalData.Years == Years.Middle ? CureNegCons : NoUniMayornCreepy);
    }

    private static void CureNegCons(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(CureNegCons_0);
    }

    private static void CureNegCons_0(GlobalData globalData)
    {
        string feverCure = globalData.TheCostOfDiseaseVars.FeverCure;
        if (string.IsNullOrEmpty(feverCure)) feverCure = globalData.PlayerAName;

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.AngryMob_Icon, PopUpButton.Confirm, OptiontoKillStart,
            content => content.FormatWithReplacement(0, feverCure));
    }

    private static void OptiontoKillStart(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(OptiontoKillYes);
    }

    private static void OptiontoKillYes(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [OptiontoKillQuestion]);
    }

    private static void OptiontoKillQuestion(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [DetEffectRandom, OptiontoKillQuestion_No]);
    }

    private static void OptiontoKillQuestion_No(GlobalData globalData)
    {
        globalData.TheCostOfDiseaseVars.ImmortalityNone = true;
        NoUniversity(globalData);
    }

    private static void NoUniMayornCreepy(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(NoUniMayornCreepy_0);
    }

    private static void NoUniMayornCreepy_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.CompulsionBack, PopUpButton.Confirm, S5Specialbar1,
            content => content
                      .FormatWithReplacement(0, globalData.TheCostOfDiseaseVars.Mayor)
                      .FormatWithReplacement(1, globalData.TheCostOfDiseaseVars.Charity));
    }

    private static void S5Specialbar1(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(globalData.PlayersNum == 2 ? ElectionBid : S5SpecialVote);
    }

    // Originally 2p-S5SpecialVoteALT
    private static void ElectionBid(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [ElectionBid2]);
    }

    // Originally 2p-S5SpecialVoteALT2
    private static void ElectionBid2(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [ElecY, ElecN]);
    }

    private static void S5SpecialVote(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [S5SpecialVote2]);
    }

    private static void S5SpecialVote2(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [ElecY, ElecN]);
    }

    private static void ElecY(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.TheCostOfDiseaseVars.Electricity = ExtendedBool.True;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(ElecY_0);
    }

    private static void ElecY_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.DiscardEstateUpgrade_Icon, PopUpButton.Confirm,
            globalData.TheCostOfDiseaseVars.ImmortalityNone ? NoUniversityLate : DetEffectRandom);
    }

    private static void ElecN(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.TheCostOfDiseaseVars.Electricity = ExtendedBool.False;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(ElecN_0);
    }

    private static void ElecN_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.AngryMob_Icon, PopUpButton.Confirm,
            globalData.TheCostOfDiseaseVars.ImmortalityNone ? NoUniversityLate : DetEffectRandom,
            content => content.FormatWithCondition(0, () => globalData.PlayersNum == 2));
    }

    private static void NoUni3b(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [Scoring]);
    }

    #endregion Round Events
}
