BulkMailing
==
BulkMailing is a library to provide an easy way to send bulk mailings.<br/>
It's designed to support [producer consumer][blocking collection] scenarios or any other [lists][IEnumerable T] as well.

How to use
==
    var emailsToSend = new List<MailSenderMessage> {
        new MailSenderMessage(message: new MailMessage("from@from.de", "to@to.de"), userIdentifier: Guid.NewGuid())
		// and much more ;-)
    };
    
    using (var sender = new MailSender()) {
    	var sending = sender.StartSending(emailsToSend);
    
	    sending.HasError and sending.Exception
	    sending.IsFinished
	    sending.IsStopped true after calling sender.StopSending();
	    foreach (var sendResult in sending.Result.GetConsumingEnumerable()) {
		    sendResult.Canceled
		    sendResult.HasError & sendResult.Exception
		    sendResult.Successful
		    sendResult.MailMessage & sendResult.UserIdentifier
	    }
    }

- Create an MailSender() using the default configuration of SmtpClient. [`<system.net><mailSettings><smtp ...>`][smtpMsdn].
- Prepare your list of e-mails and call StartSending().
- Now you can check HasError, IsFinished or IsStopped if something happend to the background Task.
- Use Result Property to get the processed mails.
- Check the e-mail state via Canceled, HasError & Exception, Successful.

Suggestions are always welcome!

----
*For testing I'm using [mailtrap.io][mailtrap]. It's free and easy to use.<br/>
Written with [Notepag.es][notepag]*


[mailtrap]: http://mailtrap.io
[notepag]: http://notepag.es
[blocking collection]: http://msdn.microsoft.com/de-de/library/dd267312%28v=vs.110%29.aspx
[IEnumerable T]: http://msdn.microsoft.com/de-de/library/9eekhta0%28v=vs.110%29.aspx
[smtpMsdn]: http://msdn.microsoft.com/de-de/library/ms164240%28v=vs.110%29.aspx