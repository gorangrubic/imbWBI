// --------------------------------------------------------------------------------------------------------------------
// <copyright file="industryTermModelCrawlSetup.cs" company="imbVeles" >
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
// Project: imbWBI.IndustryTermModel
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
using imbSCI.Core.files.fileDataStructure;
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;
// using imbMiningContext.TFModels.WLF_ISF;
using imbMiningContext.TFModels.ILRT;
using imbMiningContext.MCRepository;
using imbNLP.Data;
using imbWEM.Core.crawler.engine;
using imbWEM.Core.crawler;
using imbNLP.Data.evaluate;
using imbNLP.PartOfSpeech.TFModels.webLemma;

namespace imbWBI.IndustryTermModel.industry
{

    /// <summary>
    /// Crawl Job common configuration for MCRepository build
    /// </summary>
    public class industryTermModelCrawlSetup
    {
        public List<String> DescribeSelf()
        {
            List<String> output = new List<string>();

            output.Add("## Crawler [" + Crawler + "]");

            output.Add(" > Language of preference: " + PrimaryLanguage.ToString());
            output.Add(" > Secondary language: " + SecondaryLanguage.ToString());

            return output;
        }

        /// <summary> Crawler class name - crawler to be used for Mining Context repository construction  </summary>
        [Category("MCRepository")]
        [DisplayName("Crawler")] //[imb(imbAttributeName.measure_letter, "")]
        [Description("Crawler class name - crawler to be used for Mining Context repository construction ")] // [imb(imbAttributeName.reporting_escapeoff)]
        public String Crawler { get; set; } = "SM_LS";


        /// <summary> Primary language to direct crawler to content on </summary>
        [Category("MCRepository")]
        [DisplayName("PrimaryLanguage")] //[imb(imbAttributeName.measure_letter, "")]
        [Description("Primary language to direct crawler to content on")] // [imb(imbAttributeName.reporting_escapeoff)]
        public basicLanguageEnum PrimaryLanguage { get; set; } = basicLanguageEnum.serbian;

        /// <summary> Secondary language, used for crawler frontier secondary preference </summary>
        [Category("MCRepository")]
        [DisplayName("SecondaryLanguage")] //[imb(imbAttributeName.measure_letter, "")]
        [Description("Secondary language, used for crawler frontier secondary preference")] // [imb(imbAttributeName.reporting_escapeoff)]
        public basicLanguageEnum SecondaryLanguage { get; set; } = basicLanguageEnum.english;


        [Category("MCRepository")]
        [DisplayName("Crawler Engine settings")] //[imb(imbAttributeName.measure_letter, "")]
        [Description("Settings for crawling engine, applied when MCRepository is built")]
        public crawlerDomainTaskMachineSettings crawlerJobEngineSettings { get; set; } = new crawlerDomainTaskMachineSettings();


        public wlfConstructorSettings wlfSettings { get; set; } = new wlfConstructorSettings();


        [Category("MCRepository")]
        [DisplayName("Crawler Instance settings")] //[imb(imbAttributeName.measure_letter, "")]
        [Description("Settings for crawler instance")]
        public spiderSettings crawlerSettings { get; set; } = new spiderSettings();


        public industryTermModelCrawlSetup()
        {

        }
    }

}