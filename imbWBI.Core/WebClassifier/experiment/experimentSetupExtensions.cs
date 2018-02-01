// --------------------------------------------------------------------------------------------------------------------
// <copyright file="experimentSetupExtensions.cs" company="imbVeles" >
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
using imbACE.Core.core;
using imbMiningContext.TFModels.ILRT;
using imbSCI.Core.files.folders;
using imbSCI.Core.reporting;
using imbWBI.Core.WebClassifier.cases;
using imbWBI.Core.WebClassifier.core;
using imbWBI.Core.WebClassifier.validation;
using imbWBI.Core.WebClassifier.wlfClassifier;
using imbWEM.Core.project;
using System.Text;
using imbSCI.Core.extensions.data;
using imbSCI.Core.extensions.io;
using imbSCI.Data;
using imbSCI.Data.collection.nested;
using System.Xml.Serialization;
using imbSCI.Core.files.fileDataStructure;
using imbNLP.PartOfSpeech.flags.basic;
using System.ComponentModel;
using imbSCI.Core.extensions.text;
using imbNLP.PartOfSpeech.TFModels.semanticCloud;
using imbNLP.PartOfSpeech.TFModels.semanticCloudMatrix;

namespace imbWBI.Core.WebClassifier.experiment
{

    /// <summary>
    /// 
    /// </summary>
    public static class experimentSetupExtensions
    {





        /// <summary>
        /// Sets the tw.
        /// </summary>
        /// <param name="fve">The fve.</param>
        /// <param name="flag">The flag.</param>
        public static void SetTW(this semanticFVExtractor fve, String flag = "std", Double DFC=1.1, Boolean IDFOn = true)
        {
            flag = flag.ToLower();

            fve.termTableConstructor.settings.documentFrequencyMaxFactor= DFC;
            fve.termTableConstructor.settings.doUseIDF = IDFOn;

            switch (flag) {
                case "std":
                    fve.termTableConstructor.settings.titleTextFactor = 1;
                    fve.termTableConstructor.settings.anchorTextFactor = 0.75;
                    fve.termTableConstructor.settings.contentTextFactor = 0.5;
                    break;
                case "bst":
                    fve.termTableConstructor.settings.titleTextFactor = 10;
                    fve.termTableConstructor.settings.anchorTextFactor = 1;
                    fve.termTableConstructor.settings.contentTextFactor = 0.1;
                    break;
                case "off":
                    fve.termTableConstructor.settings.titleTextFactor = 1;
                    fve.termTableConstructor.settings.anchorTextFactor = 1;
                    fve.termTableConstructor.settings.contentTextFactor = 1;
                    break;
            }

        }


        /// <summary>
        /// Sets the tc.
        /// </summary>
        /// <param name="constructor">The constructor.</param>
        /// <param name="flag">The flag.</param>
        public static void SetTC(this cloudConstructorSettings constructor, String flag = "std")
        {
            flag = flag.ToLower();

            
            switch (flag)
            {
                case "std":
                    constructor.PrimaryTermWeightFactor = 2;
                    constructor.SecondaryTermWeightFactor = 1;
                    constructor.ReserveTermWeightFactor = 0.5;
                    constructor.doFactorToClassClouds = true;
                    
                    break;
                case "bst1":
                case "bst":
                    constructor.PrimaryTermWeightFactor = 5;
                    constructor.SecondaryTermWeightFactor = 1;
                    constructor.ReserveTermWeightFactor = 0.1;
                    constructor.doFactorToClassClouds = true;
                    break;
                case "bst2":
                    constructor.PrimaryTermWeightFactor = 10;
                    constructor.SecondaryTermWeightFactor = 2;
                    constructor.ReserveTermWeightFactor = 0.1;
                    constructor.doFactorToClassClouds = true;
                    break;
                case "bst3":
                    constructor.PrimaryTermWeightFactor = 20;
                    constructor.SecondaryTermWeightFactor = 4;
                    constructor.ReserveTermWeightFactor = 0.1;
                    constructor.doFactorToClassClouds = true;
                    break;
                case "lin1":
                case "lin":
                    constructor.PrimaryTermWeightFactor = 10;
                    constructor.SecondaryTermWeightFactor = 5;
                    constructor.ReserveTermWeightFactor = 1;
                    constructor.doFactorToClassClouds = true;
                    break;
                case "lin2":
                    constructor.PrimaryTermWeightFactor = 20;
                    constructor.SecondaryTermWeightFactor = 10;
                    constructor.ReserveTermWeightFactor = 1;
                    constructor.doFactorToClassClouds = true;
                    break;
                case "lin3":
                    constructor.PrimaryTermWeightFactor = 40;
                    constructor.SecondaryTermWeightFactor = 20;
                    constructor.ReserveTermWeightFactor = 1;
                    constructor.doFactorToClassClouds = true;
                    break;
                case "flt1":
                    constructor.PrimaryTermWeightFactor = 10;
                    constructor.SecondaryTermWeightFactor = 10;
                    constructor.ReserveTermWeightFactor = 1;
                    constructor.doFactorToClassClouds = true;
                    break;
                case "flt2":
                    constructor.PrimaryTermWeightFactor = 20;
                    constructor.SecondaryTermWeightFactor = 20;
                    constructor.ReserveTermWeightFactor = 1;
                    constructor.doFactorToClassClouds = true;
                    break;
                case "flt3":
                    constructor.PrimaryTermWeightFactor = 40;
                    constructor.SecondaryTermWeightFactor = 40;
                    constructor.ReserveTermWeightFactor = 1;
                    constructor.doFactorToClassClouds = true;
                    break;
                case "off":
                    constructor.PrimaryTermWeightFactor = 1;
                    constructor.SecondaryTermWeightFactor = 1;
                    constructor.ReserveTermWeightFactor = 1;
                    constructor.doFactorToClassClouds = false;
                    break;
            }
        }


        /// <summary>
        /// Sets the redux.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="flag">The flag.</param>
        /// <param name="LPF">The LPF.</param>
        public static void SetRedux(this cloudMatrixSettings settings, String flag = "div", Int32 LPF = 2)
        {
            flag = flag.ToLower();

            if (LPF > 0)
            {

                settings.lowPassFilter = LPF;
                settings.doCutOffByCloudFrequency = true;
            }
            switch (flag)
            {
                case "div":
                    settings.doDivideWeightWithCloudFrequency = true;
                    settings.doUseSquareFunctionOfCF = false;
                    settings.isActive = true;
                    break;
                case "sqr":
                    settings.doDivideWeightWithCloudFrequency = false;
                    settings.doUseSquareFunctionOfCF = true;
                    settings.isActive = true;
                    break;
                case "off":
                    settings.doDivideWeightWithCloudFrequency = false;
                    settings.doUseSquareFunctionOfCF = false;
                    
                    settings.isActive = false;
                    break;
            }

        }

     


    }

}