// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FeatureVectorDefinition.cs" company="imbVeles" >
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

    /// <summary>
    /// Meta definition of a FeatureVector
    /// </summary>
    /// <seealso cref="imbSCI.Data.interfaces.IObjectWithNameAndDescription" />
    public class FeatureVectorDefinition:IObjectWithNameAndDescription
    {
        public FeatureVectorDefinition()
        {

        }
        [XmlAttribute]
        public String name { get; set; } = "";

        [XmlIgnore]
        public String description { get; set; } = "";



        /// <summary> Position of the Feature Vector in the array </summary>
        [Category("Index")]
        [DisplayName("id")]
        [imb(imbAttributeName.measure_letter, "")]
        [imb(imbAttributeName.measure_setUnit, "n")]
        [Description("Position of the Feature Vector in the array")] // [imb(imbAttributeName.measure_important)][imb(imbAttributeName.reporting_valueformat, "")]
        [XmlAttribute]
        public Int32 id { get; set; } = 0;



        /// <summary> If true it will normalize value to min-max range 0 - 1 </summary>
        [Category("Switch")]
        [DisplayName("doNormalizeVector")]
        [Description("If true it will normalize value to min-max range 0 - 1")]
        [XmlIgnore]
        public Boolean doNormalizeVector { get; set; } = false;


        /// <summary> Feature Vector weight factor </summary>
        [Category("Ratio")]
        [DisplayName("Factor")]
        [imb(imbAttributeName.measure_letter, "F")]
        [imb(imbAttributeName.measure_setUnit, "")]
        [imb(imbAttributeName.reporting_valueformat, "F5")]
        [Description("Feature Vector weight factor - used as value correction")] // [imb(imbAttributeName.measure_important)][imb(imbAttributeName.reporting_escapeoff)]
        [XmlIgnore]
        public Double Factor { get; set; } = 1;



        /// <summary> If <c>true</c> this feature vector will be used </summary>
        [Category("Flag")]
        [DisplayName("isActive")]
        [imb(imbAttributeName.measure_letter, "On")]
        [Description("If <c>true</c> this feature vector will be used")]
        [XmlAttribute]
        public Boolean isActive { get; set; } = true;





    }

}