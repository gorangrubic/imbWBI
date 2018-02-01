// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DSCCReports.cs" company="imbVeles" >
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
using System.Data;
using System.Text;
using imbNLP.PartOfSpeech.TFModels.webLemma.core;
using imbNLP.PartOfSpeech.pipelineForPos.subject;
using imbWBI.Core.WebClassifier.validation;
using System.ComponentModel;
using imbSCI.Core.attributes;
using imbWBI.Core.WebClassifier.wlfClassifier;
using imbSCI.Data.collection.nested;
using imbSCI.DataComplex.tables;
using imbWBI.Core.WebClassifier.experiment;
using imbACE.Core;
using imbSCI.Core.math.classificationMetrics;
using imbWBI.Core.WebClassifier.cases;

namespace imbWBI.Core.WebClassifier.reportData
{


    /// <summary>
    /// Set of reports for single fold of k-fold
    /// </summary>
    /// <seealso cref="imbSCI.Data.collection.nested.aceDictionarySet{imbWBI.Core.WebClassifier.wlfClassifier.IWebPostClassifier, imbWBI.Core.WebClassifier.cases.DocumentSetCaseCollectionReport}" />
    public class DSCCReports:aceDictionarySet<IWebPostClassifier, DocumentSetCaseCollectionReport>
    {
        public DocumentSetCaseCollectionSet parent { get; set; }

        public Dictionary<IWebPostClassifier, DocumentSetCaseCollectionReport> avgReports { get; set; } = new Dictionary<IWebPostClassifier, DocumentSetCaseCollectionReport>();

        public DataTable GetFullValidationTable(experimentExecutionContext context)
        {
            objectTable<DocumentSetCaseCollectionReport> tp = new objectTable<DocumentSetCaseCollectionReport>(nameof(DocumentSetCaseCollectionReport.Name), parent.validationCase.name + "_avg");

            foreach (var pair in this)
            {
                foreach (var r in pair.Value)
                {
                    tp.Add(r);
                }
                
            }

            DataTable output = tp.GetDataTable();
            String foldName = parent.validationCase.name;
            parent.validationCase.context.AddExperimentInfo(output);
            output.SetDescription($"Results of fold [{foldName}] evaluation, with all entries");
            output.SetAdditionalInfoEntry("Report type", "All entries");
            output.AddExtra("Most relevant rows are annoted with [(mean)] word");
            

            return output;
        }

        

        public DataTable GetAverageTable(experimentExecutionContext context)
        {
            objectTable<DocumentSetCaseCollectionReport> tp = new objectTable<DocumentSetCaseCollectionReport>(nameof(DocumentSetCaseCollectionReport.Name), parent.validationCase.name + "_avg");

            foreach (var pair in avgReports)
            {
                tp.Add(pair.Value);
            }

            DataTable output = tp.GetDataTable();
            String foldName = parent.validationCase.name;
            parent.validationCase.context.AddExperimentInfo(output);
            output.SetDescription($"Aggregates of [{foldName}] evaluation - only averages");
            output.SetAdditionalInfoEntry("Report type", "Average per classifier");
            //output.AddExtra("Most relevant rows are annoted with [(mean)] word");
            

            return output;
        }
    }

}