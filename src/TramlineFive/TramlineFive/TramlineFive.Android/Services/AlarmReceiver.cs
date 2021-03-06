﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using GalaSoft.MvvmLight.Ioc;
using SkgtService.Models;
using TramlineFive.Common.Services;
using TramlineFive.Services;

namespace TramlineFive.Droid.Services
{
    [BroadcastReceiver]
    [IntentFilter(new string[] { "com.company.BROADCAST" })]
    public class AlarmReceiver : BroadcastReceiver
    {
        private const string CHANNEL_ID = "tram";

        public override async void OnReceive(Context context, Intent intent)
        {
            try
            {
                if (!SimpleIoc.Default.IsRegistered<IApplicationService>())
                {
                    Log.Info("VERSION", "Application service not registered. Registering new instance...");
                    SimpleIoc.Default.Register<IApplicationService, ApplicationService>();
                }

                NewVersion version = await VersionService.CheckForUpdates();
                if (version != null)
                {
                    string message = intent.GetStringExtra("message");
                    string title = intent.GetStringExtra("title");

                    Intent notificationIntent = new Intent(Intent.ActionView);
                    notificationIntent.SetData(Android.Net.Uri.Parse(version.ReleaseUrl));
                    PendingIntent pending = PendingIntent.GetActivity(context, 0, notificationIntent, PendingIntentFlags.CancelCurrent);

                    NotificationManager manager = NotificationManager.FromContext(context);

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                    {
                        NotificationChannel notificationChannel = new NotificationChannel(CHANNEL_ID, "My Notifications", NotificationImportance.Max);

                        // Configure the notification channel.
                        notificationChannel.Description = "Channel description";
                        notificationChannel.EnableLights(true);
                        notificationChannel.LightColor = Color.AliceBlue;
                        notificationChannel.SetVibrationPattern(new long[] { 0, 1000, 500, 1000 });
                        notificationChannel.EnableVibration(true);
                        manager.CreateNotificationChannel(notificationChannel);
                    }

                    Notification.Builder builder =
                        new Notification.Builder(context, CHANNEL_ID)
                            .SetContentTitle(title)
                            .SetContentText(message)
                            .SetSmallIcon(Resource.Drawable.icon); 

                    builder.SetContentIntent(pending);

                    Notification notification = builder.Build(); 

                    manager.Notify(1337, notification);
                }
            }
            catch (Exception ex)
            {
                Log.Error("VERSION", ex.Message);
            }
            //PendingIntent pendingz = PendingIntent.GetBroadcast(context, 0, intent, PendingIntentFlags.UpdateCurrent);

            //AlarmManager alarmManager = context.GetSystemService(Context.AlarmService).JavaCast<AlarmManager>();
            //alarmManager.SetExact(AlarmType.ElapsedRealtimeWakeup, SystemClock.ElapsedRealtime() + 10 * 1000, pendingz);
        }
    }
}
