namespace MtcEast
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {            //---- �Q�d�N���h�~ ----//
            string mutexName = "MultiTrainController";  // �A�v���P�[�V�������ƂɃ��j�[�N�Ȗ��O��ݒ�
            bool createdNew;
            using (Mutex mutex = new(true, mutexName, out createdNew))    // Mutex���쐬
            {
                if (!createdNew)
                {
                    // ����Mutex�����݂���ꍇ�A�A�v���P�[�V�������I��

                    MessageBox.Show("�A�v���P�[�V�����͊��ɋN�����Ă��܂��B", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                // To customize application configuration such as set high DPI settings or default font,
                // see https://aka.ms/applicationconfiguration.
                ApplicationConfiguration.Initialize();
                Application.Run(new Form1());

                mutex.ReleaseMutex();   // �A�v���P�[�V�����I������Mutex�����
            }
        }
    }
}