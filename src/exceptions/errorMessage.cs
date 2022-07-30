/*
	DataBridges C# client Library
	https://www.databridges.io/



	Copyright 2022 Optomate Technologies Private Limited.

	Licensed under the Apache License, Version 2.0 (the "License");
	you may not use this file except in compliance with the License.
	You may obtain a copy of the License at

	    http://www.apache.org/licenses/LICENSE-2.0

	Unless required by applicable law or agreed to in writing, software
	distributed under the License is distributed on an "AS IS" BASIS,
	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
	See the License for the specific language governing permissions and
	limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dBridges.exceptions
{ 
    public static class errorMessage
    {
        //version: 20220419
        public static readonly Dictionary<string, int[]> Lookup = new Dictionary<string, int[]>(){
            {"E001", new int[] {1,1}},
            {"E002", new int[] {1,2}},
            {"E004", new int[] {1,4}},
            {"E006", new int[] {1,5}},
            {"E008", new int[] {1,5}},
            {"E009", new int[] {1,7}},
            {"E010", new int[] {1,7}},
            {"E011", new int[] {4,8}},
            {"E012", new int[] {5,9}},
            {"E013", new int[] {5,10}},
            {"E014", new int[] {6,8}},
            {"E015", new int[] {6,11}},
            {"E024", new int[] {11,8}},
            {"E025", new int[] {11,11}},
            {"E026", new int[] {11,11}},
            {"E027", new int[] {11,14}},
            {"E028", new int[] {11,11}},
            {"E030", new int[] {12,16}},
            {"E033", new int[] {20,8}},
            {"E038", new int[] {20,24}},
            {"E039", new int[] {20,11}},
            {"E040", new int[] {3,3}},
            {"E041", new int[] {10,13}},
            {"E042", new int[] {20,19}},
            {"E048", new int[] {21,18}},
            {"E051", new int[] {21,18}},
            {"E052", new int[] {21,18}},
            {"E053", new int[] {21,8}},
            {"E054", new int[] {15,3}},
            {"E055", new int[] {22,3}},
            {"E058", new int[] {6,20}},
            {"E059", new int[] {6,20}},
            {"E060", new int[] {1,21}},
            {"E061", new int[] {24,22}},
            {"E062", new int[] {24,23}},
            {"E063", new int[] {1,8}},
            {"E064", new int[] {7,3}},
            {"E065", new int[] {14,3}},
            {"E066", new int[] {16,9}},
            {"E067", new int[] {16,10}},
            {"E068", new int[] {27,8}},
            {"E070", new int[] {13,3}},
            {"E076", new int[] {18,9}},
            {"E077", new int[] {18,10}},
            {"E079", new int[] {15,8}},
            {"E080", new int[] {15,25}},
            {"E082", new int[] {9,3}},
            {"E084", new int[] {28,3}},
            {"E088", new int[] {29,3}},
            {"E090", new int[] {30,8}},
            {"E091", new int[] {11,28}},
            {"E092", new int[] {30,28}},
            {"E093", new int[] {11,29}},
            {"E094", new int[] {30,30}},
            {"E095", new int[] {30,31}},
            {"E096", new int[] {12,40}},
            {"E097", new int[] {12,32}},
            {"E098", new int[] {12,8}},
            {"E099", new int[] {31,31}},
            {"E100", new int[] {31,40}},
            {"E101", new int[] {31,34}},
            {"E102", new int[] {31,8}},
            {"E103", new int[] {30,41}},
            {"E104", new int[] {21,36}},
            {"E105", new int[] {27,37}},
            {"E108", new int[] {23,38}},
            {"E109", new int[] {20,38}},
            {"E110", new int[] {32,39}},
            {"E111", new int[] {32,10}}
        };

    }
}
