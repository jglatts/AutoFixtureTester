/**
*
*       AT-CTF-PCB-001
*       Main Controller Firmware V0.1
*
*       Author: John Glatts
*       Date:   8/21/2025
*/
#include "MainController.h"
#include <Wire.h>


bool MainController::init() {
	Wire.begin();
	return true;
}

void MainController::waitForCmd() {
	while (1) {
		// check what we get here 
		if (Serial.available() > 0) {
			if (Serial.readString().toInt() == PC_HOST_START_TEST)
			{
				Serial.println("will be starting test");
				return;
			}
		}
	}
}

void MainController::sendToSignalController(int addr, int data) {
	Wire.beginTransmission(addr);
	Wire.write(data);
	Wire.endTransmission();
}

void MainController::testComms(int addr) {
	int x = 1;
	while (1) {
		Wire.beginTransmission(addr);
		char send_buff[500];
		sprintf(send_buff, "sending %d", x);
		Serial.println(send_buff);
		Wire.write(x++);
		Wire.endTransmission();
		Wire.requestFrom(addr, 1);
		int ret = -1;
		while (Wire.available()) {
			ret = Wire.read();
		}
		char rec_buff[100];
		sprintf(rec_buff, "got %d", ret);
		Serial.println(rec_buff);
		delay(500);
	}
}