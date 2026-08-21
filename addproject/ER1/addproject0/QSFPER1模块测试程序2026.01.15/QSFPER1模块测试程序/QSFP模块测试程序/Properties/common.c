/*
********************************************************************************
*   For XFP-10G Transceiver.
*   Author:   
*   MCU:      ADuC7020   (IAR EWARM 4.42A)
*   LDD:      Gn2017     
*   Version:  2.0
*   Date   :  2011.8.3
*
*   Remark :
*
********************************************************************************
*/

#include "includes.h"



//***************************  Public Variable  ******************************//
//----------------------------------------------------------------------------//

/*
// TEMP, VCC,TX_PWR, RX_PWR,BIAS Alarm Threshold 
int16  temp_alarm_hi;
int16  temp_alarm_lo;
uint16 vcc_alarm_hi;
uint16 vcc_alarm_lo;
uint16 tx_pwr_alarm_hi;
uint16 tx_pwr_alarm_lo;
uint16 rx_pwr_alarm_hi;
uint16 rx_pwr_alarm_lo;
uint16 bias_alarm_hi;
uint16 bias_alarm_lo;

//TEMP, VCC,TX_PWR, RX_PWR,BIAS Warning Threshold 
int16  temp_warn_hi;
int16  temp_warn_lo;
uint16 vcc_warn_hi;
uint16 vcc_warn_lo;
uint16 tx_pwr_warn_hi;
uint16 tx_pwr_warn_lo;
uint16 rx_pwr_warn_hi;
uint16 rx_pwr_warn_lo;
uint16 bias_warn_hi;
uint16 bias_warn_lo;
*/
/*
unsigned char isr_80_B = gLowerMem[80];
unsigned char isr_81_B = gLowerMem[81];
unsigned char isr_82_B = gLowerMem[82];
unsigned char isr_83_B = gLowerMem[83];
unsigned char isr_84_B = gLowerMem[84];
unsigned char isr_85_B = gLowerMem[85];
unsigned char isr_86_B = gLowerMem[86];
unsigned char isr_87_B = gLowerMem[87];
unsigned char isr_88_B = gLowerMem[88];
unsigned char isr_89_B = gLowerMem[89];
unsigned char isr_90_B = gLowerMem[90];
unsigned char isr_91_B = gLowerMem[91];
unsigned char isr_92_B = gLowerMem[92];
unsigned char isr_93_B = gLowerMem[93];
unsigned char isr_94_B = gLowerMem[94];
unsigned char isr_95_B = gLowerMem[95];
*/


/////////////////////////////////////////////////////

/////////////////////////////////////////////////
uint8 gLowerMem[128] =  // 0-127    128*1  = 128
{
	0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
	0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
};
uint8 gTable0[128] =  // Table0   128*1  = 128
{
	0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
	0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
};
uint8 gSerialTbl[128] =  // Table1   128*1  = 128
{
	0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
	0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
};
uint8 gUserTbl[128] =  // Table2   128*1  = 128
{
	0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
	0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
};

uint8 gVendorTbl[128] =  // Table3   128*1  = 128
{
	0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
	0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
};

uint16 gERLookupTbl[ER_LOOKUP_CNT + 5][2] =  // Table4   32*2*2 = 128 
{	
 	{ 150, 70 },    // -40
	{ 150, 70 },    // -35
	{ 150, 70 },    // -30
	{ 150, 70 },    // -25
	{ 150, 70 },    // -20
	{ 150, 70 },    // -15
	{ 150, 70 },    // -10
	{ 150, 70 },    // -5
	{ 150, 70 },    // 0
	{ 150, 70 },    // 5
	{ 150, 70 },    // 10
	{ 150, 70 },    // 15
	{ 150, 70 },    // 20
	{ 150, 70 },    // 25
	{ 150, 70 },    // 30
	{ 150, 70 },    // 35
	{ 150, 70 },    // 40
	{ 150, 70 },    // 45
	{ 150, 70 },    // 50
	{ 150, 70 },    // 55
	{ 150, 70 },    // 60
	{ 150, 70 },    // 65
	{ 150, 70 },    // 70
    { 150, 70 },    // 75
	{ 150, 70 },    // 80
	{ 150, 70 },    // 85
	{ 150, 70 },    // 90
	{ 150, 70 },    // 95
	//
	{ 150, 70 },    // 100
	{ 150, 70 },    // 105
	{ 150, 70 },    // 110
	{ 150, 70 }     // 115
};

int32 gCalParam[16][2] =  // Table5   16*2*4 = 128
{
	{ 1,  0 }, // TX_PWR K B 2008.11.07
	{ 1,  0 }, // RX_PWR K B
	{ 0,  0 }, 
	{ 0,  0 }, 
	{ 0,  0 }, 
	{ 0,  0 },
	{ 0,  0 },
	{ 0,  0 },
	{ 0,  0 },
	{ 0,  0 }
};

uint8  *g_pbTalbe = gLowerMem;
uint8  gByteAddr  = 0;
uint8  gDDMUpdateFlag = 0;
uint16 gADCValue      = 0;
uint8  gI2CWriteFlag  = 0;

uint8  gInfoUpdateFlag = 0;

uint8  gDacUpdateFlag = 1;

uint8  gUserTblWrite  = 0;
uint8  gSerialTblWrite  = 0;

uint8  gVendorFlagValid  = 0;
uint8  gUserFlagValid    = 0;
uint8  gSerialFlagValid  = 0;

uint8  tempIndex = 0;

uint32  gModSetDACVal = 0;
uint32  gApcSetDACVal = 0;

uint8 isr_80_B = 0;
uint8 isr_81_B = 0;
uint8 isr_82_B = 0;
uint8 isr_83_B = 0;
uint8 isr_84_B = 0;
uint8 isr_85_B = 0;
uint8 isr_86_B = 0;
uint8 isr_87_B = 0;
uint8 isr_88_B = 0;
uint8 isr_89_B = 0;
uint8 isr_90_B = 0;
uint8 isr_91_B = 0;
uint8 isr_92_B = 0; 
uint8 isr_93_B = 0;
uint8 isr_94_B = 0;
uint8 isr_95_B = 0;

void Delay(unsigned int i)
{
	while (i--);
}

/***----------------------------------------------------------------------------
* Call mode:  void DACOutUpdate(void)
* Function :  Update DAC Output, for DAC Compensate Function call.
------------------------------------------------------------------------------*/
void ErDACOutUpdate(void)
{
	static uint8 oldTempIndex = 200;
	
	if (oldTempIndex == tempIndex)
	{
		return;
	}
	
	oldTempIndex = tempIndex;
      
        gModSetDACVal = gERLookupTbl[oldTempIndex][0];
        gApcSetDACVal = gERLookupTbl[oldTempIndex][1];
        
        gVendorTbl[0x70]=(uint8)((gModSetDACVal >> 8) & 0xFF); 
        gVendorTbl[0x71]=(uint8)(gModSetDACVal & 0xFF);      
        gVendorTbl[0x6E]=0;
        gVendorTbl[0x6F]=(uint8)((gApcSetDACVal & 0xFF));
	
	// MOD
	I2CWriteData(IIC_ADDR, 110, (uint8)(gModSetDACVal & 0xFF));
	I2CWriteData(IIC_ADDR, 111, (uint8)((gModSetDACVal >> 8) & 0xFF));
        
        // APC
	I2CWriteData(IIC_ADDR, 126, (uint8)(gApcSetDACVal & 0xFF));
	I2CWriteData(IIC_ADDR, 127, (uint8)(gApcSetDACVal & 0xFF));
				
}


////////////////////////////////////////////////////////////////////////////////


// The End.
//----------------------------------------------------------------------------//

