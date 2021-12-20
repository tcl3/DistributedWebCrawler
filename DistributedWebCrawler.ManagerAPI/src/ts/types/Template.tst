﻿${
    // Enable extension methods by adding using Typewriter.Extensions.*
    using Typewriter.Extensions.Types;
    using System.Text.RegularExpressions;
    using System.Collections;
    using System.Text;

    Template(Settings settings)
    {
        settings.OutputExtension = ".ts";
    }

    // debug trick, "printf" lives!! https://github.com/frhagn/Typewriter/issues/121#issuecomment-231323983
    // concatenate anything you need into this debugInfo variable from your custom methods
    // and then throw $PrintDebugInfo into the main template space below to view in the output window
    static string debugInfo = "";
    string PrintDebugInfo(File f) => debugInfo; 

    static string[] Namespaces => new[] 
    {
        "DistributedWebCrawler.ManagerAPI.Models",
        "DistributedWebCrawler.Core.Models",
        "DistributedWebCrawler.Core.Enums"
    };

    // nested class support: https://github.com/frhagn/Typewriter/issues/134#issuecomment-253771122
    static List<Class> _classesInFile = new List<Class>();
    static List<Enum> _enumsInFile = new List<Enum>();

    bool ClassFilter(Class c) => (Namespaces.Any(c.Namespace.StartsWith));

    bool EnumFilter(Enum e) => (Namespaces.Any(e.Namespace.StartsWith));

    IEnumerable<Class> ClassesInFile(File f) {
        _classesInFile = f.Classes.Where(ClassFilter).Concat(f.Classes.SelectMany(c => c.NestedClasses).Where(ClassFilter)).ToList();
        return _classesInFile;
    }

    IEnumerable<Enum> EnumsInFile(File f) {
        _enumsInFile = f.Enums.Where(EnumFilter).ToList();
        return _enumsInFile;
    }

    string ClassName(Class c) {
        return c.Name + (c.BaseClass != null ? " extends " + c.BaseClass.Name : "");
    }

    string ImportClass(Class c){
        var imports = new List<string>();

        foreach (var p in c.Properties) {
            var types = TypeAndGenericTypes(p.Type);
            foreach (var type in types) {
                var importType = ImportType(type);
                if (!string.IsNullOrEmpty(importType)) {
                    imports.Add(importType);
                }
            }
        }

        if (c.BaseClass != null) { 
            imports.Add("import { " + c.BaseClass.Name +" } from \"./" + c.BaseClass.Name + "\";");
            imports.Add(ImportClass(c.BaseClass));
        } else{
            // no import needed
        }

        return String.Join("\n", imports.Distinct());
    }

    static string[] ExcludeImportTypes = new[] { "any", "Uri" };

    string ImportType(Type t) {
        var isExcluded = ExcludeImportTypes.Contains(t.ClassName());
        
        if (!isExcluded && (!t.IsPrimitive || t.IsEnum)) {
            var typeName = t.Name;
            if (t.IsEnum) {
                if (_enumsInFile.Any(o => o.Name == typeName)) {
                    return "";
                }
            } else {
                typeName = t.IsEnumerable ? typeName.Replace("[]","") : typeName;
                if (_classesInFile.Any(o => o.Name == typeName)) {
                    return "";
                }
            }
            return "import { " + typeName.TrimEnd('[',']') + " } from \"./" + typeName.TrimEnd('[',']') + "\";";
        }

        return "";
    }

    string Imports(File f) {
        var classesInFile = ClassesInFile(f);
        var enumsInFile = EnumsInFile(f);

        List<string> imports = classesInFile.Select(c => ImportClass(c)).ToList();

        return String.Join("", imports.Distinct());
    }

    string TypeConverter(Property p) {
        if (p.Type.Name == "Uri") {
            return "string";
        }
        var result = p.Type.Name;
        if (p.Attributes.Any(a => a.Name == "Nullable") || p.Type.IsNullable) {
            result += " | null";
        }
        return result;
    }

    IEnumerable<Type> TypeAndGenericTypes(Type t) {
        var results = new List<Type>();
        results.Add(t);

        if (t.IsGeneric || t.IsEnumerable){
            foreach(var subtypes in t.TypeArguments){
                 results.AddRange(TypeAndGenericTypes(subtypes));
            }
        }
        return results;
    }

    string TypeDefault(Property p) {
        if (p.Attributes.Any(a => a.Name == "Nullable") || p.Type.IsNullable) {
            return " = null";
        }
        if (p.Type.IsEnum) {
            return $" = {p.Type.Name}.{p.Type.Constants[0].Name}";
        }
        if (p.Type.IsGuid) {
            return " = \"00000000-0000-0000-0000-000000000000\"";
        }
        if (p.Type.Name == "string") {
            return " = \"\"";
        }
        if (p.Type.IsEnumerable && !p.Type.IsGeneric) {
            return " = []";
        }
        if (p.Type.Default() == "null" && p.Attributes.Any(a => a.Name == "NotNullable")) {
            return "";
        }

        return " = " + p.Type.Default();
    }}$Imports$Classes(c => ClassFilter(c) || c.NestedClasses.Any(ClassFilter))[]$ClassesInFile[

// This file was generated by TypeWriter. Do not modify.
export interface $ClassName {
    $Properties[
    $name: $TypeConverter;]
}]$Enums(c => EnumFilter(c))[

// This file was generated by TypeWriter. Do not modify.
export const enum $Name {
    $Values[$Name = $Value][,
    ]
}]