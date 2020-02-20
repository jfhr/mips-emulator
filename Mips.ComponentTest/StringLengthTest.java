import java.util.Arrays;
import java.util.List;

import org.junit.*;

import static edu.gvsu.mipsunit.munit.MUnit.Register.*;
import static edu.gvsu.mipsunit.munit.MUnit.*;

public class StringLengthTest {

  @Test
  public void verify_correct_0() {
    String s = "Hello World";
    int expected = s.length();
    Label string = asciiData(s);
    set(a0, string);
    int a0_value = get(a0);

    run("strlen");

    Assert.assertEquals(expected, get(v0), "Returned false strlen");
    Assert.assertEquals(a0_value, get(a0), "Should not modify a0");
    Assert.assertTrue(noOtherMemoryModifications());
  }
}
