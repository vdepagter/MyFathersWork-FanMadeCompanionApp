namespace MyFathersWorkWebApp;

public static partial class TheCostOfDisease
{
    private const int _RND_ULTIMATE_DISEASE_PLAYER = 123;
    private const int _RND_DET_EFFECT_PHRASE       = 124; // 124 - 125
    private const int _RND_DET_EFFECT              = 126; // 126 - 127
    private const int _RND_DET_EFFECT_VP           = 128; // 128 - 129
    private const int _RND_SANITY_CHECK            = 130;

    public static void University(GlobalData globalData)
    {
        // Round 19 -> University1
        // Round 20 -> University2
        // Round 21 -> University3
        TheCostOfDiseaseVars vars = globalData.TheCostOfDiseaseVars;

        vars.HubId           = CostOfDiseaseHubId.University;
        globalData.ActiveHub = new GameplayHub(globalData);
        globalData.ActiveHub.SetDefaultTitle();
        globalData.ActiveHub.SetSubtitle(globalData.Years);

        const string       ultimateDiseaseSection = "UltimateDisease";
        GameplayHubSection ultimateDisease        = globalData.ActiveHub.AddSection(ultimateDiseaseSection, true);
        ultimateDisease.ReplaceShouldShow(() => globalData.Years == Years.Middle);
        ultimateDisease.AddDefaultContent(ultimateDiseaseSection);

        const string       nobelSection = "Nobel";
        GameplayHubSection nobel        = globalData.ActiveHub.AddSection(nobelSection, true);
        nobel.AddDefaultContent(nobelSection, content => content
                                                        .FormatWithReplacement(0, globalData.TownName)
                                                        .FormatWithIndex(1, globalData.LocalizedPlayerNumberIndex()));

        const string       mentalSection = "Mental";
        GameplayHubSection mental        = globalData.ActiveHub.AddSection(mentalSection);
        mental.AddDefaultContent(mentalSection, content => content.FormatWithReplacement(0, vars.Mental.ToString()));

        GameplayHubSection nextRound = globalData.ActiveHub.AddSection(string.Empty);
        nextRound.ReplaceShouldShow(() => globalData.Years is Years.Early or Years.Middle);
        nextRound.AddClickHereContinueNextRound(University_0);

        globalData.ActiveHub.AddEndOfGenerationSection(University_0);
    }

    private static void University_0(GlobalData globalData)
    {
        if (globalData.Years == Years.Late) globalData.TheCostOfDiseaseVars.Ending = CostOfDiseaseEnding.UniGood;

        globalData.ShowEndOfRoundPopUp(globalData.Years switch
        {
            Years.Early  => UniEquity1,
            Years.Middle => UniEvent2UltimateDisease,
            Years.Late   => SanityCheck,
            _            => _ => { }
        });
    }

    #region Setup

    private static void UniversitySetup(GlobalData globalData)
    {
        globalData.SaveToUndo();

        TheCostOfDiseaseVars vars = globalData.TheCostOfDiseaseVars;

        if (!vars.SuspicionMoved)
        {
            vars.SuspicionMoved =  true;
            vars.Tracker        += 2;
            if (globalData.PlayersNum == 4) vars.Tracker += 1;
        }

        vars.Gen3SetupShown    = true;
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.AngryMobSetup1, PopUpButton.Confirm, University,
            content => content
                      .FormatWithReplacement(0, vars.Tracker.ToString())
                      .FormatWithCondition(1, () => globalData.PlayersNum == 3));
    }

    private static void UniversityLate(GlobalData globalData)
    {
        if (globalData.TheCostOfDiseaseVars.Ultimate) UniversityLateSetup(globalData);
        else University(globalData);
    }

    private static void UniversityLateSetup(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.VillageChronicleCover, PopUpButton.Confirm, University);
    }

    #endregion Setup

    #region Intro

    private static void UniversityIntro(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddGameplayTitle(GlobalTags.Gameplay_Generation_III);
        globalData.ActiveWindow.AddDefaultSubtitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(UniMayor);
    }

    private static void UniMayor(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content
                                                            .FormatWithCondition(0, () => globalData.TheCostOfDiseaseVars.Uni == ExtendedBool.True)
                                                            .FormatWithReplacement(1, globalData.TownName)
                                                            .FormatWithReplacement(2, globalData.TheCostOfDiseaseVars.Mayor));
        globalData.ActiveWindow.AddClickHereToContinue(UniMayor_0);
    }

    private static void UniMayor_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S1_MayorCoin, PopUpButton.Confirm,
            globalData.TheCostOfDiseaseVars.LifeCount == 0 ? UniEquity : UniImmortal,
            content => content.FormatWithReplacement(0, globalData.TheCostOfDiseaseVars.Mayor));
    }

    private static void UniImmortal(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(UniImmortal_0);
    }

    private static string ImmortalCaretakersText(GlobalData globalData, string tag)
    {
        string content = string.Empty;

        foreach (string player in globalData.GetActivePlayers())
        {
            if (!globalData.TheCostOfDiseaseVars.Life[player]) continue;
            content += globalData.GetScenarioLocalizedTag(tag + "1").FormatWithReplacement(0, player);
        }

        return content + globalData.GetScenarioLocalizedTag(tag + "2");
    }

    private static void UniImmortal_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.GainCaretakerFromLost, PopUpButton.Confirm, UniEquity, string.Empty);
        globalData.ActivePopup.ReplaceDescription(ImmortalCaretakersText(globalData, "UniImmortal_0_Content"));
    }

    private static void UniEquity(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, globalData.TownName));
        globalData.ActiveWindow.AddClickHereToContinue(UniEquity_0);
    }

    private static void UniEquity_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.ScoreTrackMarker, PopUpButton.Confirm, InsanitySign);
    }

    private static void InsanitySign(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddAllPlayersNamesAsOptions(playerName =>
        {
            globalData.TheCostOfDiseaseVars.SanePlayer = playerName;
            SanityChoice(globalData);
        });
    }

    private static void SanityChoice(GlobalData globalData)
    {
        string sanePlayer = globalData.TheCostOfDiseaseVars.SanePlayer;

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle(content => content.FormatWithReplacement(0, sanePlayer));
        globalData.ActiveWindow.AddHandStorybookToNoJump(sanePlayer);
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, sanePlayer));
        globalData.ActiveWindow.AddNextContentWithLinks(1, [_ => SanityChoiceMental(3, globalData), _ => SanityChoiceMental(4, globalData), _ => SanityChoiceMental(5, globalData),
            _ => SanityChoiceMental(6, globalData), _ => SanityChoiceMental(7, globalData)], false, content => content.FormatWithReplacement(0, sanePlayer));
    }

    private static void SanityChoiceMental(int mental, GlobalData globalData)
    {
        globalData.TheCostOfDiseaseVars.Mental = mental;
        PanaceaUnleashCons2(globalData);
    }

    private static void PanaceaUnleashCons2(GlobalData globalData)
    {
        if (globalData.TheCostOfDiseaseVars.Pana != PanaceaVal.Unleash)
        {
            UniversitySetup(globalData);
            return;
        }

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(PanaceaUnleashCons2_0);
    }

    private static void PanaceaUnleashCons2_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.Servant, PopUpButton.Confirm, UniversitySetup);
    }

    #endregion Intro

    #region Early Years Events

    private static void UniEquity1(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(UniEquity1_0);
    }

    private static void UniEquity1_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.ScoreTrackMarker, PopUpButton.Confirm, UniEvent1UltimateDisease);
    }

    private static void UniEvent1UltimateDisease(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content
                                                            .FormatWithIndex(0, globalData.LocalizedPlayerNumberIndex())
                                                            .FormatWithReplacement(1, globalData.TownName));
        globalData.ActiveWindow.AddClickHereToContinue(UniCharity);
    }

    private static void UniCharity(GlobalData globalData)
    {
        TheCostOfDiseaseVars vars = globalData.TheCostOfDiseaseVars;

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content
                                                            .FormatWithReplacement(1, globalData.TownName)
                                                            .FormatWithReplacement(2, vars.Mayor)
                                                            .FormatWithReplacement(3, vars.Charity)
                                                            .FormatWithCondition(0, () => vars.Mayor == vars.Charity));
        globalData.ActiveWindow.AddClickHereToContinue(UniCharity_0);
    }

    private static void UniCharity_0(GlobalData globalData)
    {
        TheCostOfDiseaseVars vars = globalData.TheCostOfDiseaseVars;

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S1_HeartToken, PopUpButton.Confirm, DetEffectRandom,
            content => content
                      .FormatWithReplacement(1, vars.Charity)
                      .FormatWithCondition(0, () => vars.Mayor == vars.Charity));
    }

    #endregion Early Years Events

    #region Middle Years Events

    private static void UniEvent2UltimateDisease(GlobalData globalData)
    {
        string player = globalData.TheCostOfDiseaseVars.RandomElement(globalData.GetActivePlayers().ToList(), _RND_ULTIMATE_DISEASE_PLAYER);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, player));
        globalData.ActiveWindow.AddNextContentWithLinks(1, [UniEvent2UltimateDisease2]);
    }

    private static void UniEvent2UltimateDisease2(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [UniEvent2Success, UniEvent2Failure]);
    }

    private static void UniEvent2Success(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.TheCostOfDiseaseVars.Ultimate = true;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(UniEvent2Success_0);
    }

    private static void UniEvent2Success_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.Creepy_Icon, PopUpButton.Confirm, DetEffectRandom);
    }

    private static void UniEvent2Failure(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(UniEvent2Failure_0);
    }

    private static void UniEvent2Failure_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.Creepy_Icon, PopUpButton.Confirm, DetEffectRandom,
            content => content.FormatWithCondition(0, () => globalData.PlayersNum < 4));
    }

    #endregion Middle Years Events

    #region End of Generation

    private static void SanityCheck(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(SanityCheck_0);
    }

    private static void SanityCheck_0(GlobalData globalData)
    {
        TheCostOfDiseaseVars vars = globalData.TheCostOfDiseaseVars;

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.Insanity_Icon, PopUpButton.Confirm, ForScience,
            content => content
                      .FormatWithReplacement(0, vars.Mental.ToString())
                      .FormatWithReplacement(1, vars.SanePlayer)
                      .FormatWithReplacement(2, vars.RandomElement([8, 9], _RND_SANITY_CHECK).ToString()));
    }

    private static void ForScience(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(ForScience_0);
    }

    private static void ForScience_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.Creepy_Icon, PopUpButton.Confirm, Scoring);
    }

    #endregion End of Generation

    #region Effects of Immortality

    private static void DetEffectRandom(GlobalData globalData)
    {
        TheCostOfDiseaseVars vars = globalData.TheCostOfDiseaseVars;

        if (vars.Killed)
        {
            DeteriorationHub(globalData);
            return;
        }

        // Title suffix names the round that just ended, only shown in the No University branch
        int titleIndex = vars.HubId == CostOfDiseaseHubId.NoUniversity ? globalData.Years == Years.Middle ? 1 : 2 : 0;

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle(content => content.FormatWithIndex(0, titleIndex));
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithIndex(0, vars.RandomElement([0, 1, 2], _RND_DET_EFFECT_PHRASE + Math.Min(vars.DetCount, 1))));
        globalData.ActiveWindow.AddNextContentWithLinks(1, [EffectRandomizer]);
    }

    private static void EffectRandomizer(GlobalData globalData)
    {
        TheCostOfDiseaseVars     vars    = globalData.TheCostOfDiseaseVars;
        List<Action<GlobalData>> effects = new();

        if (!vars.DetVisited[0]) effects.Add(DetEffect1);
        if (!vars.DetVisited[1]) effects.Add(DetEffect2);
        if (!vars.DetVisited[2]) effects.Add(DetEffect3);
        if (!vars.DetVisited[3]) effects.Add(DetEffect4);

        if (effects.Count == 0)
        {
            DeteriorationHub(globalData);
            return;
        }

        vars.RandomElement(effects, _RND_DET_EFFECT + Math.Min(vars.DetCount, 1)).Invoke(globalData);
    }

    private static void DetEffect1(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.TheCostOfDiseaseVars.DetCount      += 1;
        globalData.TheCostOfDiseaseVars.DetVisited[0] =  true;
        globalData.TheCostOfDiseaseVars.Killed        =  true;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(DetEffect1_0);
    }

    private static void DetEffect1_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S1_Immortality, PopUpButton.Confirm, DeteriorationHub);
    }

    private static void DetEffect2(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.TheCostOfDiseaseVars.DetCount      += 1;
        globalData.TheCostOfDiseaseVars.DetVisited[1] =  true;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(DetEffect2_0);
    }

    private static void DetEffect2_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S1_Immortality, PopUpButton.Confirm, DeteriorationHub);
    }

    private static void DetEffect3(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.TheCostOfDiseaseVars.DetCount      += 1;
        globalData.TheCostOfDiseaseVars.DetVisited[2] =  true;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(DetEffect3_0);
    }

    private static void DetEffect3_0(GlobalData globalData)
    {
        TheCostOfDiseaseVars vars = globalData.TheCostOfDiseaseVars;

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S1_Immortality, PopUpButton.Confirm, DeteriorationHub,
            content => content.FormatWithReplacement(0, vars.RandomElement([1, 2], _RND_DET_EFFECT_VP + Math.Min(Math.Max(vars.DetCount - 1, 0), 1)).ToString()));
    }

    private static void DetEffect4(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.TheCostOfDiseaseVars.DetCount      += 1;
        globalData.TheCostOfDiseaseVars.DetVisited[3] =  true;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(DeteriorationHub);
    }

    private static void DeteriorationHub(GlobalData globalData)
    {
        bool late = globalData.Years == Years.Late;

        if (globalData.TheCostOfDiseaseVars.HubId == CostOfDiseaseHubId.University)
        {
            if (late) UniversityLate(globalData);
            else University(globalData);
        }
        else
        {
            if (late) NoUniversityLate(globalData);
            else NoUniversity(globalData);
        }
    }

    #endregion Effects of Immortality
}
