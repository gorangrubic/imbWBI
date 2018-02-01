// --------------------------------------------------------------------------------------------------------------------
// <copyright file="kFoldValidationCase.cs" company="imbVeles" >
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
using imbSCI.Core.files.fileDataStructure;
using imbSCI.Core.files.folders;
using imbSCI.Core.reporting;
using imbWBI.Core.WebClassifier.cases;
using imbWBI.Core.WebClassifier.core;
using imbWBI.Core.WebClassifier.experiment;
using imbWBI.Core.WebClassifier.reportData;
using imbWBI.Core.WebClassifier.wlfClassifier;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace imbWBI.Core.WebClassifier.validation
{

    [fileStructure(nameof(name), fileStructureMode.subdirectory, fileDataFilenameMode.propertyValue, fileDataPropertyOptions.textDescription)]
    public class kFoldValidationCase : fileDataStructure, IFileDataStructure
    {
        [XmlIgnore]
        public kFoldValidationCollection kFoldMaster { get; set; }

        public WebFVExtractorKnowledgeLibrary knowledgeLibrary
        {
            get
            {
                return kFoldMaster.knowledgeLibrary;
            }
        }
        

        public String name { get; set; }

        public String description { get; set; }

        public kFoldValidationCase()
        {

        }

        public Int32 id { get; set; } = 0;

        [XmlIgnore]
        public experimentExecutionContext context { get; set; }

        [XmlIgnore]
        public IWebFVExtractor extractor { get; set; }

        

        [Description("Cases used for training purposes")]
        public validationCaseCollectionSet trainingCases { get; set; } = new validationCaseCollectionSet();

        
        [Description("Cases used for validation purposes")]
        public validationCaseCollectionSet evaluationCases { get; set; } = new validationCaseCollectionSet();

        [XmlIgnore]
        public DSCCReports testReport { get; set; }

        [XmlIgnore]
        public DocumentSetCaseCollectionSet evaluationResults { get; set; }

        [XmlIgnore]
        public folderNode caseFolder { get; internal set; }

        /// <summary>
        /// Randomly picked cases for Micro-analysis of FV Extraction - similarity computation between a case and a category
        /// </summary>
        /// <value>
        /// The case sample folder.
        /// </value>
        [XmlIgnore]
        public folderNode caseSampleFolder { get; internal set; }

        public override void OnLoaded()
        {
            trainingCases.kFoldCase = this;
            evaluationCases.kFoldCase = this;
            if (caseFolder == null)
            {
                caseFolder = folder.Add("cases", "Cases", "Repository with knowledge on cases");
                caseSampleFolder = caseFolder.Add("microAnalysis", "Micro-analysis of FV Extraction", "Randomly picked cases for Micro-analysis of FV Extraction - similarity computation between a case and a category");
            }
           // caseFolder = folder
        }

        public override void OnBeforeSave()
        {
            
        }
    }

}