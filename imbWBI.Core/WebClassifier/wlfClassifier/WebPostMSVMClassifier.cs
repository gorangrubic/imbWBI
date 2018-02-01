// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebPostMSVMClassifier.cs" company="imbVeles" >
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
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Statistics.Kernels;
using Accord.Math.Optimization.Losses;
using Accord.MachineLearning.VectorMachines;

namespace imbWBI.Core.WebClassifier.wlfClassifier
{

    /// <summary>
    /// MulticlassSupportVectorMachine classifier
    /// </summary>
    /// <seealso cref="imbWBI.Core.WebClassifier.wlfClassifier.WebPostClassifierBase{Accord.MachineLearning.VectorMachines.MulticlassSupportVectorMachine{Accord.Statistics.Kernels.Linear}}" />
    public class WebPostMSVMClassifier : WebPostClassifierBase<MulticlassSupportVectorMachine<Linear>>
    {
        
        public Loss lossFunctionForTraining { get => setup.lossFunctionForTraining; }

        public WebPostMSVMClassifier()
        {
            name = "mSVM";
        }

        public override void DoSelect(DocumentSetCase target, DocumentSetCaseCollectionSet caseSet, ILogBuilder logger)
        {
            var state = states.GetState(caseSet, GetExperimentSufix());
            Int32 c = state.machine.Decide(target.data.featureVectors.GetValues().ToArray());
            target.data[this].SetValues(c);
            
        }

        public override string GetExperimentSufix()
        {
            String output = name + "_";

            output += lossFunctionForTraining.ToString();

            return output;
        }

        private MulticlassSupportVectorLearning<Linear> _teacher;

        public override void DoTraining(DocumentSetCaseCollectionSet trainingSet, classifierTools tools, ILogBuilder logger)
        {
            var state = states.SetState(trainingSet, GetExperimentSufix());

            // Create a one-vs-one multi-class SVM learning algorithm 
            MulticlassSupportVectorLearning<Linear> teacher = new MulticlassSupportVectorLearning<Linear>()
            {
                // using LIBLINEAR's L2-loss SVC dual for each SVM
                Learner = (p) => new LinearDualCoordinateDescent()
                {
                    Loss = lossFunctionForTraining
                }
            };

            _teacher = teacher;

            // The following line is only needed to ensure reproducible results. Please remove it to enable full parallelization
            teacher.ParallelOptions.MaxDegreeOfParallelism = 1; // (Remove, comment, or change this line to enable full parallelism)

            // Learn a machine
            state.machine = teacher.Learn(state.data.inputs, state.data.outputs);
            state.SaveState();
            
        }

        public override List<string> DescribeSelf()
        {
            List<String> output = new List<string>();

            var state = states.FirstOrDefault().Value;

            output.Add("Classification using Multiclass Support Vector Machine [" + state.machine.GetType().Name + "] with kernel [" + state.machine.Kernel.GetType().Name + "].");
            output.Add("Method [" + state.machine.Method.GetType().Name + "].");
            
            output.Add("Trained with [" + state.data.NumberOfCases + "] cases, using [" + _teacher.GetType().Name + "]");


            return output;
        }

    }

}