// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DocumentSetReportingExtensions.cs" company="imbVeles" >
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
using imbSCI.Core.extensions.io;
using imbWBI.Core.WebClassifier.cases;
using imbWBI.Core.math;
using imbSCI.Data.collection.nested;
using System.Drawing;

namespace imbWBI.Core.WebClassifier.reportData
{

    public static class DocumentSetReportingExtensions
    {
       
        public static DataTable BuildShema(this DocumentSetCaseCollection host, Boolean isSingleCategoryReport = true, Boolean isTrainingCollection = false, Boolean doFVAnalysis=true)
        {
            var setClass = host.setClass;
            var validationCase = host.validationCase;

            String tableName = "";
            if (isSingleCategoryReport)
            {
                
                tableName = (setClass.name + validationCase.name).getCleanFilepath();
            } else if (isTrainingCollection)
            {
                tableName = (setClass.name + validationCase.name + "_training").getCleanFilepath();
            } else 
            {
                tableName = validationCase.name + "full".getCleanFilePath();
            }

            DataTable output = new DataTable(tableName);

            


            if (!isSingleCategoryReport)
            {
                output.Add("Origin", "Name of the origin class", "", typeof(String), imbSCI.Core.enums.dataPointImportance.normal, "", "Origin").SetWidth(25).SetGroup("Case").SetDefaultBackground(Color.OrangeRed);
            }

            output.Add("Case", "Name of the case evaluated", "C_n", typeof(String), imbSCI.Core.enums.dataPointImportance.normal, "", "Case name").SetWidth(25).SetGroup("Case").SetDefaultBackground(Color.OrangeRed);




            if (!isTrainingCollection)
            {
                output.Add("Correct", "Number of classifiers that classified this case correctly. Web sites with zero or very low values accross multiple experiments are probably problem in the training set", "", typeof(Double), imbSCI.Core.enums.dataPointImportance.normal, "P1", "Mean Success Rate").SetUnit("%").SetGroup("Case Control");


                foreach (var cl in validationCase.context.setup.classifiers)
                {


                    output.Add("EvalTrue" + cl.name, "If classification result is correct", "C_" + cl.name, typeof(Int32), imbSCI.Core.enums.dataPointImportance.normal, "", "True").SetGroup(cl.name).SetUnit("Tp").SetWidth(7);
                    output.Add("ClassResultName" + cl.name, "Name of class associated by [" + cl.GetExperimentSufix() + "][" + cl.GetType().Name + "] classifier", "R_" + cl.name, typeof(String), imbSCI.Core.enums.dataPointImportance.normal, "", "Class name").SetGroup(cl.name).SetWidth(14);
                    
                }
            }

            foreach (var pair in setClass.parent.GetClasses())
            {

                foreach (var fv in validationCase.extractor.settings.featureVectors.serialization)
                {
                    if (fv.isActive)
                    {
                        output.Add(fv.name + "_" + pair.treeLetterAcronim, fv.description + " - for " + pair.name, fv.name + "_" + pair.classID, typeof(Double), imbSCI.Core.enums.dataPointImportance.normal, "F5",  fv.name + " for " + pair.name).SetGroup("FEATURE VECTORS");
                    }
                   // output.Add("Terms_" + pair.treeLetterAcronim, "If classification was true", "M_" + pair.classID, typeof(Int32), imbSCI.Core.enums.dataPointImportance.normal, "", "Matched for " + pair.name).SetGroup("FEATURE VECTORS");
                }
   
            }

            if (doFVAnalysis)
            {
                foreach (var fv in validationCase.extractor.settings.featureVectors.serialization)
                {
                    if (fv.isActive)
                    {
                        output.Add("FVRange" + fv.name, "Standard deviation of values in the similarity vector [" + fv.name + "] for this row", fv.name, typeof(Double), imbSCI.Core.enums.dataPointImportance.normal, "F5", fv.name + " S.Dev.").SetGroup("FVE Control metrics").SetDefaultBackground("#FF22639a");
                        output.Add("CFV_Ratio" + fv.name, "Value ratio indicating the position of correct category FV, within the range", fv.name, typeof(Double), imbSCI.Core.enums.dataPointImportance.normal, "F5", fv.name + " Range Position").SetGroup("FVE Control metrics").SetDefaultBackground("#FF22639a");
                    }
                    // output.Add("Terms_" + pair.treeLetterAcronim, "If classification was true", "M_" + pair.classID, typeof(Int32), imbSCI.Core.enums.dataPointImportance.normal, "", "Matched for " + pair.name).SetGroup("FEATURE VECTORS");
                }


                //output.Add("FVRange", "Max - Min value range - of FVE values for each category", "C_" + cl.name, typeof(Int32), imbSCI.Core.enums.dataPointImportance.normal, "", "True").SetGroup(cl.name).SetUnit("Tp").SetWidth(7);
            }


            output.Add("name", "UID name for the row", "id", typeof(String), imbSCI.Core.enums.dataPointImportance.normal, "", "Name").SetWidth(50).SetGroup("Case");
            return output;
        }

        public static void SetAdditionalInfo(this DocumentSetCaseCollection host, DataTable output, Boolean isSingleCategoryReport = true, Boolean isTrainingCollection = false)
        {
            var setClass = host.setClass;
            var validationCase = host.validationCase;

            if (isSingleCategoryReport) { output.SetAdditionalInfoEntry("Category", setClass.name); }

            output.SetAdditionalInfoEntry("Feature extractor", validationCase.extractor.name);
            output.SetAdditionalInfoEntry("Training cases", validationCase.trainingCases.First().Count());
            output.SetAdditionalInfoEntry("Eval cases", validationCase.evaluationCases.First().Count());
            output.SetAdditionalInfoEntry("Fold case", validationCase.name);

            
            if (isSingleCategoryReport)
            {
                output.SetDescription("Results of [" + validationCase.name + "] fold, for class[" + setClass.name + "]");
                output.SetTitle(setClass.name + " in " + validationCase.name);
                if (isSingleCategoryReport)
                {

                    output.AddExtra("Test sample subset :: Case-level Feature Extraction data on [" + host.setClass.name + "] class.");
                }
                else
                {
                    output.AddExtra("Test sample subset :: Case-level cassification results and feature Extraction data on [" + host.setClass.name + "] class [" + validationCase.name + "]");
                }
            }
            else if (isTrainingCollection)
            {
                output.SetTitle(validationCase.name + " training inputs");
                output.SetDescription("Feature Vectors used for [" + validationCase.name + "] fold, class[" + setClass.name + "]");
                if (isSingleCategoryReport)
                {

                    output.AddExtra("Training sample subset :: Case-level Feature Extraction data on [" + host.setClass.name + "] class.");
                }
                else
                {
                    output.AddExtra("Training sample subset :: Case-level cassification results and feature Extraction data on [" + host.setClass.name + "] class [" + validationCase.name + "]");
                }
            }
            else
            {
                output.SetTitle(validationCase.name + " - all classes");
                output.SetDescription("Results of [" + validationCase.name + "] fold - all classes");
            }

            foreach (var fv in validationCase.extractor.settings.featureVectors.serialization)
            {
                if (!fv.isActive)
                {
                    output.SetAdditionalInfoEntries("FV " + fv.name, "is not active");
                }

                
                // output.Add("Terms_" + pair.treeLetterAcronim, "If classification was true", "M_" + pair.classID, typeof(Int32), imbSCI.Core.enums.dataPointImportance.normal, "", "Matched for " + pair.name).SetGroup("FEATURE VECTORS");
            }

          

            output.SetAggregationAspect(imbSCI.Core.math.aggregation.dataPointAggregationAspect.subSetOfRows);
            

        }

        

        public static DataRow BuildRow(this DocumentSetCaseCollection host, DocumentSetCase setCase, DataTable output, Boolean isTrainingCollection = false, Boolean doFVAnalysis=true)
        {
            var setClass = host.setClass;
            var validationCase = host.validationCase;

            DataRow dr = output.NewRow();

            dr["name"] = host.validationCase.name + "_" + setCase.subject.name;

            if (output.Columns.Contains("Origin"))
            {
                dr["Origin"] = host.setClass.name;
            }


            dr["Case"] = setCase.subject.name;

            if (!isTrainingCollection)
            {
                Int32 cor = 0;
                foreach (var cl in validationCase.context.setup.classifiers)
                {
                    String cName = "";
                    Int32 t = 0;
                    if (setCase.data[cl].selected != null)
                    {
                        cName = setCase.data[cl].selected.name;
                        if (setCase.data[cl].selected.classID == host.rightClassID)
                        {
                            t = 1;
                        }
                        else
                        {
                            t = 0;
                        }
                    }
                    else
                    {
                        cName = "- not set -";
                    }
                    dr["ClassResultName" + cl.name] = cName;

                    cor += t;

                    dr["EvalTrue" + cl.name] = t;

                }

                dr["Correct"] = cor.GetRatio(validationCase.context.setup.classifiers.Count);
            }

            foreach (var cl in setCase.data.setClassCollection.GetClasses())
            {
                foreach (var fv in validationCase.extractor.settings.featureVectors.serialization)
                {
                    if (fv.isActive)
                    {


                        dr[fv.name + "_" + cl.treeLetterAcronim] = setCase.data.featureVectors[cl.classID][fv];
                    }
                }
            }



            if (doFVAnalysis)
            {

               // aceDictionary2D<String, String, rangeFinder> matrix = new aceDictionary2D<string, string, rangeFinder>();
               
                Dictionary<String, rangeFinderWithData> rangers = new Dictionary<string, rangeFinderWithData>();

                foreach (var cl in setCase.data.setClassCollection.GetClasses())
                {
                    foreach (var fv in validationCase.extractor.settings.featureVectors.serialization)
                    {
                        if (fv.isActive)
                        {
                            if (!rangers.ContainsKey(fv.name)) rangers.Add(fv.name, new rangeFinderWithData(fv.name));

                            rangers[fv.name].Learn(setCase.data.featureVectors[cl.classID][fv]);
                        }
                    }
                }



                foreach (var fv in validationCase.extractor.settings.featureVectors.serialization)
                {

                    if (fv.isActive)
                    {
                        dr["FVRange" + fv.name] = rangers[fv.name].doubleEntries.GetStdDeviation(false);
                        dr["CFV_Ratio" + fv.name] = rangers[fv.name].GetPositionInRange(setCase.data.featureVectors[setClass.classID][fv]);
                        // output.Add("CFV_Ratio" + fv.name, "Value ratio indicating the position of correct category FV, within the range", fv.name, typeof(Double), imbSCI.Core.enums.dataPointImportance.normal, "F5", fv.name + " Range Position").SetGroup("FV Metrics");
                    }
                    // output.Add("Terms_" + pair.treeLetterAcronim, "If classification was true", "M_" + pair.classID, typeof(Int32), imbSCI.Core.enums.dataPointImportance.normal, "", "Matched for " + pair.name).SetGroup("FEATURE VECTORS");
                }

                
            }

          

            output.Rows.Add(dr);
            return dr;
        }


    }

}