// --------------------------------------------------------------------------------------------------------------------
// <copyright file="featureExtractionMetrics.cs" company="imbVeles" >
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
using imbSCI.Core.attributes;
using imbSCI.Core.math;
using imbWBI.Core.math;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace imbWBI.Core.WebClassifier.reportData
{
    /// <summary>
    /// Data object keeping data on feature vector extraction model metrics
    /// </summary>
    public class featureExtractionMetrics
    {
        public featureExtractionMetrics()
        {

        }

        public featureExtractionMetrics(String _model, String _category)
        {
            modelName = _model;
            categoryName = _category;
        }

        /// <summary>
        /// Computes the average from given set of metrics
        /// </summary>
        /// <param name="source">The source.</param>
        public void ComputeAverage(IEnumerable<featureExtractionMetrics> source)
        {
            Int32 c = 0;
            foreach (var item in source)
            {
                AddValues(item);
                c++;
            }
            DivideValues(Convert.ToDouble(c));

        }

        public void DivideValues(Double divisor)
        {
            SuccessRate = SuccessRate.GetRatio(divisor);
            SuccessStDev = SuccessStDev.GetRatio(divisor);
            RangeWidthStd = RangeWidthStd.GetRatio(divisor);
            RangeWidthAvg = RangeWidthAvg.GetRatio(divisor);
            RangePositionStd = RangePositionStd.GetRatio(divisor);
            RangePositionAvg = RangePositionAvg.GetRatio(divisor);

        }


        public void AddValues(featureExtractionMetrics source)
        {
            SuccessRate += source.SuccessRate;
            SuccessStDev += source.SuccessStDev;
            RangeWidthStd += source.RangeWidthStd;
            RangeWidthAvg += source.RangeWidthAvg;
            RangePositionStd += source.RangePositionStd;
            RangePositionAvg += source.RangePositionAvg;

        }


        public void SetValues(rangeFinderForDataTable ranger)
        {
            SuccessRate = ranger["Correct"].Average;
            SuccessStDev = ranger["Correct"].doubleEntries.GetStdDeviation(false);

            var rangers = ranger.GetRangerStartingWith("FVRange");

            RangeWidthAvg = rangers.Average(x => x.Average);
            RangeWidthStd = rangers.Average(x => x.doubleEntries.GetStdDeviation(false));

            var positionRangers = ranger.GetRangerStartingWith("CFV_Ratio");



            RangePositionAvg = positionRangers.Average(x => x.Average); // ranger["CFV_Ratio" + modelName].Average;
            RangePositionStd = positionRangers.Average(x => x.doubleEntries.GetStdDeviation(false)); //ranger["CFV_Ratio" + modelName].doubleEntries.GetStdDeviation(false);

        }

        [Category("Label")]
        [DisplayName("Model")] //[imb(imbAttributeName.measure_letter, "")]
        [Description("Name of the model")]
        public String modelName { get; set; }



        /// <summary> Name of category this metrics was gathered from  </summary>
        [Category("Label")]
        [DisplayName("categoryName")] //[imb(imbAttributeName.measure_letter, "")]
        [Description("Name of category this metrics was gathered from ")] // [imb(imbAttributeName.reporting_escapeoff)]
        public String categoryName { get; set; } = default(String);





        /// <summary> Average success rate of this category or for complete FVE </summary>
        [Category("Sample Performance")]
        [DisplayName("AverageSuccess")]
        [imb(imbAttributeName.measure_letter, "Tp")]
        [imb(imbAttributeName.measure_setUnit, "%")]
        [imb(imbAttributeName.reporting_valueformat, "P2")]
        [Description("Macroaverage success rate of cases within this sample span - very low success rate means that case is probably wrongly placed (by human) in the category at beginning")] // [imb(imbAttributeName.measure_important)][imb(imbAttributeName.reporting_valueformat, "")][imb(imbAttributeName.reporting_escapeoff)]
        public Double SuccessRate { get; set; } = default(Double);


        /// <summary> Ratio </summary>
        [Category("Sample Performance")]
        [DisplayName("SuccessStDev")]
        [imb(imbAttributeName.measure_letter, "")]
        [imb(imbAttributeName.measure_setUnit, "%")]
        [Description("Standard deviation within measured success rate for cases in this sample span - greater st.dev. means greater difference in classifiers' performance regarding the cases in the scope ")] // [imb(imbAttributeName.measure_important)][imb(imbAttributeName.reporting_valueformat, "")][imb(imbAttributeName.reporting_escapeoff)]
        public Double SuccessStDev { get; set; } = default(Double);





        /// <summary> Range position </summary>
        [Category("FVE Performance")]
        [DisplayName("Range Position")]
        [imb(imbAttributeName.measure_letter, "FVp")]
        [imb(imbAttributeName.measure_setUnit, "%")]
        [imb(imbAttributeName.reporting_valueformat, "F5")]
        [Description("Average range position of cases within the right Feature Vactors, for the proper category - greater position avg => tells that more cases are property favorized in regard to their category similarity")] // [imb(imbAttributeName.measure_important)][imb(imbAttributeName.reporting_valueformat, "")][imb(imbAttributeName.reporting_escapeoff)]
        public Double RangePositionAvg { get; set; } = default(Double);



        /// <summary> Average range width - range of feature vector values accross the categoris </summary>
        [Category("FVE Performance")]
        [DisplayName("Range Width Avg")]
        [imb(imbAttributeName.measure_letter, "FVw")]
        [imb(imbAttributeName.measure_setUnit, "%")]
        [imb(imbAttributeName.reporting_valueformat, "F5")]
        [Description("By range width we mean Standard Deviation of similarity vector dimensions - i.e. range of feature vector values accross the categoris - greater width => FVE is making greater difference when describing case-vs-category similarity")] // [imb(imbAttributeName.measure_important)][imb(imbAttributeName.reporting_escapeoff)]
        public Double RangeWidthAvg { get; set; } = default(Double);

        [Category("FVE Performance")]
        [DisplayName("Range Position Std")]
        [imb(imbAttributeName.measure_letter, "FVp_dev")]
        [imb(imbAttributeName.measure_setUnit, "%")]
        [imb(imbAttributeName.reporting_valueformat, "F5")]
        [Description("Average range position of cases within the right Feature Vactors, for the proper category. Lower standard deviation => cases are better described/matched with the category ")] // [imb(imbAttributeName.measure_important)][imb(imbAttributeName.reporting_valueformat, "")][imb(imbAttributeName.reporting_escapeoff)]
        public Double RangePositionStd { get; set; } = default(Double);



        /// <summary> Average range width - range of feature vector values accross the categoris </summary>
        [Category("FVE Performance")]
        [DisplayName("Range Width Std")]
        [imb(imbAttributeName.measure_letter, "FVw_dev")]
        [imb(imbAttributeName.measure_setUnit, "%")]
        [imb(imbAttributeName.reporting_valueformat, "F5")]
        [Description("By range width we mean Standard Deviation of similarity vector dimensions - i.e. range of feature vector values accross the categoris. Lower st.dev. => cases are closer to equally good, described by FVE ")] // [imb(imbAttributeName.measure_important)][imb(imbAttributeName.reporting_escapeoff)]
        public Double RangeWidthStd { get; set; } = default(Double);





        /// <summary> Suitability measure describes how effectivly FVE describes cases, in regard to their categories </summary>
        [Category("Score")]
        [DisplayName("S1Measure")]
        [imb(imbAttributeName.measure_letter, "FVw * FVp")]
        [imb(imbAttributeName.measure_setUnit, "%")]
        [Description("Suitability measure S1 describes how effectivly FVE describes the cases, in regard to their categories")] // [imb(imbAttributeName.measure_important)][imb(imbAttributeName.reporting_valueformat, "")][imb(imbAttributeName.reporting_escapeoff)]
        public Double S1Measure { get {
                return RangePositionAvg * RangeWidthAvg;
            }
            set { } }





        /// <summary> name of this metrics entry </summary>
        [Category("Label")]
        [DisplayName("Name")] //[imb(imbAttributeName.measure_letter, "")]
        [Description("name of this metrics entry")] // [imb(imbAttributeName.reporting_escapeoff)]
        [XmlIgnore]
        public String Name {
            get {
                return modelName + "_" + categoryName;
            }
            set
            {
            }
        }




     //   public List<rangeFinderForDataTable> categoryRangeFinders { get; set; } = new List<rangeFinderForDataTable>();


    }
}
