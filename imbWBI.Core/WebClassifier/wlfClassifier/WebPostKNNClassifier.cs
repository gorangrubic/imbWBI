// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebPostKNNClassifier.cs" company="imbVeles" >
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


    public class WebPostKNNClassifier:WebPostClassifierBase<KNearestNeighbors<Double[]>>
    {
        public WebPostKNNClassifier()
        {
            name = "kNN";
        }

        

        private IDistance<double[]> _distance = null;

        public override void DoTraining(DocumentSetCaseCollectionSet trainingSet, classifierTools tools, ILogBuilder logger)
        {

            var state = states.SetState(trainingSet, GetExperimentSufix());

            _distance = new SquareEuclidean();
            var kNearest = new KNearestNeighbors<Double[]>(k: setup.kNN_k, distance:_distance );

            kNearest.Learn(state.data.inputs, state.data.outputs);           
            state.machine = kNearest;

            state.SaveState();
            
        }

        public override void DoSelect(DocumentSetCase target, DocumentSetCaseCollectionSet caseSet, ILogBuilder logger)
        {
            var state = states.GetState(caseSet, GetExperimentSufix());
            var vls = target.data.featureVectors.GetValues();

            Int32 c = state.machine.Decide(vls.ToArray());
            target.data[this].SetValues(c);
            
        }

        public override string GetExperimentSufix()
        {
            String output = name + "_k";

            output += setup.kNN_k.ToString();

            return output;
        }

        public override List<string> DescribeSelf()
        {
            List<String> output = new List<string>();
            var state = states.FirstOrDefault().Value;

            output.Add("Classification using k-Nearest Neighbors [" + state.machine.GetType().Name + "] with K = " + setup.kNN_k + ".");
            output.Add("Distance structure [" + _distance.GetType().Name + "].");
            
            output.Add("Trained with [" + state.data.NumberOfCases + "] cases.");


            return output;
        }
    }

}