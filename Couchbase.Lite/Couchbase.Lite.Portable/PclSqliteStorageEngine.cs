using System;
using System.Linq;
using SQLitePCL;
using System.Threading;
using System.Text;
using Couchbase.Lite.Storage;
using System.Collections.Generic;
using Sharpen;

namespace Couchbase.Lite
{
	public class PclSqliteStorageEngine : Couchbase.Lite.Storage.SQLiteStorageEngine
	{
		public PclSqliteStorageEngine ()
		{
		}

		int transactionCount = 0;
		SQLiteConnection connection;
		bool shouldCommit = false;

		#region implemented abstract members of SQLiteStorageEngine

		public override bool Open (string path)
		{
			var result = true;
			try {
				shouldCommit = false;
				connection = new SQLiteConnection (path);

				CreateFunctions ();
			} catch (Exception ex) {
				Log.E(Tag, "Error opening the Sqlite connection using connection String: {0}".Fmt(connectionString.ToString()), ex);
				result = false;    
			}

			return result;
		}

		void CreateFunctions()
		{
			connection.CreateFunction(
				"JSON",
				2,
				new Function((arguments) =>
					JsonCollator.Compare (
						JsonCollationMode.Unicode, 
						(string)arguments [0], 
						(string)arguments [1], 
						Int32.MaxValue)),
				true);

			connection.CreateFunction(
				"JSON_ASCII",
				2,
				new Function((arguments) =>
					JsonCollator.Compare (
						JsonCollationMode.Ascii, 
						(string)arguments [0], 
						(string)arguments [1], 
						Int32.MaxValue)),
				true);

			connection.CreateFunction(
				"JSON_RAW",
				2,
				new Function((arguments) =>
					JsonCollator.Compare (
						JsonCollationMode.Raw, 
						(string)arguments [0], 
						(string)arguments [1], 
						Int32.MaxValue)),
				true);

		}

		public override int GetVersion ()
		{
			var result = -1;

			try {
				using (var cmd = connection.Prepare ("PRAGMA user_version;")) {
					var stmt = cmd.Step ();

					if (cmd.Step () == SQLiteResult.ROW)
						result = (Int32)stmt [0];
				}
			} catch (Exception e) {
				Log.E (Tag, "Error getting user version", e);
			}

			return result;
		}

		public override void SetVersion (int version)
		{
			try {
				using (var cmd = connection.Prepare("PRAGMA user_version = " + version))
					cmd.Step ();
			} catch (Exception e) {
				Log.E(Tag, "Error getting user version", e);
			} 
		}

		public override void BeginTransaction ()
		{
			BeginTransaction (IsolationLevel.Default);
		}

		public override void BeginTransaction (IsolationLevel isolationLevel)
		{
			Interlocked.Increment(ref transactionCount);

			using (var stmt = connection.Prepare ("BEGIN TRANSACTION"))
				stmt.Step ();
		}

		public override void EndTransaction ()
		{
			if (connection == null)
				throw new InvalidOperationException("Database is not open.");

			if (Interlocked.Decrement(ref transactionCount) > 0)
				return;

//			if (!connection.IsInTransaction) {
//				if (shouldCommit)
//					throw new InvalidOperationException ("Transaction missing.");
//				return;
//			}

			if (shouldCommit) {
				using (var stmt = connection.Prepare ("COMMIT TRANSACTION"))
					stmt.Step ();
				shouldCommit = false;
			} else {
				using (var stmt = connection.Prepare ("ROLLBACK TRANSACTION"))
					stmt.Step ();		
			}
		}

		public override void SetTransactionSuccessful ()
		{
			shouldCommit = true;
		}

		public override void ExecSQL (string sql, params object[] paramArgs)
		{
			using (var stmt = connection.Prepare (sql)) {
				for (int i = 0; i < paramArgs.Length; i++){
					stmt.Bind (i, paramArgs [i]);
				}

				stmt.Step ();
			}
		}

		public override Cursor RawQuery (string sql, params object[] paramArgs)
		{
			using (var stmt = connection.Prepare (sql)) {
				for (int i = 0; i < paramArgs.Length; i++){
					stmt.Bind (i, paramArgs [i]);
				}

				return new Cursor (stmt);
			}
		}

		public override Cursor RawQuery (string sql, CommandBehavior behavior, params object[] paramArgs)
		{
			throw new NotImplementedException ();
		}

		public override long Insert (string table, string nullColumnHack, ContentValues values)
		{
			return InsertWithOnConflict(table, null, values, ConflictResolutionStrategy.None);
		}

		public override long InsertWithOnConflict (string table, string nullColumnHack, ContentValues initialValues, ConflictResolutionStrategy conflictResolutionStrategy)
		{
			throw new NotImplementedException ();
		}

		public override int Update (string table, ContentValues values, string whereClause, params string[] whereArgs)
		{
			throw new NotImplementedException ();
		}

		public override int Delete (string table, string whereClause, params string[] whereArgs)
		{
			throw new NotImplementedException ();
		}

		public override void Close ()
		{
			connection.Close ();
			connection.Dispose ();
			connection = null;
		}

		public override bool IsOpen {
			get {
				return connection != null;
			}
		}

		#endregion

		#region Non-public Members

		ISQLiteStatement BuildCommand (string sql, object[] paramArgs)
		{
			var stmt = connection.Prepare (sql.ReplacePositionalParams ());

			if (paramArgs != null && paramArgs.Length > 0) {
				for (int i = 0; i < paramArgs.Length; i++) {
					stmt.Bind ("@" + i, paramArgs [i]);
				}
			}

			return stmt;
		}

		/// <summary>
		/// Avoids the additional database trip that using SqliteCommandBuilder requires.
		/// </summary>
		/// <returns>The update command.</returns>
		/// <param name="table">Table.</param>
		/// <param name="values">Values.</param>
		/// <param name="whereClause">Where clause.</param>
		/// <param name="whereArgs">Where arguments.</param>
		ISQLiteStatement GetUpdateCommand (string table, ContentValues values, string whereClause, string[] whereArgs)
		{
			var builder = new StringBuilder("UPDATE ");

			builder.Append(table);
			builder.Append(" SET ");

			// Append our content column names and create our SQL parameters.
			var valueSet = values.ValueSet();
			var valueSetLength = valueSet.Count();

			var whereArgsLength = (whereArgs != null ? whereArgs.Length : 0);
			var sqlParams = new Dictionary<string, object>(valueSetLength + whereArgsLength);

			foreach(var column in valueSet)
			{
				if (sqlParams.Count > 0) {
					builder.Append(",");
				}
				builder.AppendFormat( "{0} = @{0}", column.Key);
				sqlParams.Add(column.Key, column.Value);
			}
				
			if (!whereClause.IsEmpty()) {
				builder.Append(" WHERE ");
				builder.Append(whereClause.ReplacePositionalParams());
			}

			if (whereArgsLength > 0) {
				for (int i = 0; i < whereArgs.Length; i++)
					sqlParams.Add ("@" + i, whereArgs [i]);
			}

			var sql = builder.ToString();
			var stmt = connection.Prepare (sql);
			foreach (var p in sqlParams)
				stmt.Bind (p.Key, p.Value);

			return stmt;
		}

		/// <summary>
		/// Avoids the additional database trip that using SqliteCommandBuilder requires.
		/// </summary>
		/// <returns>The insert command.</returns>
		/// <param name="table">Table.</param>
		/// <param name="values">Values.</param>
		/// <param name="conflictResolutionStrategy">Conflict resolution strategy.</param>
		ISQLiteStatement GetInsertCommand (String table, ContentValues values, ConflictResolutionStrategy conflictResolutionStrategy)
		{
			var builder = new StringBuilder("INSERT");

			if (conflictResolutionStrategy != ConflictResolutionStrategy.None) {
				builder.Append(" OR ");
				builder.Append(conflictResolutionStrategy);
			}

			builder.Append(" INTO ");
			builder.Append(table);
			builder.Append(" (");

			// Append our content column names and create our SQL parameters.
			var valueSet = values.ValueSet();
			var sqlParams = new Dictionary<string, object> (valueSet.LongCount ()); 
			var valueBuilder = new StringBuilder();
			var index = 0L;

			foreach(var column in valueSet)
			{
				if (index > 0) {
					builder.Append(",");
					valueBuilder.Append(",");
				}

				builder.AppendFormat( "{0}", column.Key);
				valueBuilder.AppendFormat("@{0}", column.Key);

				index++;
				sqlParams.Add (column.Key, column.Value);
			}

			builder.Append(") VALUES (");
			builder.Append(valueBuilder);
			builder.Append(")");

			var stmt = connection.Prepare (builder.ToString ());
			foreach (var p in sqlParams)
				stmt.Bind (p.Key, p.Value);

			return stmt;
		}

		/// <summary>
		/// Avoids the additional database trip that using SqliteCommandBuilder requires.
		/// </summary>
		/// <returns>The delete command.</returns>
		/// <param name="table">Table.</param>
		/// <param name="whereClause">Where clause.</param>
		/// <param name="whereArgs">Where arguments.</param>
		string GetDeleteCommand (string table, string whereClause, string[] whereArgs)
		{
			var builder = new StringBuilder("DELETE FROM ");

			builder.Append(table);

			if (!whereClause.IsEmpty()) {
				builder.Append(" WHERE ");
				builder.Append(whereClause.ReplacePositionalParams());
			}

			return builder.ToString ();
		}

		#endregion

		#region IDisposable implementation

		public void Dispose ()
		{
			if (connection != null) {
				connection.Dispose ();
				connection = null;
			}
		}

		#endregion
	}

	public enum IsolationLevel {
		Default
	}

	[Flags]
	public enum CommandBehavior
	{
		Default = 0,
		SingleResult = 1,
		SchemaOnly = 2,
		KeyInfo = 4,
		SingleRow = 8,
		SequentialAccess = 16,
		CloseConnection = 32
	}
}

