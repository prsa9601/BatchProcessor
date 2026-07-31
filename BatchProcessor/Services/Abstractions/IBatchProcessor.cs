namespace BatchProcessor.Services.Abstractions
{
    public interface IBatchProcessor<T>
    {
        void Add(T item);
        Task FlushAsync();
    }
}
