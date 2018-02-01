// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebPostClassifierBase.cs" company="imbVeles" >
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
using Accord.MachineLearning;
using Accord.Math.Distances;
using Accord.IO;
using System.Collections.Concurrent;

namespace imbWBI.Core.WebClassifier.wlfClassifier
{
    public interface IWebPostClassifier
    {
        String name { get; set; }
        String GetExperimentSufix();
        


        void DoTraining(DocumentSetCaseCollectionSet trainingSet, classifierTools tools, ILogBuilder logger);
        void DoSelect(DocumentSetCase target, DocumentSetCaseCollectionSet caseSet, ILogBuilder logger);
        void Deploy(WebPostClassifierSettings _setup);

        /// <summary>
        /// Describes self in multiple lines. Description contains the most important settings and way of operation
        /// </summary>
        /// <returns></returns>
        List<String> DescribeSelf();
    }

    public interface IWebPostClassifierState
    {


        folderNode folder { get; set; }
        String name { get; set; }
        AITrainingData data { get; set; }

        void SaveState();
        Boolean LoadState(Boolean onlyIfRequired = false);
    }

    public class WebPostClassifierStates<T>:ConcurrentDictionary<String, WebPostClassifierState<T>> where T : class
    {
        /// <summary>
        /// Sets <see cref="NumberOfCases"/> and other information, and returns training data
        /// </summary>
        /// <param name="trainingSet">The training set.</param>
        /// <returns></returns>
        public WebPostClassifierState<T> SetState(DocumentSetCaseCollectionSet trainingSet, String prefix, T _machine=null)
        {

            var state = new WebPostClassifierState<T>(trainingSet.validationCase.folder, prefix + trainingSet.validationCase.name, _machine);
            

            AITrainingData data = trainingSet.GetAITrainingData();

            state.data = data;
            state.SaveState();

            TryAdd(state.name, state);

            return state;
        }

        public WebPostClassifierState<T> GetState(DocumentSetCaseCollectionSet caseSet, String prefix)
        {
            String key = prefix + caseSet.validationCase.name;
            var state = this[key];

            state.LoadState(true);
            return state;
        }
    }

    public class WebPostClassifierState<T>:IWebPostClassifierState where T : class
    {
        public WebPostClassifierState(folderNode _folder, String _name, T _machine)
        {
            folder = _folder;
            name = _name;
            machine = _machine;
        }

        public folderNode folder { get; set; }

        public String name { get; set; } = "";

        public T machine { get; set; } = null;

        public AITrainingData data { get; set; }

        public Double errorRate { get; set; } = 1;

        public Int32 trainingIteration { get; set; } = 0;

        protected String GetStatePath()
        {
            return folder.pathFor(name + ".bin", imbSCI.Data.enums.getWritableFileMode.none, "Binary dump of [" + name + "] classifier model/memory - after training performed.");
        }

        public void SaveState()
        {
            if (machine != null) machine.Save(GetStatePath());
        }

        public Boolean LoadState(Boolean onlyIfRequired = false)
        {
            if (onlyIfRequired)
            {
                if (machine != null) return true;
            }
            String path = GetStatePath();
            if (File.Exists(path))
            {
                machine = Serializer.Load<T>(path);
                if (machine != null) return true;
            }
            return false;
        }


    }

    public abstract class WebPostClassifierBase<T>:IWebPostClassifier where T:class
    {
        protected WebPostClassifierBase()
        {

        }

        protected WebPostClassifierSettings setup { get; set; }
        public void Deploy(WebPostClassifierSettings _setup)
        {
            setup = _setup;
            name = _setup.name;
        }

        /// <summary>
        /// Describes self in multiple lines. Description contains the most important settings and way of operation
        /// </summary>
        /// <returns></returns>
        public abstract List<String> DescribeSelf();
        

      

        //[XmlIgnore]
        //public Boolean fullDebugMode { get; set; } = true;


        /// <summary>
        /// Gets or sets the states.
        /// </summary>
        /// <value>
        /// The states.
        /// </value>
        public WebPostClassifierStates<T> states { get; set; } = new WebPostClassifierStates<T>();


        public String name { get; set; } = "";

        public abstract String GetExperimentSufix();
        public abstract void DoTraining(DocumentSetCaseCollectionSet trainingSet, classifierTools tools, ILogBuilder logger);
        public abstract void DoSelect(DocumentSetCase target, DocumentSetCaseCollectionSet caseSet, ILogBuilder logger);
    }

}