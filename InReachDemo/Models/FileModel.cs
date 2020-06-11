using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Web;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

namespace InReachDemo.Models
{
    public class FileModel
    {
        [DisplayName("Input Email: ")]
        [Required(ErrorMessage = "You must input an email.")]
        public string Email { get; set; }

        [DisplayName("Select File: ")]
        [Required(ErrorMessage = "You must upload a file.")]
        public HttpPostedFileBase UserFile { get; set; }

        private IAmazonS3 s3Client;

        //Uploads UserFile to S3, then sends an email with the link. Returns string for ViewBag
        public string SendOut()
        {
            if (!UploadAWS())
            {
                return "AWS upload failed";
            }
            if (!SendEmail())
            {
                return "Sending Email Failed";
            }
            return "Accepted file, your email should arrive shortly! ";
        }

        //Inspired by https://docs.aws.amazon.com/AmazonS3/latest/dev/UploadObjectPreSignedURLDotNetSDK.html
        private bool UploadAWS()
        {
            s3Client = new AmazonS3Client(RegionEndpoint.USWest2);
            var url = GeneratePreSignedURL();

            return UploadObject(url);
        }

        //Uploads FileModel.UserFile to a presigned s3 URL
        private bool UploadObject(string url)
        {
            try
            {
                HttpWebRequest httpRequest = WebRequest.Create(url) as HttpWebRequest;
                httpRequest.Method = "PUT";
                using (Stream dataStream = httpRequest.GetRequestStream())
                {

                    var buffer = new byte[8000];
                    var tempFile = UserFile;

                    using (Stream fileStream = tempFile.InputStream)
                    {
                        int bytesRead = 0;
                        while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            dataStream.Write(buffer, 0, bytesRead);
                        }
                    }
                }
                HttpWebResponse response = httpRequest.GetResponse() as HttpWebResponse;

                return true;
            }
            catch
            {
                return false;
            }
        }

        //Generates a presigned S3 URL for uploading.
        private string GeneratePreSignedURL()
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = "romanstempbucket",
                Key = UserFile.FileName,
                Verb = HttpVerb.PUT,
                Expires = DateTime.Now.AddMinutes(5)
            };

            

            string url = s3Client.GetPreSignedURL(request);
            return url;
        }


        //Sends an email with a generated bucket link.
        private bool SendEmail()
        {
            try
            {
                MailMessage message = new MailMessage();
                message.To.Add(Email);
                message.Subject = "Romans File Forwarder: " + UserFile.FileName;
                message.Body = "https://romanstempbucket.s3.amazonaws.com/" + UserFile.FileName;

                //The plan originally was to use Amazon SES, but that's a pain to set up, so it's time for SMTP :) 
                SmtpClient smtp = new SmtpClient();
                smtp.Send(message);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}