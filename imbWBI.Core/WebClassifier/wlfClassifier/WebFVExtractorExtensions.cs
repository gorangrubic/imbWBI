// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebFVExtractorExtensions.cs" company="imbVeles" >
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
using imbNLP.PartOfSpeech.flags.basic;

namespace imbWBI.Core.WebClassifier.wlfClassifier
{

    public static class WebFVExtractorExtensions
    {

        public static Dictionary<itmModelEnum, IWebFVExtractor> GetModelInstances(this IEnumerable<itmModelEnum> models)
        {
            Dictionary<itmModelEnum, IWebFVExtractor> output = new Dictionary<itmModelEnum, IWebFVExtractor>();

            foreach (itmModelEnum m in models)
            {
                output.Add(m, GetModelInstance(m));
            }

            return output;
        }

        public static List<IWebFVExtractor> GetModelInstanceList(this IEnumerable<itmModelEnum> models)
        {
            List<IWebFVExtractor> output = new List<IWebFVExtractor>();

            foreach (itmModelEnum m in models)
            {
                var sc = GetModelInstance(m);
                
                output.Add(sc);
            }

            return output;
        }


        public static IWebFVExtractor GetModelInstance(this itmModelEnum model)
        {
            switch (model)
            {
                case itmModelEnum.semantic:

                    var semTFIDF = new semanticFVExtractor();
                    return semTFIDF;
                    break;
                case itmModelEnum.chunkTFIDF:
                    //return new complexWLFClassifier();
                    break;
                default:
                case itmModelEnum.wordTFIDF:
                    var tfidf = new tfidfFVExtractor();
                    return tfidf;
                    break;

                case itmModelEnum.wordTFIDF_complex:
                    //return new complexWLFClassifier();
                    break;

            }
            return null;
        }

        public static IWebPostClassifier GetClassifierInstance(this WebPostClassifierType model)
        {
            switch (model)
            {
                default:
                case WebPostClassifierType.simpleTopScore:
                    return null;
                    break;
                case WebPostClassifierType.kNearestNeighbors:
                    return new WebPostKNNClassifier();
                    break;
                case WebPostClassifierType.multiClassSVM:
                    return new WebPostMSVMClassifier();
                    break;
                case WebPostClassifierType.naiveBayes:
                    return new WebPostNaiveBayesClassifier();
                    break;
                case WebPostClassifierType.backPropagationActivationNeuralNetwork:
                    return new WebPostBackPropActNNClassifier();
                    break;
            }
        }

        public static List<IWebPostClassifier> GetClassifierInstanceList(this IEnumerable<WebPostClassifierType> models)
        {
            List<IWebPostClassifier> output = new List<IWebPostClassifier>();

            foreach (WebPostClassifierType m in models)
            {
                var sc = GetClassifierInstance(m);

                output.Add(sc);
            }

            return output;
        }

    }

}