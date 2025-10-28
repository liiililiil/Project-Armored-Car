using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

//비주얼 스튜디오 윈도우 폼 상속 겁나 힘드네
using static Project_Armored_Car.TcpChatBase;


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

        public Server()
        {
            //암호화 기본 설정
            aes.KeySize = 256;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            InitializeComponent();

            LOG_TEXTBOX.LinkClicked += LOG_TEXTBOX_LinkClicked;

            Log(ref LOG_TEXTBOX, "원하는 포트를 입력 후 생성 버튼을 누르세요.");
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

                SuccessLog(ref LOG_TEXTBOX, $"서버가 시작되었습니다. ");
                Log(ref LOG_TEXTBOX, $"PublicIP : {await GetPublicIP()} | PrivateIP : {await GetPrivateIP()} | Port : {intPort}");

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


        private async Task Receive(TcpClient tcpClient)
        {
            try
            {
                Stream stream = tcpClient.GetStream();

                //통신 암호화
                await KeySwap(aes, stream, LOG_TEXTBOX);

                Invoke(() => SuccessLog(ref LOG_TEXTBOX, $"성공적으로 연결되었습니다."));

                while (tcpListener.Server.IsBound)
                {
                    byte[] msg = await DataReceive(stream);

                    //값을 클라이언트에 퍼트리기
                    _ = BroadcastMessage(msg, tcpClient);

                    //복호화
                    string text = Decrypt(aes, msg);

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
        private async Task<string> ServerCommand(string text)
        {
            string result = ""; 
            
            text = text.ToLower();
            string[] strings = text.Split(' ');
            switch (strings[0])
            {
                case "help":
                    {
                        result = $"{await GetBaseHelp()}\n{await RightPad("kick [ip]", 20)} | 특정 클라이언트의 연결을 종료합니다.\n{await RightPad("list", 20)} | 현재 연결된 서버의 정보를 반환합니다.";
                        break;
                    }
                case "kick":
                    {
                        if (!IPAddress.TryParse(strings[1], out _))
                        {
                            Log(ref LOG_TEXTBOX, "올바르지 않은 IP 주소입니다.");
                        }

                        if(tcpClients.Count <= 0)
                        {
                            result = "현재 연결된 클라이언트가 없습니다.";
                            break;
                        }

                        for (int i = 0; i < tcpClients.Count; i++)
                        {
                            TcpClient client = tcpClients[i];
                            if (((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString() == strings[1])
                            {
                                client.Close();
                                lock (tcpClients)
                                {
                                   tcpClients.RemoveAt(i);
                                }

                                result = $"{strings[1]} 클라이언트의 연결을 종료하였습니다.";
                                break;
                            }

                            result = $"{strings[1]} 클라이언트를 찾을 수 없습니다.";
                        }
                        break;
                    }

                case "list":
                    {
                        if(tcpClients.Count <= 0)
                        {
                            result = "현재 연결된 클라이언트가 없습니다.";
                        }
                        else
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine("현재 연결된 클라이언트 목록입니다.");
                            lock (tcpClients)
                            {
                                foreach (TcpClient client in tcpClients)
                                {
                                    sb.AppendLine($"{((IPEndPoint)client.Client.RemoteEndPoint).Address}");
                                }
                            }
                            result = sb.ToString();
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





        private void INPUT_TEXTBOX_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Sand(INPUT_TEXTBOX.Text);
            }
        }
        private async void Sand(string text)
        {
            try
            {
                if (text[0] == '/' || text[0] == '!')
                {
                    Log(ref LOG_TEXTBOX, await ServerCommand(text.Substring(1)));
                    return;
                }
                else
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


                    byte[] data = Encrypt(aes, text);
                    _ = BroadcastMessage(data, null);

                    Invoke(() => UserLog(ref LOG_TEXTBOX, text));
                }
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


    }
}
