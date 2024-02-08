using System.Dynamic;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;
public class WorkingPart
{
    Dictionary<string,string> ABtoKASKADAssociations = new() {
        {"INT", "int"},
        {"DINT", "int"},
        {"BIT", "bool"},
        {"BOOL", "bool"},
        {"SINT", "int"}
    };

    Dictionary<string, string> KaskadDTidMap = new() {
        {"char", "19"},
        {"uint", "20"},
        {"ulong", "58"},
        {"int", "21"},
        {"long", "54"},
        {"float", "22"},
        {"bool", "23"},
        {"bit32", "24"},
        {"bit64", "50"},
        {"string", "25"},
        {"time", "26"},
        {"dpid", "27"},
        {"langString", "42"},
        {"blob", "46"},
        {"folded", "41"}
    };

    IEnumerable<XElement> LoadMultipleUDTsFromXML(string filePath) 
    {
        var doc = XDocument.Load(filePath);
        var udts = doc
            ?.Element("RSLogix5000Content")
            ?.Element("Controller")
            ?.Element("DataTypes")
            ?.Descendants("DataType");

        foreach (var udt in udts)
        {
            yield return udt;
        }
    }
    IEnumerable<XElement> LoadUDTMembersFromUDT(XElement dataType, bool ignoreHidden = true)
    {
        // GETTING ALL UDT_ED_D MEMBERS
        var UDTMembers = dataType
            ?.Element("Members")
            ?.Descendants("Member");
        
        if (ignoreHidden)
            UDTMembers = UDTMembers.Where(m => m.Attribute("Hidden")?.Value != "true");

        foreach (var member in UDTMembers)
        {
            yield return member;
        }
    }
    /// <summary>
    /// [Legacy] works for docs with single UDT 
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    IEnumerable<XElement> LoadUDTMembersFromXML(string filePath)
    {
        var udt = XDocument.Load(filePath);

        // GETTING ALL UDT_ED_D MEMBERS
        var UDTMembers = udt
            ?.Element("RSLogix5000Content")
            ?.Element("Controller")
            ?.Element("DataTypes")
            ?.Element("DataType")
            ?.Element("Members")
            ?.Descendants("Member");
        
        foreach (var member in UDTMembers)
        {
            yield return member;
        }
    }

    string GetABToKaskadTypeId(string ABtype, out string folded)
    {
        if(ABtoKASKADAssociations.ContainsKey(ABtype))
        {
            string assoc = ABtoKASKADAssociations[ABtype];
            folded = string.Empty;
            return KaskadDTidMap[assoc];
        }
        else
        {
            folded = ABtype;
            return "41";
        }; // on fault we give nested custom type
    }

    string ParseXMLToKaskadUDT(XElement UDT, bool ignoreHidden = true)
    {
        int i = 1;
        string result = "";

        string UDTName = UDT.Attribute("Name")?.Value ?? "Undefined";
        IEnumerable<XElement> UDTMembers = LoadUDTMembersFromUDT(UDT, ignoreHidden);

        result += $"{UDTName}.{UDTName}\t1#{i}\n";

        foreach (var field in UDTMembers)//.Where(a => a.Name == "Member"))
        {
            i++;

            string ABDataType = field.Attribute("DataType")?.Value ?? "INT";
            string DTid = GetABToKaskadTypeId(ABDataType, out string folded);

            // Для вложенных UDT
            string folding = string.IsNullOrEmpty(folded) ? "" : $":{folded}";

            result += $"\t{field?.Attribute("Name")?.Value}\t{DTid}#{i}{folding}\n";    
        }

        return result;
    }
    string ParseXMLToKaskadSingleUDT(IEnumerable<XElement> UDTMembers, string UDTName)
    {
        int i = 1;
        string result = "";
        result += $"{UDTName}.{UDTName}\t1#{i}\n";

        foreach (var field in UDTMembers)//.Where(a => a.Name == "Member"))
        {
            i++;

            string ABDataType = field.Attribute("DataType")?.Value ?? "INT";
            string DTid = GetABToKaskadTypeId(ABDataType, out string folded);

            // Для вложенных UDT
            string folding = string.IsNullOrEmpty(folded) ? "" : $":{folded}";

            result += $"\t{field?.Attribute("Name")?.Value}\t{DTid}#{i}{folding}\n";    
        }

        return result;
    }

    public string Convert(List<string> input, bool ignoreHidden = true)
    {
        if (input.Count == 0)
        throw new Exception("Путей не подано");

        string kaskadFiller = "";
        kaskadFiller += "TypeName\n";

        // Выгрузить все UDT из всех файлов
        foreach (var path in input)
        {
            if (File.Exists(path))
                foreach (XElement UDT in LoadMultipleUDTsFromXML(path))
                    kaskadFiller += ParseXMLToKaskadUDT(UDT, ignoreHidden);
            else
            {
                throw new Exception($"Ошибка. Файл по пути {path} не существует");
            }
        }

        return kaskadFiller;
    }

    public string ConvertOld(List<string> input)
    {
        if (input.Count == 0)
        throw new Exception("Путей не подано");

        string kaskadFiller = "";
        kaskadFiller += "TypeName\n";

        foreach (var path in input)
        {
            if (File.Exists(path))
                kaskadFiller += ParseXMLToKaskadSingleUDT(LoadUDTMembersFromXML(path)
                                                ,Path.GetFileNameWithoutExtension(path));
            else
            {
                throw new Exception($"Ошибка. Файл по пути {path} не существует");
            }
        }

        return kaskadFiller;
    }
    
}