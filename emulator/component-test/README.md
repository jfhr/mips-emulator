# Component tests

These tests contain assembly programs that have been verified to fulfill their specifications when run with MUnit.
It is then verified that they show the same behavior in our emulator.

In order to verify the correctness of the test data itself, you need to have `munit` installed locally.

Then choose one test scenario, e.g. the `NaivePrimalityTest`, and run:
```
javac -cp munit.jar NaivePrimalityTest.java
java -jar munit.jar NaivePrimalityTest.asm NaivePrimalityTest.class
```
