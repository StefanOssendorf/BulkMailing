namespace StefanOssendorf.BulkMailing {
    internal class EmptySmtpConfiguration : SmtpConfiguration {
        internal override bool IsEmpty { get { return true; } }
        #region Overrides of SmtpConfiguration
        internal override SmtpConfiguration Clone() {
            return new EmptySmtpConfiguration();
        }
        #endregion
    }
}