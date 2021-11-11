// ******************************************************************
// Copyright � 2015-2018 nventive inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// ******************************************************************
using System;
using System.Linq.Expressions;

namespace Uno.Expressions
{
    internal static class ExpressionFactory
    {
        public static Expression<Func<T, bool>> PropertyEqualConstant<T>(string propertyName, object value)
        {
            var parameter = Expression.Parameter(typeof (T), "item");

            var property = Expression.Property(parameter, propertyName);
            var constant = Expression.Constant(value);

            var body = Expression.Equal(property, constant);

            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }
    }
}