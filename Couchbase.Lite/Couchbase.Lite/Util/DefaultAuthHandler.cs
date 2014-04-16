//
// DefaultAuthHandler.cs
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Couchbase.Lite;
using Couchbase.Lite.Replicator;
using Couchbase.Lite.Util;
using Sharpen;
using System.Linq;
using System.Net;
using System.Net.Http;
#if !PORTABLE
using System.Web;
using System.Web.UI;
#endif
using System.Threading;

namespace Couchbase.Lite.Replicator
{

    internal sealed class DefaultAuthHandler : MessageProcessingHandler
    {
        public DefaultAuthHandler(HttpClientHandler context) : base()
        {
            this.context = context;
        }

        #region implemented abstract members of MessageProcessingHandler

        protected override HttpResponseMessage ProcessResponse (HttpResponseMessage response, CancellationToken cancellationToken)
        {
            return response;
        }

        /// <exception cref="Org.Apache.Http.HttpException"></exception>
        /// <exception cref="System.IO.IOException"></exception>
        protected override HttpRequestMessage ProcessRequest(HttpRequestMessage request, CancellationToken token)
        {
//              AuthState authState = (AuthState)context.GetAttribute(ClientContext.TargetAuthState
//                  );
//              CredentialsProvider credsProvider = (CredentialsProvider)context.GetAttribute(ClientContext
//                  .CredsProvider);
//              HttpHost targetHost = (HttpHost)context.GetAttribute(ExecutionContext.HttpTargetHost
//                  );
//              if (authState.GetAuthScheme() == null)
//              {
//                  AuthScope authScope = new AuthScope(targetHost.GetHostName(), targetHost.GetPort(
//                      ));
//                  authState.SetAuthScheme(new BasicScheme());
//                  authState.SetCredentials(creds);
//              }

            if (!context.UseDefaultCredentials && (context.Credentials == null))
            {

                context.Credentials = request.ToCredentialsFromUri();
            }

            return request;
        }

        #endregion

        private IEnumerator GetEnumerator() 
        {
            #if PORTABLE
            //TODO: Figure out PCL Implementation
            return null;
            #else
            return AuthenticationManager.RegisteredModules; 
            #endif
        }

        private readonly HttpClientHandler context;
    }
}
