using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.MetadataEditor.Models.OpenData
{

    public class Metadata
    {
        public string context { get; set; }
        public string id { get; set; }
        public string type { get; set; }
        public string conformsTo { get; set; }
        public string describedBy { get; set; }
        public Dataset[] dataset { get; set; }
    }

    public class Dataset
    {
        public string type { get; set; }
        public string accessLevel { get; set; }
        public string accrualPeriodicity { get; set; }
        public string[] bureauCode { get; set; }
        public string conformsTo { get; set; }
        public Contactpoint contactPoint { get; set; }
        public string describedBy { get; set; }
        public bool dataQuality { get; set; }
        public string description { get; set; }
        public Distribution[] distribution { get; set; }
        public string identifier { get; set; }
        public string issued { get; set; }
        public string[] keyword { get; set; }
        public string landingPage { get; set; }
        public string[] language { get; set; }
        public string license { get; set; }
        public DateTime modified { get; set; }
        public string primaryITInvestmentUII { get; set; }
        public string[] programCode { get; set; }
        public Publisher publisher { get; set; }
        public string[] references { get; set; }
        public string rights { get; set; }
        public string spatial { get; set; }
        public string systemOfRecords { get; set; }
        public string temporal { get; set; }
        public string[] theme { get; set; }
        public string title { get; set; }
        public string isPartOf { get; set; }
    }

    public class Contactpoint
    {
        public string type { get; set; }
        public string fn { get; set; }
        public string hasEmail { get; set; }
    }

    public class Publisher
    {
        public string type { get; set; }
        public string name { get; set; }
        public Suborganizationof subOrganizationOf { get; set; }
    }

    public class Suborganizationof
    {
        public string type { get; set; }
        public string name { get; set; }
        public Suborganizationof1 subOrganizationOf { get; set; }
    }

    public class Suborganizationof1
    {
        public string type { get; set; }
        public string name { get; set; }
        public Suborganizationof2 subOrganizationOf { get; set; }
    }

    public class Suborganizationof2
    {
        public string type { get; set; }
        public string name { get; set; }
    }

    public class Distribution
    {
        public string type { get; set; }
        public string description { get; set; }
        public string downloadURL { get; set; }
        public string format { get; set; }
        public string mediaType { get; set; }
        public string title { get; set; }
        public string conformsTo { get; set; }
        public string describedBy { get; set; }
        public string describedByType { get; set; }
        public string accessURL { get; set; }
    }

}