#region  License
/*
Copyright 2017 - Jaap Lamfers - jlamfers@xipton.net

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
 * */
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xioc.Core;
using XPression.Core;
using XPression.Core.Functions;
using XPression.Language.Syntax;
using XPression.Core.Tokens;

namespace Xioc.Config
{
   internal class ConfigScriptExtender : DefaultSyntaxExtender
   {

      public override void ExtendSyntax(ISyntax syntax)
      {
         ConfigScriptContext.Syntax = syntax;

         foreach (var type in AppDomain.CurrentDomain.GetExportedTypes().Where(t => !t.IsAbstract && typeof(IXiocConfigExtender).IsAssignableFrom(t)))
         {
            var tuples = new List<Tuple<string, MemberInfo>>();
            Activator.CreateInstance(type).CastTo<IXiocConfigExtender>().ExtendSyntax(tuples);
            foreach (var t in tuples)
            {
               syntax.Functions.Add(new FunctionMap(t.Item1, t.Item2));
            }
         }

         var symbols = syntax.Symbols;

         foreach (var m in typeof(ConfigScriptContext).GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(m => Attribute.IsDefined(m, typeof(ScriptMethodAttribute))))
         {
            syntax.Functions.Add(new FunctionMap(m.GetCustomAttribute<ScriptMethodAttribute>().Name, m));
         }


         syntax.NonBreakingIdentifierChars.Add('-');
         syntax.NonBreakingIdentifierChars.Add('|');

         syntax.SyntaxChars.Remove('-');
         syntax.SyntaxChars.Remove('|');
         syntax.SyntaxChars.Add('-', TokenType.UnKnown); // because it is contained in '->'

         symbols.Add("sub", TokenType.Sub); // replace substract '-' by operator named 'sub' (would we need it?)
         symbols.Add("bw-or", TokenType.BitwiseOr); // replace bitwise or '|' by operator named 'bw-or' (again would we need it?)

         symbols.Add("->", TokenType.FunctionalBinaryOperator); // bind operator
         symbols.Add("<+", TokenType.FunctionalBinaryOperator); // decorate operator
         symbols.Add("<!", TokenType.FunctionalBinaryOperator); // intercept operator
         symbols.Add("as", TokenType.FunctionalBinaryOperator);
         symbols.Add("with", TokenType.FunctionalBinaryOperator);
         symbols.Add("using", TokenType.FunctionalUnaryOperator);


         var functions = syntax.Functions;
         functions.Add(new FunctionMap("->", GetMethodInfo(x => x.Bind(null, null))));
         functions.Add(new FunctionMap("->", GetMethodInfo(x => x._Bind(null, (Tuple<Type, string>)null))));// tuple overload
         functions.Add(new FunctionMap("->", GetMethodInfo(x => x._Bind(null, (Tuple<Type, Tuple<string, IDictionary<string, object>>>)null))));// tuple overload
         functions.Add(new FunctionMap("->", GetMethodInfo(x => x._Bind(null, (Tuple<string, IDictionary<string, object>>)null))));// tuple overload

         functions.Add(new FunctionMap("<+", GetMethodInfo(x => x.Decorate(null, null))));
         functions.Add(new FunctionMap("<+", GetMethodInfo(x => x._Decorate(null, (Tuple<Type, string>)null))));// tuple overload
         functions.Add(new FunctionMap("<+", GetMethodInfo(x => x._Decorate(null, (Tuple<Type, Tuple<string, IDictionary<string, object>>>)null))));// tuple overload
         functions.Add(new FunctionMap("<+", GetMethodInfo(x => x._Decorate(null, (Tuple<string, IDictionary<string, object>>)null))));// tuple overload

         functions.Add(new FunctionMap("<!", GetMethodInfo(x => x.Intercept(null, null))));

         functions.Add(new FunctionMap("as", GetMethodInfo(x => x._As(null, (string)null))));
         functions.Add(new FunctionMap("as", GetMethodInfo(x => x._As(null, (Tuple<string, IDictionary<string, object>>)null))));

         functions.Add(new FunctionMap("with", GetMethodInfo(x => x._WithDependencies(null, null))));

         functions.Add(new FunctionMap("using", GetMethodInfo(x => x._Using(null))));
      }

      private MethodInfo GetMethodInfo(Expression<Func<ConfigScriptContext, object>> expression)
      {
         return expression.GetMethodInfo();
      }

   }
}
