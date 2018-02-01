// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DocumentSetCaseCollectionReport.cs" company="imbVeles" >
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
using imbSCI.Core.math.classificationMetrics;

namespace imbWBI.Core.WebClassifier.reportData
{


    public class DocumentSetCaseCollectionReport:classificationReport
    {

        public DocumentSetCaseCollectionReport()
        {

        }

        public DocumentSetCaseCollectionReport(String caseName)
        {
            Name = caseName;
        }

        

        //public static DocumentSetCaseCollectionReport GetAverage(IEnumerable<DocumentSetCaseCollectionReport> input)
        //{
        //    DocumentSetCaseCollectionReport output = null;
        //    DocumentSetCaseCollectionReport first = null;
        //    Int32 c = 0;
        //    foreach (var i in input)
        //    {
        //        if (first == null)
        //        {
        //            first = i;
        //            output = new DocumentSetCaseCollectionReport(first.Classifier + "_" + "")
        //            output.Classifier = first.Classifier;
        //            output.kFoldCase = output.Classifier + " (mean)";
        //        }
        //        output.AddValues(i);
        //        c++;
        //    }
        //    output.DivideValues(c);

        //    return output;
        //}



        //public void AddValues(classificationEvalMetricSet metrics)
        //{
        //    Precision += metrics.GetPrecision();
        //    Recall += metrics.GetRecall();
        //    F1measure += metrics.GetPrecision();

        //    foreach (var p in metrics)
        //    {
        //        Correct += p.Value.correct;
        //        Wrong += p.Value.wrong;
        //        Targets += p.Value.correct + p.Value.wrong;
        //    }

        //}

        //public void AddValues(DocumentSetCaseCollectionReport source)
        //{
        //    Precision += source.Precision;
        //    Recall += source.Recall;
        //    F1measure += source.F1measure;
        //    Correct += source.Correct;
        //    Targets += source.Targets;
        //    Wrong += source.Wrong;
        //}

        //public void DivideValues(Double divisor, Boolean OnlyRatios=true)
        //{
        //    var source = this;
        //    Precision = Precision.GetRatio(divisor);
        //    Recall = Recall.GetRatio(divisor);
        //    F1measure = source.F1measure.GetRatio(divisor);
        //    if (!OnlyRatios)
        //    {
        //        Correct = source.Correct.GetRatio(divisor);
        //        Targets = source.Targets.GetRatio(divisor);
        //        Wrong = source.Wrong.GetRatio(divisor);
        //    }
        //}

        //private classificationEvalMetricSet metrics;

        //public classificationEvalMetricSet GetSetMetrics(classificationEvalMetricSet _metrics=null)
        //{
        //    if (_metrics != null) metrics = _metrics;
        //    return metrics;
        //}

        ///// <summary> the title attached to this k-fold evaluation case instance </summary>
        //[Category("Label")]
        //[DisplayName("kFoldCase")] //[imb(imbAttributeName.measure_letter, "")]
        //[Description("the title attached to this k-fold evaluation case instance")]
        //[imb(imbAttributeName.reporting_columnWidth, "50")]
        //public String name { get; set; } = default(String);



        ///// <summary> Name of post classifier </summary>
        //[Category("Label")]
        //[DisplayName("Classifier")] //[imb(imbAttributeName.measure_letter, "")]
        //[Description("Name of post classifier")] // [imb(imbAttributeName.reporting_escapeoff)]
        //public String Classifier { get; set; } = default(String);





        ///// <summary> Correct classifications </summary>
        //[Category("Evaluation")]
        //[DisplayName("Correct")]
        //[imb(imbAttributeName.measure_letter, "E_c")]
        //[imb(imbAttributeName.measure_setUnit, "n")]
        //[Description("Correct classifications")] // [imb(imbAttributeName.measure_important)][imb(imbAttributeName.reporting_valueformat, "")]
        //public Double Correct { get; set; } = 0;


        ///// <summary> Wrong </summary>
        //[Category("Count")]
        //[DisplayName("Wrong")]
        //[imb(imbAttributeName.measure_letter, "E_w")]
        //[imb(imbAttributeName.measure_setUnit, "n")]
        //[Description("Wrong classification count")] // [imb(imbAttributeName.measure_important)][imb(imbAttributeName.reporting_valueformat, "")]
        //public Double Wrong { get; set; } = default(Int32);




        ///// <summary> Number of web sites designated for model evaluation </summary>
        //[Category("Evaluation")]
        //[DisplayName("Targets")]
        //[imb(imbAttributeName.measure_letter, "W_n")]
        //[imb(imbAttributeName.measure_setUnit, "n")]
        //[Description("Number of web sites designated for model evaluation")] // [imb(imbAttributeName.measure_important)][imb(imbAttributeName.reporting_valueformat, "")]
        //public Double Targets { get; set; } = default(Int32);


        ///// <summary> Ratio </summary>
        //[Category("Ratio")]
        //[DisplayName("Precision")]
        //[imb(imbAttributeName.measure_letter, "P")]
        //[imb(imbAttributeName.measure_setUnit, "%")]
        //[imb(imbAttributeName.reporting_valueformat, "F5")]
        //[Description("Rate of correctly classified cases in all evaluated")] // [imb(imbAttributeName.measure_important)][imb(imbAttributeName.reporting_valueformat, "")][imb(imbAttributeName.reporting_escapeoff)]
        //public Double Precision { get; set; } = default(Double);


        ///// <summary> Ratio </summary>
        //[Category("Ratio")]
        //[DisplayName("Recall")]
        //[imb(imbAttributeName.measure_letter, "R")]
        //[imb(imbAttributeName.measure_setUnit, "%")]
        //[imb(imbAttributeName.reporting_valueformat, "F5")]
        //[Description("Rate of correctly classified web sites in total number of web sites of the class")] // [imb(imbAttributeName.measure_important)][imb(imbAttributeName.reporting_valueformat, "")][imb(imbAttributeName.reporting_escapeoff)]
        //public Double Recall { get; set; } = default(Double);



        ///// <summary> F1 measure - harmonic mean of precision and recall </summary>
        //[Category("Ratio")]
        //[DisplayName("F1measure")]
        //[imb(imbAttributeName.measure_letter, "F1")]
        //[imb(imbAttributeName.measure_setUnit, "%")]
        //[imb(imbAttributeName.reporting_valueformat, "F5")]
        //[Description("F1 measure - harmonic mean of precision and recall")] // [imb(imbAttributeName.measure_important)][imb(imbAttributeName.reporting_valueformat, "")][imb(imbAttributeName.reporting_escapeoff)]
        //public Double F1measure { get; set; } = default(Double);





    }

}