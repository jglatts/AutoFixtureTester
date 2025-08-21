/**
*
*       AT-CTF-PCB-001
*       Main Controller Firmware V0.1
*
*       Author: John Glatts
*       Date:   8/21/2025
*/
#include "MainController.h"
#include <wire.h>


bool MainController::init() {
	wire.begin();
	return true;
}

bool MainController::waitForCmd() {
	return true;
}

void MainController::sendToSignalController(int addr, int data) {
	Wire.beginTransmission(addr);
	Wire.write(data);
	Wire.endTransmission();
}

void MainController::testComms(int addr) {
	while (1) {
		Wire.beginTransmission(addr);
		Wire.write(data);
		Wire.endTransmission();
		Wire.requestFrom(SIG_CONTROLLER_ADDR, 10);
		string ret = "";
		while (Wire.available()) { 
			char c = Wire.read(); // Receive a byte as a character
			ret += c;
		}
		Serial.println(ret);
		delay(500);
	}
}