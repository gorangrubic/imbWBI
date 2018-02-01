using System;
using System.IO;
using imbACE.Core.core;
using imbSCI.Core.files;
using imbSCI.Core.files.fileDataStructure;
using imbSCI.Core.files.folders;
using imbSCI.Data;
using imbWBI.Core.WebClassifier.experiment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace imbWBI.TestUnit
{
    [TestClass]
    public class UnitTest1
    {

        [TestMethod]
        public void TestModificationLoad()
        {
            folderNode folder = new folderNode();
            folder = folder.Add("diagnostic", "Diagnostic", "Directory used for testing");
            var logger = new builderForLog();

            String p = folder.pathFor("experimentTest2" + ".xml");

            var test2 = objectSerialization.loadObjectFromXML<experimentSetup>(p, logger);

            
            Assert.AreEqual("MOD", test2.description);
            
        }

        [TestMethod]
        public void TestExperimentSetupLoadSave()
        {
            
            folderNode folder = new folderNode();
            folder = folder.Add("diagnostic", "Diagnostic", "Directory used for testing");
            var logger = new builderForLog();


            var test = experimentSetup.GetDefaultExperimentSetup();
            test.name = "experimentTest";
            test.description = "testing experiment load and save";
            String p = folder.pathFor(test.name + ".xml");
            objectSerialization.saveObjectToXML(test, p);

            var test2 = objectSerialization.loadObjectFromXML<experimentSetup>(p, logger);
            
            Assert.AreEqual(test.name, test2.name);
            Assert.AreEqual(test.description, test2.description);
            Assert.AreEqual(test.featureVectorExtractors_semantic.Count, test2.featureVectorExtractors_semantic.Count);

        }
    }
}
