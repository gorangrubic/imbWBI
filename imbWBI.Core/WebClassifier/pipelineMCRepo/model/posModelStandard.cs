// --------------------------------------------------------------------------------------------------------------------
// <copyright file="posModelStandard.cs" company="imbVeles" >
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
using imbNLP.PartOfSpeech.pipeline.core;
using imbNLP.PartOfSpeech.pipeline.machine;
using imbNLP.PartOfSpeech.pipeline.models;
using imbNLP.PartOfSpeech.pipelineForPos.node;
using imbNLP.PartOfSpeech.pipelineForPos.subject;
using imbSCI.Core.extensions.data;
using imbSCI.DataComplex.tf_idf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace imbWBI.Core.WebClassifier.pipelineMCRepo.model
{
    /// <summary>
    /// Simple demonstrative model for content token processing
    /// </summary>
    /// <seealso cref="imbNLP.PartOfSpeech.pipeline.core.pipelineModelForContentToken" />
    public class posModelStandard : pipelineModel<pipelineTaskSubjectContentToken>
    {

        public mcRepoProcessModelSetup setup { get; set; } = new mcRepoProcessModelSetup();

        /// <summary>
        /// Self constructed standard model
        /// </summary>
        public posModelStandard() : base()
        {
            description = "Standard NLP token processing pipeline model for dealing with Serbian web content";
        }




        public override void constructionProcess()
        {
            AddNode(new pipelineProperFormTransformer(setup.setup_tableOne));
            //AddNode(new pipelineLexicResourceResolverNode(setup.setup_multitext_lex, setup.setup_multitext_specs));
            

          //  AddNode(new pipelineTransliterationNode("sr.txt", false)).AddNode(new pipelineProperFormTransformer("sr_properform_one.xlsx"), false);
        }

        public List<IPipelineTask> createPrimaryTasks(TFDFContainer[] resources) => createPrimaryTasks(resources);



        /// <summary>
        /// It will be called by <see cref="!:pipelineMachine.run(IPipelineModel)" /> method to get initial tasks to run
        /// </summary>
        /// <param name="resources">Arbitrary resources that might be used for task creation</param>
        /// <returns></returns>
        public override List<IPipelineTask> createPrimaryTasks(Object[] resources)
        {
            List<IPipelineTask> output = new List<IPipelineTask>();

            List<pipelineTask<pipelineTaskTFDFContentSubject>> tsks = resources.getFirstOfType<List<pipelineTask<pipelineTaskTFDFContentSubject>>>(false, output, false);

            output.AddRange(tsks);

            return output;
        }
    }
}
