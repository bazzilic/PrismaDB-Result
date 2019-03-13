using Npgsql;
using PrismaDB.Result;
using System;
using System.Collections.Generic;
using Xunit;

namespace ResultTests
{
    [Collection("PostgreSQL Database Collection")]
    public class TestPostgreSqlResult : IDisposable
    {
        private NpgsqlConnection dbConn;

        public TestPostgreSqlResult(PostgreSqlDatabaseFixture fixture)
        {
            dbConn = fixture.DbConn;

            DropTables();

            var createSql = @"CREATE TABLE TT
                            (
                                a INT,
                                b VARCHAR(100),
                                c FLOAT8
                            ) ;";

            using (var cmd = new NpgsqlCommand(createSql, dbConn))
            {
                cmd.ExecuteNonQuery();
            }

            var insertSql = @"INSERT INTO TT (a, b, c) VALUES
                              ( 1, 'Hello', 0.0    ),
                              (12, 'Test',  12.345 ),
                              ( 0, 'data',  23     ),
                              (71, 'La_st', 098.450) ;";

            using (var cmd = new NpgsqlCommand(insertSql, dbConn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public void Dispose()
        {
            DropTables();
        }

        private void DropTables()
        {
            try
            {
                var dropSql = @"DROP TABLE TT ;";

                using (var cmd = new NpgsqlCommand(dropSql, dbConn))
                {
                    cmd.ExecuteNonQuery();
                }

            }
            catch { }
        }

        [Fact(DisplayName = "ResultReader Load")]
        public void TestResultReaderLoad()
        {
            var reader = new ResultReader();

            var selectSql = @"SELECT * FROM TT ;";
            using (var cmd = new NpgsqlCommand(selectSql, dbConn))
            {
                reader.Load(cmd.ExecuteReader());
            }

            Assert.Equal("a", reader.Columns[0].ColumnName);
            Assert.Equal("b", reader.Columns[1].ColumnName);
            Assert.Equal("c", reader.Columns[2].ColumnName);

            Assert.Equal(typeof(int), reader.Columns[0].DataType);
            Assert.Equal(typeof(string), reader.Columns[1].DataType);
            Assert.Equal(typeof(double), reader.Columns[2].DataType);

            Assert.Equal(100, reader.Columns[1].MaxLength);

            var results = new List<object[]>();
            while (reader.Read())
            {
                var row = new object[3];
                row[0] = reader[0];
                row[1] = reader[1];
                row[2] = reader[2];
                results.Add(row);
            }
            reader.Dispose();


            Assert.Equal("data", results[2][1]);
            Assert.Equal(4, results.Count);
        }
    }
}