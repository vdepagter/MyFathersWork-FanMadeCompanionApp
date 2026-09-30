namespace MyFathersWorkWebApp;

public static partial class TheCostOfDisease
{
    private const int _RND_MAYOR_RESOLVE_VP  = 70;
    private const int _RND_HUNT_REWARDS      = 71;  // 71 - 77 (reward per position, 8th position uses _RND_HUNT_REWARD_LAST)
    private const int _RND_HUNTERS           = 78;  // 78 - 80 (hunter per position)
    private const int _RND_HUNT_CHOOSER      = 81;  // 81 - 84 (chooser, hunt name) x 2 hunts
    private const int _RND_HUNT_MONSTER      = 85;  // 85 - 88 (one per direction)
    private const int _RND_HUNT_SUCCESS      = 89;  // 89 - 94 (title, sound, reward) x 2 hunts
    private const int _RND_HUNT_FAIL         = 95;  // 95 - 98 (title, verb) x 2 hunts
    private const int _RND_HUNT_CHECK        = 99;  // 99 - 101
    private const int _RND_ANGRY_MOB         = 102; // 102 - 111
    private const int _RND_ANGRY_MOB_VP      = 112; // 112 - 121
    private const int _RND_HUNT_REWARD_LAST  = 122;
    private const int _ANGRY_MOB_RANDOM_SIZE = 10;

    // Monsters: 0 - Strigoi, 1 - Moon Presence, 2 - Manticore, 3 - Golem, 4 - Wight, 5 - Pricolici, 6 - Troll, 7 - Priest
    private static readonly int[][] _DirectionMonsters =
    [
        [4, 1], // North
        [5, 6], // East
        [3, 2], // West
        [0, 7]  // South
    ];

    public static void Prosperity(GlobalData globalData)
    {
        // Round 13 -> Prosperity1
        // Round 14 -> Prosperity2
        // Round 15 -> Prosperity3 / Prosperity3b
        TheCostOfDiseaseVars vars = globalData.TheCostOfDiseaseVars;

        vars.HubId           = CostOfDiseaseHubId.Prosperity;
        globalData.ActiveHub = new GameplayHub(globalData);
        globalData.ActiveHub.SetDefaultTitle(content => content.FormatWithIndex(0, vars.ReturnToEvil ? 2 : globalData.Years == Years.Early ? 0 : 1));
        globalData.ActiveHub.SetSubtitle(globalData.Years);

        bool Order() => vars.Society == Society.OrderOfStHubertus && !vars.ReturnToEvil;
        bool Hunters() => vars.Society == Society.FraternityOfHunters && !vars.ReturnToEvil;

        // Order of St. Hubertus
        const string       acceptanceSection = "Acceptance";
        GameplayHubSection acceptance        = globalData.ActiveHub.AddSection(acceptanceSection, true);
        acceptance.ReplaceShouldShow(Order);
        acceptance.AddDefaultContent(acceptanceSection);

        const string       schoolSection = "School";
        GameplayHubSection school        = globalData.ActiveHub.AddSection(schoolSection);
        school.ReplaceShouldShow(Order);
        school.AddDefaultContent(schoolSection);

        const string       experimentsFearedSection = "ExperimentsFeared";
        GameplayHubSection experimentsFeared        = globalData.ActiveHub.AddSection(experimentsFearedSection);
        experimentsFeared.ReplaceShouldShow(Order);
        experimentsFeared.AddDefaultContent(experimentsFearedSection);

        const string       farmersMarketSection = "FarmersMarket";
        GameplayHubSection farmersMarket        = globalData.ActiveHub.AddSection(farmersMarketSection);
        farmersMarket.ReplaceShouldShow(() => Order() && globalData.Years is Years.Middle or Years.Late);
        farmersMarket.AddDefaultContent(farmersMarketSection);

        const string       angryMobSection = "AngryMob";
        GameplayHubSection angryMob        = globalData.ActiveHub.AddSection(angryMobSection);
        angryMob.ReplaceShouldShow(Order);
        angryMob.AddDefaultContent(angryMobSection);
        angryMob.AddSpecialClickHere(angryMobSection, AngryMobStorybook);

        // Fraternity of Hunters
        const string       huntersHavenSection = "HuntersHaven";
        GameplayHubSection huntersHaven        = globalData.ActiveHub.AddSection(huntersHavenSection, true);
        huntersHaven.ReplaceShouldShow(Hunters);
        huntersHaven.AddDefaultContent(huntersHavenSection);

        const string       engineeringSection = "Engineering";
        GameplayHubSection engineering        = globalData.ActiveHub.AddSection(engineeringSection);
        engineering.ReplaceShouldShow(() => Hunters() && globalData.Years != Years.Late);
        engineering.AddDefaultContent(engineeringSection);

        const string       endlessHuntSection = "EndlessHunt";
        GameplayHubSection endlessHunt        = globalData.ActiveHub.AddSection(endlessHuntSection);
        endlessHunt.ReplaceShouldShow(() => Hunters() && globalData.Years != Years.Late);
        endlessHunt.AddDefaultContent(endlessHuntSection);

        // Both
        const string       scientificAchievementSection = "ScientificAchievement";
        GameplayHubSection scientificAchievement        = globalData.ActiveHub.AddSection(scientificAchievementSection);
        scientificAchievement.ReplaceShouldShow(() => !vars.ReturnToEvil && globalData.Years == Years.Late);
        scientificAchievement.AddDefaultContent(scientificAchievementSection);

        // A Return to Evil
        const string       monsterSpawnSection = "MonsterSpawn";
        GameplayHubSection monsterSpawn        = globalData.ActiveHub.AddSection(monsterSpawnSection, true);
        monsterSpawn.ReplaceShouldShow(() => vars.ReturnToEvil);
        monsterSpawn.AddDefaultContent(monsterSpawnSection);

        GameplayHubSection nextRound = globalData.ActiveHub.AddSection(string.Empty);
        nextRound.ReplaceShouldShow(() => globalData.Years is Years.Early or Years.Middle);
        nextRound.AddClickHereContinueNextRound(Prosperity_0);

        globalData.ActiveHub.AddEndOfGenerationSection(Prosperity_0);
    }

    private static void Prosperity_0(GlobalData globalData)
    {
        TheCostOfDiseaseVars vars  = globalData.TheCostOfDiseaseVars;
        bool                 order = vars.Society == Society.OrderOfStHubertus;

        if (globalData.Years == Years.Late)
        {
            vars.Ending = vars.ReturnToEvil ? CostOfDiseaseEnding.WolvesEvil2 : order ? CostOfDiseaseEnding.WolvesGood1 : CostOfDiseaseEnding.HunterGood1;
        }

        globalData.ShowEndOfRoundPopUp(globalData.Years switch
        {
            Years.Early  => order ? WolvesEcoFriendly : CharityAwardGood,
            Years.Middle => order ? GoodFrenzyEvent : CureMoonSick1,
            Years.Late   => Scoring,
            _            => _ => { }
        });
    }

    #region Setup

    private static void ProsperitySetup(GlobalData globalData)
    {
        globalData.SaveToUndo();

        TheCostOfDiseaseVars vars  = globalData.TheCostOfDiseaseVars;
        bool                 order = vars.Society == Society.OrderOfStHubertus;

        if (order && !vars.SuspicionMoved)
        {
            vars.SuspicionMoved =  true;
            vars.Tracker        -= 2;
            if (globalData.PlayersNum == 4) vars.Tracker -= 1;
        }

        vars.Gen3SetupShown    = true;
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, order ? PopUpIcon.AngryMobSetup1 : PopUpIcon.S1_MastersStudy, PopUpButton.Confirm, Prosperity, string.Empty);

        string content = globalData.GetScenarioLocalizedTag(order ? "ProsperitySetup_Content" : "ProsperitySetup_Content1")
                                   .FormatWithReplacement(0, vars.Tracker.ToString())
                                   .FormatWithCondition(1, () => globalData.PlayersNum == 3);
        content += BuildingTilesText(globalData, "ProsperitySetup_Content2", value => value > 0);
        content += globalData.GetScenarioLocalizedTag("ProsperitySetup_Content3");
        globalData.ActivePopup.ReplaceDescription(content);
    }

    private static void ProsperityReturnToEvil(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.TheCostOfDiseaseVars.ReturnToEvil = true;

        bool overrun = globalData.TheCostOfDiseaseVars.Overrun == ExtendedBool.True;
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S1_Suspicious_Building, PopUpButton.Confirm, Prosperity, string.Empty);

        string content = globalData.GetScenarioLocalizedTag(overrun ? "ProsperityReturnToEvil_Content" : "ProsperityReturnToEvil_Content1");
        content += BuildingTilesText(globalData, "ProsperitySetup_Content2", value => value > 0);
        globalData.ActivePopup.ReplaceDescription(content);
    }

    #endregion Setup

    #region Intro

    private static void ProsperityHunterIntro(GlobalData globalData)
    {
        globalData.SaveToUndo();

        TheCostOfDiseaseVars vars = globalData.TheCostOfDiseaseVars;
        vars.HuntCount   = 0;
        vars.HuntRewards = [-1, -1, -1, -1, -1, -1, -1, -1];
        vars.HunterOrder = [string.Empty, string.Empty, string.Empty, string.Empty];

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddGameplayTitle(GlobalTags.Gameplay_Generation_III);
        globalData.ActiveWindow.AddDefaultSubtitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(Evilsforgive);
    }

    private static void ProsperityWolvesIntro(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddGameplayTitle(GlobalTags.Gameplay_Generation_III);
        globalData.ActiveWindow.AddDefaultSubtitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(GoodConsequences);
    }

    private static void GoodConsequences(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(GoodConsequences_0);
    }

    private static void GoodConsequences_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.Creepy_Icon, PopUpButton.Confirm, Evilsforgive);
    }

    private static void Evilsforgive(GlobalData globalData)
    {
        TheCostOfDiseaseVars vars = globalData.TheCostOfDiseaseVars;

        if (vars.Society == Society.FraternityOfHunters)
        {
            if (vars.HCount == 0) EquitableValues(globalData);
            else EvilsforgiveWary(globalData);
            return;
        }

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(Evilsforgive_0);
    }

    private static void Evilsforgive_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S1_MastersStudy, PopUpButton.Confirm, EquitableValues);
    }

    private static void EvilsforgiveWary(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(AnyAlly(globalData, Faction.Wolves) ? EvilsforgiveWary_0 : EquitableValues);
    }

    private static void EvilsforgiveWary_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.Creepy_Icon, PopUpButton.Confirm, EquitableValues, string.Empty);
        globalData.ActivePopup.ReplaceDescription(AlliesList(globalData, Faction.Wolves, "EvilsforgiveWary_0_Content1"));
    }

    private static void EquitableValues(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithCondition(0, () => globalData.TheCostOfDiseaseVars.Society == Society.OrderOfStHubertus));
        globalData.ActiveWindow.AddClickHereToContinue(EquitableValues_0);
    }

    private static void EquitableValues_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.Creepy_Icon, PopUpButton.Confirm, LycanMessageGood,
            content => content.FormatWithCondition(0, () => globalData.PlayersNum <= 3));
    }

    private static void LycanMessageGood(GlobalData globalData)
    {
        if (globalData.TheCostOfDiseaseVars.Wolves != Affiliation.Good)
        {
            ProsperitySetup(globalData);
            return;
        }

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [LycanMessageGood_Yes, ProsperitySetup]);
    }

    private static void LycanMessageGood_Yes(GlobalData globalData)
    {
        globalData.TheCostOfDiseaseVars.Lycan = true;
        LycanGood(globalData);
    }

    private static void LycanGood(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(LycanGood_0);
    }

    private static void LycanGood_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S1_MWUpdateLycanthropic, PopUpButton.Confirm, ProsperitySetup);
    }

    #endregion Intro

    #region Angry Mob

    private static void AngryMobStorybook(GlobalData globalData)
    {
        globalData.SaveToUndo();

        TheCostOfDiseaseVars vars  = globalData.TheCostOfDiseaseVars;
        int                  index = Math.Min(vars.AngryMobCount, _ANGRY_MOB_RANDOM_SIZE - 1);
        vars.AngryMobCount += 1;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content
                                                            .FormatWithReplacement(1, globalData.TownName)
                                                            .FormatWithCondition(0, () => vars.RandomBool(_RND_ANGRY_MOB + index)));
        globalData.ActiveWindow.AddClickHereToContinue(AngryMobStorybook_0);
    }

    private static void AngryMobStorybook_0(GlobalData globalData)
    {
        globalData.SaveToUndo();

        TheCostOfDiseaseVars vars  = globalData.TheCostOfDiseaseVars;
        int                  index = Math.Min(Math.Max(vars.AngryMobCount - 1, 0), _ANGRY_MOB_RANDOM_SIZE - 1);

        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.Creepy_Icon, PopUpButton.Confirm, Prosperity,
            content => content
                      .FormatWithReplacement(1, vars.RandomElement([2, 3], _RND_ANGRY_MOB_VP + index).ToString())
                      .FormatWithCondition(0, () => globalData.Years == Years.Late));
    }

    #endregion Angry Mob

    #region Order of St. Hubertus Events

    private static void WolvesEcoFriendly(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(WolvesEcoFriendly_0);
    }

    private static void WolvesEcoFriendly_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.Creepy_Icon, PopUpButton.Confirm,
            globalData.TheCostOfDiseaseVars.Building == BankOrLibrary.Library ? MayoralAward : WolvesBankMayorGood);
    }

    private static void MayoralAward(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddHandStorybookToNoJump(globalData.TheCostOfDiseaseVars.Mayor + " III");
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, globalData.TheCostOfDiseaseVars.Mayor));
        globalData.ActiveWindow.AddClickHereToContinue(MayoralAward_0);
    }

    private static void MayoralAward_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S1_EstateUpgradeBACK, PopUpButton.Confirm, ResolveCharityWolves);
    }

    private static void WolvesBankMayorGood(GlobalData globalData)
    {
        if (globalData.TheCostOfDiseaseVars.Building != BankOrLibrary.Bank)
        {
            ResolveCharityWolves(globalData);
            return;
        }

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddHandStorybookToNoJump(globalData.TheCostOfDiseaseVars.Mayor + " III");
        globalData.ActiveWindow.AddDefaultContent(content => content
                                                            .FormatWithReplacement(0, globalData.TheCostOfDiseaseVars.Mayor)
                                                            .FormatWithReplacement(1, globalData.TownName));
        globalData.ActiveWindow.AddClickHereToContinue(WolvesBankMayorGood_0);
    }

    private static void WolvesBankMayorGood_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S1_EstateUpgradeBACK, PopUpButton.Confirm, ResolveCharityWolves,
            content => content.FormatWithReplacement(0, globalData.TheCostOfDiseaseVars.Mayor));
    }

    private static void ResolveCharityWolves(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddHandStorybookToNoJump(globalData.TheCostOfDiseaseVars.Charity + " III");
        globalData.ActiveWindow.AddDefaultContent(content => content
                                                            .FormatWithReplacement(0, globalData.TheCostOfDiseaseVars.Charity)
                                                            .FormatWithReplacement(1, globalData.TownName));
        globalData.ActiveWindow.AddClickHereToContinue(ResolveCharityWolves_0);
    }

    private static void ResolveCharityWolves_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S1_EstateUpgradeBACK, PopUpButton.Confirm, Prosperity,
            content => content.FormatWithReplacement(0, globalData.TheCostOfDiseaseVars.Charity));
    }

    private static void GoodFrenzyEvent(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(globalData.PlayersNum == 2 ? FrenzyBid : GoodFrenzyEvent2);
    }

    // Originally 2p-FrenzyALT
    private static void FrenzyBid(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [FrenzyBid2]);
    }

    // Originally 2p-FrenzyALTb
    private static void FrenzyBid2(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [YesFrenzy, NoFrenzy]);
    }

    private static void GoodFrenzyEvent2(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [GoodFrenzyEvent2b]);
    }

    private static void GoodFrenzyEvent2b(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [YesFrenzy, NoFrenzy]);
    }

    private static void YesFrenzy(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.TheCostOfDiseaseVars.Frenzy = ExtendedBool.True;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, globalData.TownName));
        globalData.ActiveWindow.AddClickHereToContinue(ProsperityReturnToEvil);
    }

    private static void NoFrenzy(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.TheCostOfDiseaseVars.Frenzy = ExtendedBool.False;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(NoFrenzy_0);
    }

    private static void NoFrenzy_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S1_MastersStudy, PopUpButton.Confirm, Prosperity);
    }

    #endregion Order of St. Hubertus Events

    #region Fraternity of Hunters Events

    // 0 - first hunt (after the Early Years), 1 - second hunt (after the Middle Years)
    private static int HuntIndex(GlobalData globalData) => globalData.Years == Years.Middle ? 0 : 1;

    // Hunters and rewards are shuffled (Harlowe "shuffled") but drawn one position at a time when they are shown,
    // so mirror mode asks for them when the official app shows them
    private static string[] CurrentHunters(GlobalData globalData)
    {
        if (globalData.PlayersNum == 2) return [globalData.PlayerAName, globalData.PlayerBName];
        if (HuntIndex(globalData) == 0) return [HunterAt(globalData, 0), HunterAt(globalData, 1)];
        return globalData.PlayersNum == 3 ? [HunterAt(globalData, 2), HunterAt(globalData, 1)] : [HunterAt(globalData, 2), HunterAt(globalData, 3)];
    }

    private static string HunterAt(GlobalData globalData, int position)
    {
        TheCostOfDiseaseVars vars = globalData.TheCostOfDiseaseVars;

        for (int x = 0; x <= position; ++x)
        {
            if (vars.HunterOrder[x] != string.Empty) continue;
            List<string> remaining = globalData.GetActivePlayers().Except(vars.HunterOrder).ToList();
            vars.HunterOrder[x] = vars.RandomElement(remaining, _RND_HUNTERS + x);
        }

        return vars.HunterOrder[position];
    }

    // 0 - 3 first hunt, 4 - 7 second hunt (North, East, West, South)
    private static int HuntRewardAt(GlobalData globalData, int position)
    {
        TheCostOfDiseaseVars vars = globalData.TheCostOfDiseaseVars;
        if (vars.HuntRewards[position] >= 0) return vars.HuntRewards[position];

        List<int> remaining = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7 }.Except(vars.HuntRewards).ToList();
        vars.HuntRewards[position] = vars.RandomElement(remaining, position == 7 ? _RND_HUNT_REWARD_LAST : _RND_HUNT_REWARDS + position);
        return vars.HuntRewards[position];
    }

    private static void CharityAwardGood(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddHandStorybookToNoJump(globalData.TheCostOfDiseaseVars.Charity + " III");
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, globalData.TheCostOfDiseaseVars.Charity));
        globalData.ActiveWindow.AddClickHereToContinue(CharityAwardGood_0);
    }

    private static void CharityAwardGood_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S1_EstateUpgradeBACK, PopUpButton.Confirm, CureMoonSick1);
    }

    private static void CureMoonSick1(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithCondition(0, () => globalData.Years == Years.Middle));
        globalData.ActiveWindow.AddClickHereToContinue(CureMoonSick1_0);
    }

    private static void CureMoonSick1_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.CompulsionBack, PopUpButton.Confirm,
            globalData.Years == Years.Middle ? Huntround : MayorResolveHunters);
    }

    private static void MayorResolveHunters(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddHandStorybookToNoJump(globalData.TheCostOfDiseaseVars.Mayor + " III");
        globalData.ActiveWindow.AddDefaultContent(content => content
                                                            .FormatWithReplacement(0, globalData.TheCostOfDiseaseVars.Mayor)
                                                            .FormatWithReplacement(1, globalData.TownName));
        globalData.ActiveWindow.AddClickHereToContinue(MayorResolveHunters_0);
    }

    private static void MayorResolveHunters_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S1_MayorCoin, PopUpButton.Confirm, Huntround,
            content => content
                      .FormatWithReplacement(0, globalData.TheCostOfDiseaseVars.Mayor)
                      .FormatWithReplacement(1, globalData.TheCostOfDiseaseVars.RandomElement([1, 2], _RND_MAYOR_RESOLVE_VP).ToString()));
    }

    private static void Huntround(GlobalData globalData)
    {
        string[] hunters = CurrentHunters(globalData);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content
                                                            .FormatWithReplacement(0, hunters[0])
                                                            .FormatWithReplacement(1, hunters[1]));
        globalData.ActiveWindow.AddClickHereToContinue(Huntround_0);
    }

    private static void Huntround_0(GlobalData globalData)
    {
        string[] hunters = CurrentHunters(globalData);

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.Spouse_Servant, PopUpButton.Confirm, HuntersChoice,
            content => content
                      .FormatWithReplacement(0, hunters[0])
                      .FormatWithReplacement(1, hunters[1]));
    }

    private static void HuntersChoice(GlobalData globalData)
    {
        globalData.SaveToUndo();

        TheCostOfDiseaseVars vars    = globalData.TheCostOfDiseaseVars;
        int                  hunt    = HuntIndex(globalData);
        string[]             hunters = CurrentHunters(globalData);
        string               chooser = vars.RandomElement(hunters.ToList(), _RND_HUNT_CHOOSER + hunt * 2);
        vars.HuntName = string.Empty; // drawn in HuntNight, where it is shown

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle(content => content.FormatWithIndex(0, hunt));
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, chooser));
        globalData.ActiveWindow.AddNextContentWithLinks(1, [HuntersChoice_North, HuntersChoice_East, HuntersChoice_West, HuntersChoice_South], false,
            content => content
                      .FormatWithIndex(0, HuntRewardAt(globalData, hunt * 4))
                      .FormatWithIndex(1, HuntRewardAt(globalData, hunt * 4 + 1))
                      .FormatWithIndex(2, HuntRewardAt(globalData, hunt * 4 + 2)));
    }

    private static void HuntersChoice_North(GlobalData globalData) => HuntersChoiceDirection(0, globalData);
    private static void HuntersChoice_East(GlobalData  globalData) => HuntersChoiceDirection(1, globalData);
    private static void HuntersChoice_West(GlobalData  globalData) => HuntersChoiceDirection(2, globalData);
    private static void HuntersChoice_South(GlobalData globalData) => HuntersChoiceDirection(3, globalData);

    private static void HuntersChoiceDirection(int direction, GlobalData globalData)
    {
        TheCostOfDiseaseVars vars = globalData.TheCostOfDiseaseVars;
        vars.HuntDirection = direction;
        vars.HuntReward    = HuntRewardAt(globalData, HuntIndex(globalData) * 4 + direction);
        vars.HuntBeast     = vars.RandomElement(_DirectionMonsters[direction].ToList(), _RND_HUNT_MONSTER + direction);
        HuntNight(globalData);
    }

    private static void HuntNight(GlobalData globalData)
    {
        TheCostOfDiseaseVars vars = globalData.TheCostOfDiseaseVars;

        globalData.SaveToUndo();

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle(content => content.FormatWithIndex(0, vars.HuntBeast));
        globalData.ActiveWindow.AddNextContent(1 + vars.HuntDirection, false, content => content.FormatWithReplacement(0, globalData.TownName));
        globalData.ActiveWindow.AddNextContent(10 + vars.HuntBeast, true, content =>
        {
            // Only some beasts name a hunter - draw the name when it is shown
            if (content.Contains("{{0=") && vars.HuntName == string.Empty) vars.HuntName = vars.RandomElement(CurrentHunters(globalData).ToList(), _RND_HUNT_CHOOSER + HuntIndex(globalData) * 2 + 1);
            return content.FormatWithReplacement(0, vars.HuntName);
        });
        globalData.ActiveWindow.AddNextContent(20, false, content => content.FormatWithIndex(0, vars.HuntReward));
        globalData.ActiveWindow.AddNextContentWithLinks(21, [HuntSuccess, HuntFail]);
    }

    private static void HuntSuccess(GlobalData globalData)
    {
        globalData.SaveToUndo();

        TheCostOfDiseaseVars vars = globalData.TheCostOfDiseaseVars;
        int                  hunt = HuntIndex(globalData);
        vars.HuntCount += 1;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle(content => content.FormatWithIndex(0, vars.RandomElement([0, 1, 2, 3, 4], _RND_HUNT_SUCCESS + hunt)));
        globalData.ActiveWindow.AddDefaultContent(content => content
                                                            .FormatWithIndex(0, vars.HuntBeast)
                                                            .FormatWithIndex(1, vars.RandomElement([0, 1, 2], _RND_HUNT_SUCCESS + 2 + hunt)));
        globalData.ActiveWindow.AddClickHereToContinue(HuntSuccess_0);
    }

    private static void HuntSuccess_0(GlobalData globalData)
    {
        int hunt = HuntIndex(globalData);

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.Creepy_Icon, PopUpButton.Confirm, hunt == 0 ? Prosperity : HuntSuccessCheck,
            content => content.FormatWithIndex(0, globalData.TheCostOfDiseaseVars.RandomElement([0, 1, 2], _RND_HUNT_SUCCESS + 4 + hunt)));
    }

    private static void HuntFail(GlobalData globalData)
    {
        TheCostOfDiseaseVars vars = globalData.TheCostOfDiseaseVars;
        int                  hunt = HuntIndex(globalData);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle(content => content.FormatWithIndex(0, vars.RandomElement([0, 1, 2, 3, 4], _RND_HUNT_FAIL + hunt)));
        globalData.ActiveWindow.AddDefaultContent(content => content
                                                            .FormatWithIndex(0, vars.HuntBeast)
                                                            .FormatWithIndex(1, vars.RandomElement([0, 1, 2], _RND_HUNT_FAIL + 2 + hunt)));
        globalData.ActiveWindow.AddClickHereToContinue(HuntFail_0);
    }

    private static void HuntFail_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.Servant, PopUpButton.Confirm, HuntIndex(globalData) == 0 ? Prosperity : HuntSuccessCheck);
    }

    private static void HuntSuccessCheck(GlobalData globalData)
    {
        TheCostOfDiseaseVars vars = globalData.TheCostOfDiseaseVars;

        bool success = vars.HuntCount switch
        {
            >= 2 => true,
            1    => vars.RandomBool(_RND_HUNT_CHECK),
            _    => false
        };

        if (success) HuntSuccessCheckSuccess(globalData);
        else HuntSuccessCheckFailure(globalData);
    }

    private static void HuntSuccessCheckSuccess(GlobalData globalData)
    {
        TheCostOfDiseaseVars vars = globalData.TheCostOfDiseaseVars;

        globalData.SaveToUndo();
        vars.Overrun = ExtendedBool.False;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle(content => content.FormatWithIndex(0, vars.RandomElement([0, 1], _RND_HUNT_CHECK + 1)));
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, vars.HuntCount.ToString()));
        globalData.ActiveWindow.AddClickHereToContinue(HuntSuccessCheckSuccess_0);
    }

    private static void HuntSuccessCheckSuccess_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S1_MastersStudy, PopUpButton.Confirm, Prosperity);
    }

    private static void HuntSuccessCheckFailure(GlobalData globalData)
    {
        TheCostOfDiseaseVars vars = globalData.TheCostOfDiseaseVars;

        globalData.SaveToUndo();
        vars.Overrun = ExtendedBool.True;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle(content => content.FormatWithIndex(0, vars.RandomElement([0, 1], _RND_HUNT_CHECK + 2)));
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, globalData.TownName));
        globalData.ActiveWindow.AddClickHereToContinue(ProsperityReturnToEvil);
    }

    #endregion Fraternity of Hunters Events
}
