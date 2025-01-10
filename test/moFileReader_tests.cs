using System;
using System.IO;
using NUnit.Framework;
using static MoFileLib.ConvenienceClasses;

namespace MoFileLib
{
    [TestFixture]
    public class moFileLibTests
    {
        private string MO_TEST_FILE = "languages/nl.mo";

        [Test]
        public void LoadMoFile()
        {
            var moFR = new MoFileReader();
            var res = moFR.ReadFile(MO_TEST_FILE);
            Console.WriteLine(moFR.GetErrorDescription());
            Assert.That(res, Is.EqualTo(MoFileReader.ErrorCode.Success));
        }

        [Test]
        public void LoadBrokenFile()
        {
            var moFR = new MoFileReader();
            Console.WriteLine(moFR.GetErrorDescription());
            Assert.That(moFR.ReadFile("languages/fail.txt"), Is.EqualTo(MoFileReader.ErrorCode.FileInvalid));
        }

        [Test]
        public void LoadNoMoFile()
        {
            var moFR = new MoFileReader();
            Console.WriteLine(moFR.GetErrorDescription());
            Assert.That(moFR.ReadFile("XD"), Is.EqualTo(MoFileReader.ErrorCode.FileNotFound));
        }

        [Test]
        public void CountStrings()
        {
            var moFR = new MoFileReader();
            moFR.ReadFile(MO_TEST_FILE);
            Assert.That(moFR.GetNumStrings(), Is.EqualTo(7));
        }

        [Test]
        public void EmptiesLookupTable()
        {
            var moFR = new MoFileReader();
            moFR.ReadFile(MO_TEST_FILE);
            Assert.Multiple(() =>
            {
                Assert.That(moFR.Lookup("String English One"), Is.EqualTo("Text Nederlands Een"));
                Assert.That(moFR.GetNumStrings(), Is.EqualTo(7));
            });
            moFR.ClearTable();
            Assert.Multiple(() =>
            {
                Assert.That(moFR.Lookup("String English One"), Is.EqualTo("String English One"));
                Assert.That(moFR.GetNumStrings(), Is.EqualTo(0));
            });
        }

        [Test]
        public void LookupString()
        {
            var moFR = new MoFileReader();
            moFR.ReadFile(MO_TEST_FILE);
            Assert.Multiple(() =>
            {
                /* This is the first comment. */
                Assert.That(moFR.Lookup("String English One"), Is.EqualTo("Text Nederlands Een"));
                /* This is the second comment. */
                Assert.That(moFR.Lookup("String English Two"), Is.EqualTo("Text Nederlands Twee"));
                /* This is the third comment.  */
                Assert.That(moFR.Lookup("String English Three"), Is.EqualTo("Text Nederlands Drie"));
            });
        }

        [Test]
        public void LookupStringWithContext()
        {
            var moFR = new MoFileReader();
            moFR.ReadFile(MO_TEST_FILE);
            Assert.Multiple(() =>
            {
                /* This is the first comment. */
                Assert.That(moFR.LookupWithContext("TEST|String|1", "String English"), Is.EqualTo("Text Nederlands Een"));
                /* This is the second comment. */
                Assert.That(moFR.LookupWithContext("TEST|String|2", "String English"), Is.EqualTo("Text Nederlands Twee"));
                /* This is the third comment.  */
                Assert.That(moFR.LookupWithContext("TEST|String|3", "String English"), Is.EqualTo("Text Nederlands Drie"));
            });
        }

        [Test]
        public void LookupNotExistingStrings()
        {
            var moFR = new MoFileReader();
            moFR.ReadFile(MO_TEST_FILE);
            Assert.Multiple(() =>
            {
                Assert.That(moFR.Lookup("No match"), Is.EqualTo("No match"));
                Assert.That(moFR.Lookup("Can't touch this"), Is.EqualTo("Can't touch this"));
            });
        }

        [Test]
        public void LookupNotExistingStringsWithContext()
        {
            var moFR = new MoFileReader();
            moFR.ReadFile(MO_TEST_FILE);
            Assert.Multiple(() =>
            {
                Assert.That(moFR.LookupWithContext("Nope", "String English"), Is.EqualTo("String English"));
                Assert.That(moFR.LookupWithContext("TEST|String|1", "Not this one"), Is.EqualTo("Not this one"));
            });
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
            Assert.That(res, Is.EqualTo(MoFileReader.ErrorCode.Success));
        }

        [Test]
        public void CountStrings()
        {
            MoReadMoFile(MO_TEST_FILE);
            Assert.That(MoFileGetNumStrings(), Is.EqualTo(7));
        }

        [Test]
        public void LookupString()
        {
            MoReadMoFile(MO_TEST_FILE);
            Assert.Multiple(() =>
            {
                /* This is the first comment. */
                Assert.That(_("String English One"), Is.EqualTo("Text Nederlands Een"));
                /* This is the second comment. */
                Assert.That(_("String English Two"), Is.EqualTo("Text Nederlands Twee"));
                /* This is the third comment.  */
                Assert.That(_("String English Three"), Is.EqualTo("Text Nederlands Drie"));
            });
        }
    }
}