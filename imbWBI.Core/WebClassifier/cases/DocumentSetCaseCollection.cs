// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DocumentSetCaseCollection.cs" company="imbVeles" >
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
// using imbMiningContext.TFModels.WLF_ISF;
using imbNLP.PartOfSpeech.flags.token;
using imbNLP.PartOfSpeech.pipeline.machine;
using imbNLP.PartOfSpeech.pipelineForPos.subject;
using imbNLP.PartOfSpeech.resourceProviders.core;
using imbNLP.PartOfSpeech.TFModels.webLemma;
using imbSCI.Core.extensions.data;
using imbSCI.DataComplex.extensions.data.schema;
using imbSCI.DataComplex.extensions.data.operations;
using imbSCI.DataComplex.extensions.data.formats;
using imbSCI.DataComplex.extensions.data.modify;
using imbSCI.DataComplex.extensions.data;
using imbSCI.Core.extensions.table;
using imbSCI.Core.files.folders;
using imbSCI.Core.math;
using imbSCI.Core.reporting;
using imbSCI.Data;
using imbSCI.DataComplex.tf_idf;
using imbWBI.Core.WebClassifier.core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using imbNLP.PartOfSpeech.TFModels.webLemma.core;
using imbNLP.PartOfSpeech.pipelineForPos.subject;
using imbWBI.Core.WebClassifier.validation;
using System.ComponentModel;
using imbSCI.Core.attributes;
using imbSCI.Core.extensions.io;
using System.Threading;
using imbWBI.Core.WebClassifier.reportData;
using imbWBI.Core.math;

namespace imbWBI.Core.WebClassifier.cases
{

    /// <summary>
    /// Collection of documentSetCases within one industry class
    /// </summary>
    /// <seealso cref="System.Collections.Generic.List{imbWBI.Core.WebClassifier.cases.DocumentSetCase}" />
    public class DocumentSetCaseCollection:List<DocumentSetCase>
    {
        public validationCaseCollection validation { get; set; }
        public kFoldValidationCase validationCase { get; set; }

        public WebFVExtractorKnowledgeLibrary knowledgeLibrary
        {
            get
            {
                return validationCase.knowledgeLibrary;
            }
        }


        public IWebFVExtractorKnowledge classKnowledge { get; set; }

        public DocumentSetCaseCollection(IDocumentSetClass _setClass)
        {
            setClass = _setClass;
            rightClassID = setClass.classID;
        }



        private Object deployLock = new Object();


        /// <summary>
        /// Deploys the specified v case coll.
        /// </summary>
        /// <param name="vCaseColl">The v case coll.</param>
        /// <param name="validationCase">The validation case.</param>
        /// <param name="subjects">The subjects.</param>
        /// <param name="classes">The classes.</param>
        public void deploy(validationCaseCollection vCaseColl, kFoldValidationCase _validationCase, IEnumerable<IPipelineTaskSubject> subjects, DocumentSetClasses classes)
        {
            lock (deployLock)
            {
                validation = vCaseColl;
                validationCase = _validationCase;

                Thread.Sleep(100);

                var sites = subjects.ToSubjectTokenType<pipelineTaskMCSiteSubject>().ToList();
               // var sitesCopy = sites.ToList();

                sites = vCaseColl.FilterSites(sites);

                foreach (pipelineTaskMCSiteSubject site in sites)
                {
                    DocumentSetCase sCase = new DocumentSetCase();
                    sCase.subject = site;
                    sCase.data = new WebClassifierResultSet(classes, validationCase.context, validationCase.extractor.settings);

                    Add(sCase);
                }
            }
            
        }


        /// <summary>
        /// Gets the report table on one collection
        /// </summary>
        /// <returns></returns>
        public DataTable GetReportTable(Boolean isTrainingCollection=false, Boolean isSingleCategoryReport = true)
        {
            DataTable output = this.BuildShema(isSingleCategoryReport, isTrainingCollection);

            this.SetAdditionalInfo(output, isSingleCategoryReport, isTrainingCollection);

            foreach (var setCase in this)
            {
                this.BuildRow(setCase, output, isTrainingCollection);
            }

            ranger = new rangeFinderForDataTable(output, "name");

            ranger.AddRangeRows(setClass.name, output, true, imbSCI.Core.math.aggregation.dataPointAggregationType.avg | imbSCI.Core.math.aggregation.dataPointAggregationType.stdev);
            

            return output;
        }

        public rangeFinderForDataTable ranger { get; set; } 

      

        public IDocumentSetClass setClass { get; set; }

        /// <summary>
        /// Identification of the right class for this collection
        /// </summary>
        /// <value>
        /// The right class identifier.
        /// </value>
        public Int32 rightClassID { get; set; } = 0;

       


    }

}