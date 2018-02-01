// --------------------------------------------------------------------------------------------------------------------
// <copyright file="secondaryReportOnExperiment.cs" company="imbVeles" >
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
using imbNLP.PartOfSpeech.TFModels.semanticCloud;
using imbSCI.Core.attributes;
using imbSCI.Core.files;
using imbSCI.Core.files.folders;
using imbSCI.Core.reporting;
using imbSCI.DataComplex.tables;
using imbWBI.Core.WebClassifier.experiment;
using System.ComponentModel;
using System.IO;
using System.Text;
using imbWBI.Core.math;

using imbSCI.Core.math;
using System.Xml.Serialization;
using System.Data;
using imbSCI.Core.extensions.text;
using imbSCI.Data.collection.nested;
using imbWBI.Core.WebClassifier.wlfClassifier;
using imbSCI.Graph.FreeGraph;

namespace imbWBI.Core.WebClassifier.reportData
{

    /// <summary>
    /// Secondary (extracted) report on one experiment with several <see cref="secondaryReportOnFVE"/> sub experiment reports
    /// </summary>
    public class secondaryReportOnExperiment
    {
        [XmlIgnore]
        public folderNode folderRoot { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="secondaryReportOnExperiment"/> class.
        /// </summary>
        public secondaryReportOnExperiment()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="secondaryReportOnExperiment"/> class.
        /// </summary>
        /// <param name="__experiment">The experiment.</param>
        /// <param name="__report">The report.</param>
        public secondaryReportOnExperiment(experimentSetup __experiment, experimentReport __report)
        {
            experiment = __experiment;
            report = __report;
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="secondaryReportOnExperiment"/> class.
        /// </summary>
        /// <param name="pathToExperimentSetupXML">The path to experiment setup XML.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="_folderRoot">The folder root.</param>
        public secondaryReportOnExperiment(String pathToExperimentSetupXML, ILogBuilder logger, folderNode _folderRoot)
        {
            folderRoot = _folderRoot;

            Load(pathToExperimentSetupXML, logger, _folderRoot);

            Process(logger);

        }

        /// <summary>
        /// Loads all data on 
        /// </summary>
        /// <param name="pathOfExperimentSetupXML">Path pointing to experimentSetup.xml file</param>
        /// <param name="logger">The logger.</param>
        public void Load(String pathOfExperimentSetupXML, ILogBuilder logger, folderNode _folderRoot)
        {
            folderRoot = _folderRoot;
            folder = new DirectoryInfo(Path.GetDirectoryName(pathOfExperimentSetupXML));

            report = objectSerialization.loadObjectFromXML<experimentReport>(folder.pathFor("ReportSummary.xml"), logger);
            experiment = objectSerialization.loadObjectFromXML<experimentSetup>(folder.pathFor("experimentSetup.xml"), logger);
            //topperformers = report.bestPerformingClassifiers;
            topperformers = new objectTable<DocumentSetCaseCollectionReport>(folder.pathFor("TopPerformers.xml"), true, nameof(DocumentSetCaseCollectionReport.Name), "TopPerformers");

            

        }


        /// <summary>
        /// Loads the semantic clouds of the specified FVE
        /// </summary>
        /// <param name="fve">The fve.</param>
        /// <param name="logger">The logger.</param>
        public void LoadSemanticClouds(semanticFVExtractor fve, ILogBuilder logger) {


            DirectoryInfo di = folder;

            var dirs = di.GetDirectories(fve.name);

            if (!dirs.Any())
            {
                logger.log("Failed to find subfolder for FVE [" + fve.name + "]");
            }
            else
            {
                DirectoryInfo dir = dirs.First();

                folderNode fveFolder = dir;

                var allCloudFiles = fveFolder.findFiles("*Cloud.xml", SearchOption.AllDirectories);
                Int32 cl = 0;
                foreach (String cloudFile in allCloudFiles)
                {
                    if (cloudFile.Contains("General") || cloudFile.Contains("SharedKnowledge"))
                    {

                    }
                    else
                    {
                        semanticClouds.Add(fve.name, objectSerialization.loadObjectFromXML<lemmaSemanticCloud>(cloudFile, logger));
                        cl++;
                    }

                }

                logger.log("Semantic clouds loaded [" + cl + "] for " + fve.name);

            }
        }

        /// <summary>
        /// The use data table
        /// </summary>
        public Boolean useDataTable = true;

        /// <summary>
        /// Processes the specified logger.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public void Process(ILogBuilder logger)
        {
            Double F1 = Double.MinValue;

            foreach (wlfClassifier.semanticFVExtractor fve in experiment.featureVectorExtractors_semantic)
            {
                secondaryReportOnFVE secReport = new secondaryReportOnFVE();
                secReport.Experiment = experiment.name;
                secReport.Folds = experiment.validationSetup.k;
                String p = folder.path.Remove(folderRoot.path);

                secReport.Path = p;

                LoadSemanticClouds(fve, logger);


                secReport.UpdateSecondaryRecord(fve);

                secReport.Randomized = experiment.validationSetup.randomize;

                secReport.Classifiers = experiment.classifiers.Count;

                rangeFinderWithData ranger = new rangeFinderWithData();

                imbSCI.Core.math.classificationMetrics.classificationReportRowFlags flags = imbSCI.Core.math.classificationMetrics.classificationReportRowFlags.classifier;

                if (useDataTable)
                {
                    var dataTable = topperformers.GetDataTable();
                    foreach (DataRow topReport in dataTable.Rows) //report.bestPerformingClassifiers)
                    {
                        String name = topReport["Name"].toStringSafe();
                        Double f1 = 0;
                        Double.TryParse(topReport["F1measure"].ToString(), out f1);
                        if (name.Contains(fve.name))
                        {
                            secReport.Classifier = topReport["Classifier"].ToString();
                            secReport.F1Score = f1;
                        }
                        ranger.Learn(f1);
                    }
                }
                else
                {
                    foreach (DocumentSetCaseCollectionReport topReport in topperformers.GetList()) //report.bestPerformingClassifiers)
                    {
                        ranger.Learn(topReport.F1measure);

                        if (topReport.Name.Contains(fve.name))
                        {
                            secReport.Classifier = topReport.Classifier;
                            secReport.F1Score = topReport.F1measure;
                        }
                    }
                }

                secReport.F1ScoreDeviation = ranger.doubleEntries.GetStdDeviation(false);
                secReport.F1ScoreMean = ranger.Average;

                // <------------------------------------ COMPUTING THE CLOUD METRICS
                Int32 cn = 0;
                Double nodeCount = 0;
                Double linkCount = 0;
                Double pingLength = 0;
                if (semanticClouds.ContainsKey(fve.name))
                {
                    foreach (lemmaSemanticCloud sc in semanticClouds[fve.name])
                    {
                        nodeCount += sc.nodes.Count();
                        linkCount += sc.links.Count();

                        
                        pingLength += freeGraphExtensions.PingGraphSize(sc, sc.primaryNodes, true, freeGraphPingType.maximumPingLength);
                        cn++;
                    }


                    nodeCount = nodeCount.GetRatio(cn);
                    linkCount = linkCount.GetRatio(cn);
                    pingLength = pingLength.GetRatio(semanticClouds[fve.name].Count());
                }

                secReport.NodeCount = nodeCount;
                secReport.LinkRatio = linkCount.GetRatio(nodeCount);
                secReport.GraphDepth = pingLength.GetRatio(nodeCount);

                // <------------------------------------------------------ FINISHED WITH CLOUD METRICS

                items.Add(secReport);

                if (secReport.F1Score > F1)
                {
                    F1 = secReport.F1Score;
                    topPerformer = secReport;
                }
                //F1 = Math.Max(secReport.F1Score, F1);

                logger.log("Collected data on [" + fve.name + "] [" + fve.description + "]");
            }
        }

        [XmlIgnore]
        public folderNode folder { get; set; }


        // :::> COLECTED OBJECTS ::: ---------------------------------------------------------------------------------------- ::::
        [XmlIgnore]
        public objectTable<DocumentSetCaseCollectionReport> topperformers { get; set; }
        [XmlIgnore]
        public experimentReport report { get; set; }
        [XmlIgnore]
        public experimentSetup experiment { get; set; }

        [XmlIgnore]
        public aceDictionarySet<String, lemmaSemanticCloud> semanticClouds { get; set; } = new aceDictionarySet<String, lemmaSemanticCloud>();

        public secondaryReportOnFVE topPerformer { get; set; }

        // :::> PRODUCED ITEMS ::::: ---------------------------------------------------------------------------------------- ::::
        public List<secondaryReportOnFVE> items { get; set; } = new List<secondaryReportOnFVE>();


        
        

    }

}