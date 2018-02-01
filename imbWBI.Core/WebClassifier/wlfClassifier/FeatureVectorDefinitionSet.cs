// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FeatureVectorDefinitionSet.cs" company="imbVeles" >
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

namespace imbWBI.Core.WebClassifier.wlfClassifier
{

    public class FeatureVectorDefinitionSet
    {

        public FeatureVectorDefinitionSet()
        {

        }

        /// <summary>
        /// Creates multi-line description of current configuration
        /// </summary>
        /// <returns>List of description lines</returns>
        public List<string> DescribeSelf()
        {
            List<String> output = new List<string>();

            if (Count() == 1)
            {
                output.Add("### One Feature Vector is extracted");

            }
            else
            {
                output.Add("### [" + Count() + "] Feature Vectors are supported by the Extractor");
            }


            foreach (var pair in serialization)
            {
                if (pair.isActive)
                {

                    output.Add("[" + pair.id.ToString("D3") + "]   " + pair.name + "    Active[" + pair.isActive.ToString() + "]");
                   // output.Add("      > " + pair.description);
                    if (pair.doNormalizeVector)
                    {
                        output.Add("      > The vector is normalized (min-max to 0-1)");
                    }
                    if (pair.Factor != 1)
                    {
                        output.Add("      > The vector is modified by weight factor: " + pair.Factor.ToString("F5"));
                    }
                }
            }

            output.Add("--- Remark: Only Feature Vector(s) with Active[true] are shown");

            return output;
        }


        public Int32 Count() => serialization.Count;
        
        public FeatureVectorDefinition this[String name]
        {
            get
            {
                foreach (var fv in serialization)
                {
                    if (fv.name.Equals(name, StringComparison.InvariantCultureIgnoreCase)) return fv;
                }
                return null;
            }
        }

        public FeatureVectorDefinition this[Int32 id]
        {
            get
            {
                foreach (var fv in serialization)
                {
                    if (fv.id == id) return fv;
                }
                return null;
            }
        }

        /// <summary>
        /// Adds new FeatureVector definition.
        /// </summary>
        /// <param name="_name">The name.</param>
        /// <param name="_desc">The desc.</param>
        /// <param name="_factor">The factor.</param>
        /// <param name="_doNormalizeVector">if set to <c>true</c> [do normalize vector].</param>
        /// <returns></returns>
        public FeatureVectorDefinition Add(String _name, String _desc, Double _factor=1, Boolean _doNormalizeVector=false)
        {
            FeatureVectorDefinition output = new FeatureVectorDefinition();
            output.name = _name;
            output.description = _desc;
            output.Factor = _factor;
            output.doNormalizeVector = _doNormalizeVector;
            return Add(output);
        }

        /// <summary>
        /// Adds new FeatureVector definition.
        /// </summary>
        /// <param name="item">The FeatureVector to add into definition set</param>
        /// <returns></returns>
        public FeatureVectorDefinition Add(FeatureVectorDefinition item)
        {
            Int32 key = serialization.Count;
            item.id = key;
            if (!serialization.Any(x=>x.id == key))
            {
                
                serialization.Add(item);
                return item;
            } else
            {
                return this[key];
            }
            
        }


        /// <summary>
        /// Collection of feature vectors used for serialization
        /// </summary>
        /// <value>
        /// The serialization.
        /// </value>
        public List<FeatureVectorDefinition> serialization { get; set; } = new List<FeatureVectorDefinition>();

        
    }

}