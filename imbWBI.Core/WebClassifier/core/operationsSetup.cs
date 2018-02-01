// --------------------------------------------------------------------------------------------------------------------
// <copyright file="operationsSetup.cs" company="imbVeles" >
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
using imbSCI.Core.files.fileDataStructure;
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;
// using imbMiningContext.TFModels.WLF_ISF;
using imbMiningContext.TFModels.ILRT;
using imbMiningContext.MCRepository;
using imbNLP.Data;
using imbWEM.Core.crawler.engine;
using imbNLP.PartOfSpeech.pipeline.models;
using imbSCI.Core.files.folders;
using imbMiningContext;
using System.IO;
using imbACE.Core;
using imbWBI.Core.WebClassifier.pipelineMCRepo;
using imbWBI.Core.WebClassifier.wlfClassifier;
using imbSCI.Core.reporting;
using imbACE.Core.core;
using imbWBI.Core.WebClassifier.validation;
using imbWBI.Core.WebClassifier.cases;
using imbSCI.Data.data.sample;
using imbSCI.Core.attributes;

namespace imbWBI.Core.WebClassifier.core
{

    public class operationsSetup
    {
        public operationsSetup()
        {


        }



        /// <summary> do reports in paralell - it true it will perform reporting in a separate thread</summary>
        [Category("Switch")]
        [DisplayName("doReportsInParalell")]
        [Description("do reports in paralell - it true it will perform reporting in a separate thread")]
        public Boolean doReportsInParalell { get; set; } = true;



        /// <summary> Number of parallel threads used for experiment execution  </summary>
        [Category("Count")]
        [DisplayName("ParallelThreads")]
        [imb(imbAttributeName.measure_letter, "")]
        [imb(imbAttributeName.measure_setUnit, "n")]
        [Description("Number of parallel threads used for experiment execution ")] // [imb(imbAttributeName.measure_important)][imb(imbAttributeName.reporting_valueformat, "")]
        public Int32 ParallelThreads { get; set; } = 5;



        /// <summary> If true it will create semantic cloud and matrix reports at experiment init procedure </summary>
        [Category("Switch")]
        [DisplayName("doCreateDiagnosticMatrixAtStart")]
        [Description("If true it will create semantic cloud and matrix reports at experiment init procedure")]
        public Boolean doCreateDiagnosticMatrixAtStart { get; set; } = true;

        /// <summary> Do perform full diagnostic report on classification experiment start-up </summary>
        [Category("Switch")]
        [DisplayName("doFullDiagnosticReport")]
        [Description("Do perform full diagnostic report on classification experiment start-up")]
        public Boolean doFullDiagnosticReport { get; set; } = true;



        /// <summary> if true it will generate Directed Graph for each class in each k-fold </summary>
        [Category("Switch")]
        [DisplayName("doMakeGraphForClasses")]
        [Description("if true it will generate Directed Graph for each class in each k-fold")]
        public Boolean doMakeGraphForClassClouds { get; set; } = true;


        /// <summary> If <c>true</c> it will use simple graph styling </summary>
        [Category("Flag")]
        [DisplayName("doUseSimpleGraphs")]
        [imb(imbAttributeName.measure_letter, "")]
        [Description("If <c>true</c> it will use simple graph styling")]
        public Boolean doUseSimpleGraphs { get; set; } = true;




        /// <summary> If true it will generate Directed Graph for each case  </summary>
        [Category("Switch")]
        [DisplayName("doMakeGraphForCases")]
        [Description("If true it will generate Directed Graph for each case ")]
        public Boolean doMakeGraphForCases { get; set; } = true;




        /// <summary> Generates decomposition graph for each web site </summary>
        [Category("Switch")]
        [DisplayName("doMakeGraphForEachSite")]
        [Description("Generates decomposition graph for each web site")]
        public Boolean doMakeGraphForEachSite { get; set; } = true;


        /// <summary> Generate repository graph for each repository </summary>
        [Category("Switch")]
        [DisplayName("doMakeRepositoryGraph")]
        [Description("Generate repository graph for each repository")]
        public Boolean doMakeRepositoryGraph { get; set; } = true;


        public Boolean DoMakeReportForEachClassifier { get; set; } = false;

        public Boolean DoRandomCaseGraphReportMode { get; set; } = true;
        public Int32 In100RandomCaseGraphReport { get; set; } = 2;



        public Boolean DoUseCachedLemmaResource { get; set; } = false;

        public Boolean doUseExistingKnowledge { get; set; } = false;


        public bool doSaveKnowledgeForCases { get; set; } = false;

        public bool doSaveKnowledgeForClasses { get; set; } = false;
        public bool doMakeClassificationReportForCases { get;  set; }


        public bool doRebootFVEOnCrash { get; set; } = true;
        public Int32 doRebootFVEOnCrashRetryLimit { get; set; } = 3;

    }


}