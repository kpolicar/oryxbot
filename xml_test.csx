using System;
using System.Xml.Linq;

var xml = @"<cluster xsi:noNamespaceSchemaLocation=""cluster.xsd"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
</cluster>";
var doc = XDocument.Parse(xml);
Console.WriteLine(doc.Root.Name);
Console.WriteLine(doc.Root.Name == "cluster");
