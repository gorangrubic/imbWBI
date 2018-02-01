// --------------------------------------------------------------------------------------------------------------------
// <copyright file="secondaryReportOnFVE.cs" company="imbVeles" >
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
using imbNLP.PartOfSpeech.TFModels.semanticCloud;
using imbSCI.Core.attributes;
using imbSCI.Core.files;
using imbSCI.Core.files.folders;
using imbSCI.Core.extensions.text;
using imbSCI.Core.extensions.data;
using imbSCI.Core.reporting;
using imbWBI.Core.WebClassifier.experiment;
using System.ComponentModel;
using System.IO;
using System.Text;
using imbWBI.Core.WebClassifier.wlfClassifier;
using imbSCI.Core.math;
using imbSCI.Data;
using imbSCI.Graph.DOT;
using imbSCI.Graph.attributes;

namespace imbWBI.Core.WebClassifier.reportData
{

    /// <summary>
    /// FVE model level entry
    /// </summary>
    public class secondaryReportOnFVE
    {
        
        public secondaryReportOnFVE() { }

        public static secondaryReportOnFVE GetExperimentIntroductionLine(String experimentName, Int32 folds)
        {
            secondaryReportOnFVE output = new secondaryReportOnFVE();

            output.Experiment = experimentName;
            output.Folds = folds;
            output.FVEHash = "------------";
            output.FVEModel = "------";
            output.FVPType = "------";
            output.HTMLTagFactors = "------";
            output.UID = experimentName;
            output.Comment = "[separaptor row]";
            return output;
        }

        /// <summary>
        /// Updates the secondary record.
        /// </summary>
        /// <param name="fve">The fve.</param>
        public void UpdateSecondaryRecord(semanticFVExtractor fve)
        {
            //semanticFVExtractor fve = this;
            var record = this;

            record.FVEModel = fve.name;


            //record.FVEHash = fve.get

            record.TermDemotion = "";
            if (fve.settings.semanticCloudFilter.doDemoteAnyRepeatingPrimaryTerm) record.TermDemotion += "[P-DEM]";
            if (fve.settings.semanticCloudFilter.doDemoteAnyRepeatingSecondaryTerm) record.TermDemotion += "[S-DEM]";
            if (fve.settings.semanticCloudFilter.doAssignMicroWeightInsteadOfRemoval) record.TermDemotion += "[P-MIN]";

            foreach (var fv in fve.settings.featureVectors.serialization)
            {
                if (fv.isActive)
                {
                    record.FVPType += fv.name + " ";
                }

            }

            record.IDF = fve.termTableConstructor.settings.doUseIDF;
            record.DFC = fve.termTableConstructor.settings.documentFrequencyMaxFactor;

            record.CloudDSF = fve.CloudConstructor.settings.documentSetFreqLowLimit;
            record.CloudTCF = fve.CloudConstructor.settings.termInChunkLowerLimit;
            record.CloudPTT = fve.CloudConstructor.settings.primaryTermLowTargetCount;

            record.CloudAlgorithm = fve.CloudConstructor.settings.algorithm.ToString();



            record.HTMLTagFactors = fve.termTableConstructor.settings.titleTextFactor + ":" + fve.termTableConstructor.settings.anchorTextFactor + ":" + fve.termTableConstructor.settings.contentTextFactor;
            record.TCBOn = fve.CloudConstructor.settings.doFactorToClassClouds;
            record.TermCategory = fve.CloudConstructor.settings.PrimaryTermWeightFactor + ":" + fve.CloudConstructor.settings.SecondaryTermWeightFactor + ":" + fve.CloudConstructor.settings.ReserveTermWeightFactor;

            if (fve.settings.semanticCloudFilter.doDivideWeightWithCloudFrequency || fve.settings.semanticCloudFilter.doUseSquareFunctionOfCF)
            {
                if (!fve.settings.semanticCloudFilter.doUseSquareFunctionOfCF)
                {
                    record.ReduxFunction = "[1/CF]";
                }
                else
                {
                    record.ReduxFunction = "[1/Sq(CF)]";
                }
            }
            else
            {
                record.ReduxFunction = "[OFF]";
            }
            record.TermExpansionOptions = fve.settings.caseTermExpansionOptions.ToString();

            List<String> ops = record.TermExpansionOptions.SplitSmart(",");
            List<String> tags = new List<string>();
            String teo = "";
            foreach (String op in ops)
            {
                teo = teo.add(op.imbGetAbbrevation(3, true), ",");
            }

            record.TermExpansionOptions = teo;
            record.StrictPOS = fve.termTableConstructor.settings.strictPosTypePolicy;
            

            record.TermExpansion = fve.settings.caseTermExpansionSteps;
            record.LowpassFreq = fve.settings.semanticCloudFilter.lowPassFilter;

            if (fve.settings.semanticCloudFilter.isActive)
            {
                if (fve.settings.semanticCloudFilter.doCutOffByCloudFrequency)
                {

                    if (!fve.settings.semanticCloudFilter.doAssignMicroWeightInsteadOfRemoval)
                    {
                        record.LowPassFunction = "[Remove]";
                    }
                    else
                    {
                        record.LowPassFunction = "[W=mini]";
                    }

                }
                else
                {
                    record.LowPassFunction = "[OFF]";
                }
            }
            else
            {
                record.LowPassFunction = "[OFF]";
                record.LowpassFreq = 0;
            }


            String ls = objectSerialization.ObjectToXML(fve); 
            
            record.FVEHash = md5.GetMd5Hash(ls, false);

            record.UID = md5.GetMd5Hash(Path + record.FVEHash + record.Experiment, false);

        }



        /// <summary> Experiment code name </summary>
        [Category("Test ID")]
        [DisplayName("Experiment")] //[imb(imbAttributeName.measure_letter, "")]
        [Description("Experiment code name")] // [imb(imbAttributeName.reporting_escapeoff)]
        [imb(imbAttributeName.reporting_columnWidth, 45)]
        [GraphExtraction]
        public String Experiment { get; set; } = default(String);



        /// <summary> Number of folds </summary>
        [Category("Test ID")]
        [DisplayName("Folds")]
        [imb(imbAttributeName.measure_letter, "k")]
        [imb(imbAttributeName.measure_setUnit, "n")]
        [Description("Number of folds")] // [imb(imbAttributeName.measure_important)][imb(imbAttributeName.reporting_valueformat, "")]
        [imb(imbAttributeName.reporting_columnWidth, 7)]
        public Int32 Folds { get; set; } = default(Int32);




        /// <summary> Feature Vector Model name </summary>
        [Category("Test ID")]
        [DisplayName("FVE Model")] //[imb(imbAttributeName.measure_letter, "")]
        [Description("Feature Vector Model name")] // [imb(imbAttributeName.reporting_escapeoff)]
        [GraphExtraction]
        public String FVEModel { get; set; } = "";


        /// <summary> Feature Vector Provider </summary>
        [Category("Test ID")]
        [DisplayName("FVP Type")] //[imb(imbAttributeName.measure_letter, "")]
        [Description("Feature Vector Provider(S) used to produce Feature Vectors")] // [imb(imbAttributeName.reporting_escapeoff)]
        [imb(imbAttributeName.reporting_columnWidth, 10)]
        public String FVPType { get; set; } = "";



        /// <summary> Number of classifiers </summary>
        [Category("Test ID")]
        [DisplayName("Classifiers")]
        [imb(imbAttributeName.measure_letter, "Cn")]
        [imb(imbAttributeName.measure_setUnit, "n")]
        [Description("Number of classifiers")] // [imb(imbAttributeName.measure_important)][imb(imbAttributeName.reporting_valueformat, "")]
        [imb(imbAttributeName.reporting_columnWidth, 10)]
        public Int32 Classifiers { get; set; } = default(Int32);





        /// <summary> if was sample randomized  </summary>
        [Category("Test ID")]
        [DisplayName("Randomized")]
        [imb(imbAttributeName.measure_letter, "")]
        [Description("if sample was randomized ")]
        public Boolean Randomized { get; set; } = false;




        /// <summary> Classification algorithm that achieved the highest F1 score </summary>
        [Category("Best Result")]
        [DisplayName("Classifier")] //[imb(imbAttributeName.measure_letter, "")]
        [Description("Classification algorithm that achieved the highest F1 score")] // [imb(imbAttributeName.reporting_escapeoff)]
        [imb(imbAttributeName.reporting_columnWidth, 10)]
        public String Classifier { get; set; } = default(String);


        /// <summary> The highest F1-score achieved </summary>
        [Category("Best Result")]
        [DisplayName("F1Score")]
        [imb(imbAttributeName.measure_letter, "F1")]
        [imb(imbAttributeName.measure_setUnit, "%")]
        [Description("The highest F1-score achieved")] // [imb(imbAttributeName.measure_important)][imb(imbAttributeName.reporting_escapeoff)]
        [imb(imbAttributeName.reporting_valueformat, "F5")]
        [imb(imbAttributeName.basicColor, "#FFFF0000")]
        [imb(imbAttributeName.reporting_columnWidth, 10)]
        public Double F1Score { get; set; } = default(Double);






        /// <summary> HTML Tag Weights mode </summary>
        [Category("TF-IDF")]
        [DisplayName("HTMLTagFactors")] //[imb(imbAttributeName.measure_letter, "")]
        [Description("How HTML Tag Weights are distributed --- the format: [Title and H tags] : [Link*] : [Content], * link text includes ALT text attribute of image tags observed within A tag")] // [imb(imbAttributeName.reporting_escapeoff)]
        [imb(imbAttributeName.reporting_columnWidth, 10)]
        [imb(imbAttributeName.basicColor, "#FF0033FF")]
        [imb(imbAttributeName.measure_setUnit, "H:L:T")]
        public String HTMLTagFactors { get; set; } = default(String);



        /// <summary> True - if IDF was computed, false - if only normalized TF was used  </summary>
        [Category("TF-IDF")]
        [DisplayName("IDF")]
        [imb(imbAttributeName.measure_setUnit, "On/Off")]
        [Description("True - if IDF was computed, false - if only normalized TF was used ")]
        
        [imb(imbAttributeName.reporting_columnWidth, 7)]
        public Boolean IDF { get; set; } = false;


        /// <summary> Document Frequency Correction -- ratio that multiplies document count before IDF computation </summary>
        [Category("TF-IDF")]
        [DisplayName("DFC")]
        [imb(imbAttributeName.measure_letter, "")]
        [imb(imbAttributeName.measure_setUnit, "%")]
        [Description("Document Frequency Correction -- ratio that multiplies document count before IDF computation")] // [imb(imbAttributeName.measure_important)][imb(imbAttributeName.reporting_valueformat, "")][imb(imbAttributeName.reporting_escapeoff)]
        [imb(imbAttributeName.reporting_valueformat, "F2")]
        [imb(imbAttributeName.reporting_columnWidth, 7)]
        public Double DFC { get; set; } = default(Double);

        
        /// <summary> Semantic expansion steps - in how many iterations were Case Web Lemma terms expanded, before Semantic Similarity assesment </summary>
        [Category("Term Expansion")]
        [DisplayName("Steps")]
        [imb(imbAttributeName.measure_letter, "Stx")]
        [imb(imbAttributeName.measure_setUnit, "n")]
        [Description("Semantic expansion steps - in how many iterations were Case Web Lemma terms expanded, before Semantic Similarity assesment")] // [imb(imbAttributeName.measure_important)][imb(imbAttributeName.reporting_valueformat, "")]
        [imb(imbAttributeName.reporting_columnWidth, 7)]
        public Int32 TermExpansion { get; set; } = default(Int32);



        /// <summary> Lemma expansion options that were applied </summary>
        [Category("Term Expansion")]
        [DisplayName("Options")]
        [Description("Lemma expansion options that were applied")]
        [imb(imbAttributeName.reporting_columnWidth, 15)]
        public String TermExpansionOptions { get; set; } = "";




        /// <summary> Redux Function - applied for term weight reduction by cloud matrix </summary>
        [Category("Cloud Matrix")]
        [DisplayName("Redux Function")] //[imb(imbAttributeName.measure_letter, "")]
        [Description("Redux Function - applied for term weight reduction by cloud matrix: [DIV] W=1/CF,  [SQF] W=1/CF^2  [OFF] W=W")] // [imb(imbAttributeName.reporting_escapeoff)]
        [imb(imbAttributeName.reporting_columnWidth, 10)]
        [imb(imbAttributeName.basicColor, "#FF66FF00")]
        public String ReduxFunction { get; set; } = "not set";


        [Category("Cloud Matrix")]
        [DisplayName("Lowpass mode")] //[imb(imbAttributeName.measure_letter, "")]
        [Description("LPF Function - how term is affected if CF over set low-pass frequency: [CUT] removes the terms, [MiW] sets the minimum weight (just above 0) to matched terms, [OFF] LPF is off ")] // [imb(imbAttributeName.reporting_escapeoff)]
        [imb(imbAttributeName.reporting_columnWidth, 10)]
        public String LowPassFunction { get; set; } = "not set";


        /// <summary> Low pass cloud frequency - used in Cloud Matrix settings </summary>
        [Category("Cloud Matrix")]
        [DisplayName("Lowpass freq")]
        [imb(imbAttributeName.measure_letter, "")]
        [imb(imbAttributeName.measure_setUnit, "n")]
        [Description("Low pass cloud frequency - used in Cloud Matrix settings")] // [imb(imbAttributeName.measure_important)][imb(imbAttributeName.reporting_valueformat, "")]
        [imb(imbAttributeName.reporting_columnWidth, 7)]
        public Int32 LowpassFreq { get; set; } = default(Int32);




        [Category("Cloud Matrix")]
        [DisplayName("Term demotion")] //[imb(imbAttributeName.measure_letter, "")]
        [Description("Term Demotion mode - [PR] Removes any primary term found to be more then in one category [PD] Demotes Primary terms if CF > 1, [SD] Demotes secondary if CF > 1, [OFF] term demotion is not used")] // [imb(imbAttributeName.reporting_escapeoff)]
        [imb(imbAttributeName.reporting_columnWidth, 20)]
        public String TermDemotion { get; set; } = "not set";



        /// <summary> If the Term Category Boost is on </summary>
        [Category("Cloud Construction")]
        [DisplayName("TCBOn")]
        [imb(imbAttributeName.measure_letter, "")]
        [Description("If the Term Category Boost is on")]
        [imb(imbAttributeName.reporting_columnWidth, 7)]
        [imb(imbAttributeName.basicColor, "#FFFF6600")]
        public Boolean TCBOn { get; set; } = false;



        /// <summary>  </summary>
        [Category("Cloud Construction")]
        [DisplayName("TCB")] //[imb(imbAttributeName.measure_letter, "")]
        [Description("Term category bonus, applied on term weights --- the format: [Prim] : [Secondary] : [Reserve]")] // [imb(imbAttributeName.reporting_escapeoff)]
        [imb(imbAttributeName.reporting_columnWidth, 10)]
        [imb(imbAttributeName.measure_setUnit, "Pri:Sec:Res")]
        public String TermCategory { get; set; } = default(String);




        /// <summary> If the Strict POS type policy was used </summary>
        [Category("Details")]
        [DisplayName("Strict POS")]
        [imb(imbAttributeName.measure_letter, "")]
        [Description("If the Strict POS type policy was used: if true then tokens with POS type flags not specified in allowed types will be banned from Lemma Table")]
        [imb(imbAttributeName.reporting_columnWidth, 7)]
        public Boolean StrictPOS { get; set; } = false;




        /// <summary> If the Strict POS type policy was used </summary>
        [Category("Cloud Construction")]
        [DisplayName("Algorithm")]
        [imb(imbAttributeName.measure_letter, "")]
        [Description("Algorithm used to construct the Semantic Cloud")]
        [imb(imbAttributeName.reporting_columnWidth, 10)]
        public String CloudAlgorithm { get; set; } 

        /// <summary> If the Strict POS type policy was used </summary>
        [Category("Cloud Construction")]
        [DisplayName("DSF")]
        [imb(imbAttributeName.measure_letter, "")]
        [Description("Minimum or starting - Document Set Frequency used to select primary set of chunks")]
        [imb(imbAttributeName.reporting_columnWidth,7)]
        public Int32 CloudDSF { get; set; } = 1;

        [Category("Cloud Construction")]
        [DisplayName("TCF")]
        [imb(imbAttributeName.measure_letter, "")]
        [Description("Cloud term in chunk frequency - the minimum required for a term to be included in the cloud")]
        [imb(imbAttributeName.reporting_columnWidth, 7)]
        public Int32 CloudTCF { get; set; } = 0;

        [Category("Cloud Construction")]
        [DisplayName("PTT")]
        [imb(imbAttributeName.measure_letter, "")]
        [Description("Primary Term Target - when the optimization algorithm should stop downsizing the cloud")]
        [imb(imbAttributeName.reporting_columnWidth, 7)]
        public Int32 CloudPTT { get; set; }





        /// <summary> Ratio </summary>
        [Category("Results analytics")]
        [DisplayName("F1 Score Mean")]
        [imb(imbAttributeName.measure_letter, "F1_cm")]
        [imb(imbAttributeName.measure_setUnit, "%")]
        [Description("Mean F1 Score computed for all classifiers tested with this FVE model. The measure is not to be directly iterpreted for any conclusion.")] // [imb(imbAttributeName.measure_important)][imb(imbAttributeName.reporting_valueformat, "")][imb(imbAttributeName.reporting_escapeoff)]
        [imb(imbAttributeName.reporting_columnWidth, 7)]
        [imb(imbAttributeName.reporting_valueformat, "F5")]
        public Double F1ScoreMean { get; set; } = default(Double);


        /// <summary> Ratio </summary>
        [Category("Results analytics")]
        [DisplayName("F1 Score Deviation")]
        [imb(imbAttributeName.measure_letter, "F1_var")]
        [imb(imbAttributeName.measure_setUnit, "%")]
        [Description("Standard Deviation of F1 scores for all classifiers tested with this FVE mode. This measure, together with F1 mean, indicates how effective is the FVE model")][imb(imbAttributeName.reporting_valueformat, "F5")]
        // [imb(imbAttributeName.measure_important)][imb(imbAttributeName.reporting_valueformat, "")][imb(imbAttributeName.reporting_escapeoff)]
        public Double F1ScoreDeviation { get; set; }



        /// <summary> Cross category number of nodes in the Semantic Cloud </summary>
        [Category("Cloud metrics")]
        [DisplayName("TotalSize")]
        [imb(imbAttributeName.measure_letter, "|T|")]
        [imb(imbAttributeName.measure_setUnit, "N_n")]
        [Description("Cross category number of nodes in the Semantic Cloud")]
        [imb(imbAttributeName.reporting_columnWidth, 10)]
        [imb(imbAttributeName.reporting_valueformat, "F2")]// [imb(imbAttributeName.measure_important)][imb(imbAttributeName.reporting_valueformat, "")][imb(imbAttributeName.reporting_escapeoff)]
        public Double NodeCount { get; set; } = default(Double);


        /// <summary> Cross category average number of links per node, in the Semantic Cloud </summary>
        
        [Category("Cloud metrics")]
        [imb(imbAttributeName.measure_letter, "|T|/|L|")]
        [imb(imbAttributeName.measure_setUnit, "L_r")]
        [imb(imbAttributeName.reporting_columnWidth, 10)]
        [imb(imbAttributeName.reporting_valueformat, "F3")]
        [Description("Cross category average number of links per node, in the Semantic Cloud")] // [imb(imbAttributeName.measure_important)][imb(imbAttributeName.reporting_valueformat, "")][imb(imbAttributeName.reporting_escapeoff)]
        public Double LinkRatio { get; set; } = default(Double);

        [Category("Cloud metrics")]
        [imb(imbAttributeName.measure_letter, "D")]
        [imb(imbAttributeName.measure_setUnit, "n")]
        [imb(imbAttributeName.reporting_columnWidth, 10)]
        [imb(imbAttributeName.reporting_valueformat, "F5")]
        [Description("Graph ping length measured from the Primary Terms. Ping length can be interpreted as maximum possibile term expansion")] // [imb(imbAttributeName.measure_important)][imb(imbAttributeName.reporting_valueformat, "")]
        public Double GraphDepth { get; set; } = default(Double);



        /// <summary> Comment - additional notes on the setup </summary>
        [Category("Meta")]
        [DisplayName("Path")] //[imb(imbAttributeName.measure_letter, "")]
        [Description("Relative directory path where the experiment report was extracted from")] // [imb(imbAttributeName.reporting_escapeoff)]
        [imb(imbAttributeName.reporting_columnWidth, 40)]
        public String Path { get; set; } = default(String);
        

        /// <summary> Comment - additional notes on the setup </summary>
        [Category("Meta")]
        [DisplayName("Comment")] //[imb(imbAttributeName.measure_letter, "")]
        [Description("Comment - additional notes on the setup")] // [imb(imbAttributeName.reporting_escapeoff)]
        [imb(imbAttributeName.reporting_columnWidth, 20)]
        public String Comment { get; set; } = default(String);



        /// <summary> Se </summary>
        [Category("Meta")]
        [DisplayName("FVEHash")] //[imb(imbAttributeName.measure_letter, "")]
        [Description("MD5 hash computed from serialized XML of the model setup clone. From the cloned XML: description, name and semantic step expansion values were removed before MD5 hashing. The hash should show if there was accidental mistake when was configured.")] // [imb(imbAttributeName.reporting_escapeoff)]
        [imb(imbAttributeName.reporting_columnWidth, 40)]
        public String FVEHash { get; set; } = default(String);


        /// <summary> Unique identifier for the FVE recods </summary>
        [Category("Meta")]
        [DisplayName("UID")] //[imb(imbAttributeName.measure_letter, "")]
        [Description("Unique identifier for the FVE recods")] // [imb(imbAttributeName.reporting_escapeoff)]
        [imb(imbAttributeName.reporting_columnWidth, 40)]
        public String UID { get; set; } = default(String);


    }

}