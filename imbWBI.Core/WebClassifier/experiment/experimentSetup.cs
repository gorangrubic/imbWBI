// --------------------------------------------------------------------------------------------------------------------
// <copyright file="experimentSetup.cs" company="imbVeles" >
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
    /// Project class for industryTermModel
    /// </summary>
    public class experimentSetup
    {
        [XmlAttribute]
        public String derivedFrom { get; set; } = "";


        public experimentSetup()
        {

        }

        public void deploy()
        {
            foreach (IWebFVExtractor model in models)
            {
                model.BuildFeatureVectorDefinition();
            }
        }

        public static experimentSetup GetDefaultExperimentSetup()
        {
            experimentSetup setup = new experimentSetup();
           

            setup.classifiers_settings.Add(new WebPostClassifierSettings(WebPostClassifierType.kNearestNeighbors, "kNN"));
            setup.classifiers_settings.Add(new WebPostClassifierSettings(WebPostClassifierType.multiClassSVM, "mSVM"));
            setup.classifiers_settings.Add(new WebPostClassifierSettings(WebPostClassifierType.naiveBayes, "nBayes"));
            setup.classifiers_settings.Add(new WebPostClassifierSettings(WebPostClassifierType.backPropagationActivationNeuralNetwork, "bpANN"));

            //= new List<pos_type> { pos_type.A, pos_type.N };
            var tfe = new semanticFVExtractor();
            tfe.termTableConstructor.settings.allowedLemmaTypes.AddUnique(pos_type.A);
            tfe.termTableConstructor.settings.allowedLemmaTypes.AddUnique(pos_type.N);


            var sfe = new semanticFVExtractor();
            sfe.termTableConstructor.settings.allowedLemmaTypes.AddUnique(pos_type.A);
            sfe.termTableConstructor.settings.allowedLemmaTypes.AddUnique(pos_type.N);

            setup.featureVectorExtractors_semantic.Add(tfe);
            setup.featureVectorExtractors_semantic.Add(sfe);
            
            setup.validationSetup.name = setup.name;
            setup.validationSetup.k = 5;

            setup.setClassifiers();

            return setup;
        }

        /// <summary>
        /// Name of the experiment
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public String name { get; set; } = "Exp01";

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public String description { get; set; } = "Experiment with Web Classification - this is default definition, change this XML to customize the experiment";


        private List<IWebFVExtractor> _models = new List<IWebFVExtractor>();

        /// <summary>
        /// Gets the models.
        /// </summary>
        /// <value>
        /// The models.
        /// </value>
        [XmlIgnore]
        public List<IWebFVExtractor> models
        {
            get
            {
                if (!_models.Any())
                {
                    featureVectorExtractors_semantic.ForEach(x => _models.Add(x as IWebFVExtractor));
                   // featureVectorExtractors_tfidf.ForEach(x => _models.Add(x as IWebFVExtractor));
                }
                return _models;
            }
        }


        public void RemoveAllModelsExcept(IWebFVExtractor model=null)
        {
            featureVectorExtractors_semantic.Clear();
            models.Clear();
            if (model != null) models.Add(model);
        }



        /// <summary> If true it will use the same knowledge object for all FVEs in the test. If will reduce experiment execution time but must be used carefully - experiment must not contain FVEs that describe cases on different way. </summary>
        [Category("Switch")]
        [DisplayName("doShareTheCaseKnowledgeAmongFVEModels")]
        [Description("If true it will use the same knowledge object for all FVEs in the test. If will reduce experiment execution time but must be used carefully - experiment must not contain FVEs that describe cases on different way.")]
        public Boolean doShareTheCaseKnowledgeAmongFVEModels { get; set; } = true;






        private Object setClassiferLock = new Object();


        /// <summary>
        /// Sets the classifiers.
        /// </summary>
        public void setClassifiers()
        {
            lock (setClassiferLock)
            {
                if (_classifiers == null)
                {
                    _classifiers = new List<IWebPostClassifier>();

                    List<WebPostClassifierSettings> cls = new List<WebPostClassifierSettings>();
                    List<String> names = new List<string>();
                    foreach (WebPostClassifierSettings set in classifiers_settings)
                    {
                        if (!names.Contains(set.name))
                        {
                            names.Add(set.name);
                            cls.Add(set);
                        }
                    }
                    classifiers_settings = cls;

                    foreach (WebPostClassifierSettings set in classifiers_settings)
                    {
                        _classifiers.Add(set.GetClassifier());
                    }
                }
            }
        }

       

        private List<IWebPostClassifier> _classifiers;

        [XmlIgnore]
        public List<IWebPostClassifier> classifiers
        {
            get
            {
                if (_classifiers == null)
                {
                   
                    setClassifiers();
                }
                return _classifiers;
            }

        }

        /// <summary>
        /// Gets or sets the classifiers.
        /// </summary>
        /// <value>
        /// The classifiers.
        /// </value>
        public List<WebPostClassifierSettings> classifiers_settings { get; set; } = new List<WebPostClassifierSettings>();



        /// <summary>
        /// Gets or sets the feature vector extractors tfidf.
        /// </summary>
        /// <value>
        /// The feature vector extractors tfidf.
        /// </value>
      //  public List<tfidfFVExtractor> featureVectorExtractors_tfidf { get; set; } = new List<tfidfFVExtractor>();

        /// <summary>
        /// Gets or sets the feature vector extractors semantic.
        /// </summary>
        /// <value>
        /// The feature vector extractors semantic.
        /// </value>
        public List<semanticFVExtractor> featureVectorExtractors_semantic { get; set; } = new List<semanticFVExtractor>();

     

        /// <summary>
        /// Gets or sets the validation setup.
        /// </summary>
        /// <value>
        /// The validation setup.
        /// </value>
        public kFoldValidationSetup validationSetup { get; set; } = new kFoldValidationSetup();


    }

}