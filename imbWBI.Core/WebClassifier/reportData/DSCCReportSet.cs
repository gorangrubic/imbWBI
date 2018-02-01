// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DSCCReportSet.cs" company="imbVeles" >
//
// Copyright (C) 2018 imbVeles
//
// This program is free software: you can redistribute it and/or modify
// it under the +terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see http://www.gnu.org/licenses/. 
// </copyright>
// <summary>
// Project: imbWBI.Core
// Author: Goran Grubic
// ------------------------------------------------------------------------------------------------------------------
// Project web site: http://blog.veles.rs
// GitHub: http://github.com/gorangrubic
// Mendeley profile: http://www.mendeley.com/profiles/goran-grubi2/
// ORCID ID: http://orcid.org/0000-0003-2673-9471
// Email: hardy@veles.rs
// </summary>
// ------------------------------------------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Collections.Generic;
using imbMiningContext.MCDocumentStructure;
using imbNLP.PartOfSpeech.flags.token;
using imbNLP.PartOfSpeech.pipeline.machine;
using imbNLP.PartOfSpeech.pipelineForPos.subject;
using imbNLP.PartOfSpeech.resourceProviders.core;
using imbNLP.PartOfSpeech.TFModels.webLemma;
using imbSCI.Core.extensions.data;
using imbSCI.DataComplex.extensions.data.schema;
using imbSCI.DataComplex.extensions.data.operations;
using imbSCI.DataComplex.extensions.data.formats;
using imbSCI.DataComplex.extensions.data.modify;
using imbSCI.DataComplex.extensions.data;
using imbSCI.Core.extensions.table;
using imbSCI.Core.files.folders;
using imbSCI.Core.math;
using imbSCI.Core.reporting;
using imbSCI.Data;
using imbSCI.DataComplex.tf_idf;
using imbWBI.Core.WebClassifier.core;
using System.Data;
using System.Text;
using imbNLP.PartOfSpeech.TFModels.webLemma.core;
using imbNLP.PartOfSpeech.pipelineForPos.subject;
using imbWBI.Core.WebClassifier.validation;
using System.ComponentModel;
using imbSCI.Core.attributes;
using imbWBI.Core.WebClassifier.wlfClassifier;
using imbSCI.Data.collection.nested;
using imbSCI.DataComplex.tables;
using imbWBI.Core.WebClassifier.experiment;
using imbACE.Core;
using imbSCI.Core.math.classificationMetrics;
using imbWBI.Core.WebClassifier.cases;
using imbWBI.Core.math;

namespace imbWBI.Core.WebClassifier.reportData
{

    public class DSCCReportSet : Dictionary<kFoldValidationCase, DSCCReports>
    {

        public DSCCReportSet(IWebFVExtractor _extractor)
        {
            extractor = _extractor;
        }

        protected DocumentSetCaseCollectionReport topClassifierReport { get; set; } = null;
        protected Double highestF1Value { get; set; } = Double.MinValue;
        protected IWebFVExtractor extractor { get; set; } = null;

        public DocumentSetCaseCollectionReport meanClassifierReport { get; set; } = null;


        public DocumentSetCaseCollectionReport GetTopClassifierReport()
        {
            if (topClassifierReport == null) topClassifierReport = new DocumentSetCaseCollectionReport("null");
            topClassifierReport.Name = extractor.name + " - top F1";
            return topClassifierReport;
        }

        public static Boolean DOMAKE_MICROaverage = false;


        public void MakeReports(experimentExecutionContext context, folderNode folder)
        {
            meanClassifierReport = new DocumentSetCaseCollectionReport(extractor.name);

            aceDictionary2D<IWebPostClassifier, kFoldValidationCase, DocumentSetCaseCollectionReport> tempStructure = new aceDictionary2D<IWebPostClassifier, kFoldValidationCase, DocumentSetCaseCollectionReport>();

            DSCCReports firstCase = null;
            List<IWebPostClassifier> classifiers = new List<IWebPostClassifier>();

            foreach (var kFoldCasePair in this)
            {
                if (firstCase == null) firstCase = kFoldCasePair.Value;
                foreach (var pair in kFoldCasePair.Value.avgReports)
                {
                    tempStructure[pair.Key, kFoldCasePair.Key] = pair.Value;
                    if (!classifiers.Contains(pair.Key)) classifiers.Add(pair.Key);
                }
            }

           

           // DataSet dataSet = new DataSet(context.setup.name);

           

            // <---------- CREATING AVERAGE TABLE -----------------------------------------------------
            var tpAvgMacro = new DataTableTypeExtended<DocumentSetCaseCollectionReport>(context.setup.name + " summary", "Cross k-fold averages measures, fold-level measures are computed by macro-average method");
            var tpAvgMicro = new DataTableTypeExtended<DocumentSetCaseCollectionReport>(context.setup.name + " summary", "Cross k-fold averages measures, fold-level measures are computed by micro-average method");

            List<DocumentSetCaseCollectionReport> macroaverages = new List<DocumentSetCaseCollectionReport>();
            DataTableTypeExtended<DocumentSetCaseCollectionReport> EMperKFolds = new DataTableTypeExtended<DocumentSetCaseCollectionReport>(extractor.name + "_allReports");


            foreach (IWebPostClassifier classifier in classifiers)
            {
                // < ---- report on each classifier

                context.logger.log("-- producing report about [" + classifier.name + "]");
                //objectTable<DocumentSetCaseCollectionReport> tp = new objectTable<DocumentSetCaseCollectionReport>(nameof(DocumentSetCaseCollectionReport.Name), classifier + "_sum");

                

                DocumentSetCaseCollectionReport avg = new DocumentSetCaseCollectionReport(classifier.name + " macro-averaging, k-fold avg. ");

                DocumentSetCaseCollectionReport rep_eval = new DocumentSetCaseCollectionReport(classifier.name + " micro-averaging, k-fold avg.");
                
                rep_eval.Classifier = classifier.name;

                classificationEvalMetricSet metrics = new classificationEvalMetricSet();
                classificationEval eval = new classificationEval();
                //eval = metrics[classifier.name];

                Int32 c = 0;
                foreach (KeyValuePair<kFoldValidationCase, DSCCReports> kFoldCasePair in this)
                {
                    DocumentSetCaseCollectionReport rep = kFoldCasePair.Value.avgReports[classifier];
                    kFoldValidationCase vCase = kFoldCasePair.Key;

                    
                    classificationEvalMetricSet met = rep.GetSetMetrics();

                    if (met != null)
                    {
                        foreach (IDocumentSetClass cl in context.classes.GetClasses())
                        {

                            eval = eval + met[cl.name];
                        }
                    }

                    rep.Name = classifier.name + "_" + vCase.name;
                    avg.AddValues(rep);
                    EMperKFolds.AddRow(rep);
                    
                    c++;
                }

                rep_eval.AddValues(metrics, classificationMetricComputation.microAveraging);


                
                avg.Classifier = classifier.name;
                avg.DivideValues(c);

                // <<< detecting the best performed classifier in all evaluation folds
                if (avg.F1measure > highestF1Value)
                {
                    highestF1Value = avg.F1measure;
                    topClassifierReport = avg;
                }

                meanClassifierReport.AddValues(avg);
                

                // -----------------

                EMperKFolds.AddRow(avg);

                tpAvgMacro.AddRow(avg);

                macroaverages.Add(avg);

               if (DOMAKE_MICROaverage) tpAvgMicro.AddRow(rep_eval);
                // tp.Add(rep_eval);

                if (context.tools.operation.DoMakeReportForEachClassifier)
                {

                    DataTable cTable = EMperKFolds;
                    cTable.SetTitle($"{classifier.name} report");
                    cTable.SetDescription("Summary " + context.setup.validationSetup.k + "-fold validation report for [" + classifier.name + "]");


                    cTable.SetAdditionalInfoEntry("FV Extractor", extractor.name);
                    cTable.SetAdditionalInfoEntry("Classifier", classifier.name);
                    cTable.SetAdditionalInfoEntry("Class name", classifier.GetType().Name);

                    cTable.SetAdditionalInfoEntry("Correct", rep_eval.Correct);
                    cTable.SetAdditionalInfoEntry("Wrong", rep_eval.Wrong);

                    //cTable.SetAdditionalInfoEntry("Precision", rep_eval.Precision);
                    //cTable.SetAdditionalInfoEntry("Recall", rep_eval.Recall);
                    //cTable.SetAdditionalInfoEntry("F1", rep_eval.F1measure);

                    cTable.SetAdditionalInfoEntry("True Positives", metrics[classifier.name].truePositives);
                    cTable.SetAdditionalInfoEntry("False Negatives", metrics[classifier.name].falseNegatives);
                    cTable.SetAdditionalInfoEntry("False Positives", metrics[classifier.name].falsePositives);


                    cTable.AddExtra("Classifier: " + classifier.name + " [" + classifier.GetType().Name + "]");
                    var info = classifier.DescribeSelf();
                    info.ForEach(x => cTable.AddExtra(x));

                    cTable.AddExtra("-----------------------------------------------------------------------");

                    cTable.AddExtra("Precision, Recall and F1 measures expressed in this table are computed by macroaveraging shema");
                    //  output.CopyRowsFrom(cTable);


                    cTable.GetReportAndSave(folder, appManager.AppInfo, extractor.name + "_classifier_" + classifier.name);

                   // dataSet.AddTable(cTable);
                }
            }



            rangeFinderForDataTable rangerMacro = new rangeFinderForDataTable(tpAvgMacro, "Name");
            



            meanClassifierReport.DivideValues(classifiers.Count);
            if (macroaverages.Count > 0)
            {

                Double maxF1 = macroaverages.Max(x => x.F1measure);
                Double minF1 = macroaverages.Min(x => x.F1measure);

                List<String> minCaseNames = macroaverages.Where(x => x.F1measure == minF1).Select(x => x.Name).ToList();
                List<String> maxCaseNames = macroaverages.Where(x => x.F1measure == maxF1).Select(x => x.Name).ToList();


                var style = EMperKFolds.GetRowMetaSet().SetStyleForRowsWithValue<String>(DataRowInReportTypeEnum.dataHighlightA, nameof(DocumentSetCaseCollectionReport.Name), maxCaseNames);
                
                EMperKFolds.GetRowMetaSet().AddUnit(style);


              //  style = tpAvgMacro.GetRowMetaSet().SetStyleForRowsWithValue<String>(DataRowInReportTypeEnum.dataHighlightC, nameof(DocumentSetCaseCollectionReport.Name), minCaseNames);
                


                tpAvgMacro.SetAdditionalInfoEntry("FV Extractor", extractor.name);
                if (DOMAKE_MICROaverage) tpAvgMicro.SetAdditionalInfoEntry("FV Extractor", extractor.name);


                List<String> averageNames = macroaverages.Select(x => x.Name).ToList();
                var avg_style = EMperKFolds.GetRowMetaSet().SetStyleForRowsWithValue<String>(DataRowInReportTypeEnum.dataHighlightC, nameof(DocumentSetCaseCollectionReport.Name),averageNames);
                foreach (var x in averageNames)
                {
                    avg_style.AddMatch(x);
                }
            }
            
            // ::: ------------------------------------------------------------------------------------------------- ::: --------------------------------------------------------------------- ::: //

            tpAvgMacro.SetTitle($"{extractor.name} - macroaverage report");
            if (DOMAKE_MICROaverage) tpAvgMicro.SetTitle($"{extractor.name} - microaverage report");

            tpAvgMacro.AddExtra("Complete report on " + context.setup.validationSetup.k + "-fold validation FVE [" + extractor.name + "]");
             tpAvgMacro.AddExtra("Fold-level P, R and F1 measures are computed by macroaveraging method, values here are cross k-fold means.");

            if (DOMAKE_MICROaverage) tpAvgMicro.AddExtra("Complete " + context.setup.validationSetup.k + "-fold validation report for FVE [" + extractor.name + "]");
            if (DOMAKE_MICROaverage) tpAvgMicro.AddExtra("Fold-level P, R and F1 measures are computed by microaveraging method, values here are cross k-fold means.");

            context.AddExperimentInfo(tpAvgMacro);
            if (DOMAKE_MICROaverage) context.AddExperimentInfo(tpAvgMicro);

            tpAvgMacro.AddExtra(extractor.description);
            

            if (extractor is semanticFVExtractor)
            {
                semanticFVExtractor semExtractor = (semanticFVExtractor)extractor;

                semExtractor.termTableConstructor.DescribeSelf().ForEach(x => tpAvgMacro.AddExtra(x));
                semExtractor.CloudConstructor.DescribeSelf().ForEach(x => tpAvgMacro.AddExtra(x));
                semExtractor.termTableConstructor.DescribeSelf().ForEach(x => tpAvgMicro.AddExtra(x));
                semExtractor.CloudConstructor.DescribeSelf().ForEach(x => tpAvgMicro.AddExtra(x));
            }

            context.logger.log("-- producing summary reports on [" + extractor.name + "]");

            rangerMacro.AddRangeRows("Macroaverage ", tpAvgMacro, true, 
                imbSCI.Core.math.aggregation.dataPointAggregationType.min | imbSCI.Core.math.aggregation.dataPointAggregationType.max 
                | imbSCI.Core.math.aggregation.dataPointAggregationType.avg 
                | imbSCI.Core.math.aggregation.dataPointAggregationType.stdev);
            tpAvgMacro.GetReportAndSave(folder, appManager.AppInfo, extractor.name + "_macroaverage_report", true,true);


            EMperKFolds.AddExtra("The table shows average measures for each fold --- rows marked with colored background show averages for all folds, per classifier.");
            
            EMperKFolds.GetReportAndSave(folder, appManager.AppInfo, extractor.name + "_allFolds", true, true);

            if (DOMAKE_MICROaverage) tpAvgMicro.GetReportAndSave(folder, appManager.AppInfo, extractor.name + "_microaverage_report", true, true);
            //dataSet.GetReportVersion().serializeDataSet(extractor.name + "_classifiers_MultiSheetSummary", folder, imbSCI.Data.enums.reporting.dataTableExportEnum.excel, appManager.AppInfo);

        }

    }

}