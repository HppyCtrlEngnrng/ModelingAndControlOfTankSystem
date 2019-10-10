mutable struct PID
    kp::Float32;
    ki::Float32;
    kd::Float32;
    dt::Float32;
    err_sum::Float32;
    err_prev::Float32;
end

PID(kp, ki, kd, dt) = PID(kp, ki, ki, dt, 0.0, 0.0);

function Calc(pid::PID, error::Float32)
    u = pid.kp * error + pid.ki * pid.err_sum + pid.kd * (error-pid.err_prev)/pid.dt;

    pid.err_sum += error * pid.dt;
    pid.err_prev = error;

    return u;
end