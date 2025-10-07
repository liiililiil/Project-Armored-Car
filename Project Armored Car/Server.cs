using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Project_Armored_Car
{
    public partial class Server : Form
    {
        // 서버 
        private TcpListener tcpListener;

        // 클라이언트들 저장할거
        private readonly List<TcpClient> tcpClients = [];

        // 공개키 암호화
        private readonly Aes aes = Aes.Create();

        //암호화
        private byte[] Encrypt(string plainText)
        {

            using MemoryStream ms = new();
            using CryptoStream cs = new(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
            byte[] bytes = Encoding.UTF8.GetBytes(plainText);
            cs.Write(bytes, 0, bytes.Length);
            cs.FlushFinalBlock();
            return ms.ToArray();
        }

        //복호화
        private string Decrypt(byte[] cipherText)
        {

            using MemoryStream ms = new(cipherText);
            using CryptoStream cs = new(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using StreamReader reader = new(cs);
            return reader.ReadToEnd();
        }

        public Server()
        {
            //암호화 기본 설정
            aes.KeySize = 256;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            InitializeComponent();

            Log(ref LOG_TEXTBOX, "원하는 포트를 입력 후 생성 버튼을 누르세요.");
        }

        private async void Create_Click(object sender, EventArgs e)
        {
            try
            {
                Create.Enabled = false;

                //포트 검사
                string port = PORT_TEXTBOX.Text;
                if (!int.TryParse(port, out _))
                {
                    throw new Exception("올바르지 않은 포트입니다!");
                }

                int intPort = int.Parse(port);

                //리스너 생성
                tcpListener = new TcpListener(IPAddress.Any, intPort);
                tcpListener.Start();

                SuccessLog(ref LOG_TEXTBOX, $"서버가 시작되었습니다. | IP : {await GetPublicIP()} | 포트 : {intPort}");

                //키 생성
                aes.GenerateKey();
                aes.GenerateIV();

                Invoke(() => SuccessLog(ref LOG_TEXTBOX, "암호화를 완료하였습니다."));

                while (tcpListener.Server.IsBound)
                {
                    //클라이언트 들어오는거 감지
                    TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync();

                    //리스트에 추가
                    lock (tcpClients)
                    {
                        tcpClients.Add(tcpClient);
                    }

                    //로그 띄우기
                    Invoke(() => AddLog(ref LOG_TEXTBOX, Color.Green, "접속이 감지되었습니다.", ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString()));

                    //수신 대기
                    _ = Receive(tcpClient);
                }




            }
            catch (Exception ex)
            {
                ErrorLog(ref LOG_TEXTBOX, ex.Message);
            }
            finally
            {
                Create.Enabled = true;
                if (tcpListener != null && tcpListener.Server.IsBound)
                {
                    tcpListener.Stop();
                }
            }
        }
        private async Task Receive(TcpClient tcpClient)
        {
            try
            {
                Stream stream = tcpClient.GetStream();

                //통신 암호화
                await KeySwap(stream);

                Invoke(() => SuccessLog(ref LOG_TEXTBOX, $"성공적으로 연결되었습니다."));

                while (tcpListener.Server.IsBound)
                {
                    byte[] msg = await DataReceive(stream);

                    //값을 클라이언트에 퍼트리기
                    _ = BroadcastMessage(msg, tcpClient);

                    //복호화
                    string text = Decrypt(msg);

                    //출력
                    Invoke(() => UserLog(ref LOG_TEXTBOX, text));
                }
            }
            catch (Exception ex)
            {
                Invoke(() => ErrorLog(ref LOG_TEXTBOX, $"{ex.Message}"));
            }
            finally
            {
                lock (tcpClients)
                {
                    _ = tcpClients.Remove(tcpClient);
                }
                tcpClient.Close();
            }

        }

        private async Task BroadcastMessage(byte[] data, TcpClient? sender)
        {

            lock (tcpClients)
            {
                //모든 클라이언트에게 송신
                foreach (TcpClient client in tcpClients)
                {
                    if (client == sender)
                    {
                        continue; // 보내는 사람 제외
                    }

                    try
                    {
                        _ = DataSend(client.GetStream(), data);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"메시지를 공유하는 중 문제가 발생하였습니다! {ex.Message}");
                    }
                }
            }
        }
        //암호화
        private async Task KeySwap(Stream stream)
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

            Invoke(() => Log(ref LOG_TEXTBOX, $"RSA 공유 키가 수신되었습니다."));

            // 대칭 키 송신
            await DataSend(stream, rsa.Encrypt(aes.Key, RSAEncryptionPadding.Pkcs1));

            Invoke(() => Log(ref LOG_TEXTBOX, $"AES 키가 송신되었습니다. "));

            // IV 송신
            await DataSend(stream, rsa.Encrypt(aes.IV, RSAEncryptionPadding.Pkcs1));

            Invoke(() => Log(ref LOG_TEXTBOX, $"AES 초기화 백터가 송신되었습니다. "));

            Invoke(() => SuccessLog(ref LOG_TEXTBOX, $"키 교환이 완료되었습니다."));
        }

        private async Task<byte[]> DataReceive(Stream stream)
        {
            byte[] lenBuffer = new byte[4];
            await ReadExactAsync(stream, lenBuffer, 4);

            int len = BitConverter.ToInt32(lenBuffer, 0);

            byte[] dataBuffer = new byte[len];
            await ReadExactAsync(stream, dataBuffer, len);
            return dataBuffer;
        }
        private async Task DataSend(Stream stream, byte[] data)
        {
            //사이즈
            byte[] len = BitConverter.GetBytes(data.Length);
            await stream.WriteAsync(len, 0, len.Length);

            //본 메시지
            await stream.WriteAsync(data, 0, data.Length);
        }

        // JSON 직렬화
        public class RsaKeyInfo
        {
            public string Modulus { get; set; }
            public string Exponent { get; set; }
        }

        //사이즈만큼만 버퍼 읽기
        private async Task ReadExactAsync(Stream stream, byte[] buffer, int size)
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

        //내 공용 ip 
        private async Task<string> GetPublicIP()
        {
            using HttpClient client = new();
            string ip = await client.GetStringAsync("https://api.ipify.org");
            return ip.Trim();
        }

        /// <summary>
        /// 로그 출력용 
        /// </summary>
        private void SuccessLog(ref RichTextBox textBox, string text)
        {
            AddLog(ref textBox, Color.Green, "Success", text);
        }
        private void ErrorLog(ref RichTextBox textBox, string text)
        {
            AddLog(ref textBox, Color.Red, "Error", text);
        }

        private void Log(ref RichTextBox textBox, string text)
        {
            AddLog(ref textBox, Color.Gray, "Log", text);
        }

        private void AddLog(ref RichTextBox textBox, Color selectColor, string Header, string text)
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

        /// <summary>
        /// 시간 출력용 
        /// </summary>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string Time()
        {
            return DateTime.Now.ToString("HH:mm:ss");
        }

        private void INPUT_TEXTBOX_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Sand(INPUT_TEXTBOX.Text);
            }
        }
        private void Sand(string text)
        {
            try
            {
                INPUT_TEXTBOX.Clear();

                if (string.IsNullOrEmpty(text))
                {
                    throw new Exception("내용이 없습니다!");
                }

                if (tcpListener == null && !tcpListener.Server.IsBound)
                {
                    throw new Exception("서버가 생성되지 않았습니다!");
                }

                if (tcpClients.Count <= 0)
                {
                    throw new Exception("서버에 연결된 클라이언트가 하나도 없습니다!");
                }

                text = $"{NAME_TEXTBOX.Text} : {text}";


                byte[] data = Encrypt(text);
                _ = BroadcastMessage(data, null);

                Invoke(() => UserLog(ref LOG_TEXTBOX, text));
            }
            catch (Exception ex)
            {
                ErrorLog(ref LOG_TEXTBOX, ex.Message);
            }
        }

        private void SEND_BUTTON_Click(object sender, EventArgs e)
        {
            Sand(INPUT_TEXTBOX.Text);
        }
        private void UserLog(ref RichTextBox textBox, string text)
        {
            textBox.AppendText($"[{Time()}] {text}\n");
            textBox.ScrollToCaret();
        }

    }
}
