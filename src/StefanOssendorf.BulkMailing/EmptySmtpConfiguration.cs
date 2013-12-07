namespace StefanOssendorf.BulkMailing {
    internal class EmptySmtpConfiguration : SmtpConfiguration {
        internal override bool IsEmpty { get { return true; } }
    }
}