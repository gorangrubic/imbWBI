// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DocumentSetPipelineCollectionReports.cs" company="imbVeles" >
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
using imbSCI.Data.extensions;
using imbSCI.Core.extensions.table;
using imbSCI.Core.data;
using imbSCI.DataComplex.tables;
using imbSCI.DataComplex.tables.extensions;
using imbSCI.DataComplex.extensions.data.schema;
using imbSCI.DataComplex.extensions.data.operations;
using imbSCI.DataComplex.extensions.data.modify;
using imbSCI.DataComplex.extensions.data.formats;
using imbSCI.DataComplex.extensions.data;
using imbSCI.DataComplex.extensions;
using imbSCI.DataComplex.tf_idf;
using imbWBI.Core.WebClassifier.core;
using System.Text;
using imbMiningContext.MCRepository;
using imbNLP.PartOfSpeech.pipeline.core;
using imbSCI.Core.files.folders;
using imbNLP.PartOfSpeech.TFModels.webLemma.core;
using imbWBI.Core.WebClassifier.validation;
using imbWBI.Core.WebClassifier.experiment;
using imbSCI.Core.extensions.table.style;
using imbSCI.Core.extensions.table.dynamics;
using imbSCI.Data.collection.nested;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using imbWBI.Core.WebCase.collections;
using imbWBI.Core.WebCase;
using imbNLP.PartOfSpeech.TFModels.semanticCloud;
using imbWBI.Core.WebClassifier.wlfClassifier;
using imbNLP.PartOfSpeech.TFModels.semanticCloudMatrix;
using imbSCI.DataComplex.tables;
using imbACE.Core;
using imbSCI.Graph.Converters;
using System.Data;
using imbSCI.DataComplex.special;
using System.IO;
using System.ComponentModel;
using imbSCI.Core.attributes;
using imbNLP.PartOfSpeech.flags.basic;
using imbNLP.PartOfSpeech.flags.data;
using imbWBI.Core.WebClassifier.cases;

namespace imbWBI.Core.WebClassifier.reportData
{

    public static class DocumentSetPipelineCollectionReports
    {

        public static DataTable GetClassKnowledgeReport(this DocumentSetPipelineCollection pipelineCollection, IDocumentSetClass caseSet, DataTable output=null)
        {

            if (output == null)
            {
                output = new DataTable();
                output.SetTitle(caseSet.name);
                output.Add("Name", "Name of class or web site", "", typeof(String), imbSCI.Core.enums.dataPointImportance.normal, "", "Name").SetGroup("Repository").SetWidth(25).SetUnit("");
                output.Add("Sites", "Number of sites in cateogry - or 1 for single site entry", "|C_ds|", typeof(Int32), imbSCI.Core.enums.dataPointImportance.normal, "", "Sites").SetGroup("Repository").SetWidth(10).SetUnit("n");
                output.Add("Pages", "Total number of pages detected in the repository", "|C_d|", typeof(Int32), imbSCI.Core.enums.dataPointImportance.normal, "", "Pages Crawled").SetGroup("Repository").SetWidth(15).SetUnit("n");
                output.Add("PagesValid", "Number of pages used for the category or site", "|C_dv|", typeof(Int32), imbSCI.Core.enums.dataPointImportance.normal, "", "Pages Used").SetGroup("Pipeline").SetWidth(15).SetUnit("n");
                output.Add("Blocks", "Number of blocks for category or site", "|C_b|", typeof(Int32), imbSCI.Core.enums.dataPointImportance.normal, "").SetGroup("Pipeline").SetWidth(10).SetUnit("n");
                output.Add("Streams", "Number of streams for category or site", "|C_ts|", typeof(Int32), imbSCI.Core.enums.dataPointImportance.normal, "").SetGroup("Pipeline").SetWidth(10).SetUnit("n");
                output.Add("Tokens", "Number of tokens for category or site", "|C_t|", typeof(Int32), imbSCI.Core.enums.dataPointImportance.normal, "").SetGroup("Pipeline").SetWidth(10).SetUnit("n");
                output.Add("Chunks", "Number of chunks for category - disabled for sites", "|C_c|", typeof(Int32), imbSCI.Core.enums.dataPointImportance.normal, "").SetGroup("NLP").SetWidth(10).SetUnit("n");
                output.Add("OnlyLetters", "Number of tokens for category or site with onlyLetter tag", "|C_ttl|", typeof(Int32), imbSCI.Core.enums.dataPointImportance.normal, "", "Only Letters").SetGroup("Only Letters").SetWidth(10).SetUnit("n");
                output.Add("OnlyLettersResolved", "Number of tokens resolved by morphosyntactic resource", "|C_ttlr|", typeof(Int32), imbSCI.Core.enums.dataPointImportance.normal, "", "Accepted Tokens").SetGroup("Only Letters").SetWidth(10).SetUnit("n");
                output.Add("OnlyLettersUnresolved", "Number of tokens unresolved by morphosyntactic resource", "|C_ttlu|", typeof(Int32), imbSCI.Core.enums.dataPointImportance.normal, "", "Dismissed").SetGroup("Only Letters").SetWidth(10).SetUnit("n");
                output.Add("Numbers", "Number of tokens for category or site with a numeric content tag", "|C_ttn|", typeof(Int32), imbSCI.Core.enums.dataPointImportance.normal, "", "Number").SetGroup("Other").SetWidth(10).SetUnit("n");
                output.Add("Symbols", "Number of tokens for category or site with a symbolic content tag", "|C_tts|", typeof(Int32), imbSCI.Core.enums.dataPointImportance.normal, "", "Symbols").SetGroup("Other").SetWidth(10).SetUnit("n");
                output.Add("Business", "Number of tokens for category or site with any dat_business tag", "|C_ttb|", typeof(Int32), imbSCI.Core.enums.dataPointImportance.normal, "", "Business tags").SetGroup("Special").SetWidth(10).SetUnit("n");
                output.Add("Potential", "Number of tokens for category or site with any tkn_potential data point tag", "|C_ttp|", typeof(Int32), imbSCI.Core.enums.dataPointImportance.normal, "", "Potential tags").SetGroup("Special").SetWidth(10).SetUnit("n");

            } else
            {
                output.SetTitle("Class Set Report");
                output.TableName = "multi_class_report";
            }

            output.SetAdditionalInfoEntry("Class " + caseSet.treeLetterAcronim + " name", caseSet.name);
            output.SetAdditionalInfoEntry("Class " + caseSet.treeLetterAcronim + " repo", caseSet.MCRepositoryName);

            var sites = pipelineCollection.sitesByCategory[caseSet].ToList();
            PipelineReportForClass repForClass = new PipelineReportForClass();
            repForClass.deployClass(caseSet);
            repForClass.Chunks = pipelineCollection.chunksByCategory[caseSet].Count();
            foreach (var site in sites)
            {
                var dr = output.NewRow();

                PipelineReportForClass repForSite = new PipelineReportForClass();
                repForSite.deploySite(site);
                repForSite.deployTokens(pipelineCollection.tokenBySite[site as pipelineTaskMCSiteSubject]);


                repForSite.SetDataRow(dr);

                output.Rows.Add(dr);
                repForClass.sum(repForSite);

            }

            var drc = output.NewRow();
            repForClass.SetDataRow(drc);
            output.Rows.Add(drc);
            output.GetRowMetaSet().AddUnit(new dataValueMatchCriterionDynamicStyle<String,DataRowInReportTypeEnum>(new String[] { repForClass.name}, DataRowInReportTypeEnum.dataHighlightA, "Name" ));

            return output;
        }
    }

}