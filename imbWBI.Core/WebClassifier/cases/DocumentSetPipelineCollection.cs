// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DocumentSetPipelineCollection.cs" company="imbVeles" >
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
// using imbMiningContext.TFModels.WLF_ISF;
using imbNLP.PartOfSpeech.flags.token;
using imbNLP.PartOfSpeech.pipeline.machine;
using imbNLP.PartOfSpeech.pipelineForPos.subject;
using imbNLP.PartOfSpeech.resourceProviders.core;
using imbNLP.PartOfSpeech.TFModels.webLemma;
using imbSCI.Core.extensions.data;
using imbSCI.Core.math;
using imbSCI.Core.reporting;
using imbSCI.Data;
using imbSCI.Data.extensions;
using imbSCI.Core.extensions.table;
using imbSCI.Core.data;
using imbSCI.DataComplex.tables;
using imbSCI.DataComplex.extensions.data.schema;
using imbSCI.DataComplex.extensions.data.operations;
using imbSCI.DataComplex.extensions.data.modify;
using imbSCI.DataComplex.extensions.data.formats;
using imbSCI.DataComplex.extensions.data;
using imbSCI.DataComplex.extensions;
using imbSCI.DataComplex.tf_idf;
using imbWBI.Core.WebClassifier.core;
using System.Text;
using imbMiningContext.MCRepository;
using imbNLP.PartOfSpeech.pipeline.core;
using imbSCI.Core.files.folders;
using imbNLP.PartOfSpeech.TFModels.webLemma.core;
using imbWBI.Core.WebClassifier.validation;
using imbWBI.Core.WebClassifier.experiment;
using imbSCI.Data.collection.nested;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using imbWBI.Core.WebCase.collections;
using imbWBI.Core.WebCase;
using imbNLP.PartOfSpeech.TFModels.semanticCloud;
using imbWBI.Core.WebClassifier.wlfClassifier;
using imbNLP.PartOfSpeech.TFModels.semanticCloudMatrix;
using imbSCI.DataComplex.tables;
using imbACE.Core;
using imbSCI.Graph.Converters;
using System.Data;
using imbSCI.DataComplex.special;
using System.IO;
using System.ComponentModel;
using imbSCI.Core.attributes;
using imbNLP.PartOfSpeech.flags.basic;
using imbNLP.PartOfSpeech.flags.data;
using imbACE.Core.core.exceptions;
using imbNLP.PartOfSpeech.TFModels.webLemma.table;
using imbSCI.Graph.FreeGraph;
using imbWBI.Core.WebClassifier.reportData;

namespace imbWBI.Core.WebClassifier.cases
{


    /// <summary>
    /// 
    /// </summary>
    public class DocumentSetPipelineCollection
    {
        /// <summary>
        /// The machine
        /// </summary>
        protected pipelineMachine machine = new pipelineMachine();

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentSetPipelineCollection"/> class.
        /// </summary>
        public DocumentSetPipelineCollection()
        {
            
        }

       // public kFoldValidationCollection diagnosticValidation { get; set; }


        public webProjectKnowledgeSet caseKnowledgeSet { get; set; }

        /// <summary>
        /// Prepares for parallel execution.
        /// </summary>
        /// <param name="tools">The tools.</param>
        /// <param name="_context">The context.</param>
        public webProjectKnowledgeSet PrepareForParallelExecution(classifierTools tools, experimentExecutionContext _context)
        {
            if (caseKnowledgeSet == null) caseKnowledgeSet = new webProjectKnowledgeSet();

            if (items.Any())
            {
                experimentContext.notes.log("Mining Context was ready already.");
                return caseKnowledgeSet;
            }
            DateTime startTime = DateTime.Now;
            experimentContext = _context;

            

            List<webCaseKnowledge> cases = new List<webCaseKnowledge>();

            folderNode classReportFolder = experimentContext.folder.Add("General", "General and diagnostic reports", "The folder contains general (outside k-folds) reports on analysied industries (categories), web sites and other diagnostic data");

            // <----------------------------------------------------------------------------------------------------------------        [ performing pipeline ]
            experimentContext.notes.log("Executing the Mining Context decomposition with the pipeline model");
            foreach (IDocumentSetClass classSet in experimentContext.classes.GetClasses())
            {
                var pipelineContext = GetContextForPipeline(tools, classSet);
                sitesByCategory.Add(classSet, new List<pipelineTaskMCSiteSubject>());

                if (!pipelineContext.exitByType.ContainsKey(typeof(pipelineTaskMCSiteSubject)))
                {
                    throw new aceGeneralException("Pipeline context output contains no web site subjects! Check the pipeline Site Task constructor.", null, pipelineContext, "Pipeline broken");
                }

                var sitesForContext = pipelineContext.exitByType[typeof(pipelineTaskMCSiteSubject)]; // <----- preparing 
                foreach (var site in sitesForContext)
                {
                    tokenBySite.Add(site as pipelineTaskMCSiteSubject, new ConcurrentBag<pipelineTaskSubjectContentToken>());
                    sitesByCategory[classSet].Add(site as pipelineTaskMCSiteSubject);

                    webCaseKnowledge webCase = new webCaseKnowledge(site as pipelineTaskMCSiteSubject, classSet);

                    caseKnowledgeSet.Add(webCase);
                    cases.Add(webCase);
                }

                semanticFVExtractorKnowledge kn = new semanticFVExtractorKnowledge();
                kn.name = classSet.name + "_general";
                kn.relatedItemPureName = classSet.name;
                kn.type = WebFVExtractorKnowledgeType.aboutCompleteCategory;
                kn.Deploy(classReportFolder, experimentContext.logger);
                knowledgeByClass.TryAdd(classSet, kn);
            }

            experimentContext.notes.log("Sorting tokens for all sites [in parallel]");
            Parallel.ForEach(tokenBySite.Keys, site =>
            {
                var leafs = site.getAllLeafs();
                foreach (var leaf in leafs)
                {
                    pipelineTaskSubjectContentToken token = leaf as pipelineTaskSubjectContentToken;
                    if (token != null)
                    {
                        tokenBySite[site].Add(token);
                    }
                }
            });

            foreach (var c in cases)
            {
                c.tokens = tokenBySite[c.MCSiteSubject];
            }


            experimentContext.notes.log("Building diagnostic TF-IDF master tables for all classes [in parallel]");


            Boolean useIntegratedApproach = false;
            


            if (useIntegratedApproach)
            {
                var valCase = experimentContext.validationCollections[experimentContext.masterExtractor.name].GetDiagnosticCase(experimentContext.classes);
                Parallel.ForEach(sitesByCategory, pair =>
                {
                    
                    knowledgeByClass.TryAdd(pair.Key, experimentContext.masterExtractor.DoFVExtractionForClassViaCases(valCase.trainingCases[pair.Key.classID], pair.Key, valCase, experimentContext.tools, experimentContext.logger));
                });

            }
            else
            {

                Parallel.ForEach(sitesByCategory, pair =>
                {
                    IDocumentSetClass category = pair.Key;
                    List<pipelineTaskMCSiteSubject> sites = pair.Value;

                    var lt = BuildLemmaTableForClass(tools, category, sites);
                    lt.Save();
                // lt.SaveAs(classReportFolder.pathFor(lt.info.Name), imbSCI.Data.enums.getWritableFileMode.overwrite);

                });

            }

            experimentContext.notes.log("Saving lexic resource cache subset - for later reuse in case of repeated experiment run");
            tools.SaveCache();


            if (!useIntegratedApproach)
            {

                experimentContext.notes.log("Performing chunk construction for all web sites in all categories [in serial]");



                foreach (IDocumentSetClass classSet in experimentContext.classes.GetClasses())
                {
                    BuildChunksForClass(tools, classSet);
                }



                foreach (IDocumentSetClass classSet in experimentContext.classes.GetClasses())
                {
                    experimentContext.masterExtractor.chunkTableConstructor.process(chunksByCategory[classSet], cnt_level.mcPage, knowledgeByClass[classSet].WLChunkTableOfIndustryClass, null, experimentContext.logger, false);
                }

            }

            if (tools.operation.doCreateDiagnosticMatrixAtStart)
            {
                experimentContext.notes.log("Performing diagnostic analysis on all categories...[doCreateDiagnosticMatrixAtStart=true]");



                folderNode matrixReport = classReportFolder.Add("clouds", "More reports on semantic cloud", "Directory contains exported DirectedGraphs, varous matrix derivates, combined cloud and other diagnostic things");

                List<lemmaSemanticCloud> clouds = new List<lemmaSemanticCloud>();
                List<lemmaSemanticCloud> filteredClouds = new List<lemmaSemanticCloud>();

                var converter = lemmaSemanticCloud.GetDGMLConverter();

                foreach (IDocumentSetClass classSet in experimentContext.classes.GetClasses())
                {
                    // experimentContext.masterExtractor.chunkTableConstructor.process(chunksByCategory[classSet], cnt_level.mcPage, knowledgeByClass[classSet].WLChunkTableOfIndustryClass, null, experimentContext.logger, false);
                    

                    var cloud = experimentContext.masterExtractor.CloudConstructor.process(knowledgeByClass[classSet].WLChunkTableOfIndustryClass, knowledgeByClass[classSet].WLTableOfIndustryClass, knowledgeByClass[classSet].semanticCloud, experimentContext.logger, tokenBySite.Keys.ToList(), tools.GetLemmaResource());
                    knowledgeByClass[classSet].semanticCloud.className = classSet.name;
                    clouds.Add(cloud);

                    if (experimentContext.tools.operation.doUseSimpleGraphs)
                    {
                        cloud.GetSimpleGraph(true).Save(matrixReport.pathFor("cloud_initial_" + classSet.name, imbSCI.Data.enums.getWritableFileMode.none, "Initial version of full-sample set, diagnostic Semantic Cloud for category [" + classSet.name + "]"));
                    }
                    else
                    {


                        converter.ConvertToDMGL(cloud).Save(matrixReport.pathFor("cloud_initial_" + classSet.name, imbSCI.Data.enums.getWritableFileMode.none, "Initial version of full-sample set, diagnostic Semantic Cloud for category [" + classSet.name + "]"));
                    }


                   


                    knowledgeByClass[classSet].semanticCloudFiltered = knowledgeByClass[classSet].semanticCloud.CloneIntoType<lemmaSemanticCloud>(true);
                    knowledgeByClass[classSet].semanticCloudFiltered.className = classSet.name;
                    filteredClouds.Add(knowledgeByClass[classSet].semanticCloudFiltered);
                    
                }

                cloudMatrix matrix = new cloudMatrix("CloudMatrix", "Diagnostic cloud matrix created from the complete sample set of [" + clouds.Count() + "] classes");
                matrix.build(filteredClouds, experimentContext.logger);

                lemmaSemanticCloud mergedCloudInitial = matrix.GetUnifiedCloud();
                mergedCloudInitial.Save(matrixReport.pathFor("unified_initial_cloud.xml", imbSCI.Data.enums.getWritableFileMode.overwrite, "Serialized object - Initial version of Semantic Cloud built as union of full-sample set Semantic Clouds of all categories"));


                var reductions =  matrix.TransformClouds(experimentContext.masterExtractor.settings.semanticCloudFilter, experimentContext.logger);

                var p = matrixReport.pathFor("reductions_nodes.txt", imbSCI.Data.enums.getWritableFileMode.overwrite, "Report on Cloud Matrix transformation process");
                File.WriteAllLines(p, reductions);


               




                matrix.BuildTable(experimentContext.masterExtractor.settings.semanticCloudFilter, cloudMatrixDataTableType.initialState | cloudMatrixDataTableType.maxCloudFrequency | cloudMatrixDataTableType.absoluteValues).GetReportAndSave(matrixReport, appManager.AppInfo, "matrix_max_cf_initial", true, experimentContext.tools.operation.doReportsInParalell);

                matrix.BuildTable(experimentContext.masterExtractor.settings.semanticCloudFilter, cloudMatrixDataTableType.initialState | cloudMatrixDataTableType.overlapSize | cloudMatrixDataTableType.absoluteValues).GetReportAndSave(matrixReport, appManager.AppInfo, "matrix_overlap_size_initial", true, experimentContext.tools.operation.doReportsInParalell);

                matrix.BuildTable(experimentContext.masterExtractor.settings.semanticCloudFilter, cloudMatrixDataTableType.initialState | cloudMatrixDataTableType.overlapValue | cloudMatrixDataTableType.absoluteValues).GetReportAndSave(matrixReport, appManager.AppInfo, "matrix_overlap_value_initial", true, experimentContext.tools.operation.doReportsInParalell);

               
                matrix.ExportTextReports(matrixReport, true, "matrix_cf");
                matrix.ExportTextReports(matrixReport, false, "matrix_cf");

                lemmaSemanticCloud mergedCloudAfterReduction = matrix.GetUnifiedCloud();
                mergedCloudAfterReduction.Save(matrixReport.pathFor("unified_reduced_cloud.xml", imbSCI.Data.enums.getWritableFileMode.overwrite, "Serialized object -Version of all-categories diagnostic Semantic Cloud, after Cloud Matrix filter was applied"));

                if (experimentContext.tools.operation.doUseSimpleGraphs)
                {
                    mergedCloudInitial.GetSimpleGraph(true).Save(matrixReport.pathFor("unified_initial_cloud", imbSCI.Data.enums.getWritableFileMode.overwrite, "DirectedGraphML file - unified Semantic Cloud, before Cloud Matrix filter was applied - Open this in VisualStudo)"));
                }
                else
                {
                    converter = lemmaSemanticCloud.GetDGMLConverter();

                    converter.ConvertToDMGL(mergedCloudInitial).Save(matrixReport.pathFor("unified_initial_cloud", imbSCI.Data.enums.getWritableFileMode.overwrite, "DirectedGraphML file - unified Semantic Cloud, before Cloud Matrix filter was applied - Open this in VisualStudo)"));
                }


                // <-------- analysis -----------------------------------------------------------------------------------
                DataTableTypeExtended<freeGraphReport> cloudReports = new DataTableTypeExtended<freeGraphReport>();
                foreach (var cl in filteredClouds)
                {
                    freeGraphReport fgReport = new freeGraphReport(cl);
                    fgReport.Save(matrixReport);
                    cloudReports.AddRow(fgReport);
                }
                freeGraphReport unifiedReport = new freeGraphReport(mergedCloudAfterReduction);
                unifiedReport.Save(matrixReport);
                cloudReports.AddRow(unifiedReport);

                
                cloudReports.GetReportAndSave(matrixReport, appManager.AppInfo, "analysis_SemanticClouds");
                // <-------- analysis -----------------------------------------------------------------------------------



                foreach (IDocumentSetClass classSet in experimentContext.classes.GetClasses())
                {
                    var cloud = knowledgeByClass[classSet].semanticCloudFiltered; // .WLChunkTableOfIndustryClass, knowledgeByClass[classSet].WLTableOfIndustryClass, knowledgeByClass[classSet].semanticCloud, experimentContext.logger, tokenBySite.Keys.ToList());


                    if (experimentContext.tools.operation.doUseSimpleGraphs)
                    {
                        cloud.GetSimpleGraph(true).Save(matrixReport.pathFor("unified_initial_cloud", imbSCI.Data.enums.getWritableFileMode.overwrite, "DirectedGraphML file - unified Semantic Cloud, before Cloud Matrix filter was applied - Open this in VisualStudo)"));
                    }
                    else
                    {
                        converter = lemmaSemanticCloud.GetDGMLConverter();

                        converter.ConvertToDMGL(cloud).Save(matrixReport.pathFor("unified_initial_cloud", imbSCI.Data.enums.getWritableFileMode.overwrite, "DirectedGraphML file - unified Semantic Cloud, before Cloud Matrix filter was applied - Open this in VisualStudo)"));
                    }



                    //converter.ConvertToDMGL(cloud).Save(matrixReport.pathFor("cloud_reduced_" + classSet.name, imbSCI.Data.enums.getWritableFileMode.none, "DirectedGraphML file - Initial version of Semantic Cloud built as union of full-sample set Semantic Clouds of all categories (Open this with VS)"), imbSCI.Data.enums.getWritableFileMode.overwrite);
                    
                }

                instanceCountCollection<String> tfcounter = new instanceCountCollection<string>();
                foreach (IDocumentSetClass classSet in experimentContext.classes.GetClasses())
                {
                   
                    var wlt = knowledgeByClass[classSet].WLTableOfIndustryClass.GetDataTable();
                    wlt.DefaultView.Sort = "termFrequency desc";
                    var sorted = wlt.DefaultView.ToTable();
                    var tbl = wlt.GetClonedShema<DataTable>(true);
                    
                    tbl.CopyRowsFrom(sorted, 0, 100);
                    tbl.GetReportAndSave(classReportFolder, appManager.AppInfo, classSet.name + "_WebLemma", true, experimentContext.tools.operation.doReportsInParalell);

                    var cht = knowledgeByClass[classSet].WLChunkTableOfIndustryClass.GetDataTable();
                    cht.DefaultView.Sort = "termFrequency desc";
                    var csorted = cht.DefaultView.ToTable();

                    tbl = cht.GetClonedShema<DataTable>(true);
                    tbl.CopyRowsFrom(csorted, 0, 100);
                    tbl.GetReportAndSave(classReportFolder, appManager.AppInfo, classSet.name + "_Chunks", true, experimentContext.tools.operation.doReportsInParalell);
                    
                    tfcounter.AddInstanceRange(knowledgeByClass[classSet].WLTableOfIndustryClass.unresolved);
                    

                    knowledgeByClass[classSet].OnBeforeSave();

                }

                List<String> countSorted = tfcounter.getSorted();
                StringBuilder sb = new StringBuilder();
                foreach (String s in countSorted)
                {
                    sb.AppendLine(String.Format("{1}  :  {0}", s, tfcounter[s]));
                }
                String pt = classReportFolder.pathFor("unresolved_tokens.txt", imbSCI.Data.enums.getWritableFileMode.none, "Cloud Frequency list of all unresolved letter-only tokens");
                File.WriteAllText(pt, sb.ToString());
            }


            if (tools.operation.doFullDiagnosticReport)
            {
                experimentContext.notes.log("Generating full diagnostic report on classes...");
                DataTable rep = null;
                foreach (IDocumentSetClass classSet in experimentContext.classes.GetClasses())
                {
                    rep = this.GetClassKnowledgeReport(classSet, rep);
                }
                rep.SetAdditionalInfoEntry("Experiment", experimentContext.setup.name);

                rep.AddExtra("Experiment: "+ experimentContext.setup.name);

                rep.AddExtra("Info: " + experimentContext.setup.description);

                rep.SetDescription("Structural report for all classes in the experiment");
                rep.GetReportAndSave(classReportFolder, appManager.AppInfo, "structural_class_report", true, experimentContext.tools.operation.doReportsInParalell);

            }

            classReportFolder.generateReadmeFiles(appManager.AppInfo);


            experimentContext.notes.log("Mining Context preprocessing done in [" + DateTime.Now.Subtract(startTime).TotalMinutes.ToString("F2") + "] minutes");
            return caseKnowledgeSet;
        }

        protected pipelineModelExecutionContext GetContextForPipeline(classifierTools tools, IDocumentSetClass documentSetClass)
        {
            if (!items.ContainsKey(documentSetClass.name))
            {
                pipelineModelExecutionContext context = machine.run(tools.model, documentSetClass.MCRepositoryName, documentSetClass, new List<String>());

                items.Add(documentSetClass.name, context);
            }

            return items[documentSetClass.name];
        }

        protected webLemmaTermTable BuildLemmaTableForClass(classifierTools tools, IDocumentSetClass documentSetClass, List<pipelineTaskMCSiteSubject> sites)
        {
            var context = items[documentSetClass.name];
            experimentContext.notes.log("Master TF-IDF table construction (used for POS flagging)... [" + documentSetClass.name + "]");
            webLemmaTermTable lemmaTable = knowledgeByClass[documentSetClass].WLTableOfIndustryClass; // new webLemmaTermTable(experimentContext.folder.pathFor("master_table_" + documentSetClass.name + ".xml"), true, "master_table_" + documentSetClass.name);
            lemmaTable.Clear();
            experimentContext.masterConstructor.process(GetTokensForSites<IPipelineTaskSubject>(sites), cnt_level.mcPage, lemmaTable, tools.GetLemmaResource(), context.logger, false);
            
            //lemmaTableByClass.TryAdd(documentSetClass, lemmaTable);
            return lemmaTable;
        }

        protected void BuildChunksForClass(classifierTools tools, IDocumentSetClass documentSetClass)
        {
            var context = items[documentSetClass.name];
            //lemmaTable.SaveAs(experimentContext.folder.pathFor("master_table_" + documentSetClass.name + ".xml", imbSCI.Data.enums.getWritableFileMode.overwrite));
            experimentContext.chunkComposer.reset();
            experimentContext.notes.log("Chunk construction... [" + documentSetClass.name + "]");

            ConcurrentBag<IPipelineTaskSubject> MCStreams = context.exitByLevel[cnt_level.mcTokenStream];  //context.exitSubjects.GetSubjectChildrenTokenType<pipelineTaskSubjectContentToken, IPipelineTaskSubject>(new cnt_level[] { cnt_level.mcTokenStream, cnt_level.mcChunk }, true); // sites.getAllChildren();  //context.exitSubjects.GetSubjectsOfLevel<IPipelineTaskSubject>(cnt_level.mcTokenStream);

            streamsByCategory.Add(documentSetClass, MCStreams.ToList());

            

            List<pipelineTaskSubjectContentToken> Chunks = experimentContext.chunkComposer.process(MCStreams.ToSubjectToken(), experimentContext.logger);

            chunksByCategory.Add(documentSetClass, Chunks);

            if (Chunks.Count == 0)
            {
                experimentContext.logger.log("-- no chunks produced for [" + documentSetClass.name + "] -- Stream input count [" + MCStreams.Count + "]");
            }
            else
            {
                experimentContext.notes.log("[" + Chunks.Count + "] chunks constructed for class [" + documentSetClass.name + "]");
            }
        }

       

        /// <summary>
        /// Gets the context.
        /// </summary>
        /// <param name="tools">The tools.</param>
        /// <param name="documentSetClass">The document set class.</param>
        /// <returns></returns>
        public pipelineModelExecutionContext GetContext(classifierTools tools, IDocumentSetClass documentSetClass)
        {
            return GetContextForPipeline(tools, documentSetClass);
        }
        /// <summary>
        /// Gets the tokens for sites.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sites">The sites.</param>
        /// <returns></returns>
        public List<T> GetTokensForSites<T>(IEnumerable<pipelineTaskMCSiteSubject> sites) where T:class, IPipelineTaskSubject
        {
            List<T> output = new List<T>();
            foreach (var st in sites) {

                foreach (var tk in tokenBySite[st])
                {
                    output.Add(tk as T);
                }
            }
            return output;
        }

        public List<pipelineTaskSubjectContentToken> GetTokensForSite(pipelineTaskMCSiteSubject site)
        {
            return this.tokenBySite[site].ToList();
        }

        internal Dictionary<pipelineTaskMCSiteSubject, ConcurrentBag<pipelineTaskSubjectContentToken>> tokenBySite { get; set; } = new Dictionary<pipelineTaskMCSiteSubject, ConcurrentBag<pipelineTaskSubjectContentToken>>();

        internal Dictionary<String, pipelineModelExecutionContext> items { get; set; } = new Dictionary<string, pipelineModelExecutionContext>();

        internal Dictionary<IDocumentSetClass, List<pipelineTaskMCSiteSubject>> sitesByCategory { get; set; } = new Dictionary<IDocumentSetClass, List<pipelineTaskMCSiteSubject>>();

        internal Dictionary<IDocumentSetClass, List<IPipelineTaskSubject>> streamsByCategory { get; set; } = new Dictionary<IDocumentSetClass, List<IPipelineTaskSubject>>();

        internal Dictionary<IDocumentSetClass, List<pipelineTaskSubjectContentToken>> chunksByCategory { get; set; } = new Dictionary<IDocumentSetClass, List<pipelineTaskSubjectContentToken>>();

        internal ConcurrentDictionary<IDocumentSetClass, semanticFVExtractorKnowledge> knowledgeByClass { get; set; } = new ConcurrentDictionary<IDocumentSetClass, semanticFVExtractorKnowledge>();

        public IPipelineModel model { get; set; }

        public experimentExecutionContext experimentContext { get; set; }

    }

}