using System;

namespace MJTradier
{
    public partial class Form1
    {
        /* KiwoomLibrary */
        int SubTimeToTime(int timeToBeSub, int timeToSub)
        {
            if (timeToBeSub <= timeToSub)
                return 0;

            int secToBeSub = (int)(timeToBeSub / 10000) * 3600 + (int)(timeToBeSub / 100) % 100 * 60 + timeToBeSub % 100;
            int secToSub = (int)(timeToSub / 10000) * 3600 + (int)(timeToSub / 100) % 100 * 60 + timeToSub % 100;
            int diffTime = secToBeSub - secToSub;
            int hour = diffTime / 3600;
            int minute = (diffTime % 3600) / 60;
            int second = diffTime % 60;

            return hour * 10000 + minute * 100 + second;
        }

        int SubTimeToTimeAndSec(int timeToBeSub, int timeToSub)
        {
            if (timeToBeSub <= timeToSub)
                return 0;

            int secToBeSub = (int)(timeToBeSub / 10000) * 3600 + (int)(timeToBeSub / 100) % 100 * 60 + timeToBeSub % 100;
            int secToSub = (int)(timeToSub / 10000) * 3600 + (int)(timeToSub / 100) % 100 * 60 + timeToSub % 100;

            return secToBeSub - secToSub;
        }


        int SubTimeBySec(int timeToBeSub, int subSec)
        {
            int secToBeSub = (int)(timeToBeSub / 10000) * 3600 + (int)(timeToBeSub / 100) % 100 * 60 + timeToBeSub % 100;
            if (secToBeSub <= subSec)
                return 0;
            secToBeSub -= subSec;
            int hour = secToBeSub / 3600;
            int minute = (secToBeSub % 3600) / 60;
            int second = secToBeSub % 60;

            return hour * 10000 + minute * 100 + second;
        }

        int AddTimeBySec(int timeToBeAdd, int addSec)
        {
            int secToBeAdd = (int)(timeToBeAdd / 10000) * 3600 + (int)(timeToBeAdd / 100) % 100 * 60 + timeToBeAdd % 100;
            secToBeAdd += addSec;
            int hour = secToBeAdd / 3600;
            int minute = (secToBeAdd % 3600) / 60;
            int second = secToBeAdd % 60;

            return hour * 10000 + minute * 100 + second;
        }

        int GetKiwoomTime(int timeSec)
        {
            return (int)(timeSec / 3600) * 10000 + (int)(timeSec % 3600 / 60) * 100 + timeSec % 60;
        }

        int GetSec(int kiwoomTime)
        {
            return (int)(kiwoomTime / 10000) * 3600 + (int)(kiwoomTime / 100) % 100 * 60 + kiwoomTime % 100;
        }

        int GetKosdaqGap(int price)
        {
            int gap;
            if (price < 1000)
                gap = 1;
            else if (price < 5000)
                gap = 5;
            else if (price < 10000)
                gap = 10;
            else if (price < 50000)
                gap = 50;
            else
                gap = 100;
            return gap;
        }

        int GetKospiGap(int price)
        {
            int gap;
            if (price < 1000)
                gap = 1;
            else if (price < 5000)
                gap = 5;
            else if (price < 10000)
                gap = 10;
            else if (price < 50000)
                gap = 50;
            else if (price < 100000)
                gap = 100;
            else if (price < 500000)
                gap = 500;
            else
                gap = 1000;

            return gap;
        }

        
        double GetAngleBetween(double fN, double fM)
        {
            double fAngleDirection;

            if (fN == fM) // same
            {
                fAngleDirection = 0;
            }
            else if (fN * fM == -1) // -1
            {
                if (fN > 0)
                {
                    fAngleDirection = -90;
                }
                else
                {
                    fAngleDirection = 90;
                }
            }
            else
            {
                // 각도가 반시계 방향이면 + 
                //        시계방향이면 -
                if (fN < fM)
                {

                    if (fN * fM < 0)
                    {
                        if (fN * fM < -1)
                        {
                            fAngleDirection = 180 + Math.Atan((fM - fN) / (1 + fN * fM)) * (180 / Math.PI);
                        }
                        else
                        {
                            fAngleDirection = Math.Atan((fM - fN) / (1 + fN * fM)) * (180 / Math.PI);
                        }


                    }
                    else
                    {
                        fAngleDirection = Math.Atan((fM - fN) / (1 + fN * fM)) * (180 / Math.PI);
                    }

                }
                else
                {
                    if (fN * fM < 0)
                    {
                        if (fN * fM < -1)
                        {
                            fAngleDirection = -180 + Math.Atan((fM - fN) / (1 + fN * fM)) * (180 / Math.PI);
                        }
                        else
                        {
                            fAngleDirection = Math.Atan((fM - fN) / (1 + fN * fM)) * (180 / Math.PI);
                        }

                    }
                    else
                    {
                        fAngleDirection = Math.Atan((fM - fN) / (1 + fN * fM)) * (180 / Math.PI);
                    }

                }

            }
            return fAngleDirection;
        }

        /// KiwoomLibrary Part 종료
        /////////////////////////////////

    }
}
