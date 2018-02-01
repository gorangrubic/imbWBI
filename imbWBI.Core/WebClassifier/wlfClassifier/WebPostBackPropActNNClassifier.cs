// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebPostBackPropActNNClassifier.cs" company="imbVeles" >
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
using Accord.Neuro;
using Accord.Neuro.Learning;
using Accord.Neuro.ActivationFunctions;
using imbSCI.Core.extensions.io;
using imbSCI.Core.extensions.text;
using imbACE.Core.core.exceptions;

namespace imbWBI.Core.WebClassifier.wlfClassifier
{

    

    /// <summary>
    /// MulticlassSupportVectorMachine classifier
    /// </summary>
    /// {D255958A-8513-4226-94B9-080D98F904A1}<seealso cref="imbWBI.Core.WebClassifier.wlfClassifier.WebPostClassifierBase{Accord.Neuro.ActivationNetwork}" />
    /// {D255958A-8513-4226-94B9-080D98F904A1}<seealso cref="imbWBI.Core.WebClassifier.wlfClassifier.WebPostClassifierBase{Accord.MachineLearning.VectorMachines.MulticlassSupportVectorMachine{Accord.Statistics.Kernels.Linear}}" />
    public class WebPostBackPropActNNClassifier : WebPostClassifierBase<ActivationNetwork>
    {

        //public Double learningRate { get => setup.ne.learningRate; }
        //public Double momentum { get => setup.momentum; } 

      //  public Int32 HiddenLayerOneNeuronCount { get => setup.HiddenLayerOneNeuronCount; }

       // public Int32 HiddenLayerTwoNeuronCount { get => setup.HiddenLayerTwoNeuronCount; } 

        //public Double alpha { get => setup.alpha; }


        public WebPostBackPropActNNClassifier()
        {
            name = "ActNN";
        }

        

        public override void DoSelect(DocumentSetCase target, DocumentSetCaseCollectionSet caseSet, ILogBuilder logger)
        {
            var state = states.GetState(caseSet, GetExperimentSufix());
            Double[] cs = state.machine.Compute(target.data.featureVectors.GetValues().ToArray());
            target.data[this].SetValues(cs);

        }

        public override string GetExperimentSufix()
        {
            String output = name + "_";

            output += setup.neuralnetwork.learningRate.ToString("F2") + "_"; //HiddenLayerOneNeuronCount + "_" + HiddenLayerTwoNeuronCount;
            setup.neuralnetwork.HiddenLayersNeuronCounts.ForEach(x => output = output.add(x.ToString(), "_"));

            return output;
        }

        private ISupervisedLearning teacherRef;
        private IActivationFunction activationFunction;

        public override void DoTraining(DocumentSetCaseCollectionSet trainingSet, classifierTools tools, ILogBuilder logger)
        {
            
            var state = states.SetState(trainingSet, GetExperimentSufix());


            if (activationFunction == null) activationFunction = new BipolarSigmoidFunction(setup.neuralnetwork.alpha);

            var neurons = setup.neuralnetwork.HiddenLayersNeuronCounts.ToList();

            ActivationNetwork machine = null;

            switch (neurons.Count)
            {
                case 0:
                    machine = new ActivationNetwork(new BipolarSigmoidFunction(setup.neuralnetwork.alpha), state.data.NumberOfInputs, state.data.NumberOfClasses);
                    break;
                case 1:
                    machine = new ActivationNetwork(new BipolarSigmoidFunction(setup.neuralnetwork.alpha), state.data.NumberOfInputs, neurons[0], state.data.NumberOfClasses);
                    break;
                case 2:
                    machine = new ActivationNetwork(new BipolarSigmoidFunction(setup.neuralnetwork.alpha), state.data.NumberOfInputs, neurons[0], neurons[1], state.data.NumberOfClasses);
                    break;

                case 3:
                    machine = new ActivationNetwork(new BipolarSigmoidFunction(setup.neuralnetwork.alpha), state.data.NumberOfInputs, neurons[0], neurons[1], neurons[2], state.data.NumberOfClasses);
                    break;

                case 4:
                    machine = new ActivationNetwork(new BipolarSigmoidFunction(setup.neuralnetwork.alpha), state.data.NumberOfInputs, neurons[0], neurons[1], neurons[2], neurons[3], state.data.NumberOfClasses);
                    break;
                case 5:
                    machine = new ActivationNetwork(new BipolarSigmoidFunction(setup.neuralnetwork.alpha), state.data.NumberOfInputs, neurons[0], neurons[1], neurons[2], neurons[3], neurons[4], state.data.NumberOfClasses);
                    break;
                case 6:
                    machine = new ActivationNetwork(new BipolarSigmoidFunction(setup.neuralnetwork.alpha), state.data.NumberOfInputs, neurons[0], neurons[1], neurons[2], neurons[3], neurons[4], neurons[5], state.data.NumberOfClasses);
                    break;
                case 7:
                    machine = new ActivationNetwork(new BipolarSigmoidFunction(setup.neuralnetwork.alpha), state.data.NumberOfInputs, neurons[0], neurons[1], neurons[2], neurons[3], neurons[4], neurons[5], neurons[6], state.data.NumberOfClasses);
                    break;
                default:
                    throw new aceGeneralException("At current implementation NN with [" + neurons.Count + "] hidden layers is not allowed.", null, this, "To high number of hidden layers");
                    break;
            }

            new NguyenWidrow(machine).Randomize();
            state.machine = machine;

           // BackPropagationLearning teacher = new BackPropagationLearning(machine);
            LevenbergMarquardtLearning teacher = new LevenbergMarquardtLearning(machine);
            teacher.LearningRate = setup.neuralnetwork.learningRate;

            var outputs = Accord.Statistics.Tools.Expand(state.data.outputs, state.data.NumberOfClasses, -1, 1);
            //teacher.Momentum = momentum;
            Int32 itOfSameError = 0;
            Int32 itOfSameErrorLimit = setup.neuralnetwork.learningIterationsMax / 10;
            Double errorSignificantSpan = setup.neuralnetwork.errorLowerLimit * setup.neuralnetwork.errorLowerLimit;
            for (int i = 0; i < setup.neuralnetwork.learningIterationsMax; i++)
            {

                double error = teacher.RunEpoch(state.data.inputs, outputs);

                if (Math.Abs(error - state.errorRate)< errorSignificantSpan)
                {
                    itOfSameError++;


                }

                if (itOfSameError > itOfSameErrorLimit)
                {
                    logger.log("Stoping training in [" + i.ToString("D3") + "] because error rate had no significant change [" + errorSignificantSpan.ToString("F8") +"] in last [" + itOfSameError + "] iterations [" + error.ToString("F8")+"]");
                    break;
                }
                if (i % 10 == 0)
                {
                    logger.log("Learning Neural Network [" + i.ToString("D3") + "]  Error rate: " +  error.ToString("F5"));
                    
                }
                if (error < state.errorRate)
                {
                    state.errorRate = error;
                }
                if (error<setup.neuralnetwork.errorLowerLimit)
                {
                    break;
                }
            }
            if (teacherRef == null) teacherRef = teacher;
            state.SaveState();

        }

        public override List<string> DescribeSelf()
        {
            List<String> output = new List<string>();

            var pair = states.FirstOrDefault();

            output.Add("Classification using neural network [" + pair.Value.machine.GetType().Name + "] with " + pair.Value.machine.Layers.Count() + " layers.");
            output.Add("Supervised learning [" + teacherRef.GetType().Name + "] with LearningRate = " + setup.neuralnetwork.learningRate + " and Momentum = " + setup.neuralnetwork.momentum + ".");
            output.Add("Learning in max. iterations [" + setup.neuralnetwork.learningIterationsMax + "], terminated earlier if error rate is lower then [" + setup.neuralnetwork.errorLowerLimit.ToString("F5") + "]");

          //  output.Add("Input layer [0] -> [" + pair.Value.data.NumberOfInputs + "]");
            for (int i = 0; i < pair.Value.machine.Layers.Length; i++)
            {
                Layer l = pair.Value.machine.Layers[i];
                output.Add("Layer [" + (i) + "] -> In[" + l.InputsCount + "] -> Neurons[" + l.Neurons.Length + "] -> Out[" + l.Output.Length + "]");
            }
            output.Add("Output layer [" + (pair.Value.machine.Layers.Length) + "] -> [" + pair.Value.data.NumberOfClasses + "]");
            output.Add("Neuron function [" + activationFunction.GetType().Name + "]. Trained with [" + pair.Value.data.NumberOfCases + "] cases.");


            return output;
        }
    }

}