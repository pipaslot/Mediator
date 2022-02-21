namespace Demo.Shared
{
    /// <summary>
    /// Simple exaaple how validations can be involved in Mediator actions
    /// THIS IS NOT A GOOD PRACISE TO JOIN THE MEDIATOR ACTION AND VALIDATOR TOGETHER. 
    /// </summary>
    public interface IValidable
    {
        /// <summary>
        /// Validate object and return error message if error occures
        /// </summary>
        string[] Validate();
    }
}
