using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace MissionDisk
{
    class Program
    {
        static IPAddress ip = IPAddress.Parse("127.0.0.1");
        static int local_port = 7000;
        static int remote_port = 6999;
        static IPEndPoint localEP = new IPEndPoint(ip, local_port);
        static IPEndPoint remoteEP = new IPEndPoint(ip, remote_port);

        static UdpClient udp = new UdpClient(localEP);
        static byte[] snd = new byte[16];

        static float fwd_speed;
        static float yaw_rate;
        static float roll;
        static float pitch;
        static float turret_angle;
        static float gun_angle;
        static float turret_com;
        static float gun_elev_com;
        static float steer;
        static byte flgs;

        static PID turret_controller = new PID(5f, 0.0f, 10f, 0.05f);
        static PID gun_controller = new PID(2f, 0.05f, 2f, 0.05f);

        static void Main(string[] args)
        {
            while (true)
            {
                ReceiveSensorData();
                MakeControlInput();

                udp.Send(snd, snd.Length, remoteEP);
            }
        }

        static void ReceiveSensorData()
        {
            byte[] rcvd = udp.Receive(ref remoteEP);
            fwd_speed = BitConverter.ToSingle(rcvd, 0);
            yaw_rate = BitConverter.ToSingle(rcvd, 4);
            roll = BitConverter.ToSingle(rcvd, 8);
            pitch = BitConverter.ToSingle(rcvd, 12);
            turret_angle = BitConverter.ToSingle(rcvd, 16);
            gun_angle = BitConverter.ToSingle(rcvd, 20);
            turret_com = BitConverter.ToSingle(rcvd, 24);
            gun_elev_com = BitConverter.ToSingle(rcvd, 28);
            steer = BitConverter.ToSingle(rcvd, 32);
            flgs = rcvd[36];
        }

        static void MakeControlInput()
        {
            if ((flgs & 1) != 0) BitConverter.GetBytes(1000000.0F).CopyTo(snd, 0);
            else if ((flgs & 4) != 0) BitConverter.GetBytes(-1000000.0F).CopyTo(snd, 0);
            else BitConverter.GetBytes(0.0F).CopyTo(snd, 0);

            if ((flgs & 2) != 0) BitConverter.GetBytes(-50.0F).CopyTo(snd, 4);
            else if ((flgs & 8) != 0) BitConverter.GetBytes(50.0F).CopyTo(snd, 4);
            else BitConverter.GetBytes(0.0F).CopyTo(snd, 4);

            float turret_input = turret_controller.Calc(CalcAngleDiff(turret_com, turret_angle));
            BitConverter.GetBytes(turret_input).CopyTo(snd, 8);

            float gun_input = gun_controller.Calc(CalcAngleDiff(gun_elev_com, gun_angle));
            BitConverter.GetBytes(gun_input).CopyTo(snd, 12);
        }

        static float CalcAngleDiff(float a1, float a0)
        {
            float ret = a1 - a0;
            if (Math.Abs(ret) > 180)
                ret -= Math.Sign(ret) * 360;

            return ret;
        }
    }
}
