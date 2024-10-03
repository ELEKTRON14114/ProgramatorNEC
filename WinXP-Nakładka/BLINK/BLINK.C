#pragma SFR
#pragma NOP

void delay(unsigned char time);
unsigned char ucCnt200us;
unsigned char i,y;

void hdwinit(void){
 WDTM = 0b01110111;  /*wylancz watchdog */
 PCC = 0x00; /* F oscylatora bez dzielenia przez 2 */

 for (ucCnt200us = 0; ucCnt200us < 20; ucCnt200us++){
 NOP();
 }
}

void main(void)
{
PU0 = 0b00000000;
PM0 = 0b11111110;

while (1)
	{
	P0 = 0x00;
	delay(100);
	P0 = 0x01;
        delay(100);
	}	
}

void delay(unsigned char time)
{
for (y = 0; y < 100; y++)
for (i = 0; i < time; i++)
 	{
  	NOP();
 	}
}