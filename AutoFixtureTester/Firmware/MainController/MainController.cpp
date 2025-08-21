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
	startPin = -1;
	endPin = -1;
	return true;
}

void MainController::waitForCmd() {
	while (1) {
		if (Serial.available() > 0) {
			int cmd = Serial.parseInt();
			char buff[200];
			if (cmd == PC_DATA_CMD) {
				startPin = Serial.parseInt();
				endPin = Serial.parseInt();
				int type = Serial.parseInt();
				if (type == PC_HOST_START_FULL_TEST) {
					testType = TEST_OPEN_SHORT;
				}
				if (type == PC_HOST_START_OPEN_TEST) {
					testType = TEST_OPEN;
				}
				if (type == PC_HOST_START_SHORT_TEST) {
					testType = TEST_SHORT;
				}
				return;
			}
		}
	}
}

void MainController::run() {
	waitForCmd();
	findIndicies();
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

void MainController::findIndicies() {
	for (int i = 0; i < NUM_TEST_PINS; i++) {
		if (dutPins[i].dut_test_pin == startPin)
			startIndex = i;
		if (dutPins[i].dut_test_pin == endPin)
			endIndex = i + 1;
	}
}

bool MainController::runOpenTest() {
	// clear pin state
	for (int i = 0; i < NUM_TEST_PINS; i++) {
		pinMode(dutPins[i].pcb_pin, INPUT_PULLUP);
	}
	delay(5);

	// generate test signals
	sendToSignalController(SIG_CONTROLLER_ADDR, RUN_OPEN_TEST);
	delay(200);

	// test the pins
	for (int i = startIndex; i < endIndex; i++) {
		char buff[200];
		bool check = true;
		if (digitalRead(dutPins[i].pcb_pin) == HIGH) {
			check = false;
		}
		if (check)
			sprintf(buff, "pin,%d,passed", dutPins[i].dut_test_pin);
		else
			sprintf(buff, "pin,%d,open", dutPins[i].dut_test_pin);

		Serial.println(buff);
		delay(5);
	}

	Serial.println("done");

	return true;
}

bool MainController::runShortTest() {
	sendToSignalController(SIG_CONTROLLER_ADDR, CLEAR_SIG_PINS);
	delay(10);
	for (int i = startIndex; i < endIndex; i++) {
		PinMapping curr_pin = dutPins[i];
		for (int ii = 0; ii < 40; ii++) {
			pinMode(dutPins[ii].pcb_pin, INPUT_PULLUP);
		}
		pinMode(curr_pin.pcb_pin, OUTPUT);
		digitalWrite(curr_pin.pcb_pin, LOW);
		bool ret = true;
		int j;
		for (j = startIndex; j < endIndex; j++) {
			if (i == j)
				continue;
			if (digitalRead(dutPins[j].pcb_pin) == LOW) {
				ret = false;
				break;
			}
		}
		char buff[200];
		if (ret) {
			sprintf(buff, "pin,%d,noshort", dutPins[i].dut_test_pin);
			Serial.println(buff);
		}
		else {
			sprintf(buff, "pin,%d,short,%d", dutPins[i].dut_test_pin, dutPins[j].dut_test_pin);
			Serial.println(buff);
		}
	}
	Serial.println("done");
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

	for (int i = 0; i < 40; i++) {
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

void MainController::testShortTest() {
	sendToSignalController(SIG_CONTROLLER_ADDR, CLEAR_SIG_PINS);
	delay(10);
	for (int i = 0; i < 40; i++) {
		int short_pin = 0;
		PinMapping curr_pin = dutPins[i];
		Serial.print("pin ");
		Serial.print(dutPins[i].dut_test_pin);
		for (int ii = 0; ii < 40; ii++) {
			pinMode(dutPins[ii].pcb_pin, INPUT_PULLUP);
		}
		pinMode(curr_pin.pcb_pin, OUTPUT);
		digitalWrite(curr_pin.pcb_pin, LOW);
		bool ret = true;
		for (int j = 0; j < 40; j++) {
			if (i == j)
				continue;
			if (digitalRead(dutPins[j].pcb_pin) == LOW) {
				ret = false;
				short_pin = dutPins[j].dut_test_pin;
				break;
			}
		}
		if (ret)
			Serial.println(" no short");
		else {
			Serial.print(" short with pin ");
			Serial.println(short_pin);
		}
	}
	delay(1000);
}

