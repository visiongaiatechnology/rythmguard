using System.Reflection;
using VGT.RhythmGuard.Core.Tests;

namespace VGT.RhythmGuard.FactRunner;

/// <summary>
/// Minimaler, testhost-freier Runner für die xUnit-Fakten.
/// Hintergrund: In der aktuellen Ausführungsumgebung kann der VSTest-Testhost
/// wegen Prozesszugriffsrechten nicht starten (Win32Exception 5 auf OpenProcess).
/// Dieser Runner führt dieselben [Fact]/[Theory]-Methoden per Reflection aus und
/// liefert einen belastbaren Exitcode, ohne die Tests zu duplizieren.
/// </summary>
internal static class Program
{
    private static int Main()
    {
        var assembly = typeof(RhythmSettingsTests).Assembly;
        var types = assembly.GetTypes()
            .Where(t => t.Namespace == "VGT.RhythmGuard.Core.Tests" && t.IsClass && !t.IsAbstract)
            .OrderBy(t => t.FullName, StringComparer.Ordinal)
            .ToList();

        var passed = 0;
        var failed = 0;
        var skipped = 0;
        var failures = new List<string>();

        foreach (var type in types)
        {
            var testMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => HasAttribute(m, "FactAttribute") || HasAttribute(m, "TheoryAttribute"))
                .OrderBy(m => m.Name, StringComparer.Ordinal)
                .ToList();

            foreach (var method in testMethods)
            {
                var cases = GetCases(method);
                if (cases.Count == 0)
                {
                    skipped++;
                    Console.WriteLine($"SKIP {type.Name}.{method.Name}");
                    continue;
                }

                foreach (var args in cases)
                {
                    var label = args.Count == 0
                        ? $"{type.Name}.{method.Name}"
                        : $"{type.Name}.{method.Name}({string.Join(", ", args.Select(FormatArg))})";

                    try
                    {
                        var instance = Activator.CreateInstance(type);
                        var result = method.Invoke(instance, args.ToArray());
                        if (result is Task task)
                        {
                            task.GetAwaiter().GetResult();
                        }

                        passed++;
                        Console.WriteLine($"PASS {label}");
                    }
                    catch (TargetInvocationException ex)
                    {
                        failed++;
                        var root = ex.InnerException ?? ex;
                        var message = $"{root.GetType().Name}: {root.Message}";
                        failures.Add($"{label} -> {message}");
                        Console.WriteLine($"FAIL {label} -> {message}");
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        failures.Add($"{label} -> {ex.GetType().Name}: {ex.Message}");
                        Console.WriteLine($"FAIL {label} -> {ex.GetType().Name}: {ex.Message}");
                    }
                }
            }
        }

        Console.WriteLine();
        Console.WriteLine($"SUMMARY: executed={passed + failed}, passed={passed}, failed={failed}, skipped={skipped}");
        if (failures.Count > 0)
        {
            Console.WriteLine("FAILURES:");
            foreach (var failure in failures)
            {
                Console.WriteLine($"  - {failure}");
            }
        }

        return failed == 0 ? 0 : 1;
    }

    private static bool HasAttribute(MemberInfo member, string attributeName)
        => member.GetCustomAttributes(inherit: true).Any(a => a.GetType().Name == attributeName);

    private static List<List<object?>> GetCases(MethodInfo method)
    {
        var cases = new List<List<object?>>();
        var isTheory = HasAttribute(method, "TheoryAttribute");
        if (!isTheory)
        {
            cases.Add(new List<object?>());
            return cases;
        }

        foreach (var attribute in method.GetCustomAttributes(inherit: true))
        {
            if (attribute.GetType().Name != "InlineDataAttribute")
            {
                continue;
            }

            var getData = attribute.GetType().GetMethod("GetData", new[] { typeof(MethodInfo) });
            if (getData is null)
            {
                continue;
            }

            if (getData.Invoke(attribute, new object[] { method }) is not System.Collections.IEnumerable data)
            {
                continue;
            }

            foreach (var row in data)
            {
                if (row is object?[] values)
                {
                    cases.Add(values.ToList());
                }
            }
        }

        return cases;
    }

    private static string FormatArg(object? value) => value switch
    {
        null => "null",
        string text => $"\"{text}\"",
        _ => value.ToString() ?? "?",
    };
}
