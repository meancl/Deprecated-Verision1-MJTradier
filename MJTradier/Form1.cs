using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;

// ========================================================================
// 철학 : Being simple is the best.
// ========================================================================
namespace MJTradier
{
    public partial class Form1 : Form
    {
        // ------------------------------------------------------
        // 숫자 상수 변수
        // ------------------------------------------------------
        public const long ONE_TRILLION =      1000000000000;
        public const long HUNDRED_BILLION = 100000000000;
        public const long TEN_BILLION =     10000000000;
        public const int BILLION =            1000000000;
        public const int TWENTY_MILLION = 20000000;
        public const int TEN_MILLION = 10000000;
        public const int FIVE_MILLION = 5000000;
        public const int MILLION = 1000000;
        public const int BRUSH = 10;
        public const long SHARE_INIT = long.MaxValue;

        public const long INDEX_DEGREE = TEN_BILLION;

        // ------------------------------------------------------
        // 상수 변수
        // ------------------------------------------------------
        private const byte KOSDAQ_ID = 0;  // 코스닥을 증명하는 상수
        private const byte KOSPI_ID = 1; // 코스피를 증명하는 상수
        private const int MAX_STOCK_NUM = 1000000; // 최종주식종목 수 0 ~ 999999 
        public const int NUM_SEP_PER_SCREEN = 100; // 한 화면번호 당 가능요청 수
        public const double STOCK_TAX = 0.0023; // 거래세 
        public const double STOCK_FEE = 0.00015; // 증권 매매 수수료
        public const double VIRTUAL_STOCK_FEE = 0.0035; // 증권 매매 수수료
        public const int MAX_STOCK_HOLDINGS_NUM = 200; // 보유주식을 저장하는 구조체 최대 갯수
        public const int EYES_CLOSE_NUM = 3; // 현재가에서 EYES_CLOSE_NUM 스텝만큼 가격을 올려 지정가에 두기 위한 스텝 변수
        public const int SHUTDOWN_TIME = 150000; // 마감시간
        public const int BAN_BUY_TIME = 144000; // 매수 종료시간
        public const int IGNORE_REQ_SEC = 10; // 요청무시용 seconds 변수

        // ------------------------------------------------------
        // 각 종목 구조체 변수
        // ------------------------------------------------------
        private int nEachStockIdx; // 개인구조체의 인덱스를 설정하기 위한 변수 0부터 시작
        private int[] eachStockIdxArray; // 개인구조체의 인덱스를 저장한 배열  //초기화대상
        EachStock[] eachStockArray;  // 각 주식이 가지는 실시간용 구조체(개인구조체) //초기화대상
        public int nCurIdx; // 현재 개인구조체의 인덱스

        // ------------------------------------------------------
        // 기타 변수
        // ------------------------------------------------------
        public char[] charsToTrim = { '+', '-', ' ' };
        public bool isDepositSet; // 예수금이 세팅돼있는 지 확인하는 변수
        public int nStockLength;
        // ------------------------------------------------------
        // 스크린번호 변수
        // ------------------------------------------------------
        public const int REAL_SCREEN_NUM_START = 1000; // 실시간 시작화면번호
        public const int REAL_SCREEN_NUM_END = 1100; // 실시간 마지막화면번호
        public const int TR_SCREEN_NUM_START = 1101; // TR 초기화면번호
        public const int TR_SCREEN_NUM_END = 2000; // TR 마지막화면번호
        public const int TRADE_SCREEN_NUM_START = 2001; // 매매 시작화면번호 
        public const int TRADE_SCREEN_NUM_END = 9999; // 매매 마지막화면전호

        private int nTrScreenNum = TR_SCREEN_NUM_START;
        private int nRealScreenNum = REAL_SCREEN_NUM_START;
        private int nTradeScreenNum = TRADE_SCREEN_NUM_START;

        // ------------------------------------------------------
        // 종목획득 변수
        // ------------------------------------------------------
        // private string sConfigurationPath = @"D:\MJ\stock\getData\kiwoom\"; // 코스피, 코스닥 종목을 저장해놓은 파일의 디렉터리 경로
        private static string sBasicInfoPath = @"기본정보\";
        private static string sMessageLogPath = @"로그\";
        private string[] kosdaqCodes; // 코스닥 종목들을 저장한 문자열 배열 //초기화대상
        private string[] kospiCodes; //  코스피 종목들을 저장한 문자열 배열 //초기화대상


        // ------------------------------------------------------
        // 공유 변수
        // ------------------------------------------------------
        public bool isMarketStart; // true면 장중, false면 장시작전,장마감후
        public string sAccountNum; // 계좌번호
        public int nSharedTime; // 모든 종목들이 공유하는 현재시간
        public int nCurDeposit;  // 현재 예수금
        public int nCurDepositCalc; // 계산하기 위한 예수금
        public int nShutDown; // 장마감이 되면 양수가 됨.
        public bool isForCheckHoldings; // 현재잔고를 확인만 하기위한 기능
        public int nFirstDisposal; // 장시작이 되면 매도체크, only one chance: nFirstDisposal == 0
        public int nTimeSep = 100;

        // ------------------------------------------------------
        // 매매관련 변수
        // ------------------------------------------------------
        public int nMaxPriceForEachStock = 3000000;   // 각 종목이 한번에 최대 살 수 있는 금액 ex. 삼백만원
        public Queue<TradeSlot> tradeQueue = new Queue<TradeSlot>(); // 매매신청을 담는 큐, 매매컨트롤러가 사용할 큐 
        TradeSlot curSlot; // 임시로 사용하능한 매매요청, 매매컨트롤러 변수
        public const int MAX_REQ_SEC = 600;

        //--------------------------------------------------------
        // 계좌평가잔고내역요청 변수
        //--------------------------------------------------------
        public Holdings[] holdingsArray = new Holdings[MAX_STOCK_HOLDINGS_NUM]; // 현재 보유주식을 담을 구조체 배열 
        public int nHoldingCnt; // 총 보유주식의 수
        public int nCurHoldingsIdx; // 보유주식을 담을때 사용하는 인덱스 변수 


        //--------------------------------------------------------
        // 개인구조체 매수슬롯 변수
        //--------------------------------------------------------
        public const int BUY_SLOT_NUM = 50;
        public const int BUY_LIMIT_NUM = 10;
        public BuySlot[,] buySlotArray; //초기화대상
        public int[] buySlotCntArray; //초기화대상

        //--------------------------------------------------------
        // 주문제한횟수 관련 변수
        //--------------------------------------------------------
        public int nBeforeOrderTime;
        public int nCurOrderTime;
        public int nAccumCount;
        public const int LIMIT_SENDORDER_NUM = 5;
        public bool isSendOrder;
        public bool isForbidTrade;

        public int nTenTime;
        public int nFirstTime;

        public const int nUpdateTime = 20;
        public const double fPushWeight = 0.8;
        public const double fTightPushWeight = 0.9;
        public const double fAlmostPushWeight = 0.95;

        public StreamReader sr;
        public int srCnt;

        public StreamWriter swLogFirstAndLast = new StreamWriter(new FileStream(sMessageLogPath + "messageFirstAndLast.txt", FileMode.Create));
        public StreamWriter swLogFirstAndLastEverage = new StreamWriter(new FileStream(sMessageLogPath + "messageFirstAndLastEverage.txt", FileMode.Create));
        public StreamWriter swLogTopAndBottom = new StreamWriter(new FileStream(sMessageLogPath + "messageTopAndBottom.txt", FileMode.Create));
        public StreamWriter swLogTopAndBottomEverage = new StreamWriter(new FileStream(sMessageLogPath + "messageTopAndBottomEverage.txt", FileMode.Create));
        public StreamWriter swGapUp, swGapDown, swGapMiddle;

        public StreamWriter swLogPower2 = new StreamWriter(new FileStream(sMessageLogPath + "messageLogPower2.txt", FileMode.Create));
        public StreamWriter swLogPower5 = new StreamWriter(new FileStream(sMessageLogPath + "messageLogPower5.txt", FileMode.Create));
        public StreamWriter swLogPower1 = new StreamWriter(new FileStream(sMessageLogPath + "messageLogPower1.txt", FileMode.Create));
        public StreamWriter swLogLongPower5 = new StreamWriter(new FileStream(sMessageLogPath + "messageLogLongPower5.txt", FileMode.Create));
        public StreamWriter swLogPowerM2 = new StreamWriter(new FileStream(sMessageLogPath + "messageLogPowerM2.txt", FileMode.Create));
        public StreamWriter swLogPowerM5 = new StreamWriter(new FileStream(sMessageLogPath + "messageLogPowerM5.txt", FileMode.Create));

        public StreamWriter swLogSpeed10 = new StreamWriter(new FileStream(sMessageLogPath + "messageLogSpeed10.txt", FileMode.Create));
        public StreamWriter swLogSpeed20 = new StreamWriter(new FileStream(sMessageLogPath + "messageLogSpeed20.txt", FileMode.Create));
        public StreamWriter swLogSpeed30 = new StreamWriter(new FileStream(sMessageLogPath + "messageLogSpeed30.txt", FileMode.Create));
        public StreamWriter swLogSpeed40 = new StreamWriter(new FileStream(sMessageLogPath + "messageLogSpeed40.txt", FileMode.Create));

        public StreamWriter swLogHogaRatio60 = new StreamWriter(new FileStream(sMessageLogPath + "messageLogHogaRatio60.txt", FileMode.Create));
        public StreamWriter swLogHogaRatio70 = new StreamWriter(new FileStream(sMessageLogPath + "messageLogHogaRatio70.txt", FileMode.Create));
        public StreamWriter swLogHogaRatio80 = new StreamWriter(new FileStream(sMessageLogPath + "messageLogHogaRatio80.txt", FileMode.Create));
        public StreamWriter swLogHogaRatio90 = new StreamWriter(new FileStream(sMessageLogPath + "messageLogHogaRatio90.txt", FileMode.Create));

        public StreamWriter swLogHogaVolume100 = new StreamWriter(new FileStream(sMessageLogPath + "messageLogHogaVolume100.txt", FileMode.Create));
        public StreamWriter swLogHogaVolume80 = new StreamWriter(new FileStream(sMessageLogPath + "messageLogHogaVolume80.txt", FileMode.Create));
        public StreamWriter swLogHogaVolume60 = new StreamWriter(new FileStream(sMessageLogPath + "messageLogHogaVolume60.txt", FileMode.Create));
        public StreamWriter swLogHogaVolume40 = new StreamWriter(new FileStream(sMessageLogPath + "messageLogHogaVolume40.txt", FileMode.Create));

        public StreamWriter swLogTradeVolume200 = new StreamWriter(new FileStream(sMessageLogPath + "messageLogTradeVolume200.txt", FileMode.Create));
        public StreamWriter swLogTradeVolume150 = new StreamWriter(new FileStream(sMessageLogPath + "messageLogTradeVolume150.txt", FileMode.Create));
        public StreamWriter swLogTradeVolume100 = new StreamWriter(new FileStream(sMessageLogPath + "messageLogTradeVolume100.txt", FileMode.Create));
        public StreamWriter swLogTradeVolume70 = new StreamWriter(new FileStream(sMessageLogPath + "messageLogTradeVolume70.txt", FileMode.Create));

        public StreamWriter swLogPowerLong15 = new StreamWriter(new FileStream(sMessageLogPath + "messageLogPowerLong15.txt", FileMode.Create));
        public StreamWriter swLogPowerLong30 = new StreamWriter(new FileStream(sMessageLogPath + "messageLogPowerLong30.txt", FileMode.Create));
        public StreamWriter swLogPowerLong45 = new StreamWriter(new FileStream(sMessageLogPath + "messageLogPowerLong45.txt", FileMode.Create));
        public StreamWriter swLogPowerLong60 = new StreamWriter(new FileStream(sMessageLogPath + "messageLogPowerLong60.txt", FileMode.Create));

        public StreamWriter swLogTotalCheck5 = new StreamWriter(new FileStream(sMessageLogPath + "messageLogTotalCheck5.txt", FileMode.Create));
        public StreamWriter swLogTotalCheck7 = new StreamWriter(new FileStream(sMessageLogPath + "messageLogTotalCheck7.txt", FileMode.Create));
        public StreamWriter swLogTotalCheck10 = new StreamWriter(new FileStream(sMessageLogPath + "messageLogTotalCheck10.txt", FileMode.Create));
        public StreamWriter swLogTotalCheck15 = new StreamWriter(new FileStream(sMessageLogPath + "messageLogTotalCheck15.txt", FileMode.Create));

        public StreamWriter swLogSpeed6 = new StreamWriter(new FileStream(sMessageLogPath + "messageLogSpeed6.txt", FileMode.Create));
        public StreamWriter swLogSpeedAfterNoon = new StreamWriter(new FileStream(sMessageLogPath + "messageLogSpeedAfterNoon.txt", FileMode.Create));
        public StreamWriter swLogVolume80 = new StreamWriter(new FileStream(sMessageLogPath + "messageLogVolume80.txt", FileMode.Create));
        public StreamWriter swLogVolume70 = new StreamWriter(new FileStream(sMessageLogPath + "messageLogVolume70.txt", FileMode.Create));
        public StreamWriter swLogOneToTenSucceed = new StreamWriter(new FileStream(sMessageLogPath + "messageLogOneToTenSucceed.txt", FileMode.Create));
        public StreamWriter swLogQuarterSucceed = new StreamWriter(new FileStream(sMessageLogPath + "messageLogQuarterSucceed.txt", FileMode.Create));
        public StreamWriter swLogHalfSucceed = new StreamWriter(new FileStream(sMessageLogPath + "messageLogHalfSucceed.txt", FileMode.Create));
        public StreamWriter swLogFullSucceed = new StreamWriter(new FileStream(sMessageLogPath + "messageLogFullSucceed.txt", FileMode.Create));
        public StreamWriter swLogHalfFullSucceed = new StreamWriter(new FileStream(sMessageLogPath + "messageLogHalfFullSucceed.txt", FileMode.Create));
        public StreamWriter swLogDoubleSucceed = new StreamWriter(new FileStream(sMessageLogPath + "messageLogDoubleSucceed.txt", FileMode.Create));


        public StreamWriter swLogOnlyUp2 = new StreamWriter(new FileStream(sMessageLogPath + "messageLogOnlyUp2.txt", FileMode.Create));
        public StreamWriter swLogOnlyUp4 = new StreamWriter(new FileStream(sMessageLogPath + "messageLogOnlyUp4.txt", FileMode.Create));
        public StreamWriter swLogOnlyUp6 = new StreamWriter(new FileStream(sMessageLogPath + "messageLogOnlyUp6.txt", FileMode.Create));
        public StreamWriter swLogOnlyUp8 = new StreamWriter(new FileStream(sMessageLogPath + "messageLogOnlyUp8.txt", FileMode.Create));

        public StreamWriter swLogOnlyDown2 = new StreamWriter(new FileStream(sMessageLogPath + "messageLogOnlyDown2.txt", FileMode.Create));
        public StreamWriter swLogOnlyDown4 = new StreamWriter(new FileStream(sMessageLogPath + "messageLogOnlyDown4.txt", FileMode.Create));
        public StreamWriter swLogOnlyDown6 = new StreamWriter(new FileStream(sMessageLogPath + "messageLogOnlyDown6.txt", FileMode.Create));
        public StreamWriter swLogOnlyDown8 = new StreamWriter(new FileStream(sMessageLogPath + "messageLogOnlyDown8.txt", FileMode.Create));

        public StreamWriter swLogMarketSituSec = new StreamWriter(new FileStream(sMessageLogPath + "messageLogMarketSituSec.txt", FileMode.Create));
        public StreamWriter swLogMarketSituMin = new StreamWriter(new FileStream(sMessageLogPath + "messageLogMarketSituMin.txt", FileMode.Create));


        public const int REC_NUM = 8;
        public double[,] arrKospiIndex = new double[BRUSH + 390, REC_NUM];
        public double[,] arrKosdaqIndex = new double[BRUSH + 390, REC_NUM];

        public double fKospiIndexFirst;
        public double fKospiIndexEnd;
        public double fKospiIndexMax;
        public double fKospiIndexMin;
        public double fKospiIndexFollow;
        public int nKospiIndexIdxPointer;

        public double fKosdaqIndexFirst;
        public double fKosdaqIndexEnd;
        public double fKosdaqIndexMax;
        public double fKosdaqIndexMin;
        public double fKosdaqIndexFollow;
        public int nKosdaqIndexIdxPointer;

        public double fKosdaqGap;
        public double fKospiGap;

        public double fCurKospiIndexGap;
        public double fCurKosdaqIndexGap;
        public double fInitKospiIndexGap;
        public double fInitKosdaqIndexGap;

        public double fCurKospiIndexUnGap;
        public double fCurKosdaqIndexUnGap;
        public double fInitKospiIndexUnGap;
        public double fInitKosdaqIndexUnGap;

        public double fCurKospiGapInterestRatio;
        public double fCurKosdaqGapInterestRatio;
        public double fCurKospiUnGapInterestRatio;
        public double fCurKosdaqUnGapInterestRatio;

        public int nCurMarketSec;
        public int nCurMarketMin = -1;
        public int nPrevMarektMin;

        public Random rand = new Random();

        public double fKospiAngleDirection;
        public double fKosdaqAngleDirection;
        
        public int nRandomi = 50;
        public int nRecentArea = 30;
        public int nFlowIdx;
        public int nFlowIdxDiff;
        public double fInclination;
        public double fRecentInclination;
        public int nInclinationCnt;
        public int nRecentInclinationCnt;
        public double fFluctuation;
        public double fResultInclinationEvg;
        public double fResultRecentInclinationEvg;
        public double fY;

        public Form1()
        {


            InitializeComponent(); // c# 고유 고정메소드  

            MappingFileToStockCodes();


            // --------------------------------------------------
            // Winform Event Handler 
            // --------------------------------------------------
            checkMyAccountInfoButton.Click += Button_Click;
            checkMyHoldingsButton.Click += Button_Click;
            setOnMarketButton.Click += Button_Click;//삭제
            setDepositCalcButton.Click += Button_Click;//삭제
            // --------------------------------------------------
            // Event Handler 
            // --------------------------------------------------
            axKHOpenAPI1.OnEventConnect += OnEventConnectHandler; // 로그인 event slot connect
            axKHOpenAPI1.OnReceiveTrData += OnReceiveTrDataHandler; // TR event slot connect
            axKHOpenAPI1.OnReceiveRealData += OnReceiveRealDataHandler; // 실시간 event slot connect
            axKHOpenAPI1.OnReceiveChejanData += OnReceiveChejanDataHandler; // 체결,접수,잔고 event slot connect

            testTextBox.AppendText("로그인 시도..\r\n"); //++
            axKHOpenAPI1.CommConnect();
        }


        // ============================================
        // 버튼 클릭 이벤트의 핸들러 메소드
        // 1. 예수금상세현황요청
        // 2. 계좌평가잔고내역요청
        // 3. (테스트용) 강제 장시작 버튼
        // 4. (테스트용) 계산용 예수금 세팅
        // ============================================
        private void Button_Click(object sender, EventArgs e)
        {

            if (sender.Equals(checkMyAccountInfoButton)) // 예수금상세현황요청
            {
                RequestDeposit();
            }
            else if (sender.Equals(checkMyHoldingsButton)) // 계좌평가현황요청 
            {
                isForCheckHoldings = true;
                RequestHoldings(0);
            }
            else if (sender.Equals(setOnMarketButton))//삭제
            {
                isMarketStart = true;
                testTextBox.AppendText("강제 장시작 완료\r\n"); //++
            }
            else if (sender.Equals(setDepositCalcButton))
            {
                depositCalcLabel.Text = nCurDepositCalc.ToString() + "(원)";
                testTextBox.AppendText("계산용예수금 세팅 완료\r\n"); //++
            }
        }





        // ============================================
        // 주식종목들을 특정 txt파일에서 읽어
        // 코스닥, 코스피 변수에 string[] 형식으로 각각 저장
        // 코스닥, 코스피 종목갯수의 합만큼의 eachStockArray구조체 배열을 생성
        // ============================================
        private void MappingFileToStockCodes()
        {
            kosdaqCodes = System.IO.File.ReadAllLines("today_kosdaq_stock_code.txt");
            kospiCodes = System.IO.File.ReadAllLines("today_kospi_stock_code.txt");

            //kosdaqCodes = new string[0];
            //kospiCodes = new string[0];

            testTextBox.AppendText("Kosdaq : " + kosdaqCodes.Length.ToString() + "\r\n"); //++
            testTextBox.AppendText("Kospi : " + kospiCodes.Length.ToString() + "\r\n"); //++

            nStockLength = kosdaqCodes.Length + kospiCodes.Length;

            eachStockIdxArray = new int[MAX_STOCK_NUM];
            eachStockArray = new EachStock[nStockLength];
            buySlotArray = new BuySlot[nStockLength, BUY_SLOT_NUM]; // ex. [1600, 50]
            buySlotCntArray = new int[nStockLength]; // ex. [1600]
        }

        public void Delay(int ms)
        {
            DateTime dateTimeNow = DateTime.Now;
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, ms);
            DateTime dateTimeAdd = dateTimeNow.Add(duration);
            while (dateTimeAdd >= dateTimeNow)
            {
                System.Windows.Forms.Application.DoEvents();
                dateTimeNow = DateTime.Now;
            }
            return;
        }


        // ============================================
        // 매매용 화면번호 재설정 메소드
        // ============================================
        private string SetTradeScreenNo()
        {
            if (nTradeScreenNum > TRADE_SCREEN_NUM_END)
                nTradeScreenNum = TRADE_SCREEN_NUM_START;

            string sTradeScreenNum = nTradeScreenNum.ToString();
            nTradeScreenNum++;
            return sTradeScreenNum;

        }

        // ============================================
        // 실시간용 화면번호 재설정 메소드
        // ============================================
        private string SetRealScreenNo()
        {
            if (nRealScreenNum > REAL_SCREEN_NUM_END)
                nRealScreenNum = REAL_SCREEN_NUM_START;

            string sRealScreenNum = nRealScreenNum.ToString();
            nRealScreenNum++;
            return sRealScreenNum;
        }


        // ============================================
        // Tr용 화면번호 재설정메소드
        // ============================================
        private string SetTrScreenNo()
        {
            if (nTrScreenNum > TR_SCREEN_NUM_END)
                nTrScreenNum = TR_SCREEN_NUM_START;

            string sTrScreenNum = nTrScreenNum.ToString();
            nTrScreenNum++;
            return sTrScreenNum;
        }




        // ============================================
        // string형  코스닥, 코스피 종목코드의 배열 string[n] 변수에서
        // 한 화면번호 당 (최대)100개씩 넣고 주식체결 fid를 넣고
        // 실시간 데이터 요청을 진행
        // 코스닥과 코스피 배열에서 100개가 안되는 나머지 종목들은 코스닥,코스피 각 다른 화면번호에 실시간 데이터 요청
        // ============================================
        private void SubscribeRealData()
        {
            testTextBox.AppendText("구독 시작..\r\n"); //++
            int kosdaqIndex = 0;
            int kosdaqCodesLength = kosdaqCodes.Length;
            int kosdaqIterNum = kosdaqCodesLength / NUM_SEP_PER_SCREEN;
            int kosdaqRestNum = kosdaqCodesLength % NUM_SEP_PER_SCREEN;
            string strKosdaqCodeList;
            const string sFID = "41;228"; // 체결강도. 실시간 목록 FID들 중 겹치는게 가장 적은 FID
            string sScreenNum;
            // ------------------------------------------------------
            // 코스닥 실시간 등록
            // ------------------------------------------------------
            // 100개 단위
            for (int i = 0; i < kosdaqIterNum; i++)
            {
                sScreenNum = SetRealScreenNo();
                strKosdaqCodeList = ConvertStrCodeList(kosdaqCodes, kosdaqIndex, kosdaqIndex + NUM_SEP_PER_SCREEN, KOSDAQ_ID, sScreenNum);
                axKHOpenAPI1.SetRealReg(sScreenNum, strKosdaqCodeList, sFID, "0");
                kosdaqIndex += NUM_SEP_PER_SCREEN;
            }
            // 나머지
            if (kosdaqRestNum > 0)
            {
                sScreenNum = SetRealScreenNo();
                strKosdaqCodeList = ConvertStrCodeList(kosdaqCodes, kosdaqIndex, kosdaqIndex + kosdaqRestNum, KOSDAQ_ID, sScreenNum);
                axKHOpenAPI1.SetRealReg(sScreenNum, strKosdaqCodeList, sFID, "0");
            }

            int kospiIndex = 0;
            int kospiCodesLength = kospiCodes.Length;
            int kospiIterNum = kospiCodesLength / NUM_SEP_PER_SCREEN;
            int kospiRestNum = kospiCodesLength % NUM_SEP_PER_SCREEN;
            string strKospiCodeList;

            // ------------------------------------------------------
            // 코스피 실시간 등록
            // ------------------------------------------------------
            // 100개 단위
            for (int i = 0; i < kospiIterNum; i++)
            {
                sScreenNum = SetRealScreenNo();
                strKospiCodeList = ConvertStrCodeList(kospiCodes, kospiIndex, kospiIndex + NUM_SEP_PER_SCREEN, KOSPI_ID, sScreenNum);
                axKHOpenAPI1.SetRealReg(sScreenNum, strKospiCodeList, sFID, "0");
                kospiIndex += NUM_SEP_PER_SCREEN;
            }
            // 나머지
            if (kospiRestNum > 0)
            {
                sScreenNum = SetRealScreenNo();
                strKospiCodeList = ConvertStrCodeList(kospiCodes, kospiIndex, kospiIndex + kospiRestNum, KOSPI_ID, sScreenNum);
                axKHOpenAPI1.SetRealReg(sScreenNum, strKospiCodeList, sFID, "0");
            }
            testTextBox.AppendText("구독 완료..\r\n"); //++
        }




        // ============================================
        // 매개변수 : 
        //  1.  string[] codes : 주식종목코드 배열
        //  2.  s : 배열의 시작 인덱스
        //  3.  e : 배열의 끝 인덱스 (포함 x)
        //  4.  marketGubun : 코스닥, 코스피 구별변수
        //  5.  sScreenNum : 실시간 화면번호
        //
        // 키움 실시간 신청메소드의 두번째 인자인 strCodeList는
        // 종목코드1;종목코드2;종목코드3;....;종목코드n(;마지막은 생략가능) 형식으로 넘겨줘야하기 때문에
        // s부터 e -1 인덱스까지 string 변수에 추가하며 사이사이 ';'을 붙여준다
        //
        // 실시간메소드에서 각 종목의 구조체를 사용하기 위해 초기화과정이 필요한데
        // 이 메소드에서 같이 진행해준다.
        // ============================================
        private string ConvertStrCodeList(string[] codes, int s, int e, int marketGubun, string sScreenNum)
        {
            string sCodeList = "";
            string sEachBasicInfo;
            string[] sBasicInfoSplited;

            for (int i = s; i < e; i++)
            {
                int codeIdx = int.Parse(codes[i]);

                // TODO. Map(java) 기능과 속도 비교 후 수정 예정
                ////// eachStockIdx 설정 부분 ///////
                eachStockIdxArray[codeIdx] = nEachStockIdx;
                nEachStockIdx++;
                /////////////////////////////////////

                ////// eachStock 초기화 부분 //////////
                nCurIdx = eachStockIdxArray[codeIdx];
                eachStockArray[nCurIdx].sRealScreenNum = sScreenNum;
                eachStockArray[nCurIdx].sCode = codes[i];
                eachStockArray[nCurIdx].nMarketGubun = marketGubun;
                eachStockArray[nCurIdx].arrRecord = new double[BRUSH + 390, REC_NUM];


                bool isEmpty = false; 
                try
                {
                    sr = new StreamReader(sBasicInfoPath + codes[i] + ".txt");
                    sEachBasicInfo = sr.ReadLine();
                    isEmpty = true;
                    sBasicInfoSplited = sEachBasicInfo.Split(',');

                    eachStockArray[nCurIdx].sCodeName = sBasicInfoSplited[0];
                    eachStockArray[nCurIdx].lShareOutstanding = long.Parse(sBasicInfoSplited[1]);
                    eachStockArray[nCurIdx].fShareOutstandingRatio = double.Parse(sBasicInfoSplited[2]);
                    eachStockArray[nCurIdx].f250TopCompare = double.Parse(sBasicInfoSplited[3]);
                    eachStockArray[nCurIdx].f250BottomCompare = double.Parse(sBasicInfoSplited[4]);
                    eachStockArray[nCurIdx].lTotalNumOfStock = long.Parse(sBasicInfoSplited[5]);
                    eachStockArray[nCurIdx].nYesterdayEndPrice = Math.Abs(int.Parse(sBasicInfoSplited[6]));


                    if (eachStockArray[nCurIdx].nMarketGubun == KOSDAQ_ID)
                    {
                        fInitKosdaqIndexGap += (double)(eachStockArray[nCurIdx].lTotalNumOfStock * eachStockArray[nCurIdx].nYesterdayEndPrice) / INDEX_DEGREE;
                        fCurKosdaqIndexGap += (double)(eachStockArray[nCurIdx].lTotalNumOfStock * eachStockArray[nCurIdx].nYesterdayEndPrice) / INDEX_DEGREE;

                        fInitKosdaqIndexUnGap += (double)(eachStockArray[nCurIdx].lTotalNumOfStock * eachStockArray[nCurIdx].nYesterdayEndPrice) / INDEX_DEGREE;
                        fCurKosdaqIndexUnGap += (double)(eachStockArray[nCurIdx].lTotalNumOfStock * eachStockArray[nCurIdx].nYesterdayEndPrice) / INDEX_DEGREE;
                    }
                    else
                    {
                        fInitKospiIndexGap += (double)(eachStockArray[nCurIdx].lTotalNumOfStock * eachStockArray[nCurIdx].nYesterdayEndPrice) / INDEX_DEGREE;
                        fCurKospiIndexGap += (double)(eachStockArray[nCurIdx].lTotalNumOfStock * eachStockArray[nCurIdx].nYesterdayEndPrice) / INDEX_DEGREE;

                        fInitKospiIndexUnGap += (double)(eachStockArray[nCurIdx].lTotalNumOfStock * eachStockArray[nCurIdx].nYesterdayEndPrice) / INDEX_DEGREE;
                        fCurKospiIndexUnGap += (double)(eachStockArray[nCurIdx].lTotalNumOfStock * eachStockArray[nCurIdx].nYesterdayEndPrice) / INDEX_DEGREE;
                    }
                    sr.Close();
                }
                catch (Exception ex)
                {
                    if (isEmpty)
                        sr.Close();
                    srCnt += 1;
                    RequestBasicStockInfo(codes[i]);
                    testTextBox.AppendText(codes[i] + " 종목은 기존파일이 없어서 " + srCnt.ToString() + "번째 TR요청\r\n");
                    Delay(1000);
                }

                string sDate = DateTime.Now.ToString("yyyy-MM-dd"); //삭제예정
                eachStockArray[nCurIdx].swLog = new StreamWriter(new FileStream(sMessageLogPath + sDate + "-" + eachStockArray[nCurIdx].sCode + "-" + eachStockArray[nCurIdx].sCodeName +".txt", FileMode.Create));
                //////////////////////////////////////

                sCodeList += codes[i];
                if (i < e - 1)
                    sCodeList += ';';
            }
            return sCodeList;
        }




        // ============================================
        // 로그인 이벤트발생시 핸들러 메소드
        // ============================================
        private void OnEventConnectHandler(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnEventConnectEvent e)
        {
            if (e.nErrCode == 0) // 로그인 성공
            {



                testTextBox.AppendText("로그인 성공\r\n"); //++
                string sMyName = axKHOpenAPI1.GetLoginInfo("USER_NAME");
                string sAccList = axKHOpenAPI1.GetLoginInfo("ACCLIST"); // 로그인 사용자 계좌번호 리스트 요청
                string[] accountArray = sAccList.Split(';');

                sAccountNum = accountArray[0]; // 처음계좌가 main계좌
                accountComboBox.Text = sAccountNum;
                SubscribeRealData(); // 실시간 구독 
                RequestDeposit(); // 예수금상세현황요청 


                foreach (string sAccount in accountArray)
                {
                    if (sAccount.Length > 0)
                        accountComboBox.Items.Add(sAccount);
                }
                myNameLabel.Text = sMyName;

            }
            else
            {
                MessageBox.Show("로그인 실패");
            }
        } // END ---- 로그인 이벤트 핸들러





        // ============================================
        // 계좌평가잔고내역요청 TR요청메소드
        // CommRqData 3번째 인자 sPrevNext가 0일 경우 처음 20개의 종목을 요청하고
        // 2일 경우 초기20개 초과되는 종목들을 계속해서 요청한다.
        // ============================================
        private void RequestHoldings(int sPrevNext)
        {
            if (sPrevNext == 0)
            {
                nHoldingCnt = 0;
                nCurHoldingsIdx = 0;
            }
            axKHOpenAPI1.SetInputValue("계좌번호", sAccountNum);
            axKHOpenAPI1.SetInputValue("비밀번호", "");
            axKHOpenAPI1.SetInputValue("비밀번호입력매체구분", "00");
            axKHOpenAPI1.SetInputValue("조회구분", "2"); // 1:합산 2:개별
            axKHOpenAPI1.CommRqData("계좌평가잔고내역요청", "opw00018", sPrevNext, SetTrScreenNo());
        }


        // ============================================
        // 예수금상세현황요청 TR요청메소드
        // ============================================
        private void RequestDeposit()
        {
            axKHOpenAPI1.SetInputValue("계좌번호", sAccountNum);
            axKHOpenAPI1.SetInputValue("비밀번호", "");
            axKHOpenAPI1.SetInputValue("비밀번호입력매체구분", "00");
            axKHOpenAPI1.SetInputValue("조회구분", "2");
            axKHOpenAPI1.CommRqData("예수금상세현황요청", "opw00001", 0, SetTrScreenNo());
        }

        // ============================================
        // 당일실현손익상세요청 TR요청메소드
        // ============================================
        private void RequestTradeResult()
        {
            axKHOpenAPI1.SetInputValue("계좌번호", sAccountNum);
            axKHOpenAPI1.SetInputValue("비밀번호", "");
            axKHOpenAPI1.SetInputValue("종목코드", "");
            axKHOpenAPI1.CommRqData("당일실현손익상세요청", "opt10077", 0, SetTrScreenNo());
        }

        // ============================================
        // 주식기본정보요청 TR요청메소드
        // ============================================
        private void RequestBasicStockInfo(string sCode)
        {
            axKHOpenAPI1.SetInputValue("종목코드", sCode);
            axKHOpenAPI1.CommRqData("주식기본정보요청", "opt10001", 0, SetTrScreenNo());
        }

        // ============================================
        // TR 이벤트발생시 핸들러 메소드
        // ============================================
        private void OnReceiveTrDataHandler(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent e)
        {
            if (e.sRQName.Equals("예수금상세현황요청"))
            {
                nCurDeposit = Math.Abs(int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, 0, "주문가능금액")));
                if (!isDepositSet)
                {
                    nCurDepositCalc = nCurDeposit;
                    depositCalcLabel.Text = nCurDepositCalc.ToString() + "(원)";
                    testTextBox.AppendText("계산용예수금 세팅 완료\r\n"); //++
                    isDepositSet = true;
                }
                testTextBox.AppendText("예수금 세팅 완료\r\n"); //++
                myDepositLabel.Text = nCurDeposit.ToString() + "(원)";
            }
            else if (e.sRQName.Equals("계좌평가잔고내역요청"))
            {
                int rows = axKHOpenAPI1.GetRepeatCnt(e.sTrCode, e.sRecordName);
                nHoldingCnt += rows;

                for (int i = 0; nCurHoldingsIdx < nHoldingCnt; nCurHoldingsIdx++, i++)
                {
                    holdingsArray[nCurHoldingsIdx].sCode = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, i, "종목번호").Trim().Substring(1);
                    holdingsArray[nCurHoldingsIdx].sCodeName = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, i, "종목명").Trim();
                    holdingsArray[nCurHoldingsIdx].fYield = double.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, i, "수익률(%)"));
                    holdingsArray[nCurHoldingsIdx].nHoldingQty = Math.Abs(int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, i, "보유수량")));
                    holdingsArray[nCurHoldingsIdx].nBuyedPrice = Math.Abs(int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, i, "매입가")));
                    holdingsArray[nCurHoldingsIdx].nCurPrice = Math.Abs(int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, i, "현재가")));
                    holdingsArray[nCurHoldingsIdx].nTotalPL = Math.Abs(int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, i, "평가손익")));
                    holdingsArray[nCurHoldingsIdx].nNumPossibleToSell = Math.Abs(int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, i, "매매가능수량")));
                }

                if (e.sPrevNext.Equals("2"))
                {
                    RequestHoldings(2);
                }
                else // 보유잔고 확인 끝
                {
                    if (isForCheckHoldings)
                    {
                        isForCheckHoldings = false;
                        if (nHoldingCnt == 0)
                        {
                            testTextBox.AppendText("현재 보유종목이 없습니다.\r\n");//++
                        }
                        else
                        {
                            for (int i = 0; i < nHoldingCnt; i++)
                            {
                                testTextBox.AppendText((i + 1).ToString() + " 종목번호 : " + holdingsArray[i].sCode + ", 종목명 : " + holdingsArray[i].sCodeName + ", 보유수량 : " + holdingsArray[i].nHoldingQty.ToString() + ", 평가손익 : " + holdingsArray[i].nTotalPL.ToString() + "\r\n"); //++
                            }
                        }
                    }
                    else if ((nFirstDisposal == 0) || (nShutDown > 0))
                    {
                        nFirstDisposal++;

                        if (nHoldingCnt == 0)
                        {
                            testTextBox.AppendText("현재 보유종목이 없습니다.\r\n");//++
                        }
                        else
                        {
                            for (int i = 0; i < nHoldingCnt; i++)
                            {
                                int nSellReqResult = axKHOpenAPI1.SendOrder("시간초과매도", SetTradeScreenNo(), sAccountNum, 2, holdingsArray[i].sCode, holdingsArray[i].nNumPossibleToSell, 0, "03", "");

                                if (nSellReqResult != 0) // 요청이 성공하지 않으면
                                {
                                    testTextBox.AppendText(holdingsArray[i].sCode + " 매도신청 전송 실패 \r\n"); //++

                                }
                                else
                                {
                                    nCurIdx = eachStockIdxArray[int.Parse(holdingsArray[i].sCode)];
                                    eachStockArray[nCurIdx].nSellReqCnt++;
                                    testTextBox.AppendText((i + 1).ToString() + " " + holdingsArray[i].sCode + " 매도신청 전송 성공 \r\n"); //++
                                }
                                Delay(300); // 1초에 5번 제한이지만 혹시 모르니 1초에 3번정도로 제한으로
                            }
                        }
                    }
                }
            } // END ---- e.sRQName.Equals("계좌평가잔고내역요청")

            else if (e.sRQName.Equals("당일실현손익상세요청"))
            {
                int nTodayResult = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, 0, "당일실현손익"));
                testTextBox.AppendText("당일실현손익 : " + nTodayResult.ToString() + "(원) \r\n"); //++
                int rows = axKHOpenAPI1.GetRepeatCnt(e.sTrCode, e.sRecordName);

                string sCode;
                string sCodeName;
                int nTradeVolume;
                double fBuyPrice;
                int nTradePrice;
                double fYield;

                for (int i = 0; i < rows; i++)
                {
                    sCode = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, i, "종목코드").Trim().Substring(1);
                    sCodeName = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, i, "종목명").Trim();
                    fYield = double.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, i, "손익율"));
                    nTradeVolume = Math.Abs(int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, i, "체결량")));
                    fBuyPrice = Math.Abs(double.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, i, "매입단가")));
                    nTradePrice = Math.Abs(int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, i, "체결가")));

                    testTextBox.AppendText("종목명 : " + sCodeName + ", 종목코드 : " + sCode + ", 체결량 : " + nTradeVolume.ToString() + ", 매입단가 : " + fBuyPrice.ToString() + ", 체결가 : " + nTradePrice.ToString() + ", 손익율 : " + fYield.ToString() + "(%) \r\n"); //++
                }
            } // END ---- e.sRQName.Equals("당일실현손익상세요청")
            else if (e.sRQName.Equals("주식기본정보요청"))
            {
                int nCodeIdx = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, 0, "종목코드"));
                nCurIdx = eachStockIdxArray[nCodeIdx];

                eachStockArray[nCurIdx].sCodeName = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, 0, "종목명").Trim();
                try
                {
                    eachStockArray[nCurIdx].lShareOutstanding = long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, 0, "유통주식")) * 1000;
                    eachStockArray[nCurIdx].fShareOutstandingRatio = double.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, 0, "유통비율"));
                    eachStockArray[nCurIdx].lTotalNumOfStock = long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, 0, "상장주식")) * 1000; ;
                    eachStockArray[nCurIdx].nYesterdayEndPrice = Math.Abs(int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, 0, "현재가")));
                }
                catch (Exception ex)
                {
                    eachStockArray[nCurIdx].lShareOutstanding = long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, 0, "상장주식")) * 1000;
                    eachStockArray[nCurIdx].fShareOutstandingRatio = 100.0;
                    eachStockArray[nCurIdx].lTotalNumOfStock = eachStockArray[nCurIdx].lShareOutstanding;
                    eachStockArray[nCurIdx].nYesterdayEndPrice = Math.Abs(int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, 0, "현재가")));
                }
                try
                {
                    eachStockArray[nCurIdx].f250TopCompare = double.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, 0, "250최고가대비율"));
                }
                catch (Exception ex)
                {
                    eachStockArray[nCurIdx].f250TopCompare = 0.0;
                }

                try
                {
                    eachStockArray[nCurIdx].f250BottomCompare = double.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, 0, "250최저가대비율"));
                }
                catch (Exception ex)
                {
                    eachStockArray[nCurIdx].f250BottomCompare = 0.0;
                }

                if (eachStockArray[nCurIdx].nMarketGubun == KOSDAQ_ID)
                {
                    fInitKosdaqIndexGap += (double)(eachStockArray[nCurIdx].lTotalNumOfStock * eachStockArray[nCurIdx].nYesterdayEndPrice) / INDEX_DEGREE;
                    fCurKosdaqIndexGap += (double)(eachStockArray[nCurIdx].lTotalNumOfStock * eachStockArray[nCurIdx].nYesterdayEndPrice) / INDEX_DEGREE;

                    fInitKosdaqIndexUnGap += (double)(eachStockArray[nCurIdx].lTotalNumOfStock * eachStockArray[nCurIdx].nYesterdayEndPrice) / INDEX_DEGREE;
                    fCurKosdaqIndexUnGap += (double)(eachStockArray[nCurIdx].lTotalNumOfStock * eachStockArray[nCurIdx].nYesterdayEndPrice) / INDEX_DEGREE;
                }
                else
                {
                    fInitKospiIndexGap += (double)(eachStockArray[nCurIdx].lTotalNumOfStock * eachStockArray[nCurIdx].nYesterdayEndPrice) / INDEX_DEGREE;
                    fCurKospiIndexGap += (double)(eachStockArray[nCurIdx].lTotalNumOfStock * eachStockArray[nCurIdx].nYesterdayEndPrice) / INDEX_DEGREE;

                    fInitKospiIndexUnGap += (double)(eachStockArray[nCurIdx].lTotalNumOfStock * eachStockArray[nCurIdx].nYesterdayEndPrice) / INDEX_DEGREE;
                    fCurKospiIndexUnGap += (double)(eachStockArray[nCurIdx].lTotalNumOfStock * eachStockArray[nCurIdx].nYesterdayEndPrice) / INDEX_DEGREE;
                }

                StreamWriter tmpSw = new StreamWriter(new FileStream(sBasicInfoPath + eachStockArray[nCurIdx].sCode + ".txt", FileMode.Create));
                tmpSw.Write(eachStockArray[nCurIdx].sCodeName + "," + eachStockArray[nCurIdx].lShareOutstanding.ToString() + "," + eachStockArray[nCurIdx].fShareOutstandingRatio.ToString() + "," + eachStockArray[nCurIdx].f250TopCompare.ToString() + "," + eachStockArray[nCurIdx].f250BottomCompare.ToString() + "," + eachStockArray[nCurIdx].lTotalNumOfStock.ToString() + "," + eachStockArray[nCurIdx].nYesterdayEndPrice.ToString());
                tmpSw.Close();
            }

        } // END ---- TR 이벤트 핸들러




        // ============================================
        // 실시간 이벤트발생시 핸들러메소드 
        // ============================================
        private void OnReceiveRealDataHandler(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveRealDataEvent e)
        {
            string sCode = e.sRealKey;

            if (tradeQueue.Count > 0) // START ---- 매매컨트롤러
            {
                curSlot = tradeQueue.Dequeue(); // 우선 디큐한다

                if (isForbidTrade) // 거래정지상태
                {
                    nCurOrderTime = nSharedTime;
                    if (nCurOrderTime != nBeforeOrderTime) // 1초가 지나면 거래 풀어줌.
                    {
                        isForbidTrade = false;
                        nBeforeOrderTime = nCurOrderTime;
                        nAccumCount = 0;
                    }
                }

                if (!isForbidTrade)  // 거래정지가 아니라면
                {
                    if (SubTimeToTimeAndSec(nSharedTime, curSlot.nRqTime) <= IGNORE_REQ_SEC) // 현재시간 - 요청시간 < 10초 : 요청시간이 너무 길어진 요청의 처리를 위한 분기문  
                    {

                        if (curSlot.nOrderType <= 2) // 신규매수매도 신규매수:1 신규매도:2
                        {
                            // 아직 매수중이거나 매도중일때는
                            if ((eachStockArray[curSlot.nEachStockIdx].nBuyReqCnt > 0) || (eachStockArray[curSlot.nEachStockIdx].nSellReqCnt > 0)) // 현재 거래중이면
                            {
                                if (curSlot.nOrderType == 2) // 매도신청은 버려져서는 안됨.
                                    curSlot.nRqTime = nSharedTime; // 요청시간을 현재시간으로 설정
                                tradeQueue.Enqueue(curSlot); // 디큐했던 슬롯을 다시 인큐한다.
                            }
                            else // 거래중이 아닐때 (단, 매수취소는 예외)
                            {
                                if (curSlot.nOrderType == 1) // 신규매수
                                {
                                    int nEstimatedPrice = curSlot.nOrderPrice; // 종목의 요청했던 최우선매도호가를 받아온다.
                                                                               // 반복해서 가격을 n칸 올린다.
                                    if (eachStockArray[curSlot.nEachStockIdx].nMarketGubun == KOSDAQ_ID) // 코스닥일 경우
                                    {
                                        for (int i = 0; i < EYES_CLOSE_NUM; i++)
                                            nEstimatedPrice += GetKosdaqGap(nEstimatedPrice);
                                    }
                                    else if (eachStockArray[curSlot.nEachStockIdx].nMarketGubun == KOSPI_ID) // 코스피의 경우
                                    {
                                        for (int i = 0; i < EYES_CLOSE_NUM; i++)
                                            nEstimatedPrice += GetKospiGap(nEstimatedPrice);
                                    }

                                    double fCurLimitPriceFee = (nEstimatedPrice * (1 + VIRTUAL_STOCK_FEE));

                                    int nNumToBuy = (int)(nCurDepositCalc / fCurLimitPriceFee); // 현재 예수금으로 살 수 있을 만큼
                                    int nMaxNumToBuy = (int)(nMaxPriceForEachStock / fCurLimitPriceFee); // 최대매수가능금액으로 살 수 있을 만큼

                                    if (nNumToBuy > nMaxNumToBuy) // 최대매수가능수를 넘는다면
                                        nNumToBuy = nMaxNumToBuy; // 최대매수가능수로 세팅

                                    // 구매수량이 있고 현재종목의 최우선매도호가가 요청하려는 지정가보다 낮을 경우 구매요청을 걸 수 있다.
                                    if ((nNumToBuy > 0) && (eachStockArray[curSlot.nEachStockIdx].nFs < nEstimatedPrice))
                                    {
                                        if (curSlot.sHogaGb.Equals("03")) // 시장가모드 : 시장가로 하면 키움에서 상한가값으로 계산해서 예수금만큼 살 수 가 없다
                                        {
                                            if (buySlotCntArray[curSlot.nEachStockIdx] < BUY_LIMIT_NUM) // 개인 구매횟수를 넘기지 않았다면
                                            {
                                                eachStockArray[curSlot.nEachStockIdx].nCurLimitPrice = nEstimatedPrice; // 지정상한가 설정
                                                eachStockArray[curSlot.nEachStockIdx].fTargetPercent = curSlot.fTargetPercent; // 익절퍼센트 설정
                                                eachStockArray[curSlot.nEachStockIdx].fBottomPercent = curSlot.fBottomPercent; // 손절퍼센트 설정
                                                eachStockArray[curSlot.nEachStockIdx].nCurRqTime = nSharedTime; // 현재시간설정

                                                testTextBox.AppendText(eachStockArray[curSlot.nEachStockIdx].nCurRqTime.ToString() + " : " + curSlot.sCode + " 매수신청 전송 \r\n"); //++
                                                int nBuyReqResult = axKHOpenAPI1.SendOrder(curSlot.sRQName, SetTradeScreenNo(), sAccountNum,
                                                    curSlot.nOrderType, curSlot.sCode, nNumToBuy, nEstimatedPrice,
                                                    "00", curSlot.sOrgOrderId); // 높은 매도호가에 지정가로 걸어 시장가처럼 사게 한다
                                                                                // 최우선매도호가보다 높은 가격에 지정가를 걸면 현재매도호가에 구매하게 된다.
                                                isSendOrder = true;
                                                nCurOrderTime = nSharedTime;
                                                if (nBuyReqResult == 0) // 요청이 성공하면
                                                {
                                                    eachStockArray[curSlot.nEachStockIdx].nBuyReqCnt++; // 구매횟수 증가
                                                    testTextBox.AppendText(curSlot.sCode + " 매수신청 전송 성공 \r\n"); //++
                                                }
                                            }
                                            else  // 개인 구매횟수를 넘겼다면
                                                testTextBox.AppendText(curSlot.sCode + " 종목의 구매횟수를 초과했습니다.\r\n"); //++
                                        }
                                    }
                                } // END ---- 신규매수
                                else if (curSlot.nOrderType == 2) // 신규매도
                                {
                                    if (curSlot.sHogaGb.Equals("03")) // 시장가매도
                                    {
                                        eachStockArray[curSlot.nEachStockIdx].nCurRqTime = nSharedTime; // 현재시간설정

                                        testTextBox.AppendText(nSharedTime.ToString() + " : " + curSlot.sCode + " 매도신청 전송 \r\n"); //++
                                        int nSellReqResult = axKHOpenAPI1.SendOrder(curSlot.sRQName, SetTradeScreenNo(), sAccountNum,
                                                curSlot.nOrderType, curSlot.sCode, curSlot.nQty, 0,
                                                curSlot.sHogaGb, curSlot.sOrgOrderId);
                                        isSendOrder = true;
                                        nCurOrderTime = nSharedTime;
                                        if (nSellReqResult != 0) // 요청이 성공하지 않으면
                                        {
                                            testTextBox.AppendText(curSlot.sCode + " 매도신청 전송 실패 \r\n"); //++
                                            buySlotArray[curSlot.nEachStockIdx, curSlot.nBuySlotIdx].isSelled = false; // 요청실패일때 다시 요청하기 위해
                                                                                                                       // 해당 buySlot에서 판매완료시그널을 false로 세팅해준다
                                                                                                                       // 이 작업을 하기 위해서 TradeSlot 구조체에 nBuySlotIdx 변수가 필요한것이다.
                                        }
                                        else
                                        {
                                            testTextBox.AppendText(curSlot.sCode + " 매도신청 전송 성공 \r\n"); //++
                                            eachStockArray[curSlot.nEachStockIdx].nSellReqCnt++; // 매도요청전송이 성공하면 매도횟수를 증가한다.
                                        }
                                    }
                                } // END ---- 신규매도
                            }
                        } // End ---- 신규매수매도
                        else if (curSlot.nOrderType == 3) // 매수취소 매수취소는 매수중일때만 요청되고 매수와 함께 슬롯입장이 가능하다. 매도중일때는 안된다.
                        {
                            // 구매중일때만 매수취소가 가능하니 buySlotArray의 인덱스는 매수취소종목의 마지막인덱스로 확정되니
                            // 건들지 않는다

                            testTextBox.AppendText(nSharedTime.ToString() + " : " + curSlot.sCode + " 매수취소신청 전송 \r\n"); //++
                            int nCancelReqResult = axKHOpenAPI1.SendOrder(curSlot.sRQName, SetTradeScreenNo(), sAccountNum,
                                curSlot.nOrderType, curSlot.sCode, 0, 0,
                                "", curSlot.sOrgOrderId);
                            isSendOrder = true;
                            nCurOrderTime = nSharedTime;
                            if (nCancelReqResult != 0) // 매수취소가 성공하지 않으면
                            {
                                eachStockArray[curSlot.nEachStockIdx].isCancelMode = false; // 해당종목의 현재 매수취소시그널을 false한다
                                                                                            // 이래야지 매수취소를 다시 신청할 수 있다.
                            }
                            else
                            {
                                testTextBox.AppendText(curSlot.sCode + " 매수취소신청 전송 성공 \r\n"); //++
                            }
                        } // End ---- 매수취소


                        if (isSendOrder) // 주문을 요청했을때만
                        {
                            isSendOrder = false;
                            if (nCurOrderTime != nBeforeOrderTime) // 주문시간이 이전주문시간과 다르다 == 1초 제한이 아니다
                            {
                                nBeforeOrderTime = nCurOrderTime;
                                nAccumCount = 1;
                            }
                            else  // 주문시간이 이전시간과 같다 == 1초 제한 카운트 증가
                            {
                                nAccumCount++;
                                if (nAccumCount >= (LIMIT_SENDORDER_NUM - 1)) // 5번제한이지만 혹시 모르니 4번제한으로
                                {
                                    isForbidTrade = true; // 제한에 걸리면 1초가 지날때까지는 매매 금지
                                }
                            }
                        } // END ---- 주문을 요청했을때만

                    } // End ---- 현재시간 - 요청시간 < 10초
                } // END ---- 거래정지가 아니라면
            } // END ---- 매매컨트롤러


            if (e.sRealType.Equals("주식호가잔량")) // ##호가##
            {
                int nCodeIdx = int.Parse(sCode);
                nCurIdx = eachStockIdxArray[nCodeIdx];

                if (isMarketStart && !eachStockArray[nCurIdx].isExcluded)
                {

                    try
                    {
                        int a = int.Parse(axKHOpenAPI1.GetCommRealData(sCode, 64)); // 매도4호가잔량
                        int b = int.Parse(axKHOpenAPI1.GetCommRealData(sCode, 74)); // 매수4호가잔량

                        if ( a == 0 && b == 0 )
                        {
                            eachStockArray[nCurIdx].isViMode = true;
                        }
                        else
                        {
                            if (eachStockArray[nCurIdx].isViMode)
                            {
                                eachStockArray[nCurIdx].isViMode = false;
                                eachStockArray[nCurIdx].nViTime = AddTimeBySec(nSharedTime, 20); // vi가 걸렸을때 가격변동이 있을 수 있으니 그때를 대비한 시간
                            }

                        }

                       
                    }

                    catch (Exception ex)
                    {

                    }

                    if (!eachStockArray[nCurIdx].isViMode)
                    {
                        try
                        {
                            eachStockArray[nCurIdx].nTotalBuyHogaVolume = Math.Abs(int.Parse(axKHOpenAPI1.GetCommRealData(sCode, 125)));  // 매수호가총잔량
                        }
                        catch (Exception ex)
                        {
                            eachStockArray[nCurIdx].nTotalBuyHogaVolume = 0;
                        }
                        try
                        {
                            eachStockArray[nCurIdx].nTotalSellHogaVolume = Math.Abs(int.Parse(axKHOpenAPI1.GetCommRealData(sCode, 121)));  // 매도호가총잔량
                        }
                        catch (Exception ex)
                        {
                            eachStockArray[nCurIdx].nTotalSellHogaVolume = 0;
                        }

                        eachStockArray[nCurIdx].nTotalHogaVolume = eachStockArray[nCurIdx].nTotalBuyHogaVolume + eachStockArray[nCurIdx].nTotalSellHogaVolume;

                        if (eachStockArray[nCurIdx].nTotalHogaVolume > 0)
                        {
                            eachStockArray[nCurIdx].fCurHogaRatio = (double)(eachStockArray[nCurIdx].nTotalSellHogaVolume - eachStockArray[nCurIdx].nTotalBuyHogaVolume) / eachStockArray[nCurIdx].nTotalHogaVolume;
                        }
                        else
                        {
                            eachStockArray[nCurIdx].fCurHogaRatio = 0.0;
                        }


                        // 총호가량 벨 변수 작업
                        if (eachStockArray[nCurIdx].fTotalHogaVolumeVal == 0)
                            eachStockArray[nCurIdx].fTotalHogaVolumeVal = eachStockArray[nCurIdx].nTotalHogaVolume;
                        else
                            eachStockArray[nCurIdx].fTotalHogaVolumeVal = eachStockArray[nCurIdx].nTotalHogaVolume * fPushWeight + eachStockArray[nCurIdx].fTotalHogaVolumeVal * (1 - fPushWeight);

                        // 호가비율 벨 변수 작업
                        if (eachStockArray[nCurIdx].fHogaRatioVal == 0 || double.IsNaN(eachStockArray[nCurIdx].fHogaRatioVal))
                            eachStockArray[nCurIdx].fHogaRatioVal = eachStockArray[nCurIdx].fCurHogaRatio;
                        else
                            eachStockArray[nCurIdx].fHogaRatioVal = eachStockArray[nCurIdx].fCurHogaRatio * fPushWeight + eachStockArray[nCurIdx].fHogaRatioVal * (1 - fPushWeight);

                    }
                }
            }
            else if (e.sRealType.Equals("주식체결")) // ##체결##
            {

                nSharedTime = Math.Abs(int.Parse(axKHOpenAPI1.GetCommRealData(sCode, 20))); // 현재시간
                int nCodeIdx = int.Parse(sCode);
                nCurIdx = eachStockIdxArray[nCodeIdx];

                if (nFirstTime == 0)
                {
                    nFirstTime = nSharedTime;
                    nTenTime = AddTimeBySec(nFirstTime, 3600);
                }

                if (isMarketStart && !eachStockArray[nCurIdx].isExcluded) // 장이 시작 안했으면 접근 금지
                {


                    //if (nSharedTime >= SHUTDOWN_TIME) // 3시가 넘었으면
                    //{
                    //    testTextBox.AppendText("3시가 지났다\r\n");
                    //    nShutDown++; // 장이 끝남을 알린다 (nShutDown이 0일때는 전량매도작업을 수행하지 않기 때문에)
                    //    isMarketStart = false; // 장 중 시그널을 off한다
                    //    for (int nScreenNum = REAL_SCREEN_NUM_START; nScreenNum <= REAL_SCREEN_NUM_END; nScreenNum++)
                    //    {
                    //        // 실시간 체결에 할당된 화면번호들에 대해 다 디스커넥트한다
                    //        // 실시간 체결만 받고있는 화면번호들만이 아니라 전부를 디스커넥트하는 이유는
                    //        // 전부를 디스커넥트하는 잠깐의 시간동안 잔여 실시간주식체결데이터들이 처리되는 것을 기다리는 기능도 있다.
                    //        axKHOpenAPI1.DisconnectRealData(nScreenNum.ToString());
                    //    }

                    //    RequestHoldings(0); // 잔고현황을 체크한다. 이때 nShutDown이 양수이기 때문에 남아있는 주식들이 있으면 전량 매도한다.
                    //    return;
                    //}

                    try
                    {
                        eachStockArray[nCurIdx].nFs = Math.Abs(int.Parse(axKHOpenAPI1.GetCommRealData(sCode, 27))); // 최우선매도호가
                        eachStockArray[nCurIdx].nFb = Math.Abs(int.Parse(axKHOpenAPI1.GetCommRealData(sCode, 28))); // 최우선매수호가
                        eachStockArray[nCurIdx].nTv = int.Parse(axKHOpenAPI1.GetCommRealData(sCode, 15)); // 거래량
                        eachStockArray[nCurIdx].fTs = double.Parse(axKHOpenAPI1.GetCommRealData(sCode, 228)); // 체결강도
                        eachStockArray[nCurIdx].fPower = double.Parse(axKHOpenAPI1.GetCommRealData(sCode, 12)) / 100; // 등락율
                    }
                    catch (Exception ex)
                    {
                        return;
                    }

                    eachStockArray[nCurIdx].nCnt++; // 인덱스를 올린다.

                    if (eachStockArray[nCurIdx].nFs == 0 && eachStockArray[nCurIdx].nFb == 0)  // 둘 다 데이터가 없는경우는 가격초기화가 불가능하기 return
                        return;
                    else
                    {
                        // 둘다 제대로 받아졌거나 , 둘 중 하나가 안받아졌거나
                        if (eachStockArray[nCurIdx].nFs == 0) // fs가 안받아졌으면 fb 가격에 fb갭 한칸을 더해서 설정
                        {
                            int gap = 0;
                            if (eachStockArray[nCurIdx].nMarketGubun == KOSDAQ_ID)
                                gap = GetKosdaqGap(eachStockArray[nCurIdx].nFb);
                            else if (eachStockArray[nCurIdx].nMarketGubun == KOSPI_ID)
                                gap = GetKospiGap(eachStockArray[nCurIdx].nFb);

                            eachStockArray[nCurIdx].nFs = eachStockArray[nCurIdx].nFb + gap;
                        }
                        if (eachStockArray[nCurIdx].nFb == 0) // fb가 안받아졌으면 fs 가격에 (fs-1)갭 한칸을 마이너스해서 설정
                        {
                            // fs-1 인 이유는 fs가 1000원이라하면 fb는 999여야하는데 갭을 받을때 5를 받게되니 fb가 995가 되어버린다.이는 오류!
                            int gap = 0;
                            if (eachStockArray[nCurIdx].nMarketGubun == KOSDAQ_ID)
                                gap = GetKosdaqGap(eachStockArray[nCurIdx].nFs - 1);
                            else if (eachStockArray[nCurIdx].nMarketGubun == KOSPI_ID)
                                gap = GetKospiGap(eachStockArray[nCurIdx].nFs - 1);

                            eachStockArray[nCurIdx].nFb = eachStockArray[nCurIdx].nFs - gap;
                        }

                    }
                    eachStockArray[nCurIdx].nDiff = eachStockArray[nCurIdx].nFs - eachStockArray[nCurIdx].nFb;

                    // 이상 데이터 감지
                    // fs와 fb의 가격차이가 2퍼가 넘을경우 이상데이터라 생각하고 리턴한다.
                    // 미리 리턴하는 이유는 이런 이상 데이터로는 전략에 사용하지 않기위해서 전략찾는 부분 위에서 리턴여부를 검증한다.
                    // if ((eachStockArray[nCurIdx].nFs - eachStockArray[nCurIdx].nFb) / eachStockArray[nCurIdx].nFb > 0.02)
                    //    return;





                    // 처음가격과 시간등을 맞추려는 변수이다.
                    if (!eachStockArray[nCurIdx].isFirstCheck) // 개인 초기작업
                    {

                        //if (eachStockArray[nCurIdx].nFs < 1000) // 1000원도 안한다면 폐기처분
                        //{
                        //    axKHOpenAPI1.SetRealRemove(eachStockArray[nCurIdx].sRealScreenNum, eachStockArray[nCurIdx].sCode);
                        //    eachStockArray[nCurIdx].isExcluded = true;
                        //}

                        eachStockArray[nCurIdx].isFirstCheck = true; // 가격설정이 끝났으면 이종목의 초기체크는 완료 설정
                        int nStartGap = int.Parse(axKHOpenAPI1.GetCommRealData(sCode, 11)); // 어제종가와 비교한 가격변화

                        eachStockArray[nCurIdx].nStartGap = nStartGap; // 갭

                        if (eachStockArray[nCurIdx].nYesterdayEndPrice == 0)
                        {
                            eachStockArray[nCurIdx].nYesterdayEndPrice = eachStockArray[nCurIdx].nFs - nStartGap; // 시초가에서 변화를 제거하면 어제 종가가 나옴
                            eachStockArray[nCurIdx].nTodayStartPrice = eachStockArray[nCurIdx].nFs; // 오늘 시초가
                        }
                        else
                        {
                            eachStockArray[nCurIdx].nTodayStartPrice = eachStockArray[nCurIdx].nYesterdayEndPrice + eachStockArray[nCurIdx].nStartGap;
                        }
                        eachStockArray[nCurIdx].fStartGap = (double)eachStockArray[nCurIdx].nStartGap / eachStockArray[nCurIdx].nYesterdayEndPrice; // 갭의 등락율
                        eachStockArray[nCurIdx].lPrevTotalPriceOfStock = eachStockArray[nCurIdx].lTotalNumOfStock * eachStockArray[nCurIdx].nFs;

                        if (eachStockArray[nCurIdx].nMarketGubun == KOSDAQ_ID)
                        {

                            fCurKosdaqIndexGap += (double)(eachStockArray[nCurIdx].lTotalNumOfStock * eachStockArray[nCurIdx].nStartGap) / INDEX_DEGREE; //GAP있는 부분만 갭 영향을 주겠다.
                            fKosdaqGap += (double)(eachStockArray[nCurIdx].lTotalNumOfStock * eachStockArray[nCurIdx].nStartGap) / INDEX_DEGREE;
                        }
                        else
                        {

                            fCurKospiIndexGap += (double)(eachStockArray[nCurIdx].lTotalNumOfStock * eachStockArray[nCurIdx].nStartGap) / INDEX_DEGREE;
                            fKospiGap += (double)(eachStockArray[nCurIdx].lTotalNumOfStock * eachStockArray[nCurIdx].nStartGap) / INDEX_DEGREE;
                        }


                        if (eachStockArray[nCurIdx].fStartGap > 0.02)
                        {
                            swGapUp = new StreamWriter(new FileStream(sMessageLogPath + "GapUp.txt", FileMode.Append));
                            swGapUp.WriteLine(eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + eachStockArray[nCurIdx].fStartGap.ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString());
                            swGapUp.Close();
                        }
                        else if (eachStockArray[nCurIdx].fStartGap < -0.02)
                        {
                            swGapDown = new StreamWriter(new FileStream(sMessageLogPath + "GapDown.txt", FileMode.Append));
                            swGapDown.WriteLine(eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + eachStockArray[nCurIdx].fStartGap.ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString());
                            swGapDown.Close();
                        }
                        else
                        {
                            swGapMiddle = new StreamWriter(new FileStream(sMessageLogPath + "GapMiddle.txt", FileMode.Append));
                            swGapMiddle.WriteLine(eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + eachStockArray[nCurIdx].fStartGap.ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString());
                            swGapMiddle.Close();
                        }

                    } // END ---- 개인 초기작업


                    /// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                    /// 코스피 코스닥 지수 기록
                    /// 코스피
                    int nKospiIndexMinPointer = (int)(GetSec(nSharedTime - nFirstTime) / 60) + BRUSH; // 현재시간과 9시를 뺀 결과를 분단위로 받음
                    if (nKospiIndexIdxPointer != nKospiIndexMinPointer) // 기록된 포인터와 새로운 포인터가 다르면 (과거처리)
                    {
                        int nDiff = nKospiIndexMinPointer - nKospiIndexIdxPointer;
                        int i = 0;

                        for (i = 0; i < nDiff; i++)
                        {
                            if (nKospiIndexIdxPointer < BRUSH)
                            {
                                arrKospiIndex[nKospiIndexIdxPointer, 0] = nFirstTime;
                                arrKospiIndex[nKospiIndexIdxPointer, 1] = fInitKospiIndexUnGap;
                                arrKospiIndex[nKospiIndexIdxPointer, 2] = fInitKospiIndexUnGap;
                                arrKospiIndex[nKospiIndexIdxPointer, 3] = fInitKospiIndexUnGap;
                                arrKospiIndex[nKospiIndexIdxPointer, 4] = fInitKospiIndexUnGap;
                            }
                            else
                            {
                                arrKospiIndex[nKospiIndexIdxPointer, 0] = AddTimeBySec(nFirstTime, (nKospiIndexIdxPointer - BRUSH) * 60);
                                if (fKospiIndexEnd == 0) // 서킷브레이커 대비
                                {
                                    arrKospiIndex[nKospiIndexIdxPointer, 1] = fKospiIndexFollow;
                                    arrKospiIndex[nKospiIndexIdxPointer, 2] = fKospiIndexFollow;
                                    arrKospiIndex[nKospiIndexIdxPointer, 3] = fKospiIndexFollow;
                                    arrKospiIndex[nKospiIndexIdxPointer, 4] = fKospiIndexFollow;
                                }
                                else
                                {
                                    arrKospiIndex[nKospiIndexIdxPointer, 1] = fKospiIndexFirst;
                                    arrKospiIndex[nKospiIndexIdxPointer, 2] = fKospiIndexEnd;
                                    arrKospiIndex[nKospiIndexIdxPointer, 3] = fKospiIndexMax;
                                    arrKospiIndex[nKospiIndexIdxPointer, 4] = fKospiIndexMin;
                                }
                            }

                            fKospiIndexFirst = 0;
                            fKospiIndexEnd = 0;
                            fKospiIndexMax = 0;
                            fKospiIndexMin = 0;
                            nKospiIndexIdxPointer++;
                            if (nKospiIndexIdxPointer > BRUSH) // 추세 확인
                            {

                                fInclination = 0;
                                fRecentInclination = 0;
                                nInclinationCnt = 0;
                                nRecentInclinationCnt = 0;
                                fFluctuation = 0;

                                for (i = 0; i < nRandomi; i++)
                                {
                                    nFlowIdx = rand.Next(nKospiIndexIdxPointer);
                                    fInclination += (fCurKospiIndexUnGap - arrKospiIndex[nFlowIdx, 2]) / (SubTimeToTimeAndSec(nSharedTime, (int)arrKospiIndex[nFlowIdx, 0]) / 60);
                                    nInclinationCnt++;

                                }
                                fResultInclinationEvg = fInclination / nInclinationCnt; // 평균추세선

                                for (i = 0; i < nRandomi; i++)
                                {
                                    if (nKospiIndexIdxPointer >= nRecentArea)
                                    {
                                        nFlowIdx = rand.Next(nKospiIndexIdxPointer - nRecentArea, nKospiIndexIdxPointer);
                                    }
                                    else
                                        nFlowIdx = rand.Next(nKospiIndexIdxPointer);
                                    nRecentInclinationCnt++;
                                    fRecentInclination += (fCurKospiIndexUnGap - arrKospiIndex[nFlowIdx, 2]) / (SubTimeToTimeAndSec(nSharedTime, (int)arrKospiIndex[nFlowIdx, 0]) / 60);
                                    nFlowIdxDiff = rand.Next(nKospiIndexIdxPointer);

                                    fY = fResultInclinationEvg * (SubTimeToTimeAndSec(nSharedTime, (int)arrKospiIndex[nFlowIdx, 0]) / 60) + fCurKospiIndexUnGap - fResultInclinationEvg * (SubTimeToTimeAndSec(nSharedTime, nFirstTime) / 60); // 기울기에 따른 linear 함수

                                    fFluctuation += Math.Pow(fY - arrKospiIndex[nFlowIdx, 2], 2); // 변동폭
                                }
                                fResultRecentInclinationEvg = fRecentInclination / nRecentInclinationCnt; // 근접추세선

                                arrKospiIndex[nKospiIndexIdxPointer, 5] = fResultInclinationEvg;
                                arrKospiIndex[nKospiIndexIdxPointer, 6] = fResultRecentInclinationEvg;
                                arrKospiIndex[nKospiIndexIdxPointer, 7] = Math.Sqrt(fFluctuation);


                                double fN = fResultInclinationEvg;
                                double fM = fResultRecentInclinationEvg;
                                int nNDown = 0;
                                int nNUp = 0;

                                while (fN > 10)
                                {
                                    nNDown++;
                                    fN /= 10;
                                }
                                while (fN < 1)
                                {
                                    nNUp++;
                                    fN *= 10;
                                }
                                for (i = 0; i < nNDown; i++)
                                {
                                    fM /= 10;
                                }
                                for (i = 0; i < nNUp; i++)
                                {
                                    fM *= 10;
                                }

                                if (fN == fM) // same
                                {
                                    fKospiAngleDirection = 0;
                                }
                                else if (fN * fM == -1) // -1
                                {
                                    fKospiAngleDirection = 90;
                                }
                                else if (-1 < fN * fM) // -1 < x
                                {
                                    if (fN * fM < 0)  // -1 < x < 0
                                    {
                                        // 분자만 양수로 만들면 된다.
                                        if (fN < fM)
                                        {
                                            fKospiAngleDirection = 180 - Math.Atan((fM - fN) / (1 + fN * fM)) * (180 / Math.PI);
                                        }
                                        else // fResultRecentInclinationEvg < fResultInclinationEvg 
                                        {
                                            fKospiAngleDirection = 180 - Math.Atan((fN - fM) / (1 + fN * fM)) * (180 / Math.PI);
                                        }
                                    }
                                    else // 0 <= x 
                                    {
                                        // 분자만 양수로 만들면 된다.
                                        if (fN < fM)
                                        {
                                            fKospiAngleDirection = Math.Atan((fM - fN) / (1 + fN * fM)) * (180 / Math.PI);
                                        }
                                        else // fResultRecentInclinationEvg < fResultInclinationEvg 
                                        {
                                            fKospiAngleDirection = Math.Atan((fN - fM) / (1 + fN * fM)) * (180 / Math.PI);
                                        }
                                    }
                                }
                                else // x  < -1
                                {
                                    // 분자를 음수로 만들면 된다.
                                    if (fN < fM)
                                    {
                                        fKospiAngleDirection = Math.Atan((fN - fM) / (1 + fN * fM)) * (180 / Math.PI);
                                    }
                                    else
                                    {
                                        fKospiAngleDirection = Math.Atan((fM - fN) / (1 + fN * fM)) * (180 / Math.PI);
                                    }
                                }

                            }
                        }
                    }

                    /// 코스닥
                    int nKosdaqIndexMinPointer = (int)(GetSec(nSharedTime - nFirstTime) / 60) + BRUSH; // 현재시간과 9시를 뺀 결과를 분단위로 받음
                    if (nKosdaqIndexIdxPointer != nKosdaqIndexMinPointer) // 기록된 포인터와 새로운 포인터가 다르면 (과거처리)
                    {
                        int nDiff = nKosdaqIndexMinPointer - nKosdaqIndexIdxPointer;
                        int i = 0;

                        for (i = 0; i < nDiff; i++)
                        {
                            if (nKosdaqIndexIdxPointer < BRUSH)
                            {
                                arrKosdaqIndex[nKosdaqIndexIdxPointer, 0] = nFirstTime;
                                arrKosdaqIndex[nKosdaqIndexIdxPointer, 1] = fInitKosdaqIndexUnGap;
                                arrKosdaqIndex[nKosdaqIndexIdxPointer, 2] = fInitKosdaqIndexUnGap;
                                arrKosdaqIndex[nKosdaqIndexIdxPointer, 3] = fInitKosdaqIndexUnGap;
                                arrKosdaqIndex[nKosdaqIndexIdxPointer, 4] = fInitKosdaqIndexUnGap;
                            }
                            else
                            {
                                arrKosdaqIndex[nKosdaqIndexIdxPointer, 0] = AddTimeBySec(nFirstTime, (nKosdaqIndexIdxPointer - BRUSH) * 60);
                                if (fKosdaqIndexEnd == 0) // 서킷브레이커 대비
                                {
                                    arrKosdaqIndex[nKosdaqIndexIdxPointer, 1] = fKospiIndexFollow;
                                    arrKosdaqIndex[nKosdaqIndexIdxPointer, 2] = fKospiIndexFollow;
                                    arrKosdaqIndex[nKosdaqIndexIdxPointer, 3] = fKospiIndexFollow;
                                    arrKosdaqIndex[nKosdaqIndexIdxPointer, 4] = fKospiIndexFollow;
                                }
                                else
                                {
                                    arrKosdaqIndex[nKosdaqIndexIdxPointer, 1] = fKosdaqIndexFirst;
                                    arrKosdaqIndex[nKosdaqIndexIdxPointer, 2] = fKosdaqIndexEnd;
                                    arrKosdaqIndex[nKosdaqIndexIdxPointer, 3] = fKosdaqIndexMax;
                                    arrKosdaqIndex[nKosdaqIndexIdxPointer, 4] = fKosdaqIndexMin;
                                }
                                
                            }
                            fKosdaqIndexFirst = 0;
                            fKosdaqIndexEnd = 0;
                            fKosdaqIndexMax = 0;
                            fKosdaqIndexMin = 0;
                            nKosdaqIndexIdxPointer++;
                            if (nKosdaqIndexIdxPointer > BRUSH) // 추세 확인
                            {

                                fInclination = 0;
                                fRecentInclination = 0;
                                nInclinationCnt = 0;
                                nRecentInclinationCnt = 0;
                                fFluctuation = 0;

                                for (i = 0; i < nRandomi; i++)
                                {
                                    nFlowIdx = rand.Next(nKosdaqIndexIdxPointer);
                                    fInclination += (fCurKosdaqIndexUnGap - arrKosdaqIndex[nFlowIdx, 2]) / SubTimeToTimeAndSec(nSharedTime, (int)arrKosdaqIndex[nFlowIdx, 0]);
                                    nInclinationCnt++;

                                }
                                fResultInclinationEvg = fInclination / nInclinationCnt; // 평균추세선

                                for (i = 0; i < nRandomi; i++)
                                {
                                    if (nKosdaqIndexIdxPointer >= nRecentArea)
                                    {
                                        nFlowIdx = rand.Next(nKosdaqIndexIdxPointer - nRecentArea, nKosdaqIndexIdxPointer);
                                    }
                                    else
                                        nFlowIdx = rand.Next(nKosdaqIndexIdxPointer);
                                    nRecentInclinationCnt++;
                                    fRecentInclination += (fCurKosdaqIndexUnGap - arrKosdaqIndex[nFlowIdx, 2]) / SubTimeToTimeAndSec(nSharedTime, (int)arrKosdaqIndex[nFlowIdx, 0]);
                                    nFlowIdxDiff = rand.Next(nKosdaqIndexIdxPointer);

                                    fY = fResultInclinationEvg * (SubTimeToTimeAndSec(nSharedTime, (int)arrKosdaqIndex[nFlowIdx, 0]) / 60) + fCurKosdaqIndexUnGap - fResultInclinationEvg * (SubTimeToTimeAndSec(nSharedTime, nFirstTime) / 60); // 기울기에 따른 linear 함수
                                    fFluctuation += Math.Pow(fY - arrKosdaqIndex[nFlowIdx, 2], 2); // 변동폭
                                }
                                fResultRecentInclinationEvg = fRecentInclination / nRecentInclinationCnt; // 근접추세선

                                arrKosdaqIndex[nKosdaqIndexIdxPointer, 5] = fResultInclinationEvg;
                                arrKosdaqIndex[nKosdaqIndexIdxPointer, 6] = fResultRecentInclinationEvg;
                                arrKosdaqIndex[nKosdaqIndexIdxPointer, 7] = Math.Sqrt(fFluctuation);


                                double fN = fResultInclinationEvg;
                                double fM = fResultRecentInclinationEvg;
                                int nNDown = 0;
                                int nNUp = 0;

                                while (fN > 10)
                                {
                                    nNDown++;
                                    fN /= 10;
                                }
                                while (fN < 1)
                                {
                                    nNUp++;
                                    fN *= 10;
                                }
                                for (i = 0; i < nNDown; i++)
                                {
                                    fM /= 10;
                                }
                                for (i = 0; i < nNUp; i++)
                                {
                                    fM *= 10;
                                }

                                if (fN == fM) // same
                                {
                                    fKosdaqAngleDirection = 0;
                                }
                                else if (fN * fM == -1) // -1
                                {
                                    fKosdaqAngleDirection = 90;
                                }
                                else if (-1 < fN * fM) // -1 < x
                                {
                                    if (fN * fM < 0)  // -1 < x < 0
                                    {
                                        // 분자만 양수로 만들면 된다.
                                        if (fN < fM)
                                        {
                                            fKosdaqAngleDirection = 180 - Math.Atan((fM - fN) / (1 + fN * fM)) * (180 / Math.PI);
                                        }
                                        else // fResultRecentInclinationEvg < fResultInclinationEvg 
                                        {
                                            fKosdaqAngleDirection = 180 - Math.Atan((fN - fM) / (1 + fN * fM)) * (180 / Math.PI);
                                        }
                                    }
                                    else // 0 <= x 
                                    {
                                        // 분자만 양수로 만들면 된다.
                                        if (fN < fM)
                                        {
                                            fKosdaqAngleDirection = Math.Atan((fM - fN) / (1 + fN * fM)) * (180 / Math.PI);
                                        }
                                        else // fResultRecentInclinationEvg < fResultInclinationEvg 
                                        {
                                            fKosdaqAngleDirection = Math.Atan((fN - fM) / (1 + fN * fM)) * (180 / Math.PI);
                                        }
                                    }
                                }
                                else // x  < -1
                                {
                                    // 분자를 음수로 만들면 된다.
                                    if (fN < fM)
                                    {
                                        fKosdaqAngleDirection = Math.Atan((fN - fM) / (1 + fN * fM)) * (180 / Math.PI);
                                    }
                                    else
                                    {
                                        fKosdaqAngleDirection = Math.Atan((fM - fN) / (1 + fN * fM)) * (180 / Math.PI);
                                    }
                                }
                            }
                        }
                    }
                    /// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@


                    //Gap을 Gap에는 안더해주고 Gap아닌애는 더해줘야하는 거 아닌가 ??
                    //1000-> 1100(갭)-> 1200(현재)인 경우
                    //1000을 기준으로 하면 더 크지만 1100을 기준으로 하면 작아지는게 작아져야 갭의 영향을 덜 받는다고 생각할 수 있으니..이러면 초기가격설정에서 Gap변수를 지우고 Init을 갭없는 버전만 갭을 더해주면 된다(갭없는 거에 갭을 더해준다라는 말이 어려울 수 있는데 갭을 더해줌으로써 기준에 갭의 영향을 줄이는 효과를 주는것이다.)


                    int nR = 3;

                    eachStockArray[nCurIdx].lCurTotalPriceOfStock = eachStockArray[nCurIdx].lTotalNumOfStock * eachStockArray[nCurIdx].nFs;

                    if (eachStockArray[nCurIdx].nMarketGubun == KOSDAQ_ID)
                    {
                        fCurKosdaqIndexGap += (double)(eachStockArray[nCurIdx].lCurTotalPriceOfStock - eachStockArray[nCurIdx].lPrevTotalPriceOfStock) / INDEX_DEGREE;
                        fCurKosdaqIndexUnGap += (double)(eachStockArray[nCurIdx].lCurTotalPriceOfStock - eachStockArray[nCurIdx].lPrevTotalPriceOfStock) / INDEX_DEGREE;
                    }
                    else
                    {
                        fCurKospiIndexGap += (double)(eachStockArray[nCurIdx].lCurTotalPriceOfStock - eachStockArray[nCurIdx].lPrevTotalPriceOfStock) / INDEX_DEGREE;
                        fCurKospiIndexUnGap += (double)(eachStockArray[nCurIdx].lCurTotalPriceOfStock - eachStockArray[nCurIdx].lPrevTotalPriceOfStock) / INDEX_DEGREE;
                    }
                    eachStockArray[nCurIdx].lPrevTotalPriceOfStock = eachStockArray[nCurIdx].lCurTotalPriceOfStock;

                    fCurKospiGapInterestRatio = (fCurKospiIndexGap - fInitKospiIndexGap) / fInitKospiIndexGap;
                    fCurKospiUnGapInterestRatio = (fCurKospiIndexUnGap - fInitKospiIndexUnGap) / fInitKospiIndexUnGap;
                    fCurKosdaqGapInterestRatio = (fCurKosdaqIndexGap - fInitKosdaqIndexGap) / fInitKosdaqIndexGap;
                    fCurKosdaqUnGapInterestRatio = (fCurKosdaqIndexUnGap - fInitKosdaqIndexUnGap) / fInitKosdaqIndexUnGap;

                    if( nCurMarketSec != nSharedTime )
                    {
                        nCurMarketSec = nSharedTime;
                        swLogMarketSituSec.WriteLine(nSharedTime.ToString() + "\t" 
                            + Math.Round(fKosdaqGap,nR).ToString() + "\t" + Math.Round(arrKosdaqIndex[nKosdaqIndexIdxPointer, 5], nR).ToString() + "\t" + Math.Round(arrKosdaqIndex[nKosdaqIndexIdxPointer, 6], nR).ToString() + "\t" + Math.Round(fKosdaqAngleDirection,nR).ToString() + "\t" + Math.Round(arrKosdaqIndex[nKosdaqIndexIdxPointer, 7], nR).ToString() + "\t"
                            + Math.Round(fCurKosdaqIndexGap, nR).ToString() + "\t" + Math.Round(fInitKosdaqIndexGap, nR).ToString() + "\t" + Math.Round(fCurKosdaqGapInterestRatio * 100, nR).ToString() + "\t"
                            + Math.Round(fCurKosdaqIndexUnGap, nR).ToString() + "\t" + Math.Round(fInitKosdaqIndexUnGap, nR).ToString() + "\t" + Math.Round(fCurKosdaqUnGapInterestRatio * 100, nR).ToString() + "\t" 
                           
                            + Math.Round(fKospiGap, nR).ToString() + "\t" + Math.Round(arrKospiIndex[nKospiIndexIdxPointer, 5], nR).ToString() + "\t" + Math.Round(arrKospiIndex[nKospiIndexIdxPointer, 6], nR).ToString() + "\t" + Math.Round(fKospiAngleDirection, nR).ToString() + "\t" + Math.Round(arrKospiIndex[nKospiIndexIdxPointer, 7], nR).ToString() + "\t"
                            + Math.Round(fCurKospiIndexGap, nR).ToString() + "\t" + Math.Round(fInitKospiIndexGap, nR).ToString() + "\t" + Math.Round(fCurKospiGapInterestRatio * 100, nR).ToString() + "\t"
                            + Math.Round(fCurKospiIndexUnGap, nR).ToString() + "\t" + Math.Round(fInitKospiIndexUnGap, nR).ToString() + "\t" + Math.Round(fCurKospiUnGapInterestRatio * 100, nR).ToString()
                            );
                    }

                    if (nCurMarketMin != (int)(SubTimeToTimeAndSec(nSharedTime , nFirstTime) / 60))
                    {
                        nCurMarketMin = (int)(SubTimeToTimeAndSec(nSharedTime, nFirstTime) / 60);
                        swLogMarketSituMin.WriteLine(nSharedTime.ToString() + "\t"
                            + Math.Round(fKosdaqGap, nR).ToString() + "\t" + Math.Round(arrKosdaqIndex[nKosdaqIndexIdxPointer, 5], nR).ToString() + "\t" + Math.Round(arrKosdaqIndex[nKosdaqIndexIdxPointer, 6], nR).ToString() + "\t" + Math.Round(fKosdaqAngleDirection, nR).ToString() + "\t" + Math.Round(arrKosdaqIndex[nKosdaqIndexIdxPointer, 7], nR).ToString() + "\t"
                            + Math.Round(fCurKosdaqIndexGap, nR).ToString() + "\t" + Math.Round(fInitKosdaqIndexGap, nR).ToString() + "\t" + Math.Round(fCurKosdaqGapInterestRatio * 100, nR).ToString() + "\t"
                            + Math.Round(fCurKosdaqIndexUnGap, nR).ToString() + "\t" + Math.Round(fInitKosdaqIndexUnGap, nR).ToString() + "\t" + Math.Round(fCurKosdaqUnGapInterestRatio * 100, nR).ToString() + "\t"

                            + Math.Round(fKospiGap, nR).ToString() + "\t" + Math.Round(arrKospiIndex[nKospiIndexIdxPointer, 5], nR).ToString() + "\t" + Math.Round(arrKospiIndex[nKospiIndexIdxPointer, 6], nR).ToString() + "\t" + Math.Round(fKospiAngleDirection, nR).ToString() + "\t" + Math.Round(arrKospiIndex[nKospiIndexIdxPointer, 7], nR).ToString() + "\t"
                            + Math.Round(fCurKospiIndexGap, nR).ToString() + "\t" + Math.Round(fInitKospiIndexGap, nR).ToString() + "\t" + Math.Round(fCurKospiGapInterestRatio * 100, nR).ToString() + "\t"
                            + Math.Round(fCurKospiIndexUnGap, nR).ToString() + "\t" + Math.Round(fInitKospiIndexUnGap, nR).ToString() + "\t" + Math.Round(fCurKospiUnGapInterestRatio * 100, nR).ToString()
                            );
                    }


                    if (fKospiIndexFirst == 0)
                        fKospiIndexFirst = fCurKospiIndexUnGap;
                    fKospiIndexEnd = fCurKospiIndexUnGap;
                    fKospiIndexFollow = fCurKospiIndexUnGap;
                    if (fKospiIndexMax == 0 | fKospiIndexMax < fCurKospiIndexUnGap)
                        fKospiIndexMax = fCurKospiIndexUnGap;
                    if (fKospiIndexMin == 0 | fKospiIndexMin > fCurKospiIndexUnGap)
                        fKospiIndexMin = fCurKospiIndexUnGap;

                    if (fKosdaqIndexFirst == 0)
                        fKosdaqIndexFirst = fCurKosdaqIndexUnGap;
                    fKosdaqIndexEnd = fCurKosdaqIndexUnGap;
                    fKosdaqIndexFollow = fCurKosdaqIndexUnGap;
                    if (fKosdaqIndexMax == 0 | fKosdaqIndexMax < fCurKosdaqIndexUnGap)
                        fKosdaqIndexMax = fCurKosdaqIndexUnGap;
                    if (fKosdaqIndexMin == 0 | fKosdaqIndexMin > fCurKosdaqIndexUnGap)
                        fKosdaqIndexMin = fCurKosdaqIndexUnGap;


                    if (eachStockArray[nCurIdx].nCnt == 1)  // 첫데이터는  tv가 너무 높을 수 있느니 패스
                        return;






                 



                    ////////////////////////////////////////////////////////////////////
                    ////////////////////////////////////////////////////////////////////
                    ////////////////////////////////////////////////////////////////////
                    ///////////// 점수 Part /////////////////////////////////////////////

                    eachStockArray[nCurIdx].lCurTradeAmount += Math.Abs(eachStockArray[nCurIdx].nTv);
                    if (eachStockArray[nCurIdx].nTv > 0)
                        eachStockArray[nCurIdx].lCurTradeAmountOnlyUp += eachStockArray[nCurIdx].nTv;
                    else
                        eachStockArray[nCurIdx].lCurTradeAmountOnlyDown -= eachStockArray[nCurIdx].nTv;

                    eachStockArray[nCurIdx].fCurTradeAmountRatio = (double)eachStockArray[nCurIdx].lCurTradeAmount / eachStockArray[nCurIdx].lShareOutstanding;
                    eachStockArray[nCurIdx].fCurTradeAmountRatioOnlyUp = (double)eachStockArray[nCurIdx].lCurTradeAmountOnlyUp / eachStockArray[nCurIdx].lShareOutstanding;
                    eachStockArray[nCurIdx].fCurTradeAmountRatioOnlyDown = (double)eachStockArray[nCurIdx].lCurTradeAmountOnlyDown / eachStockArray[nCurIdx].lShareOutstanding;

                    // 일정시간마다 fJar 값을 감소시킨다. 이 일정시간을 어떻게 매길것인 지는 고민해볼 문제
                    // 시간 당 ... 
                    int nTimeUpdate = SubTimeToTimeAndSec(nSharedTime, eachStockArray[nCurIdx].nPrevUpdateTime) / nUpdateTime;
                    for (int idxUpdate = 0; idxUpdate < nTimeUpdate; idxUpdate++)
                    {
                        // 스피드 업데이트
                        if (eachStockArray[nCurIdx].fSpeedVal == 0)
                            eachStockArray[nCurIdx].fSpeedVal = eachStockArray[nCurIdx].nSpeedPush;
                        else
                            eachStockArray[nCurIdx].fSpeedVal = eachStockArray[nCurIdx].nSpeedPush * fPushWeight + eachStockArray[nCurIdx].fSpeedVal * (1 - fPushWeight);
                        eachStockArray[nCurIdx].nSpeedPush = 0;

                        // 체결량 업데이트
                        if (eachStockArray[nCurIdx].fTradeVal == 0)
                            eachStockArray[nCurIdx].fTradeVal = eachStockArray[nCurIdx].fTradePush;
                        else
                            eachStockArray[nCurIdx].fTradeVal = eachStockArray[nCurIdx].fTradePush * fPushWeight + eachStockArray[nCurIdx].fTradeVal * (1 - fPushWeight);
                        eachStockArray[nCurIdx].fTradePush = 0;

                        // 순체결량 업데이트
                        if (eachStockArray[nCurIdx].fPureTradeVal == 0)
                            eachStockArray[nCurIdx].fPureTradeVal = eachStockArray[nCurIdx].fPureTradePush;
                        else
                            eachStockArray[nCurIdx].fPureTradeVal = eachStockArray[nCurIdx].fPureTradePush * fPushWeight + eachStockArray[nCurIdx].fPureTradeVal * (1 - fPushWeight);
                        eachStockArray[nCurIdx].fPureTradePush = 0;

                        eachStockArray[nCurIdx].nPrevUpdateTime = AddTimeBySec(eachStockArray[nCurIdx].nPrevUpdateTime, nUpdateTime);
                        if (idxUpdate == 0)
                            eachStockArray[nCurIdx].nScoreCheckTrigger++;
                    }
                    // 가격은 매초당 조금씩 줄여나가게 한다
                    // 1분이 지났을 시 0.547퍼센트
                    int nTimeGap = SubTimeToTimeAndSec(nSharedTime, eachStockArray[nCurIdx].nPrevPriceUpdateTime);
                    for (int idxTimeGap = 0; idxTimeGap < nTimeGap; idxTimeGap++)
                    {
                        // 가격변화 업데이트
                        eachStockArray[nCurIdx].fPowerLongJar *= 0.999;
                        eachStockArray[nCurIdx].fPowerJar *= 0.995;
                        eachStockArray[nCurIdx].fPowerOnlyUp *= 0.995;
                        eachStockArray[nCurIdx].fPowerOnlyDown *= 0.995;

                        eachStockArray[nCurIdx].nPrevPriceUpdateTime = nSharedTime;
                    }

                    // 파워는 최우선매수호가와 초기가격의 손익률로 계산한다
                    eachStockArray[nCurIdx].fPowerWithoutGap = eachStockArray[nCurIdx].fPower - eachStockArray[nCurIdx].fStartGap;


                    int nTimePassed = SubTimeToTimeAndSec(nSharedTime, eachStockArray[nCurIdx].nPrevUpdateTime); // 시간이 지났다!
                    double fTimePassedWeight = (double)nTimePassed / nUpdateTime; // 시간이 얼만큼 지났느냐 0 ~ ( nUpdateTime -1) /nUpdateTime
                    double fTimePassedPushWeight = fTimePassedWeight * fPushWeight;

                 

                    //  속도 실시간 처리
                    eachStockArray[nCurIdx].nSpeedPush++;
                    if (eachStockArray[nCurIdx].fSpeedVal == 0)
                        eachStockArray[nCurIdx].fCurSpeed = (double)eachStockArray[nCurIdx].nSpeedPush / nUpdateTime;
                    else
                        eachStockArray[nCurIdx].fCurSpeed = (eachStockArray[nCurIdx].nSpeedPush * fTimePassedPushWeight + eachStockArray[nCurIdx].fSpeedVal * (1 - fTimePassedPushWeight)) / nUpdateTime;


                    // 체결량 실시간 처리
                    eachStockArray[nCurIdx].fTradePush += Math.Abs(eachStockArray[nCurIdx].nTv);

                    if (eachStockArray[nCurIdx].fTradeVal == 0)
                        eachStockArray[nCurIdx].fCurTrade = eachStockArray[nCurIdx].fTradePush;
                    else
                        eachStockArray[nCurIdx].fCurTrade = eachStockArray[nCurIdx].fTradePush * fTimePassedPushWeight + eachStockArray[nCurIdx].fTradeVal * (1 - fTimePassedPushWeight);

                    // 순체결량 실시간 처리
                    eachStockArray[nCurIdx].fPureTradePush += eachStockArray[nCurIdx].nTv;
                    if (eachStockArray[nCurIdx].fPureTradeVal == 0)
                        eachStockArray[nCurIdx].fCurPureTrade = eachStockArray[nCurIdx].fPureTradePush;
                    else
                        eachStockArray[nCurIdx].fCurPureTrade = eachStockArray[nCurIdx].fPureTradePush * fTimePassedPushWeight + eachStockArray[nCurIdx].fPureTradeVal * (1 - fTimePassedPushWeight);



                    // 가격변화 실시간 처리
                    eachStockArray[nCurIdx].fPowerJar += (eachStockArray[nCurIdx].fPowerWithoutGap - eachStockArray[nCurIdx].fPrevPowerWithoutGap) * 100;
                    eachStockArray[nCurIdx].fPowerLongJar += (eachStockArray[nCurIdx].fPowerWithoutGap - eachStockArray[nCurIdx].fPrevPowerWithoutGap) * 100;
                    eachStockArray[nCurIdx].fPowerOnlyUp += (eachStockArray[nCurIdx].fPowerWithoutGap - eachStockArray[nCurIdx].fPrevPowerWithoutGap) * 100;
                    eachStockArray[nCurIdx].fPowerOnlyDown += (eachStockArray[nCurIdx].fPowerWithoutGap - eachStockArray[nCurIdx].fPrevPowerWithoutGap) * 100;
                    if(eachStockArray[nCurIdx].fPowerOnlyUp < 0)
                    {
                        eachStockArray[nCurIdx].fPowerOnlyUp = 0;
                    }
                    if(eachStockArray[nCurIdx].fPowerOnlyDown > 0)
                    {
                        eachStockArray[nCurIdx].fPowerOnlyDown = 0;
                    }


                    eachStockArray[nCurIdx].fPrevPowerWithoutGap = eachStockArray[nCurIdx].fPowerWithoutGap;

                    // 실시간 처리
                    eachStockArray[nCurIdx].fSharePerTrade = eachStockArray[nCurIdx].lShareOutstanding / eachStockArray[nCurIdx].fCurTrade; // 0에 가까울 수  록 좋음

                    if (eachStockArray[nCurIdx].fTotalHogaVolumeVal == 0)
                    {
                        eachStockArray[nCurIdx].fSharePerHoga = BILLION;
                        eachStockArray[nCurIdx].fHogaPerTrade = BILLION;
                    }
                    else
                    {
                        eachStockArray[nCurIdx].fSharePerHoga = eachStockArray[nCurIdx].lShareOutstanding / eachStockArray[nCurIdx].fTotalHogaVolumeVal; // 0에 가까울 수 록 좋음
                        eachStockArray[nCurIdx].fHogaPerTrade = eachStockArray[nCurIdx].fTotalHogaVolumeVal / eachStockArray[nCurIdx].fCurTrade; // 0에 가까울 수 록 좋음
                    }

                    /// 점수 공식 
                    if ((eachStockArray[nCurIdx].fCurTrade * eachStockArray[nCurIdx].nFs) < MILLION) // 현체결량이 100만원 이하면
                        eachStockArray[nCurIdx].fPurePerTrade = eachStockArray[nCurIdx].fCurPureTrade / (MILLION / (double)eachStockArray[nCurIdx].nFs);
                    else
                    {
                        if (Math.Abs(eachStockArray[nCurIdx].fCurPureTrade) > eachStockArray[nCurIdx].fCurTrade)
                        {
                            eachStockArray[nCurIdx].fPurePerTrade = eachStockArray[nCurIdx].fCurPureTrade / (eachStockArray[nCurIdx].fCurTrade + Math.Abs(eachStockArray[nCurIdx].fCurPureTrade));
                        }
                        else
                            eachStockArray[nCurIdx].fPurePerTrade = eachStockArray[nCurIdx].fCurPureTrade / eachStockArray[nCurIdx].fCurTrade; // 절대값 1에 가까울 수 록 좋음 -면 매도, +면 매수
                    }

                    /// direction
                    // eachStockArray[nCurIdx].fScoreDirection = eachStockArray[nCurIdx].f2000Ratio * 0.15 + eachStockArray[nCurIdx].fPurePerTrade * 0.5 + eachStockArray[nCurIdx].fHogaRatioVal * 0.35;
                    eachStockArray[nCurIdx].fScoreDirection = eachStockArray[nCurIdx].fPurePerTrade * 0.65 + eachStockArray[nCurIdx].fHogaRatioVal * 0.35;

                   
                    /// volume 
                    ///

                    double fLog20 = eachStockArray[nCurIdx].fSharePerHoga / 100;
                    double fLog2 = eachStockArray[nCurIdx].fHogaPerTrade;

                    eachStockArray[nCurIdx].fScoreVolume = 0;

                    if (fLog20 < 1 && fLog2 < 1)
                        eachStockArray[nCurIdx].fScoreVolume = 100;
                    else
                    {
                        if ( fLog20 < 1)
                        {
                            eachStockArray[nCurIdx].fScoreVolume = 40 + 60 / fLog2;
                        }
                        else if (fLog2 < 1)
                        {
                            eachStockArray[nCurIdx].fScoreVolume = 40 / fLog20 + 60;
                        }
                        else
                        {
                            eachStockArray[nCurIdx].fScoreVolume = 40 / fLog20 + 60 / fLog2;
                        }
                    }



                    /// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                    /// 
                    int nMinPointer = (int)(GetSec(nSharedTime - nFirstTime) / 60) + BRUSH; // 현재시간과 9시를 뺀 결과를 분단위로 받음
                    if (eachStockArray[nCurIdx].nIdxPointer != nMinPointer) // 기록된 포인터와 새로운 포인터가 다르면 (과거처리)
                    {
                        int nDiff = nMinPointer - eachStockArray[nCurIdx].nIdxPointer;
                        int i = 0;
                        eachStockArray[nCurIdx].isGooiTime = false;
                        eachStockArray[nCurIdx].isGooiTimeEverage = false;
                        eachStockArray[nCurIdx].isGooiTimeEnd = false;
                        eachStockArray[nCurIdx].isGooiTimeEndEverage = false;

                        for ( i = 0; i < nDiff; i++)
                        {
                            if (eachStockArray[nCurIdx].nIdxPointer < BRUSH)
                            {
                                eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 0] = nFirstTime; 
                            }
                            else
                            {
                                eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 0] = AddTimeBySec(nFirstTime, (eachStockArray[nCurIdx].nIdxPointer - BRUSH) * 60);
                            }

                            if (eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 2] == 0) // 기록이 아예없는 경우 ( 초기 or vi or 거래없는경우)
                            {
                                if (eachStockArray[nCurIdx].nFsPointer == 0) // 초기
                                {
                                    eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 1] = eachStockArray[nCurIdx].nTodayStartPrice;
                                    eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 2] = eachStockArray[nCurIdx].nTodayStartPrice;
                                    eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 3] = eachStockArray[nCurIdx].nTodayStartPrice;
                                    eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 4] = eachStockArray[nCurIdx].nTodayStartPrice;
                                }
                                else // vi or 거래없는경우
                                {
                                    eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 1] = eachStockArray[nCurIdx].nFsPointer;
                                    eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 2] = eachStockArray[nCurIdx].nFsPointer;
                                    eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 3] = eachStockArray[nCurIdx].nFsPointer;
                                    eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 4] = eachStockArray[nCurIdx].nFsPointer;
                                }
                            }
                            else // 기록이 존재하는경우  그냥 인덱스만 올리면 된다.  ( 추후 알맹이 추가)
                            {
                                int maxPart;
                                int minPart;

                                if (eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 1] < eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 2]) // 시가 < 종가
                                {
                                    maxPart = (int)eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 2];
                                    minPart = (int)eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 1];
                                }
                                else
                                {
                                    maxPart = (int)eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 1];
                                    minPart = (int)eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 2];
                                }
                                int EverageFs = (maxPart + minPart) / 2;

                                int maxTop = (int)eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 3]; // 고가 
                                int minBottom = (int)eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 4]; // 저가
                                int EverageMiddle = (maxTop + minBottom) / 2;


                                // 시종가
                                if (eachStockArray[nCurIdx].nMaxFs < maxPart)
                                {
                                    eachStockArray[nCurIdx].nMaxFs = maxPart;
                                    eachStockArray[nCurIdx].nMaxTime = (int)eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 0];
                                    eachStockArray[nCurIdx].nMinFs = maxPart;
                                    eachStockArray[nCurIdx].nMinTime = (int)eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 0];
                                }

                                if (eachStockArray[nCurIdx].nMinFs > minPart)
                                {
                                    eachStockArray[nCurIdx].nMinFs = minPart;
                                    eachStockArray[nCurIdx].nMinTime = (int)eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 0];
                                }

                                // 시종가평균
                                if (eachStockArray[nCurIdx].nMaxEverageFs < EverageFs)
                                {
                                    eachStockArray[nCurIdx].nMaxEverageFs = EverageFs;
                                    eachStockArray[nCurIdx].nMaxEverageTime = (int)eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 0];
                                    eachStockArray[nCurIdx].nMinEverageFs = EverageFs;
                                    eachStockArray[nCurIdx].nMinEverageTime = (int)eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 0];
                                }
                                if (eachStockArray[nCurIdx].nMinEverageFs > EverageFs)
                                {
                                    eachStockArray[nCurIdx].nMinEverageFs = EverageFs;
                                    eachStockArray[nCurIdx].nMinEverageTime = (int)eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 0];
                                }

                                // 저고가
                                if (eachStockArray[nCurIdx].nMaxTopFs < maxTop)
                                {
                                    eachStockArray[nCurIdx].nMaxTopFs = maxTop;
                                    eachStockArray[nCurIdx].nMaxTopTime = (int)eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 0];
                                    eachStockArray[nCurIdx].nMinBottomFs = maxTop;
                                    eachStockArray[nCurIdx].nMinBottomTime = (int)eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 0];
                                }
                                if (eachStockArray[nCurIdx].nMinBottomFs > minBottom)
                                {
                                    eachStockArray[nCurIdx].nMinBottomFs = minBottom;
                                    eachStockArray[nCurIdx].nMinBottomTime = (int)eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 0];
                                }

                                // 저고가평균
                                if (eachStockArray[nCurIdx].nMaxTopEverageFs < EverageMiddle)
                                {
                                    eachStockArray[nCurIdx].nMaxTopEverageFs = EverageMiddle;
                                    eachStockArray[nCurIdx].nMaxTopEverageTime = (int)eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 0];
                                    eachStockArray[nCurIdx].nMinBottomEverageFs = EverageMiddle;
                                    eachStockArray[nCurIdx].nMinBottomEverageTime = (int)eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 0];
                                }
                                if (eachStockArray[nCurIdx].nMinBottomEverageFs > EverageMiddle)
                                {
                                    eachStockArray[nCurIdx].nMinBottomEverageFs = EverageMiddle;
                                    eachStockArray[nCurIdx].nMinBottomEverageTime = (int)eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 0];
                                }
                            }

                            eachStockArray[nCurIdx].nIdxPointer++;
                            if (eachStockArray[nCurIdx].nIdxPointer > BRUSH) // 추세 확인
                            {

                                fInclination = 0;
                                fRecentInclination = 0;
                                nInclinationCnt = 0;
                                nRecentInclinationCnt = 0;
                                fFluctuation = 0;

                                for (i = 0; i < nRandomi; i++)
                                {
                                    nFlowIdx = rand.Next(eachStockArray[nCurIdx].nIdxPointer);
                                    fInclination += (eachStockArray[nCurIdx].nFs - eachStockArray[nCurIdx].arrRecord[nFlowIdx, 2]) / SubTimeToTimeAndSec(nSharedTime, (int)eachStockArray[nCurIdx].arrRecord[nFlowIdx, 0]);
                                    nInclinationCnt++;

                                }
                                fResultInclinationEvg = fInclination / nInclinationCnt; // 평균추세선

                                for (i = 0; i < nRandomi; i++)
                                {
                                    if (eachStockArray[nCurIdx].nIdxPointer >= nRecentArea)
                                    {
                                        nFlowIdx = rand.Next(eachStockArray[nCurIdx].nIdxPointer - nRecentArea, eachStockArray[nCurIdx].nIdxPointer);
                                    }
                                    else
                                        nFlowIdx = rand.Next(eachStockArray[nCurIdx].nIdxPointer);
                                    nRecentInclinationCnt++;
                                    fRecentInclination += (eachStockArray[nCurIdx].nFs - eachStockArray[nCurIdx].arrRecord[nFlowIdx, 2]) / SubTimeToTimeAndSec(nSharedTime, (int)eachStockArray[nCurIdx].arrRecord[nFlowIdx, 0]);

                                    nFlowIdxDiff = rand.Next(eachStockArray[nCurIdx].nIdxPointer);

                                    fY = fResultInclinationEvg * (SubTimeToTimeAndSec(nSharedTime, (int)eachStockArray[nCurIdx].arrRecord[nFlowIdxDiff, 0]) / 60) + eachStockArray[nCurIdx].nFs - fResultInclinationEvg * (SubTimeToTimeAndSec(nSharedTime, nFirstTime) / 60); // 기울기에 따른 linear 함수
                                    fFluctuation += Math.Pow(fY - eachStockArray[nCurIdx].arrRecord[nFlowIdxDiff, 2], 2); // 변동폭
                                }
                                fResultRecentInclinationEvg = fRecentInclination / nRecentInclinationCnt; // 근접추세선

                                if (eachStockArray[nCurIdx].nMarketGubun == KOSPI_ID)
                                {
                                    eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 5] = fResultInclinationEvg / GetKospiGap(eachStockArray[nCurIdx].nFs);
                                    eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 6] = fResultRecentInclinationEvg / GetKospiGap(eachStockArray[nCurIdx].nFs);
                                    eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 7] = Math.Sqrt(fFluctuation) / GetKospiGap(eachStockArray[nCurIdx].nFs); 
                                }
                                else
                                {
                                    eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 5] = fResultInclinationEvg / GetKosdaqGap(eachStockArray[nCurIdx].nFs);
                                    eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 6] = fResultRecentInclinationEvg / GetKosdaqGap(eachStockArray[nCurIdx].nFs);
                                    eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 7] = Math.Sqrt(fFluctuation) / GetKosdaqGap(eachStockArray[nCurIdx].nFs);
                                }

                                double fN = fResultInclinationEvg;
                                double fM = fResultRecentInclinationEvg;
                                int nNDown = 0;
                                int nNUp = 0;

                                while( fN > 10)
                                {
                                    nNDown++;
                                    fN /= 10;
                                }
                                while( fN < 1)
                                {
                                    nNUp++;
                                    fN *= 10;
                                }
                                for (i = 0; i < nNDown; i++)
                                {
                                    fM /= 10;
                                }
                                for (i = 0; i < nNUp; i++)
                                {
                                    fM *= 10;
                                }

                                if (fN == fM) // same
                                {
                                    eachStockArray[nCurIdx].fAngleDirection = 0;
                                }
                                else if(fN * fM == -1) // -1
                                {
                                    eachStockArray[nCurIdx].fAngleDirection =  90;
                                }
                                else if( -1 < fN * fM) // -1 < x
                                {
                                    if (fN * fM < 0)  // -1 < x < 0
                                    {
                                        // 분자만 양수로 만들면 된다.
                                        if (fN < fM) 
                                        {
                                            eachStockArray[nCurIdx].fAngleDirection = 180 - Math.Atan((fM - fN) / (1 + fN * fM)) * (180 / Math.PI);
                                        }
                                        else // fResultRecentInclinationEvg < fResultInclinationEvg 
                                        {
                                            eachStockArray[nCurIdx].fAngleDirection = 180 - Math.Atan((fN - fM) / (1 + fN * fM)) * (180 / Math.PI);
                                        }
                                    }
                                    else // 0 <= x 
                                    {
                                        // 분자만 양수로 만들면 된다.
                                        if (fN < fM)
                                        {
                                            eachStockArray[nCurIdx].fAngleDirection =  Math.Atan((fM - fN) / (1 + fN * fM)) * (180 / Math.PI);
                                        }
                                        else // fResultRecentInclinationEvg < fResultInclinationEvg 
                                        {
                                            eachStockArray[nCurIdx].fAngleDirection = Math.Atan((fN - fM) / (1 + fN * fM)) * (180 / Math.PI);
                                        }
                                    }
                                }
                                else // x  < -1
                                {
                                    // 분자를 음수로 만들면 된다.
                                    if (fN < fM)
                                    {
                                        eachStockArray[nCurIdx].fAngleDirection = Math.Atan((fN - fM) / (1 + fN * fM)) * (180 / Math.PI);
                                    }
                                    else
                                    {
                                        eachStockArray[nCurIdx].fAngleDirection = Math.Atan((fM - fN) / (1 + fN * fM)) * (180 / Math.PI);
                                    }
                                }
                                
                            }
                        }

                        eachStockArray[nCurIdx].nFirstPointer = 0;
                        eachStockArray[nCurIdx].nLastPointer = 0;
                        eachStockArray[nCurIdx].nMaxPointer = 0;
                        eachStockArray[nCurIdx].nMinPointer = 0;

                        
                    }


                    /// 전고점 테스트
                    /// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                    if ( !eachStockArray[nCurIdx].isGooiTime && 
                        eachStockArray[nCurIdx].nMaxFs < eachStockArray[nCurIdx].nFs &&
                        eachStockArray[nCurIdx].nMaxFs - eachStockArray[nCurIdx].nMinFs > eachStockArray[nCurIdx].nYesterdayEndPrice * 0.01 &&
                        eachStockArray[nCurIdx].nMinTime > eachStockArray[nCurIdx].nMaxTime
                        )
                    {
                        eachStockArray[nCurIdx].isGooiTime = true;
                        swLogFirstAndLast.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + SubTimeToTime(eachStockArray[nCurIdx].nMinTime, eachStockArray[nCurIdx].nMaxTime).ToString() + "\t" + SubTimeToTime(nSharedTime, eachStockArray[nCurIdx].nMaxTime).ToString() + "\t" + SubTimeToTime(nSharedTime, eachStockArray[nCurIdx].nMinTime).ToString() + "\t" + (eachStockArray[nCurIdx].nMaxFs - eachStockArray[nCurIdx].nMinFs).ToString() + "\t" + Math.Round((double)(eachStockArray[nCurIdx].nMaxFs - eachStockArray[nCurIdx].nMinFs) / eachStockArray[nCurIdx].nYesterdayEndPrice, nR).ToString() + "\t" +
                        Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                    }

                    if ( !eachStockArray[nCurIdx].isGooiTimeEverage && 
                        eachStockArray[nCurIdx].nMaxEverageFs < eachStockArray[nCurIdx].nFs &&
                        eachStockArray[nCurIdx].nMaxEverageFs - eachStockArray[nCurIdx].nMinEverageFs > eachStockArray[nCurIdx].nYesterdayEndPrice * 0.01 &&
                        eachStockArray[nCurIdx].nMinEverageTime > eachStockArray[nCurIdx].nMaxEverageTime
                        )
                    {
                        eachStockArray[nCurIdx].isGooiTimeEverage = true;
                        swLogFirstAndLastEverage.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + SubTimeToTime(eachStockArray[nCurIdx].nMinEverageTime, eachStockArray[nCurIdx].nMaxEverageTime).ToString() + "\t" + SubTimeToTime(nSharedTime, eachStockArray[nCurIdx].nMaxEverageTime).ToString() + "\t" + SubTimeToTime(nSharedTime, eachStockArray[nCurIdx].nMinEverageTime).ToString() + "\t" + (eachStockArray[nCurIdx].nMaxEverageFs - eachStockArray[nCurIdx].nMinEverageFs).ToString() + "\t" + Math.Round((double)(eachStockArray[nCurIdx].nMaxEverageFs - eachStockArray[nCurIdx].nMinEverageFs) / eachStockArray[nCurIdx].nYesterdayEndPrice, nR).ToString() + "\t" +
                        Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());

                    }
                    if ( !eachStockArray[nCurIdx].isGooiTimeEnd &&
                        eachStockArray[nCurIdx].nMaxTopFs < eachStockArray[nCurIdx].nFs &&
                        eachStockArray[nCurIdx].nMaxTopFs - eachStockArray[nCurIdx].nMinBottomFs > eachStockArray[nCurIdx].nYesterdayEndPrice * 0.01 &&
                        eachStockArray[nCurIdx].nMinBottomTime > eachStockArray[nCurIdx].nMaxTopTime
                        )
                    {
                        eachStockArray[nCurIdx].isGooiTimeEnd = true;
                        swLogTopAndBottom.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + SubTimeToTime(eachStockArray[nCurIdx].nMinBottomTime, eachStockArray[nCurIdx].nMaxTopTime).ToString() + "\t" + SubTimeToTime(nSharedTime, eachStockArray[nCurIdx].nMaxTopTime).ToString() + "\t" + SubTimeToTime(nSharedTime, eachStockArray[nCurIdx].nMinBottomTime).ToString() + "\t" + (eachStockArray[nCurIdx].nMaxTopFs - eachStockArray[nCurIdx].nMinBottomFs).ToString() + "\t" + Math.Round((double)(eachStockArray[nCurIdx].nMaxTopFs - eachStockArray[nCurIdx].nMinBottomFs) / eachStockArray[nCurIdx].nYesterdayEndPrice, nR).ToString() + "\t" +
                        Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());

                    }

                    if ( !eachStockArray[nCurIdx].isGooiTimeEndEverage &&
                        eachStockArray[nCurIdx].nMaxTopEverageFs < eachStockArray[nCurIdx].nFs &&
                        eachStockArray[nCurIdx].nMaxTopEverageFs - eachStockArray[nCurIdx].nMinBottomEverageFs > eachStockArray[nCurIdx].nYesterdayEndPrice * 0.01 &&
                        eachStockArray[nCurIdx].nMinBottomEverageTime > eachStockArray[nCurIdx].nMaxTopEverageTime
                        )
                    {
                        eachStockArray[nCurIdx].isGooiTimeEndEverage = true;
                        swLogTopAndBottomEverage.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + SubTimeToTime(eachStockArray[nCurIdx].nMinBottomEverageTime, eachStockArray[nCurIdx].nMaxTopEverageTime).ToString() + "\t" + SubTimeToTime(nSharedTime, eachStockArray[nCurIdx].nMaxTopEverageTime).ToString() + "\t" + SubTimeToTime(nSharedTime, eachStockArray[nCurIdx].nMinBottomEverageTime).ToString() + "\t" + (eachStockArray[nCurIdx].nMaxTopEverageFs - eachStockArray[nCurIdx].nMinBottomEverageFs).ToString() + "\t" + Math.Round((double)(eachStockArray[nCurIdx].nMaxTopEverageFs - eachStockArray[nCurIdx].nMinBottomEverageFs) / eachStockArray[nCurIdx].nYesterdayEndPrice, nR).ToString() + "\t" +
                        Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());

                    }


                    
                    //////////////////////////////////////////////////////////
                    /// 현재기록
                    if (eachStockArray[nCurIdx].nFirstPointer == 0)
                    {
                        eachStockArray[nCurIdx].nFirstPointer = eachStockArray[nCurIdx].nFs;
                        eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 1] = eachStockArray[nCurIdx].nFirstPointer;
                    }

                    eachStockArray[nCurIdx].nLastPointer = eachStockArray[nCurIdx].nFs;
                    eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 2] = eachStockArray[nCurIdx].nLastPointer;

                    if (eachStockArray[nCurIdx].nMaxPointer == 0 || eachStockArray[nCurIdx].nMaxPointer < eachStockArray[nCurIdx].nFs)
                    {
                        eachStockArray[nCurIdx].nMaxPointer = eachStockArray[nCurIdx].nFs;
                        eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 3] = eachStockArray[nCurIdx].nMaxPointer;
                    }

                    if (eachStockArray[nCurIdx].nMinPointer == 0 || eachStockArray[nCurIdx].nMinPointer > eachStockArray[nCurIdx].nFs)
                    {
                        eachStockArray[nCurIdx].nMinPointer = eachStockArray[nCurIdx].nFs;
                        eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 4] = eachStockArray[nCurIdx].nMinPointer;
                    }

                    eachStockArray[nCurIdx].nFsPointer = eachStockArray[nCurIdx].nFs;
                    /// END  -- 현재기록 
                    /////////////////////////////////////////////////////////////



                    ////// 점수 Part End //////////////////////////////
                    ////////////////////////////////////////////////////
                    //////////////////////////////////////////////////
                    /////////////////////////////////////////////////////
                    int nTimeDiff = SubTimeToTimeAndSec(nSharedTime, nFirstTime);
                    eachStockArray[nCurIdx].fCntPerTime = (double)eachStockArray[nCurIdx].nCnt / (nTimeDiff + 10);

                    if (eachStockArray[nCurIdx].fMaxPowerWithoutGap == 0 || eachStockArray[nCurIdx].fMaxPowerWithoutGap < eachStockArray[nCurIdx].fPowerWithoutGap)
                    {
                        eachStockArray[nCurIdx].fMaxPowerWithoutGap = eachStockArray[nCurIdx].fPowerWithoutGap;
                    }

                    if (eachStockArray[nCurIdx].fMinPowerWithoutGap == 0 || eachStockArray[nCurIdx].fMinPowerWithoutGap > eachStockArray[nCurIdx].fPowerWithoutGap)
                    {
                        eachStockArray[nCurIdx].fMinPowerWithoutGap = eachStockArray[nCurIdx].fPowerWithoutGap;
                    }








                    if (!eachStockArray[nCurIdx].isOneToTenSucceed && eachStockArray[nCurIdx].fCurTradeAmountRatioOnlyUp >= 0.1)
                    {
                        eachStockArray[nCurIdx].isOneToTenSucceed = true;
                        swLogOneToTenSucceed.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                    }

                    if (!eachStockArray[nCurIdx].isQuarterSucceed && eachStockArray[nCurIdx].fCurTradeAmountRatioOnlyUp >= 0.25)
                    {
                        eachStockArray[nCurIdx].isQuarterSucceed = true;
                        swLogQuarterSucceed.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                    }

                    if (!eachStockArray[nCurIdx].isHalfSucceed && eachStockArray[nCurIdx].fCurTradeAmountRatioOnlyUp >= 0.5)
                    {
                        eachStockArray[nCurIdx].isHalfSucceed = true;
                        swLogHalfSucceed.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                    }

                    if (!eachStockArray[nCurIdx].isFullSucceed && eachStockArray[nCurIdx].fCurTradeAmountRatioOnlyUp >= 1)
                    {
                        eachStockArray[nCurIdx].isFullSucceed = true;
                        swLogFullSucceed.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                    }

                    if (!eachStockArray[nCurIdx].isHalfFullSucceed && eachStockArray[nCurIdx].fCurTradeAmountRatioOnlyUp >= 1.5)
                    {
                        eachStockArray[nCurIdx].isHalfFullSucceed = true;
                        swLogHalfFullSucceed.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                    }

                    if (!eachStockArray[nCurIdx].isDoubleSucceed && eachStockArray[nCurIdx].fCurTradeAmountRatioOnlyUp >= 2)
                    {
                        eachStockArray[nCurIdx].isDoubleSucceed = true;
                        swLogDoubleSucceed.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                    }
                    if (eachStockArray[nCurIdx].fPowerLongJar >= 5)
                    {
                        swLogLongPower5.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                    }
                    if (eachStockArray[nCurIdx].fPowerJar >= 1)
                    {
                        swLogPower1.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                    }
                    if (eachStockArray[nCurIdx].fPowerJar >= 5)
                    {
                        swLogPower5.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                    }
                    if (eachStockArray[nCurIdx].fPowerJar <= -2)
                    {
                        swLogPowerM2.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                    }
                    if (eachStockArray[nCurIdx].fPowerJar <= -5)
                    {
                        swLogPowerM5.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                    }

                    if (eachStockArray[nCurIdx].fPowerJar >= 2)
                    {
                        swLogPower2.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                        eachStockArray[nCurIdx].nUpgradeTime = nSharedTime;
                    }


                    // 속도 관련
                    if (eachStockArray[nCurIdx].fCurSpeed >= 10)
                    {
                        eachStockArray[nCurIdx].nSpeed10Time = nSharedTime;
                        swLogSpeed10.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                    }
                    if (eachStockArray[nCurIdx].fCurSpeed >= 20)
                    {
                        eachStockArray[nCurIdx].nSpeed20Time = nSharedTime;
                        swLogSpeed20.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                    }
                    if (eachStockArray[nCurIdx].fCurSpeed >= 30)
                    {
                        eachStockArray[nCurIdx].nSpeed30Time = nSharedTime;
                        swLogSpeed30.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                    }
                    if (eachStockArray[nCurIdx].fCurSpeed >= 40)
                    {
                        eachStockArray[nCurIdx].nSpeed40Time = nSharedTime;
                        swLogSpeed40.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                    }

                    // 호가비율
                    if (eachStockArray[nCurIdx].fHogaRatioVal >= 0.6)
                    {
                        eachStockArray[nCurIdx].nHogaRatio6Time = nSharedTime;
                        swLogHogaRatio60.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                    }
                    if (eachStockArray[nCurIdx].fHogaRatioVal >= 0.7)
                    {
                        eachStockArray[nCurIdx].nHogaRatio7Time = nSharedTime;
                        swLogHogaRatio70.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                    }
                    if (eachStockArray[nCurIdx].fHogaRatioVal >= 0.8)
                    {
                        eachStockArray[nCurIdx].nHogaRatio8Time = nSharedTime;
                        swLogHogaRatio80.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                    }
                    if (eachStockArray[nCurIdx].fHogaRatioVal >= 0.9)
                    {
                        eachStockArray[nCurIdx].nHogaRatio9Time = nSharedTime;
                        swLogHogaRatio90.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                    }

                    // 호가량
                    if (eachStockArray[nCurIdx].fSharePerHoga <= 100)
                    {
                        eachStockArray[nCurIdx].nHogaVolume100Time = nSharedTime;
                        swLogHogaVolume100.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                    }
                    if (eachStockArray[nCurIdx].fSharePerHoga <= 80)
                    {
                        eachStockArray[nCurIdx].nHogaVolume80Time = nSharedTime;
                        swLogHogaVolume80.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                    }
                    if (eachStockArray[nCurIdx].fSharePerHoga <= 60)
                    {
                        eachStockArray[nCurIdx].nHogaVolume60Time = nSharedTime;
                        swLogHogaVolume60.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                    }
                    if (eachStockArray[nCurIdx].fSharePerHoga <= 40)
                    {
                        eachStockArray[nCurIdx].nHogaVolume40Time = nSharedTime;
                        swLogHogaVolume40.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                    }

                    // 체결량
                    if ((eachStockArray[nCurIdx].fSharePerHoga * eachStockArray[nCurIdx].fHogaPerTrade) <= 200)
                    {
                        eachStockArray[nCurIdx].nTrade200Time = nSharedTime;
                        swLogTradeVolume200.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                    }
                    if ((eachStockArray[nCurIdx].fSharePerHoga * eachStockArray[nCurIdx].fHogaPerTrade) <= 150)
                    {
                        eachStockArray[nCurIdx].nTrade150Time = nSharedTime;
                        swLogTradeVolume150.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                    }
                    if ((eachStockArray[nCurIdx].fSharePerHoga * eachStockArray[nCurIdx].fHogaPerTrade) <= 100)
                    {
                        eachStockArray[nCurIdx].nTrade100Time = nSharedTime;
                        swLogTradeVolume100.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                    }
                    if ((eachStockArray[nCurIdx].fSharePerHoga * eachStockArray[nCurIdx].fHogaPerTrade) <= 70)
                    {
                        eachStockArray[nCurIdx].nTrade70Time = nSharedTime;
                        swLogTradeVolume70.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                    }

                    // 가격변동
                    if (eachStockArray[nCurIdx].fPowerLongJar >= 1.5)
                    {
                        eachStockArray[nCurIdx].nPower15Time = nSharedTime;
                        swLogPowerLong15.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                    }
                    if (eachStockArray[nCurIdx].fPowerLongJar >= 3)
                    {
                        eachStockArray[nCurIdx].nPower30Time = nSharedTime;
                        swLogPowerLong30.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                    }
                    if (eachStockArray[nCurIdx].fPowerLongJar >= 4.5)
                    {
                        eachStockArray[nCurIdx].nPower45Time = nSharedTime;
                        swLogPowerLong45.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                    }
                    if (eachStockArray[nCurIdx].fPowerLongJar >= 6)
                    {
                        eachStockArray[nCurIdx].nPower60Time = nSharedTime;
                        swLogPowerLong60.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                    }


                    if (eachStockArray[nCurIdx].fPowerOnlyUp >= 2)
                    {
                        swLogOnlyUp2.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                    }
                    if (eachStockArray[nCurIdx].fPowerOnlyUp >= 4)
                    {
                        swLogOnlyUp4.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                    }
                    if (eachStockArray[nCurIdx].fPowerOnlyUp >= 6)
                    {
                        swLogOnlyUp6.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                    }
                    if (eachStockArray[nCurIdx].fPowerOnlyUp >= 8)
                    {
                        swLogOnlyUp8.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                    }

                    if (eachStockArray[nCurIdx].fPowerOnlyDown <= -2)
                    {
                        swLogOnlyDown2.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                    }
                    if (eachStockArray[nCurIdx].fPowerOnlyDown <= -4)
                    {
                        swLogOnlyDown4.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                    }
                    if (eachStockArray[nCurIdx].fPowerOnlyDown <= -6)
                    {
                        swLogOnlyDown6.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                    }
                    if (eachStockArray[nCurIdx].fPowerOnlyDown <= -8)
                    {
                        swLogOnlyDown8.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                    }


                    int nCheckCount;
                    int nSpeedPart = 0;
                    int nHogaVolumePart = 0;
                    int nHogaRatioPart = 0;
                    int nTradePart = 0;
                    int nPowerPart = 0;



                    int nLimitTime = SubTimeBySec(nSharedTime, 600);

                    if (eachStockArray[nCurIdx].nSpeed10Time > nLimitTime)
                        nSpeedPart++;
                    if (eachStockArray[nCurIdx].nSpeed20Time > nLimitTime)
                        nSpeedPart++;
                    if (eachStockArray[nCurIdx].nSpeed30Time > nLimitTime)
                        nSpeedPart++;
                    if (eachStockArray[nCurIdx].nSpeed40Time > nLimitTime)
                        nSpeedPart++;

                    if (eachStockArray[nCurIdx].nHogaVolume100Time > nLimitTime)
                        nHogaVolumePart++;
                    if (eachStockArray[nCurIdx].nHogaVolume80Time > nLimitTime)
                        nHogaVolumePart++;
                    if (eachStockArray[nCurIdx].nHogaVolume60Time > nLimitTime)
                        nHogaVolumePart++;
                    if (eachStockArray[nCurIdx].nHogaVolume40Time > nLimitTime)
                        nHogaVolumePart++;

                    if (eachStockArray[nCurIdx].nHogaRatio6Time > nLimitTime)
                        nHogaRatioPart++;
                    if (eachStockArray[nCurIdx].nHogaRatio7Time > nLimitTime)
                        nHogaRatioPart++;
                    if (eachStockArray[nCurIdx].nHogaRatio8Time > nLimitTime)
                        nHogaRatioPart++;
                    if (eachStockArray[nCurIdx].nHogaRatio9Time > nLimitTime)
                        nHogaRatioPart++;

                    if (eachStockArray[nCurIdx].nTrade200Time > nLimitTime)
                        nTradePart++;
                    if (eachStockArray[nCurIdx].nTrade150Time > nLimitTime)
                        nTradePart++;
                    if (eachStockArray[nCurIdx].nTrade100Time > nLimitTime)
                        nTradePart++;
                    if (eachStockArray[nCurIdx].nTrade70Time > nLimitTime)
                        nTradePart++;

                    if (eachStockArray[nCurIdx].nPower15Time > nLimitTime)
                        nPowerPart++;
                    if (eachStockArray[nCurIdx].nPower30Time > nLimitTime)
                        nPowerPart++;
                    if (eachStockArray[nCurIdx].nPower45Time > nLimitTime)
                        nPowerPart++;
                    if (eachStockArray[nCurIdx].nPower60Time > nLimitTime)
                        nPowerPart++;

                    nCheckCount = nSpeedPart + nHogaVolumePart + nHogaRatioPart + nTradePart + nPowerPart;

                    if (nCheckCount >= 5)
                    {
                        swLogTotalCheck5.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                    }

                    if (nCheckCount >= 7)
                    {
                        swLogTotalCheck7.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());

                    }

                    if (nCheckCount >= 10)
                    {
                        swLogTotalCheck10.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());

                    }

                    if (nCheckCount >= 15)
                    {
                        swLogTotalCheck15.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());

                    }







                  

                    if (eachStockArray[nCurIdx].fScoreVolume > 70)
                    {
                        if (eachStockArray[nCurIdx].fPurePerTrade > 0 && eachStockArray[nCurIdx].fHogaRatioVal > 0)
                        {
                            swLogVolume70.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                        }
                    }

                    if (eachStockArray[nCurIdx].fScoreVolume > 80)
                    {
                        swLogVolume80.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString());
                    }



                    if(eachStockArray[nCurIdx].nMarketGubun == KOSPI_ID)
                    {
                        eachStockArray[nCurIdx].swLog.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 5], nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 6], nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fAngleDirection, nR).ToString() + "\t" +  Math.Round(eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 7], nR).ToString() + "\t" + eachStockArray[nCurIdx].nMaxFs.ToString() + "\t" + Math.Round(((double)(eachStockArray[nCurIdx].nMaxFs - eachStockArray[nCurIdx].nFs) / eachStockArray[nCurIdx].nYesterdayEndPrice),nR).ToString() + "\t" + eachStockArray[nCurIdx].nMaxTime.ToString() + "\t" + SubTimeToTime(nSharedTime, eachStockArray[nCurIdx].nMaxTime).ToString());
                    }
                    else
                    {
                        eachStockArray[nCurIdx].swLog.WriteLine(nSharedTime.ToString() + "\t" + eachStockArray[nCurIdx].sCode + "\t" + eachStockArray[nCurIdx].sCodeName + "\t" + Math.Round(eachStockArray[nCurIdx].fCurSpeed, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fCntPerTime, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPurePerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaRatioVal, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fHogaPerTrade, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fSharePerHoga, nR).ToString() + "\t" + eachStockArray[nCurIdx].nFs.ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fStartGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerJar, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyUp, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerOnlyDown, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPowerWithoutGap, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fPower, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 5], nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 6], nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].fAngleDirection, nR).ToString() + "\t" + Math.Round(eachStockArray[nCurIdx].arrRecord[eachStockArray[nCurIdx].nIdxPointer, 7], nR).ToString() + "\t" + eachStockArray[nCurIdx].nMaxFs.ToString() + "\t" + Math.Round(((double)(eachStockArray[nCurIdx].nMaxFs - eachStockArray[nCurIdx].nFs) / eachStockArray[nCurIdx].nYesterdayEndPrice), nR).ToString()  + "\t" + eachStockArray[nCurIdx].nMaxTime.ToString() + "\t" + SubTimeToTime(nSharedTime, eachStockArray[nCurIdx].nMaxTime).ToString());
                    }
                    







                    //@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                    // 현재 거래중이고(nBuyReqCnt > 0) 현재 매수취소가 가능한 상태라면 접근 가능
                    if (eachStockArray[nCurIdx].isOrderStatus)
                    {
                        if ((eachStockArray[nCurIdx].nBuyReqCnt > 0) && !eachStockArray[nCurIdx].isCancelMode) // 미체결량이 남아있다면
                        {
                            // 현재 최우선매도호가가 지정상한가를 넘었거나 매매 요청시간과 현재시간이 너무 오래 차이난다면(= 매수가 너무 오래걸린다 = 거래량이 낮고 머 별거 없다)
                            if ((eachStockArray[nCurIdx].nFs > eachStockArray[nCurIdx].nCurLimitPrice) || (SubTimeToTimeAndSec(nSharedTime, eachStockArray[nCurIdx].nCurRqTime) >= MAX_REQ_SEC)) // 지정가를 초과하거나 오래걸린다면
                            {
                                curSlot.sRQName = "매수취소";
                                curSlot.nOrderType = 3; // 매수취소
                                curSlot.sCode = sCode;
                                curSlot.sOrgOrderId = eachStockArray[nCurIdx].sCurOrgOrderId; // 현재 매수의 원주문번호를 넣어준다.
                                curSlot.nRqTime = nSharedTime; // 현재시간설정

                                tradeQueue.Enqueue(curSlot); // 매매신청큐에 인큐

                                eachStockArray[nCurIdx].isCancelMode = true; // 현재 매수취소 불가능상태로 만든다
                                testTextBox.AppendText(nSharedTime.ToString() + " : " + sCode + " 매수취소신청 \r\n"); //++
                            }
                        }
                    }


                    int nBuySlotIdx = buySlotCntArray[nCurIdx]; // 현재 종목의 매수 record수를 체크
                                                                // 한번의 레코드 신청이 있다하더라도 매수가 완료된 시점에 ++하기 떄문에
                                                                // 처음에는 0일테고 하나의 거래가 완료되면 1이 되니 그때부터 for문에서 접근 가능
                    if (nBuySlotIdx > 0) // 보유종목이 있다면
                    {
                        int nBuyPrice;
                        double fYield;

                        for (int i = 0; i < nBuySlotIdx; i++) // 반복적 확인
                        {
                            // 그리고 !isSelled 아직 판매완료가 안됐을때 접근 가능
                            if (!buySlotArray[nCurIdx, i].isSelled)
                            {
                                bool isSell = false;

                                nBuyPrice = buySlotArray[nCurIdx, i].nBuyPrice; // 처음 초기화됐을때는 0인데 체결이 된 상태에서만 접근 가능하니 사졌을 때의 평균매입가
                                fYield = (eachStockArray[nCurIdx].nFb - nBuyPrice) / nBuyPrice; // 현재 최우선매수호가 와의 손익률을 구한다
                                fYield -= STOCK_TAX + STOCK_FEE + STOCK_FEE; // 거래세와 거래수수료 차감

                                if (fYield >= buySlotArray[nCurIdx, i].fTargetPer) // 손익률이 익절퍼센트를 넘기면
                                {
                                    isSell = true;
                                    curSlot.sRQName = "익절매도";
                                    testTextBox.AppendText(nSharedTime.ToString() + " : " + sCode + " 익절매도신청 \r\n"); //++
                                }
                                else if (fYield <= buySlotArray[nCurIdx, i].fBottomPer) // 손익률이 손절퍼센트보다 낮으면
                                {
                                    isSell = true;
                                    curSlot.sRQName = "손절매도";
                                    testTextBox.AppendText(nSharedTime.ToString() + " : " + sCode + " 손절매도신청 \r\n"); //++
                                }

                                if (isSell)
                                {
                                    curSlot.nOrderType = 2; // 신규매도
                                    curSlot.sCode = sCode;
                                    curSlot.nQty = buySlotArray[nCurIdx, i].nBuyVolume; // 이 레코드에 있는 전량을 판매한다
                                    curSlot.sHogaGb = "03";
                                    curSlot.sOrgOrderId = "";
                                    curSlot.nBuySlotIdx = i; // 나중에 요청전송이 실패할때 다시 취소하기 위해 적어놓는 변수
                                    curSlot.nEachStockIdx = nCurIdx; // 현재 종목의 인덱스
                                    curSlot.nRqTime = nSharedTime; // 현재시간 설정

                                    tradeQueue.Enqueue(curSlot); // 매매요청큐에 인큐한다
                                    buySlotArray[nCurIdx, i].isSelled = true; // 현재 거래레코드는 판매완료됐다, 요청전송 실패됐을때는 다시 false로 설정된다.

                                }
                            }
                        } // END ---- 반복적 확인 종료
                    } // END ---- 보유종목이 있다면









                }
            }// End ---- e.sRealType.Equals("주식체결")
            else if (e.sRealType.Equals("장시작시간"))
            {

                string sGubun = axKHOpenAPI1.GetCommRealData(e.sRealKey, 215); // 장운영구분 0 :장시작전, 3 : 장중, 4 : 장종료
                string sTime = axKHOpenAPI1.GetCommRealData(e.sRealKey, 20); // 체결시간
                string sTimeRest = axKHOpenAPI1.GetCommRealData(e.sRealKey, 214); // 잔여시간
                if (sGubun.Equals("0")) // 장시작 전
                {
                    testTextBox.AppendText(sTimeRest.ToString() + " : 장시작전\r\n"); ;//++
                }
                else if (sGubun.Equals("3")) // 장 중
                {
                    testTextBox.AppendText("장중\r\n");//++
                    isMarketStart = true;
                    nFirstTime = int.Parse(sTime);
                    nFirstTime -= nFirstTime % 10000;
                    
                    nTenTime = AddTimeBySec(nFirstTime, 3600);
                    RequestHoldings(0); // 장시작하고 잔여종목 전량매도
                }
                else
                {
                    if (sGubun.Equals("2")) // 장 종료 10분전 동시호가
                    {
                        testTextBox.AppendText(sTimeRest.ToString() + " : 장종료전\r\n");//++
                        isMarketStart = false;
                        nShutDown++;
                        RequestHoldings(0); // 장 끝나기 전 잔여종목 전량매도
                    }
                    else if (sGubun.Equals("4")) // 장 종료
                    {
                        testTextBox.AppendText("장종료\r\n");//++
                        isMarketStart = false;
                        nShutDown++;
                        isForCheckHoldings = true;
                        RequestHoldings(0);
                        RequestTradeResult();

                        swLogFirstAndLast.Close();
                        swLogFirstAndLastEverage.Close();
                        swLogTopAndBottom.Close();
                        swLogTopAndBottomEverage.Close();

                        swLogSpeed10.Close();
                        swLogSpeed20.Close();
                        swLogSpeed30.Close();
                        swLogSpeed40.Close();

                        swLogHogaRatio60.Close();
                        swLogHogaRatio70.Close();
                        swLogHogaRatio80.Close();
                        swLogHogaRatio90.Close();

                        swLogHogaVolume100.Close();
                        swLogHogaVolume80.Close();
                        swLogHogaVolume60.Close();
                        swLogHogaVolume40.Close();

                        swLogPowerLong15.Close();
                        swLogPowerLong30.Close();
                        swLogPowerLong45.Close();
                        swLogPowerLong60.Close();

                        swLogTradeVolume200.Close();
                        swLogTradeVolume150.Close();
                        swLogTradeVolume100.Close();
                        swLogTradeVolume70.Close();

                        swLogTotalCheck5.Close();
                        swLogTotalCheck7.Close();
                        swLogTotalCheck10.Close();
                        swLogTotalCheck15.Close();

                        swLogVolume80.Close();
                        swLogVolume70.Close();
                        swLogSpeed6.Close();
                        swLogSpeedAfterNoon.Close();

                        swLogOneToTenSucceed.Close();
                        swLogQuarterSucceed.Close();
                        swLogHalfSucceed.Close();
                        swLogFullSucceed.Close();
                        swLogHalfFullSucceed.Close();
                        swLogDoubleSucceed.Close();

                        swLogPower1.Close();
                        swLogPower2.Close();
                        swLogPower5.Close();
                        swLogLongPower5.Close();
                        swLogPowerM2.Close();
                        swLogPowerM5.Close();

                        swLogOnlyUp2.Close();
                        swLogOnlyUp4.Close();
                        swLogOnlyUp6.Close();
                        swLogOnlyUp8.Close();

                        swLogOnlyDown2.Close();
                        swLogOnlyDown4.Close();
                        swLogOnlyDown6.Close();
                        swLogOnlyDown8.Close();

                        swLogMarketSituSec.Close();
                        swLogMarketSituMin.Close();
                        for (int i =0;i < nEachStockIdx; i++)
                        {
                            eachStockArray[i].swLog.Close();
                        }
                    }
                }

            } // End ---- e.sRealType.Equals("장시작시간")
        }


        // ==================================================
        // 주식주문(접수, 체결, 잔고) 이벤트발생시 핸들러메소드
        // ==================================================
        private void OnReceiveChejanDataHandler(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveChejanDataEvent e)
        {
            if (e.sGubun.Equals("0")) // 접수와 체결 
            {

                string sTradeTime = axKHOpenAPI1.GetChejanData(908); // 체결시간
                nSharedTime = Math.Abs(int.Parse(sTradeTime));

                string sCode = axKHOpenAPI1.GetChejanData(9001).Substring(1); // 종목코드
                int nCodeIdx = int.Parse(sCode);
                nCurIdx = eachStockIdxArray[nCodeIdx];
                int nCurBuySlotIdx = buySlotCntArray[nCurIdx];

                string sOrderType = axKHOpenAPI1.GetChejanData(905).Trim(charsToTrim); // +매수, -매도, 매수취소
                string sOrderStatus = axKHOpenAPI1.GetChejanData(913).Trim(); // 주문상태(접수, 체결, 확인)
                string sOrderId = axKHOpenAPI1.GetChejanData(9203).Trim(); // 주문번호
                int nOrderVolume = Math.Abs(int.Parse(axKHOpenAPI1.GetChejanData(900))); // 주문수량
                string sCurOkTradePrice = axKHOpenAPI1.GetChejanData(914).Trim(); // 단위체결가 없을땐 ""
                string sCurOkTradeVolume = axKHOpenAPI1.GetChejanData(915).Trim(); // 단위체결량 없을땐 ""
                int nNoTradeVolume = Math.Abs(int.Parse(axKHOpenAPI1.GetChejanData(902))); // 미체결량

                //string sOkTradePrice = axKHOpenAPI1.GetChejanData(910).Trim(); // 체결가 없을땐 ""
                //string sOkTradeVolume = axKHOpenAPI1.GetChejanData(911).Trim(); // 체결량 없을땐 ""

                // ---------------------------------------------
                // 매수 데이터 수신 순서
                // 매수접수 - 매수체결
                // 매수접수 - (매수취소) - 매수체결
                // 매수접수 - (매수취소) - 매수취소접수 - 매수취소확인 - 매수접수(매수취소확인)
                // 매수접수 - (매수취소) - 매수체결 - 매수취소접수 - 매수취소확인 - 매수체결(매수취소확인)
                // ---------------------------------------------
                if (sOrderType.Equals("매수"))
                {

                    if (sOrderStatus.Equals("체결"))
                    {
                        // 매수-체결됐으면 3가지로 나눠볼 수 있는데
                        // 1. 일반적으로 일부 체결된 경우
                        // 2. 전량 체결된 경우
                        // 3. 일부 체결된 후 나머지는 매수취소된 경우(미체결 클리어를 위해 얻어지는 경우)


                        // 문자열로 받아진 단위체결량과 단위체결가를 정수로 바꾸는 작업을 한다.
                        // 접수나 취소 때는 체결가~ 종류는 "" 공백으로 받아지기 때문에
                        // 정수 캐스팅을 하면 오류가 나기 때문이다
                        int nCurOkTradeVolume;
                        int nCurOkTradePrice;
                        try
                        {
                            nCurOkTradeVolume = Math.Abs(int.Parse(sCurOkTradeVolume)); // n단위체결량
                            nCurOkTradePrice = Math.Abs(int.Parse(sCurOkTradePrice)); // n단위체결가
                        }
                        catch (Exception ex)
                        {
                            // 혹시 문자열이 ""이라면 매수체결시 받아지는 체결 메시지다.
                            eachStockArray[nCurIdx].isCancelComplete = false; // 매수취소완료버튼 초기화
                            eachStockArray[nCurIdx].isCancelMode = false; // 해당종목의 현재매수취소버튼 초기화
                            buySlotArray[nCurIdx, nCurBuySlotIdx].isAllBuyed = true; // 해당종목의 매수레코드의 매수완료 on
                            buySlotCntArray[nCurIdx]++; // 매수레코드 수 증가
                            eachStockArray[nCurIdx].nBuyReqCnt--; // 매수요청 카운트 감소
                            eachStockArray[nCurIdx].isOrderStatus = false; // 매매중 off
                            return;
                        }
                        // 예수금에 지정상한가와 매입금액과의 차이만큼을 다시 복구시켜준다.
                        nCurDepositCalc += (eachStockArray[nCurIdx].nCurLimitPrice - nCurOkTradePrice) * nCurOkTradeVolume; // 예수금에 (추정매수가 - 실매수가) * 실매수량 더해준다. //**

                        // 이것은 현재매수 구간이기 떄문에
                        // 해당레코드의 평균매입가와 매수수량을 조정하기 위한 과정이다
                        int sum = buySlotArray[nCurIdx, nCurBuySlotIdx].nBuyPrice * buySlotArray[nCurIdx, nCurBuySlotIdx].nBuyVolume;
                        sum += nCurOkTradePrice * nCurOkTradeVolume;
                        buySlotArray[nCurIdx, nCurBuySlotIdx].nBuyVolume += nCurOkTradeVolume;
                        buySlotArray[nCurIdx, nCurBuySlotIdx].nBuyPrice = sum / buySlotArray[nCurIdx, nCurBuySlotIdx].nBuyVolume;

                        if (nNoTradeVolume == 0) // 매수 전량 체결됐다면
                        {
                            // 매수가 전량 체결됐다면 
                            // 체결-매수취소와 유사하게 진행된다 하나 다른점은 매수취소완료 시그널을 건들 필요가 없다는 것이다.
                            // 현재매수취소 그리고 일부라도 체결됐으니 해당레코드에 구매됐다는 시그널을 on해주고 레코드인덱스를 한칸 늘린다
                            // 매수요청 카운트도 낮추고 현재 매매중인 시그널을 off해준다.
                            eachStockArray[nCurIdx].isCancelMode = false; // 매수취소를 했어도 취소접수가 안되면 그대로 전량체결이 되니까 이때 cancelMode를 false한다.
                            buySlotArray[nCurIdx, nCurBuySlotIdx].isAllBuyed = true; // 해당종목의 매수레코드의 매수완료 on
                            buySlotCntArray[nCurIdx]++; // 매수레코드 수 증가
                            eachStockArray[nCurIdx].nBuyReqCnt--; // 매수요청 카운트 감소
                            eachStockArray[nCurIdx].isOrderStatus = false; // 매매중 off

                            testTextBox.AppendText(sTradeTime + " : " + sCode + " 매수 체결완료 \r\n"); //++
                        }
                    } //  END ---- 매수체결 끝

                    else if (sOrderStatus.Equals("접수"))
                    {
                        if (nNoTradeVolume == 0) // 전량 매수취소가 완료됐다면
                        {
                            // 접수-매수취소는
                            // 체결이 하나도 안된상태에서 매수주문이 모두 매수취소 된 상황이다
                            // 많은 설정을 할 필요가 없다
                            // 여기서는 isAllBuyed와 현재레코드인덱스를 더하지 않는 이유는 체결데이터가 없기때문에
                            // 굳이 인덱스를 늘려 레코드만 증가시킨다면 실시간에서 관리함에 시간이 더 소요되기 때문이다
                            eachStockArray[nCurIdx].isCancelComplete = false; // 매수취소완료버튼 초기화
                            eachStockArray[nCurIdx].isCancelMode = false; // 해당종목의 현재매수취소버튼 초기화
                            eachStockArray[nCurIdx].nBuyReqCnt--; // 매수요청 카운트 감소
                            eachStockArray[nCurIdx].isOrderStatus = false; // 매매중 off
                        }
                        else // 매수 주문인경우
                        {
                            // 원주문번호만 설정해준다.

                            eachStockArray[nCurIdx].sCurOrgOrderId = sOrderId; // 현재원주문번호 설정
                            eachStockArray[nCurIdx].isOrderStatus = true; // 매매중 on

                            nCurDepositCalc -= (int)(nOrderVolume * eachStockArray[nCurIdx].nCurLimitPrice * (1 + VIRTUAL_STOCK_FEE)); // 예수금에서 매매수수료까지 포함해서 차감

                            testTextBox.AppendText(sTradeTime + " : " + sCode + ", " + nOrderVolume.ToString() + "(주) 매수 접수완료 \r\n"); //++
                                                                                                                                      //---------------------------------------------
                                                                                                                                      // 구매기록 초기화
                                                                                                                                      // --------------------------------------------
                                                                                                                                      // 여기서 퍼센트는 매수,매도 시 curSlot에 설정하고
                                                                                                                                      // 매매컨트롤러에서 eachStockArray에 설정하는 과정을 거쳐
                                                                                                                                      // buySlotArray에 설정되는 과정으로 마쳐진다.
                            buySlotArray[nCurIdx, nCurBuySlotIdx].isSelled = false;
                            buySlotArray[nCurIdx, nCurBuySlotIdx].isAllBuyed = false;
                            buySlotArray[nCurIdx, nCurBuySlotIdx].fTargetPer = eachStockArray[nCurIdx].fTargetPercent;
                            buySlotArray[nCurIdx, nCurBuySlotIdx].fBottomPer = eachStockArray[nCurIdx].fBottomPercent;
                            buySlotArray[nCurIdx, nCurBuySlotIdx].nBuyPrice = 0;
                            buySlotArray[nCurIdx, nCurBuySlotIdx].nBuyVolume = 0;
                        }
                    } // END ---- 매수접수끝
                } // END ---- orderType.Equals("매수")
                else if (sOrderType.Equals("매도"))
                {
                    if (sOrderStatus.Equals("체결"))
                    {
                        nCurDepositCalc += (int)(Math.Abs(int.Parse(sCurOkTradeVolume)) * Math.Abs(int.Parse(sCurOkTradePrice)) * (1 - (STOCK_TAX + VIRTUAL_STOCK_FEE))); //**

                        if (nNoTradeVolume == 0)
                        {
                            if (eachStockArray[nCurIdx].nSellReqCnt > 0) //** 아침에 어제 매도 안된 애들이 남아있으면 sellReqCnt가 음수가 될 수 도 있으니 0이 넘어야만 차감되게끔 한다.
                                eachStockArray[nCurIdx].nSellReqCnt--;

                            eachStockArray[nCurIdx].isOrderStatus = false; // 매매중 off
                            testTextBox.AppendText(sTradeTime + " : " + sCode + " 매도 체결완료 \r\n"); //++
                        }
                    }
                    else if (sOrderStatus.Equals("접수"))
                    {
                        testTextBox.AppendText(sTradeTime + " : " + sCode + ", " + nOrderVolume.ToString() + "(주) 매도 접수완료 \r\n"); //++
                        eachStockArray[nCurIdx].isOrderStatus = true; // 매매중 on
                        eachStockArray[nCurIdx].sCurOrgOrderId = sOrderId; // 원주문번호
                    }
                } // END ---- orderType.Equals("매도")
                else if (sOrderType.Equals("매수취소"))
                {
                    // ----------------------------------
                    // 야기할 수 있는 문제
                    // 1. 매수취소확인후 접수,체결을 안보내준다.
                    // 2. 매수취소확인전에 접수,체결을 보내준다.
                    // ----------------------------------

                    // 매수취소에서는 매수취소완료버튼 on
                    // 매수취소수량이 있으면 그만큼 예수금 더해주면 된다
                    // 거래중, 매매완료 등등의 처리는 매수에서 완료한다.
                    if (sOrderStatus.Equals("접수"))
                    {
                        testTextBox.AppendText(sTradeTime + " : " + sCode + ", " + nOrderVolume.ToString() + "(주) 매수취소 접수완료 \r\n"); //++
                        // 매수취소 접수가 되면 거의 확정적으로 매수취소확인 따라오며 
                        // 매수취소 접수때부터 이미 매수취소된거같음.
                    }
                    else if (sOrderStatus.Equals("확인"))
                    {
                        eachStockArray[nCurIdx].isCancelComplete = true; // 매수취소 완료

                        // 매수취소확인은 사실상 매수취소 수량이 있는거고 미체결량은 0인 상태일 테지만 
                        // 예기치 못한 오류로 인해 문제가 생길 수 도 있으니
                        // 매수취소 수량과 미체결량을 검사해준다.
                        if (nNoTradeVolume < nOrderVolume && nOrderVolume > 0) // 매수취소된 수량이 있다면
                        {
                            nCurDepositCalc += (int)((nOrderVolume - nNoTradeVolume) * (eachStockArray[nCurIdx].nCurLimitPrice * (1 + VIRTUAL_STOCK_FEE)));
                        }

                    }
                } // END ---- orderType.Equals("매수취소")

            } // End ---- e.sGubun.Equals("0") : 접수,체결

            else if (e.sGubun.Equals("1")) // 잔고
            {
                string sCode = axKHOpenAPI1.GetChejanData(9001).Substring(1); // 종목코드
                int nCodeIdx = Math.Abs(int.Parse(sCode));
                nCurIdx = eachStockIdxArray[nCodeIdx];

                int nHoldingQuant = Math.Abs(int.Parse(axKHOpenAPI1.GetChejanData(930))); // 보유수량
                eachStockArray[nCurIdx].nHoldingsCnt = nHoldingQuant;
            } // End ---- e.sGubun.Equals("1") : 잔고
        }

    }
}
