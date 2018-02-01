// --------------------------------------------------------------------------------------------------------------------
// <copyright file="pipelineTokenLanguageFilterNode.cs" company="imbVeles" >
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
    using imbNLP.PartOfSpeech.pipelineForPos.subject;
    using imbNLP.Data.evaluate;
    using imbNLP.PartOfSpeech.flags.token;
    using imbNLP.Data;


    /// <summary>
    /// Pipeline transformer node
    /// </summary>
    public class pipelineTokenLanguageFilterNode : pipelineNodeRegular<pipelineTaskSubjectContentToken>
    {

        protected basicLanguageEnum languagePrimary { get; set; }
        protected List<basicLanguageEnum> languages { get; set; }


        protected multiLanguageEvaluationTask settings { get; set; }
        protected multiLanguageEvaluator mLanguageEval { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="pipelineTokenLanguageFilterNode"/> class.
        /// </summary>
        public pipelineTokenLanguageFilterNode(multiLanguageEvaluationTask evaluationSettings, List<basicLanguageEnum> __languages, basicLanguageEnum __primLanguage)
        {
            _nodeType = pipelineNodeTypeEnum.distributor;

            languages = __languages;
            languagePrimary = __primLanguage;

            mLanguageEval = new multiLanguageEvaluator();
            mLanguageEval.setup(languages);


            settings = evaluationSettings;
        }

        /// <summary>
        /// Processes the specified task.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns></returns>
        public override IPipelineNode process(IPipelineTask task)
        {

            // <---- tagging code

            pipelineTask<pipelineTaskSubjectContentToken> realTask = task as pipelineTask<pipelineTaskSubjectContentToken>;
            if (realTask == null) return next;

            
            pipelineTaskSubjectContentToken realSubject = realTask.subject;

            if (realSubject.contentLevelType != cnt_level.mcToken) return next;

            if (realSubject.currentForm == "εμάσ")
            {

            }

            var tkns = mLanguageEval.GetAllProperTokensSortedByFrequency(realSubject.currentForm, settings.tokenLengthMin);

            var mle = mLanguageEval.evaluate(settings, tkns);

            
            if (mle.languageEnums.Contains(languagePrimary))
            {
                List<Object> l = new List<object>();
                mle.languageEnums.ForEach(x => l.Add(x));
                realSubject.flagBag.AddRange(l);

                return forward;
            }
            else
            {
                return task.model.trashBin;
            }

            return forward;
        }
    }

}