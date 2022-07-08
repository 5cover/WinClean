namespace Scover.WinClean.BusinessLogic;

public interface IScriptSerializer
{
    Script Deserialize(FileInfo source);

    void Serialize(Script s);
}