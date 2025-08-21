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

#define CLEAR_SIG_PINS 4

#define RUN_OPEN_TEST	1
#define RUN_SHORT_TEST	2
#define RUN_FULL_TEST	3

#define NUM_TEST_PINS 40

typedef enum TestType {
    TEST_OPEN,
    TEST_SHORT,
    TEST_OPEN_SHORT
} TestType;

typedef struct PinMapping {
    int dut_test_pin;
    int pcb_pin;
} PinMapping;

class SignalController {
public:
    bool init();
    bool waitForCmd();
    void testComms();
    void clearPins();
    void setPins();
private:
    static SignalController* instance;
    static void requestEventStatic();
    static void callBackStatic(int bytes);
    void requestEvent();
    void callBack(int bytes);
    TestType testType;
    // need to fill out test pins here - probe out?
    PinMapping dutPins[NUM_TEST_PINS] = {
        { 1, 3 },
        { 2, 2 },
        { 3, 5 },
        { 4, 4 },
        { 5, 7 },
        { 6, 6 },
        { 7, 9 },
        { 8, 8 },
        { 9, 11 },
        { 10, 10 },
        { 11, 13 },
        { 12, 12 },
        { 13, 23 },
        { 14, 22 },
        { 15, 25 },
        { 16, 24 },
        { 17, 27 },
        { 18, 26 },
        { 19, 29 },
        { 20, 28 },
        { 21, 31 },
        { 22, 30 },
        { 23, 33 },
        { 24, 32 },
        { 25, 35 },
        { 26, 34 },
        { 27, 37 },
        { 28, 36 },
        { 29, 39 },
        { 30, 38 },
        { 31, 41 },
        { 32, 40 },
        { 33, 43 },
        { 34, 42 },
        { 35, 45 },
        { 36, 44 },
        { 37, 47 },
        { 38, 46 },
        { 39, 49 },
        { 40, 48 }
    };
};


#endif // ! __SIG__CONTROLLER__H
