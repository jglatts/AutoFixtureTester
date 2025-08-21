/**
*
*       AT-CTF-PCB-001
*       Signal Controller Firmware V0.1
*
*       Author: John Glatts
*       Date:   8/21/2025
*/
#ifndef  __SIG__CONTROLLER__H_

#define ADDR 5

#define RUN_OPEN_TEST	1
#define RUN_SHORT_TEST	2
#define RUN_FULL_TEST	3

typedef enum TestType {
    TEST_OPEN,
    TEST_SHORT,
    TEST_OPEN_SHORT
};

typedef struct PinMapping {
    int dut_test_pins[50];
    int pcb_pins[50];
} PinMapping;

class SignalController {
public:
    bool init();
    bool waitForCmd();
    void testComms();
private:
    TestType testType;
    // need to fill out test pins here - probe out?
    PinMapping dutPins = {
        .dut_test_pins = {},
        .pcb_pins = {}
    };
};


#endif // ! __SIG__CONTROLLER__H
