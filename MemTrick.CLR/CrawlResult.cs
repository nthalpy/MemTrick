namespace MemTrick.CLR
{
    internal unsafe sealed class CrawlResult
    {
        public readonly void* Location;
        public readonly AssemblyPattern Pattern;
        
        public CrawlResult(void* location, AssemblyPattern pattern)
        {
            Location = location;
            Pattern = pattern;
        }
    }
}
