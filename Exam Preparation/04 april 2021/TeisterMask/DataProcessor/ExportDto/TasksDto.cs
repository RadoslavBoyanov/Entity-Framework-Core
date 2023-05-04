﻿using System.Xml.Serialization;

namespace TeisterMask.DataProcessor.ExportDto
{
    [XmlType("Task")]
    public class TasksDto
    {
        [XmlElement("Name")]
        public string Name { get; set; }
        [XmlElement("Label")]
        public string Label { get; set; }
    }
}