// --------------------------------------------------------------------------------------------------------------------
// <copyright file="experimentTemplateManager.cs" company="imbVeles" >
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
using imbSCI.Core.files;

namespace imbWBI.Core.WebClassifier.experiment
{

    public class experimentTemplateManager
    {
        public const String EXPERIMENT_TEMPLATE_FILENAME_PREFIX = "ext_";
        public const String EXPERIMENT_TEMPLATE_FILENAME_EXT = "xml";

        public experimentTemplateManager()
        {

        }

        public experimentTemplate MakeTemplate([Description("Name of the experiment to process")] experimentSetup experiment,
            [Description("Starting value for Stx")] Int32 start = 3,
            [Description("Ending value for Sts")] Int32 end = 8,
            [Description("3 or 4 letter code indicating how the settings are different then in other experiments")] String name = "",
            [Description("Comment line for experiment header")] String comment = "",
            experimentTemplateOption option = experimentTemplateOption.STX)
        {
            experimentTemplate output = new experimentTemplate();
            output.DeriveBlueprint(experiment, name);
            output.comment = comment;

            semanticFVExtractor model = experiment.models.First() as semanticFVExtractor;
            experiment.RemoveAllModelsExcept();

            String currentValue = "";
            String xmlElementName = "";

            switch (option)
            {
                case experimentTemplateOption.LPF:
                    currentValue = model.settings.semanticCloudFilter.lowPassFilter.ToString();
                    xmlElementName = "lowPassFilter";
                    break;
                case experimentTemplateOption.none:
                    break;
                case experimentTemplateOption.REPEAT:
                    break;
                case experimentTemplateOption.STX:
                    currentValue = model.settings.caseTermExpansionSteps.ToString();
                    xmlElementName = "caseTermExpansionSteps";
                    break;
                case experimentTemplateOption.TC:
                    break;
            }

            for (int i = start; i < end; i++)
            {
                var exp = new experimentTemplateVariable();
                exp.needle = "<" + xmlElementName + ">" + currentValue + "</" + xmlElementName + ">";
                exp.replace = "<" + xmlElementName + ">" + i + "</" + xmlElementName + ">";
                exp.i = i;
                output.replacements.Add(exp);
            }




            return output;
           


        }

        public void LoadAll(folderNode _folder)
        {

        }

        public void SaveAll(folderNode _folder=null)
        {
            if (_folder != null) folder = _folder;

        }

        public folderNode folder { get; set; }

        public List<experimentTemplate> templates { get; set; } = new List<experimentTemplate>();
    }

}