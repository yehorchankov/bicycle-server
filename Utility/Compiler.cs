using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using BiisControllers;
using Microsoft.CSharp;

namespace Utility
{
    /// <summary>
    /// Allows to compile handling code from .biis.code
    /// </summary>
    public static class Compiler
    {
        /// <summary>
        /// Executes a method that was tied to control and 
        /// modifying controls according to the method
        /// </summary>
        /// <param name="controls">Collection of controls</param>
        /// <param name="path">URI path</param>
        /// <param name="directory">Directory on disk</param>
        /// <returns>Collection of modified controls</returns>
        public static Dictionary<string, IBiisControl> ExecuteBiisCode(Dictionary<string, IBiisControl> controls, string path, string directory)
        {
            //Regex to find raw code from function
            Regex codeRegex = new Regex("[\\s\\S]*void[\\s\\S]*([\\w\\W]*)[\\s]*{[\\s]*(?<code>[\\s\\S]*)}[\\s]*");
            const string codePattern = 
            @"  using System;
                using BiisControllers;
                using System.Collections.Generic;

                namespace Utility {
                    public static class Bar{
                        public static void @method(System.Collections.Generic.Dictionary<string, IBiisControl> controls){
                            @code
                        }
                    }
                }";

            //Checks if there is button with handling function on it
            foreach (var biisControl in controls.Values
                .Where(biisControl => !string.IsNullOrEmpty(biisControl.HandlerFunction) &&
                                                    biisControl.GetType().Name == "Button"))
            {
                string code;
                using (var streamReader = new StreamReader(@".\" + directory + path + ".code"))
                    code = streamReader.ReadToEnd();
                //Extracting only method's body
                code = codeRegex.Match(code).Groups["code"].Value;

                string temp = new string(code.ToCharArray());
                //Replacing all calls by name of control from code to explicit calls from dictionary
                code = controls.Keys.Where(key => temp.Contains(key + "."))
                    .Aggregate(code, (current, key) => current
                        .Replace(key + ".", string.Format("controls[\"{0}\"].", key)));
                code = codePattern.Replace("@code", code);
                code = code.Replace("@method", biisControl.HandlerFunction);

                //Compiling code
                var compilingResults = Compile(code);

                if (!compilingResults.Errors.HasErrors)
                {
                    Type assemblyType = compilingResults.CompiledAssembly.GetType("Utility.Bar");
                    MethodInfo method = assemblyType.GetMethod(biisControl.HandlerFunction);
                    method.Invoke(null, new object[] { controls });
                }
                else
                {
                    var errorString = new StringBuilder();
                    foreach (CompilerError compilerError in compilingResults.Errors)
                        errorString.AppendFormat("Error in line {0}:\n\n{1}", compilerError.Line, compilerError.ErrorText);
                    Console.WriteLine(errorString);
                }
            }
            return controls;
        }

        /// <summary>
        /// Compiling input code
        /// </summary>
        /// <param name="code">C# code to compile</param>
        /// <returns>Compilation results</returns>
        private static CompilerResults Compile(string code)
        {
            CompilerResults results;
            CompilerParameters compilerParameters = new CompilerParameters
            {
                GenerateExecutable = false,
                GenerateInMemory = true,
                IncludeDebugInformation = false
            };
            compilerParameters.ReferencedAssemblies.Add("System.dll");
            compilerParameters.ReferencedAssemblies.Add("BiisControllers.dll");
            CodeSnippetCompileUnit compiledCode = new CodeSnippetCompileUnit(code);
            using (CSharpCodeProvider codeProvider = new CSharpCodeProvider())
                results = codeProvider.CompileAssemblyFromDom(compilerParameters, compiledCode);
            return results;
        }
    }
}
