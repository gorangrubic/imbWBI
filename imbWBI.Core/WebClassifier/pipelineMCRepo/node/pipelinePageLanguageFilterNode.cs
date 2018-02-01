// --------------------------------------------------------------------------------------------------------------------
// <copyright file="pipelinePageLanguageFilterNode.cs" company="imbVeles" >
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
namespace imbWBI.Core.WebClassifier.pipelineMCRepo.model
{
    using System.Text;

    using imbNLP.PartOfSpeech.pipeline.core;
    using imbNLP.PartOfSpeech.pipeline.machine;
    using imbSCI.Core.extensions.data;
    using System.Text.RegularExpressions;
    using imbSCI.Data.collection.graph;
    using imbMiningContext;
    using System.IO;
    using imbACE.Core;
    using imbMiningContext.MCRepository;
    using imbSCI.Core.files.folders;
    using imbSCI.Core.files.fileDataStructure;
    using imbMiningContext.MCWebSite;
    using imbNLP.PartOfSpeech.pipelineForPos.subject;
    using imbNLP.Data.evaluate;
    using imbNLP.Data.basic;
    using imbNLP.Data;


    /// <summary>
    /// Task builder node. If the task is not for it, it will forward it to <see cref="IPipelineNode.next"/>, 
    /// </summary>
    /// <remarks>
    /// <para>if task is processed and new tasks were fed into <see cref="pipelineModelExecutionContext"/> it will forward the processed task to the <see cref="IPipelineNode.forward"/></para>
    /// </remarks>
    /// <seealso cref="imbNLP.PartOfSpeech.pipeline.core.pipelineNodeRegular{imbNLP.PartOfSpeech.pipeline.machine.pipelineTaskSubjectContentToken}" />
    public class pipelinePageLanguageFilterNode : pipelineNodeRegular<pipelineTaskMCRepoSubject>
    {

        protected basicLanguageEnum languagePrimary { get; set; }
        protected List<basicLanguageEnum> languages { get; set; }

        public Int32 limitValidPageCount { get; set; }
        
        protected multiLanguageEvaluationTask settings { get; set; }
        protected multiLanguageEvaluator mLanguageEval { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="pipelinePageLanguageFilterNode" /> class.
        /// </summary>
        /// <param name="evaluationSettings">The evaluation settings.</param>
        /// <param name="__languages">The languages.</param>
        /// <param name="__primLanguage">The prim language.</param>
        /// <param name="limitPageCount">The limit page count - it will only allow positivly evaluated pages to reach specified count.</param>
        public pipelinePageLanguageFilterNode(multiLanguageEvaluationTask evaluationSettings, List<basicLanguageEnum> __languages, basicLanguageEnum __primLanguage, Int32 limitPageCount)
        {
            _nodeType = pipelineNodeTypeEnum.distributor;

            languages = __languages;
            languagePrimary = __primLanguage;

            mLanguageEval = new multiLanguageEvaluator();
            mLanguageEval.setup(languages);

            limitValidPageCount = limitPageCount;
            settings = evaluationSettings;

        }


        


        /// <summary>
        /// Task builder for <see cref="imbMCRepository"/> level of subject. Sends to next if task is not with <see cref="pipelineTaskMCRepoSubject"/>
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns></returns>
        public override IPipelineNode process(IPipelineTask task)
        {
            pipelineTask<pipelineTaskMCPageSubject> realTask = task as pipelineTask<pipelineTaskMCPageSubject>;
            if (realTask == null) return next;

            
            pipelineTaskMCPageSubject realSubject = realTask.subject;
            var tkns = mLanguageEval.GetAllProperTokensSortedByFrequency(realSubject.MCPage.TextContent, settings.tokenLengthMin);

            var mle = mLanguageEval.evaluate(settings, tkns);

            
            if (mle.result_language == languagePrimary)
            {
                Int32 vc = task.context.GetAndChangeCustomDataProperty("validPageCount_" + realSubject.parent.name, 1);
                if (vc > limitValidPageCount)
                {
                    return task.model.trashBin;
                }
                else
                {
                    return forward;
                }
            } else
            {
                return task.model.trashBin;
            }
            
        }
    }

}