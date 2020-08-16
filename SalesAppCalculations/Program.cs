using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace SalesAppCalculations
{
    public class Program
    {
        #region Enum
        public enum Categories { Books, Medical, Food, Other };
        #endregion
        #region Constants
        public const string DATA_XPATH_PRODUCT = "/ProductCatalog/Product";
        public const string DATA_NAME = "Name";
        public const string DATA_CATEGORY = "Category";
        public const string INPUT_PATH_KEY = "InputPath";
        public const string PRODUCT_CATALOG = "ProductCatalog";
        #endregion
        static void Main(string[] args)
        {

            string inputPath = ConfigurationManager.AppSettings[INPUT_PATH_KEY];
            if (string.IsNullOrEmpty(inputPath))
            {
                Console.WriteLine("ERROR: path not found!");
            }
            else
            {
                string xmlPath = ConfigurationManager.AppSettings[PRODUCT_CATALOG];
                if (string.IsNullOrEmpty(xmlPath))
                {
                    Console.WriteLine("ERROR: product catalog not found!");
                }
                else
                {
                    if (!IsValidXML(xmlPath))
                    {
                        Console.WriteLine("ERROR: invalid XML for product catalog!");
                    }
                    else
                    {
                        Process(inputPath, xmlPath);
                    }
                }
            }
            Console.ReadLine();
        }

        public static void Process(string inputPath, string xmlPath)
        {
            try
            {
                List<string> outputLines = GenerateOutputLines(inputPath, xmlPath);

                if (outputLines != null)
                {

                    OutputResults(outputLines);
                }
                else
                {
                    Console.WriteLine("Cannot output results as there was at least one error in the input file!");
                }
            }
            catch (FileNotFoundException fe)
            {
                Console.WriteLine("ERROR: " + fe.Message);
            }
        }

        public static List<string> GenerateOutputLines(string inputPath, string xmlPath)
        {
            bool isOK = true;

            List<string> outputLines = new List<string>();

            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.Load(xmlPath);

            try
            {
                GenerateOutputLines(inputPath, outputLines, xmlDoc);
            }
            catch (FileNotFoundException fe)
            {
                Console.WriteLine("ERROR: " + fe.Message);
                isOK = false;
            }

            return (isOK ? outputLines : null);
        }

        private static void GenerateOutputLines(string inputPath, List<string> outputLines, XmlDocument xmlDoc)
        {
            double totalTax = 0;
            double totalAmount = 0;

            using (StreamReader sr = File.OpenText(inputPath))
            {
                while (!sr.EndOfStream)
                {
                    string inputLine = sr.ReadLine();

                    if (!IsInputLineValid(inputLine))
                    {
                        Console.WriteLine("ERROR: invalid line: " + inputLine);
                    }
                    else
                    {
                        outputLines.Add(GetOutputLine(inputLine, xmlDoc, out double partialTax, out double partialAmount));

                        totalTax += partialTax;
                        totalAmount += partialAmount;
                    }
                }
            }
            outputLines.Add(string.Format("Sales Taxes: {0:0.00} Total: {1:0.00}", totalTax, totalAmount));
        }

        public static void OutputResults(List<string> outputLines)
        {
            foreach (string line in outputLines)
            {
                Console.WriteLine(line);
            }
        }

        public static bool IsInputLineValid(string line)
        {
            Regex rx = new Regex(@"^(\d)+\s(\w|\s)+\sat\s(\d)*.(\d\d)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            MatchCollection matches = rx.Matches(line);
            return matches.Count > 0;
        }

        public static string GetOutputLine(string inputLine, XmlDocument xmlDoc, out double salesTax, out double transactionAmount)
        {
            // The input line has been validated. Now, let's parse it.
            char[] delimiterChars = { ' ', '\t' };
            string[] parts = inputLine.Split(delimiterChars);
            int len = parts.Length;
            double amount = Convert.ToDouble(parts[len - 1]);
            StringBuilder sb = new StringBuilder(parts[0]);
            StringBuilder nsb = new StringBuilder();

            for (int i = 1; i < len - 2; i++)
            {
                sb.Append(" ");
                sb.Append(parts[i]);
                nsb.Append(parts[i]);
                if (i < len - 3)
                {
                    nsb.Append(" ");
                }
            }

            int n = Convert.ToInt32(parts[0]);
            string description = nsb.ToString();

            transactionAmount = CalculateAmount(n, description, amount, xmlDoc);
            sb.Append(string.Format(": {0:0.00}", transactionAmount));

            salesTax = Math.Round(transactionAmount - n * amount, 2);

            return sb.ToString();
        }

        public static double CalculateAmount(int n, string description, double amount, XmlDocument xmlDoc)
        {
            double newAmount = n * amount;
            bool found = false;

            XmlNodeList xmlNodeList = xmlDoc.SelectNodes(DATA_XPATH_PRODUCT);
            foreach (XmlNode xmlNode in xmlNodeList)
            {
                string productName = xmlNode[DATA_NAME].InnerText;
                string category = xmlNode[DATA_CATEGORY].InnerText;

                if (String.Compare(description, productName, true) == 0)
                {
                    newAmount = CalculateAmount(description, newAmount, category);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                // The product is not in the catalog. If it is imported, though, we need to make sure it is taxed accordingly.
                if (description.ToLower().Contains("imported"))
                {
                    newAmount *= 1.05;
                    newAmount = Math.Round(newAmount, 2);
                }
            }
            return newAmount;
        }

        public static double CalculateAmount(string description, double newAmount, string category)
        {
            bool isImported = description.ToLower().Contains("imported");

            if (!string.IsNullOrEmpty(category) &&
                String.Compare(category, Categories.Books.ToString(), true) != 0 &&
                String.Compare(category, Categories.Medical.ToString(), true) != 0 &&
                String.Compare(category, Categories.Food.ToString(), true) != 0)
            {
                newAmount *= (isImported ? 1.15 : 1.1);
            }
            else
            {
                newAmount *= (isImported ? 1.05 : 1);
            }

            return Math.Round(newAmount,2);
        }

        public static bool IsValidXML(string xmlPath)
        {
            try
            {
                // Check we actually have a value
                if (string.IsNullOrEmpty(xmlPath) == false)
                {
                    // Try to load the value into a document
                    XmlDocument xmlDoc = new XmlDocument();

                    xmlDoc.Load(xmlPath);

                    // If we managed with no exception then this is valid XML!
                    return true;
                }
                else
                {
                    // A blank value is not valid xml
                    return false;
                }
            }
            catch (System.Xml.XmlException)
            {
                return false;
            }
        }
    }
}
