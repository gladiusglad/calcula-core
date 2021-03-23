namespace CalculaCore
{
    /// <summary>
    /// A mathematical symbol or operation which can be resolved to a <c>decimal</c> with <see cref="Resolve">Resolve()</see>.
    /// </summary>
    public interface IOperation
    {
        decimal Resolve();
    }
}
