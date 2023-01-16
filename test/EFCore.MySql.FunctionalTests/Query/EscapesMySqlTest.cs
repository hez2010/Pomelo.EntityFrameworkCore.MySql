﻿using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Pomelo.EntityFrameworkCore.MySql.FunctionalTests.TestUtilities;
using Pomelo.EntityFrameworkCore.MySql.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Pomelo.EntityFrameworkCore.MySql.FunctionalTests.Query
{
    public class EscapesMySqlTest : EscapesMySqlTestBase<EscapesMySqlTest.EscapesMySqlFixture>
    {
        public EscapesMySqlTest(EscapesMySqlFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [ConditionalFact]
        public override void Input_query_escapes_parameter()
        {
            base.Input_query_escapes_parameter();

            if (AppConfig.ServerVersion.Supports.Returning)
            {
                AssertSql(
                    @"@p0='Back\slash's Garden Party' (Nullable = false) (Size = 4000)

SET AUTOCOMMIT = 1;
INSERT INTO `Artists` (`Name`)
VALUES (@p0)
RETURNING `ArtistId`;",
                    //
                    @"SELECT `a`.`ArtistId`, `a`.`Name`
FROM `Artists` AS `a`
WHERE `a`.`Name` LIKE '% Garden Party'");
            }
            else
            {
                AssertSql(
                    @"@p0='Back\slash's Garden Party' (Nullable = false) (Size = 4000)

INSERT INTO `Artists` (`Name`)
VALUES (@p0);
SELECT `ArtistId`
FROM `Artists`
WHERE ROW_COUNT() = 1 AND `ArtistId` = LAST_INSERT_ID();",
                    //
                    @"SELECT `a`.`ArtistId`, `a`.`Name`
FROM `Artists` AS `a`
WHERE `a`.`Name` LIKE '% Garden Party'");
            }
        }

        [ConditionalTheory]
        public override async Task Where_query_escapes_literal(bool async)
        {
            await base.Where_query_escapes_literal(async);

            AssertSql(
                @"SELECT `a`.`ArtistId`, `a`.`Name`
FROM `Artists` AS `a`
WHERE `a`.`Name` = 'Back\\slasher''s'");
        }

        [ConditionalTheory]
        public override async Task Where_query_escapes_parameter(bool async)
        {
            await base.Where_query_escapes_parameter(async);

            AssertSql(
                @"@__artistName_0='Back\slasher's' (Size = 4000)

SELECT `a`.`ArtistId`, `a`.`Name`
FROM `Artists` AS `a`
WHERE `a`.`Name` = @__artistName_0");
        }

        [ConditionalTheory]
        public override async Task Where_contains_query_escapes(bool async)
        {
            await base.Where_contains_query_escapes(async);

            AssertSql(
                @"SELECT `a`.`ArtistId`, `a`.`Name`
FROM `Artists` AS `a`
WHERE `a`.`Name` IN ('Back\\slasher''s', 'John''s Chill Box')");
        }

        public class EscapesMySqlFixture : EscapesMySqlFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => MySqlTestStoreFactory.Instance;
        }
    }
}
