internal interface IProducer
{
    Task ExecuteAsync(
        CompilationContext context,
        CancellationToken cancellationToken);
}