// --------------------------------------------------------------------------------------------------------------------
// <copyright file="webCaseKnowledge.cs" company="imbVeles" >
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
using imbMiningContext.MCDocumentStructure;
using imbMiningContext.MCWebSite;
// using imbMiningContext.TFModels.WLF_ISF;
using imbNLP.PartOfSpeech.pipeline.machine;
using imbNLP.PartOfSpeech.pipelineForPos.subject;
using imbNLP.PartOfSpeech.TFModels.semanticCloud;
using imbNLP.PartOfSpeech.TFModels.webLemma.table;
using imbSCI.Data;
using imbSCI.Data.collection.nested;
using imbWBI.Core.WebClassifier.cases;
using imbWBI.Core.WebClassifier.core;
using imbWBI.Core.WebClassifier.validation;
using imbWBI.Core.WebClassifier.wlfClassifier;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace imbWBI.Core.WebCase
{
    public class webCaseKnowledge
    {

        public webCaseKnowledge()
        {

        }


        public webCaseKnowledge(pipelineTaskMCSiteSubject _MCSiteSubject, IDocumentSetClass classSet)
        {
            SetSiteSubject(_MCSiteSubject);
            name = MCSite.domainInfo.domainRootName;
            industry = classSet;
        }


        private String _name = "";
        /// <summary>
        /// Name for this instance
        /// </summary>
        [Category("Info")]
        [DisplayName("Name")] //[imb(imbAttributeName.measure_letter, "")]
        [Description("Name of the case")]
        public String name
        {
            get { return _name; }
            set { _name = value; }
        }

        private String _description = "";
        /// <summary>
        /// Human-readable description of object instance
        /// </summary>
        [Category("Info")]
        [DisplayName("Description")] //[imb(imbAttributeName.measure_letter, "")]
        [Description("Description of this web case")]
        public String description
        {
            get { return _description; }
            set { _description = value; }
        }


        public void UpdateDescription()
        {
            description = "Industry of [" + industry.name + "], doing [";

            var builder = new StringBuilder();
            builder.Append(description);


            foreach (var n in semanticCloud.primaryNodes)
            {
                var lemma = WLChunkTableOfIndustryClass[n.name];
                if (lemma != null)
                {
                    builder.Append(lemma.name + " ");
                }
            }

            description = builder.ToString();

            
            description += "]";

       


        }



        #region Pipeline knowledge ------------------------------------------------------------------------

        public void SetSiteSubject(pipelineTaskMCSiteSubject _MCSiteSubject)
        {
            MCSiteSubject = _MCSiteSubject;
            MCSite = MCSiteSubject.MCSite;
            MCSiteElement = MCSiteSubject.mcElement;
        }

      
        /// <summary>
        /// Gets or sets the mc element.
        /// </summary>
        /// <value>
        /// The mc element.
        /// </value>
        public imbMCDocumentElement MCSiteElement { get; set; }

        /// <summary>
        /// Mining context web site
        /// </summary>
        public imbMCWebSite MCSite { get; set; }

        [XmlIgnore]
        public pipelineTaskMCSiteSubject MCSiteSubject { get; set; }

        #endregion -----------------------------------------------------------------------------------------


        #region Pipeline knowledge ------------------------------------------------------------------------

        [XmlIgnore]
        public IDocumentSetClass industry { get; set; }  // [done]

        [XmlIgnore]
        public ConcurrentBag<pipelineTaskSubjectContentToken> tokens { get; set; } // [done]

     //   [XmlIgnore]
       // public List<IPipelineTaskSubject> chunks { get; set; }

        [XmlIgnore]
        public ConcurrentDictionary<kFoldValidationCase, DocumentSetCase> kFoldValidationCases { get; set; }

        #endregion -----------------------------------------------------------------------------------------


        #region Extractor knowledge ------------------------------------------------------------------------

        public void SetFVEKnowledge(IWebFVExtractorKnowledge _knowledge)
        {
            if (_knowledge is semanticFVExtractorKnowledge)
            {
                semanticFVExtractorKnowledge knowledge = (semanticFVExtractorKnowledge)_knowledge;
                WLTableOfIndustryClass = knowledge.WLTableOfIndustryClass;
                WLChunkTableOfIndustryClass = knowledge.WLChunkTableOfIndustryClass;
                semanticCloud = knowledge.semanticCloud;
            }
            FVEKnowledge = _knowledge;
        }

        [XmlIgnore]
        public IWebFVExtractorKnowledge FVEKnowledge { get; set; }


        [XmlIgnore]
        public webLemmaTermTable WLTableOfIndustryClass { get; set; }

        [XmlIgnore]
        public webLemmaTermTable WLChunkTableOfIndustryClass { get; set; }

        [XmlIgnore]
        public lemmaSemanticCloud semanticCloud { get; set; }

        #endregion -----------------------------------------------------------------------------------------

    }
}
