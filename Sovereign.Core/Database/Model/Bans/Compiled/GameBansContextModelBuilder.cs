﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

#pragma warning disable 219, 612, 618
#nullable disable

namespace Sovereign.Core.Database.Model.Bans.Compiled
{
    public partial class GameBansContextModel
    {
        partial void Initialize()
        {
            var gameBansHistoryEntry = GameBansHistoryEntryEntityType.Create(this);

            GameBansHistoryEntryEntityType.CreateAnnotations(gameBansHistoryEntry);

            AddAnnotation("ProductVersion", "8.0.8");
            AddRuntimeAnnotation("Relational:RelationalModel", CreateRelationalModel());
        }

        private IRelationalModel CreateRelationalModel()
        {
            var relationalModel = new RelationalModel(this);

            var gameBansHistoryEntry = FindEntityType("Sovereign.Core.Database.Model.Bans.GameBansHistoryEntry")!;

            var defaultTableMappings = new List<TableMappingBase<ColumnMappingBase>>();
            gameBansHistoryEntry.SetRuntimeAnnotation("Relational:DefaultMappings", defaultTableMappings);
            var sovereignCoreDatabaseModelBansGameBansHistoryEntryTableBase = new TableBase("Sovereign.Core.Database.Model.Bans.GameBansHistoryEntry", null, relationalModel);
            var banIdColumnBase = new ColumnBase<ColumnMappingBase>("BanId", "INTEGER", sovereignCoreDatabaseModelBansGameBansHistoryEntryTableBase);
            sovereignCoreDatabaseModelBansGameBansHistoryEntryTableBase.Columns.Add("BanId", banIdColumnBase);
            var domainColumnBase = new ColumnBase<ColumnMappingBase>("Domain", "TEXT", sovereignCoreDatabaseModelBansGameBansHistoryEntryTableBase);
            sovereignCoreDatabaseModelBansGameBansHistoryEntryTableBase.Columns.Add("Domain", domainColumnBase);
            var gameIdColumnBase = new ColumnBase<ColumnMappingBase>("GameId", "INTEGER", sovereignCoreDatabaseModelBansGameBansHistoryEntryTableBase);
            sovereignCoreDatabaseModelBansGameBansHistoryEntryTableBase.Columns.Add("GameId", gameIdColumnBase);
            var idColumnBase = new ColumnBase<ColumnMappingBase>("Id", "INTEGER", sovereignCoreDatabaseModelBansGameBansHistoryEntryTableBase);
            sovereignCoreDatabaseModelBansGameBansHistoryEntryTableBase.Columns.Add("Id", idColumnBase);
            var timeColumnBase = new ColumnBase<ColumnMappingBase>("Time", "TEXT", sovereignCoreDatabaseModelBansGameBansHistoryEntryTableBase);
            sovereignCoreDatabaseModelBansGameBansHistoryEntryTableBase.Columns.Add("Time", timeColumnBase);
            relationalModel.DefaultTables.Add("Sovereign.Core.Database.Model.Bans.GameBansHistoryEntry", sovereignCoreDatabaseModelBansGameBansHistoryEntryTableBase);
            var sovereignCoreDatabaseModelBansGameBansHistoryEntryMappingBase = new TableMappingBase<ColumnMappingBase>(gameBansHistoryEntry, sovereignCoreDatabaseModelBansGameBansHistoryEntryTableBase, true);
            sovereignCoreDatabaseModelBansGameBansHistoryEntryTableBase.AddTypeMapping(sovereignCoreDatabaseModelBansGameBansHistoryEntryMappingBase, false);
            defaultTableMappings.Add(sovereignCoreDatabaseModelBansGameBansHistoryEntryMappingBase);
            RelationalModel.CreateColumnMapping((ColumnBase<ColumnMappingBase>)idColumnBase, gameBansHistoryEntry.FindProperty("Id")!, sovereignCoreDatabaseModelBansGameBansHistoryEntryMappingBase);
            RelationalModel.CreateColumnMapping((ColumnBase<ColumnMappingBase>)banIdColumnBase, gameBansHistoryEntry.FindProperty("BanId")!, sovereignCoreDatabaseModelBansGameBansHistoryEntryMappingBase);
            RelationalModel.CreateColumnMapping((ColumnBase<ColumnMappingBase>)domainColumnBase, gameBansHistoryEntry.FindProperty("Domain")!, sovereignCoreDatabaseModelBansGameBansHistoryEntryMappingBase);
            RelationalModel.CreateColumnMapping((ColumnBase<ColumnMappingBase>)gameIdColumnBase, gameBansHistoryEntry.FindProperty("GameId")!, sovereignCoreDatabaseModelBansGameBansHistoryEntryMappingBase);
            RelationalModel.CreateColumnMapping((ColumnBase<ColumnMappingBase>)timeColumnBase, gameBansHistoryEntry.FindProperty("Time")!, sovereignCoreDatabaseModelBansGameBansHistoryEntryMappingBase);

            var tableMappings = new List<TableMapping>();
            gameBansHistoryEntry.SetRuntimeAnnotation("Relational:TableMappings", tableMappings);
            var gameBansHistoryTable = new Table("GameBansHistory", null, relationalModel);
            var idColumn = new Column("Id", "INTEGER", gameBansHistoryTable);
            gameBansHistoryTable.Columns.Add("Id", idColumn);
            var banIdColumn = new Column("BanId", "INTEGER", gameBansHistoryTable);
            gameBansHistoryTable.Columns.Add("BanId", banIdColumn);
            var domainColumn = new Column("Domain", "TEXT", gameBansHistoryTable);
            gameBansHistoryTable.Columns.Add("Domain", domainColumn);
            var gameIdColumn = new Column("GameId", "INTEGER", gameBansHistoryTable);
            gameBansHistoryTable.Columns.Add("GameId", gameIdColumn);
            var timeColumn = new Column("Time", "TEXT", gameBansHistoryTable);
            gameBansHistoryTable.Columns.Add("Time", timeColumn);
            var pK_GameBansHistory = new UniqueConstraint("PK_GameBansHistory", gameBansHistoryTable, new[] { idColumn });
            gameBansHistoryTable.PrimaryKey = pK_GameBansHistory;
            var pK_GameBansHistoryUc = RelationalModel.GetKey(this,
                "Sovereign.Core.Database.Model.Bans.GameBansHistoryEntry",
                new[] { "Id" });
            pK_GameBansHistory.MappedKeys.Add(pK_GameBansHistoryUc);
            RelationalModel.GetOrCreateUniqueConstraints(pK_GameBansHistoryUc).Add(pK_GameBansHistory);
            gameBansHistoryTable.UniqueConstraints.Add("PK_GameBansHistory", pK_GameBansHistory);
            relationalModel.Tables.Add(("GameBansHistory", null), gameBansHistoryTable);
            var gameBansHistoryTableMapping = new TableMapping(gameBansHistoryEntry, gameBansHistoryTable, true);
            gameBansHistoryTable.AddTypeMapping(gameBansHistoryTableMapping, false);
            tableMappings.Add(gameBansHistoryTableMapping);
            RelationalModel.CreateColumnMapping(idColumn, gameBansHistoryEntry.FindProperty("Id")!, gameBansHistoryTableMapping);
            RelationalModel.CreateColumnMapping(banIdColumn, gameBansHistoryEntry.FindProperty("BanId")!, gameBansHistoryTableMapping);
            RelationalModel.CreateColumnMapping(domainColumn, gameBansHistoryEntry.FindProperty("Domain")!, gameBansHistoryTableMapping);
            RelationalModel.CreateColumnMapping(gameIdColumn, gameBansHistoryEntry.FindProperty("GameId")!, gameBansHistoryTableMapping);
            RelationalModel.CreateColumnMapping(timeColumn, gameBansHistoryEntry.FindProperty("Time")!, gameBansHistoryTableMapping);
            return relationalModel.MakeReadOnly();
        }
    }
}
