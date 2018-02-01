// --------------------------------------------------------------------------------------------------------------------
// <copyright file="experimentReport.cs" company="imbVeles" >
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
using imbACE.Core.core;
using imbMiningContext.TFModels.ILRT;
using imbSCI.Core.files.folders;
using imbSCI.Core.reporting;
using imbWBI.Core.WebClassifier.cases;
using imbWBI.Core.WebClassifier.core;
using imbWBI.Core.WebClassifier.validation;
using imbWBI.Core.WebClassifier.wlfClassifier;
using imbWEM.Core.project;
using System.Text;
using imbSCI.Core.extensions.data;
using imbSCI.Core.extensions.io;
using imbSCI.Data;
using imbSCI.Data.collection.nested;
using System.ComponentModel;
using imbSCI.Core.attributes;
using System.Xml.Serialization;
using System.IO;
using imbSCI.Core.files;
using imbSCI.DataComplex.tables;
using imbACE.Core;
using System.Data;
using imbSCI.Core.extensions.table;
using imbSCI.Core.math;
using imbWBI.Core.WebClassifier.experiment;

namespace imbWBI.Core.WebClassifier.reportData
{

    /// <summary>
    /// Report covering the whole experiment
    /// </summary>
    public class experimentReport
    {
        [XmlIgnore]
        public experimentSetup experiment { get; set; }

        [XmlIgnore]
        public experimentExecutionContext context { get; set; }

        public experimentReport()
        {

        }

        public experimentReport(experimentExecutionContext _context)
        {
            context = _context;
            experiment = context.setup;
            
            StartTime = DateTime.Now.ToLongTimeString();
            start = DateTime.Now;
        }

        public void AddBestPerformer(DocumentSetCaseCollectionReport bestPerformingClassifier, DocumentSetCaseCollectionReport meanPerformance, IWebFVExtractor fveModel)
        {
            bestPerformingClassifiers.Add(bestPerformingClassifier);
            String line = "[" + fveModel.name + "] completed " + DateTime.Now.ToLongTimeString();
            fveFinishedRecods.Add(line);
            meanPerformanceForExtractors.Add(meanPerformance);
        }

        public void AddModelMetrics(featureExtractionMetrics metrics)
        {
            modelMetrics.Add(metrics);
        }

        public List<String> valColVsModelVsSampleHash { get; set; } = new List<string>();


        public List<String> DescribeSelf(List<String> output=null)
        {
            if (output == null) output = new List<string>();
            
            output.Add("Experiment [" + experiment.name + "] done in: " + Duration.ToString("F2") + " minutes");
            output.Add(context.setup.description);
            
        //    context.validationCollections.Count

            output.Add("k-Fold cross validation k[" + experiment.validationSetup.k + "] - RND(T/E)SMP[" + experiment.validationSetup.randomize.ToString() + "] - FVE models [" + experiment.models.Count + "] - Classiffiers [" + experiment.classifiers.Count + "]");
            Int32 nCats = 0;
            Int32 nCases = 0;
            Double nCasePerCat = 0;

            foreach (var c in context.classes.GetClasses())
            {
                nCats++;
                nCases += c.WebSiteSample.Count();
            }

            nCasePerCat = nCases.GetRatio(nCats);

            output.Add("Categories [" + nCats + "] with [" + nCases + "] -- cases per category [" + nCasePerCat.ToString("F2") + "]");

            var model = context.tools.model as pipelineMCRepo.model.mcRepoProcessModel;

            output.Add("Pages per web site (limit) [" + model.setup.target_languagePagePerSite + "]");

            foreach (var m in context.setup.models)
            {
                

                String ln = m.name.TrimToMaxLength(15);
                
                foreach (var fv in m.settings.featureVectors.serialization)
                {
                    if (fv.isActive) {
                        ln = ln.add("["+fv.name.TrimToMaxLength(10," ")+"]", " ");
                    } else
                    {
                        ln = ln.add("["+("-".Repeat(10))+"]", " ");
                    }
                }
                
            }




            output.Add("----");

            output.Add("The best classifier per FVE models, by cross k-fold mean of F1 (macro-average): ");

            output.Add(String.Format("[{0,-30}] [{1,10}] [{2,10:F5}]", "Feature Vector Model", "Top class.", "Macro F1"));

            foreach (var cl in bestPerformingClassifiers)
            {
                if (cl == theBestPerformer)
                {
                    output.Add(String.Format("[{0,-30}] [{1,10}] [{2,10:F5}] <-- the best ", cl.Name, cl.Classifier, cl.F1measure));
                }
                else
                {
                    output.Add(String.Format("[{0,-30}] [{1,10}] [{2,10:F5}]", cl.Name, cl.Classifier, cl.F1measure));
                }
            }

            output.Add("----");

            output.Add("The best performer: ");

            output.Add("Name: " + theBestPerformer.Name);
            output.Add("Classifier: " + theBestPerformer.Classifier);
            output.Add("F1 measure: " + theBestPerformer.F1measure.ToString("F5"));

            output.Add("----");

            output.Add("The FVE with highest S1 measure: ");
            output.Add("Name: " + bestModel.modelName);
            output.Add("Range width:    " + bestModel.RangeWidthAvg.ToString("F5"));
            output.Add("Range position: " + bestModel.RangePositionAvg.ToString("F5"));
            output.Add("S1 measure:     " + bestModel.S1Measure.ToString("F5"));



            output.Add("----");

            output.Add("Mean classifier performances by FVE models: ");


            DocumentSetCaseCollectionReport minMean = new DocumentSetCaseCollectionReport();
            minMean.F1measure = 1;
            DocumentSetCaseCollectionReport maxMean = new DocumentSetCaseCollectionReport();
            maxMean.F1measure = 0;

            foreach (var cl in meanPerformanceForExtractors)
            {
                if (cl.F1measure <= minMean.F1measure) minMean = cl;
                if (cl.F1measure > maxMean.F1measure) maxMean = cl;
            }

            foreach (var cl in meanPerformanceForExtractors)
            {
                String lb = " --- ";
                if (cl == minMean) lb = " min ";
                if (cl == maxMean) lb = " max ";

                output.Add(String.Format("[{0,-30}] P[{1,10:F5}] R[{2,10:F5}] F1[{3,10:F5}] [{4,5}]", cl.Name, cl.Precision, cl.Recall, cl.F1measure, lb));
            }

            output.Add(" --- FVE cross-classifier means are computed as quality infication for FVE's configuration");

            output.Add(" --- FVE models and k-fold sample distribution MD5 hash");

            foreach (var c in valColVsModelVsSampleHash)
            {
                output.Add(c);
            }

            return output;
        }



        public void CloseReport(folderNode folderToSaveInto)
        {
            foreach (var cl in modelMetrics)
            {
                if (cl.S1Measure > S1Measure)
                {
                    S1Measure = cl.S1Measure;
                    bestModel = cl;
                }
            }

            foreach (var cl in bestPerformingClassifiers)
            {
                
                if (cl.F1measure > BestF1)
                {
                    theBestPerformer = cl;
                }
                BestF1 = Math.Max(BestF1, cl.F1measure);
            }

            Duration = DateTime.Now.Subtract(start).TotalMinutes;

            if (folderToSaveInto != null)
            {
                var ds = DescribeSelf();
                var p = folderToSaveInto.pathFor("ReportSummary.txt", imbSCI.Data.enums.getWritableFileMode.none, "Short summary on the results of the experiment");
                File.WriteAllLines(p, ds);

                objectSerialization.saveObjectToXML(this, folderToSaveInto.pathFor("ReportSummary.xml", imbSCI.Data.enums.getWritableFileMode.none, "XML Serialized ReportSummary object - for automatic multi-experiment results processing"));

                objectTable<DocumentSetCaseCollectionReport> tp = new objectTable<DocumentSetCaseCollectionReport>(folderToSaveInto.pathFor("TopPerformers.xml", imbSCI.Data.enums.getWritableFileMode.overwrite, "XML Serialized object - with best performing FVE models - for later results post-reporting"), false, nameof(DocumentSetCaseCollectionReport.Name), "TopPerformers");


                var mp = folderToSaveInto.pathFor("ModelMetrics.xml", imbSCI.Data.enums.getWritableFileMode.overwrite, "The FVE with highest S1 score, for this experiment");
                bestModel.saveObjectToXML(mp);


                tp.AddRange(bestPerformingClassifiers);
                DataTable dt = tp.GetDataTable();
                context.AddExperimentInfo(dt);
                dt.GetRowMetaSet().SetStyleForRowsWithValue<String>(DataRowInReportTypeEnum.dataHighlightA, "Name", theBestPerformer.Name);

                dt.SetDescription("Overview table with the best performing FVE - vs - Classifier pairs of the experiment [" + experiment.name + "]");
                dt.GetReportAndSave(folderToSaveInto, appManager.AppInfo, "TopPerformers", true, context.tools.operation.doReportsInParalell);
                tp.Save(imbSCI.Data.enums.getWritableFileMode.overwrite);
            }
        }


        /// <summary> The best F1 ratio achieved during experiment </summary>
        [Category("Ratio")]
        [DisplayName("BestF1")]
        [imb(imbAttributeName.measure_letter, "F1")]
        [imb(imbAttributeName.measure_setUnit, "%")]
        [Description("The best F1 ratio achieved during experiment")] // [imb(imbAttributeName.measure_important)][imb(imbAttributeName.reporting_valueformat, "")][imb(imbAttributeName.reporting_escapeoff)]
        public Double BestF1 { get; set; } = default(Double);

        [Category("Score")]
        [DisplayName("S1Measure")]
        [imb(imbAttributeName.measure_letter, "FVw * FVp")]
        [imb(imbAttributeName.measure_setUnit, "%")]
        [Description("Suitability measure S1 describes how effectivly FVE describes the cases, in regard to their categories")] // [imb(imbAttributeName.measure_important)][imb(imbAttributeName.reporting_valueformat, "")][imb(imbAttributeName.reporting_escapeoff)]
        public Double S1Measure { get; set; } = Double.MinValue;

        private DateTime start { get; set; }


        /// <summary>  </summary>
        [Category("Label")]
        [DisplayName("StartTime")] //[imb(imbAttributeName.measure_letter, "")]
        [Description("Time signature of experiment start")] // [imb(imbAttributeName.reporting_escapeoff)]
        public String StartTime { get; set; } = default(String);



        /// <summary>  </summary>
        [Category("Label")]
        [DisplayName("EndTime")] //[imb(imbAttributeName.measure_letter, "")]
        [Description("Time signature of experiment end")] // [imb(imbAttributeName.reporting_escapeoff)]
        public String EndTime { get; set; } = default(String);


        /// <summary>
        /// String lines telling when each of FVE models were finished
        /// </summary>
        /// <value>
        /// The fve finished recods.
        /// </value>
        public List<String> fveFinishedRecods { get; set; } = new List<string>();


        /// <summary> Ratio </summary>
        [Category("Ratio")]
        [DisplayName("Duration")]
        [imb(imbAttributeName.measure_letter, "T")]
        [imb(imbAttributeName.measure_setUnit, "min")]
        [Description("Duration of the whole experiment in minutes")] // [imb(imbAttributeName.measure_important)][imb(imbAttributeName.reporting_valueformat, "")][imb(imbAttributeName.reporting_escapeoff)]
        public Double Duration { get; set; } = default(Double);


        public DocumentSetCaseCollectionReport theBestPerformer { get; set; }

        public featureExtractionMetrics bestModel { get; set; } 

        public List<featureExtractionMetrics> modelMetrics { get; set; } = new List<featureExtractionMetrics>();

        public List<DocumentSetCaseCollectionReport> bestPerformingClassifiers { get; set; } = new List<DocumentSetCaseCollectionReport>();

        
        public List<DocumentSetCaseCollectionReport> meanPerformanceForExtractors { get; set; } = new List<DocumentSetCaseCollectionReport>();



    }

}