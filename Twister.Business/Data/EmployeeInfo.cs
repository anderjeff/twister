using System;
using System.Xml;
using Twister.Business.Shared;
using Twister.Business.Tests;

namespace Twister.Business.Data
{
    /// <summary>
    ///     An IO class for getting employee information.
    /// </summary>
    public class EmployeeInfo
    {
        private const string EMPLOYEE_PATH = "G:\\Programs\\Twister 2015\\Employees.xml";

        public static void LoadOperatorInfo(BenchOperator benchOperator)
        {
            try
            {
                // load xml file
                XmlDocument dom = new XmlDocument();
                dom.Load(EMPLOYEE_PATH);

                // find node by employee number
                XmlNode node = dom.DocumentElement.SelectSingleNode("//employee[@clockId='104']");
                if (node == null)
                {
                    benchOperator = null;
                }
                else
                {
                    // add first and last name properties to the bench operator.

                    XmlNode firstNameNode = node.Attributes.GetNamedItem("firstName");
                    if (firstNameNode != null)
                        benchOperator.FirstName = firstNameNode.Value;
                    XmlNode lastNameNode = node.Attributes.GetNamedItem("lastName");
                    if (lastNameNode != null)
                        benchOperator.LastName = lastNameNode.Value;
                }
            }
            catch (Exception ex)
            {
                Messages.GeneralExceptionMessage(ex, "DataValidator.LoadOperatorInfo()");
            }
        }
    }
}