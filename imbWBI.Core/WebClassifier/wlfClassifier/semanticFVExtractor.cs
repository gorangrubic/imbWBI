// --------------------------------------------------------------------------------------------------------------------
// <copyright file="semanticFVExtractor.cs" company="imbVeles" >
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
using imbSCI.DataComplex.tf_idf;
using imbWBI.Core.WebClassifier.cases;
using imbWBI.Core.WebClassifier.core;
using imbWBI.Core.WebClassifier.validation;
using System.Text;
using System.Xml.Serialization;
using imbNLP.PartOfSpeech.TFModels.semanticCloud;
using System.IO;
using imbNLP.PartOfSpeech.decomposing.chunk;
using imbSCI.DataComplex.tables;
using imbACE.Core;
using Accord.MachineLearning;
using Accord.IO;
using Accord.Math.Distances;
using imbNLP.PartOfSpeech.TFModels.semanticCloudMatrix;
using imbSCI.Graph.Converters;
using imbACE.Core.core.exceptions;
using imbSCI.Data.enums;
using imbNLP.PartOfSpeech.TFModels.webLemma.table;
using imbWBI.Core.WebClassifier.reportData;

namespace imbWBI.Core.WebClassifier.wlfClassifier
{

    /// <summary>
    /// Classifier
    /// </summary>
    /// <seealso cref="imbWBI.Core.WebClassifier.wlfClassifier.WebFVExtractorBase{imbWBI.Core.WebClassifier.wlfClassifier.semanticFVExtractor}" />
    public class semanticFVExtractor : WebFVExtractorBase<semanticFVExtractorKnowledge, semanticFVExtractorSettings>
    {
        

        public wlfConstructorTFIDF termTableConstructor { get; set; } = new wlfConstructorTFIDF();
        public chunkConstructorTF chunkTableConstructor { get; set; } = new chunkConstructorTF();
        public cloudConstructor CloudConstructor { get; set; } = new cloudConstructor();

        /// <summary>
        /// Gets the short name.
        /// </summary>
        /// <param name="infix">The infix.</param>
        /// <returns></returns>
        public String GetShortName(String infix="")
        {
            String output = "";

            foreach (var fv in settings.featureVectors.serialization)
            {
                if (fv.isActive) {
                    output = output.add(fv.name, "_");
                }
            }
            output = output.add(infix, "_");
            //if (termTableConstructor.settings.doUseIDF)
            //{
            //    output = output.add("DFC" + termTableConstructor.settings.documentFrequencyMaxFactor.ToString().Replace(".", "_"), "_");
            //}
            output = output.add("E" + settings.caseTermExpansionSteps, "_");
            return output;
        }

        ///// <summary>
        ///// Updates the secondary record.
        ///// </summary>
        ///// <param name="record">The record.</param>
        ///// <returns></returns>
        //public secondaryReportOnFVE UpdateSecondaryRecord(secondaryReportOnFVE record)
        //{
        //    semanticFVExtractor fve = this;

        //    foreach (var fv in fve.settings.featureVectors.serialization)
        //    {
        //        if (fv.isActive)
        //        {
        //            record.FVPType += fv.name + " ";
        //        }

        //    }

        //    record.IDF = fve.termTableConstructor.settings.doUseIDF;
        //    record.DFC = fve.termTableConstructor.settings.documentFrequencyMaxFactor;

        //    record.HTMLTagFactors = fve.termTableConstructor.settings.titleTextFactor + ":" + fve.termTableConstructor.settings.anchorTextFactor + ":" + fve.termTableConstructor.settings.titleTextFactor;
        //    record.TCBOn = fve.CloudConstructor.settings.doFactorToClassClouds;
        //    record.TermCategory = fve.CloudConstructor.settings.PrimaryTermWeightFactor + ":" + fve.CloudConstructor.settings.SecondaryTermWeightFactor + ":" + fve.CloudConstructor.settings.ReserveTermWeightFactor;

        //    if (fve.settings.semanticCloudFilter.doDivideWeightWithCloudFrequency || fve.settings.semanticCloudFilter.doUseSquareFunctionOfCF)
        //    {
        //        if (!fve.settings.semanticCloudFilter.doUseSquareFunctionOfCF)
        //        {
        //            record.ReduxFunction = "[1/CF]";
        //        }
        //        else
        //        {
        //            record.ReduxFunction = "[1/Sq(CF)]";
        //        }
        //    }
        //    else
        //    {
        //        record.ReduxFunction = "[false]";
        //    }

        //    record.LowpassFreq = fve.settings.semanticCloudFilter.lowPassFilter;

        //    if (fve.settings.semanticCloudFilter.isActive)
        //    {
        //        if (fve.settings.semanticCloudFilter.doCutOffByCloudFrequency)
        //        {

        //            if (!fve.settings.semanticCloudFilter.doAssignMicroWeightInsteadOfRemoval)
        //            {
        //                record.LowPassFunction = "[Remove]";
        //            }
        //            else
        //            {
        //                record.LowPassFunction = "[W=mini]";
        //            }

        //        }
        //        else
        //        {
        //            record.LowPassFunction = "[Off]";
        //        }
        //    }
        //    else
        //    {
        //        record.LowPassFunction = "[Off]";
        //        record.LowpassFreq = 0;
        //    }

        //    return record;
        //}

        /// <summary>
        /// Gets the short description.
        /// </summary>
        /// <returns></returns>
        public String GetShortDescription()
        {
            String output = "";

            foreach (var fv in settings.featureVectors.serialization)
            {
                if (fv.isActive) { output += " [" + fv.name + "]"; }
            }

            output += "useIDF[" + termTableConstructor.settings.doUseIDF + "]";
            if (termTableConstructor.settings.doUseIDF)
            {
                output += " DFC[" + termTableConstructor.settings.documentFrequencyMaxFactor + "] ";
            }

            output += " TW[" + termTableConstructor.settings.titleTextFactor + ":" + termTableConstructor.settings.anchorTextFactor + ":" + termTableConstructor.settings.titleTextFactor + "] TCBoost[" + CloudConstructor.settings.doFactorToClassClouds + "] ";
            if (CloudConstructor.settings.doFactorToClassClouds) output += " TCw[" + CloudConstructor.settings.PrimaryTermWeightFactor + ":" + CloudConstructor.settings.SecondaryTermWeightFactor + ":" + CloudConstructor.settings.ReserveTermWeightFactor + "] ";
            output += " Matrix[" + settings.semanticCloudFilter.isActive + "] ";
            if (settings.semanticCloudFilter.isActive)
            {
                if (settings.semanticCloudFilter.doDivideWeightWithCloudFrequency || settings.semanticCloudFilter.doUseSquareFunctionOfCF)
                {
                    if (!settings.semanticCloudFilter.doUseSquareFunctionOfCF)
                    {
                        output += " Redux[1/CF] ";
                    }
                    else
                    {
                        output += " Redux[1/Sq(CF)] ";
                    }
                }
                else
                {
                    output += " Redux[false] ";
                }

                if (settings.semanticCloudFilter.doCutOffByCloudFrequency)
                {
                    output += " LPF[";

                    if (!settings.semanticCloudFilter.doAssignMicroWeightInsteadOfRemoval)
                    {
                        output += "Remove";
                    }
                    else
                    {
                        output += "W=mini";
                    }
                    output += "]=[" + settings.semanticCloudFilter.lowPassFilter + "]";
                }

            }


            return output;


        }

        /// <summary>
        /// Creates multi-line description of current configuration
        /// </summary>
        /// <returns>List of description lines</returns>
        public override List<string> DescribeSelf()
        {
            List<String> output = new List<string>();

            output.Add("# [" + name + "] Semantic Feature Vector Extractor");

            output.Add(description);

            output.Add(" > Performs Feature Vector Extraction from web sites (DocumentSet) in the targeted MC Repository (collection of DocumentSets)");
            output.Add(" > For each category (DocumentSetClass) the following resources are constructed:");
            output.Add(" > + Web Lemma TF-IDF table");
            output.Add(" > + Chunk TF-IDF table");
            output.Add(" > + Semantic Cloud (Lexicon)");

           
            output.AddRange(settings.DescribeSelf());

            output.Add("---");

            output.Add("-- Utilizes the following components with corresponding settings --");

            output.AddRange(termTableConstructor.DescribeSelf());

            output.Add("---");


            output.AddRange(chunkTableConstructor.DescribeSelf());

            output.Add("---");

            output.AddRange(CloudConstructor.DescribeSelf());

            return output;
        }



        public semanticFVExtractor()
        {
            name = "Semantic";
        }


        /// <summary>
        /// Performing post processing of FV knowledge
        /// </summary>
        /// <param name="validationCase">The validation case.</param>
        /// <param name="tools">The tools.</param>
        /// <param name="logger">The logger.</param>
        public override void DoFVPostProcessing(kFoldValidationCase validationCase, classifierTools tools, ILogBuilder logger)
        {

            List<lemmaSemanticCloud> clouds = new List<lemmaSemanticCloud>();

            foreach (var docClass in validationCase.context.classes.GetClasses())
            {
                var knowledge = validationCase.knowledgeLibrary.GetKnowledgeInstance<semanticFVExtractorKnowledge>(docClass, validationCase, logger);
                knowledge.semanticCloudFiltered = knowledge.semanticCloud.Clone();
                clouds.Add(knowledge.semanticCloudFiltered);
                knowledge.semanticCloud.className = docClass.name;
                knowledge.semanticCloudFiltered.className = docClass.name + "flt";
                if (settings.semanticCloudFilter.isActive)
                {
                    knowledge.semanticCloudFiltered.description = "Semantic cloud filtered with cloud matrix";
                } else
                {
                    knowledge.semanticCloudFiltered.description = "Semantic cloud filter is off - this is initial cloud";
                }
                
            }
            if (settings.semanticCloudFilter.isActive)
            {

                logger.log(validationCase.name + ": Cloud matrix creation starts...");
                cloudMatrix matrix = new cloudMatrix(validationCase.name, "Cloud overlap matrix of [" + clouds.Count + "] for fold [" + validationCase.name + "] of experiment [" + validationCase.context.setup.name + "]");

                matrix.build(clouds, logger);

                matrix.TransformClouds(settings.semanticCloudFilter, logger);


                if (tools.operation.doMakeGraphForClassClouds)
                {
                    foreach (var cloud in clouds)
                    {
                        if (tools.operation.doUseSimpleGraphs)
                        {
                            cloud.GetSimpleGraph(true).Save(validationCase.caseFolder.pathFor("class_" + cloud.className + "_reducedCloud", getWritableFileMode.overwrite));
                        }
                        else
                        {
                            var converter = lemmaSemanticCloud.GetDGMLConverter();
                            converter.ConvertToDMGL(cloud).Save(validationCase.caseFolder.pathFor("class_" + cloud.className + "_reducedCloud", getWritableFileMode.overwrite));
                        }
                    }
                }


                //logger.log(validationCase.name + ": Cloud matrix report creation ...");
               // matrix.BuildTable(settings.semanticCloudFilter, cloudMatrixDataTableType.initialState | cloudMatrixDataTableType.overlapSize | cloudMatrixDataTableType.absoluteValues).GetReportAndSave(validationCase.folder, appManager.AppInfo, "matrix_overlap_norm_initial");
               // matrix.BuildTable(settings.semanticCloudFilter, cloudMatrixDataTableType.stateAfterReduction | cloudMatrixDataTableType.overlapSize | cloudMatrixDataTableType.absoluteValues).GetReportAndSave(validationCase.folder, appManager.AppInfo, "matrix_overlap_abs_initial");
               // matrix.BuildTable(settings.semanticCloudFilter, cloudMatrixDataTableType.stateAfterReduction | cloudMatrixDataTableType.overlapSize | cloudMatrixDataTableType.normalizedValues).GetReportAndSave(validationCase.folder, appManager.AppInfo, "matrix_overlap_norm_reduced");
               // matrix.BuildTable(settings.semanticCloudFilter, cloudMatrixDataTableType.stateAfterReduction | cloudMatrixDataTableType.overlapSize | cloudMatrixDataTableType.absoluteValues).GetReportAndSave(validationCase.folder, appManager.AppInfo, "matrix_overlap_abs_reduced");
               // matrix.BuildTable(settings.semanticCloudFilter, cloudMatrixDataTableType.stateAfterReduction | cloudMatrixDataTableType.maxCloudFrequency | cloudMatrixDataTableType.normalizedValues).GetReportAndSave(validationCase.folder, appManager.AppInfo, "matrix_CF_norm_reduced");
               // logger.log(validationCase.name + ": Cloud matrix report done.");
            } else
            {
                logger.log(validationCase.name + ": Cloud matrix is not active");
            }

        }


        public override semanticFVExtractorKnowledge DoFVExtractionForClassViaCases(validationCaseCollection vCaseColl, IDocumentSetClass documentSetClass, kFoldValidationCase validationCase, classifierTools tools, ILogBuilder logger)
        {
            semanticFVExtractorKnowledge knowledge = vCaseColl.kFoldMaster.knowledgeLibrary.GetKnowledgeInstance<semanticFVExtractorKnowledge>(documentSetClass, vCaseColl.kFoldCase, logger);

            knowledge.SetRebuild(!tools.DoUseExistingKnowledge);


            if (knowledge.ShouldBuildAny())
            {
                DocumentSetCaseCollection dSetCol = new DocumentSetCaseCollection(documentSetClass);


                var context = tools.context.pipelineCollection.GetContext(tools, documentSetClass);

                //var sites = context.exitByType[typeof(pipelineTaskMCSiteSubject)].ConvertList<IPipelineTaskSubject, pipelineTaskMCSiteSubject>().ToList();
                var sites = context.exitByType[typeof(pipelineTaskMCSiteSubject)].ToList();
                List<pipelineTaskMCSiteSubject> ISites = sites.ConvertList<IPipelineTaskSubject, pipelineTaskMCSiteSubject>().ToList();

                List<pipelineTaskMCSiteSubject> fSites = vCaseColl.FilterSites(ISites);


                dSetCol.deploy(vCaseColl, validationCase, fSites, classes);

                List<webLemmaTermTable> tables = new List<webLemmaTermTable>();
                //List<webLemmaTermTable> chunkTables = new List<webLemmaTermTable>();



                foreach (DocumentSetCase vc in dSetCol)
                {
                    semanticFVExtractorKnowledge cKnowledge = vCaseColl.kFoldMaster.knowledgeLibrary.GetKnowledgeInstance<semanticFVExtractorKnowledge>(vc, validationCase, logger);
                    DoMakeKnowledgeForCase(vc, tools, dSetCol, logger);
                    tables.Add(cKnowledge.WLTableOfIndustryClass);
                }

                var tbl = tables.GetMergedLemmaTable(knowledge.name, logger);
                termTableConstructor.recompute(knowledge.WLTableOfIndustryClass, logger, false, tbl.GetList());
                
                

                DoMakeKnowledge(fSites, tools, knowledge, logger);
            }

            //  SetKnowledge(knowledge);
            //knowledge.OnBeforeSave();

            logger.log("[ALTPROC] Feature Extraction by [" + name + "][" + vCaseColl.kFoldCase.name + "][" + documentSetClass.name + "] done for " + vCaseColl.className);

            return knowledge;
        }



        /// <summary>
        /// Does the make knowledge.
        /// </summary>
        /// <param name="subjects">The subjects.</param>
        /// <param name="tools">The tools.</param>
        /// <param name="knowledge">The knowledge.</param>
        /// <param name="logger">The logger.</param>
        public override void DoMakeKnowledge(List<pipelineTaskMCSiteSubject> subjects, classifierTools tools, semanticFVExtractorKnowledge knowledge, ILogBuilder logger)
        {
            Boolean report = tools.DoReport;

            if (knowledge.doBuildTermTable)
            {
                knowledge.WLTableOfIndustryClass.Clear();
                knowledge.WLTableOfIndustryClass = termTableConstructor.process(tools.context.pipelineCollection.GetTokensForSites<IPipelineTaskSubject>(subjects), cnt_level.mcPage, knowledge.WLTableOfIndustryClass, tools.GetLemmaResource(), logger, subjects.Count == 1);
            } else
            {
                if (subjects.Count == 1)
                {
                   // logger.log("Using existing Web Lemma Table on [" + subjects.First().name + "]");
                }
                
            }
            
            if (knowledge.doBuildChunkTable)
            {
                if ((subjects.Count > 1) || SVMChunkSimilarity.isActive)
                {
                    if (semanticSimilarity.isActive || SVMChunkSimilarity.isActive || cosineSemanticSimilarity.isActive)
                    {


                        List<IPipelineTaskSubject> MCChunks = subjects.GetSubjectChildrenTokenType<IPipelineTaskSubject, IPipelineTaskSubject>(new cnt_level[] { cnt_level.mcChunk }, true); // sites.getAllChildren();  //context.exitSubjects.GetSubjectsOfLevel<IPipelineTaskSubject>(cnt_level.mcTokenStream);

                        if (!MCChunks.Any())
                        {
                            throw new aceScienceException("No chunks found from [" + subjects.Count + "] web sites", null, subjects, "FVE Chunk construction :: Pipeline context returned no chunks");
                        }

                        knowledge.WLChunkTableOfIndustryClass.Clear();
                        knowledge.WLChunkTableOfIndustryClass = chunkTableConstructor.process(MCChunks, cnt_level.mcPage, knowledge.WLChunkTableOfIndustryClass, null, logger, subjects.Count == 1);
                    }
                }
            } else
            {

            }

            if (knowledge.doBuildSemanticCloud)
            {
                if ((subjects.Count > 1))
                {
                   
                    if (knowledge.WLChunkTableOfIndustryClass.Count > 0)
                    {
                        if (knowledge.semanticCloud.Any())
                        {
                            if (tools.operation.doUseExistingKnowledge)
                            {
                                logger.log(" ::: Rebuilding semantic cloud for [" + subjects.Count + "] subjects, despite the cloud already had [" + knowledge.semanticCloud.CountNodes() + "] nodes and doUseExistingKnowledge=true !! ");
                                logger.log(" ::: This is not proper behaviour --- seems the code has bugs :)");
                            }
                        }

                        knowledge.semanticCloud.Clear();
                        knowledge.semanticCloud = CloudConstructor.process(knowledge.WLChunkTableOfIndustryClass, knowledge.WLTableOfIndustryClass, knowledge.semanticCloud, logger, subjects, tools.GetLemmaResource());
                        knowledge.semanticCloud.name = knowledge.name;

                        knowledge.semanticCloud.description = "Original semantic cloud, extracted from chunks";

                        if (tools.operation.doUseSimpleGraphs)
                        {
                            knowledge.semanticCloud.GetSimpleGraph(true).Save(knowledge.folder.pathFor("class_" + knowledge.semanticCloud.className + "_initialCloud", getWritableFileMode.overwrite));
                        }
                        else
                        {
                            var converter = lemmaSemanticCloud.GetDGMLConverter();
                            converter.ConvertToDMGL(knowledge.semanticCloud).Save(knowledge.folder.pathFor("class_" + knowledge.semanticCloud.className + "_initialCloud", getWritableFileMode.overwrite));
                        }

                        //if (tools.operation.doMakeGraphForClassClouds)
                        //{
                        //    var converter = lemmaSemanticCloud.GetDGMLConverter();
                        //    converter.ConvertToDMGL(knowledge.semanticCloud).Save(knowledge.folder.pathFor(knowledge.name + "_initialCloud", getWritableFileMode.overwrite, "Semantic cloud in initial state - before Cloud Matrix filter applied"));
                        //}

                        if (knowledge.semanticCloud.CountNodes() == 0)
                        {
                            throw new aceScienceException("Semantic cloud [" + knowledge.name + "] construction failed -- zero nodes produced!", null, knowledge, "Sound cloud construction failed", subjects);
                        }

                    }   
                   
                }
            }

               

            if (tools.DoReport)
            {
             //   knowledge.WLTableOfIndustryClass.GetDataTable().GetReportAndSave(knowledge.folder, appManager.AppInfo, "wfl_" + knowledge.name);
              //  knowledge.WLChunkTableOfIndustryClass.GetDataTable().GetReportAndSave(knowledge.folder, appManager.AppInfo, "wfl_" + knowledge.name + "_chunks");
            }
        }

        private Random rnd = new Random();



        /// <summary>
        /// Gets the classification.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="caseSet">The case set.</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        public override WebSiteClassifierResult GetClassification(DocumentSetCase target, DocumentSetCaseCollectionSet caseSet, ILogBuilder logger)
        {
            if (target == null)
            {
                logger.log("-- target is null -- [GetClassification]");
                return null;
            }

            semanticFVExtractorKnowledge caseKnowledge = target.caseKnowledge as semanticFVExtractorKnowledge;

            List<webLemmaTerm> caseTerms = caseKnowledge.WLTableOfIndustryClass.GetList();
            //  StringBuilder sb = new StringBuilder();
           
            foreach (DocumentSetCaseCollection caseColl in caseSet.Values)
            {

                Boolean doReportInDetail = caseSet.validationCase.context.tools.operation.doMakeClassificationReportForCases;

                if (caseSet.validationCase.context.tools.operation.DoRandomCaseGraphReportMode && doReportInDetail)
                {
                    Int32 r = rnd.Next(100);
                    if (r <= caseSet.validationCase.context.tools.operation.In100RandomCaseGraphReport)
                    {
                        doReportInDetail = true;
                    }
                    else
                    {
                        doReportInDetail = false;
                    }
                }
                else
                {
                    
                }

                semanticFVExtractorKnowledge classKnowledge = caseColl.classKnowledge as semanticFVExtractorKnowledge;
                webLemmaTermPairCollection lemmaOverlap = null;

                if (semanticSimilarity.isActive)
                {

                    var expandedCloud = classKnowledge.semanticCloudFiltered.ExpandTermsToCloud(caseTerms, settings.caseTermExpansionSteps, true, settings.caseTermExpansionOptions);
                    //expandedCloud.InverseWeights(true, true);
                    //expandedCloud.normalizeNodeWeights();
                   // expandedCloud.normalizeLinkWeights();
                    
                    lemmaOverlap = classKnowledge.semanticCloudFiltered.GetMatchingTerms(caseTerms, true);

                    SSRMComputation debug = null;
                    if (doReportInDetail)
                    {
                        debug = new SSRMComputation(classKnowledge.name, caseKnowledge.name);

                    }

                    Double Similarity = expandedCloud.GetSSRM(lemmaOverlap, logger, debug);

                    

                    target.data.featureVectors[caseColl.setClass.classID][semanticSimilarity] += Similarity;

                    target.data.featureVectors[caseColl.setClass.classID].termMatched += lemmaOverlap.Count;

                    if (doReportInDetail)
                    {

                       // var dt = lemmaOverlap.GetDataTable();
                      //  dt.GetReportAndSave(caseColl.setClass.folder, appManager.AppInfo, "cosine_similarity_" + caseKnowledge.name + "_" + classKnowledge.name);

                        freeGraphToDMGL converter = new freeGraphToDMGL();

                        String dgmlOutput = "expandedCloud_" + caseKnowledge.name + "_" + classKnowledge.name + ".dgml";

                        var dgml = converter.ConvertToDMGL(expandedCloud);

                        if (debug != null)
                        {
                            var simNode = dgml.Nodes.AddNode("sim", "Sim(d,t) = " + debug.similarity.ToString("F5"));
                            var simUp = dgml.Nodes.AddNode("up", debug.upper.ToString("F5"));
                            var simLow = dgml.Nodes.AddNode("low", debug.lower.ToString("F5"));

                            dgml.Links.AddLink(simNode, simUp, "Above fraction");
                            dgml.Links.AddLink(simNode, simLow, "Below fraction");


                        }

                        dgml.Save(caseSet.validationCase.caseSampleFolder.pathFor(dgmlOutput, getWritableFileMode.autoRenameThis, "DGML export of expanded terms for [" + caseKnowledge.name + "] against class cloud [" + classKnowledge.name + "]"));

                      

                        logger.log("DGML Saved [" + dgmlOutput + "]");
                        
                    }

                    if (debug != null)
                    {
                       
                        File.WriteAllText(caseSet.validationCase.caseSampleFolder.pathFor(debug.GetFilename(), imbSCI.Data.enums.getWritableFileMode.overwrite), debug.sb.ToString());
                    }
                }

                if (cosineSemanticSimilarity.isActive)
                {
                    var caseLemmaDictionary = lemmaSemanticCloud.GetWebLemmaDictionary(caseKnowledge.semanticCloud.nodes);

                    List<webLemmaTerm> expandedTerms = classKnowledge.semanticCloudFiltered.ExpandTerms(caseTerms, settings.caseTermExpansionSteps, settings.caseTermExpansionOptions);
                    
                    var cloudOverlap = classKnowledge.semanticCloudFiltered.GetMatchingTerms(expandedTerms);

                    if (doReportInDetail)
                    {
                        var dt = cloudOverlap.GetDataTable();
                        dt.GetReportAndSave(caseSet.validationCase.caseSampleFolder, appManager.AppInfo, "cosine_similarity_" + caseKnowledge.name + "_" + classKnowledge.name, true, caseSet.validationCase.context.tools.operation.doReportsInParalell);

                    }


                    target.data.featureVectors[caseColl.setClass.classID][cosineSemanticSimilarity] += cloudOverlap.GetCosineSimilarity(logger);
                }

                if (SVMSimilarity.isActive)
                {
                    lemmaOverlap = classKnowledge.WLTableOfIndustryClass.GetMatchingTerms(caseKnowledge.WLTableOfIndustryClass);
                    target.data.featureVectors[caseColl.setClass.classID][SVMSimilarity] += lemmaOverlap.GetCosineSimilarity(logger);
                }

                if (SVMChunkSimilarity.isActive)
                {
                  //  lemmaOverlap = classKnowledge.WLChunkTableOfIndustryClass.GetMatchingTerms(caseKnowledge.WLChunkTableOfIndustryClass);

                    Double similarity = 0;
                    foreach (var primChunk in classKnowledge.semanticCloudFiltered.primaryChunks)
                    {
                        if (caseKnowledge.WLChunkTableOfIndustryClass.ContainsKey(primChunk))
                        {
                            similarity += caseKnowledge.WLChunkTableOfIndustryClass[primChunk].documentFrequency.GetRatio(caseKnowledge.WLChunkTableOfIndustryClass.meta.maxDF);
                        } 
                    }

                    foreach (var primChunk in classKnowledge.semanticCloudFiltered.secondaryChunks)
                    {
                        if (caseKnowledge.WLChunkTableOfIndustryClass.ContainsKey(primChunk))
                        {
                            similarity += (caseKnowledge.WLChunkTableOfIndustryClass[primChunk].documentFrequency.GetRatio(caseKnowledge.WLChunkTableOfIndustryClass.meta.maxDF)) * 0.25;
                        }
                    }

                    target.data.featureVectors[caseColl.setClass.classID][SVMChunkSimilarity] += similarity;
                }
                
                
            }

           

            //target.result.selected = target.result.GetClassWithHighestScore();

            // <---------------------------------- ovde treba da se desi poziv ka klasifikatoru

               // sb.AppendLine("kNN used - class selected is: " + c.ToString() + " [" + target.result.selected.name + "]");
            

            //String path = caseKnowledge.folder.pathFor(caseKnowledge.name + "_log.txt");
            //File.WriteAllText(path, sb.ToString());

            return target.data.featureVectors;
        }

        [XmlIgnore]
        public FeatureVectorDefinition semanticSimilarity { get { return settings.featureVectors["SSRM"]; } }

        [XmlIgnore]
        public FeatureVectorDefinition SVMSimilarity { get { return settings.featureVectors["SVM"]; } }

        [XmlIgnore]
        public FeatureVectorDefinition SVMChunkSimilarity { get { return settings.featureVectors["SVMChunk"]; } }

        [XmlIgnore]
        public FeatureVectorDefinition cosineSemanticSimilarity { get { return settings.featureVectors["cSSRM"]; } }

        public override void BuildFeatureVectorDefinition()
        {
            if (cosineSemanticSimilarity == null) settings.featureVectors.Add("cSSRM", "Cosine similarity between Semantic Cloud of the class and Case TF-IDF");
            if (semanticSimilarity == null) settings.featureVectors.Add("SSRM", "SSRM similarity between Semantic Cloud of the class and DocumentSetCase Web Lemma TF-IDF");
            if (SVMSimilarity == null) settings.featureVectors.Add("SVM", "Cosine similarity between DocumentSetClass Web Lemma TF-IDF and DocumentSetCase Web Lemma TF-IDF").isActive = false;
            if (SVMChunkSimilarity == null) settings.featureVectors.Add("SVMChunk", "Cosine similarity between DocumentSetClass POS Chunk TF-IDF and DocumentSetCase POS Chunk TF-IDF").isActive = false;
        }



    }

}