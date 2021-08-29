namespace PassivePicasso.GameImporter.SN_Fixes
{
    public interface ISNFix
    {
        string GetTaskName();
        void Run();
    }
}
