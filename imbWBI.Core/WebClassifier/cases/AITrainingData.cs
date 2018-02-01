// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AITrainingData.cs" company="imbVeles" >
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
using imbWBI.Core.WebClassifier.core;
using System.Text;
using imbMiningContext.MCRepository;
using imbNLP.PartOfSpeech.pipeline.core;
using imbSCI.Core.files.folders;
using imbNLP.PartOfSpeech.TFModels.webLemma.core;
using imbWBI.Core.WebClassifier.validation;
using imbSCI.Core.math.classificationMetrics;
using System.Data;
using imbSCI.Core.extensions.table;
using imbSCI.DataComplex.extensions.data.schema;
using imbSCI.DataComplex.tables;
using imbWBI.Core.WebClassifier.wlfClassifier;
using imbACE.Network.extensions;
using System.Xml.Serialization;

namespace imbWBI.Core.WebClassifier.cases
{

    public class AITrainingData
    {

        public AITrainingData()
        {

        }

        public void Deploy(DocumentSetCaseCollectionSet input)
        {
            NumberOfClasses = input.Count;
            if (inputs.Length > 0)
            {
                NumberOfInputs = inputs[0].Length;
            }
            NumberOfCases = outputs.Length;

        }
        private Int32 _NumberOfClasses = 0;
        /// <summary> </summary>
        [XmlIgnore]
        public Int32 NumberOfClasses
        {
            get
            {
                return _NumberOfClasses;
            }
            protected set
            {
                _NumberOfClasses = value;
            }
        }


        private Int32 _NumberOfInputs = 0;
        /// <summary> </summary>
        [XmlIgnore]
        public Int32 NumberOfInputs
        {
            get
            {
                return _NumberOfInputs;
            }
            protected set
            {
                _NumberOfInputs = value;
            }
        }


        private Int32 _NumberOfCases = 0;
        /// <summary> </summary>
        [XmlIgnore]
        public Int32 NumberOfCases
        {
            get
            {
                return _NumberOfCases;
            }
            protected set
            {
                _NumberOfCases = value;

            }
        }

        public Double[][] inputs { get; set; } = new Double[][] { };

        public Int32[] outputs { get; set; } = new int[] { };


        //public Double[][] outputs_for_neurons { get; set; } = new Double[][] { };
    }
}