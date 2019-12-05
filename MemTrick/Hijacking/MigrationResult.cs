namespace MemTrick.Hijacking
{
    public struct MigrationResult
    {
        public readonly int SrcOffset;
        public readonly int DstOffset;

        public MigrationResult(int srcOffset, int dstOffset)
        {
            SrcOffset = srcOffset;
            DstOffset = dstOffset;
        }
    }
}
