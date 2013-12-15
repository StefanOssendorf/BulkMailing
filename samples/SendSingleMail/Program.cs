using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Threading;
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
                Console.WriteLine("Cancelled: {0}", result.Cancelled);
                Console.WriteLine("Exception: {0}", result.Exception);
                Console.ReadLine();
            }
        }

        private static void SendBulkMailExample() {
            using (var sender = CreateMailSender()) {
                var list = new List<MailSenderMessage>();

                for (int i = 0; i < 10000; i++) {
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
                
                //Console.ReadLine();
                //if (!t1.IsCompleted) {
                //    sender.SendAsyncCancel();
                //}

                var result = t1.Result;
                foreach (var sendResult in result) {
                    Console.Write("Identifier:{0},Successful:{1},Cancelled:{2},Exception:{3}", sendResult.UserIdentifier, sendResult.Successful, sendResult.Cancelled, sendResult.Exception);
                    Console.WriteLine();
                }
            }
        }
    }
}
