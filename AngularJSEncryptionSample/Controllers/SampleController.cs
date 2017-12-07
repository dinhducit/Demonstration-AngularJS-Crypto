using Newtonsoft.Json;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;

namespace AngularJSEncryptionSample
{
    [RoutePrefix("sample")]
    public class SampleController : ApiController
    {
        // The key is the same as the client. Only the client's key is in base64 format.
        private const string KEY = "abcdefghijklmnopqrstuvwxyz123456";
        private const string KEY_CHANGED = "123456abcdefghijklmnopqrstuvwxyz";

        [HttpPost]
        [Route("")]
        public Sample Post(Sample sample)
        {
            var result = default(Sample);

            var password = Decrypt(sample.content, KEY);
            password = "HttpPost - Encrypted Property Changed";

            sample.content = Encrypt(password, KEY);

            result = sample;
            return result;
        }

        [HttpPost]
        [Route("full")]
        public string PostFull([FromBody]string content)
        {
            var result = default(string);

            var sample = JsonConvert.DeserializeObject<Sample>(Decrypt(content, KEY));
            sample.content = "HttpPost - Full Body Encrypted Changed";

            result = Encrypt(JsonConvert.SerializeObject(sample), KEY);
            return result;
        }

        [HttpGet]
        [Route("")]
        public Sample Get()
        {
            var result = new Sample();

            result.name = "Get encrypted property";
            result.content = "HttpGet - Encrypted Property Changed";
            result.content = Encrypt(result.content, KEY);

            return result;
        }

        [HttpGet]
        [Route("full")]
        public string GetFull()
        {
            var result = default(string);

            var sample = new Sample();
            sample.name = "Get Full encrypted";
            sample.content = "HttpGet - Full Body Encrypted Changed";

            result = Encrypt(JsonConvert.SerializeObject(sample), KEY);

            return result;
        }

        [HttpGet]
        [Route("keyChange")]
        public string KeyChange()
        {
            var result = default(string);

            var sample = new Sample();
            sample.name = "Get full encrypted with key changed";
            sample.content = "HttpGet - Full Body Encrypted with New Key";

            result = Encrypt(JsonConvert.SerializeObject(sample), KEY_CHANGED);

            return result;
        }

        private static string Decrypt(string cipherText, string key)
        {
            var keyArray = UTF8Encoding.UTF8.GetBytes(key);
            var cipherTextArray = Convert.FromBase64String(cipherText);

            var result = default(string);
            using (var managed = new RijndaelManaged())
            {
                managed.Key = keyArray;
                managed.Mode = CipherMode.ECB;
                managed.Padding = PaddingMode.PKCS7;

                using (var transform = managed.CreateDecryptor() as RijndaelManagedTransform)
                {
                    result = UTF8Encoding.UTF8.GetString(transform.TransformFinalBlock(cipherTextArray, 0, cipherTextArray.Length));
                }
            }
            return result;
        }

        private static string Encrypt(string plainText, string key)
        {
            var keyArray = UTF8Encoding.UTF8.GetBytes(key);
            var cipherTextArray = UTF8Encoding.UTF8.GetBytes(plainText);

            var result = default(string);
            using (var managed = new RijndaelManaged())
            {
                managed.Key = keyArray;
                managed.Mode = CipherMode.ECB;
                managed.Padding = PaddingMode.PKCS7;

                using (var transform = managed.CreateEncryptor() as RijndaelManagedTransform)
                {
                    result = Convert.ToBase64String(transform.TransformFinalBlock(cipherTextArray, 0, cipherTextArray.Length));
                }
            }
            return result;
        }
    }
}