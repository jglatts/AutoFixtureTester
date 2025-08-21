/**
*
*       AT-CTF-PCB-001
*       Signal Controller Firmware V0.1
*
*       Author: John Glatts
*       Date:   8/21/2025
*/
#include "SignalController.h"
#include "Arduino.h"
#include <Wire.h>

int test_data = 0;

void requestEvent() {
	// data is requested 
	// will run sig output here
	Wire.write(test_data + 1);
	Serial.println(test_data + 1);
}

void callBack(int size) {
	// data is sent
	while (Wire.available()) {
		test_data = Wire.read();
		Serial.print("got ");
		Serial.println(test_data);
	}
}

bool SignalController::init() {
	Wire.begin(ADDR);
	Wire.onRequest(requestEvent);
	Wire.onReceive(callBack);
	return true;
}
