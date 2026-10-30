namespace MyFathersWorkApp;

public static partial class TheCostOfDisease
{
    #region Scoring

    private static void Scoring(GlobalData globalData)
    {
        TheCostOfDiseaseVars vars = globalData.TheCostOfDiseaseVars;

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        if (vars.Lycan) globalData.ActiveWindow.AddNextContent(1);
        if (vars.Immort) globalData.ActiveWindow.AddNextContent(2);
        globalData.ActiveWindow.AddNextContentWithLinks(3, [FinalScoring.Start], true);
    }

    #endregion Scoring

    #region Endings

    public static void FinalEnding(GlobalData globalData)
    {
        TheCostOfDiseaseVars vars = globalData.TheCostOfDiseaseVars;

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.MarkFinalEnding();
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
