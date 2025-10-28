using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

//비주얼 스튜디오 윈도우 폼 상속 겁나 힘드네
using static Project_Armored_Car.TcpChatBase;

namespace Project_Armored_Car
{
    
    public partial class Client : Form
    {

        //대칭키
        private readonly Aes aes = Aes.Create();

        //클라이언트
        private TcpClient client = new();
        private Stream stream;

        public Client()
        {
            //기본 설정
            aes.KeySize = 256;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            InitializeComponent();
            LOG_TEXTBOX.LinkClicked += LOG_TEXTBOX_LinkClicked;

            Log(ref LOG_TEXTBOX, "연결을 원하는 서버의 ip와 포트를 입력 후 연결 버튼을 누르세요.");
        }
        protected void LOG_TEXTBOX_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            try
            {
                Clipboard.SetText(e.LinkText);
                SuccessLog(ref LOG_TEXTBOX, "클립보드에 복사되었습니다.");
            }
            catch (Exception ex)
            {
                ErrorLog(ref LOG_TEXTBOX, $"클립보드 복사에 실패하였습니다!");
            }
        }

        private void disconn()
        {
            client.Close();
            client.Dispose();
            stream.Close();
            stream.Dispose();
            client = null;
            stream = null;

            Log(ref LOG_TEXTBOX, $"서버와의 연결이 종료되었습니다.");
        }

        private async Task<string> ClientCommand(string text)
        {
            string result = "";

            text = text.ToLower();
            string[] strings = text.Split(' ');

            switch (strings[0])
            {
                case "help":
                    {
                        result = $"{await GetBaseHelp()}\n{await RightPad("disconnect", 20)} | 서버와의 연결을 종료합니다.\n{await RightPad("info",20)} | 현재 연결된 서버의 정보를 반환합니다.";
                        break;
                    }
                case "disconnect":
                    {
                        if (client == null || !client.Connected)
                        {
                            result = $"서버에 연결되어 있지 않습니다!";
                        }
                        else
                        {
                            Invoke(() =>disconn());
                        }
                        break;
                    }

                case "info":
                    {
                        if(client == null || !client.Connected)
                        {
                            result = $"서버에 연결되어 있지 않습니다!";
                        }
                        else
                        {
                            result = $"서버 IP : {((IPEndPoint)client.Client.RemoteEndPoint).Address} | 서버 PORT : {((IPEndPoint)client.Client.RemoteEndPoint).Port}";
                        }

                        break;

                    }

                default:
                    {
                        result = await BaseCommand(text);
                        break;
                    }

            }

            return result;
        }

        private async void CONNENT_BUTTON_Click(object sender, EventArgs e)
        {


            try
            {

                CONNENT_BUTTON.Enabled = false;

                string ip = IP_TEXTBOX.Text;
                string port = PORT_TEXTBOX.Text;

                //포트랑 아이피 검사
                if (!IPAddress.TryParse(ip, out _))
                {
                    throw new Exception("IP가 유효하지 않습니다!");
                }

                if (!int.TryParse(port, out _))
                {
                    throw new Exception("PORT가 유효하지 않습니다!");
                }


                //포트 범위 검사
                if (int.Parse(port) is < 1 or > 65535)
                {
                    throw new Exception("PORT 범위가 벗어났습니다!");
                }

                await ConnentAsync(IPAddress.Parse(ip), int.Parse(port));


            }
            catch (Exception ex)
            {

                ErrorLog(ref LOG_TEXTBOX, $"{ex.Message}");
            }
            finally
            {
                if (stream != null && stream.CanRead)
                {
                    disconn();
                }

                CONNENT_BUTTON.Enabled = true;
            }

        }
        private async Task ConnentAsync(IPAddress ip, int port)
        {
            // 기존 연결이 있으면 종료
            if (client != null)
            {
                client.Close();
                client.Dispose();
            }

            client = new TcpClient();

            await client.ConnectAsync(ip.ToString(), port);
            stream = client.GetStream();

            Invoke(() => Log(ref LOG_TEXTBOX, $"서버가 감지되었습니다."));

            await KeySwap();

            Invoke(() => SuccessLog(ref LOG_TEXTBOX, $"성공적으로 연결되었습니다."));

            await Receive();

        }

        //통신 암호화
        private async Task KeySwap()
        {
            using RSA rsa = RSA.Create(2048);
            byte[] lengthBuffer = new byte[4];

            // 공개키 직렬화
            RSAParameters publicKey = rsa.ExportParameters(false);
            var keyInfo = new
            {
                Modulus = Convert.ToBase64String(publicKey.Modulus),
                Exponent = Convert.ToBase64String(publicKey.Exponent)
            };
            string data = JsonSerializer.Serialize(keyInfo);

            //공개키 전송
            await DataSend(stream, Encoding.UTF8.GetBytes(data));

            Invoke(() => Log(ref LOG_TEXTBOX, $"RAS 공유 키가 송신되었습니다."));

            // Key 가져오기
            aes.Key = rsa.Decrypt(await DataReceive(stream), RSAEncryptionPadding.Pkcs1);

            Invoke(() => Log(ref LOG_TEXTBOX, $"AES 키가 수신되었습니다."));

            // IV 가져오기
            aes.IV = rsa.Decrypt(await DataReceive(stream), RSAEncryptionPadding.Pkcs1);

            Invoke(() => Log(ref LOG_TEXTBOX, $"AES 초기화 백터가 수신되었습니다."));
        }

        private async Task Receive()
        {
            byte[] lenBuffer = new byte[4];

            while (true)
            {
                if(stream == null || !stream.CanRead)
                {
                    throw new Exception("서버와의 연결이 끊어졌습니다.");
                }

                string msg = Decrypt(aes,await DataReceive(stream));
                Invoke(() => UserLog(ref LOG_TEXTBOX, msg));

            }
        }


        private void SEND_BUTTON_Click(object sender, EventArgs e)
        {
            Sand(INPUT_TEXTBOX.Text);
        }
        private async void Sand(string text)
        {
            try
            {
                if (text[0] == '/' || text[0] == '!')
                {
                    INPUT_TEXTBOX.Clear();

                    Log(ref LOG_TEXTBOX, await ClientCommand(text.Substring(1)));
                }
                else
                {
                    INPUT_TEXTBOX.Clear();

                    if (string.IsNullOrEmpty(text))
                    {
                        throw new Exception("내용이 없습니다!");
                    }

                    if (stream == null || !stream.CanWrite)
                    {
                        throw new Exception("아직 서버와 연결되지 않았습니다!");
                    }

                    text = $"{NAME_TEXTBOX.Text} : {text}";

                    await DataSend(stream, Encrypt(aes, text));

                    Invoke(() => UserLog(ref LOG_TEXTBOX, text));

                }
            }
            catch (Exception ex)
            {
                ErrorLog(ref LOG_TEXTBOX, ex.Message);
            }
        }





        private void INPUT_TEXTBOX_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Sand(INPUT_TEXTBOX.Text);
            }
        }
    }
}
