// --------------------------------------------------------------------------------------------------------------------
// <copyright file="semanticFVExtractorSettings.cs" company="imbVeles" >
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
using imbSCI.Core.extensions.text;
using imbWBI.Core.WebClassifier.cases;
using imbWBI.Core.WebClassifier.core;
using imbWBI.Core.WebClassifier.validation;
using System.Text;
using System.Xml.Serialization;
using imbNLP.PartOfSpeech.TFModels.semanticCloud;
using System.IO;
using imbNLP.PartOfSpeech.decomposing.chunk;
using imbSCI.DataComplex.tables;
using imbACE.Core;
using System.ComponentModel;
using imbNLP.PartOfSpeech.TFModels.semanticCloudMatrix;

namespace imbWBI.Core.WebClassifier.wlfClassifier
{

    /// <summary>
    /// Settings for FVExtractor
    /// </summary>
    /// <seealso cref="imbWBI.Core.WebClassifier.wlfClassifier.FVExtractorSettingsBase" />
    public class semanticFVExtractorSettings:FVExtractorSettingsBase
    {



        /// <summary>
        /// Initializes a new instance of the <see cref="semanticFVExtractorSettings"/> class.
        /// </summary>
        public semanticFVExtractorSettings()
        {

        }

      

        /// <summary>
        /// Gets or sets the semantic cloud filter.
        /// </summary>
        /// <value>
        /// The semantic cloud filter.
        /// </value>
        public cloudMatrixSettings semanticCloudFilter { get; set; } = new cloudMatrixSettings();


        /// <summary>
        /// Gets or sets the case term expansion steps.
        /// </summary>
        /// <value>
        /// The case term expansion steps.
        /// </value>
        public Int32 caseTermExpansionSteps { get; set; } = 3;

        /// <summary>
        /// Gets or sets the case term expansion options.
        /// </summary>
        /// <value>
        /// The case term expansion options.
        /// </value>
        public lemmaExpansionOptions caseTermExpansionOptions { get; set; } = lemmaExpansionOptions.initialWeightFromParent | lemmaExpansionOptions.weightAsSemanticDistanceFromParent | lemmaExpansionOptions.divideWeightByNumberOfSynonims;

        /// <summary>
        /// Describes it selfs.
        /// </summary>
        /// <returns></returns>
        public override List<string> DescribeSelf()
        {
            List<String> output = new List<string>();

            output.Add("## Settings");

            output.AddRange(featureVectors.DescribeSelf());

            output.Add(" --- ");

            output.Add(" > DocumentSetCase terms, matching the Semantic Cloud, are expanded in [" + caseTermExpansionSteps + "] steps");

            output.Add(" > > Match pair weight computation options: ");

            var options = caseTermExpansionOptions.getEnumListFromFlags<lemmaExpansionOptions>();
            foreach (lemmaExpansionOptions op in options)
            {
                output.Add(" > > + " + op.ToString().imbTitleCamelOperation(true));
            }

            //if (DoBuildCaseDebugDGML)
            //{
            //    output.Add(" > It will build DirectedGraphMarkupLanguage debug graph for each case evaluated");
            //}

            output.Add(" --- ");

            output.AddRange(semanticCloudFilter.DescribeSelf());



            return output;
        }
    }

}