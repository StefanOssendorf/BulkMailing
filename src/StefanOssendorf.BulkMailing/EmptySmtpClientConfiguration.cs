namespace StefanOssendorf.BulkMailing {
    internal class EmptySmtpClientConfiguration : SmtpClientConfiguration {
        internal override bool IsEmpty { get { return true; } }
        public EmptySmtpClientConfiguration() {
            EnableSsl = false;
            Host = null;
            Password = null;
            Port = 0;
            UseDefaultCredentials = false;
            UserName = null;
        }
        #region Overrides of SmtpClientConfiguration
        internal override SmtpClientConfiguration Clone() {
            return new EmptySmtpClientConfiguration();
        }
        #endregion
    }
}