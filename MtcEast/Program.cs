namespace MtcEast
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {            //---- ２重起動防止 ----//
            string mutexName = "MultiTrainController";  // アプリケーションごとにユニークな名前を設定
            bool createdNew;
            using (Mutex mutex = new(true, mutexName, out createdNew))    // Mutexを作成
            {
                if (!createdNew)
                {
                    // 既にMutexが存在する場合、アプリケーションを終了

                    MessageBox.Show("アプリケーションは既に起動しています。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                // To customize application configuration such as set high DPI settings or default font,
                // see https://aka.ms/applicationconfiguration.
                ApplicationConfiguration.Initialize();
                Application.Run(new Form1());

                mutex.ReleaseMutex();   // アプリケーション終了時にMutexを解放
            }
        }
    }
}