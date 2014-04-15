//
// Cursor.cs
//
// Author:
//	Zachary Gramana  <zack@xamarin.com>
//
// Copyright (c) 2013, 2014 Xamarin Inc (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
/**
* Original iOS version by Jens Alfke
* Ported to Android by Marty Schoch, Traun Leyden
*
* Copyright (c) 2012, 2013, 2014 Couchbase, Inc. All rights reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file
* except in compliance with the License. You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software distributed under the
* License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
* either express or implied. See the License for the specific language governing permissions
* and limitations under the License.
*/
using System;
//using System.Data;
//using System.ComponentModel.Design;
using System.Collections.Generic;
//using Mono.Data.Sqlite;
using System.Linq;
using System.Text;
using SQLitePCL;

namespace Couchbase.Lite
{
	public class Cursor : IDisposable
	{
		const Int32 DefaultChunkSize = 8192;

		ISQLiteStatement statement;
		SQLiteResult stepResult;

		Boolean HasRows {
			get {
				return hasRows;
			}
		}

		bool hasRows = false;
		Int64 currentRow;

		public Cursor (ISQLiteStatement statement)
		{
			this.statement = statement;
			currentRow = -1;

			hasRows = statement.Step () == SQLiteResult.ROW;
			statement.Reset ();
		}

		public bool MoveToNext ()
		{
			stepResult = statement.Step ();

			currentRow++;

			return stepResult == SQLiteResult.ROW;
		}

		public int GetInt (int columnIndex)
		{
			try { 
				return (int)statement[columnIndex];
			} catch { 
				return -1;
			}
		}

		public long GetLong (int columnIndex)
		{
			try { 
				return (long)statement[columnIndex];
			} catch { 
				return -1;
			}
		}

		public string GetString (int columnIndex)
		{
			try { 
				return (string)statement[columnIndex];
			} catch { 
				return null;
			}
		}

		public byte[] GetBlob (int columnIndex)
		{
			return GetBlob(columnIndex, DefaultChunkSize);
		}

		public byte[] GetBlob (int columnIndex, int chunkSize)
		{
			if (statement [columnIndex] == null)
				return new byte[2];

			var str = statement [columnIndex] as string;
			if (str != null) {
				return Encoding.UTF8.GetBytes (str);
			}

			return stepResult [columnIndex] as byte[];

//			var chunkBuffer = new byte[chunkSize];
//			var blob = new List<Byte>(chunkSize); // We know we'll be reading at least 1 chunk, so pre-allocate now to avoid an immediate resize.
//
//			long bytesRead;
//			do
//			{
//				chunkBuffer.Initialize(); // Resets all values back to zero.
//				bytesRead = r.GetBytes(columnIndex, blob.Count, chunkBuffer, 0, chunkSize);
//				blob.AddRange(chunkBuffer.Take(Convert.ToInt32(bytesRead)));
//			} while (bytesRead > 0);
//
//			return blob.ToArray();
		}

		public void Close ()
		{
		}

		public bool IsAfterLast ()
		{
			return !HasRows;
		}

		#region IDisposable implementation

		public void Dispose ()
		{
			if (statement != null)
				statement.Dispose ();
		}

		#endregion
	}
}

