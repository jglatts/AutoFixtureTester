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

SignalController* SignalController::instance = nullptr;

void SignalController::requestEvent() {
	Wire.write(test_data + 1);
	Serial.println(test_data + 1);
}

void SignalController::callBack(int size) {
	while (Wire.available()) {
		test_data = Wire.read();
		if (test_data == RUN_OPEN_TEST)
			setPins();
		if (test_data == CLEAR_SIG_PINS)
			clearPins();
	}
}

void SignalController::setPins() {
	for (int i = 0; i < NUM_TEST_PINS; i++) {
		pinMode(dutPins[i].pcb_pin, OUTPUT);
		digitalWrite(dutPins[i].pcb_pin, LOW);
	}
}

void SignalController::clearPins() {
	for (int i = 0; i < NUM_TEST_PINS; i++) {
		pinMode(dutPins[i].pcb_pin, INPUT);
	}
}

void SignalController::requestEventStatic() {
	if (instance) instance->requestEvent();
}

void SignalController::callBackStatic(int bytes) {
	if (instance) instance->callBack(bytes);
}

bool SignalController::init() {
	instance = this;
	Wire.begin(ADDR);
	Wire.onRequest(requestEventStatic);
	Wire.onReceive(callBackStatic);
	return true;
}
