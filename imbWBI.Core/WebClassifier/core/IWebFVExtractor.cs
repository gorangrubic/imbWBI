// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IWebFVExtractor.cs" company="imbVeles" >
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
using imbSCI.Core.reporting;
using imbWBI.Core.WebClassifier.cases;
using imbWBI.Core.WebClassifier.validation;
using imbWBI.Core.WebClassifier.wlfClassifier;
using System.Text;
using imbSCI.Data.enums.fields;

namespace imbWBI.Core.WebClassifier.core
{

    public interface IWebFVExtractor
    {
        String name { get; set; }

        void DoFVEAndTraining(kFoldValidationCase validationCase, classifierTools tools, ILogBuilder logger);

        IFVExtractorSettings settings { get; }

        DocumentSetCaseCollectionSet DoClassification(kFoldValidationCase validationCase, classifierTools tools, ILogBuilder logger, Boolean processTraining=false);

        WebSiteClassifierResult GetClassification(DocumentSetCase target, DocumentSetCaseCollectionSet cases, ILogBuilder logger);

        DocumentSetClasses classes { get; set; }
        String description { get; set; }

        void BuildFeatureVectorDefinition();

        /// <summary>
        /// Describes self in multiple lines. Description contains the most important settings and way of operation
        /// </summary>
        /// <returns></returns>
        List<String> DescribeSelf();
    }

}