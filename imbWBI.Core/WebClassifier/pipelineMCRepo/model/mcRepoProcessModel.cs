// --------------------------------------------------------------------------------------------------------------------
// <copyright file="mcRepoProcessModel.cs" company="imbVeles" >
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
using imbNLP.PartOfSpeech.decomposing.block;
using imbNLP.PartOfSpeech.decomposing.stream;
using imbNLP.PartOfSpeech.decomposing.token;
using imbNLP.PartOfSpeech.flags.token;
using imbNLP.PartOfSpeech.pipeline.core;
using imbNLP.PartOfSpeech.pipeline.machine;
using imbNLP.PartOfSpeech.pipeline.mcRepoNodes;
using imbNLP.PartOfSpeech.pipeline.models;
using imbNLP.PartOfSpeech.pipelineForPos.node;
using imbNLP.PartOfSpeech.pipelineForPos.subject;
using imbSCI.Core.extensions.data;
using imbSCI.Data.data.sample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using imbWBI.Core.WebClassifier.pipelineMCRepo;
using imbWBI.Core.WebClassifier.cases;

namespace imbWBI.Core.WebClassifier.pipelineMCRepo.model
{


    /// <summary>
    /// Simple demonstrative model for content token processing
    /// </summary>
    /// <seealso cref="imbNLP.PartOfSpeech.pipeline.core.pipelineModelForContentToken" />
    public class mcRepoProcessModel : pipelineModel<pipelineTaskSubjectContentToken>, IMCRepoProcessModel<mcRepoProcessModelSetup>
    {

        /// <summary>
        /// It will be called by <see cref="M:imbNLP.PartOfSpeech.pipeline.machine.pipelineMachine.run(imbNLP.PartOfSpeech.pipeline.core.IPipelineModel)" /> method to get initial tasks to run
        /// </summary>
        /// <param name="resources">Arbitrary resources that might be used for task creation</param>
        /// <returns></returns>
        public override List<IPipelineTask> createPrimaryTasks(object[] resources)
        {
            String repoName = resources.getFirstOfType<String>();
            List<String> targetNames = resources.getFirstOfType<List<String>>();

            pipelineTaskMCRepoSubject subject = new pipelineTaskMCRepoSubject();
            subject.MCRepoName = repoName;
            var tmp = resources.getFirstOfType<IDocumentSetClass>(false, null, true);
            
            if (tmp != null)
            {
                subject.WebSiteSample.AddRange(tmp.WebSiteSample);
                subject.MCSiteTargets.AddRange(tmp.WebSiteSample);
            } else
            {
                
            }
            

            pipelineTask<pipelineTaskMCRepoSubject> realTask = new pipelineTask<pipelineTaskMCRepoSubject>(subject);

            List<IPipelineTask> output = new List<IPipelineTask>();

            output.Add(realTask);
            return output;
        }


        /// <summary>
        /// Gets or sets the setup.
        /// </summary>
        /// <value>
        /// The setup.
        /// </value>
        public mcRepoProcessModelSetup setup { get; set; } = new mcRepoProcessModelSetup();




        /// <summary>
        /// Self constructed standard model
        /// </summary>
        public mcRepoProcessModel() : base()
        {
            description = "Basic model for MC Repo processing pipeline";



        }




        public pipelineContentTokenLevelDistribution mainDistributionNode { get; set; }




        public pipelineFlagRuleDistributor<pipelineTaskSubjectContentToken> numericTokenNode { get; set; }

        public pipelineFlagRuleDistributor<pipelineTaskSubjectContentToken> cleanwordsTokenNode { get; set; }

        public pipelineFlagRuleDistributor<pipelineTaskSubjectContentToken> mixedcharsTokenNode { get; set; }




        public override void constructionProcess()
        {

            logger.log("Pipeline model self construction process started");

            mainDistributionNode = AddNode(new pipelineContentTokenLevelDistribution()) as pipelineContentTokenLevelDistribution;

            mainDistributionNode.repoPipeline = AddStealthLink(new pipelineRepoTaskBuilderNode(setup.webSiteSampling));

            mainDistributionNode.sitePipeline = AddStealthLink(new pipelineSiteTaskBuilderNode(setup.webPageSampling));
                        
            // <----------------- [ 

            mainDistributionNode.pagePipeline = AddStealthLink(new pipelinePageLanguageFilterNode(setup.pageFilterSettings, setup.pageFilterSettings.testLanguages, setup.primaryLanguage, setup.target_languagePagePerSite));


            mainDistributionNode.pagePipeline.AddNode(new pipelinePageTaskBuilderNode(new blockComposerSM()));

            // <----------------- pipelineTaskSubjectContentToken 

            mainDistributionNode.blockPipeline = AddStealthLink(new pipelineBlockTaskBuilderNode(new streamComposerBasic()));

            // < ---------------- stream pipeline starts

            logger.log("Repo to block pipelines done, starting with stream pipeline");

            mainDistributionNode.streamPipeline = AddStealthLink(new pipelineTransliterationNode(setup.setup_translit, false));

            mainDistributionNode.streamPipeline.AddNode(new pipelineProperFormTransformer(setup.setup_tableOne));

            mainDistributionNode.streamPipeline.AddNode(new pipelineTokenGenericAnnotation());


            // < --------------- ovde treba obezbediti makro content mehanizam sa maskiranjem vec izdvojenog sadrzaja
            // ------------ za izdvajanje> brojeva telefona, email adrese 


            mainDistributionNode.streamPipeline.AddNode(new pipelineStreamTaskBuilderNode(new tokenComposerBasic()));

            // < ---------------- stram pipeline ends

            logger.log("Starting with token pipeline");

            // < -------------------------------------------- here 
            mainDistributionNode.tokenPipeline = AddStealthLink(new pipelineTokenGenericAnnotation());

            mainDistributionNode.tokenPipeline.AddNode(new pipelineTableAnnotationNode(setup.setup_tableTagger));
            

            numericTokenNode = new pipelineFlagRuleDistributor<pipelineTaskSubjectContentToken>(
                containsQueryTypeEnum.ContainsAny, 
                new object[] {
                    tkn_numeric.numberInFormat,
                });

            mainDistributionNode.tokenPipeline.AddNode(numericTokenNode);



            cleanwordsTokenNode = new pipelineFlagRuleDistributor<pipelineTaskSubjectContentToken>(containsQueryTypeEnum.ContainsAny,
                new object[]
                {
                    tkn_contains.onlyLetters
                });
            
            mainDistributionNode.tokenPipeline.AddNode(cleanwordsTokenNode);


            mainDistributionNode.tokenPipeline.AddNode(new pipelineHTMLTagDetection());



            // mainDistributionNode.tokenPipeline.AddNode(new pipelineTokenLanguageFilterNode(setup.pageFilterSettings, setup.pageFilterSettings.testLanguages, setup.primaryLanguage));

            //            cleanwordsTokenNode.AddNode(new pipelineTokenLanguageFilterNode(setup.pageFilterSettings, setup.pageFilterSettings.testLanguages, setup.primaryLanguage));


            //cleanwordsTokenNode.AddNode(new pipelineLexicResourceResolverNode(setup.setup_multitext_lex, setup.setup_multitext_specs));


            logger.log("---- mcRepoProcessModel building process finished ----");

            
            
        }

       
    }
}
