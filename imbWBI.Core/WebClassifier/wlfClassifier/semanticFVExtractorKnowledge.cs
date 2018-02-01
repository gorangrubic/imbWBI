// --------------------------------------------------------------------------------------------------------------------
// <copyright file="semanticFVExtractorKnowledge.cs" company="imbVeles" >
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
using imbWBI.Core.WebClassifier.cases;
using imbWBI.Core.WebClassifier.core;
using imbWBI.Core.WebClassifier.validation;
using System.Text;
using System.Xml.Serialization;
using imbNLP.PartOfSpeech.TFModels.semanticCloud;
using System.IO;
using imbSCI.Data.enums;
using imbNLP.PartOfSpeech.TFModels.webLemma.table;
using imbSCI.Graph.FreeGraph;

namespace imbWBI.Core.WebClassifier.wlfClassifier
{

    /// <summary>
    /// Classifier knowledge
    /// </summary>
    /// <seealso cref="imbWBI.Core.WebClassifier.core.WebFVExtractorKnowledge" />
    public class semanticFVExtractorKnowledge : WebFVExtractorKnowledge
    {
        

        public override void OnBeforeSave()
        {
            if (WLTableOfIndustryClass.Count > 0) WLTableOfIndustryClass.Save();
            if (WLChunkTableOfIndustryClass.Count > 0) WLChunkTableOfIndustryClass.Save();
            if (semanticCloud.CountNodes() > 0) semanticCloud.Save(folder.pathFor(name + "Cloud.xml", getWritableFileMode.overwrite, "Initial version of the semantic cloud, extracted from the Chunk Table"));
            if (semanticCloudFiltered.CountNodes() > 0) semanticCloudFiltered.Save(folder.pathFor(name + "CloudFiltered.xml", getWritableFileMode.overwrite, "Transformed version of the semantic cloud, after the Cloud Matrix was applied"));
        }

        public override void OnLoaded()
        {
            WLTableOfIndustryClass = new webLemmaTermTable(folder.pathFor(name + "WLTable.xml", getWritableFileMode.none, "Web Lemma TF-IDF table for [" + name + "] - as XML Serialized object"), name+"Lemmas");
            WLChunkTableOfIndustryClass = new webLemmaTermTable(folder.pathFor(name + "WLChunkTable.xml", getWritableFileMode.none, "Chunks TF-IDF table for [" + name + "] - as XML Serialized object"),name+"Chunks");
            
            if (semanticCloud == null)
            {
                semanticCloud = lemmaSemanticCloud.Load<lemmaSemanticCloud>(folder.pathFor(name + "Cloud.xml", getWritableFileMode.existing, "Initial version of the semantic cloud, extracted from the Chunk Table"), true);
                semanticCloud.name = name;
            }


            if (semanticCloudFiltered == null)
            {
                semanticCloudFiltered = lemmaSemanticCloud.Load<lemmaSemanticCloud>(folder.pathFor(name + "CloudFiltered.xml", getWritableFileMode.existing, "Initial version of the semantic cloud, extracted from the Chunk Table"), true);
                semanticCloudFiltered.name = name + "flt";
            }

            
        }

       



        [XmlIgnore]
        public webLemmaTermTable WLTableOfIndustryClass { get; set; }

        [XmlIgnore]
        public webLemmaTermTable WLChunkTableOfIndustryClass { get; set; }

        [XmlIgnore]
        public lemmaSemanticCloud semanticCloud { get; set; } = new lemmaSemanticCloud();

        [XmlIgnore]
        public lemmaSemanticCloud semanticCloudFiltered { get; set; } = new lemmaSemanticCloud();

        public Boolean doBuildSemanticCloud
        {
            get
            {
                return !semanticCloud.Any() || doBuild;
            }
        }
        public Boolean doBuildTermTable
        {
            get
            {
                if (doBuild) return true;
                if (WLTableOfIndustryClass.Count > 0) return false;
                return true;
            }
        }

        public Boolean doBuildChunkTable
        {
            get
            {
                if (doBuild) return true;
                if (WLChunkTableOfIndustryClass.Count > 0) return false;
                return true;
            }
        }

        protected Boolean doBuild { get; set; } = true;

        public override void SetRebuild(bool to = true)
        {
            doBuild = to;
        }

        public override bool ShouldBuildAny()
        {

            return doBuild || doBuildChunkTable || doBuildSemanticCloud || doBuildTermTable;
        }
    }

}