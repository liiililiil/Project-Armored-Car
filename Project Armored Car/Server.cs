using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Project_Armored_Car
{
    public partial class Server : TcpChatBase
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



        //내 공용 ip 
        private async Task<string> GetPublicIP()
        {
            using HttpClient client = new();
            string ip = await client.GetStringAsync("https://api.ipify.org");
            return ip.Trim();
        }
        public async Task<string> GetPrivateIP()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up &&
                            n.NetworkInterfaceType != NetworkInterfaceType.Loopback);

            foreach (var ni in interfaces)
            {
                var ipProps = ni.GetIPProperties();
                foreach (var addr in ipProps.UnicastAddresses)
                {
                    if (addr.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        var ip = addr.Address;
                        if (await IsPrivateIP(ip))
                            return ip.ToString();
                    }
                }
            }

            // ip 못찾으면 예외 처리
            throw new Exception("Private IPv4 주소를 찾을 수 없습니다.");
        }

        // 주소 검증
        private async Task<bool> IsPrivateIP(IPAddress ip)
        {
            var b = ip.GetAddressBytes();
            return b[0] == 10 ||
                   (b[0] == 172 && b[1] >= 16 && b[1] <= 31) ||
                   (b[0] == 192 && b[1] == 168);
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


                byte[] data = Encrypt(aes, text);
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


    }
}
