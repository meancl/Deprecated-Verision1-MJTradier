namespace MJTradier
{
    public partial class Form1
    {
        // ============================================
        // 각 종목이 가지는 개인 구조체
        // ============================================
        public struct EachStock
        {

            public bool isExcluded; // 실시간 제외대상확인용 bool변수
            // ----------------------------------
            // 기본정보 변수
            // ----------------------------------
            public string sRealScreenNum; // 실시간화면번호
            public string sCode; // 종목번호
            public string sCodeName; // 종목명
            public int nMarketGubun; // 코스닥번호면 코스닥, 코스피번호면 코스피
            public long lShareOutstanding; // 유통주식수
            public long lFullNumOfStock;
            public double fShareOutstandingRatio; // 유통비율
            public double f250TopCompare; // 250최고가대비율
            public double f250BottomCompare; // 250최저가대비율

            // ----------------------------------
            // 매매관련 변수
            // ----------------------------------
            public int nCurLimitPrice; // 지정가가 estimatedPrice를 초과하는 미체결 수량이 남았다면 처분하기 위한 변수
            public int nCurRqTime; // 매수주문했을때의 시간
            public bool isOrderStatus; // 현재 매매중인 지 확인하는 변수;
            public string sCurOrgOrderId; // 원주문번호   default:""
            public int nBuyReqCnt; // 현재 종목의 매수신청카운트
            public int nSellReqCnt; // 현재 종목의 매도신청카운트 
            public bool isCancelMode; // 현재 매수에서 매수취소가 나왔으면 더이상의 현재의 거래에서 매수취소요청을 금지하기 위한 변수
            public bool isCancelComplete; // 매수취소가 성공한 경우를 판별하는 변수
            public int nHoldingsCnt; // 보유종목수
            public double fTargetPercent; // 익절 퍼센트
            public double fBottomPercent; // 손절 퍼센트

            // ----------------------------------
            // 초기 변수
            // ----------------------------------
            public bool isFirstCheck; // 초기설정용 bool 변수
            public int nTodayStartPrice; // 시초가
            public int nYesterdayEndPrice; // 어제 종가 
            public int nStartGap; // 갭 가격
            public double fStartGap; // 갭 등락율

            // ----------------------------------
            // 주식호가 변수
            // ----------------------------------
            public int nTotalBuyHogaVolume; // 매수호가량
            public int nTotalSellHogaVolume; // 매도호가량
            public int nTotalHogaVolume; //  총호가량


            // ----------------------------------
            // 주식체결 변수
            // ----------------------------------
            public int nFs; // 최우선 매도호가
            public int nFb; // 최우선 매수호가
            public int nDiff;
            public int nCnt; // 인덱스 
            public int nTv;  // 체결량 
            public double fTs; // 체결강도
            public double fPowerWithoutGap; // 시초가 등락율
            public double fPower; // 전일종가 등락률 
            public double fPrevPowerWithoutGap; // 이전 시초가 등락율;
            public double fMaxPowerWithoutGap;
            public double fMinPowerWithoutGap;

            public bool isViMode;
            public int nViTime;

            // ----------------------------------
            // 주식상태 변수
            // ----------------------------------
            public int nPrevUpdateTime; // 이전기본(속도, 체결량, 순체결량)조정 시간
            public int nPrevPriceUpdateTime; // 이전가격조정 시간
            public double fSpeedVal; // 속도재료1
            public int nSpeedPush; // 속도재료2
            public double fCurSpeed; // 속도변수( 이전 * 0.2 + 현재 * 0.8 )
            public double fTradeVal; // 체결량재료1
            public double fTradePush; // 체결량재료2
            public double fCurTrade; // 체결량변수
            public double fCurHogaRatio; // 호가비율재료1
            public double fSharePerTrade; // 유통주식수 per 체결량( 질량재료 )
            public double fSharePerHoga; // 유통주식수 per 호가량( 질량재료 )
            public double fHogaPerTrade; // 호가량 per 체결량( 질량재료 )
            public double fPurePerTrade; // 순체결량변수( 방향변수, 이전 * 0.2 + 현재 * 0.8 )
            public double fPureTradeVal; // 순체결량재료1
            public double fPureTradePush; // 순체결량재료2
            public double fCurPureTrade; // 순체결량재료3( 이전 * 0.2 + 현재 * 0.8 )
            public double fTotalHogaVolumeVal; // 총호가량변수( 이전 * 0.2 + 현재 * 0.8 )
            public double fHogaRatioVal; // 호가비율변수( 방향재료, 이전 * 0.2 + 현재 * 0.8 )
            public double fPowerJar; // 등락율변수( 매초당 0.99퍼 )
            public double fPowerLongJar;
            public double fScoreDirection; // 점수계산용 방향변수( fPurePerTrade, f2000Ratio, fHogaRatioVal )
            public double fScoreVolume; // 점수계산용 질량변수( fSharePerTrade, fSharePerHoga, fHogaPerTrade )
            public int nScoreCheckTrigger; // 초기화된후 nUpdateTime이 지나야 fCurScore이 어느정도 안정되었다고 봐야하기 때문에 nScoreCheckTrigger가 2부터 가격을 계산가능
            public int nScoreUpdateTime; // 이전점수조정 시간
            public double fCntPerTime;

            public int nIdxPointer;
            public int nFsPointer;
            public int nMaxPointer;
            public int nMinPointer;
            public int nFirstPointer;
            public int nLastPointer;

            public double[,] arrRecord;

            // 시종가
            public int nMaxFs;
            public int nMaxTime;
            public int nMinFs;
            public int nMinTime;
            public bool isGooiTime;

            // 시종가 평균
            public int nMaxEverageFs;
            public int nMaxEverageTime;
            public int nMinEverageFs;
            public int nMinEverageTime;
            public bool isGooiTimeEverage;

            // 저고가
            public int nMaxTopFs;
            public int nMaxTopTime;
            public int nMinBottomFs;
            public int nMinBottomTime;
            public bool isGooiTimeEnd;

            // 저고가 평균
            public int nMaxTopEverageFs;
            public int nMaxTopEverageTime;
            public int nMinBottomEverageFs;
            public int nMinBottomEverageTime;
            public bool isGooiTimeEndEverage;

            public double fPowerOnlyUp;
            public double fPowerOnlyDown;

            public int nUpgradeCnt; // power가 2이상일때 과거시간과 10분이상 차이나게 된다면 카운트를 올린다.
            public int nUpgradeTime; // 이전 power와의 차이를 기록하기 위한 시간변수
           
            public long lCurTradeAmount;
            public long lCurTradeAmountOnlyUp;
            public long lCurTradeAmountOnlyDown;
            public double fCurTradeAmountRatio;
            public double fCurTradeAmountRatioOnlyUp;
            public double fCurTradeAmountRatioOnlyDown;
            public double fAngleDirection;
            public System.IO.StreamWriter swLog;


            /// <summary>
            // 속도
            public int nSpeed10Time;
            public int nSpeed20Time;
            public int nSpeed30Time;
            public int nSpeed40Time;

            // g호가비율
            public int nHogaRatio6Time;
            public int nHogaRatio7Time;
            public int nHogaRatio8Time;
            public int nHogaRatio9Time;

            // 호가총량
            public int nHogaVolume100Time;
            public int nHogaVolume80Time;
            public int nHogaVolume60Time;
            public int nHogaVolume40Time;

            // 체결량
            public int nTrade200Time;
            public int nTrade150Time;
            public int nTrade100Time;
            public int nTrade70Time;

            // 가격변동
            public int nPower15Time;
            public int nPower30Time;
            public int nPower45Time;
            public int nPower60Time;
            /// 
            /// </summary>
            ///  
            public bool isOneToTenSucceed;
            public bool isQuarterSucceed;
            public bool isHalfSucceed;
            public bool isFullSucceed;
            public bool isHalfFullSucceed;
            public bool isDoubleSucceed;

            public bool isIndexChecked;

            public long lTotalNumOfStock;
            public long lPrevTotalPriceOfStock;
            public long lCurTotalPriceOfStock;


            public double fEverageFlowLine;
            public double fRecentEverageFlowLine;
            public double fFluctuationVar;
        }



        // ============================================
        // 매매요청 큐에 저장하기 위한 구조체변수
        // ============================================
        public struct TradeSlot
        {
            public int nRqTime; // 주문요청시간
            public double fTargetPercent; // 익절 퍼센트 
            public double fBottomPercent; // 손절 퍼센트 
            public int nEachStockIdx; // 개인구조체인덱스
            public int nBuySlotIdx; // 구매열람인덱스 , 매도요청이 실패하면 해당인덱스를 통해 다시 요청할 수 있게 하기 위한 변수
            // ----------------------------------
            // SendOrder 인자들
            // ----------------------------------
            public string sRQName; // 사용자 구분명
            public string sScreenNo; // 화면번호
            public string sAccNo; // 계좌번호 10자리
            public int nOrderType; // 주문유형 1:신규매수, 2:신규매도 3:매수취소, 4:매도취소, 5:매수정정, 6:매도정정
            public string sCode; // 종목코드(6자리)
            public int nQty; // 주문수량
            public int nOrderPrice; // 주문가격
            public string sHogaGb; // 거래구분 (00:지정가, 03:시장가, ...)
            public string sOrgOrderId;  // 원주문번호. 신규주문에는 공백 입력, 정정/취소시 입력합니다.
        }

        // ============================================
        // 현재보유종목 열람용 구조체변수
        // ============================================
        public struct Holdings
        {
            public string sCode;
            public string sCodeName;
            public double fYield;
            public int nHoldingQty;
            public int nBuyedPrice;
            public int nCurPrice;
            public int nTotalPL;
            public int nNumPossibleToSell;
        }

        // ============================================
        // 개인구조체 구매횟수 구조체
        // ============================================
        public struct BuySlot //TODAY
        {
            public int nBuyPrice; // 얼마에 구매했어
            public int nBuyVolume; // 얼마나 구매했어
            public double fTargetPer; // 얼마에 익절할거야
            public double fBottomPer; // 얼마에 손절할거야
            public bool isSelled; // 전부 팔렸어
            public bool isAllBuyed; // 전부 사졌어
            public int nBuyEndTime; //  체결완료됐을때 시간
            public int nFloatingTime; // 현재 익절선과 손절선을 안 건들인 지 얼마나 됐는지를 체크

        }

    }
}
