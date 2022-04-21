//******************************************************************************************************
//  Copyright © 2022, S. Christison. No Rights Reserved.
//
//  Licensed to [You] under one or more License Agreements.
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//
//******************************************************************************************************

using System.Collections.Generic;

namespace BTNET.BV.Abstract
{
    public class StoredListLong
    {
        public List<long> List
        {
            get;
            set;
        }

        public StoredListLong(List<long> list)
        {
            List = list;
        }
    }
}