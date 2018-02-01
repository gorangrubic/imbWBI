// --------------------------------------------------------------------------------------------------------------------
// <copyright file="mcRepoProcessModelSetup.cs" company="imbVeles" >
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
using imbNLP.PartOfSpeech.decomposing.block;
using imbNLP.PartOfSpeech.decomposing.stream;
using imbNLP.PartOfSpeech.decomposing.token;
using imbNLP.PartOfSpeech.flags.token;
using imbNLP.PartOfSpeech.pipeline.core;
using imbNLP.PartOfSpeech.pipeline.machine;
using imbNLP.PartOfSpeech.pipeline.mcRepoNodes;
// using imbNLP.PartOfSpeech.pipeline.tokenNodes;
using imbSCI.Core.extensions.data;
using imbSCI.Data.data.sample;
using System.Text;
using imbNLP.Data.evaluate;
using System.ComponentModel;
using imbNLP.Data;

namespace imbWBI.Core.WebClassifier.pipelineMCRepo.model
{

    /// <summary>
    /// Setup class for <see cref="mcRepoProcessModel"/> auto-construction procedure
    /// </summary>
    public class mcRepoProcessModelSetup
    {
        public mcRepoProcessModelSetup()
        {
            webSiteSampling = new samplingSettings();
            webSiteSampling.limit = -1;

            webPageSampling = new samplingSettings();
            webPageSampling.parts = 1;
            
        }


        /// <summary>
        /// Settings for multi language filter test, used to filterout web pages written on another language
        /// </summary>
        /// <value>
        /// The multi language.
        /// </value>
        [Category("MCRepository")]
        [DisplayName("LanguageFilterSettings")] //[imb(imbAttributeName.measure_letter, "")]
        [Description("Settings for multi language filter test, used to filterout web pages written on another language")]
        public multiLanguageEvaluationTask pageFilterSettings { get; set; } = new multiLanguageEvaluationTask();
               
        public basicLanguageEnum primaryLanguage { get; set; } = basicLanguageEnum.serbian;

        public String setup_translit { get; set; } = "sr.txt";
        public String setup_multitext_lex { get; set; } = "srLex_v1.2.mtx";
        public String setup_multitext_specs { get; set; } = "sr_multitext_conversion.xlsx";

        public String setup_tableTagger { get; set; } = "sr_properform_tagger.xlsx";
        public String setup_tableOne { get; set; } = "sr_properform_one.xlsx";

        public samplingSettings webSiteSampling { get; set; } = new samplingSettings();

        public samplingSettings webPageSampling { get; set; } = new samplingSettings();

        /// <summary>
        /// The language filter will pass up to specified number of pages in proper language
        /// </summary>
        /// <value>
        /// The target language page per site.
        /// </value>
        public Int32 target_languagePagePerSite { get; set; } = 15;



        public string setup_multitext_alt { get; set; } = "sr_cor.txt";
    }

}