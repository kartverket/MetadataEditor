using Arkitektum.GIS.Lib.SerializeUtil;
using GeoNorgeAPI;
using Kartverket.Geonorge.Utilities;
using Kartverket.MetadataEditor.Models.Translations;
using log4net;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Net.PeerToPeer;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Windows.Media.Media3D;
using www.opengis.net;

namespace Kartverket.MetadataEditor.Models.Mets
{
    internal class MetsMetadataService : IMetsMetadataService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IMetadataService _metadataService;
        private GeoNorge _geoNorge;


        public MetsMetadataService(IMetadataService metadataService)
        {
            _metadataService = metadataService;
        }

        public async Task<int> SynchronizeMetadata(string username)
        {
            Log.Info("Synching mets metadata initiated");

            var numberOfUpdatedMetadata = 0;
            
            try
            {
                var updateCount = await SynchronizeMetsMetadata(username).ConfigureAwait(false);
                numberOfUpdatedMetadata += updateCount;

                //RemoveOldMetsMetadata(username); 

            }
            catch (Exception ex) 
            {
                Log.Error($"Error mets: ", ex);
            }
            

            Log.Info("Number of metadata updated: " + numberOfUpdatedMetadata);
            return numberOfUpdatedMetadata;
        }

        public async Task<int> SynchronizeMetsMetadata(string username)
        {
            var numberOfMetadataCreatedUpdated = await UpdateMetsMetadata(username);               

            return numberOfMetadataCreatedUpdated;
        }

        string _userName = "";



        int numberOfItems = 0;
        int limit = 10;

        private async Task<int> UpdateMetsMetadata(string username)
        {
            _userName = username;

            limit = 200;

            RunSearchNina(1);

            //RunSearchClimateSeries(1);

            limit = 200;

            RunSearchClimateDatasets(WebConfigurationManager.AppSettings["ClimateSerieUuid1"], 1);

            limit = 200;

            RunSearchClimateDatasets(WebConfigurationManager.AppSettings["ClimateSerieUuid2"], 1);

            limit = 200;

            //RunSearchSentinel(1);

            //RunSearch(1); //series 

            limit = 200;

            RunSearchNiva(1);

            return numberOfItems;

            ////NILU

            //_geoNorge = new GeoNorge("", "", "https://adc.csw.met.no/");

            //filters = new object[]
            //{
            //            new PropertyIsLikeType
            //                {
            //                    escapeChar = "\\",
            //                    singleChar = "_",
            //                    wildCard = "%",
            //                    PropertyName = new PropertyNameType {Text = new[] {"apiso:OrganisationName"}},
            //                    Literal = new LiteralType {Text = new[] {"NILU"}}
            //                }
            //};

            //filterNames = new ItemsChoiceType23[]
            //{
            //     ItemsChoiceType23.PropertyIsLike,
            //};


            //res = _geoNorge.SearchWithFilters(filters, filterNames, 1, 50, false, true);

            //_geoNorge = new GeoNorge(WebConfigurationManager.AppSettings["GeoNetworkUsername"], WebConfigurationManager.AppSettings["GeoNetworkPassword"], WebConfigurationManager.AppSettings["GeoNetworkUrl"]);

            //if (res != null && res.numberOfRecordsMatched != "0")
            //{
            //    foreach (MD_Metadata_Type item in res.Items)
            //    {
            //        try
            //        {
            //            MD_Metadata_Type existingMetadata = null;

            //            try
            //            {
            //                Log.Info("Harvest metadata for uuid: " + item.fileIdentifier.CharacterString);
            //                existingMetadata = _geoNorge.GetRecordByUuid(item.fileIdentifier.CharacterString);
            //            }
            //            catch { }

            //            SimpleMetadata simpleMetadata = new SimpleMetadata(item);
            //            string organizationName = "Stiftelsen NILU";
            //            simpleMetadata.ContactMetadata = new SimpleContact { Organization = organizationName, Email = simpleMetadata.ContactMetadata.Email, Name = simpleMetadata.ContactMetadata.Name, Role = simpleMetadata.ContactMetadata.Role, PositionName = simpleMetadata.ContactMetadata.PositionName };
            //            simpleMetadata.ContactOwner = new SimpleContact { Organization = organizationName, Email = simpleMetadata.ContactMetadata.Email, Name = simpleMetadata.ContactMetadata.Name, Role = "owner", PositionName = simpleMetadata.ContactMetadata.PositionName };

            //            if (existingMetadata == null)
            //            {
            //                var trans = _geoNorge.MetadataInsert(simpleMetadata.GetMetadata(), Kartverket.MetadataEditor.Util.GeoNetworkUtil.CreateAdditionalHeadersWithUsername(username, "true"));
            //            }
            //            else
            //            {
            //                var trans = _geoNorge.MetadataUpdate(simpleMetadata.GetMetadata(), Kartverket.MetadataEditor.Util.GeoNetworkUtil.CreateAdditionalHeadersWithUsername(username, "true"));
            //            }

            //            numberOfItems++;
            //        }
            //        catch (Exception ex)
            //        {
            //            Log.Error(ex);
            //        }
            //    }
            //}
        }

        private int RunSearchClimateDatasets(string parentIdentifier, int startPosition)
        {
            Log.Info("Running search climate datasets from start position: " + startPosition);
            SearchResultsType res = null;
            try
            {
                var url = WebConfigurationManager.AppSettings["MetUrlADC"] + "SERVICE=CSW&defaultPath=";
                _geoNorge = new GeoNorge("", "", url);

                var filters = new object[]
                {
                new PropertyIsLikeType
                    {
                        escapeChar = "\\",
                        singleChar = "_",
                        wildCard = "%",
                        PropertyName = new PropertyNameType {Text = new[] {"apiso:ParentIdentifier"}},
                        Literal = new LiteralType {Text = new[] {parentIdentifier}}
                    }
                };

                var filterNames = new ItemsChoiceType23[]
                {
                 ItemsChoiceType23.PropertyIsLike,
                };


                res = _geoNorge.SearchWithFilters(filters, filterNames, startPosition, limit, false, true);


                _geoNorge = new GeoNorge(WebConfigurationManager.AppSettings["GeoNetworkUsername"], WebConfigurationManager.AppSettings["GeoNetworkPassword"], WebConfigurationManager.AppSettings["GeoNetworkUrl"]);

                if (res != null && res.numberOfRecordsMatched != "0")
                {
                    foreach (MD_Metadata_Type item in res.Items)
                    {
                        MD_Metadata_Type existingMetadata = null;

                        try
                        {
                            Log.Info("Harvest metadata for uuid: " + item.fileIdentifier.CharacterString);
                            existingMetadata = _geoNorge.GetRecordByUuid(item.fileIdentifier.CharacterString);
                        }
                        catch (Exception exx)
                        {
                            Log.Info("Error Harvest metadata for uuid: " + item.fileIdentifier.CharacterString, exx);
                        }
                        if (item.hierarchyLevel[0].MD_ScopeCode.Value == "dataset") 
                        { 
                            if (existingMetadata == null)
                            {
                                var trans = _geoNorge.MetadataInsert(item, Kartverket.MetadataEditor.Util.GeoNetworkUtil.CreateAdditionalHeadersWithUsername(_userName, "true"));
                            }
                            else
                            {
                                var trans = _geoNorge.MetadataUpdate(item, Kartverket.MetadataEditor.Util.GeoNetworkUtil.CreateAdditionalHeadersWithUsername(_userName, "true"));
                            }

                            numberOfItems++;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Log.Error("Error in metadata from Geonetwork position: " + startPosition + " + " + limit + ".", exception);
            }

            int nextRecord;
            int numberOfRecordsMatched;

            nextRecord = int.Parse(res.nextRecord);
            numberOfRecordsMatched = int.Parse(res.numberOfRecordsMatched);

            int diff = numberOfRecordsMatched - nextRecord;
            if (nextRecord + limit > numberOfRecordsMatched)
            {
                limit = diff + 1;
            }

            if (nextRecord > 0 && nextRecord < numberOfRecordsMatched)
            {
                RunSearchClimateDatasets(parentIdentifier, nextRecord);
            }

            return numberOfItems;
        }

        private int RunSearchClimateSeries(int startPosition)
        {
            Log.Info("Running search climate series from start position: " + startPosition);
            SearchResultsType res = null;
            try
            {
            var url = WebConfigurationManager.AppSettings["MetUrlADC"] + "SERVICE=CSW&VERSION=2.0.2&REQUEST=GetRecords&RESULTTYPE=results&TYPENAMES=csw:Record&ElementSetName=full&outputschema=http://www.isotc211.org/2005/gmd&CONSTRAINTLANGUAGE=CQL_TEXT&CONSTRAINT=apiso:Type%20like%20%27series%27&startPosition=" + startPosition + "&defaultPath=";
            _geoNorge = new GeoNorge("", "", url);

            res = _geoNorge.GetFromEndpointUrl();

            _geoNorge = new GeoNorge(WebConfigurationManager.AppSettings["GeoNetworkUsername"], WebConfigurationManager.AppSettings["GeoNetworkPassword"], WebConfigurationManager.AppSettings["GeoNetworkUrl"]);

            if (res != null && res.numberOfRecordsMatched != "0")
            {
                foreach (MD_Metadata_Type item in res.Items)
                {
                    MD_Metadata_Type existingMetadata = null;

                    try
                    {
                        Log.Info("Harvest metadata for uuid: " + item.fileIdentifier.CharacterString);
                        existingMetadata = _geoNorge.GetRecordByUuid(item.fileIdentifier.CharacterString);
                    }
                    catch (Exception exx)
                    {
                        Log.Info("Error Harvest metadata for uuid: " + item.fileIdentifier.CharacterString, exx);
                    }

                    if (existingMetadata == null)
                    {
                        var trans = _geoNorge.MetadataInsert(item, Kartverket.MetadataEditor.Util.GeoNetworkUtil.CreateAdditionalHeadersWithUsername(_userName, "true"));
                    }
                    else
                    {
                        var trans = _geoNorge.MetadataUpdate(item, Kartverket.MetadataEditor.Util.GeoNetworkUtil.CreateAdditionalHeadersWithUsername(_userName, "true"));
                    }

                    numberOfItems++;
                }
            }
            }
            catch (Exception exception)
            {
                Log.Error("Error in metadata from Geonetwork position: " + startPosition + " + " + limit + ".", exception);
            }

            int nextRecord;
            int numberOfRecordsMatched;

            nextRecord = int.Parse(res.nextRecord);
            numberOfRecordsMatched = int.Parse(res.numberOfRecordsMatched);

            int diff = numberOfRecordsMatched - nextRecord;
            if (nextRecord + limit > numberOfRecordsMatched)
            {
                limit = diff;
            }

            if (nextRecord > 0 && nextRecord < numberOfRecordsMatched)
            {
                RunSearchClimateSeries(nextRecord);
            }

            return numberOfItems;
        }

        private int RunSearchNina(int startPosition)
        {
            Log.Info("Running search nina from start position: " + startPosition);
            SearchResultsType res = null;
            try
            {
                _geoNorge = new GeoNorge("", "", "https://csw.nina.no/csw?service=CSW&version=2.0.2&request=GetRecords&elementsetname=full&typenames=gmd:MD_Metadata&outputSchema=http://www.isotc211.org/2005/gmd&outputFormat=application/xml&resulttype=results&startPosition=" + startPosition);

                res = _geoNorge.GetFromEndpointUrl();

                _geoNorge = new GeoNorge(WebConfigurationManager.AppSettings["GeoNetworkUsername"], WebConfigurationManager.AppSettings["GeoNetworkPassword"], WebConfigurationManager.AppSettings["GeoNetworkUrl"]);

                if (res != null && res.numberOfRecordsMatched != "0")
                {
                    foreach (MD_Metadata_Type item in res.Items)
                    {
                        MD_Metadata_Type existingMetadata = null;

                        try
                        {
                            Log.Info("Harvest metadata for NINA uuid: " + item.fileIdentifier.CharacterString);
                            existingMetadata = _geoNorge.GetRecordByUuid(item.fileIdentifier.CharacterString);
                        }
                        catch (Exception exx)
                        {
                            Log.Info("Error Harvest metadata for NINA uuid: " + item.fileIdentifier.CharacterString, exx);
                        }

                        if (existingMetadata == null)
                        {
                            var trans = _geoNorge.MetadataInsert(item, Kartverket.MetadataEditor.Util.GeoNetworkUtil.CreateAdditionalHeadersWithUsername(_userName, "true"));
                        }
                        else
                        {
                            var trans = _geoNorge.MetadataUpdate(item, Kartverket.MetadataEditor.Util.GeoNetworkUtil.CreateAdditionalHeadersWithUsername(_userName, "true"));
                        }

                        numberOfItems++;
                    }
                }
            }
            catch (Exception exception)
            {
                Log.Error("Error in metadata from NINA position: " + startPosition + " + " + limit + ".", exception);
            }

            int nextRecord;
            int numberOfRecordsMatched;

            nextRecord = int.Parse(res.nextRecord);
            numberOfRecordsMatched = int.Parse(res.numberOfRecordsMatched);

            int diff = numberOfRecordsMatched - nextRecord;
            if (nextRecord + limit > numberOfRecordsMatched)
            {
                limit = diff;
            }

            if (nextRecord > 0 && nextRecord < numberOfRecordsMatched)
            {
                RunSearchNina(nextRecord);
            }

            return numberOfItems;
        }

        private void LogEventsDebug(string log)
        {

            Log.Debug(log);
        }

        private int RunSearchNiva(int startPosition)
        {
            Log.Info("Running search niva from start position: " + startPosition);
            SearchResultsType res = null;
            try
            {

                _geoNorge = new GeoNorge("", "", "https://noiso-adc.csw.met.no/");
                _geoNorge.OnLogEventDebug += new GeoNorgeAPI.LogEventHandlerDebug(LogEventsDebug);

                var filters = new object[]
                {
                        new PropertyIsLikeType
                            {
                                escapeChar = "\\",
                                singleChar = "_",
                                wildCard = "%",
                                PropertyName = new PropertyNameType {Text = new[] {"apiso:OrganisationName"}},
                                Literal = new LiteralType {Text = new[] {"Norwegian Institute for Water Research"}}
                            }
                };

                var filterNames = new ItemsChoiceType23[]
                {
                 ItemsChoiceType23.PropertyIsLike,
                };


                res = _geoNorge.SearchWithFilters(filters, filterNames, startPosition, limit, false, true);

                _geoNorge = new GeoNorge(WebConfigurationManager.AppSettings["GeoNetworkUsername"], WebConfigurationManager.AppSettings["GeoNetworkPassword"], WebConfigurationManager.AppSettings["GeoNetworkUrl"]);

                if (res != null && res.numberOfRecordsMatched != "0")
                {
                    if (res.Items == null)
                    {
                        Log.Info("No items in response");
                        Log.Info(SerializeUtil.SerializeToString(res));
                        Log.Info("numberOfRecordsMatched: " + res.numberOfRecordsMatched);
                        return 0;
                    }

                    foreach (MD_Metadata_Type item in res.Items)
                    {
                        MD_Metadata_Type existingMetadata = null;

                        try
                        {
                            if(item.fileIdentifier.CharacterString == "no.met.adc:ceb8319c-5ebe-51cc-8359-959067daeadd" || item.fileIdentifier.CharacterString == "no.met.adc:ae988ee1-3369-5443-85f2-18359ae213e0")
                            { //todo can be removed later
                                Log.Info("Skipping climate series...");
                                continue;
                            }
                            Log.Info("Harvest metadata for uuid: " + item.fileIdentifier.CharacterString);
                            existingMetadata = _geoNorge.GetRecordByUuid(item.fileIdentifier.CharacterString);
                        }
                        catch (Exception exx)
                        {
                            Log.Info("Error Harvest metadata for uuid: " + item.fileIdentifier.CharacterString, exx);
                        }

                        if (existingMetadata == null)
                        {
                            var trans = _geoNorge.MetadataInsert(item, Kartverket.MetadataEditor.Util.GeoNetworkUtil.CreateAdditionalHeadersWithUsername(_userName, "true"));
                        }
                        else
                        {
                            var trans = _geoNorge.MetadataUpdate(item, Kartverket.MetadataEditor.Util.GeoNetworkUtil.CreateAdditionalHeadersWithUsername(_userName, "true"));
                        }

                        numberOfItems++;
                    }
                }
            }
            catch (Exception exception)
            {
                Log.Error("Error in metadata from Geonetwork position: " + startPosition + " + " + limit + ".", exception);
            }

            int nextRecord;
            int numberOfRecordsMatched;

            nextRecord = int.Parse(res.nextRecord);
            numberOfRecordsMatched = int.Parse(res.numberOfRecordsMatched);

            int diff = numberOfRecordsMatched - nextRecord;
            if (nextRecord + limit > numberOfRecordsMatched)
            {
                limit = diff;
            }

            if (nextRecord > 0 && nextRecord < numberOfRecordsMatched)
            {
                RunSearchNiva(nextRecord);
            }

            return numberOfItems;
        }
        private int RunSearch(int startPosition)
        {
            Log.Info("Running search from start position: " + startPosition);
            SearchResultsType res = null;
            try
            {
                _geoNorge = new GeoNorge("", "", WebConfigurationManager.AppSettings["MetUrl"]);

                var filters = new object[]
                          {

                    new BinaryLogicOpType()
                        {
                            Items = new object[]
                                {
                                    new PropertyIsLikeType
                                    {
                                        escapeChar = "\\",
                                        singleChar = "_",
                                        wildCard = "%",
                                        PropertyName = new PropertyNameType {Text = new[] {"apiso:Title"}},
                                        Literal = new LiteralType {Text = new[] { "%%" }}
                                    },
                                    new PropertyIsLikeType
                                    {
                                        escapeChar = "\\",
                                        singleChar = "_",
                                        wildCard = "%",
                                        PropertyName = new PropertyNameType {Text = new[] {"apiso:Type"}},
                                        Literal = new LiteralType {Text = new[] { "series" }}
                                    }
                                },

                                ItemsElementName = new ItemsChoiceType22[]
                                    {
                                        ItemsChoiceType22.PropertyIsLike, ItemsChoiceType22.PropertyIsLike
                                    }
                        },

                          };

                var filterNames = new ItemsChoiceType23[]
                    {
                    ItemsChoiceType23.And
                    };


                res = _geoNorge.SearchWithFilters(filters, filterNames, startPosition, limit, false, true);

                _geoNorge = new GeoNorge(WebConfigurationManager.AppSettings["GeoNetworkUsername"], WebConfigurationManager.AppSettings["GeoNetworkPassword"], WebConfigurationManager.AppSettings["GeoNetworkUrl"]);

                if (res != null && res.numberOfRecordsMatched != "0")
                {
                    foreach (MD_Metadata_Type item in res.Items)
                    {
                        MD_Metadata_Type existingMetadata = null;

                        try
                        {
                            Log.Info("Harvest metadata for uuid: " + item.fileIdentifier.CharacterString);
                            existingMetadata = _geoNorge.GetRecordByUuid(item.fileIdentifier.CharacterString);
                        }
                        catch(Exception exx) 
                        { 
                            Log.Info("Error Harvest metadata for uuid: " + item.fileIdentifier.CharacterString, exx); 
                        }

                        if (existingMetadata == null)
                        {
                            var trans = _geoNorge.MetadataInsert(item, Kartverket.MetadataEditor.Util.GeoNetworkUtil.CreateAdditionalHeadersWithUsername(_userName, "true"));
                        }
                        else
                        {
                            var trans = _geoNorge.MetadataUpdate(item, Kartverket.MetadataEditor.Util.GeoNetworkUtil.CreateAdditionalHeadersWithUsername(_userName, "true"));
                        }

                        numberOfItems++;
                    }
                }
            }
            catch (Exception exception)
            {
                Log.Error("Error in metadata from Geonetwork position: " + startPosition + " + "+ limit +".", exception);
            }

            int nextRecord;
            int numberOfRecordsMatched;

            nextRecord = int.Parse(res.nextRecord);
            numberOfRecordsMatched = int.Parse(res.numberOfRecordsMatched);

            int diff = numberOfRecordsMatched - nextRecord;
            if(nextRecord + limit > numberOfRecordsMatched)
            {
                limit = diff;
            }

            if (nextRecord > 0 && nextRecord < numberOfRecordsMatched)
            {
                RunSearch(nextRecord);
            }

            return numberOfItems;
        }

        private int RunSearchSentinel(int startPosition)
        {
            Log.Info("Running search from start position: " + startPosition);
            SearchResultsType res = null;
            try
            {
                _geoNorge = new GeoNorge("", "", "https://nbs.csw.met.no/csw?");

                var filters = new object[]
{
                        new PropertyIsLikeType
                            {
                                escapeChar = "\\",
                                        singleChar = "_",
                                        wildCard = "%",
                                        PropertyName = new PropertyNameType {Text = new[] {"apiso:Type"}},
                                        Literal = new LiteralType {Text = new[] { "series" }}
                            }
};

                var filterNames = new ItemsChoiceType23[]
                {
                 ItemsChoiceType23.PropertyIsLike,
                };


                res = _geoNorge.SearchWithFilters(filters, filterNames, startPosition, limit, false, true);

                _geoNorge = new GeoNorge(WebConfigurationManager.AppSettings["GeoNetworkUsername"], WebConfigurationManager.AppSettings["GeoNetworkPassword"], WebConfigurationManager.AppSettings["GeoNetworkUrl"]);

                if (res != null && res.numberOfRecordsMatched != "0")
                {
                    foreach (MD_Metadata_Type item in res.Items)
                    {
                        MD_Metadata_Type existingMetadata = null;

                        try
                        {
                            Log.Info("Harvest metadata for uuid: " + item.fileIdentifier.CharacterString);
                            existingMetadata = _geoNorge.GetRecordByUuid(item.fileIdentifier.CharacterString);
                        }
                        catch (Exception exx)
                        {
                            Log.Info("Error Harvest metadata for uuid: " + item.fileIdentifier.CharacterString, exx);
                        }

                        if (existingMetadata == null)
                        {
                            var trans = _geoNorge.MetadataInsert(item, Kartverket.MetadataEditor.Util.GeoNetworkUtil.CreateAdditionalHeadersWithUsername(_userName, "true"));
                        }
                        else
                        {
                            var trans = _geoNorge.MetadataUpdate(item, Kartverket.MetadataEditor.Util.GeoNetworkUtil.CreateAdditionalHeadersWithUsername(_userName, "true"));
                        }

                        numberOfItems++;
                    }
                }
            }
            catch (Exception exception)
            {
                Log.Error("Error in metadata from Geonetwork position: " + startPosition + " + " + limit + ".", exception);
            }

            int nextRecord;
            int numberOfRecordsMatched;

            nextRecord = int.Parse(res.nextRecord);
            numberOfRecordsMatched = int.Parse(res.numberOfRecordsMatched);

            int diff = numberOfRecordsMatched - nextRecord;
            if (nextRecord + limit > numberOfRecordsMatched)
            {
                limit = diff;
            }

            if (nextRecord > 0 && nextRecord < numberOfRecordsMatched)
            {
                RunSearch(nextRecord);
            }

            return numberOfItems;
        }

        private void RemoveOldMetsMetadata(string username)
        {
            //todo remove NIVA and NILU?
            List<string> metsSeriesInGeonorge = new List<string>();
            List<string> metsSeries = new List<string>();

            _geoNorge = new GeoNorge(WebConfigurationManager.AppSettings["GeoNetworkUsername"], WebConfigurationManager.AppSettings["GeoNetworkPassword"], WebConfigurationManager.AppSettings["GeoNetworkUrl"]);

            var filters = new object[]
                      {

                    new BinaryLogicOpType()
                        {
                            Items = new object[]
                                {
                                    new PropertyIsLikeType
                                    {
                                        escapeChar = "\\",
                                        singleChar = "_",
                                        wildCard = "%",
                                        PropertyName = new PropertyNameType {Text = new[] {"Type"}},
                                        Literal = new LiteralType {Text = new[] { "series" }}
                                    },
                                    new PropertyIsLikeType
                                    {
                                        escapeChar = "\\",
                                        singleChar = "_",
                                        wildCard = "%",
                                        PropertyName = new PropertyNameType {Text = new[] {"OrganisationName"}},
                                        Literal = new LiteralType {Text = new[] { "Meteorologisk_institutt" }}
                                    }
                                },

                                ItemsElementName = new ItemsChoiceType22[]
                                    {
                                        ItemsChoiceType22.PropertyIsLike, ItemsChoiceType22.PropertyIsLike
                                    }
                        },

                      };

            var filterNames = new ItemsChoiceType23[]
                {
                    ItemsChoiceType23.And
                };

            SearchResultsType res = _geoNorge.SearchWithFilters(filters, filterNames, 1, 400);

            if (res != null && res.numberOfRecordsMatched != "0")
            {
                foreach (var item in res.Items)
                {
                    RecordType record = (RecordType)item;
                    string uuid = "";
                    for (int i = 0; i < record.ItemsElementName.Length; i++)
                    {
                        var name = record.ItemsElementName[i];
                        var value = record.Items[i].Text != null ? record.Items[i].Text[0] : null;

                        if (name == ItemsChoiceType24.identifier) {
                            uuid = value;
                            metsSeriesInGeonorge.Add(uuid);
                        }
                    }
                }
            }

            _geoNorge = new GeoNorge("", "", WebConfigurationManager.AppSettings["MetUrl"]);

            filters = new object[]
                      {

                    new BinaryLogicOpType()
                        {
                            Items = new object[]
                                {
                                    new PropertyIsLikeType
                                    {
                                        escapeChar = "\\",
                                        singleChar = "_",
                                        wildCard = "%",
                                        PropertyName = new PropertyNameType {Text = new[] {"apiso:Title"}},
                                        Literal = new LiteralType {Text = new[] { "%%" }}
                                    },
                                    new PropertyIsLikeType
                                    {
                                        escapeChar = "\\",
                                        singleChar = "_",
                                        wildCard = "%",
                                        PropertyName = new PropertyNameType {Text = new[] {"apiso:Type"}},
                                        Literal = new LiteralType {Text = new[] { "series" }}
                                    }
                                },

                                ItemsElementName = new ItemsChoiceType22[]
                                    {
                                        ItemsChoiceType22.PropertyIsLike, ItemsChoiceType22.PropertyIsLike
                                    }
                        },

                      };

            filterNames = new ItemsChoiceType23[]
                {
                    ItemsChoiceType23.And
                };


            res = _geoNorge.SearchWithFilters(filters, filterNames, 1, 200, false, true);

            if (res != null && res.numberOfRecordsMatched != "0")
            {
                foreach (MD_Metadata_Type item in res.Items)
                {
                    metsSeries.Add(item.fileIdentifier.CharacterString);
                }
            }


            var deleteItems = metsSeriesInGeonorge.Where(x => !metsSeries.Contains(x)).ToList();

            foreach (var uuid in deleteItems) 
            {
                MetadataViewModel model = new MetadataViewModel();
                model.Uuid = uuid;
                _metadataService.DeleteMetadata(model, username, "Mets delete synchronize");
                Log.Info("Delete metadata for uuid: " + uuid);
            }

        }


    }

    public interface IMetsMetadataService
    {
        Task<int> SynchronizeMetadata(string username);   
    }
}