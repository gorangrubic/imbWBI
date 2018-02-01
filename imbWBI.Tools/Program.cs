// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="imbVeles" >
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
// Project: imbWBI.Tools
// Author: Goran Grubic
// ------------------------------------------------------------------------------------------------------------------
// Project web site: http://blog.veles.rs
// GitHub: http://github.com/gorangrubic
// Mendeley profile: http://www.mendeley.com/profiles/goran-grubi2/
// ORCID ID: http://orcid.org/0000-0003-2673-9471
// Email: hardy@veles.rs
// </summary>
// ------------------------------------------------------------------------------------------------------------------
using imbACE.Services.application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imbWBI.Tools
{
    public class Program:aceConsoleApplication<imbWBIToolConsole>
    {
        public String[] args { get; set; }

        public static void Main(string[] args)
        {
            var app = new Program();
            app.args = args;
            app.onEventApplicationClosing += App_onEventApplicationClosing;
            app.StartApplication(args);

        }

        private static void App_onEventApplicationClosing(object sender, imbACE.Core.events.aceEventGeneralArgs e)
        {
            
        }

        public override void setAboutInformation()
        {
            appAboutInfo = new imbACE.Core.application.aceApplicationInfo
            {
                software = "imbWBI Console tool",
                author = "Goran Grubić",
                license = "GNU General Public Licence v3.0",
                copyright = "Copyright (c) 2017-2018",
                applicationVersion = "v0.3",
                organization = "imbVeles",
                welcomeMessage = "Application provides access to imbWBI (Web Business/Competitive Intelligence), imbWEM (Semantic & Focused Web Crawling) and imbNLP (Natural Language Processing and Ontology auto-construction) plug-ins.",
                comment = "Console tool for command line and ACE script execution of imbWBI experiments and analitics tasks"
            };
        }
    }
}
