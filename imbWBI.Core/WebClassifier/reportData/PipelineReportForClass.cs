// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PipelineReportForClass.cs" company="imbVeles" >
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

    public class PipelineReportForClass
    {
        public String name { get; set; } = "";

        /// <summary> Number of </summary>
        [Category("Count")]
        [imb(imbAttributeName.measure_letter, "|C_ds|")]
        [imb(imbAttributeName.measure_setUnit, "n")]
        [Description("Number of sites in cateogry - or 1 for single site entry")] // [imb(imbAttributeName.measure_important)][imb(imbAttributeName.reporting_valueformat, "")]
        public Int32 Sites { get; set; } = default(Int32);

        public Int32 Pages { get; set; } = 0;
        public Int32 PagesValid { get; set; } = 0;
        public Int32 Blocks { get; set; } = 0;
        public Int32 Streams { get; set; } = 0;
        
        public Int32 Chunks { get; set; } = 0;

        public void deployClass(IDocumentSetClass site)
        {
            name = site.name.ToUpper();
            
        }
        public void deploySite(pipelineTaskMCSiteSubject site)
        {
            name = site.name;
            
            Sites = 1;
            Pages = site.Count();

            foreach (var p in site)
            {
                if (p.Any())
                {
                    PagesValid++;
                    Blocks += p.Count();
                    foreach (var b in p)
                    {
                        Streams += b.Count();

                    }
                }
            }


        }

        public void sum(PipelineReportForClass B)
        {
            Sites += B.Sites;
            Pages += B.Pages;
            PagesValid += B.PagesValid;
            Blocks += B.Blocks;
            Streams += B.Streams;
            Chunks += B.Chunks;

            Tokens += B.Tokens;

            OnlyLetters += B.OnlyLetters;
            OnlyLettersResolved += B.OnlyLettersResolved;
            OnlyLettersUnresolved += B.OnlyLettersUnresolved;


            Numbers += B.Numbers;
                Symbols += B.Symbols;
            Business += B.Business;
            Potential += B.Potential;
    }

        public void deployStreams(IEnumerable<pipelineTaskSubjectContentToken> streams)
        {

        }

        

        public void deployTokens(IEnumerable<pipelineTaskSubjectContentToken> tokens)
        {
            foreach (pipelineTaskSubjectContentToken tkn in tokens)
            {
                if (tkn.contentLevelType == cnt_level.mcToken) {
                    if (tkn.flagBag.Contains(tkn_contains.onlyLetters)) {
                        OnlyLetters++;

                        if (tkn.flagBag.Any(x => x.GetType() == typeof(pos_type))) {
                            OnlyLettersResolved++;
                        } else
                        {
                            OnlyLettersUnresolved++;
                        }
                    } else if (tkn.flagBag.ContainsAny(new object[] { tkn_contains.onlyNumbers, tkn_numeric.numberClean, tkn_numeric.numberInFormat, tkn_numeric.numberInPercentage })) Numbers++;
                    else if (tkn.flagBag.ContainsAny(new object[] { tkn_contains.onlySymbols, tkn_contains.symbols,tkn_specialforms.email, tkn_contains.punctation })) Symbols++;

                    if (tkn.flagBag.Any(x => x.GetType() == typeof(dat_business)))
                    {
                        Business++;
                    }
                    if (tkn.flagBag.Any(x => x.GetType() == typeof(tkn_potential)))
                    {
                        Potential++;
                    }
                    Tokens++;
                } else if (tkn.contentLevelType == cnt_level.mcTokenStream)
                {
                    Streams++;
                } else if (tkn.contentLevelType == cnt_level.mcBlock)
                {
                    Blocks++;
                } else if (tkn.contentLevelType == cnt_level.mcChunk)
                {
                    Chunks++;
                } else if (tkn.contentLevelType == cnt_level.mcPage)
                {
                    Pages++;
                } else if (tkn.contentLevelType == cnt_level.mcSite)
                {
                    Sites++;
                }
            }
        }

        public Int32 Tokens { get; set; } = 0;

        public Int32 OnlyLetters { get; set; } = 0;
        public Int32 OnlyLettersResolved { get; set; } = 0;
        public Int32 OnlyLettersUnresolved { get; set; } = 0;


        public Int32 Numbers { get; set; } = 0;
        public Int32 Symbols { get; set; } = 0;
        public Int32 Business { get; set; } = 0;
        public Int32 Potential { get; set; } = 0;

        public void SetDataRow(DataRow dr)
        {
            dr[nameof(name)] = name;
            dr[nameof(Sites)] = Sites;
            dr[nameof(Pages)] = Pages;
            dr[nameof(PagesValid)] = PagesValid;
            dr[nameof(Blocks)] = Blocks;
            dr[nameof(Streams)] = Streams;
            dr[nameof(Chunks)] = Chunks;
            dr[nameof(Tokens)] = Tokens;
            dr[nameof(OnlyLetters)] = OnlyLetters;
            dr[nameof(OnlyLettersResolved)] = OnlyLettersResolved;
            dr[nameof(OnlyLettersUnresolved)] = OnlyLettersUnresolved;

            dr[nameof(Numbers)] = Numbers;
            dr[nameof(Symbols)] = Symbols;
            dr[nameof(Business)] = Business;
            dr[nameof(Potential)] = Potential;

            
            
        }

    }

}