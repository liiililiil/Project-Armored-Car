namespace Project_Armored_Car
{
    public partial class Select : Form
    {
        //디버그 할거
        private Form? showForm = null;

        public Select()
        {
            InitializeComponent();
        }

        // 서버 선택 시
        private void ServerButton_Click(object sender, EventArgs e)
        {
            FormSwitch(new Server());
        }

        // 클라이언트 선택 시
        private void ClientButton_Click(object sender, EventArgs e)
        {
            FormSwitch(new Client());
        }

        //통합
        private void FormSwitch(Form form)
        {
            //예외 처리
            if (showForm != null)
            {
                throw new Exception("서버와 클라이언트가 동시에 열린 것 같습니다! ");
            }

            showForm = form;
            Hide();

            showForm.FormClosed += CloseSync;
            showForm.Show();
        }

        //폼 같이 닫히게
        private void CloseSync(object sender, EventArgs e)
        {
            Close();
        }
    }
}
