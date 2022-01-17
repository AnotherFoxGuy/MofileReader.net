using System;
using System.IO;
using NUnit.Framework;
using static moFileLib.ConvenienceClasses;
using static CmakeData;

namespace moFileLib
{
    [TestFixture]
    public class moFileLibTests
    {
        private string MO_TEST_FILE = $"{DataDir}/nl.mo";

        [Test]
        public void LoadMoFile()
        {
            var moFR = new MoFileReader();
            var res = moFR.ReadFile(MO_TEST_FILE);
            Assert.AreEqual(MoFileReader.ErrorCode.Success, res);
        }

        [Test]
        public void LoadBrokenFile()
        {
            var moFR = new MoFileReader();
            Console.WriteLine(moFR.GetErrorDescription());
            Assert.AreEqual(MoFileReader.ErrorCode.FileInvalid, moFR.ReadFile("languages/fail.txt"));
        }

        [Test]
        public void LoadNoMoFile()
        {
            var moFR = new MoFileReader();
            Console.WriteLine(moFR.GetErrorDescription());
            Assert.AreEqual(MoFileReader.ErrorCode.FileNotFound, moFR.ReadFile("XD"));
        }
        
        [Test]
        public void CountStrings()
        {
            var moFR = new MoFileReader();
            moFR.ReadFile(MO_TEST_FILE);
            Assert.AreEqual(7, moFR.GetNumStrings());
        }

        [Test]
        public void EmptiesLookupTable()
        {
            var moFR = new MoFileReader();
            moFR.ReadFile(MO_TEST_FILE);
            Assert.AreEqual("Text Nederlands Een", moFR.Lookup("String English One"));
            Assert.AreEqual(7, moFR.GetNumStrings());
            moFR.ClearTable();
            Assert.AreEqual("String English One", moFR.Lookup("String English One"));
            Assert.AreEqual(0, moFR.GetNumStrings());
        }

        [Test]
        public void LookupString()
        {
            var moFR = new MoFileReader();
            moFR.ReadFile(MO_TEST_FILE);
            /* This is the first comment. */
            Assert.AreEqual("Text Nederlands Een", moFR.Lookup("String English One"));
            /* This is the second comment. */
            Assert.AreEqual("Text Nederlands Twee", moFR.Lookup("String English Two"));
            /* This is the third comment.  */
            Assert.AreEqual("Text Nederlands Drie", moFR.Lookup("String English Three"));
        }

        [Test]
        public void LookupStringWithContext()
        {
            var moFR = new MoFileReader();
            moFR.ReadFile(MO_TEST_FILE);
            /* This is the first comment. */
            Assert.AreEqual("Text Nederlands Een", moFR.LookupWithContext("TEST|String|1", "String English"));
            /* This is the second comment. */
            Assert.AreEqual("Text Nederlands Twee", moFR.LookupWithContext("TEST|String|2", "String English"));
            /* This is the third comment.  */
            Assert.AreEqual("Text Nederlands Drie", moFR.LookupWithContext("TEST|String|3", "String English"));
        }

        [Test]
        public void LookupNotExistingStrings()
        {
            var moFR = new MoFileReader();
            moFR.ReadFile(MO_TEST_FILE);
            Assert.AreEqual("No match", moFR.Lookup("No match"));
            Assert.AreEqual("Can't touch this", moFR.Lookup("Can't touch this"));
        }

        [Test]
        public void LookupNotExistingStringsWithContext()
        {
            var moFR = new MoFileReader();
            moFR.ReadFile(MO_TEST_FILE);
            Assert.AreEqual("String English", moFR.LookupWithContext("Nope", "String English"));
            Assert.AreEqual("Not this one", moFR.LookupWithContext("TEST|String|1", "Not this one"));
        }
    }

    [TestFixture]
    public class ConvenienceClassesTests
    {
        private string MO_TEST_FILE = "languages/nl.mo";
        
        [Test]
        public void LoadMoFile()
        {
            var res = MoReadMoFile(MO_TEST_FILE);
            Assert.AreEqual(MoFileReader.ErrorCode.Success, res);
        }
        
        [Test]
        public void CountStrings()
        {
            MoReadMoFile(MO_TEST_FILE);
            Assert.AreEqual(7, MoFileGetNumStrings());
        }

        [Test]
        public void LookupString()
        {
            MoReadMoFile(MO_TEST_FILE);
            /* This is the first comment. */
            Assert.AreEqual("Text Nederlands Een", _("String English One"));
            /* This is the second comment. */
            Assert.AreEqual("Text Nederlands Twee", _("String English Two"));
            /* This is the third comment.  */
            Assert.AreEqual("Text Nederlands Drie", _("String English Three"));
        }
    }
}