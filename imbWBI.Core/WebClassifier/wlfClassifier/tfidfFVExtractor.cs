// --------------------------------------------------------------------------------------------------------------------
// <copyright file="tfidfFVExtractor.cs" company="imbVeles" >
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
using imbWBI.Core.WebClassifier.cases;
using imbWBI.Core.WebClassifier.core;
using imbWBI.Core.WebClassifier.validation;
using System.Text;
using System.Xml.Serialization;
using imbNLP.PartOfSpeech.TFModels.webLemma.table;

namespace imbWBI.Core.WebClassifier.wlfClassifier
{

    /// <summary>
    /// Classifier
    /// </summary>
    /// <seealso cref="imbWBI.Core.WebClassifier.wlfClassifier.WebFVExtractorBase{imbWBI.Core.WebClassifier.wlfClassifier.tfidfFVExtractorKnowledge}" />
    public class tfidfFVExtractor : WebFVExtractorBase<tfidfFVExtractorKnowledge, tfidfFVExtractorSettings>
    {
        public tfidfFVExtractor()
        {
            name = "TFIDF";
        }


        public wlfConstructorTFIDF constructor { get; set; } = new wlfConstructorTFIDF();



        [XmlIgnore]
        public FeatureVectorDefinition SVMSimilarity { get { return settings.featureVectors["SVM"]; } }



        public override void BuildFeatureVectorDefinition()
        {

            if (SVMSimilarity == null) settings.featureVectors.Add("SVM", "Cosine similarity between DocumentSetClass Web Lemma TF-IDF and DocumentSetCase Web Lemma TF-IDF");

        }


        public override List<string> DescribeSelf()
        {
            List<String> output = new List<string>();

            output.Add("# [" + name + "] Semantic Feature Vector Extractor");

            output.Add(" > Performs Feature Vector Extraction from web sites (DocumentSet) in the targeted MC Repository (collection of DocumentSets)");
            output.Add(" > For each category (DocumentSetClass) a Web Lemma Term (TF-IDF) table is constructed");


            output.AddRange(settings.DescribeSelf());

            output.Add("---");

            //  output.Add("-- Utilizes the following components with corresponding settings --");

            output.AddRange(constructor.DescribeSelf());



            //output.Add(" > Atomic element of the cloud is single word term, in lemma form, having its weight and collection of links.");
            //output.Add(" > Elements of the cloud are related by monotype links, where links may have their own weight other then 1.");




            return output;
        }

        public override tfidfFVExtractorKnowledge DoFVExtractionForClassViaCases(validationCaseCollection vCaseColl, IDocumentSetClass documentSetClass, kFoldValidationCase validationCase, classifierTools tools, ILogBuilder logger)
        {
            throw new NotImplementedException();
        }

        public override void DoFVPostProcessing(kFoldValidationCase validationCase, classifierTools tools, ILogBuilder logger)
        {
            
        }

        public override void DoMakeKnowledge(List<pipelineTaskMCSiteSubject> subjects, classifierTools tools, tfidfFVExtractorKnowledge knowledge, ILogBuilder logger)
        {


            knowledge.WLTableOfIndustryClass.Clear();
            knowledge.WLTableOfIndustryClass = constructor.process(subjects, cnt_level.mcPage, knowledge.WLTableOfIndustryClass, tools.GetLemmaResource(), logger, false);

            logger.log("TF-IDF built for [" + knowledge.name + "]");
        }

        public override WebSiteClassifierResult GetClassification(DocumentSetCase target, DocumentSetCaseCollectionSet caseSet, ILogBuilder logger)
        {
            if (target == null)
            {
                logger.log("-- target is null -- [GetClassification]");
                return null;
            }

            tfidfFVExtractorKnowledge caseKnowledge = target.caseKnowledge as tfidfFVExtractorKnowledge;

            foreach (DocumentSetCaseCollection caseColl in caseSet.Values)
            {
                tfidfFVExtractorKnowledge knowledge = caseColl.classKnowledge as tfidfFVExtractorKnowledge;
                webLemmaTermPairCollection lemmaOverlap = null;

                if (SVMSimilarity.isActive)
                {
                    lemmaOverlap = knowledge.WLTableOfIndustryClass.GetMatchingTerms(caseKnowledge.WLTableOfIndustryClass);
                    target.data.featureVectors[caseColl.setClass.classID][SVMSimilarity] += lemmaOverlap.GetCosineSimilarity(logger);
                }

            }

            //target.result.selected = target.result.GetClassWithHighestScore();

            return target.data.featureVectors;
        }

    }

}