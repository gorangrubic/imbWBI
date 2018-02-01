// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DocumentSetCaseCollectionSet.cs" company="imbVeles" >
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
using imbSCI.DataComplex.tf_idf;
using imbWBI.Core.WebClassifier.core;
using System.Text;
using imbMiningContext.MCRepository;
using imbNLP.PartOfSpeech.pipeline.core;
using imbSCI.Core.files.folders;
using imbNLP.PartOfSpeech.TFModels.webLemma.core;
using imbWBI.Core.WebClassifier.validation;
using imbSCI.Core.math.classificationMetrics;
using System.Data;
using imbSCI.Core.extensions.table;
using imbSCI.DataComplex.extensions.data.schema;
using imbSCI.DataComplex.tables;
using imbWBI.Core.WebClassifier.wlfClassifier;
using imbACE.Network.extensions;
using imbWBI.Core.WebClassifier.reportData;

namespace imbWBI.Core.WebClassifier.cases
{



    /// <summary>
    /// Contains collection of cases for one DocumentSet class/category. Key is classID, Value is the collection
    /// </summary>
    /// <seealso cref="System.Collections.Generic.Dictionary{System.Int32, imbWBI.Core.WebClassifier.wlfClassifier.DocumentSetCaseCollection}" />
    public class DocumentSetCaseCollectionSet:Dictionary<Int32, DocumentSetCaseCollection>
    {
        public kFoldValidationCase validationCase { get; set; }

        




        /// <summary>
        /// Gets the report on all cases.
        /// </summary>
        /// <returns></returns>
        public DataTable GetReportOnAllCases()
        {
            DocumentSetCaseCollection first = this.Values.First();

            DataTable output = first.BuildShema(false);

            first.SetAdditionalInfo(output, false);

            foreach (KeyValuePair<int, DocumentSetCaseCollection> pair in this)
            {
                foreach (DocumentSetCase sCase in pair.Value)
                {
                    pair.Value.BuildRow(sCase, output);
                }
            }

            

            output.SetDescription("Complete view on test cases, in this fold, for all classifiers and with feature vectors used");

            return output;
        }

        /// <summary>
        /// Gets the report table on one collection
        /// </summary>
        /// <returns></returns>
        public DSCCReports GetReports()
        {

            //String repTitle = validationCase.name + " - " + validationCase.featureVectorExtractor.name;
            //String repName = repTitle.getCleanFileName();

            DSCCReports repTable = new DSCCReports();
            repTable.parent = this;
            foreach (IWebPostClassifier classifier in validationCase.context.setup.classifiers)
            {

                classificationEvalMetricSet metrics = GetMetrics(classifier);

                Int32 i = 0;

                DocumentSetCaseCollectionReport avgReport = new DocumentSetCaseCollectionReport(validationCase.name + "_" + classifier.name + "_" + "mean");
                
                avgReport.Classifier = classifier.name;

                foreach (KeyValuePair<int, DocumentSetCaseCollection> pair in this)
                {
                    var rep = new DocumentSetCaseCollectionReport(validationCase.name + " " + pair.Value.setClass.name + " " + classifier.name);
                    //rep.kFoldCase = ;
                    rep.Classifier = classifier.name;


                    rep.Targets = metrics[pair.Value.setClass.name].correct + metrics[pair.Value.setClass.name].wrong;
                    rep.Wrong = metrics[pair.Value.setClass.name].wrong;
                    rep.Correct = metrics[pair.Value.setClass.name].correct;
                    rep.Precision = metrics[pair.Value.setClass.name].GetPrecision();
                    rep.Recall = metrics[pair.Value.setClass.name].GetRecall();
                    rep.F1measure = metrics[pair.Value.setClass.name].GetF1();
                    repTable[classifier].Add(rep);
                    avgReport.AddValues(rep);
                    i++;
                }

                //DocumentSetCaseCollectionReport checkReport = new DocumentSetCaseCollectionReport();
                //checkReport.kFoldCase = classifier.name + "(check)";
                //checkReport.Classifier = classifier.name;
                //checkReport.Precision = metrics.GetPrecision();
                //checkReport.Recall = metrics.GetRecall();
                //checkReport.F1measure = metrics.GetF1();
                
                avgReport.DivideValues(i);
                repTable[classifier].Add(avgReport);
                //repTable[classifier].Add(checkReport);
                avgReport.GetSetMetrics(metrics);
                repTable.avgReports.Add(classifier, avgReport);
            }


           // output.SetDescription("Report for " + validationCase.name + " sample evaluation");




           // ds.Tables.Add(repTable.GetDataTable());

            return repTable;
        }


 

        public Int32 CountAllCases()
        {
            Int32 output = 0;

            foreach (KeyValuePair<int, DocumentSetCaseCollection> pair in this)
            {
                
                foreach (var cs in pair.Value)
                {

                    output ++;
                }

            }
            return output;
        }


        public AITrainingData GetAITrainingData()
        {
            AITrainingData output = new AITrainingData();

           

            Int32 pc = CountAllCases();
            output.inputs = new double[pc][];
            output.outputs = new Int32[pc];
            //output.outputs_for_neurons = new double[pc][];
            Int32 p = 0;

            Int32 classCount = this.Count;

            foreach (KeyValuePair<int, DocumentSetCaseCollection> pair in this)
            {
                
                foreach (DocumentSetCase cs in pair.Value)
                {
                    //Int32 fvc = cs.data.featureVectors.CountFeatureVectors();

                    List<Double> ins = new List<Double>();
                    
                    for (int i = 0; i < cs.data.featureVectors.Count; i++)
                    {
                        ins.AddRange(cs.data.featureVectors[i].GetValues(true));
                    }

                    output.inputs[p] = ins.ToArray();

                    //output.outputs_for_neurons[p] = new double[classCount];
                    //for (int b = 0; b < classCount; b++)
                    //{
                    //    if (b != pair.Key)
                    //    {
                    //        output.outputs_for_neurons[p][b] = 0;
                    //    } else
                    //    {
                    //        output.outputs_for_neurons[p][b] = 1;
                    //    }
                    //}
                    
                    output.outputs[p] = pair.Key;
                    p++;
                }
                
            }

            output.Deploy(this);




            return output;
        }


        protected classificationEvalMetricSet GetMetrics(IWebPostClassifier classifier)
        {
            classificationEvalMetricSet metrics = new classificationEvalMetricSet();


            foreach (KeyValuePair<int, DocumentSetCaseCollection> pair in this)
            {
                String className = pair.Value.validation.className;
                foreach (var setCase in pair.Value)
                {
                    var assocClass = setCase.data[classifier].selected;

                    if (assocClass != null)
                    {

                        if (pair.Value.rightClassID == assocClass.classID)
                        {
                            metrics[className].correct++;
                            metrics[className].truePositives++;
                        }
                        else
                        {
                            metrics[className].wrong++;
                            metrics[className].falseNegatives++;
                            metrics[assocClass.name].falsePositives++;
                        }
                        
                    }
                    
                }
            }
            return metrics;
        }

        ///// <summary>
        ///// Report on kFold case
        ///// </summary>
        ///// <returns></returns>
        //public Dictionary<IWebPostClassifier, DocumentSetCaseCollectionReport> GetCaseReport()
        //{
        //    Dictionary<IWebPostClassifier, DocumentSetCaseCollectionReport> reports = new Dictionary<IWebPostClassifier, DocumentSetCaseCollectionReport>();

        //    foreach (var classifier in validationCase.context.setup.classifiers)
        //    {

        //        DocumentSetCaseCollectionReport output = new DocumentSetCaseCollectionReport(validationCase.name + "_" + classifier.name + "_" + );

        //        output.kFoldCase = ;
        //        output.Classifier = classifier.name;

        //        classificationEvalMetricSet metrics = GetMetrics(classifier);

        //        foreach (KeyValuePair<int, DocumentSetCaseCollection> pair in this)
        //        {
        //            String className = pair.Value.validation.className;
        //            foreach (var setCase in pair.Value)
        //            {
        //                var assocClass = setCase.data[classifier].selected;

        //                if (assocClass != null)
        //                {

        //                    if (pair.Value.rightClassID == assocClass.classID)
        //                    {
        //                        output.Correct++;
        //                    }
        //                    else
        //                    {
        //                        output.Wrong++;
        //                    }

        //                }
        //                output.Targets++;
        //            }
        //        }

                
        //        output.Precision = metrics.GetPrecision();
        //        output.Recall = metrics.GetRecall();
        //        output.F1measure = metrics.GetF1();
        //        reports.Add(classifier, output);

        //    }

        //    return reports;

        //}


        public DocumentSetCaseCollectionSet(kFoldValidationCase _validationCase, DocumentSetClasses documentSetClasses)
        {
            classCollection = documentSetClasses;
            validationCase = _validationCase;

            
        }

        /// <summary>
        /// Gets or sets the class collection.
        /// </summary>
        /// <value>
        /// The class collection.
        /// </value>
        public DocumentSetClasses classCollection { get; set; }


    }

}