using NAudio.CoreAudioApi;

namespace VolumeLock
{
    static class Program
    {
        private static NotifyIcon trayIcon;
        private static bool isVolumeLocked = false;
        private static float fixedVolumeLevel;
        private static MMDevice audioDevice;
        private static MMDeviceEnumerator deviceEnumerator;

        [STAThread]
        static void Main()
        {
            deviceEnumerator = new MMDeviceEnumerator();
            audioDevice = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            trayIcon = new NotifyIcon
            {
                Icon = new Icon("Resources/VolumeLock.ico"),
                Visible = true,
                Text = "Громкость заблокирована",
                ContextMenuStrip = CreateContextMenu()
            };

            trayIcon.Click += (sender, e) => ToggleVolumeLock();

            // Блокировка при запуске
            isVolumeLocked = true;
            LockVolume();
            UpdateTrayIcon();

            Application.Run();
        }


        private static ContextMenuStrip CreateContextMenu()
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add(new ToolStripMenuItem("Закрыть", null, (sender, e) => ExitApplication()));
            return menu;
        }

        private static void ToggleVolumeLock()
        {
            isVolumeLocked = !isVolumeLocked;
            if (isVolumeLocked) LockVolume();
            else UnlockVolume();
            UpdateTrayIcon();
        }

        private static void UpdateTrayIcon()
        {
            var iconPath = isVolumeLocked ? "Resources/VolumeLock.ico" : "Resources/VolumeUnlock.ico";
            if (trayIcon.Icon.ToString() != iconPath)
            {
                trayIcon.Icon = new Icon(iconPath);
            }

            trayIcon.Text = isVolumeLocked ? "Громкость заблокирована" : "Громкость разблокирована";
        }

        private static void LockVolume()
        {
            fixedVolumeLevel = audioDevice.AudioEndpointVolume.MasterVolumeLevelScalar;
            audioDevice.AudioEndpointVolume.OnVolumeNotification += VolumeNotificationHandler;
        }

        private static void UnlockVolume()
        {
            audioDevice.AudioEndpointVolume.OnVolumeNotification -= VolumeNotificationHandler;
        }

        private static void VolumeNotificationHandler(AudioVolumeNotificationData data)
        {
            if (isVolumeLocked && Math.Abs(data.MasterVolume - fixedVolumeLevel) > 0.01f)
            {
                audioDevice.AudioEndpointVolume.MasterVolumeLevelScalar = fixedVolumeLevel;
            }
        }

        private static void ExitApplication()
        {
            trayIcon.Visible = false;
            audioDevice.AudioEndpointVolume.OnVolumeNotification -= VolumeNotificationHandler;
            Application.Exit();
        }
    }
}
