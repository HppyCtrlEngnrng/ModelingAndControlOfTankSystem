using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionDisk
{
    class PID
    {
        float kp;
        float ki;
        float kd;
        float dt;

        float err_sum;
        float err_prev;

        public PID(float kp, float ki, float kd, float dt)
        {
            this.kp = kp;
            this.ki = ki;
            this.kd = kd;
            this.dt = dt;
            this.err_sum = 0.0F;
            this.err_prev = 0.0F;
        }

        public float Calc(float error)
        {
            float u = this.kp * error + this.ki * this.err_sum + this.kd * (error - this.err_prev) / this.dt;

            this.err_sum += error * this.dt;
            this.err_prev = error;

            return u;
        }
    }
}
