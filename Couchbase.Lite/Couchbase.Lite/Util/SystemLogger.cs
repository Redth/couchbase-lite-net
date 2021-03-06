//
// SystemLogger.cs
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
using System.IO;
using Couchbase.Lite.Util;
using Sharpen;
using System.Diagnostics;
using System.Threading;

namespace Couchbase.Lite.Util
{
	public class SystemLogger : ILogger
	{
		public virtual void V(string tag, string msg)
		{
			#if PORTABLE
			Debug.WriteLine(tag + ": " + msg);
			#else
            try {
				Trace.WriteLine(tag + ": " + msg);
            } catch (ThreadInterruptedException ex) {
                // swallow.
            }
			#endif
		}

		public virtual void V(string tag, string msg, Exception tr)
		{
			#if PORTABLE
			Debug.WriteLine(tag + ": " + msg + "\n" + GetStackTraceString(tr));
			#else
			Trace.WriteLine(tag + ": " + msg + "\n" + GetStackTraceString(tr));
			#endif
		}

		public virtual void D(string tag, string msg)
		{
			Debug.WriteLine(tag + ": " + msg);
		}

		public virtual void D(string tag, string msg, Exception tr)
		{
			Debug.WriteLine(tag + ": " + msg + "\n" + GetStackTraceString(tr));
		}

		public virtual void I(string tag, string msg)
		{
			#if PORTABLE
			Debug.WriteLine(tag + ": " + msg);
			#else
			Trace.TraceInformation(tag + ": " + msg);
			#endif
		}

		public virtual void I(string tag, string msg, Exception tr)
		{
			#if PORTABLE
			Debug.WriteLine(tag + ": " + msg + "\n" + GetStackTraceString(tr));
			#else
			Trace.TraceInformation(tag + ": " + msg + "\n" + GetStackTraceString(tr));
			#endif
		}

		public virtual void W(string tag, string msg)
		{
            Debug.WriteLine("Warning:" + tag + ": " + msg);
		}

		public virtual void W(string tag, Exception tr)
		{
            Debug.WriteLine("Warning:" + tag + ": " + "\n" + GetStackTraceString(tr));
		}

		public virtual void W(string tag, string msg, Exception tr)
		{
            Debug.WriteLine("Warning:" + tag + ": " + msg + "\n" + GetStackTraceString(tr));
		}

		public virtual void E(string tag, string msg)
		{
			#if PORTABLE
			Debug.WriteLine(tag + ": " + msg);
			#else
			Trace.TraceError(tag + ": " + msg);
			#endif
		}

		public virtual void E(string tag, string msg, Exception tr)
		{
			#if PORTABLE
			Debug.WriteLine(tag + ": " + msg + "\n" + GetStackTraceString(tr));
			#else
			Trace.TraceError(tag + ": " + msg + "\n" + GetStackTraceString(tr));
			#endif
		}

		private static string GetStackTraceString(Exception tr)
		{
			if (tr == null)
			{
				#if PORTABLE
				return string.Empty;
				#else
                return Environment.StackTrace;
				#endif
			}
			StringWriter stringWriter = new StringWriter();
			PrintWriter printWriter = new PrintWriter(stringWriter);
            Runtime.PrintStackTrace(tr, printWriter);
			return stringWriter.ToString();
		}
	}
}
