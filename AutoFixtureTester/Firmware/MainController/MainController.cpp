/**
*
*       AT-CTF-PCB-001
*       Main Controller Firmware V0.1
*
*       Author: John Glatts
*       Date:   8/21/2025
*/
#include "MainController.h"
#include "Arduino.h"
#include <Wire.h>

bool MainController::init() {
	Wire.begin();
	return true;
}

void MainController::waitForCmd() {
	while (1) {
		// check what we get here 
		if (Serial.available() > 0) {
			int test_type = Serial.readString().toInt();
			if (test_type == PC_HOST_START_FULL_TEST)
			{
				testType = TEST_OPEN_SHORT;
				Serial.println("will be starting full test");
				return;
			}
			if (test_type == PC_HOST_START_OPEN_TEST)
			{
				testType = TEST_OPEN;
				Serial.println("will be starting open test");
				return;
			}
			if (test_type == PC_HOST_START_SHORT_TEST)
			{
				testType = TEST_SHORT;
				Serial.println("will be starting short test");
				return;
			}
		}
	}
}

void MainController::run() {
	waitForCmd();
	if (testType == TEST_OPEN_SHORT) {
		runFullTest();
		return;
	}
	if (testType == TEST_OPEN) {
		runOpenTest();
		return;
	}
	if (testType == TEST_SHORT) {
		runShortTest();
		return;
	}
}

void MainController::runFullTest() {
	if (!runOpenTest())
		return;
	runShortTest();
}

bool MainController::runOpenTest() {
	return true;
}

bool MainController::runShortTest() {
	return true;
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

void MainController::testOpenTest() {
	for (int i = 0; i < NUM_TEST_PINS; i++) {
		pinMode(dutPins[i].pcb_pin, INPUT_PULLUP);
	}
	delay(5);
	sendToSignalController(SIG_CONTROLLER_ADDR, RUN_OPEN_TEST);
	delay(200);	// wait for signals

	for (int i = 0; i < 1; i++) {
		Serial.print("pin ");
		Serial.print(dutPins[i].dut_test_pin);
		if (digitalRead(dutPins[i].pcb_pin) == LOW) {
			Serial.println(" passed");
		}
		else {
			Serial.println(" OPEN");
		}
	}

	delay(1000);

}