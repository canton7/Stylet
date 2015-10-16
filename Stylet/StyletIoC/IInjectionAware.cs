namespace StyletIoC
{
    /// <summary>
    /// Interface to be implemented by objects if they want to be notified when property injection has occurred
    /// </summary>
    public interface IInjectionAware
    {
        /// <summary>
        /// Called by StyletIoC when property injection has occurred
        /// </summary>
        void ParametersInjected();
    }
}
