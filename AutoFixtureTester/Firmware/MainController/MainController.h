/**
* 
*       AT-CTF-PCB-001
*       Main Controller Firmware V0.1
* 
*       Author: John Glatts
*       Date:   8/21/2025 
*/
#ifndef  __MAIN_CONTROLLER__H_

// should impl result type for returns and error checking 

#define RUN_OPEN_TEST	1
#define RUN_SHORT_TEST	2
#define RUN_FULL_TEST	3

#define SIG_CONTROLLER_ADDR 5

#define PC_HOST_START_TEST  69

typedef enum TestType {
    TEST_OPEN,
    TEST_SHORT,
    TEST_OPEN_SHORT
};

typedef struct PinMapping {
    int dut_test_pins[50];
    int pcb_pins[50];
} PinMapping;

class MainController {
public:
    bool init();
    void waitForCmd();
    void sendToSignalController(int, int);
    void testComms(int);
private:
    TestType testType;
    // need to fill out test pins here - probe out?
    PinMapping dutPins = {
        .dut_test_pins = {},
        .pcb_pins = {}
    };
};



#endif // ! __MAIN_CONTROLLER__H_
