using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.MetadataEditor.Models.InspireCodelist
{

    public class PriorityDataset
    {
        [JsonProperty("metadata-codelist")]
        public MetadataCodelist metadatacodelist { get; set; }
    }

    public class MetadataCodelist
    {
        public string id { get; set; }
        public string thisversion { get; set; }
        public string latestversion { get; set; }
        public string language { get; set; }
        public Label label { get; set; }
        public Definition definition { get; set; }
        public Itemclass itemclass { get; set; }
        public Status status { get; set; }
        public GovernanceLevel governancelevel { get; set; }
        public Register register { get; set; }
        public Containeditem[] containeditems { get; set; }
    }

    public class Label
    {
        public string lang { get; set; }
        public string text { get; set; }
    }

    public class Definition
    {
        public string lang { get; set; }
        public string text { get; set; }
    }

    public class Itemclass
    {
        public string id { get; set; }
        public Label1 label { get; set; }
    }

    public class Label1
    {
        public string lang { get; set; }
        public string text { get; set; }
    }

    public class Status
    {
        public string id { get; set; }
        public Label2 label { get; set; }
    }

    public class Label2
    {
        public string lang { get; set; }
        public string text { get; set; }
    }

    public class GovernanceLevel
    {
        public string id { get; set; }
        public Label3 label { get; set; }
    }

    public class Label3
    {
        public string lang { get; set; }
        public string text { get; set; }
    }

    public class Register
    {
        public string id { get; set; }
        public Label4 label { get; set; }
        public Registry registry { get; set; }
    }

    public class Label4
    {
        public string lang { get; set; }
        public string text { get; set; }
    }

    public class Registry
    {
        public string id { get; set; }
        public Label5 label { get; set; }
    }

    public class Label5
    {
        public string lang { get; set; }
        public string text { get; set; }
    }

    public class Containeditem
    {
        public Value value { get; set; }
    }

    public class Value
    {
        public string id { get; set; }
        public string thisversion { get; set; }
        public string latestversion { get; set; }
        public Label6 label { get; set; }
        public Definition1 definition { get; set; }
        public Itemclass1 itemclass { get; set; }
        public Status1 status { get; set; }
        public Register1 register { get; set; }
        public GovernanceLevel1 governancelevel { get; set; }
        public MetadataCodelist1 metadatacodelist { get; set; }
        public Successor[] successors { get; set; }
        public Historyversion[] historyversion { get; set; }
        public Parent[] parents { get; set; }
        public Predecessor[] predecessors { get; set; }
    }

    public class Label6
    {
        public string lang { get; set; }
        public string text { get; set; }
    }

    public class Definition1
    {
        public string lang { get; set; }
        public string text { get; set; }
    }

    public class Itemclass1
    {
        public string id { get; set; }
        public Label7 label { get; set; }
    }

    public class Label7
    {
        public string lang { get; set; }
        public string text { get; set; }
    }

    public class Status1
    {
        public string id { get; set; }
        public Label8 label { get; set; }
    }

    public class Label8
    {
        public string lang { get; set; }
        public string text { get; set; }
    }

    public class Register1
    {
        public string id { get; set; }
        public Label9 label { get; set; }
        public Registry1 registry { get; set; }
    }

    public class Label9
    {
        public string lang { get; set; }
        public string text { get; set; }
    }

    public class Registry1
    {
        public string id { get; set; }
        public Label10 label { get; set; }
    }

    public class Label10
    {
        public string lang { get; set; }
        public string text { get; set; }
    }

    public class GovernanceLevel1
    {
        public string id { get; set; }
        public Label11 label { get; set; }
    }

    public class Label11
    {
        public string lang { get; set; }
        public string text { get; set; }
    }

    public class MetadataCodelist1
    {
        public string id { get; set; }
        public Label12 label { get; set; }
    }

    public class Label12
    {
        public string lang { get; set; }
        public string text { get; set; }
    }

    public class Successor
    {
        public Successor1 successor { get; set; }
    }

    public class Successor1
    {
        public string id { get; set; }
        public Label13 label { get; set; }
    }

    public class Label13
    {
        public string lang { get; set; }
        public string text { get; set; }
    }

    public class Historyversion
    {
        public string version { get; set; }
    }

    public class Parent
    {
        public Parent1 parent { get; set; }
    }

    public class Parent1
    {
        public string id { get; set; }
        public Label14 label { get; set; }
    }

    public class Label14
    {
        public string lang { get; set; }
        public string text { get; set; }
    }

    public class Predecessor
    {
        public Predecessor1 predecessor { get; set; }
    }

    public class Predecessor1
    {
        public string id { get; set; }
        public Label15 label { get; set; }
    }

    public class Label15
    {
        public string lang { get; set; }
        public string text { get; set; }
    }

}