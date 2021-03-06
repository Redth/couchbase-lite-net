//
// MultipartReader.cs
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
using System.Collections.Generic;
using System.Text;
using Couchbase.Lite.Support;
using Sharpen;
using System.Collections.ObjectModel;

namespace Couchbase.Lite.Support
{
	public class MultipartReader
	{
		private enum MultipartReaderState
		{
			kUninitialized,
			kAtStart,
			kInPrologue,
			kInBody,
			kInHeaders,
			kAtEnd,
			kFailed
		}

		private static readonly Encoding utf8 = Extensions.GetEncoding("UTF-8");

        private static readonly Byte[] kCRLFCRLF;

        static MultipartReader()
        {
            kCRLFCRLF = utf8.GetBytes("\r\n\r\n");
        }

		private MultipartReader.MultipartReaderState state;

        private List<Byte> buffer;

        private readonly String contentType;

		private byte[] boundary;

        private IMultipartReaderDelegate readerDelegate;

        public IDictionary<String, String> headers;

        public MultipartReader(string contentType, IMultipartReaderDelegate readerDelegate)
		{
            this.contentType = contentType;
            this.readerDelegate = readerDelegate;
            this.buffer = new List<Byte>(1024);
			this.state = MultipartReader.MultipartReaderState.kAtStart;
			ParseContentType();
		}

		public byte[] GetBoundary()
		{
			return boundary;
		}

		public byte[] GetBoundaryWithoutLeadingCRLF()
		{
            var rawBoundary = GetBoundary();
            var result = new ArraySegment<Byte>(rawBoundary, 2, rawBoundary.Length);
            return result.Array;
		}

		public bool Finished()
		{
			return state == MultipartReader.MultipartReaderState.kAtEnd;
		}

        private static Byte[] EOMBytes()
		{
            return Encoding.UTF8.GetBytes("--");
		}

		private bool Memcmp(byte[] array1, byte[] array2, int len)
		{
			bool equals = true;
			for (int i = 0; i < len; i++)
			{
				if (array1[i] != array2[i])
				{
					equals = false;
				}
			}
			return equals;
		}

		public Range SearchFor(byte[] pattern, int start)
		{
            var searcher = new KMPMatch();
            var matchIndex = searcher.IndexOf(buffer.ToArray(), pattern, start);
			return matchIndex != -1 
                ? new Range (matchIndex, pattern.Length) 
                    : new Range (matchIndex, 0);
		}

		public void ParseHeaders(string headersStr)
		{
            headers = new Dictionary<String, String>();
            if (!string.IsNullOrEmpty (headersStr)) {
                headersStr = headersStr.Trim ();
                var tokenizer = headersStr.Split(new[] { "\r\n" }, StringSplitOptions.None);
                foreach (var header in tokenizer) {
                    if (!header.Contains (":")) {
                        throw new ArgumentException ("Missing ':' in header line: " + header);
                    }
                    var headerTokenizer = header.Split(':');
                    var key = headerTokenizer[0].Trim ();
                    var value = headerTokenizer[1].Trim ();
                    headers.Put (key, value);
                }
            }
		}

		private void DeleteUpThrough(int location)
		{
			// int start = location + 1;  // start at the first byte after the location
            var newBuffer = new ArraySegment<Byte>(buffer.ToArray(), location, buffer.Count);
			buffer.Clear();
            buffer.AddRange(newBuffer);
		}

		private void TrimBuffer()
		{
            int bufLen = buffer.Count;
			int boundaryLen = GetBoundary().Length;
			if (bufLen > boundaryLen)
			{
				// Leave enough bytes in _buffer that we can find an incomplete boundary string
                var dataToAppend = new ArraySegment<Byte>(buffer.ToArray(), 0,  bufLen - boundaryLen).Array;
                buffer.Clear();
				readerDelegate.AppendToPart(dataToAppend);
				DeleteUpThrough(bufLen - boundaryLen);
			}
		}

		public void AppendData(byte[] data)
		{
			if (buffer == null)
			{
				return;
			}
			
            if (data.Length == 0)
			{
				return;
			}

            buffer.AddRange(data);

			MultipartReader.MultipartReaderState nextState;
			do
			{
				nextState = MultipartReader.MultipartReaderState.kUninitialized;
                var bufLen = buffer.Count;
				switch (state)
				{
					case MultipartReader.MultipartReaderState.kAtStart:
					{
						// Log.d(Database.TAG, "appendData.  bufLen: " + bufLen);
						// The entire message might start with a boundary without a leading CRLF.
                        var boundaryWithoutLeadingCRLF = GetBoundaryWithoutLeadingCRLF();
						if (bufLen >= boundaryWithoutLeadingCRLF.Length)
						{
							// if (Arrays.equals(buffer.toByteArray(), boundaryWithoutLeadingCRLF)) {
							if (Memcmp(buffer.ToArray(), boundaryWithoutLeadingCRLF, boundaryWithoutLeadingCRLF.Length))
							{
								DeleteUpThrough(boundaryWithoutLeadingCRLF.Length);
								nextState = MultipartReader.MultipartReaderState.kInHeaders;
							}
							else
							{
								nextState = MultipartReader.MultipartReaderState.kInPrologue;
							}
						}
						break;
					}
					case MultipartReader.MultipartReaderState.kInPrologue:
					case MultipartReader.MultipartReaderState.kInBody:
					{
						// Look for the next part boundary in the data we just added and the ending bytes of
						// the previous data (in case the boundary string is split across calls)
						if (bufLen < boundary.Length)
						{
							break;
						}

                        var start = Math.Max(0, bufLen - data.Length - boundary.Length);
                        var r = SearchFor(boundary, start);

						if (r.GetLength() > 0)
						{
							if (state == MultipartReader.MultipartReaderState.kInBody)
							{
                                var dataToAppend = new ArraySegment<Byte>(buffer.ToArray(), 0, r.GetLocation());
								readerDelegate.AppendToPart(dataToAppend);
								readerDelegate.FinishedPart();
							}
							DeleteUpThrough(r.GetLocation() + r.GetLength());
							nextState = MultipartReader.MultipartReaderState.kInHeaders;
						}
						else
						{
							TrimBuffer();
						}
						break;
					}

					case MultipartReader.MultipartReaderState.kInHeaders:
					{
						// First check for the end-of-message string ("--" after separator):
						if (bufLen >= 2 && Memcmp(buffer.ToArray(), EOMBytes(), 2))
						{
							state = MultipartReader.MultipartReaderState.kAtEnd;
							Close();
							return;
						}
						// Otherwise look for two CRLFs that delimit the end of the headers:
                        var r = SearchFor(kCRLFCRLF, 0);
						if (r.GetLength() > 0)
						{
                            var headersBytes = Arrays.CopyTo(buffer.ToArray(), r.GetLocation()); // 
                            // var headersBytes = new ArraySegment<Byte>(buffer.ToArray(), 0, r.GetLocation()); // <-- better?
							var headersString = utf8.GetString(headersBytes, 0, headersBytes.Length);
							ParseHeaders(headersString);
							DeleteUpThrough(r.GetLocation() + r.GetLength());
							readerDelegate.StartedPart(headers);
							nextState = MultipartReader.MultipartReaderState.kInBody;
						}
						break;
					}

					default:
					{
						throw new InvalidOperationException("Unexpected data after end of MIME body");
					}
				}
				if (nextState != MultipartReader.MultipartReaderState.kUninitialized)
				{
					state = nextState;
				}
			}
            while (nextState != MultipartReader.MultipartReaderState.kUninitialized && buffer.Count > 0);
		}

		private void Close()
		{
			buffer = null;
			boundary = null;
		}

		private void ParseContentType()
		{
            var tokenizer = contentType.Split(';');
			bool first = true;
            foreach (var token in tokenizer)
			{
				string param = token.Trim();
				if (first)
				{
					if (!param.StartsWith("multipart/", StringCompare.IgnoreCase))
					{
						throw new ArgumentException(contentType + " does not start with multipart/");
					}
					first = false;
				}
				else
				{
					if (param.StartsWith("boundary=", StringCompare.IgnoreCase))
					{
                        var tempBoundary = param.Substring(9);
						if (tempBoundary.StartsWith ("\"", StringCompare.IgnoreCase)) 
                        {
							if (tempBoundary.Length < 2 || !tempBoundary.EndsWith ("\"", StringCompare.IgnoreCase)) {
                                throw new ArgumentException (contentType + " is not valid");
                            }
                            tempBoundary = tempBoundary.Substring(1, tempBoundary.Length - 1);
                        }
						if (tempBoundary.Length < 1)
						{
							throw new ArgumentException(contentType + " has zero-length boundary");
						}
                        tempBoundary = string.Format("\r\n--{0}", tempBoundary);
                        boundary = Encoding.UTF8.GetBytes(tempBoundary);
						break;
					}
				}
			}
		}
	}

	/// <summary>Knuth-Morris-Pratt Algorithm for Pattern Matching</summary>
	internal class KMPMatch
	{
		/// <summary>Finds the first occurrence of the pattern in the text.</summary>
		/// <remarks>Finds the first occurrence of the pattern in the text.</remarks>
		public int IndexOf(byte[] data, byte[] pattern, int dataOffset)
		{
			int[] failure = ComputeFailure(pattern);
			int j = 0;
			if (data.Length == 0)
			{
				return -1;
			}
			int dataLength = data.Length;
			int patternLength = pattern.Length;
			for (int i = dataOffset; i < dataLength; i++)
			{
				while (j > 0 && pattern[j] != data[i])
				{
					j = failure[j - 1];
				}
				if (pattern[j] == data[i])
				{
					j++;
				}
				if (j == patternLength)
				{
					return i - patternLength + 1;
				}
			}
			return -1;
		}

		/// <summary>
		/// Computes the failure function using a boot-strapping process,
		/// where the pattern is matched against itself.
		/// </summary>
		/// <remarks>
		/// Computes the failure function using a boot-strapping process,
		/// where the pattern is matched against itself.
		/// </remarks>
		private int[] ComputeFailure(byte[] pattern)
		{
			int[] failure = new int[pattern.Length];
			int j = 0;
			for (int i = 1; i < pattern.Length; i++)
			{
				while (j > 0 && pattern[j] != pattern[i])
				{
					j = failure[j - 1];
				}
				if (pattern[j] == pattern[i])
				{
					j++;
				}
				failure[i] = j;
			}
			return failure;
		}
	}
}
