﻿using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using Examine.LuceneEngine;
using Examine.Web.Demo.Models;

namespace Examine.Web.Demo
{
    /// <summary>
    /// Data service for Examine using SqlCe's DirectTable reader as it is by far the fastest 
    /// way to read data from SqlCe.
    /// </summary>
    public class TableDirectReaderDataService : ISimpleDataService
    {
        public IEnumerable<SimpleDataSet> GetAllData(string indexType)
        {
            using (var db = new MyDbContext())
            {
                //using TableDirect is BY FAR one of the fastest ways to bulk insert data in SqlCe... 
                using (db.Database.Connection)
                {
                    db.Database.Connection.Open();
                    using (var cmd = (SqlCeCommand)db.Database.Connection.CreateCommand())
                    {
                        cmd.CommandText = "TestModels";
                        cmd.CommandType = CommandType.TableDirect;
                        var rs = cmd.ExecuteResultSet(ResultSetOptions.None);
                        while(rs.Read())
                        {
                            yield return new SimpleDataSet()
                            {
                                NodeDefinition = new IndexedNode()
                                {
                                    NodeId = rs.GetInt32(0),
                                    Type = "TestType"
                                },
                                RowData = new Dictionary<string, string>()
                                {
                                    {"Column1", rs.GetString(1)},
                                    {"Column2", rs.GetString(2)},
                                    {"Column3", rs.GetString(3)},
                                    {"Column4", rs.GetString(4)},
                                    {"Column5", rs.GetString(5)},
                                    {"Column6", rs.GetString(6)}
                                }
                            };
                        }
                    }                
                }
            }
        }
    }
}