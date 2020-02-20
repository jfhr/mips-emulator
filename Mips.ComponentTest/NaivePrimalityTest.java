import java.util.Arrays;
import java.util.List;

import org.junit.*;

import static edu.gvsu.mipsunit.munit.MUnit.Register.*;
import static edu.gvsu.mipsunit.munit.MUnit.*;

public class NaivePrimalityTest {

  @Test
  public void verify_correct_below_100() {
    List<Integer> primes = Arrays.asList(2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97);

    for (int i = 0; i < 100; i++) {
      run("is_prime", i);
      int expected = primes.contains(i) ? 1 : 0;
      Assert.assertEquals(expected, get(v0));
    }
  }
}
