namespace Aeon.DiskImages.Archives
{
    public interface IArchiveBuilderProgress
    {
        void ItemStart(int index, string name, long size);
        void ItemDataProcessed(long completed);
        void ItemComplete(long outputSize, bool compressed);
    }
}
