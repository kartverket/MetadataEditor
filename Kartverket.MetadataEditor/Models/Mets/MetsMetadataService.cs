using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GeoNorgeAPI;
using Kartverket.Geonorge.Utilities;
using log4net;
using System.Data.Entity;
using www.opengis.net;
using System.Threading;
using System.Web;
using System.Web.Configuration;

namespace Kartverket.MetadataEditor.Models.Mets
{
    internal class MetsMetadataService : IMetsMetadataService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IMetadataService _metadataService;
        IGeoNorge _geoNorge;


        public MetsMetadataService(IMetadataService metadataService, IGeoNorge geoNorge)
        {
            _metadataService = metadataService;
            _geoNorge = geoNorge;
        }

        public async Task<int> SynchronizeMetadata(string username)
        {
            Log.Info("Synching mets metadata initiated");

            var numberOfUpdatedMetadata = 0;
            
            try
            {
                var updateCount = await SynchronizeMetsMetadata(username).ConfigureAwait(false);
                numberOfUpdatedMetadata += updateCount;

                RemoveOldMetsMetadata(username); 

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

        private async Task<int> UpdateMetsMetadata(string username)
        {
            //todo handle nextRecord (only 10 returned) and logging

            _geoNorge = new GeoNorge("", "", "https://data.csw.met.no/?");

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
                                        Literal = new LiteralType {Text = new[] { "%satellite%" }}
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


            var res = _geoNorge.SearchWithFilters(filters, filterNames, 1, 200, false, true);

            _geoNorge = new GeoNorge(WebConfigurationManager.AppSettings["GeoNetworkUsername"], WebConfigurationManager.AppSettings["GeoNetworkPassword"], WebConfigurationManager.AppSettings["GeoNetworkUrl"]);

            int numberOfItems = 0;

            if (res != null && res.numberOfRecordsMatched != "0")
            {
                foreach (MD_Metadata_Type item in res.Items)
                {
                    MD_Metadata_Type existingMetadata = null;

                    try
                    {
                        existingMetadata = _geoNorge.GetRecordByUuid(item.fileIdentifier.CharacterString);
                    }
                    catch { }

                    if (existingMetadata == null)
                    {

                        var trans = _geoNorge.MetadataInsert(item, Kartverket.MetadataEditor.Util.GeoNetworkUtil.CreateAdditionalHeadersWithUsername(username, "true"));
                    }
                    else
                    {
                        var trans = _geoNorge.MetadataUpdate(item, Kartverket.MetadataEditor.Util.GeoNetworkUtil.CreateAdditionalHeadersWithUsername(username, "true"));
                    }

                    numberOfItems++;
                }
            }

            //NIVA

            _geoNorge = new GeoNorge("", "", " https://adc.csw.met.no/");

            filters = new object[]
            {
                        new PropertyIsLikeType
                            {
                                escapeChar = "\\",
                                singleChar = "_",
                                wildCard = "%",
                                PropertyName = new PropertyNameType {Text = new[] {"apiso:OrganisationName"}},
                                Literal = new LiteralType {Text = new[] {"NIVA"}}
                            }
            };

            filterNames = new ItemsChoiceType23[]
            {
                 ItemsChoiceType23.PropertyIsLike,
            };


            res = _geoNorge.SearchWithFilters(filters, filterNames, 1, 50, false, true);

            _geoNorge = new GeoNorge(WebConfigurationManager.AppSettings["GeoNetworkUsername"], WebConfigurationManager.AppSettings["GeoNetworkPassword"], WebConfigurationManager.AppSettings["GeoNetworkUrl"]);

            if (res != null && res.numberOfRecordsMatched != "0")
            {
                foreach (MD_Metadata_Type item in res.Items)
                {
                    try
                    {
                        MD_Metadata_Type existingMetadata = null;

                        try
                        {
                            existingMetadata = _geoNorge.GetRecordByUuid(item.fileIdentifier.CharacterString);
                        }
                        catch { }

                        SimpleMetadata simpleMetadata = new SimpleMetadata(item);
                        string organizationName = "Norsk institutt for vannforskning";
                        string organizationNameEnglish = "Research institute for water and the environment";
                        simpleMetadata.ContactMetadata = new SimpleContact { Organization = organizationName, Email = simpleMetadata.ContactMetadata.Email, Name = simpleMetadata.ContactMetadata.Name, Role = simpleMetadata.ContactMetadata.Role, PositionName = simpleMetadata.ContactMetadata.PositionName, OrganizationEnglish = organizationNameEnglish };
                        simpleMetadata.ContactOwner = new SimpleContact { Organization = organizationName, Email = simpleMetadata.ContactMetadata.Email, Name = simpleMetadata.ContactMetadata.Name, Role = "owner", PositionName = simpleMetadata.ContactMetadata.PositionName, OrganizationEnglish = organizationNameEnglish };

                        if (existingMetadata == null)
                        {

                            var trans = _geoNorge.MetadataInsert(simpleMetadata.GetMetadata(), Kartverket.MetadataEditor.Util.GeoNetworkUtil.CreateAdditionalHeadersWithUsername(username, "true"));
                        }
                        else
                        {
                            var trans = _geoNorge.MetadataUpdate(simpleMetadata.GetMetadata(), Kartverket.MetadataEditor.Util.GeoNetworkUtil.CreateAdditionalHeadersWithUsername(username, "true"));
                        }

                        numberOfItems++;
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                    }
                }
            }


            //NILU

            filters = new object[]
            {
                        new PropertyIsLikeType
                            {
                                escapeChar = "\\",
                                singleChar = "_",
                                wildCard = "%",
                                PropertyName = new PropertyNameType {Text = new[] {"apiso:OrganisationName"}},
                                Literal = new LiteralType {Text = new[] {"NILU"}}
                            }
            };

            filterNames = new ItemsChoiceType23[]
            {
                 ItemsChoiceType23.PropertyIsLike,
            };


            res = _geoNorge.SearchWithFilters(filters, filterNames, 1, 50, false, true);

            _geoNorge = new GeoNorge(WebConfigurationManager.AppSettings["GeoNetworkUsername"], WebConfigurationManager.AppSettings["GeoNetworkPassword"], WebConfigurationManager.AppSettings["GeoNetworkUrl"]);

            if (res != null && res.numberOfRecordsMatched != "0")
            {
                foreach (MD_Metadata_Type item in res.Items)
                {
                    try
                    {
                        MD_Metadata_Type existingMetadata = null;

                        try
                        {
                            existingMetadata = _geoNorge.GetRecordByUuid(item.fileIdentifier.CharacterString);
                        }
                        catch { }

                        SimpleMetadata simpleMetadata = new SimpleMetadata(item);
                        string organizationName = "Stiftelsen NILU";
                        simpleMetadata.ContactMetadata = new SimpleContact { Organization = organizationName, Email = simpleMetadata.ContactMetadata.Email, Name = simpleMetadata.ContactMetadata.Name, Role = simpleMetadata.ContactMetadata.Role, PositionName = simpleMetadata.ContactMetadata.PositionName };
                        simpleMetadata.ContactOwner = new SimpleContact { Organization = organizationName, Email = simpleMetadata.ContactMetadata.Email, Name = simpleMetadata.ContactMetadata.Name, Role = "owner", PositionName = simpleMetadata.ContactMetadata.PositionName };

                        if (existingMetadata == null)
                        {

                            var trans = _geoNorge.MetadataInsert(simpleMetadata.GetMetadata(), Kartverket.MetadataEditor.Util.GeoNetworkUtil.CreateAdditionalHeadersWithUsername(username, "true"));
                        }
                        else
                        {
                            var trans = _geoNorge.MetadataUpdate(simpleMetadata.GetMetadata(), Kartverket.MetadataEditor.Util.GeoNetworkUtil.CreateAdditionalHeadersWithUsername(username, "true"));
                        }

                        numberOfItems++;
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                    }
                }
            }

            return numberOfItems;
        }

        private void RemoveOldMetsMetadata(string username)
        {

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

            _geoNorge = new GeoNorge("", "", "https://data.csw.met.no/?");

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
                                        Literal = new LiteralType {Text = new[] { "%satellite%" }}
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
            }

        }


    }

    public interface IMetsMetadataService
    {
        Task<int> SynchronizeMetadata(string username);
    }
}