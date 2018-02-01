// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebFVExtractorKnowledgeLibrary.cs" company="imbVeles" >
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
using System.Text;
using imbWBI.Core.WebClassifier.wlfClassifier;
using imbWBI.Core.WebClassifier.cases;
using imbSCI.Core.files.folders;
// using imbMiningContext.TFModels.WLF_ISF;
using imbSCI.Core.reporting;
using imbWBI.Core.WebClassifier.validation;
using imbSCI.Data.collection.nested;
using System.Collections.Concurrent;
using System.Threading;
using imbACE.Core.core.exceptions;

namespace imbWBI.Core.WebClassifier.core
{

    /// <summary>
    /// Keeps record on knowledge of the experiment
    /// </summary>
    public class WebFVExtractorKnowledgeLibrary
    {
        public Boolean DoShareCaseKnowledge { get; set; } = false;
        public folderNode ExperimentRootFolder { get; set; } = null;
        public folderNode ExperimentSharedCasesFolder { get; set; } = null;

        public WebFVExtractorKnowledgeLibrary(kFoldValidationCollection _parent)
        {
            validationCollection = _parent;
            if (validationCollection.context.setup.doShareTheCaseKnowledgeAmongFVEModels)
            {
                validationCollection.context.notes.log("::: CASE KNOWLEDGE SHARE MODE IS ENABLED ::: MAKE SURE FVEs IN THE EXPERIMENT ARE DESCRIBING THE CASES ON THE SAME WAY :::");
                ExperimentRootFolder = validationCollection.context.folder;
                ExperimentSharedCasesFolder = ExperimentRootFolder.Add("SharedKnowledge", "Shared Knowledge on cases", "Directory with DocumentSetCase knowledge, shared among different FVE models in this experiment. The share option is set at experiment setup XML.");
                if (!validationCollection.context.tools.operation.doUseExistingKnowledge)
                {
                    validationCollection.context.notes.log(" > doUseExistingKnowledge is FALSE --- to have the case knowledge sharing used, you have to set it to TRUE ------");
                    imbACE.Services.terminal.aceTerminalInput.doBeepViaConsole(2000, 1000, 2);
                }

              
                DoShareCaseKnowledge = true;
            }
            foreach (var vCase in validationCollection.GetCases())
            {
                registry.Add(vCase, new ConcurrentDictionary<string, IWebFVExtractorKnowledge>());
                
            }
        }

        protected Dictionary<kFoldValidationCase, ConcurrentDictionary<String, IWebFVExtractorKnowledge>> registry = new Dictionary<kFoldValidationCase, ConcurrentDictionary<String, IWebFVExtractorKnowledge>>();

        protected ConcurrentDictionary<String, IWebFVExtractorKnowledge> registryForCases = new ConcurrentDictionary<String, IWebFVExtractorKnowledge>();

        protected ConcurrentDictionary<String, Int32> statistics = new ConcurrentDictionary<String, Int32>();


        /// <summary>
        /// Gets or sets the validation collection.
        /// </summary>
        /// <value>
        /// The validation collection.
        /// </value>
        public kFoldValidationCollection validationCollection { get; set; }

        public Boolean doKeepStatisticsOnAccess { get; set; } = true;


        private Object createCaseKnowledgeLock = new Object();


        private Object createKnowledgeLock = new Object();

        protected T CreateKnowledgeInstance<T>(String name, kFoldValidationCase validationCase, WebFVExtractorKnowledgeType type, ILogBuilder logger) where T : class, IWebFVExtractorKnowledge, new()
        {
            folderNode folder = validationCase.caseFolder;
            if (type == WebFVExtractorKnowledgeType.aboutDocumentSet)
            {
               folder = validationCollection.caseFolder;
                if (DoShareCaseKnowledge)
                {
                    folder = ExperimentSharedCasesFolder;
                }
            }
            

            T knowledge = new T();
            knowledge.name = name;
            knowledge.type = type;

            try
            {
                knowledge.Deploy(folder, logger);
            } catch (aceGeneralException ex)
            {
                validationCase.context.errorNotes.LogException("Create Knowledge Instance (" + typeof(T).Name + ") error for [" + name + "] in fold [" + validationCase.name + "]:" + ex.title, ex);
            }

            return knowledge;
        }


        public T GetKnowledgeInstance<T>(DocumentSetCase setCase, kFoldValidationCase validationCase, ILogBuilder logger) where T : class, IWebFVExtractorKnowledge, new()
        {
            T knowledge = GetKnowledgeInstance<T>("case_" + setCase.subject.name, validationCase, WebFVExtractorKnowledgeType.aboutDocumentSet, logger);
            knowledge.relatedItemPureName = setCase.subject.name;
            return knowledge;
        }

        public T GetKnowledgeInstance<T>(IDocumentSetClass setClass, kFoldValidationCase validationCase, ILogBuilder logger) where T : class, IWebFVExtractorKnowledge, new()
        {
            T knowledge = GetKnowledgeInstance<T>("class_" + setClass.name, validationCase, WebFVExtractorKnowledgeType.aboutCategory, logger);
            knowledge.relatedItemPureName = setClass.name;
            return knowledge;
        }

        /// <summary>
        /// Gets the knowledge instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <param name="validationCase">The validation case.</param>
        /// <param name="type">The type.</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        protected T GetKnowledgeInstance<T>(String name, kFoldValidationCase validationCase, WebFVExtractorKnowledgeType type, ILogBuilder logger) where T : class, IWebFVExtractorKnowledge, new()
        {
            IWebFVExtractorKnowledge knowledge = null;
            Boolean found = false;

            if (type == WebFVExtractorKnowledgeType.aboutCategory)
            {
                if (!registry[validationCase].TryGetValue(name, out knowledge))
                {
                    lock (createKnowledgeLock)
                    {
                        if (!registry[validationCase].ContainsKey(name))
                        {
                            statistics[name] = 0;
                            knowledge = CreateKnowledgeInstance<T>(name, validationCase, type, logger);
                            registry[validationCase][name] = knowledge;
                        }
                        else
                        {
                            knowledge = registry[validationCase][name];
                        }
                    }
                } else
                {
                    found = true;
                }
            }
            else
            {
                if (!registryForCases.TryGetValue(name, out knowledge))
                {
                    lock (createCaseKnowledgeLock)
                    {
                        if (!registryForCases.ContainsKey(name))
                        {
                            statistics[name] = 0;
                            knowledge = CreateKnowledgeInstance<T>(name, validationCase, type, logger);
                            registryForCases[name] = knowledge;
                        }
                        else
                        {
                            knowledge = registryForCases[name];
                        }
                    }
                }
                else
                {
                    found = true;
                }

            }

            
            if (doKeepStatisticsOnAccess)
            {
                if (found) statistics[name]++;
            }

            return knowledge as T;
        }


        protected ConcurrentBag<IWebFVExtractorKnowledge> savedKnowledge = new ConcurrentBag<IWebFVExtractorKnowledge>();

        private Object savedKnowledgeLock = new Object();

        public void SaveCaseKnowledge<T>(DocumentSetCase setCase, kFoldValidationCase validationCase, ILogBuilder logger) where T : class, IWebFVExtractorKnowledge, new()
        {
            IWebFVExtractorKnowledge knowledge = GetKnowledgeInstance<T>(setCase, validationCase, logger);
            if (!savedKnowledge.Contains(knowledge))
            {
                lock (savedKnowledgeLock)
                {
                    if (!savedKnowledge.Contains(knowledge))
                    {
                        savedKnowledge.Add(knowledge);
                        knowledge.OnBeforeSave();
                    }
                }
            }

            
        }


        public Dictionary<String, Boolean> reusableKnowledgeSaved = new Dictionary<String, bool>();

        //private Object resourceLock = new Object();
        public void SaveCaseKnowledgeInstances(ILogBuilder logger)
        {
            logger.log("Saving case knowledge objects for [" + validationCollection.extractor.name + "]");
            foreach (IWebFVExtractorKnowledge knowledge in registryForCases.Values)
            {
                
                    knowledge.OnBeforeSave();
                    reusableKnowledgeSaved.Add(knowledge.name, true);
            }
            logger.log("Knowledge case saved for [" + validationCollection.extractor.name + "]");
        }

        public void SaveKnowledgeInstancesForClasses(kFoldValidationCase validationCase, ILogBuilder logger)
        {
            logger.log("Saving knowledge objects for [" + validationCase.name + "] classes");
            foreach (IWebFVExtractorKnowledge knowledge in registry[validationCase].Values)
            {
                knowledge.OnBeforeSave();
            }
            logger.log("Knowledge saved for [" + validationCase.name + "] classes");
        }
    }

}