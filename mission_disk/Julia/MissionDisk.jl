using Sockets;
include("PID.jl");

function CalcAngleDiff(a1::Float32, a0::Float32)
    ret = a1 - a0;

    if (abs(ret) > 180)
        ret -= sign(ret)*360;
    end

    return ret;
end

function main()
    udp = UDPSocket();
    bind(udp, ip"127.0.0.1", 7000);
    snd = Float32.(zeros(4));

    pid_turret = PID(5.0, 0.0, 10.0, 0.05);
    pid_gun = PID(2.0, 0.05, 2.0, 0.05);

    while (true)
        rcvd = recv(udp);
        turret_angle = reinterpret(Float32, rcvd[17:20])[1];
        gun_elev = reinterpret(Float32, rcvd[21:24])[1];
        turret_angle_command = reinterpret(Float32, rcvd[25:28])[1];
        gun_elev_command = reinterpret(Float32, rcvd[29:32])[1];
        flags = rcvd[37];

        if ((flags & 0x01) != 0) snd[1] = 1000000;
        elseif ((flags & 0x04) != 0) snd[1] = -1000000;
        else
            snd[1] = 0;
        end
        
        if ((flags & 0x02) != 0) snd[2] = 50;
        elseif ((flags & 0x08) != 0) snd[2] = -50;
        else
            snd[2] = 0;
        end

        snd[3] = Calc(pid_turret, CalcAngleDiff(turret_angle_command, turret_angle));
        snd[4] = Calc(pid_gun, CalcAngleDiff(gun_elev_command, gun_elev));

        send(udp, ip"127.0.0.1", 6999, reinterpret(UInt8, snd));
    end
end