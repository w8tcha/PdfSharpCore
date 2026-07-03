using System.Linq;
using System.Reflection;

namespace MigraDocCore.Rendering.UnitTest;

/// <summary>
/// Summary description for ValueDumper.
/// </summary>
internal class ValueDumper
{
    internal ValueDumper()
    {
    }

    internal static string DumpValues(object obj)
    {
        var dumpString = "[" + obj.GetType() + "]\r\n";
        return obj.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(fieldInfo => fieldInfo.FieldType.GetTypeInfo().IsValueType).Aggregate(dumpString,
                (current, fieldInfo) =>
                    current + ("  " + fieldInfo.Name + " = " + fieldInfo.GetValue(obj) + "\r\n"));

    }
}