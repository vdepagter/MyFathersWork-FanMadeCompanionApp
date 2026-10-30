namespace MyFathersWorkApp;

public static partial class FearOfTheUnknown
{
    private const int _POLICY_TAXES       = 1;
    private const int _POLICY_NATIONALIST = 2;

    // Score change of each answer of the Asylum's assessment (AssessmentQuestion_Content1 - 9) and of the Count's (11 - 18)
    private static readonly int[][] _ASYLUM_ANSWERS =
    [
        [1, 0, 1], [0, 1, 1, 0], [1, 1, 0], [1, 0, 1, 0], [1, 0, 1], [1, 1, 0, 1], [0, 0, 1], [1, 0, 0, 1], [1, 0, 1, 1]
    ];

    private static readonly int[][] _COUNT_ANSWERS =
    [
        [1, 0, 0, 0], [-1, 0, 1], [1, -1, 0, 1], [1, -1, 0], [-1, 1, 1, 0], [1, 0, 0, 1], [1, 1, -1], [1, -1]
    ];

    public static void Liberal(GlobalData globalData)
    {
        // Round 19 -> Liberal
        // Round 20 -> Liberal2
        // Round 21 -> Liberal3
        FearOfTheUnknownVars vars = Vars(globalData);

        vars.HubId           = FearOfTheUnknownHubId.Liberal;
        globalData.ActiveHub = new GameplayHub(globalData);
        globalData.ActiveHub.SetDefaultTitle();
        globalData.ActiveHub.SetSubtitle(globalData.Years);

        const string       servantRuleSection = "ServantRule";
        GameplayHubSection servantRule        = globalData.ActiveHub.AddSection(servantRuleSection, true);
        servantRule.AddDefaultContent(servantRuleSection);

        const string       rehabilitationSection = "Rehabilitation";
        GameplayHubSection rehabilitation        = globalData.ActiveHub.AddSection(rehabilitationSection);
        rehabilitation.AddDefaultContent(rehabilitationSection);

        const string       focusSection = "Focus";
        GameplayHubSection focus        = globalData.ActiveHub.AddSection(focusSection, true);
        focus.AddDefaultContent(focusSection);
        focus.AddSpecialClickHere(focusSection, AdmittanceLog);

        const string       huntSection = "Hunt";
        GameplayHubSection hunt        = globalData.ActiveHub.AddSection(huntSection);
        hunt.ReplaceShouldShow(() => HuntActive(globalData));
        hunt.AddDefaultContent(huntSection);
        hunt.AddSpecialClickHere(huntSection, HuntExplanation);

        const string       programSection = "Program";
        GameplayHubSection program        = globalData.ActiveHub.AddSection(programSection, true);
        program.ReplaceShouldShow(() => globalData.Years != Years.Early);
        program.FormatTitle(title => title.FormatWithReplacement(0, vars.ProgramName));
        program.AddDefaultContent(programSection, content => content.FormatWithIndex(0, vars.ProgramType).FormatWithCondition(1, () => globalData.PlayersNum >= 4));

        GameplayHubSection nextRound = globalData.ActiveHub.AddSection(string.Empty);
        nextRound.ReplaceShouldShow(() => globalData.Years is Years.Early or Years.Middle);
        nextRound.AddClickHereContinueNextRound(Liberal_0);

        globalData.ActiveHub.AddEndOfGenerationSection(Liberal_0);
    }

    private static void Liberal_0(GlobalData globalData)
    {
        bool hunt = HuntActive(globalData);

        globalData.ShowEndOfRoundPopUp(globalData.Years switch
        {
            Years.Early  => hunt ? Expedition : AidToTheInstitute,
            Years.Middle => hunt ? Expedition : Round20Event,
            Years.Late   => hunt ? Expedition : MenagerieAward,
            _            => _ => { }
        });
    }

    #region Intro

    private static void LiberalIntro(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        globalData.SaveToUndo();
        vars.Path        = FearOfTheUnknownHubId.Liberal;
        vars.Expedition  = false;
        vars.AsylumCount = 0; // from now on the number of admissions to the Asylum

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.SetVoiceOver("LiberalIntro");
        globalData.ActiveWindow.AddGameplayTitle(GlobalTags.Gameplay_Generation_III);
        globalData.ActiveWindow.AddDefaultSubtitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, RandomPlayer(globalData)));
        globalData.ActiveWindow.AddClickHereToContinue(HouseForPrivateUse);
    }

    // Originally LiberalReplace
    private static void UnderstandingAndSupport(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, RandomPlayer(globalData)).FormatWithCondition(1, () => Vars(globalData).Jail == 2));
        globalData.ActiveWindow.AddClickHereToContinue(UnderstandingAndSupport_0);
    }

    private static void UnderstandingAndSupport_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S2_RehabilitationToken, PopUpButton.Confirm, ObsessionsResolve,
            content => content.FormatWithReplacement(0, Vars(globalData).Tracker.ToString()));
    }

    private static void LiberalSetup(GlobalData globalData)
    {
        bool hunt = Vars(globalData).CreatureMade == _CREATURE_NOT_CREATED;

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, hunt ? PopUpIcon.S2_HuntTokenFRONT : PopUpIcon.VillageChronicleCover, PopUpButton.Confirm, Liberal,
            content => content.FormatWithCondition(0, () => hunt).FormatWithCondition(1, () => hunt && globalData.PlayersNum == 3));
    }

    #endregion Intro

    #region The Asylum

    // Originally AsylumTreatment: a player reached the Rehabilitation token
    private static void AdmittanceLog(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle(content => content
                                                           .FormatWithReplacement(0, (151 + vars.Flavor("AdmittanceLog" + vars.AsylumAdmissions, 2850)).ToString())
                                                           .FormatWithIndex(1, vars.Flavor("AdmittanceLogLetter" + vars.AsylumAdmissions, 4)));
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddAllPlayersNamesAsOptions(player =>
        {
            vars.Patient          =  player;
            vars.AsylumAdmissions += 1;
            PatientEntry(globalData);
        });
    }

    private static int AdmissionSlot(GlobalData globalData) => Math.Min(Vars(globalData).AsylumAdmissions - 1, 9);

    // Originally AsylumTreatmentB
    private static void PatientEntry(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);
        string               key  = "PatientEntry" + vars.AsylumAdmissions;

        globalData.SaveToUndo();
        vars.AsylumCount += 1;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle(content => content.FormatWithReplacement(0, vars.Patient));
        globalData.ActiveWindow.AddDefaultContent(content =>
        {
            content = Either(globalData, 3, 4, key)(content);
            content = Either(globalData, 4, 4, key)(content);
            content = Either(globalData, 5, 3, key)(content);
            content = Either(globalData, 6, 4, key)(content);
            return content
                  .FormatWithReplacement(0, (151 + vars.Flavor(key + "#case", 2850)).ToString())
                  .FormatWithIndex(1, vars.Flavor(key + "#letter", 4))
                  .FormatWithReplacement(2, vars.Patient)
                  .FormatWithCondition(7, () => vars.Cure == ExtendedBool.True);
        });
        globalData.ActiveWindow.AddNextContentWithLinks(1, [Discharged, Assessment], true);
    }

    // Originally AsylumTreatmentBPay
    private static void Discharged(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Vars(globalData).Patient));
        globalData.ActiveWindow.AddClickHereToContinue(Discharged_0);
    }

    private static void Discharged_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S2_RehabilitationToken, PopUpButton.Confirm, Liberal);
    }

    // Originally AsylumInterviewBegin: four questions, drawn from the Asylum's (or the Count's) questionnaire
    private static void Assessment(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        globalData.SaveToUndo();
        vars.AsylumScore     = 0;
        vars.AsylumStep      = 0;
        vars.AsylumQuestions = [-1, -1, -1, -1];

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithCondition(0, () => vars.Cure == ExtendedBool.True));
        globalData.ActiveWindow.AddClickHereToContinue(AssessmentQuestion);
    }

    // Originally AsylumTest1 / AsylumQuestion1 - 9 / CountQuestion1 - 8
    private static void AssessmentQuestion(GlobalData globalData)
    {
        FearOfTheUnknownVars vars  = Vars(globalData);
        bool                 count = vars.Cure == ExtendedBool.True;
        int                  step  = vars.AsylumStep;

        globalData.SaveToUndo();

        if (vars.AsylumQuestions[step] < 0)
        {
            List<int> remaining = Enumerable.Range(0, count ? _COUNT_ANSWERS.Length : _ASYLUM_ANSWERS.Length).Except(vars.AsylumQuestions).ToList();
            vars.AsylumQuestions[step] = vars.RandomElement(remaining, _RND_ASYLUM_QUESTION + AdmissionSlot(globalData) * 4 + step);
        }

        int   question = vars.AsylumQuestions[step];
        int[] answers  = count ? _COUNT_ANSWERS[question] : _ASYLUM_ANSWERS[question];

        List<Action<GlobalData>?> callbacks = answers.Select(Action<GlobalData>? (change) => data =>
        {
            Vars(data).AsylumScore += change;
            Vars(data).AsylumStep  += 1;

            if (Vars(data).AsylumStep < 4) AssessmentQuestion(data);
            else if (Vars(data).Cure == ExtendedBool.True) CountAssessmentResults(data);
            else AssessmentResults(data);
        }).ToList();

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle(content => content.FormatWithReplacement(0, (step + 1).ToString()));
        globalData.ActiveWindow.AddNextContentWithLinks((count ? 11 : 1) + question, callbacks);
    }

    // Originally AsylumTestResults
    private static void AssessmentResults(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);
        string               key  = "AssessmentResults" + vars.AsylumAdmissions;

        globalData.SaveToUndo();
        int outcome = vars.AsylumScore <= 3 ? vars.RandomElement([1, 2, 3, 4, 5], _RND_ASYLUM_OUTCOME + AdmissionSlot(globalData)) : 6;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle(content => content.FormatWithReplacement(0, vars.Patient));
        globalData.ActiveWindow.AddDefaultContent(content => Either(globalData, 1, 4, key)(Either(globalData, 0, 5, key)(content)));
        globalData.ActiveWindow.AddNextContent(outcome, true, content => content.FormatWithReplacement(0, vars.Patient));
        globalData.ActiveWindow.AddClickHereToContinue(data =>
        {
            Vars(data).AsylumOutcome = outcome;
            AssessmentResults_0(data);
        });
    }

    private static void AssessmentResults_0(GlobalData globalData)
    {
        int outcome = Vars(globalData).AsylumOutcome;

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, outcome switch
        {
            1 => PopUpIcon.Money_Icon,
            2 => PopUpIcon.MaladjustmentBack,
            3 => PopUpIcon.Caretaker,
            4 => PopUpIcon.MaladjustmentBack,
            5 => PopUpIcon.Caretaker,
            _ => PopUpIcon.Self
        }, PopUpButton.Confirm, Liberal, string.Empty);

        globalData.ActivePopup.ReplaceDescription(Tag(globalData, "AssessmentResults_0_Content" + outcome));
    }

    // Originally CountTestResults: the Count now runs the Asylum
    private static void CountAssessmentResults(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        int outcome = vars.Patient == vars.CurePlayer ? 1 : vars.AsylumScore >= 3 ? 2 : 3;

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle(content => content.FormatWithReplacement(0, vars.Patient));
        globalData.ActiveWindow.AddNextContent(outcome, false, content => content.FormatWithReplacement(0, vars.Patient));
        globalData.ActiveWindow.AddClickHereToContinue(data =>
        {
            Vars(data).AsylumOutcome = outcome;
            CountAssessmentResults_0(data);
        });
    }

    private static void CountAssessmentResults_0(GlobalData globalData)
    {
        int outcome = Vars(globalData).AsylumOutcome;

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, outcome == 1 ? PopUpIcon.S2_BlessingOfTheStrigoi : PopUpIcon.ScoreTrackMarker, PopUpButton.Confirm, Liberal,
            string.Empty);
        globalData.ActivePopup.ReplaceDescription(Tag(globalData, "CountAssessmentResults_0_Content" + outcome));
    }

    #endregion The Asylum

    #region End of the Early Years - The Program

    // Originally LiberalEvent3
    private static void AidToTheInstitute(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddCustomContent(LetterHeader(globalData));
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(ProgramBid);
    }

    // Originally LiberalEvent3b
    private static void ProgramBid(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.SetCue(SoundCue.SpecialEvent);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereAfterBid(ProgramFocus);
    }

    // Originally LiberalEvent3Res
    private static void ProgramFocus(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [data => ChooseProgram(0, data), data => ChooseProgram(1, data), data => ChooseProgram(2, data), data => ChooseProgram(3, data)], true);
    }

    private static void ChooseProgram(int type, GlobalData globalData)
    {
        Vars(globalData).ProgramType = type;
        ProgramName(globalData);
    }

    private static void ProgramName(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveInputPopup = new GameplayInputPopup(globalData, string.Empty, PopUpButton.Confirm, value => value.Trim().Length is > 0 and <= 40, value =>
        {
            Vars(globalData).ProgramName = value.Trim();
            ProgramNews(globalData);
        });
    }

    // Originally LiberalEvent3Resa
    private static void ProgramNews(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle(content => content.FormatWithReplacement(0, vars.ProgramName));
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Newspaper(globalData)));
        globalData.ActiveWindow.AddNextContent(1 + vars.ProgramType);
        globalData.ActiveWindow.AddNextContent(5, true, content => content.FormatWithReplacement(0, globalData.TownName));
        globalData.ActiveWindow.AddClickHereToContinue(ProgramNews_0);
    }

    private static void ProgramNews_0(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S2_EstateUpgradeBACK, PopUpButton.Confirm, Liberal,
            content => content.FormatWithCondition(0, () => globalData.PlayersNum >= 4).FormatWithIndex(1, vars.ProgramType).FormatWithCondition(2, () => globalData.PlayersNum >= 4));
    }

    // Originally EssentialSalts: after the Creature was killed
    private static void EssentialSalts(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddCustomContent(JournalHeader(globalData));
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(TheChoice);
    }

    // Originally EssentialSalts1. When all refuse, the source chose between "The Risk is Too Great" and the Late Years hub,
    // skipping the story events of the round - the second outcome continues the story without the reward here.
    private static void TheChoice(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [data => Vars(data).RandomElement([AriseMyLove, AriseMyLoveTwisted], _RND_SALTS).Invoke(data),
            data => Vars(data).RandomElement([RiskTooGreat, SaltsNext], _RND_SALTS_REFUSED).Invoke(data)], true);
    }

    private static void SaltsNext(GlobalData globalData)
    {
        if (EndedYears(globalData) == Years.Early) AidToTheInstitute(globalData);
        else Round20Event(globalData);
    }

    // Originally EssentialCons
    private static void AriseMyLove(GlobalData globalData)
    {
        FearOfTheUnknownVars vars    = Vars(globalData);
        bool                 perfect = vars.RandomBool(_RND_SALTS_SPOUSE);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, RandomPlayer(globalData, nameof(EssentialSalts))));
        globalData.ActiveWindow.AddNextContent(perfect ? 1 : 2, true);
        globalData.ActiveWindow.AddClickHereToContinue(AriseMyLove_0);
    }

    private static void AriseMyLove_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.Spouse, PopUpButton.Confirm, SaltsNext,
            content => content.FormatWithCondition(0, () => !Vars(globalData).RandomBool(_RND_SALTS_SPOUSE)));
    }

    // Originally EssentialSaltsRes
    private static void AriseMyLoveTwisted(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, RandomPlayer(globalData, nameof(EssentialSalts))));
        globalData.ActiveWindow.AddClickHereToContinue(AriseMyLoveTwisted_0);
    }

    private static void AriseMyLoveTwisted_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.GainCaretakerFromLost, PopUpButton.Confirm, SaltsNext);
    }

    // Originally EssentialSaltsNo
    private static void RiskTooGreat(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, RandomPlayer(globalData, nameof(EssentialSalts))));
        globalData.ActiveWindow.AddClickHereToContinue(RiskTooGreat_0);
    }

    private static void RiskTooGreat_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.Creepy_Icon, PopUpButton.Confirm, SaltsNext);
    }

    #endregion End of the Early Years - The Program

    #region End of the Middle Years - Politics

    private static void Round20Event(GlobalData globalData)
    {
        Vars(globalData).RandomElement([EconomicStimulus, DeathAndTaxes], _RND_LIBERAL_EVENT).Invoke(globalData);
    }

    private static int BribeCost(GlobalData globalData) => globalData.PlayersNum switch
    {
        2 => 1,
        3 => 2,
        _ => Vars(globalData).RandomElement([2, 3], _RND_BRIBE)
    };

    // Originally LiberalEvent
    private static void EconomicStimulus(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddCustomContent(LetterHeader(globalData));
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(globalData.PlayersNum == 2 ? PolicyBid : PolicyVote);
    }

    // Originally 2PLiberalEvent
    private static void PolicyBid(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.SetCue(SoundCue.SpecialEvent);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, AsylumUpgradeName(globalData)));
        globalData.ActiveWindow.AddClickHereAfterBid(PolicyBidResolved);
    }

    // Originally 2PLiberalEvent1
    private static void PolicyBidResolved(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [data => ChoosePolicy(_POLICY_TAXES, data), data => ChoosePolicy(_POLICY_NATIONALIST, data)], true);
    }

    // Originally LiberalEvent1
    private static void PolicyVote(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.SetCue(SoundCue.SpecialEvent);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, AsylumUpgradeName(globalData)));
        globalData.ActiveWindow.AddClickHereAfterVote(PolicyVoteResolved);
    }

    // Originally LiberalEvent1a
    private static void PolicyVoteResolved(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [data => ChoosePolicy(_POLICY_TAXES, data), data => ChoosePolicy(_POLICY_NATIONALIST, data)]);
    }

    private static void ChoosePolicy(int policy, GlobalData globalData)
    {
        Vars(globalData).Policy = policy;
        SurprisingRebuke(globalData);
    }

    // Originally LiberalEvent2
    private static void SurprisingRebuke(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content
                                                             .FormatWithReplacement(0, RandomPlayer(globalData, nameof(EconomicStimulus)))
                                                             .FormatWithReplacement(1, BribeCost(globalData).ToString()));
        globalData.ActiveWindow.AddClickHereToContinue(Bribe);
    }

    // Originally LiberalEvent2a
    private static void Bribe(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.SetCue(SoundCue.SpecialEvent);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, BribeCost(globalData).ToString()));
        globalData.ActiveWindow.AddClickHereAfterBid(BribeResolved);
    }

    // Originally LiberalEvent2ab
    private static void BribeResolved(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, BribeCost(globalData).ToString()));
        globalData.ActiveWindow.AddNextContentWithLinks(1, [data => BribeResult(true, data), data => BribeResult(false, data)], true);
    }

    private static void BribeResult(bool succeeded, GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);
        vars.Bribe = succeeded ? ExtendedBool.True : ExtendedBool.False;

        if (vars.CreatureMade == _CREATURE_CREATED)
        {
            CreatureBenefactor(globalData);
            return;
        }

        // Without the bribe the people decide
        if (!succeeded) vars.Policy = vars.RandomElement([_POLICY_TAXES, _POLICY_NATIONALIST], _RND_POLICY);

        PolicyEnacted(globalData);
    }

    private static void PolicyEnacted(GlobalData globalData)
    {
        if (Vars(globalData).Policy == _POLICY_TAXES) EstateTaxes(globalData);
        else EthnicSeparatism(globalData);
    }

    // Originally LiberalEventGood: the Creature, now a banker, pays the bribe
    private static void CreatureBenefactor(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, RandomPlayer(globalData, nameof(EconomicStimulus))));
        globalData.ActiveWindow.AddClickHereToContinue(CreatureBenefactor_0);
    }

    private static void CreatureBenefactor_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.Money_Icon, PopUpButton.Confirm, data =>
        {
            Vars(data).Bribe = ExtendedBool.True;
            PolicyEnacted(data);
        });
    }

    // Originally LiberalEventA
    private static void EstateTaxes(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content
                                                             .FormatWithReplacement(0, RandomPlayer(globalData, nameof(EconomicStimulus)))
                                                             .FormatWithCondition(1, () => Vars(globalData).Bribe == ExtendedBool.False));
        globalData.ActiveWindow.AddClickHereToContinue(EstateTaxes_0);
    }

    private static void EstateTaxes_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.Money_Icon, PopUpButton.Confirm, StartDeathCollection);
    }

    // Originally LiberalEventB
    private static void EthnicSeparatism(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content
                                                             .FormatWithReplacement(0, RandomPlayer(globalData, nameof(EconomicStimulus)))
                                                             .FormatWithCondition(1, () => Vars(globalData).Bribe == ExtendedBool.False));
        globalData.ActiveWindow.AddClickHereToContinue(EthnicSeparatism_0);
    }

    private static void EthnicSeparatism_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S2_EstateUpgradeBACK, PopUpButton.Confirm, StartDeathCollection,
            content => content.FormatWithReplacement(0, AsylumUpgradeName(globalData)));
    }

    // Originally LiberalTaxes
    private static void DeathAndTaxes(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddCustomContent(LetterHeader(globalData));
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(TaxesChoice);
    }

    // Originally LiberalTaxes1
    private static void TaxesChoice(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [Vars(globalData).CreatureMade == _CREATURE_CREATED ? TaxesExcused : SeizureOfGoods]);
    }

    // Originally TaxesEventGood
    private static void TaxesExcused(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content
                                                             .FormatWithReplacement(0, RandomPlayer(globalData, nameof(DeathAndTaxes)))
                                                             .FormatWithReplacement(1, globalData.TownName));
        globalData.ActiveWindow.AddClickHereToContinue(TaxesExcused_0);
    }

    private static void TaxesExcused_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.Creepy_Icon, PopUpButton.Confirm, Liberal);
    }

    // Originally LiberalTaxes2
    private static void SeizureOfGoods(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, RandomPlayer(globalData, nameof(DeathAndTaxes))));
        globalData.ActiveWindow.AddClickHereToContinue(SeizureOfGoods_0);
    }

    private static void SeizureOfGoods_0(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.ScoreTrackMarker, PopUpButton.Confirm, Liberal, content => content
                                                                                                                                                           .FormatWithReplacement(0, vars.RandomInRange(1, 3, _RND_TAXES_VP).ToString())
                                                                                                                                                           .FormatWithReplacement(1, vars.RandomInRange(1, 2, _RND_TAXES_LOSS).ToString()));
    }

    #endregion End of the Middle Years - Politics

    #region End of Generation III - Liberal

    // Originally LiberalBonus
    private static void MenagerieAward(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        globalData.SaveToUndo();
        vars.Ending = vars.AsylumCount >= (globalData.PlayersNum <= 3 ? 2 : 3) ? 7 : 8;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle(content => content.FormatWithReplacement(0, vars.ProgramName));
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithIndex(0, vars.ProgramType).FormatWithCondition(1, () => globalData.PlayersNum >= 4));
        globalData.ActiveWindow.AddNextContentWithLinks(1, [ObsessionResolution], true);
    }

    #endregion End of Generation III - Liberal
}
