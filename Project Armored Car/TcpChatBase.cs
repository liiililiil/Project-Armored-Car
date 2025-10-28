using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace Project_Armored_Car
{
    // JSON 직렬화
    public class RsaKeyInfo
    {
        public required string Modulus { get; set; }
        public required string Exponent { get; set; }
    }

    public static class TcpChatBase
    {

        //암호화
        public static byte[] Encrypt(Aes aes, string plainText)
        {

            using MemoryStream ms = new();
            using CryptoStream cs = new(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
            byte[] bytes = Encoding.UTF8.GetBytes(plainText);
            cs.Write(bytes, 0, bytes.Length);
            cs.FlushFinalBlock();
            return ms.ToArray();
        }

        //복호화
        public static string Decrypt(Aes aes, byte[] cipherText)
        {

            using MemoryStream ms = new(cipherText);
            using CryptoStream cs = new(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using StreamReader reader = new(cs);
            return reader.ReadToEnd();
        }


        public static async Task<byte[]> DataReceive(Stream stream)
        {
            byte[] lenBuffer = new byte[4];
            await ReadExactAsync(stream, lenBuffer, 4);

            int len = BitConverter.ToInt32(lenBuffer, 0);

            byte[] dataBuffer = new byte[len];
            await ReadExactAsync(stream, dataBuffer, len);
            return dataBuffer;
        }
        public static async Task DataSend(Stream stream, byte[] data)
        {
            //사이즈
            byte[] len = BitConverter.GetBytes(data.Length);
            await stream.WriteAsync(len, 0, len.Length);

            //본 메시지
            await stream.WriteAsync(data, 0, data.Length);
        }


        //사이즈만큼만 버퍼 읽기
        public static async Task ReadExactAsync(Stream stream, byte[] buffer, int size)
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
        public static void UserLog(ref RichTextBox textBox, string text)
        {
            textBox.AppendText($"[{Time()}] {text}\n");
            textBox.ScrollToCaret();
        }

        public static void SuccessLog(ref RichTextBox textBox, string text)
        {
            AddLog(ref textBox, Color.Green, "Success", text);
        }
        public static void ErrorLog(ref RichTextBox textBox, string text)
        {
            AddLog(ref textBox, Color.Red, "Error", text);
        }

        public static void Log(ref RichTextBox textBox, string text)
        {
            AddLog(ref textBox, Color.Black, "Log", text);
        }

        public static void AddLog(ref RichTextBox textBox, Color selectColor, string Header, string text)
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
        public static string Time()
        {
            return DateTime.Now.ToString("HH:mm:ss");
        }

        public static async Task<string> BaseCommand(string text)
        {
            string[] strings = text.Split(' ');

            switch (strings[0])
            {
                case "privateip":
                    {
                        return $"당신의 개인 IP는 {await GetPrivateIP()} 입니다.";
                        break;
                    }
                case "publicip":
                    {
                        return $"당신의 공용 IP는 {await GetPublicIP()} 입니다.";
                        break;
                    }
                default:
                    throw new Exception("알 수 없는 명령어입니다!");
            }
        }

        public async static Task<string> GetBaseHelp()
        {
            return $"다음은 명령어 목록입니다.\n{await RightPad("help",20)} | 명령목록을 반환합니다.\n{await RightPad("privateip",20)} | 개인 ip를 반환합니다.\n{await RightPad("publicip", 20)} | 공용 ip를 반환합니다.";
        }

        public async static Task<string> RightPad(string input, int totalWidth)
        {
            return input.Length >= totalWidth ? input : input + new string(' ', totalWidth - input.Length);
        }

        //내 공용 ip 
        public static async Task<string> GetPublicIP()
        {
            using HttpClient client = new();
            string ip = await client.GetStringAsync("https://api.ipify.org");
            return ip.Trim();
        }
        public static async Task<string> GetPrivateIP()
        {
            IEnumerable<NetworkInterface> interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up &&
                            n.NetworkInterfaceType != NetworkInterfaceType.Loopback);

            foreach (NetworkInterface? ni in interfaces)
            {
                IPInterfaceProperties ipProps = ni.GetIPProperties();
                foreach (UnicastIPAddressInformation addr in ipProps.UnicastAddresses)
                {
                    if (addr.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        IPAddress ip = addr.Address;
                        if (IsPrivateIP(ip))
                        {
                            return ip.ToString();
                        }
                    }
                }
            }

            // ip 못찾으면 예외 처리
            throw new Exception("개인 IP 주소를 찾을 수 없습니다.");
        }

        // 주소 검증
        private static bool IsPrivateIP(IPAddress ip)
        {
            byte[] b = ip.GetAddressBytes();
            return b[0] == 10 ||
                   (b[0] == 172 && b[1] >= 16 && b[1] <= 31) ||
                   (b[0] == 192 && b[1] == 168);
        }


        public static string ReturnBaseHelp()
        {
            return "명령어는 다음이 있습니다. \n/help - 도움말 표시\n/clear - 채팅창 지우기";
        }
    }



}
