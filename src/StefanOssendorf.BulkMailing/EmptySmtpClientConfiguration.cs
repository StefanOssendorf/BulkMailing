namespace StefanOssendorf.BulkMailing {
    internal class EmptySmtpClientConfiguration : SmtpClientConfiguration {
        internal override bool IsEmpty { get { return true; } }
        #region Overrides of SmtpClientConfiguration
        internal override SmtpClientConfiguration Clone() {
            return new EmptySmtpClientConfiguration();
        }
        #endregion
    }
}