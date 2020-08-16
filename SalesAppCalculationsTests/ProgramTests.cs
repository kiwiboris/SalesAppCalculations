using Microsoft.VisualStudio.TestTools.UnitTesting;
using SalesAppCalculations;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SalesAppCalculations.Tests
{
    [TestClass()]
    public class ProgramTests
    {
        public const string INPUT_PATH_KEY = "InputPath";
        public const string PRODUCT_CATALOG = "ProductCatalog";

        internal XmlDocument xmlDoc;

        [TestInitialize()]
        public void Setup()
        {
            // Runs before each test. (Optional)
            xmlDoc = new XmlDocument();
            string xmlPath = ConfigurationManager.AppSettings[PRODUCT_CATALOG];
            xmlDoc.Load(xmlPath);
        }

        [TestMethod()]
        public void IsInputLineValidTest()
        {
            string inputLine = "1 imported box of chocolates at 10.00";
            Assert.IsTrue(Program.IsInputLineValid(inputLine));
        }

        [TestMethod()]
        public void IsInputLineValidTest2()
        {
            string inputLine = "www 1 imported box of chocolates at 10.00";
            Assert.IsFalse(Program.IsInputLineValid(inputLine));
        }

        [TestMethod()]
        public void IsInputLineValidTest3()
        {
            string inputLine = "1 imported box of chocolates 10.00";
            Assert.IsFalse(Program.IsInputLineValid(inputLine));
        }

        [TestMethod()]
        public void CalculateAmountTest()
        {
            int n = 1;
            string description = "Chocolate bar";
            double amount = .85;


            double totalAmount = Program.CalculateAmount(n, description, amount, xmlDoc);
            Assert.AreEqual(totalAmount, .85);
        }

        [TestMethod()]
        public void CalculateAmountTest2()
        {
            int n = 1;
            string description = "imported bottle of perfume";
            double amount = 27.99;

            double totalAmount = Program.CalculateAmount(n, description, amount, xmlDoc);
            Assert.AreEqual(totalAmount, 32.19);
        }

        [TestMethod()]
        public void CalculateAmountTest3()
        {
            int n = 1;
            string description = "bottle of perfume";
            double amount = 18.99;

            double totalAmount = Program.CalculateAmount(n, description, amount, xmlDoc);
            Assert.AreEqual(totalAmount, 20.89);
        }

        [TestMethod()]
        public void IsValidXMLTest()
        {
            string xmlPath = ConfigurationManager.AppSettings[PRODUCT_CATALOG];
            Assert.IsTrue(!string.IsNullOrEmpty(xmlPath));

            XmlDocument doc = new XmlDocument();

            try
            {
                doc.Load(xmlPath);
                Assert.IsNotNull(doc);
            }
            catch (System.Xml.XmlException)
            {
                Assert.Fail("Can't load XML");
            }
        }

        [TestMethod()]
        public void GetOutputLineTest()
        {
            string inputLine = "1 imported bottle of perfume at 47.50";
            string outputLine = Program.GetOutputLine(inputLine, xmlDoc, out double salesTax, out double transactionAmount);
            Assert.AreEqual(outputLine, "1 imported bottle of perfume: 54.62");
            Assert.AreEqual(salesTax, 7.12);
            Assert.AreEqual(transactionAmount, 54.62);
        }

        [TestMethod()]
        public void GetOutputLineTest2()
        {
            string inputLine = "1 packet of headache pills at 9.75";
            string outputLine = Program.GetOutputLine(inputLine, xmlDoc, out double salesTax, out double transactionAmount);
            Assert.AreEqual(outputLine, "1 packet of headache pills: 9.75");
            Assert.AreEqual(salesTax, 0);
            Assert.AreEqual(transactionAmount, 9.75);
        }

        [TestMethod()]
        public void GetOutputLineTest3()
        {
            string inputLine = "1 box of imported chocolates at 11.25";
            string outputLine = Program.GetOutputLine(inputLine, xmlDoc, out double salesTax, out double transactionAmount);
            Assert.AreEqual(outputLine, "1 box of imported chocolates: 11.81");
            Assert.AreEqual(salesTax, 0.56);
            Assert.AreEqual(transactionAmount, 11.81);
        }

        [TestMethod()]
        public void CalculateNewAmountTest()
        {
            string description = "imported box of chocolates";
            double amount = 11.25;
            Assert.AreEqual(Program.CalculateAmount(description, amount, "Food"), 11.81);
        }

        [TestMethod()]
        public void GenerateOutputLinesTest()
        {
            string inputPath = ConfigurationManager.AppSettings[INPUT_PATH_KEY];
            string xmlPath = ConfigurationManager.AppSettings[PRODUCT_CATALOG];
            List<string> lines = Program.GenerateOutputLines(inputPath, xmlPath);
            Assert.AreEqual(lines[lines.Count - 1], "Sales Taxes: 6.66 Total: 74.64");
        }
    }
}