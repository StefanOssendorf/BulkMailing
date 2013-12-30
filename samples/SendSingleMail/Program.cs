using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using StefanOssendorf.BulkMailing;

namespace SendSingleMail {
    /*
     * Create an app.config and set the 
     * <system.net>
     *      <mailSettings>
     *          <smtp>
     *              <network defaultCredentials="false" host="<host>" port="<port if not default,otherwise remove this attribute>" userName="<username>" password="<password>"/>
     *          </smtp>
     *      </mailSettings>
     *  </system.net>
     * or use the 
     * CreateSmtpConfiguration() Method to configure your smtp.
     * 
     * http://mailtrap.io/ for "Safe email delivery testing"
     */

    class Program {
        #region [ Helper methods ]
        private static MailSender CreateMailSender() {
            var smtpConfig = CreateSmtpConfiguration();
            if (smtpConfig != null) {
                return new MailSender(smtpConfig);
            }
            return new MailSender();
        }

        private static SmtpConfiguration CreateSmtpConfiguration() {
            return null;
            var smtpConfig = new SmtpConfiguration() {
                Host = "Host",
                UserName = "UserName",
                Port = 25,
                Password = "Password"
            };
            return smtpConfig;
        }
        #endregion

        static void Main(string[] args) {
            SendSingleMailAsyncExample();
            SendBulkMailExample();
            SendStreamMailExample();
        }

        private static void SendSingleMailAsyncExample() {
            using (var sender = CreateMailSender()) {
                var message = new MailMessage("from1@domain.de", "to1@domain.de") {
                    Body = "Das hier ist ein toller Plain Body",
                    Subject = "Ganz toller Subject"
                };
                var t1 = sender.SendAsync(message, "UniqueIdentifier");

                // If you want to cancel the send
                // sender.SendAsyncCancel();

                var result = t1.Result;
                Console.WriteLine("Successful: {0}", result.Successful);
                Console.WriteLine("Canceled: {0}", result.Canceled);
                Console.WriteLine("Exception: {0}", result.Exception);
                Console.ReadLine();
            }
        }

        private static void SendBulkMailExample() {
            using (var sender = CreateMailSender()) {
                var list = new List<MailSenderMessage>();

                for (int i = 0; i < 100; i++) {
                    string to = string.Format("to{0}@domain.de", i);
                    string from = string.Format("from{0}@domain.de", i);
                    string subject = string.Format("Subject {0}", i);
                    string body = string.Format("Body {0}", i);
                    list.Add(new MailSenderMessage() {
                        Message = new MailMessage(from, to, subject, body),
                        UserIdentifier = i
                    });
                }

                var t1 = sender.SendAsync(list);

                Console.WriteLine("Press Enter to cancel...");
                Console.ReadLine();
                if (!t1.IsCompleted) {
                    sender.SendAsyncCancel();
                }

                var result = t1.Result;
                foreach (var sendResult in result) {
                    Console.Write("Identifier:{0},Successful:{1},Canceled:{2},Exception:{3}", sendResult.UserIdentifier, sendResult.Successful, sendResult.Canceled, sendResult.Exception);
                    Console.WriteLine();
                }
            }
        }

        private static void SendStreamMailExample() {
            var cancellationTokenSource = new CancellationTokenSource();
            using (var sender = CreateMailSender()) {
                var token = cancellationTokenSource.Token;
                var input = new BlockingCollection<MailSenderMessage>();
                ConsoleKeyInfo exitCode;
                do {
                    Console.WriteLine("Press Enter to start populating e-mails");
                    Console.ReadLine();
                    Task.Run(() => {
                        int populated = 0;
                        try {
                            while (true) {
                                Thread.Sleep(100);
                                ++populated;
                                input.Add(new MailSenderMessage() { UserIdentifier = populated, Message = new MailMessage("test@tomain.de", "tast@tomain.de", string.Format("my subject {0}", populated), "myBody") }, token);
                                Console.WriteLine("Populated No. {0}", populated);
                            }
                        } finally {
                            input.CompleteAdding();
                        }
                    });
                    Console.WriteLine("Press Enter start sending e-mails");
                    Console.ReadLine();
                    Task.Run(() => {
                        var outputResult = sender.SendAsync(input);
                        var output = outputResult.Output;
                        foreach (var result in output.GetConsumingEnumerable()) {
                            Console.WriteLine("Success: {0}, Canceled: {2}, Identifier: {1}", result.Successful, result.UserIdentifier, result.Canceled);
                        }
                        Console.WriteLine("Finished");
                    });

                    Console.WriteLine("Press 'e' or 'E' to quit");
                    exitCode = Console.ReadKey();
                    Console.WriteLine();
                } while (exitCode.KeyChar != 'e' && exitCode.KeyChar != 'E');
                cancellationTokenSource.Cancel();
                sender.SendAsyncCancel();
            }
            Console.ReadLine();
        }
    }
}
