using System;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Reflection;
using System.IO;
using System.Linq;

class Program
{
    public static void Main(string[] args)
    {

        // intentionally introduce errors to see the compiler reporting code issues
        const string code = @"using System; 
using System.IO; 

namespace MathFunctions 
{ 
 public static class MathHelper
 { 
    public static void CalculateCircleArea() 
    { 
        double radius = 10;
        double result = radius * radius * System.Math.PI;
        Console.WriteLine(result.ToString()); 
    } 
  } 
}";

        // Get a SyntaxTree
        var tree = SyntaxFactory.ParseSyntaxTree(code);

        Console.WriteLine(tree);
        PrintDiagnostics(tree);

        // Create a compilation for the syntax tree
        var compilation = CSharpCompilation.Create("mylib.dll").
            WithOptions(
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)).
            AddReferences(MetadataReference.CreateFromFile(typeof(object).
            GetTypeInfo().Assembly.Location)).
            AddSyntaxTrees(tree);

        var fileName = "mylib.dll";
        var path = Path.Combine(Directory.GetCurrentDirectory(), fileName);

        // Emit an Assembly that contains the result of the Roslyn code generation
        compilation.Emit(path);

        // Use reflection to load and execute code
        var asm = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
        asm.GetType("MathFunctions.MathHelper").GetMethod("CalculateCircleArea").Invoke(null, new object[]{});
        Console.ReadLine();
    }

    private static void PrintDiagnostics(SyntaxTree tree)
    {
        // detects diagnostics in the source code
        var diagnostics = tree.GetDiagnostics();

        if (diagnostics.Any())
        {
            foreach (var diag in diagnostics)
            {
                // if any, prints diagnostic message and line/row position
                Console.WriteLine($"{diag.GetMessage()} {diag.Location.GetLineSpan()}");
            }
        }
    }
}
