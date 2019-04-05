﻿using System;
using System.Collections;
using System.Collections.Async;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mighty.Dynamic.Tests.SqlServer.TableClasses;
using NUnit.Framework;

namespace Mighty.Dynamic.Tests.SqlServer
{
	[TestFixture]
	public class AsyncReadTests
    {
		[Test]
		public async Task Guid_Arg()
		{
			// SQL Server has true Guid type support
			var db = new MightyOrm(TestConstants.ReadTestConnection);
			var guid = Guid.NewGuid();
			var command = db.CreateCommand("SELECT @0 AS val", null, guid);
			Assert.AreEqual(DbType.Guid, command.Parameters[0].DbType);
			var item = await db.SingleAsync(command);
			Assert.AreEqual(guid, item.val);
		}


		[Test]
		public async Task MaxOnFilteredSet()
		{
			var soh = new SalesOrderHeader();
			var result = await ((dynamic)soh).MaxAsync(columns: "SalesOrderID", where: "SalesOrderID < @0", args: 100000);
			Assert.AreEqual(75123, result);
		}


		[Test]
		public async Task MaxOnFilteredSet2()
		{
			var soh = new SalesOrderHeader();
			var result = await ((dynamic)soh).MaxAsync(columns: "SalesOrderID", TerritoryID: 10);
			Assert.AreEqual(75117, result);
		}


		[Test]
		public void EmptyElement_ProtoType()
		{
			var soh = new SalesOrderHeader();
			dynamic defaults = soh.New();
			Assert.IsTrue(defaults.OrderDate > DateTime.MinValue);
		}


		[Test]
		public void SchemaTableMetaDataRetrieval()
		{
			var soh = new SalesOrderHeader();
			var metaData = soh.TableMetaData;
			Assert.IsNotNull(metaData);
			Assert.AreEqual(26, metaData.Count());
			Assert.IsTrue(metaData.All(v=>v.TABLE_NAME==soh.BareTableName));
		}


		[Test]
		public async Task All_NoParameters()
		{
			var soh = new SalesOrderHeader();
			var allRows = await (await soh.AllAsync()).ToListAsync();
			Assert.AreEqual(31465, allRows.Count);
		}


		[Test]
		public async Task All_NoParameters_Streaming()
		{
			var soh = new SalesOrderHeader();
			var allRows = await soh.AllAsync();
			var count = 0;
			await allRows.ForEachAsync(r => {
				count++;
				Assert.AreEqual(26, ((IDictionary<string, object>)r).Count);        // # of fields fetched should be 26
			});
			Assert.AreEqual(31465, count);
		}


		[Test]
		public async Task All_LimitSpecification()
		{
			var soh = new SalesOrderHeader();
			var allRows = await (await soh.AllAsync(limit: 10)).ToListAsync();
			Assert.AreEqual(10, allRows.Count);
		}
		

		[Test]
		public async Task All_ColumnSpecification()
		{
			var soh = new SalesOrderHeader();
			var allRows = await (await soh.AllAsync(columns: "SalesOrderID as SOID, Status, SalesPersonID")).ToListAsync();
			Assert.AreEqual(31465, allRows.Count);
			var firstRow = (IDictionary<string, object>)allRows[0];
			Assert.AreEqual(3, firstRow.Count);
			Assert.IsTrue(firstRow.ContainsKey("SOID"));
			Assert.IsTrue(firstRow.ContainsKey("Status"));
			Assert.IsTrue(firstRow.ContainsKey("SalesPersonID"));
		}


		[Test]
		public async Task All_OrderBySpecification()
		{
			var soh = new SalesOrderHeader();
			var allRows = await (await soh.AllAsync(orderBy: "CustomerID DESC")).ToListAsync();
			Assert.AreEqual(31465, allRows.Count);
			int previous = int.MaxValue;
			foreach(var r in allRows)
			{
				int current = r.CustomerID;
				Assert.IsTrue(current <= previous);
				previous = current;
			}
		}


		[Test]
		public async Task All_WhereSpecification()
		{
			var soh = new SalesOrderHeader();
			var allRows = await (await soh.AllAsync(where: "WHERE CustomerId=@0", args: 30052)).ToListAsync();
			Assert.AreEqual(4, allRows.Count);
		}


		[Test]
		public async Task All_WhereSpecification_OrderBySpecification()
		{
			var soh = new SalesOrderHeader();
			var allRows = await (await soh.AllAsync(orderBy: "SalesOrderID DESC", where: "WHERE CustomerId=@0", args: 30052)).ToListAsync();
			Assert.AreEqual(4, allRows.Count);
			int previous = int.MaxValue;
			foreach(var r in allRows)
			{
				int current = r.SalesOrderID;
				Assert.IsTrue(current <= previous);
				previous = current;
			}
		}
		

		[Test]
		public async Task All_WhereSpecification_ColumnsSpecification()
		{
			var soh = new SalesOrderHeader();
			var allRows = await (await soh.AllAsync(columns: "SalesOrderID as SOID, Status, SalesPersonID", where: "WHERE CustomerId=@0", args: 30052)).ToListAsync();
			Assert.AreEqual(4, allRows.Count);
			var firstRow = (IDictionary<string, object>)allRows[0];
			Assert.AreEqual(3, firstRow.Count);
			Assert.IsTrue(firstRow.ContainsKey("SOID"));
			Assert.IsTrue(firstRow.ContainsKey("Status"));
			Assert.IsTrue(firstRow.ContainsKey("SalesPersonID"));
		}


		[Test]
		public async Task All_WhereSpecification_ColumnsSpecification_LimitSpecification()
		{
			var soh = new SalesOrderHeader();
			var allRows = await (await soh.AllAsync(limit: 2, columns: "SalesOrderID as SOID, Status, SalesPersonID", where: "WHERE CustomerId=@0", args: 30052)).ToListAsync();
			Assert.AreEqual(2, allRows.Count);
			var firstRow = (IDictionary<string, object>)allRows[0];
			Assert.AreEqual(3, firstRow.Count);
			Assert.IsTrue(firstRow.ContainsKey("SOID"));
			Assert.IsTrue(firstRow.ContainsKey("Status"));
			Assert.IsTrue(firstRow.ContainsKey("SalesPersonID"));
		}


		[Test]
		public async Task Find_AllColumns()
		{
			dynamic soh = new SalesOrderHeader();
			var singleInstance = await soh.FindAsync(SalesOrderID: 43666);
			Assert.AreEqual(43666, singleInstance.SalesOrderID);
		}


		[Test]
		public async Task Find_OneColumn()
		{
			dynamic soh = new SalesOrderHeader();
			var singleInstance = await soh.FindAsync(SalesOrderID: 43666, columns:"SalesOrderID");
			Assert.AreEqual(43666, singleInstance.SalesOrderID);
			var siAsDict = (IDictionary<string, object>)singleInstance;
			Assert.AreEqual(1, siAsDict.Count);
		}


		[Test]
		public async Task Get_AllColumns()
		{
			dynamic soh = new SalesOrderHeader();
			var singleInstance = await soh.GetAsync(SalesOrderID: 43666);
			Assert.AreEqual(43666, singleInstance.SalesOrderID);
		}


		[Test]
		public async Task First_AllColumns()
		{
			dynamic soh = new SalesOrderHeader();
			var singleInstance = await soh.FirstAsync(SalesOrderID: 43666);
			Assert.AreEqual(43666, singleInstance.SalesOrderID);
		}


		[Test]
		public async Task Single_AllColumns()
		{
			dynamic soh = new SalesOrderHeader();
			var singleInstance = await soh.SingleAsync(SalesOrderID: 43666);
			Assert.AreEqual(43666, singleInstance.SalesOrderID);
			Assert.AreEqual(26, ((ExpandoObject)singleInstance).ToDictionary().Count);
		}


		[Test]
		public async Task Single_ThreeColumns()
		{
			dynamic soh = new SalesOrderHeader();
			var singleInstance = await soh.SingleAsync(SalesOrderID: 43666, columns: "SalesOrderID, SalesOrderNumber, OrderDate");
			Assert.AreEqual(43666, singleInstance.SalesOrderID);
			Assert.AreEqual("SO43666", singleInstance.SalesOrderNumber);
			Assert.AreEqual(new DateTime(2011, 5, 31), singleInstance.OrderDate);
			Assert.AreEqual(3, ((ExpandoObject)singleInstance).ToDictionary().Count);
		}


		[Test]
		public async Task DynamicMethod_RespondsToCancellation()
		{
			using (CancellationTokenSource cts = new CancellationTokenSource())
			{
				dynamic soh = new SalesOrderHeader();
				IAsyncEnumerable<dynamic> manyInstances = await soh.ManyAsync(columns: "SalesOrderID, SalesOrderNumber, OrderDate", cancellationToken: cts.Token);
				int count = 0;
				Assert.ThrowsAsync<TaskCanceledException>(async () => {
					await manyInstances.ForEachAsync(singleInstance => {
						Assert.AreEqual(3, ((ExpandoObject)singleInstance).ToDictionary().Count);
						count++;
						if (count == 7)
						{
							cts.Cancel();
						}
					});
				});
				Assert.AreEqual(7, count);
			}
		}


		[Test]
		public void DynamicMethod_ReportsInvalidCancellationToken()
		{
			dynamic soh = new SalesOrderHeader();
			Assert.ThrowsAsync<InvalidOperationException>(async () => {
				await soh.ManyAsync(columns: "SalesOrderID, SalesOrderNumber, OrderDate", cancellationToken: "");
			});
		}


		[Test]
		public async Task Query_AllRows()
		{
			var soh = new SalesOrderHeader();
			var allRows = await (await soh.QueryAsync("SELECT * FROM Sales.SalesOrderHeader")).ToListAsync();
			Assert.AreEqual(31465, allRows.Count);
		}


		[Test]
		public async Task Query_Filter()
		{
			var soh = new SalesOrderHeader();
			var filteredRows = await (await soh.QueryAsync("SELECT * FROM Sales.SalesOrderHeader WHERE CustomerID=@0", 30052)).ToListAsync();
			Assert.AreEqual(4, filteredRows.Count);
		}


		[Test]
		public async Task Paged_NoSpecification()
		{
			var soh = new SalesOrderHeader();
			// no order by, so in theory this is useless. It will order on PK though
			var page2 = await soh.PagedAsync(currentPage:2, pageSize: 30);
			var pageItems = ((IEnumerable<dynamic>)page2.Items).ToList();
			Assert.AreEqual(30, pageItems.Count);
			Assert.AreEqual(31465, page2.TotalRecords);
		}


		[Test]
		public async Task Paged_OrderBySpecification()
		{
			var soh = new SalesOrderHeader();
			var page2 = await soh.PagedAsync(orderBy: "CustomerID DESC", currentPage: 2, pageSize: 30);
			var pageItems = ((IEnumerable<dynamic>)page2.Items).ToList();
			Assert.AreEqual(30, pageItems.Count);
			Assert.AreEqual(31465, page2.TotalRecords);

			int previous = int.MaxValue;
			foreach(var r in pageItems)
			{
				int current = r.CustomerID;
				Assert.IsTrue(current <= previous);
				previous = current;
			}
		}


		[Test]
		public async Task Paged_OrderBySpecification_ColumnsSpecification()
		{
			var soh = new SalesOrderHeader();
			var page2 = await soh.PagedAsync(columns: "CustomerID, SalesOrderID", orderBy: "CustomerID DESC", currentPage: 2, pageSize: 30);
			var pageItems = ((IEnumerable<dynamic>)page2.Items).ToList();
			Assert.AreEqual(30, pageItems.Count);
			Assert.AreEqual(31465, page2.TotalRecords);
			var firstRow = (IDictionary<string, object>)pageItems[0];
			Assert.AreEqual(3, firstRow.Count);
			int previous = int.MaxValue;
			foreach(var r in pageItems)
			{
				int current = r.CustomerID;
				Assert.IsTrue(current <= previous);
				previous = current;
			}
		}


		[Test]
		public async Task Count_NoSpecification()
		{
			var soh = new SalesOrderHeader();
			var total = await soh.CountAsync();
			Assert.AreEqual(31465, total);
		}


		[Test]
		public async Task Count_WhereSpecification_FromArgs()
		{
			var soh = new SalesOrderHeader();
			var total = await soh.CountAsync(where: "WHERE CustomerId=@0", args:11212);
			Assert.AreEqual(17, total);
		}


		[Test]
		public async Task Count_WhereSpecification_FromArgsPlusNameValue()
		{
			dynamic soh = new SalesOrderHeader();
			var total = await soh.CountAsync(where: "WHERE CustomerId=@0", args: 11212, ModifiedDate: new DateTime(2013, 10, 10));
			Assert.AreEqual(2, total);
		}


		[Test]
		public async Task Count_WhereSpecification_FromNameValuePairs()
		{
			dynamic soh = new SalesOrderHeader();
			var total = await soh.CountAsync(CustomerID: 11212, ModifiedDate: new DateTime(2013, 10, 10));
			Assert.AreEqual(2, total);
		}


		/// <remarks>
		/// With correct brackets round the WHERE condition in the SQL this returns 17, otherwise it returns 31465!
		/// </remarks>
		[Test]
		public async Task Count_TestWhereWrapping()
		{
			dynamic soh = new SalesOrderHeader();
			var total = await soh.CountAsync(where: "1=1 OR 0=0", CustomerID: 11212);
			Assert.AreEqual(17, total);
		}


		[Test]
		public void DefaultValue()
		{
			var soh = new SalesOrderHeader(false);
			var value = soh.GetColumnDefault("OrderDate");
			Assert.AreEqual(typeof(DateTime), value.GetType());
		}


		[Test]
		public async Task IsValid_SalesPersonIDCheck()
		{
			dynamic soh = new SalesOrderHeader();
			var toValidate = await soh.FindAsync(SalesOrderID: 45816);
			// is invalid
			Assert.AreEqual(1, soh.IsValid(toValidate).Count);

			toValidate = await soh.FindAsync(SalesOrderID: 45069);
			// is valid
			Assert.AreEqual(0, soh.IsValid(toValidate).Count);
		}


		[Test]
		public async Task PrimaryKey_Read_Check()
		{
			dynamic soh = new SalesOrderHeader();
			var toValidate = await soh.FindAsync(SalesOrderID: 45816);

			Assert.IsTrue(soh.HasPrimaryKey(toValidate));

			var pkValue = soh.GetPrimaryKey(toValidate);
			Assert.AreEqual(45816, pkValue);
		}
	}
}