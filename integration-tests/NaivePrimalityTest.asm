# Scenario: naive primality test
# If the unsigned number in $a0 is prime, set $v0 to 1
# Otherwise, set $v0 to 0

.globl is_prime

is_prime:
srl $t0,$a0,1             # $t0 = $a0 / 2
beqz $t0,not_prime        # 0, 1 are not prime
li $t1,2
beq $a0,$t1,prime         # 2 is prime
andi $t2,$a0,1            # Otherwise, if the last bit of $a0 is 0,
beqz $t2,not_prime        # then $a0 is even and not prime.
li $t2,3
beq $a0,$t2,prime         # 3 is prime

loop:
beq $t0,$t1,prime         # if we don't find a factor no larger than $t0, $a0 is prime
addi $t1,$t1,1
div $a0,$t1
mfhi $t2
beqz $t2,not_prime        # If remainder is 0, we found a factor and $a0 is not prime
b loop

not_prime:
li $v0,0
b end

prime:
li $v0,1
b end

end:
