using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Project_Armored_Car
{
    // JSON 직렬화
    public class RsaKeyInfo
    {
        public string Modulus { get; set; }
        public string Exponent { get; set; }
    }


    public abstract class TcpChatBase : Form
    {        

        //암호화
        protected byte[] Encrypt(Aes aes, string plainText)
        {

            using MemoryStream ms = new();
            using CryptoStream cs = new(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
            byte[] bytes = Encoding.UTF8.GetBytes(plainText);
            cs.Write(bytes, 0, bytes.Length);
            cs.FlushFinalBlock();
            return ms.ToArray();
        }

        //복호화
        protected string Decrypt(Aes aes, byte[] cipherText)
        {

            using MemoryStream ms = new(cipherText);
            using CryptoStream cs = new(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using StreamReader reader = new(cs);
            return reader.ReadToEnd();
        }

        //암호화
        protected async Task KeySwap(Aes aes, Stream stream, RichTextBox logBox)
        {
            using RSA rsa = RSA.Create(2048);

            // RSA 키를 가져온후 병렬화
            string json = Encoding.UTF8.GetString(await DataReceive(stream));
            RsaKeyInfo? keyInfo = JsonSerializer.Deserialize<RsaKeyInfo>(json);

            //키 바인드
            RSAParameters rsaParams = new()
            {
                Modulus = Convert.FromBase64String(keyInfo.Modulus),
                Exponent = Convert.FromBase64String(keyInfo.Exponent)
            };
            rsa.ImportParameters(rsaParams);

            Invoke(() => Log(ref logBox, $"RSA 공유 키가 수신되었습니다."));

            // 대칭 키 송신
            await DataSend(stream, rsa.Encrypt(aes.Key, RSAEncryptionPadding.Pkcs1));

            Invoke(() => Log(ref logBox, $"AES 키가 송신되었습니다. "));

            // IV 송신
            await DataSend(stream, rsa.Encrypt(aes.IV, RSAEncryptionPadding.Pkcs1));

            Invoke(() => Log(ref logBox, $"AES 초기화 백터가 송신되었습니다. "));

            Invoke(() => SuccessLog(ref logBox, $"키 교환이 완료되었습니다."));
        }

        protected async Task<byte[]> DataReceive(Stream stream)
        {
            byte[] lenBuffer = new byte[4];
            await ReadExactAsync(stream, lenBuffer, 4);

            int len = BitConverter.ToInt32(lenBuffer, 0);

            byte[] dataBuffer = new byte[len];
            await ReadExactAsync(stream, dataBuffer, len);
            return dataBuffer;
        }
        protected async Task DataSend(Stream stream, byte[] data)
        {
            //사이즈
            byte[] len = BitConverter.GetBytes(data.Length);
            await stream.WriteAsync(len, 0, len.Length);

            //본 메시지
            await stream.WriteAsync(data, 0, data.Length);
        }


        //사이즈만큼만 버퍼 읽기
        protected async Task ReadExactAsync(Stream stream, byte[] buffer, int size)
        {
            int offset = 0;
            while (offset < size)
            {
                int read = await stream.ReadAsync(buffer, offset, size - offset);
                if (read <= 0)
                {
                    throw new Exception("연결이 종료되었습니다!");
                }

                offset += read;
            }
        }

        //로그
        protected void UserLog(ref RichTextBox textBox, string text)
        {
            textBox.AppendText($"[{Time()}] {text}\n");
            textBox.ScrollToCaret();
        }

        protected void SuccessLog(ref RichTextBox textBox, string text)
        {
            AddLog(ref textBox, Color.Green, "Success", text);
        }
        protected void ErrorLog(ref RichTextBox textBox, string text)
        {
            AddLog(ref textBox, Color.Red, "Error", text);
        }

        protected void Log(ref RichTextBox textBox, string text)
        {
            AddLog(ref textBox, Color.Gray, "Log", text);
        }

        protected void AddLog(ref RichTextBox textBox, Color selectColor, string Header, string text)
        {
            if (textBox == null)
            {
                throw new ArgumentNullException("textBox가 Null입니다!");
            }

            Color color = textBox.SelectionColor;

            textBox.SelectionStart = textBox.TextLength;
            textBox.SelectionColor = selectColor;

            textBox.AppendText($"[{Time()}] {Header} : {text}\n");

            textBox.SelectionColor = color;
            textBox.SelectionStart = textBox.TextLength;
            textBox.ScrollToCaret();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected string Time()
        {
            return DateTime.Now.ToString("HH:mm:ss");
        }
    }



}
