namespace StefanOssendorf.BulkMailing {
    /// <summary>
    /// Defines a method to create an <see cref="ISmtpClient"/>.
    /// </summary>
    public interface ISmtpClientFactory {
        /// <summary>
        /// Creates a new <see cref="ISmtpClient"/>.
        /// </summary>
        /// <returns></returns>
        ISmtpClient Create();
    }
}