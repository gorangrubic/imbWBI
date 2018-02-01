// --------------------------------------------------------------------------------------------------------------------
// <copyright file="experimentTemplate.cs" company="imbVeles" >
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

    public class experimentTemplateVariable
    {
        public experimentTemplateVariable()
        {

        }

        //public experimentTemplateVariable(String xmlElementName, String currentValue) 
        [XmlIgnore]
        public String current { get; set; } = "";

        [XmlIgnore]
        public String applied { get; set; } = "";

        public Int32 i { get; set; } = 0;

        public String needle { get; set; } = "";
        public String replace { get; set; } = "";
    }

    public class experimentTemplate
    {
        public String name { get; set; } = "Template01";
        public String description { get; set; } = "Experiment template derived from [ ]";

        public String comment { get; set; }
        
        public List<experimentTemplateVariable> replacements { get; set; } = new List<experimentTemplateVariable>();

        public experimentSetup blueprint { get; set; } = new experimentSetup();

        public String DerivedFromName { get; set; } = "";
        public derivedFromType DerivedFromType { get; set; } = derivedFromType.noOne;

        protected void UpdateDescription()
        {
            description = "Experiment template derived " + DerivedFromType.ToString().imbTitleCamelOperation(false).ToLower() + " [" + DerivedFromName + "]";
            
        }

        public semanticFVExtractor CreateExperiment(experimentSetup model, ILogBuilder output)
        {
            if (model == null) model = blueprint;

            semanticFVExtractor fve = model.models.First() as semanticFVExtractor;

            String serializedModel = objectSerialization.ObjectToXML(model);

            foreach (experimentTemplateVariable variable in replacements)
            {
                String newModelXML = serializedModel;
                serializedModel = serializedModel.Replace(variable.needle, variable.replace);

                semanticFVExtractor newModel = objectSerialization.ObjectFromXML<semanticFVExtractor>(newModelXML);
                newModel.name = newModel.GetShortName(name);
                newModel.description = newModel.GetShortDescription();
                output.log("-- created model: " + newModel.name);
                //experiment.featureVectorExtractors_semantic.Add(newModel);

            }




            //for (INt32 i = start; i < end; i++)
            //{
            //    String newModelXML = serializedModel;
            //    newModelXML = newModelXML.Replace(needle, "<caseTermExpansionSteps>" + i + "</caseTermExpansionSteps>");
                

            //}

            return fve;
        }


        protected void DeployBlueprint(experimentSetup _blueprint)
        {
            blueprint = _blueprint;
            if (blueprint.featureVectorExtractors_semantic.Any())
            {
                blueprint.featureVectorExtractors_semantic.RemoveRange(1, blueprint.featureVectorExtractors_semantic.Count - 1);
            }
        }

        public void DeriveBlueprint(experimentSetup _blueprint, String _name)
        {
            DeployBlueprint(_blueprint);
            DerivedFromName = blueprint.name;
            DerivedFromType = derivedFromType.fromAnExperiment;
            name = blueprint.name.getCleanFilepath();
            UpdateDescription();
            if (_name.isNullOrEmpty()) _name = blueprint.name.getCleanFilepath() + "_child";
            name = _name;
        }

        public void DeriveBlueprint(experimentTemplate _blueprint, String _name)
        {
            DeployBlueprint(_blueprint.blueprint);
            DerivedFromName = blueprint.name;
            DerivedFromType = derivedFromType.fromAnExperimentTemplate;
            UpdateDescription();
            if (_name.isNullOrEmpty()) _name = blueprint.name.getCleanFilepath() + "_child";
        }

        public experimentTemplate()
        {

        }
    }

}