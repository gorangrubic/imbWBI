// --------------------------------------------------------------------------------------------------------------------
// <copyright file="experimentManager.cs" company="imbVeles" >
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
using imbACE.Core.core;
using imbMiningContext.TFModels.ILRT;
using imbSCI.Core.files.folders;
using imbSCI.Core.reporting;
using imbSCI.Core.extensions.table;
using imbSCI.DataComplex.tables;
using imbSCI.DataComplex.extensions.data.operations;
using imbSCI.DataComplex.extensions.data.modify;
using imbSCI.DataComplex.extensions.data.formats;
using imbSCI.DataComplex.extensions.data.schema;
using imbSCI.DataComplex.extensions.data;
using imbSCI.DataComplex.extensions;
using imbWBI.Core.WebClassifier.cases;
using imbWBI.Core.WebClassifier.core;
using imbWBI.Core.WebClassifier.validation;
using imbWBI.Core.WebClassifier.wlfClassifier;
using imbWEM.Core.project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using imbACE.Core;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using imbSCI.Data.collection.nested;
using imbSCI.Core.files;
using imbSCI.Core.extensions.text;
using imbWBI.Core.WebClassifier.reportData;
using imbWBI.Core.math;
using imbACE.Core.core.exceptions;

namespace imbWBI.Core.WebClassifier.experiment
{
    public class experimentManager
    {
        /// <summary>
        /// Gets or sets the folder.
        /// </summary>
        /// <value>
        /// The folder.
        /// </value>
        public folderNode folder { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="experimentManager"/> class.
        /// </summary>
        /// <param name="_folder">The folder.</param>
        public experimentManager(folderNode _folder)
        {
            folder = _folder;
            
        }

        /// <summary>
        /// Runs the experiment.
        /// </summary>
        /// <param name="context">The context.</param>
        public void runExperiment(experimentExecutionContext context)
        {
            try
            {
                run(context);
            }
            catch (AggregateException ae)
            {
                context.errorNotes.LogException("Experiment exception: " + ae.Message, ae);

                
                imbACE.Services.terminal.aceTerminalInput.doBeepViaConsole(2000, 1000, 5);
                context.notes.SaveNote();
            }
            catch (Exception ex)
            {
                context.errorNotes.LogException("Experiment exception: " + ex.Message, ex);
                
                imbACE.Services.terminal.aceTerminalInput.doBeepViaConsole(2000, 1000, 5);
                context.notes.SaveNote();

            }
            

        }


        protected void runModel(experimentExecutionContext context, IWebFVExtractor model)
        {
           
            imbSCI.Core.screenOutputControl.logToConsoleControl.setAsOutput(context.logger, model.name);
        Int32 crashRetries = context.tools.operation.doRebootFVEOnCrashRetryLimit;
        aceDictionarySet<IDocumentSetClass, DocumentSetCaseCollection> casesByClasses = new aceDictionarySet<IDocumentSetClass, DocumentSetCaseCollection>();
            DSCCReportSet kFoldReport = new DSCCReportSet(model);
            var valCol = context.validationCollections[model.name];

            List<DocumentSetCaseCollectionSet> modelCaseResults = new List<DocumentSetCaseCollectionSet>();
            crashRetries = context.tools.operation.doRebootFVEOnCrashRetryLimit;
            while (crashRetries > 0)
            {
                try
                {






                    experimentNotes modelNotes = new experimentNotes(valCol.folder, "Fold-level experiment settings description notes");
                    modelNotes.AppendLine("# Notes on Feature Vector Extractor: " + model.name);

                    var nts = model.DescribeSelf();
                    nts.ForEach(x => modelNotes.AppendLine(x));

             



                    context.logger.log("Executing k-fold cases with model [" + model.name + "]");




                    valCol.DescribeSampleDistribution(modelNotes);

                    context.mainReport.valColVsModelVsSampleHash.Add("[" + model.name + "]".toWidthExact(20) + " [sample distribution hash: " + valCol.SampleDistributionHash + "]");

                    modelNotes.SaveNote();

                    ParallelOptions ops = new ParallelOptions();
                    ops.MaxDegreeOfParallelism = context.tools.operation.ParallelThreads;

                    Parallel.ForEach<kFoldValidationCase>(valCol.GetCases(), ops, valCase =>
                    {
                        model.DoFVEAndTraining(valCase, context.tools, context.logger); // <---------------------------------------------------------------------------------------   BUILDING FVE

                        DocumentSetCaseCollectionSet results = model.DoClassification(valCase, context.tools, context.logger);

                        if (!results.Any())
                        {
                            throw new aceScienceException("DoClassification for [" + model.name + "] returned no results!", null, model, "DoClassification " + model.name + " failed!", context);
                        }

                        foreach (var pair in results)
                        {
                            DocumentSetCaseCollection cls = pair.Value;
                            casesByClasses.Add(cls.setClass, cls);
                        }

                        valCase.evaluationResults = results;

                        if (context.tools.DoResultReporting)
                        {
                            context.logger.log("producing reports on k-Fold case [" + valCase.name + "]");
                            DSCCReports r = results.GetReports();

                            var sumMeans = r.GetAverageTable(context); //.GetReportAndSave(valCase.folder, appManager.AppInfo, "CrossValidation_" + valCase.name);
                            sumMeans.SetDescription("FVE report, aggregated for all categories - for fold [" + valCase.name + "]");


                            sumMeans.GetReportAndSave(valCase.folder, appManager.AppInfo, "CrossValidation_" + valCase.name, true, context.tools.operation.doReportsInParalell);

                            var fveAndCase = r.GetFullValidationTable(context);
                            fveAndCase.SetDescription("Per-category aggregate statistics, for each classifier, within fold [" + valCase.name + "], used for macro-averaging");
                            fveAndCase.GetReportAndSave(valCase.folder, appManager.AppInfo, "CrossValidation_extrainfo_" + valCase.name, true, context.tools.operation.doReportsInParalell);

                            var fullCaseReport = results.GetReportOnAllCases();


                            fullCaseReport.GetReportAndSave(valCase.folder, appManager.AppInfo, "FullReport_" + valCase.name, true, context.tools.operation.doReportsInParalell);

                            kFoldReport.Add(valCase, r);
                        }

                        context.logger.log("k-Fold case [" + valCase.name + "] completed");

                        context.notes.log("- - Experiment sequence for [" + valCase.name + "] fold completed");
                        if (context.tools.operation.doSaveKnowledgeForClasses)
                        {
                            valCase.knowledgeLibrary.SaveKnowledgeInstancesForClasses(valCase, context.logger);
                        }

                    });

                    foreach (var fold in valCol.GetCases()) //  Parallel.ForEach<kFoldValidationCase>(valCol.GetCases(), ops, valCase =>
                    {
                        modelCaseResults.Add(fold.evaluationResults);
                    }

                    crashRetries = 0;
                }
                catch (Exception ex)
                {
                    crashRetries--;
                    context.errorNotes.LogException("FVE Model crashed -- retries left [" + crashRetries + "] --- ", ex, model.name);
                    context.logger.log(":::: REPEATING the model [" + model.name + "] ::: CRASHED [" + ex.Message + "] ::: RETRIES [" + crashRetries + "]");
                    imbACE.Services.terminal.aceTerminalInput.doBeepViaConsole(1200, 1000, 1);
                    imbACE.Services.terminal.aceTerminalInput.doBeepViaConsole(2400, 1000, 1);
                    imbSCI.Core.screenOutputControl.logToConsoleControl.setAsOutput(context.logger, "RETRIES[" + crashRetries + "]");

                }


            }


            imbSCI.Core.screenOutputControl.logToConsoleControl.setAsOutput(context.logger, "Reporting");


            valCol.knowledgeLibrary.SaveCaseKnowledgeInstances(context.logger);

            // DocumentSetCaseCollection second = null;
            if (modelCaseResults.Any())
            {

                featureExtractionMetrics modelMetrics = new featureExtractionMetrics(model.name, "All");
                DataTableTypeExtended<featureExtractionMetrics> modelVsCategoryMetrics = new DataTableTypeExtended<featureExtractionMetrics>(model.name, "Model metrics per category");


                // <-------------------------------------- CATEGORIES REPORT ----------------------------------------------

                DataTable allTable = modelCaseResults.First()[0].GetReportTable(false, false).GetClonedShema<DataTable>(); ; //valCol.GetCases().First().evaluationResults[0].GetReportTable(false, false);


                rangeFinderForDataTable ranger = new rangeFinderForDataTable(allTable, "name");
                ranger.columnsToSignIn.Add("Case");

                foreach (KeyValuePair<IDocumentSetClass, aceConcurrentBag<DocumentSetCaseCollection>> pair in casesByClasses)
                {
                    DocumentSetCaseCollection first = null;
                    DataTable repTable = null;

                    ranger.prepareForNextAggregationBlock(allTable, "name");

                    foreach (DocumentSetCaseCollection cn in pair.Value)
                    {

                        foreach (var cni in cn)
                        {
                            if (cni != null)
                            {
                                cn.BuildRow(cni, allTable, false);
                            }
                        }

                    }

                    ranger.AddRangeRows(pair.Key.name, allTable, true, imbSCI.Core.math.aggregation.dataPointAggregationType.avg | imbSCI.Core.math.aggregation.dataPointAggregationType.stdev);

                    var categoryMetrics = new featureExtractionMetrics(model.name, pair.Key.name);
                    categoryMetrics.SetValues(ranger);

                    modelVsCategoryMetrics.AddRow(categoryMetrics);
                    modelMetrics.AddValues(categoryMetrics);

                    categoryMetrics.saveObjectToXML(valCol.folder.pathFor(model.name + "_" + categoryMetrics.Name + ".xml", imbSCI.Data.enums.getWritableFileMode.overwrite, "FV and Category sample metrics, serialized object"));
                    //context.notes.log("- - Creating report for category [" + pair.Key.name + "] completed");
                    //repTable.GetReportAndSave(valCol.folder, appManager.AppInfo, model.name + "_category_" + pair.Key.name);
                }

                modelMetrics.DivideValues(casesByClasses.Count);
                modelMetrics.saveObjectToXML(valCol.folder.pathFor(model.name + "_metrics.xml", imbSCI.Data.enums.getWritableFileMode.overwrite, "Cross-categories macroaveraged metrics of the FVE model [" + model.name + "]"));

                modelVsCategoryMetrics.AddRow(modelMetrics);
                modelVsCategoryMetrics.GetRowMetaSet().SetStyleForRowsWithValue<String>(DataRowInReportTypeEnum.dataHighlightA, "Name", modelMetrics.Name);
                modelVsCategoryMetrics.GetReportAndSave(valCol.folder, appManager.AppInfo, model.name + "_metrics", true, true);

                context.mainReport.AddModelMetrics(modelMetrics);


                context.notes.log("- Creating report for all categories [" + model.name + "] ");
                allTable.GetReportAndSave(valCol.folder, appManager.AppInfo, model.name + "_categories", true, context.tools.operation.doReportsInParalell);
            }



            kFoldReport.MakeReports(context, valCol.folder);
            context.mainReport.AddBestPerformer(kFoldReport.GetTopClassifierReport(), kFoldReport.meanClassifierReport, model);

            // <---------------- creation of complete report

            context.notes.log("- Experiment sequence with Feature Vector Extractor [" + model.name + "] completed");
            context.notes.SaveNote();
            
        // <------------- END OF THE MODEL -------------------------------------------------------------------------------------------------

        }


        /// <summary>
        /// Runs the complete experiment
        /// </summary>
        /// <param name="context">The context.</param>
        protected void run(experimentExecutionContext context)
        {
            context.notes.SaveNote();
            
            context.notes.log("Experiment execution started");

            objectSerialization.saveObjectToXML(context.setup, context.folder.pathFor("experimentSetup.xml", imbSCI.Data.enums.getWritableFileMode.overwrite));

            context.mainReport = new experimentReport(context);

            context.notes.log("Starting experiment with FVE model/s");

            context.tools.SetReportAndCacheFolder(folder, true);
            context.tools.PreloadLemmaResource(context.logger);

            context.notes.log("Performing MCRepository decomposition");

            context.pipelineCollection.PrepareForParallelExecution(context.tools, context);

            context.tools.SaveCache();


            Int32 paralelModels = context.tools.operation.ParallelThreads / context.setup.validationSetup.k;
            if (paralelModels < 1)
            {
                paralelModels = 1;
            }

            ParallelOptions opt = new ParallelOptions();
            opt.MaxDegreeOfParallelism = paralelModels;
            Parallel.ForEach(context.setup.models, opt, model =>
            {

                runModel(context, model);
            });


            foreach (IWebFVExtractor model in context.setup.models) // <------------------------------------------------------------------------------------------------    FEATURE VECTOR EXTRACTOR
            {
               
                    
                
            }
            // <------------- END OF THE MODEL -------------------------------------------------------------------------------------------------

            context.notes.log("All parts of experiment completed");

            context.notes.AppendHeading("Classification models");

            foreach (var md in context.setup.classifiers)
            {
                context.notes.AppendLine(" > " + md.name);

                md.DescribeSelf().ForEach(x => context.notes.AppendLine(" > " + x));

                context.notes.AppendLine(" ");
            }
            
            context.notes.AppendHorizontalLine();

            context.mainReport.CloseReport(context.folder);

            context.notes.log("Best performing configuration: " + context.mainReport.theBestPerformer.Name + " [F1:" + context.mainReport.BestF1.ToString("F5") + "]");
            

            //context.notes.log("Saving local lemma resource cache file...");
           // context.tools.SaveCache();

            appManager.Application.folder.generateReadmeFiles(appManager.AppInfo);

            context.notes.SaveNote();

            context.errorNotes.SaveNote();

        }




    }
}
