// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebFVExtractorBase.cs" company="imbVeles" >
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
using imbSCI.Core.files;
using imbSCI.Core.extensions.table;
using imbSCI.DataComplex.tables;
using imbSCI.Data;
using imbSCI.DataComplex.tf_idf;
using imbWBI.Core.WebClassifier.core;
using System.Text;
using imbWBI.Core.WebClassifier.cases;
using System.Xml.Serialization;
using imbWBI.Core.WebClassifier.validation;
using imbACE.Core;
using imbSCI.DataComplex.tables;
using imbSCI.Core.files.folders;
using System.IO;
using imbNLP.PartOfSpeech.decomposing.chunk;
using imbNLP.PartOfSpeech.TFModels.industryLemma;
using imbMiningContext.TFModels.ILRT;
using System.Collections;
using imbSCI.Data.interfaces;
using System.ComponentModel;
using imbSCI.Core.attributes;
using imbNLP.PartOfSpeech.TFModels.webLemma.table;

namespace imbWBI.Core.WebClassifier.wlfClassifier
{


    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TSettings">The type of the settings.</typeparam>
    /// <seealso cref="imbWBI.Core.WebClassifier.wlfClassifier.WebFVExtractorCommonBase{T}" />
    /// <seealso cref="imbWBI.Core.WebClassifier.core.IWebFVExtractor" />
    public abstract class WebFVExtractorBase<T, TSettings>: WebFVExtractorCommonBase<T>, IWebFVExtractor 
        where T:class, IWebFVExtractorKnowledge, new()
        where TSettings:class, IFVExtractorSettings, new()
    {

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public String name { get; set; } = "WebClassifier";

        public String description { get; set; } = "-";
        
        public TSettings settings { get; set; } = new TSettings();

        
        IFVExtractorSettings IWebFVExtractor.settings { get => settings; }
        

        protected WebFVExtractorBase()
        {
            
        }





        /// <summary>
        /// Builds the feature vector definition.
        /// </summary>
        public abstract void BuildFeatureVectorDefinition();


        /// <summary>
        /// Gets the classification.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="caseSet">The case set.</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        public abstract WebSiteClassifierResult GetClassification(DocumentSetCase target, DocumentSetCaseCollectionSet caseSet, ILogBuilder logger);

        /// <summary>
        /// Does the FeatureVector knowledge post-processing.
        /// </summary>
        /// <param name="validationCase">The validation case.</param>
        /// <param name="tools">The tools.</param>
        /// <param name="logger">The logger.</param>
        public abstract void DoFVPostProcessing(kFoldValidationCase validationCase, classifierTools tools, ILogBuilder logger);
        
        /// <summary>
        /// Does the make knowledge.
        /// </summary>
        /// <param name="subjects">The subjects.</param>
        /// <param name="tools">The tools.</param>
        /// <param name="knowledge">The knowledge.</param>
        /// <param name="logger">The logger.</param>
        public abstract void DoMakeKnowledge(List<pipelineTaskMCSiteSubject> subjects, classifierTools tools, T knowledge, ILogBuilder logger);



        /// <summary>
        /// Does the training.
        /// </summary>
        /// <param name="validationCase">The validation case.</param>
        /// <param name="tools">The tools.</param>
        /// <param name="logger">The logger.</param>
        public void DoFVEAndTraining(kFoldValidationCase validationCase, classifierTools tools, ILogBuilder logger)
        {

            //tools.SetReportAndCacheFolder(validationCase.folder);

           // DocumentSetCaseCollectionSet caseCollectionSet = new DocumentSetCaseCollectionSet(validationCase, classes);

            foreach (validationCaseCollection vCaseColl in validationCase.trainingCases)
            {
                IDocumentSetClass documentSetClass = classes[vCaseColl.className];
                //tools.SetLemmaResource(documentSetClass);
                if (doUseOptimizedClassKnowledgeBuilding && tools.DoUseExistingKnowledge)
                {
                    DoFVExtractionForClassViaCases(vCaseColl, documentSetClass, validationCase, tools, logger);
                }
                else
                {
                    DoFVExtractionForClass(vCaseColl, documentSetClass, validationCase.folder, tools, logger);
                }
                //    tools.SaveAndReset(documentSetClass);
            }

            validationCase.knowledgeLibrary.SaveKnowledgeInstancesForClasses(validationCase, logger);

            DoFVPostProcessing(validationCase, tools, logger);

            DoPostTrainingProcedure(validationCase, tools, logger);

        }


       




        /// <summary>
        /// Does the make knowledge for case.
        /// </summary>
        /// <param name="setCase">The set case.</param>
        /// <param name="tools">The tools.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="logger">The logger.</param>
        public void DoMakeKnowledgeForCase(DocumentSetCase setCase, classifierTools tools, DocumentSetCaseCollection parent, ILogBuilder logger)
        {
            T knowledge = parent.knowledgeLibrary.GetKnowledgeInstance<T>(setCase, parent.validationCase, logger);
            
            knowledge.SetRebuild(!tools.DoUseExistingKnowledge);

            DoMakeKnowledge(new List<pipelineTaskMCSiteSubject> { setCase.subject }, tools, knowledge, logger);
            setCase.caseKnowledge = knowledge;

            if (tools.operation.doSaveKnowledgeForCases)
            {
                parent.knowledgeLibrary.SaveCaseKnowledge<T>(setCase as DocumentSetCase, parent.validationCase, logger);
            }
            //SetKnowledge(knowledge);
            //knowledge.OnBeforeSave();
        }

        [XmlIgnore]
        public Boolean doUseOptimizedClassKnowledgeBuilding { get; set; } = true;

        public abstract T DoFVExtractionForClassViaCases(validationCaseCollection vCaseColl, IDocumentSetClass documentSetClass, kFoldValidationCase validationCase, classifierTools tools, ILogBuilder logger);

        /// <summary>
        /// Does the training for one class. --- old method
        /// </summary>
        /// <param name="valCol">The validation case.</param>
        /// <param name="documentSetClass">The document set class.</param>
        /// <param name="valCaseFolder">The value case folder.</param>
        /// <param name="tools">The tools.</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        public IWebFVExtractorKnowledge DoFVExtractionForClass(validationCaseCollection valCol, IDocumentSetClass documentSetClass, folderNode valCaseFolder, classifierTools tools, ILogBuilder logger)
        {
            T knowledge = valCol.kFoldMaster.knowledgeLibrary.GetKnowledgeInstance<T>(documentSetClass, valCol.kFoldCase,  logger);
            //T knowledge = GetKnowledge(documentSetClass.name, valCaseFolder, WebFVExtractorKnowledgeType.aboutCategory, logger);
            
            knowledge.SetRebuild(!tools.DoUseExistingKnowledge);
             

            if (knowledge.ShouldBuildAny())
            {
                
                var context = tools.context.pipelineCollection.GetContext(tools, documentSetClass);
                
                var sites = context.exitByType[typeof(pipelineTaskMCSiteSubject)].ConvertList<IPipelineTaskSubject, pipelineTaskMCSiteSubject>().ToList();
                sites = valCol.FilterSites(sites);

                //foreach (string vc in valCol)
                //{

                //}

                //foreach (pipelineTaskMCSiteSubject site in sites)
                //{
                //    valCol.kFoldMaster.knowledgeLibrary.GetKnowledgeInstance<T>()
                //}


                DoMakeKnowledge(sites, tools, knowledge, logger);
            }

          //  SetKnowledge(knowledge);
            //knowledge.OnBeforeSave();

            logger.log("Feature Extraction by [" + name + "][" + valCol.kFoldCase.name+"][" + documentSetClass.name + "] done for " + valCol.className);
            
            return knowledge;
        }


        /// <summary>
        /// Does the post training procedure.
        /// </summary>
        /// <param name="validationCase">The validation case.</param>
        /// <param name="tools">The tools.</param>
        /// <param name="logger">The logger.</param>
        protected virtual void DoPostTrainingProcedure(kFoldValidationCase validationCase, classifierTools tools, ILogBuilder logger)
        {
            DocumentSetCaseCollectionSet trainingSet = DoClassification(validationCase, tools, logger, true);

            foreach (IWebPostClassifier pClass in validationCase.context.setup.classifiers)
            {
                pClass.DoTraining(trainingSet, tools, logger);
            }

        }


        

        //protected virtual void DoPostClassification()

        /// <summary>
        /// Does the classification.
        /// </summary>
        /// <param name="validationCase">The validation case.</param>
        /// <param name="tools">The tools.</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        public DocumentSetCaseCollectionSet DoClassification(kFoldValidationCase validationCase, classifierTools tools, ILogBuilder logger, Boolean ClassifyTrainingCases=false)
        {
            //tools.SetReportAndCacheFolder(validationCase.folder);

            DocumentSetCaseCollectionSet caseCollectionSet = new DocumentSetCaseCollectionSet(validationCase, classes);

            validationCaseCollectionSet cases = validationCase.evaluationCases;
            if (ClassifyTrainingCases) cases = validationCase.trainingCases;

            foreach (validationCaseCollection vCaseColl in cases)
            {
                IDocumentSetClass documentSetClass = classes[vCaseColl.className];
                tools.SetLemmaResource(documentSetClass);

                DocumentSetCaseCollection caseCollection = new DocumentSetCaseCollection(documentSetClass);
                String kname = documentSetClass.name;
                //if (ClassifyTrainingCases) kname = kname + "_trainingFV";

                T knowledge = validationCase.knowledgeLibrary.GetKnowledgeInstance<T>(documentSetClass, validationCase, logger);

                //T knowledge = GetKnowledge(kname, validationCase.folder, WebFVExtractorKnowledgeType.aboutCategory, logger);
                
                caseCollection.classKnowledge = knowledge;



                caseCollection.deploy(vCaseColl, validationCase, tools.context.pipelineCollection.GetContext(tools, documentSetClass).exitSubjects, classes);

                caseCollectionSet.Add(documentSetClass.classID, caseCollection);

                foreach (DocumentSetCase setCase in caseCollection)
                {
                    DoMakeKnowledgeForCase(setCase, tools, caseCollection, logger);
                }

               

                tools.SaveAndReset(documentSetClass);

            }

            foreach (var vCaseColl in caseCollectionSet)
            {
                DoClassification(vCaseColl.Value, caseCollectionSet, logger);

                if (!ClassifyTrainingCases)
                {
                    //logger.log("Starting with _WebPostClassifiers_ for [" + validationCase.name + "]");
                    foreach (IWebPostClassifier classifier in validationCase.context.setup.classifiers)
                    {

                        foreach (var setCase in vCaseColl.Value)
                        {
                            classifier.DoSelect(setCase, caseCollectionSet, logger);
                        }
                      //  logger.log("Classification done by _" + classifier.name + "_");
                    }

                }

               // var repTable = vCaseColl.Value.GetReportTable(ClassifyTrainingCases);

                if (!ClassifyTrainingCases)
                {
                    
                 //   repTable.GetReportAndSave(validationCase.folder, appManager.AppInfo, vCaseColl.Value.setClass.name);
                  //  repTable.SaveXML(validationCase.folder, "rep_" + vCaseColl.Value.validation.className);
                } else
                {
                    
                    //repTable.AddExtra("-- This report contains training Feature Vectors --");
                    //repTable.GetReportAndSave(validationCase.folder, appManager.AppInfo, vCaseColl.Value.setClass.name + "_FVin");
                    //repTable.SaveXML(validationCase.folder, "rep_" + vCaseColl.Value.validation.className + "_FVin");
                }

               

            }



            //var report = caseCollectionSet.GetCaseReport();
            //report.
            //    .saveObjectToXML(validationCase.folder.pathFor("testReport.xml"));




            return caseCollectionSet;
        }


        /// <summary>
        /// Does the classification.
        /// </summary>
        /// <param name="cases">The cases.</param>
        /// <param name="logger">The logger.</param>
        public void DoClassification(DocumentSetCaseCollection cases, DocumentSetCaseCollectionSet caseSet, ILogBuilder logger)
        {
            if (!cases.Any())
            {
                logger.log("NO CASES DEFINED");
            }


            foreach (DocumentSetCase setCase in cases)
            {

                var result = GetClassification(setCase, caseSet, logger);

            }
        }

        public abstract List<string> DescribeSelf();

        
        //public abstract void BuildFeatureVectorDefinition();
        

        
    }

}